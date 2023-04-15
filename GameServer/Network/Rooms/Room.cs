using GameServer.Game;
using GameServer.Network.Packets;
using GameServer.Timebomb;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Rooms
{
    class Room
    {
        public string Name { get; private set; }

        private int roomSize = 1;
        public int RoomSize
        {
            get { return roomSize; }
            set { roomSize = value; SetDirty(); }
        }

        private bool visible = true;
        public bool Visible
        {
            get { return visible; }
            set { visible = value;  SetDirty(); }
        }

        private bool opened = true;
        public bool Opened
        {
            get { return opened; }
            set {  opened = value; SetDirty(); }
        }

        public Player Leader { get; private set; }

        public IDictionary<int, Player> Players = new Dictionary<int, Player>();

        //Keep player order
        public List<Player> PlayerList = new List<Player>();

        private bool isDirty = false;
        private int nextPlayerTurn = -1;

        public GameInstance Game { get; private set; }

        public Room(string _name, int _roomSize, bool _visible, bool _opened)
        {
            Name = _name;
            roomSize = _roomSize;
            visible = _visible;
            opened = _opened;
        }

        public void SetLeader(int _playerId)
        {
            Player _player;
            if (!Players.TryGetValue(_playerId, out _player))
                return;

            SetLeader(_player);
        }

        public void SetLeader(Player _player)
        {
            Leader = _player;
            WritePacket.ChangeLeader(this, _player.PlayerId);
        }

        public void AddPlayer(Player _player)
        {
            if (Players.Count + 1 > roomSize)
            {
                Console.WriteLine($"Player {_player.Playername} tried to join full room {Name}.");
                WritePacket.FailToJoinRoom(_player.client.clientId, 1); // room is full
                return;
            }

            if (Players.Where(pair => pair.Value.Playername == _player.Playername).Count() != 0)
            {
                Console.WriteLine($"Player {_player.Playername} tried to join with a name already used.");
                WritePacket.FailToJoinRoom(_player.client.clientId, 2); // username already used
                return;
            }

            if (Players.Count == 0)
            {
                SetLeader(_player);
            }

            Players.Add(_player.PlayerId, _player);
            PlayerList.Add(_player);
            _player.Room = this;

            Console.WriteLine($"Player {_player.Playername} joined room {Name}.");
            WritePacket.JoinRoom(_player.client.clientId, this);
            WritePacket.PlayerJoin(this, _player);
        }

        public void RemovePlayer(Player _player)
        {
            if (Players.Remove(_player.PlayerId))
            {
                PlayerList.Remove(_player);

                if (PlayerList.Count == 0)
                {
                    RoomManager.DeleteRoom(this);
                    return;
                }

                Console.WriteLine($"Player {_player.Playername} left room {Name}.");
                WritePacket.PlayerLeave(this, _player.PlayerId);

                if (_player.IsLeader && PlayerList.Count > 0)
                {
                    SetLeader(PlayerList[0]);
                }
            }
        }

        private void SetDirty()
        {
            if (isDirty)
                return;

            isDirty = true;
            ThreadManager.AddMainThreadTask(() =>
            {
                WritePacket.UpdateRoom(this);
                isDirty = false;
            });
        }

        public void StartGame()
        {
            if (PlayerList.Where(_player => _player.IsReady).Count() < Players.Count)
            {
                WritePacket.StartGame(this, false);
                return;
            }

            Opened = false;
            int startingPlayer = Players.ContainsKey(nextPlayerTurn) ? nextPlayerTurn : PlayerList[new Random().Next(Players.Count)].PlayerId;
            Game = new GameInstance(this, startingPlayer);
            WritePacket.StartGame(this, true);
        }

        public void GameIsOver(int _nextPlayerTurn)
        {
            nextPlayerTurn = _nextPlayerTurn;
        }

        public void EndGame()
        {
            Game.Stop();
            Game = null;
            Opened = true;
        }

        public void SendPlayerMessage(int _senderId, string _message)
        {
            WritePacket.SendPlayerMessage(this, _senderId, _message);
        }

        public void SendSystemMessage(int _messageId)
        {
            WritePacket.SendSystemMessage(this, _messageId);
        }
    }
}
