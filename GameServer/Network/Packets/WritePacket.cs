using GameServer.Game;
using GameServer.Rooms;
using GameServer.Timebomb;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Network.Packets
{
    class WritePacket
    {
        #region SEND FUNCTIONS
        private static void SendTCPDataToClient(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendTCPDataToRoom(Room _room, Packet _packet)
        {
            if (_room == null)
                return;

            _packet.WriteLength();
            foreach (Player player in _room.PlayerList)
            {
                player.client.tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToRoomExceptClient(Room _room, int _clientException, Packet _packet)
        {
            if (_room == null)
                return;

            _packet.WriteLength();
            foreach (Player player in _room.PlayerList)
            {
                if (player.client.clientId != _clientException)
                    player.client.tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToRoomExceptPlayer(Room _room, int _playerException, Packet _packet)
        {
            if (_room == null)
                return;

            _packet.WriteLength();
            foreach (Player player in _room.PlayerList)
            {
                if (player.PlayerId != _playerException)
                    player.client.tcp.SendData(_packet);
            }
        }
        #endregion

        #region ROOM PACKETS
        public static void Welcome(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_toClient);

                SendTCPDataToClient(_toClient, _packet);
            }
        }

        public static void FailToConnect(int _toClient, int _errorCode)
        {
            using (Packet _packet = new Packet((int)ServerPackets.failToConnect))
            {
                _packet.Write(_errorCode);

                SendTCPDataToClient(_toClient, _packet);
            }
        }

        public static void createRoom(int _toClient, bool _success)
        {
            using (Packet _packet = new Packet((int)ServerPackets.createRoom))
            {
                _packet.Write(_success);

                SendTCPDataToClient(_toClient, _packet);
            }
        }

        public static void JoinRoom(int _toClient, Room _room)
        {
            using (Packet _packet = new Packet((int)ServerPackets.joinRoom))
            {
                _packet.Write(_room.Name);
                _packet.Write(_room.RoomSize);
                _packet.Write(_room.Visible);
                _packet.Write(_room.Opened);
                _packet.Write(_room.Leader ==  null ? -1 : _room.Leader.PlayerId);
                _packet.Write(_room.PlayerList.Count);

                foreach (Player _player in _room.PlayerList)
                {
                    _packet.Write(_player.PlayerId);
                    _packet.Write(_player.Playername);
                    _packet.Write(_player.client.clientId);
                    _packet.Write(_player.IsReady);
                }

                SendTCPDataToClient(_toClient, _packet);
            }
        }

        public static void FailToJoinRoom(int _toClient, int _errorCode)
        {
            using (Packet _packet = new Packet((int)ServerPackets.joinRoomFailed))
            {
                _packet.Write(_errorCode);

                SendTCPDataToClient(_toClient, _packet);
            }
        }

        public static void PlayerJoin(Room _room, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerJoin))
            {
                _packet.Write(_player.PlayerId);
                _packet.Write(_player.Playername);
                _packet.Write(_player.client.clientId);
                _packet.Write(_player.IsLeader);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void PlayerLeave(Room _room, int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerLeave))
            {
                _packet.Write(_playerId);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void ChangeLeader(Room _room, int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.changeLeader))
            {
                _packet.Write(_playerId);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void UpdateRoom(Room _room)
        {
            using (Packet _packet = new Packet((int)ServerPackets.updateRoom))
            {
                _packet.Write(_room.RoomSize);
                _packet.Write(_room.Visible);
                _packet.Write(_room.Opened);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void UpdatePlayer(Room _room, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.updatePlayer))
            {
                _packet.Write(_player.PlayerId);
                _packet.Write(_player.Playername);
                _packet.Write(_player.IsReady);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void StartGame(Room _room, bool _start)
        {
            using (Packet _packet = new Packet((int)ServerPackets.startGame))
            {
                _packet.Write(_start);

                SendTCPDataToRoom(_room, _packet);
            }
        }
        #endregion

        #region GAME PACKETS

        public static void SendCards(Room _room, Card[] _cards)
        {
            using (Packet _packet = new Packet((int)ServerPackets.sendCards))
            {
                _packet.Write(_cards.Length);
                foreach (Card _card in _cards)
                {
                    _packet.Write((int)_card.Type);
                }

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void SendRoles(Room _room, PlayerRole[] _roles, int _playerCount)
        {
            using (Packet _packet = new Packet((int)ServerPackets.sendRoles))
            {
                _packet.Write(_playerCount);
                foreach (PlayerRole _role in _roles)
                {
                    _packet.Write((int)_role);
                }

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void HoverACard(Room _room, int _cardId, bool _state)
        {
            using (Packet _packet = new Packet((int)ServerPackets.hoverCard))
            {
                _packet.Write(_cardId);
                _packet.Write(_state);
                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void WatchCards(Room _room, int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.watchCards))
            {
                _packet.Write(_playerId);
                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void StartRound(Room _room, int _round, int[] _cards)
        {
            using (Packet _packet = new Packet((int)ServerPackets.startRound))
            {
                _packet.Write(_round);
                _packet.Write(_cards.Length);

                foreach (int _cardId in _cards)
                {
                    _packet.Write(_cardId);
                }

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void EndRound(Room _room)
        {
            using (Packet _packet = new Packet((int)ServerPackets.endRound))
            {
                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void StartTurn(Room _room, int _turn, int _playerId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.startTurn))
            {
                _packet.Write(_turn);
                _packet.Write(_playerId);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void EndTurn(Room _room, int _cardId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.endTurn))
            {
                _packet.Write(_cardId);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void GameOver(Room _room, int _type, int _redWires)
        {
            using (Packet _packet = new Packet((int)ServerPackets.gameOver))
            {
                _packet.Write(_type);
                _packet.Write(_redWires);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void ReturnToLobby(Room _room)
        {
            using (Packet _packet = new Packet((int)ServerPackets.returnToLobby))
            {
                _room.EndGame();

                SendTCPDataToRoom(_room, _packet);
            }
        }
        #endregion

        #region MESSAGERIE

        public static void SendPlayerMessage(Room _room, int _senderId, string _message)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerMessage))
            {
                _packet.Write(_senderId);
                _packet.Write(_message);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        public static void SendSystemMessage(Room _room, int _messageId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerMessage))
            {
                _packet.Write(_messageId);

                SendTCPDataToRoom(_room, _packet);
            }
        }

        #endregion
    }
}
