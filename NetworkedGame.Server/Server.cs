using NetworkedGame.Library;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace NetworkedGame.Server
{
    class Server
    {
        private TcpListener _tcpListener;
        private UdpClient _udpListener;
        private ConcurrentDictionary<int, Client> _clients; //thread safe collection of values
        private BinaryFormatter _binaryFormatter;

        public Server(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            _tcpListener = new TcpListener(ip, port);
            _udpListener = new UdpClient(port);
            _binaryFormatter = new BinaryFormatter();
        }

        public void Start()
        {
            _clients = new ConcurrentDictionary<int, Client>();
            _tcpListener.Start();
            Console.WriteLine("Server successfully started!");

            int clientIndex = 0;

            while (true)
            {
                try
                {
                    int index = clientIndex;
                    clientIndex++;

                    Socket socket = _tcpListener.AcceptSocket();

                    //create client instance and add to client list
                    Client clientInstance = new Client(socket, "Client " + (index + 1));
                    _clients.TryAdd(index, clientInstance);

                    Console.WriteLine("A client has joined the server!");

                    //start tcp packet interpreter
                    Thread tcpThread = new Thread(() => { TCPClientMethod(index); });
                    tcpThread.Start();

                    //start udp packet interpreter
                    Thread udpThread = new Thread(() => { UDPListen(); });
                    udpThread.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error] " + e.Message + e.StackTrace);
                    break;
                }
            }
        }

        public void Stop()
        {
            _tcpListener.Stop();
            _udpListener.Close();
        }

        private void TCPClientMethod(int index)
        {
            Packet receivedPacket;

            //loop to allow continuous communication
            while ((receivedPacket = _clients[index].TCPRead()) != null)
            {
                try
                {
                    switch (receivedPacket.EPacketType)
                    {
                        case PacketType.EMPTY:
                            EmptyPacket emptyPacket = (EmptyPacket)receivedPacket;
                            break;
                        case PacketType.LOGIN:
                            LoginPacket loginPacket = (LoginPacket)receivedPacket;
                            _clients[index].clientData.posX = loginPacket.SpriteX;
                            _clients[index].clientData.posY = loginPacket.SpriteY;
                            _clients[index].clientData.playerIndex = (_clients.Count - 1);

                            //confirm connection to joining player
                            JoinConfirmationPacket joinConfirmationPacket = new JoinConfirmationPacket((_clients.Count - 1), GetAllPlayerXValues(), GetAllPlayerYValues());
                            _clients[index].TCPSend(joinConfirmationPacket);

                            PlayerJoinPacket playerJoinPacket = new PlayerJoinPacket(_clients.Count - 1, loginPacket.SpriteX, loginPacket.SpriteY);
                            Console.WriteLine(_clients.Count);
                            TCPSendPacketToAll(playerJoinPacket);
                            Console.WriteLine("PACKETS SENT");
                            break;
                        case PacketType.DISCONNECT:
                            DisconnectPacket disconnectRequestPacket = (DisconnectPacket)receivedPacket;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Error] " + e.Message + e.StackTrace);
                    break;
                }
            }

            try
            {
                _clients[index].Close();

                //Take client out of client ConcurrentBag
                _clients.TryRemove(index, out Client c);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error] " + e.Message + e.StackTrace);
            }
        }

        private void TCPSendPacketToAll(Packet packet)
        {
            foreach (Client c in _clients.Values)
            {
                c.TCPSend(packet);
            }
        }

        private void UDPListen()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] bytes = _udpListener.Receive(ref ipEndPoint);
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    Packet receivedPacket = _binaryFormatter.Deserialize(memoryStream) as Packet;

                    try
                    {
                        switch (receivedPacket.EPacketType)
                        {
                            case PacketType.PLAYERPOSITION:
                                PlayerPositionPacket playerPositionPacket = (PlayerPositionPacket)receivedPacket;
                                _clients[playerPositionPacket.PlayerIndex].clientData.posX = playerPositionPacket.X;
                                _clients[playerPositionPacket.PlayerIndex].clientData.posY = playerPositionPacket.Y;
                                Console.WriteLine(playerPositionPacket.PlayerIndex + " (" + _clients[playerPositionPacket.PlayerIndex].clientData.posX + "," + _clients[playerPositionPacket.PlayerIndex].clientData.posY + ")");
                                TCPSendPacketToAll(playerPositionPacket);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine("[Error] " + e.Message + e.StackTrace);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error] " + e.Message + e.StackTrace);
            }
        }

        private float[] GetAllPlayerXValues()
        {
            float[] xValues = new float[_clients.Count - 1];
            for(int i = 0; i < (_clients.Count - 1); i++)
            {
                xValues[i] = _clients[i].clientData.posX;
            }
            return xValues;
        }

        private float[] GetAllPlayerYValues()
        {
            float[] yValues = new float[_clients.Count - 1];
            for (int i = 0; i < (_clients.Count - 1); i++)
            {
                yValues[i] = _clients[i].clientData.posY;
            }
            return yValues;
        }
    }
}
