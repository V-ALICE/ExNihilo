using System;
using Lidgren.Network;

namespace ExNihilo.Systems.Backend.Network
{
    public enum NetworkMessageType : short
    {
        NewConnection = -2,
        Heartbeat = -1,
        BLANK,
        
        ConsoleMessage,
        PlayerIntroduction,
        StandardUpdate,
        //SetAscended,
        //IdentifyRequest,
        Disconnect,
    }

    public class ConsoleMessage : NetworkManager.MessageStruct
    {
        public string Message;

        public ConsoleMessage(long senderId, string msg) : base((short)NetworkMessageType.ConsoleMessage, senderId)
        {
            Message = msg;
        }

        public ConsoleMessage(NetIncomingMessage message) : base(message)
        {
            try
            {
                Message = message.ReadString();
            }
            catch (Exception)
            {
                Valid = false;
            }
        }

        public override void WriteOut(NetOutgoingMessage message)
        {
            if (!Valid) return;
            base.WriteOut(message);
            message.Write(Message);
        }
    }

    public class PlayerIntroduction : NetworkManager.MessageStruct
    {
        public string Name;
        public byte SubID;
        public int[] CharSet;
        public byte R, G, B;

        public PlayerIntroduction(long senderId, string name, byte subId, byte r, byte g, byte b, int[] charSet) : base((short) NetworkMessageType.PlayerIntroduction, senderId)
        {
            Name = name;
            SubID = subId;
            CharSet = charSet;
            R = r;
            G = g;
            B = b;
        }

        public PlayerIntroduction(NetIncomingMessage message) : base(message)
        {
            try
            {
                Name = message.ReadString();
                SubID = message.ReadByte();
                R = message.ReadByte();
                G = message.ReadByte();
                B = message.ReadByte();
                CharSet = new[] {message.ReadInt32(), message.ReadInt32(), message.ReadInt32(), message.ReadInt32()};
            }
            catch (Exception)
            {
                Valid = false;
            }
        }

        public override void WriteOut(NetOutgoingMessage message)
        {
            if (!Valid) return;
            base.WriteOut(message);
            message.Write(Name);
            message.Write(SubID);
            message.Write(R);
            message.Write(G);
            message.Write(B);
            message.Write(CharSet[0]);
            message.Write(CharSet[1]);
            message.Write(CharSet[2]);
            message.Write(CharSet[3]);
        }
    }

    public class StandardUpdate : NetworkManager.MessageStruct
    {
        public float X, Y;
        public sbyte State;

        public StandardUpdate(long senderId, float x, float y, sbyte state) : base((short)NetworkMessageType.StandardUpdate, senderId)
        {
            X = x;
            Y = y;
            State = state;
        }

        public StandardUpdate(NetIncomingMessage message) : base(message)
        {
            try
            {
                X = message.ReadFloat();
                Y = message.ReadFloat();
                State = message.ReadSByte();
            }
            catch (Exception)
            {
                Valid = false;
            }
        }

        public override void WriteOut(NetOutgoingMessage message)
        {
            if (!Valid) return;
            base.WriteOut(message);
            message.Write(X);
            message.Write(Y);
            message.Write(State);
        }
    }
}
