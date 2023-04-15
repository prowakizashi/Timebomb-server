using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GameServer.Network.Packets;

namespace GameServer.Network
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public static IDictionary<int, Client> clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int _fromclient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine($"Starting Server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on port {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (!clients[i].Connected)
                {
                    clients[i].Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: server is full!");
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int) ClientPackets.version, ReadPacket.CheckVersion },
                { (int) ClientPackets.goodBye, ReadPacket.Disconnect },
                { (int) ClientPackets.updatePlayer, ReadPacket.UpdatePlayer },
                { (int) ClientPackets.createRoom, ReadPacket.CreateRoom },
                { (int) ClientPackets.joinRoom, ReadPacket.JoinRoom },
                { (int) ClientPackets.leaveRoom, ReadPacket.LeaveRoom },
                { (int) ClientPackets.updateRoom, ReadPacket.UpdateRoom },
                { (int) ClientPackets.changePlayerLeader, ReadPacket.ChangePlayerLeader },
                { (int) ClientPackets.tryStartGame, ReadPacket.TryStartGame },
                { (int) ClientPackets.isReady, ReadPacket.IsGameReady },
                { (int) ClientPackets.hoverCard, ReadPacket.HoverACard },
                { (int) ClientPackets.watchCards, ReadPacket.WatchCards },
                { (int) ClientPackets.pickCard, ReadPacket.PickACard },
                { (int) ClientPackets.returnToLobby, ReadPacket.ReturnToLobby },
                { (int) ClientPackets.playerMessage, ReadPacket.SendMessage }
            };

            Console.WriteLine("Initialized packets.");
        }
    }
}
