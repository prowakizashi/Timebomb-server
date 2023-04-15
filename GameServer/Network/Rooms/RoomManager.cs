using GameServer.Network;
using GameServer.Network.Packets;
using GameServer.Timebomb;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Rooms
{
    class RoomManager
    {
        private static IDictionary<string, Room> rooms = new Dictionary<string, Room>();

        public static void CreateRoom(int _clientId, string _roomName, int _size, bool _visible, bool _opened)
        {
            var _player = Server.clients[_clientId].Player;

            if (_player.Room != null || rooms.ContainsKey(_roomName))
            {
                WritePacket.createRoom(_clientId, false);
            }
            else
            {
                WritePacket.createRoom(_clientId, true);
                var _room = new Room(_roomName, _size, _visible, _opened);
                Console.WriteLine($"Room {_roomName} create by player {_player.Playername}");
                rooms.Add(_roomName, _room);
                _room.AddPlayer(_player);
            }
        }

        public static void DeleteRoom(Room _room)
        {
            Console.WriteLine($"Room {_room.Name} destroyed.");
            rooms.Remove(_room.Name);
        }

        public static void JoinRoom(int _clientId, string _roomName)
        {
            Player _player = Server.clients[_clientId].Player;

            Room _room;
            if (_player.Room == null && rooms.TryGetValue(_roomName, out _room) && _room.Opened)
            {
                _room.AddPlayer(_player);
            }
            else
            {
                Console.WriteLine($"Player {_player.Playername} failed to join {_roomName}.");
                WritePacket.FailToJoinRoom(_clientId, 0); // room doesn't exists
            }
        }

        public static void UpdateRoom(int _clientId, int _size, bool _visible, bool _opened)
        {
            Client _client = Server.clients[_clientId];
            if (_client == null)
                return;

            var _player = _client.Player;
            Room _room = _player.Room;
            if (_room != null && _room.Leader.client.clientId != _clientId)
            {
                _room.RoomSize = _size;
                _room.Visible = _visible;
                _room.Opened = _opened;
            }
        }

        public static void LeaveRoom(int _clientId)
        {
            Client _client = Server.clients[_clientId];
            if (_client == null)
                return;

            LeaveRoom(_client.Player);
        }

        public static void LeaveRoom(Player _player)
        {
            if (_player != null && _player.Room != null)
            {
                _player.Room.RemovePlayer(_player);
                _player.Room = null;
            }
        }
    }
}
