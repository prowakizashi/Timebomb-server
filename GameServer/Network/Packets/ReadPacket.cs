using GameServer.Rooms;
using GameServer.Timebomb;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Network.Packets
{
    class ReadPacket
    {
        #region CONNECTION PACKETS
        public static void CheckVersion(int _clientId, Packet _packet)
        {
            string _version = _packet.ReadString();

            Server.clients[_clientId].CheckVersion(_version);
        }

        public static void Disconnect(int _clientId, Packet _packet)
        {
            Server.clients[_clientId].Disconnect();
        }
        #endregion

        #region ROOM PACKETS
        public static void UpdatePlayer(int _clientId, Packet _packet)
        {
            Player player = Server.clients[_clientId].Player;

            player.Playername   = _packet.ReadString();
            player.IsReady      = _packet.ReadBool();
        }

        public static void ChangePlayerLeader(int _clientId, Packet _packet)
        {
            int newLeaderId = _packet.ReadInt();

            Player player = Server.clients[_clientId].Player;
            Room room = player.Room;
            if (room != null && player.IsLeader)
            {
                room.SetLeader(newLeaderId);
            }
        }

        public static void CreateRoom(int _clientId, Packet _packet)
        {
            string  _name       = _packet.ReadString();
            int     _size       = _packet.ReadInt();
            bool    _visible    = _packet.ReadBool();
            bool    _opened     = _packet.ReadBool();

            RoomManager.CreateRoom(_clientId, _name, _size, _visible, _opened);
        }

        public static void UpdateRoom(int _clientId, Packet _packet)
        {

            int     _size       = _packet.ReadInt();
            bool    _visible    = _packet.ReadBool();
            bool    _opened     = _packet.ReadBool();

            RoomManager.UpdateRoom(_clientId, _size, _visible, _opened);
        }

        public static void JoinRoom(int _clientId, Packet _packet)
        {
            string _name = _packet.ReadString();

            RoomManager.JoinRoom(_clientId, _name);
        }

        public static void LeaveRoom(int _clientId, Packet _packet)
        {
            RoomManager.LeaveRoom(_clientId);
        }

        public static void TryStartGame(int _clientId, Packet _packet)
        {
            Player player = Server.clients[_clientId].Player;

            Room _room = player.Room;
            if (_room != null)
            {
                _room.StartGame();
            }
        }
        #endregion

        #region GAME PACKETS

        public static void IsGameReady(int _clientId, Packet _packet)
        {
            Player player = Server.clients[_clientId].Player;
            if (player.Room != null && player.Room.Game != null)
            {
                player.Room.Game.PlayerReady();
            }
        }

        public static void HoverACard(int _clientId, Packet _packet)
        {
            int _cardId = _packet.ReadInt();
            bool _state = _packet.ReadBool();

            Player player = Server.clients[_clientId].Player;
            if (player.Room != null)
            {
                WritePacket.HoverACard(player.Room, _cardId, _state);
            }
        }

        public static void WatchCards(int _clientId, Packet _packet)
        {
            Player player = Server.clients[_clientId].Player;
            if (player.Room != null && player.Room.Game != null)
            {
                player.Room.Game.PlayerWatchCards(player.PlayerId);
            }
        }

        public static void PickACard(int _clientId, Packet _packet)
        {
            int _cardId = _packet.ReadInt();
            Player player = Server.clients[_clientId].Player;
            if (player.Room != null && player.Room.Game != null)
            {
                player.Room.Game.PlayerChooseCard(_cardId);
            }
        }

        public static void ReturnToLobby(int _clientId, Packet _packet)
        {
            Player player = Server.clients[_clientId].Player;
            if (player.Room != null && player.Room.Game != null)
            {
                player.Room.Game.ReturnToLobby();
            }
        }

        #endregion

        #region MESSAGERIE


        public static void SendMessage(int _clientId, Packet _packet)
        {
            string _message = _packet.ReadString();
            Player player = Server.clients[_clientId].Player;
            if (player.Room != null)
            {
                player.Room.SendPlayerMessage(player.PlayerId, _message);
            }
        }

        #endregion
    }
}
