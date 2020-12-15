using System;
using System.Net;
using Microsoft.Xna.Framework;

namespace NetworkedGame.Library
{
    public enum PacketType
    {
        EMPTY,
        LOGIN,
        PLAYERJOIN,
        JOINCONFIRMATION,
        DISCONNECT,
        PLAYERPOSITION,
        SCOREUPDATE,
        HEALTHUPDATE
    }

    [Serializable]
    public abstract class Packet
    {
        public PacketType EPacketType { get; protected set; }

        public string GetCurrentTime()
        {
            //return DateTime.Now.ToShortTimeString(); //24 hour clock
            return DateTime.Now.ToString("h:mm:ss tt");
        }
    }

    [Serializable]
    public class EmptyPacket : Packet
    {
        public string Time { get; private set; }
        public EmptyPacket()
        {
            EPacketType = PacketType.EMPTY;
            Time = GetCurrentTime();
        }
    }

    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////


    [Serializable]
    public class LoginPacket : Packet
    {
        public float SpriteX { get; set; }
        public float SpriteY { get; set; }

        public LoginPacket(float x, float y)
        {
            EPacketType = PacketType.LOGIN;
            SpriteX = x;
            SpriteY = y;
        }
    }

    [Serializable]
    public class PlayerJoinPacket : Packet
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public int ExistingPlayers { get; private set; }

        public PlayerJoinPacket(int playerCount, float x, float y)
        {
            EPacketType = PacketType.PLAYERJOIN;
            ExistingPlayers = playerCount;
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public class JoinConfirmationPacket : Packet
    {
        public int ServerIndex { get; private set; }
        public float[] XValues { get; private set; }
        public float[] YValues { get; private set; }

        public JoinConfirmationPacket(int index, float[] xValues, float[] yValues)
        {
            EPacketType = PacketType.JOINCONFIRMATION;
            ServerIndex = index;
            XValues = xValues;
            YValues = yValues;
        }
    }

    [Serializable]
    public class DisconnectPacket : Packet
    {
        public string Time { get; private set; }
        public string ClientName { get; private set; }

        public DisconnectPacket(string name)
        {
            EPacketType = PacketType.DISCONNECT;
            ClientName = name;
            Time = GetCurrentTime();
        }
    }

    [Serializable]
    public class PlayerPositionPacket : Packet
    {
        public int PlayerIndex { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public PlayerPositionPacket(int index, float x, float y)
        {
            EPacketType = PacketType.PLAYERPOSITION;
            PlayerIndex = index;
            X = x;
            Y = y;
        }
    }

    /////Game Data/////
    public class ScoreUpdatePacket : Packet
    {
        public int PlayerIndex { get; set; }
        public int NewScore { get; set; }

        public ScoreUpdatePacket(int index, int score)
        {
            EPacketType = PacketType.SCOREUPDATE;
            PlayerIndex = index;
            NewScore = score;
        }
    }

    public class HealthUpdatePacket : Packet
    {
        public int PlayerIndex { get; set; }
        public int NewHealth { get; set; }

        public HealthUpdatePacket(int index, int health)
        {
            EPacketType = PacketType.HEALTHUPDATE;
            PlayerIndex = index;
            NewHealth = health;
        }
    }
}
