using GameServer.Network.Packets;
using System;
using System.Net.Sockets;

namespace GameServer.Network.Protocols
{
    internal class TCP
    {
        public TcpClient socket;
        public readonly Client client;
        
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(Client _client)
        {
            client = _client;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = Client.dataBufferSize;
            socket.SendBufferSize = Client.dataBufferSize;

            stream = socket.GetStream();
            receivedData = new Packet();
            receiveBuffer = new byte[Client.dataBufferSize];
            stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
        }

        public void Disconnect()
        {
            if (socket != null && socket.Connected)
            {
                socket.Close();
                socket = null;
            }

            stream = null;
            receivedData = null;
            receiveBuffer = null;
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to player {client.clientId} via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            if (!client.Connected)
                return;

            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    client.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
                client.Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            receivedData.SetBytes(_data);

            int _packetLength;
            if (!ReadPacketLength(out _packetLength))
                return true;

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.AddMainThreadTask(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        try
                        {
                            int _packetId = _packet.ReadInt();

                            if (!client.VersionChecked && _packetId != (int)ClientPackets.version)
                            {
                                WritePacket.FailToConnect(client.clientId, 0); // bad version
                                Console.WriteLine($"Client ({client.clientId}) sent a packet (id:{_packetId}) without checking version.");
                                client.Disconnect(100);
                                return;
                            }

                            Server.packetHandlers[_packetId](client.clientId, _packet);
                        }
                        catch (Exception _ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(_ex.Message);
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine(_ex.StackTrace);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                });

                if (!ReadPacketLength(out _packetLength))
                    return true;
            }

            return (_packetLength <= 1);
        }

        private bool ReadPacketLength(out int _packetLength)
        {
            _packetLength = 0;
            if (receivedData.UnreadLength() >= 4)
                _packetLength = receivedData.ReadInt();

            return (_packetLength > 0);
        }
    }
}
