
using System;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems.Backend
{
    public static class NetworkLinker
    {
        public enum NetworkMessageType : short
        {
            NewConnection = -2,
            Heartbeat = -1,
            SayHello,
        }

        public static void SayHelloMessage(NetOutgoingMessage message)
        {
            message.Write((short) NetworkMessageType.SayHello);
            message.Write("Hello!");
        }

        private static bool ReadSayHelloMessage(NetIncomingMessage message)
        {
            var hello = message.ReadString();
            GameContainer.Console.ForceMessage("<message>", hello, Color.DeepSkyBlue, Color.White);
            return true;
        }

        public static bool InterpretIncomingMessage(NetIncomingMessage message)
        {
            var id = message.ReadInt16();
            if (!Enum.IsDefined(typeof(NetworkMessageType), id)) return false;

            switch((NetworkMessageType)id)
            {
                case NetworkMessageType.NewConnection: //These are taken care of in the manager
                case NetworkMessageType.Heartbeat:
                    return true;
                case NetworkMessageType.SayHello:
                    return ReadSayHelloMessage(message);
                default:
                    GameContainer.Console.ForceMessage("<error>", "Received network message of unknown type", Color.DarkRed, Color.White);
                    return false;
            }
        }
    }
}
