using System;
using System.Collections.Generic;
using GameServer.Network.Protocols;
using GameServer.Network.Packets;
using GameServer.Timebomb;
using System.Net.Sockets;

namespace GameServer.Network
{
    class Client
    {
        public static int dataBufferSize = 4096;
        public int clientId;
        public TCP tcp;

        public Player Player { get; private set; }
        public bool Connected { get; private set; }
        public bool VersionChecked { get; private set; }

        public Client(int _clientId)
        {
            clientId = _clientId;
            Connected = false;
            VersionChecked = false;
        }

        public void Connect(TcpClient _client)
        {
            Console.WriteLine($"Player with client {clientId} connected to server.");
            Player = new Player(this);
            Connected = true;
            tcp = new TCP(this);
            tcp.Connect(_client);
            WritePacket.Welcome(clientId);
        }

        public void CheckVersion(string _version)
        {
            VersionChecked = _version == Settings.VERSION;
            if (!VersionChecked)
            {
                Console.WriteLine($"Client {clientId} kicked from server for using bad version ({_version}).");
                Disconnect();
            }
        }

        private bool isPendingDisconnect = false;
        public void Disconnect(int _delay)
        {
            if (!isPendingDisconnect)
            {
                isPendingDisconnect = true;
                ThreadManager.DelayTask(_delay, Disconnect);
            }
        }

        public void Disconnect()
        {
            if (!Connected)
                return;
            
            Connected = false;
            if (Player != null)
                Console.WriteLine($"Player {Player.Playername} ({clientId}) disconnected from server.");
            else
                Console.WriteLine($"Client ({clientId}) disconnected from server.");

            if (tcp != null)
            {
                tcp.Disconnect();
                tcp = null;
            }

            if (Player != null)
            {
                Player.Disconnect();
                Player = null;
            }
        }
    }
}
