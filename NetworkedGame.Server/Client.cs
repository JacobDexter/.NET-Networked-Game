﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkedGame.Library;
using System.Runtime.Serialization;

namespace NetworkedGame.Server
{
    class Client
    {
        public struct ClientData
        {
            public string clientNickname;
            public IPEndPoint ipEndPoint;
            public float posX;
            public float posY;
            public int playerIndex;
        }

        private Socket _socket;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private BinaryFormatter _binaryFormatter;
        private object _readLock;
        private object _writeLock;
        public ClientData clientData;

        public Client(Socket socket, string nickname)
        {
            _readLock = new object();
            _writeLock = new object();
            _socket = socket;
            _stream = new NetworkStream(socket);
            _writer = new BinaryWriter(_stream, Encoding.UTF8);
            _reader = new BinaryReader(_stream, Encoding.UTF8);
            _binaryFormatter = new BinaryFormatter();
            clientData.clientNickname = nickname;
        }

        public void Close()
        {
            //close all connections
            _stream.Close();
            _reader.Close();
            _writer.Close();
            _socket.Close();
        }

        public Packet TCPRead()
        {
            try
            {
                lock (_readLock)
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
            }
            catch (Exception e)
            {
                Console.WriteLine("[Error] " + e.Message + e.StackTrace);
                return null;
            }
        }

        public void TCPSend(Packet packet)
        {
            lock (_writeLock)
            {
                Serialize(packet);
            }
        }

        private void Serialize(Packet packet)
        {
            MemoryStream memoryStream = new MemoryStream();
            _binaryFormatter.Serialize(memoryStream, packet);
            byte[] buffer = memoryStream.GetBuffer();
            _writer.Write(buffer.Length);
            _writer.Write(buffer);
            _writer.Flush();
        }
    }
}