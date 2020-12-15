using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkedGame.Library;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace NetworkedGame
{
    public class Client
    {
        //networking
        private TcpClient _tcpClient;
        private UdpClient _udpClient;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private BinaryFormatter _binaryFormatter;

        //game
        private NetworkedGame _game;

        public Client(NetworkedGame game)
        {
            //networking
            _tcpClient = new TcpClient();
            _udpClient = new UdpClient();

            //game
            _game = game;
        }

        //CONNECT/DISCONNECT FUNCTIONALITY

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                _tcpClient.Connect(ipAddress, port);
                _udpClient.Connect(ipAddress, port);
                _stream = _tcpClient.GetStream();
                _writer = new BinaryWriter(_stream, Encoding.UTF8);
                _reader = new BinaryReader(_stream, Encoding.UTF8);
                _binaryFormatter = new BinaryFormatter();

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
                return false;
            }
        }

        public void Login(float x, float y)
        {
            try
            {
                LoginPacket loginPacket = new LoginPacket(x, y);
                TCPSendPacket(loginPacket);
            }
            catch (Exception e)
            {
                Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
            }
        }

        //TCP FUNCTIONALITY

        public void Run()
        {

            //start TCP thread for recieving packets
            Thread tcpThread = new Thread(() => { TCPProcessServerResponse(); });
            tcpThread.Start();

            //start UDP thread for recieving packets
            Thread udpThread = new Thread(() => { UDPProcessServerResponse(); });
            udpThread.Start();
        }

        private void TCPProcessServerResponse()
        {
            Packet receivedPacket;

            while ((receivedPacket = TCPRead()) != null)
            {
                try
                {
                    switch (receivedPacket.EPacketType)
                    {
                        case PacketType.EMPTY:
                            EmptyPacket emptyPacket = (EmptyPacket)receivedPacket;
                            break;
                        case PacketType.PLAYERJOIN:
                            PlayerJoinPacket playerJoinPacket = (PlayerJoinPacket)receivedPacket;
                            if(_game.localServerIndex != playerJoinPacket.ExistingPlayers)
                                _game.InitialiseNewPlayer(playerJoinPacket.X, playerJoinPacket.Y);
                            break;
                        case PacketType.JOINCONFIRMATION:
                            JoinConfirmationPacket joinConfirmationPacket = (JoinConfirmationPacket)receivedPacket;
                            _game.localServerIndex = joinConfirmationPacket.ServerIndex;

                            if(joinConfirmationPacket.ServerIndex > 0)
                            {
                                _game.LoadExistingPlayers(joinConfirmationPacket.ServerIndex, joinConfirmationPacket.XValues, joinConfirmationPacket.YValues);
                            }

                            _game.InitialiseNewPlayer(0.0f, 0.0f);
                            break;
                        case PacketType.PLAYERPOSITION:
                            PlayerPositionPacket playerPositionPacket = (PlayerPositionPacket)receivedPacket;
                            _game.ChangePlayerPosition(playerPositionPacket.PlayerIndex, playerPositionPacket.X, playerPositionPacket.Y);
                            break;
                    }
                }
                catch (SocketException e)
                {
                    Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
                    break;
                }
            }
        }

        public void TCPSendPacket(Packet packet)
        {
            if (_tcpClient.Connected)
            {
                TCPSerialize(packet);
            }
        }

        public Packet TCPRead()
        {
            try
            {
                int numberOfBytes;
                if ((numberOfBytes = _reader.ReadInt32()) != -1)
                {
                    byte[] buffer = _reader.ReadBytes(numberOfBytes);
                    MemoryStream memoryStream = new MemoryStream(buffer);
                    return _binaryFormatter.Deserialize(memoryStream) as Packet;
                }
                else
                {
                    return null;
                }
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
                return null;
            }
        }

        private void TCPSerialize(Packet packet)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                _binaryFormatter.Serialize(memoryStream, packet);
                byte[] buffer = memoryStream.GetBuffer();
                _writer.Write(buffer.Length);
                _writer.Write(buffer);
                _writer.Flush();
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
            }
        }

        //UDP FUNCTIONALITY
        public void UDPSendPacket(Packet packet)
        {
            //serialize and send packet
            UDPSerialize(packet);
        }

        private void UDPProcessServerResponse()
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] bytes = _udpClient.Receive(ref ipEndPoint);
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    Packet receivedPacket = _binaryFormatter.Deserialize(memoryStream) as Packet;

                    try
                    {
                        switch (receivedPacket.EPacketType)
                        {
                            case PacketType.EMPTY:
                                break;
                            default:
                                break;
                        }
                    }
                    catch (SocketException e)
                    {
                        Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
                        break;
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
            }
        }

        private void UDPSerialize(Packet packet)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                _binaryFormatter.Serialize(memoryStream, packet);
                byte[] buffer = memoryStream.GetBuffer();
                _udpClient.Send(buffer, buffer.Length);
            }
            catch (SerializationException e)
            {
                Debug.WriteLine("[Error] " + e.Message + e.StackTrace);
            }
        }

        private void Close()
        {
            _tcpClient.Close();
            _udpClient.Close();
            _stream.Close();
            _reader.Close();
            _writer.Close();
        }
    }
}