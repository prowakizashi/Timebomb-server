using GameServer.Network.Packets;
using GameServer.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Game
{
    public enum GameOverType
    {
        WIRES = 0,
        BOMB,
        TIMEOUT
    }

    class GameInstance
    {
        private Room room;

        private int numberOfPlayers = 0;
        private int readyPlayers = 0;

        private IDictionary<int, Card> cards;
        private int currentRound = 0;
        private int currentTurn = 0;
        private int nextPlayerTurn = 0;

        private int numberOfWires = 0;

        private PlayerRole[] roles;

        public GameInstance(Room _room, int _nextPlayerTurn)
        {
            room = _room;
            numberOfPlayers = room.Players.Count;
            nextPlayerTurn = _nextPlayerTurn;
            InitCards();
            InitRoles();
        }

        public void Stop()
        {
            room = null;
        }

        private void InitCards()
        {
            cards = new Dictionary<int, Card>();
            cards.Add(0, new Card(CardType.BOMB));
            for (int i = 1; i < numberOfPlayers + 1; ++i)
                cards.Add(i, new Card(CardType.RED_WIRE));
            for (int i = numberOfPlayers + 1; i < numberOfPlayers * 5; ++i)
                cards.Add(i, new Card(CardType.GREY_WIRE));
        }

        private void InitRoles()
        {
            int numberOfRoles = numberOfPlayers == 4 || numberOfPlayers == 7 ? numberOfPlayers + 1 : numberOfPlayers;
            int numBadguys = numberOfPlayers < 7 ? 2 : 3;

            roles = new PlayerRole[numberOfRoles];
            for (int i = 0; i < numBadguys; ++i)
                roles[i] = PlayerRole.BAD_GUY;
            for (int i = numBadguys; i < numberOfRoles; ++i)
                roles[i] = PlayerRole.GOOD_GUY;

            Random rnd = new Random();
            roles = roles.OrderBy(x => rnd.Next()).ToArray();
        }

        public void PlayerReady()
        {
            ++readyPlayers;
            if (readyPlayers == numberOfPlayers)
            {
                ThreadManager.DelayTask(1500, InitGame);
            }
        }

        private void InitGame()
        {
            WritePacket.SendCards(room, cards.Values.ToArray());
            WritePacket.SendRoles(room, roles, numberOfPlayers);
            ThreadManager.DelayTask(3000, StartRound);
        }

        private void StartRound()
        {
            Random rnd = new Random();
            int[] _cards = cards.Keys.OrderBy(pair => rnd.Next()).ToArray();
            
            for (int i = 0; i < _cards.Length; ++i)
            {
                cards[_cards[i]].Owner = room.PlayerList[i % numberOfPlayers].PlayerId;
            }

            currentTurn = 0;
            WritePacket.StartRound(room, currentRound, _cards);
            ThreadManager.DelayTask(3000, StartTurn);
        }

        private void EndRound()
        {
            if (++currentRound == 4)
            {
                GameOver(GameOverType.TIMEOUT);
            }
            else
            {
                WritePacket.EndRound(room);
                ThreadManager.DelayTask(3000, StartRound);
            }
        }

        private void StartTurn()
        {
            WritePacket.StartTurn(room, currentTurn, nextPlayerTurn);
        }

        public void PlayerWatchCards(int _playerId)
        {
            WritePacket.WatchCards(room, _playerId);
        }

        public void PlayerChooseCard(int _cardId)
        {
            WritePacket.EndTurn(room, _cardId);
            var _card = cards[_cardId];
            cards.Remove(_cardId);

            nextPlayerTurn = _card.Owner;

            if (_card.Type == CardType.BOMB)
            {
                ThreadManager.DelayTask(2000, () => GameOver(GameOverType.BOMB));
                return;
            }

            if (_card.Type == CardType.RED_WIRE)
            {
                ++numberOfWires;
                if (numberOfWires == numberOfPlayers)
                {
                    ThreadManager.DelayTask(2000, () => GameOver(GameOverType.WIRES));
                    return;
                }
            }
            
            if (++currentTurn == numberOfPlayers)
            {
                ThreadManager.DelayTask(2000, EndRound);
            }
            else
            {
                ThreadManager.DelayTask(2000, StartTurn);
            }
        }

        private void GameOver(GameOverType _type)
        {
            WritePacket.GameOver(room, (int)_type, numberOfWires);

            room.GameIsOver(nextPlayerTurn);
        }


        public void ReturnToLobby()
        {
            WritePacket.ReturnToLobby(room);
        }
    }
}
