using GameServer.Network;
using GameServer.Network.Packets;
using GameServer.Rooms;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Timebomb
{
    class Player
    {
        public int PlayerId { get; set; }

        private string playername = "NoName";
        public string Playername
        {
            get { return playername; }
            set { playername = value; UpdatePlayer(); }
        }

        private bool isReady = false;
        public bool IsReady
        {
            get { return isReady; }
            set { isReady = value;  UpdatePlayer(); }
        }

        public bool IsLeader { get { return Room == null ? false : Room.Leader == this; } }

        private bool updatePlayer = false;
        public Room Room { get; set; }
        public Client client { get; private set; }

        public Player(Client _client)
        {
            client = _client;
            PlayerId = client.clientId;
        }

        private void UpdatePlayer()
        {
            if (updatePlayer)
                return;

            Console.WriteLine("Player name: " + playername);

            updatePlayer = true;
            ThreadManager.AddMainThreadTask(() =>
            {
                updatePlayer = false;
                if (Room != null)
                    WritePacket.UpdatePlayer(Room, this);
            });
        }

        public void Disconnect()
        {
            RoomManager.LeaveRoom(this);
        }
    }
}
