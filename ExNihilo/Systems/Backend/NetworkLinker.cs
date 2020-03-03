
using System;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems.Backend
{
    public static class NetworkLinker
    {
        public enum NetworkMessageType : short
        {
            NewConnection = -2,  //int, long
            Heartbeat = -1,      //None
            BLANK,               //None
            ConsoleMessage,      //string
            InitialPlayerUpdate, //string, int, int, int, int
            StandardPlayerUpdate //int, int, int, int, byte
        }

        private static GameContainer _gameRef;

        private struct GameAssociation
        {
            public long UniqueID;
            public string Name;
            public int[] TextureSet;
        }
        private static readonly Dictionary<long, GameAssociation> _lookup = new Dictionary<long, GameAssociation>();

        public static void Initialize(GameContainer reference)
        {
            _gameRef = reference;
        }

        public static void OnDisconnect(long id)
        {
            if (id == -1) _lookup.Clear();
            else if (_lookup.ContainsKey(id))
            {
                GameContainer.Console.ForceMessage("<Asura>", _lookup[id].Name + " has disconnected", Color.Purple, Color.White);
                _lookup.Remove(id);
            }
        }

        public static bool InterpretIncomingMessage(NetIncomingMessage message)
        {
            var id = message.ReadInt16();
            if (!Enum.IsDefined(typeof(NetworkMessageType), id)) return false;

            switch ((NetworkMessageType)id)
            {
                case NetworkMessageType.NewConnection: //These are taken care of in the manager
                case NetworkMessageType.Heartbeat:
                    return true;
                case NetworkMessageType.BLANK:
                    return true;
                case NetworkMessageType.ConsoleMessage:
                    return ReadConsoleMessage(message);
                case NetworkMessageType.InitialPlayerUpdate:
                    return ReadInitialPlayerUpdate(message);
                case NetworkMessageType.StandardPlayerUpdate:
                    return false;
                default:
                    GameContainer.Console.ForceMessage("<error>", "Received network message of unknown type", Color.DarkRed, Color.White);
                    return false;
            }
        }

        public static void ConsoleMessage(object data, NetOutgoingMessage message)
        {
            message.Write((short) NetworkMessageType.ConsoleMessage);
            var name = _gameRef.Player?.Name ?? (NetworkManager.Hosting ? "Host" : "Client");
            message.Write(name);
            message.Write((string)data);
        }
        private static bool ReadConsoleMessage(NetIncomingMessage message)
        {
            var name = message.ReadString();
            var data = message.ReadString();
            GameContainer.Console.ForceMessage("<" + name + ">", data, Color.DeepSkyBlue, Color.White);
            NetworkManager.ForwardMessageToClients(message.SenderConnection.RemoteUniqueIdentifier, name, data);
            return true;
        }

        private static void InitialPlayerUpdate(object data, NetOutgoingMessage message)
        {
            if (_gameRef.Player is null)
            {
                message.Write((short)NetworkMessageType.BLANK);
                return;
            }

            message.Write((short) NetworkMessageType.InitialPlayerUpdate);
            message.Write(_gameRef.Player.Name);
            var charMap = (int[]) data;
            message.Write(charMap[0]);
            message.Write(charMap[1]);
            message.Write(charMap[2]);
            message.Write(charMap[3]);
        }
        private static bool ReadInitialPlayerUpdate(NetIncomingMessage message)
        {
            var id = message.SenderConnection.RemoteUniqueIdentifier;
            if (_lookup.ContainsKey(id)) return true;

            var name = message.ReadString();
            var charMap = new[] {message.ReadInt32(), message.ReadInt32(), message.ReadInt32(), message.ReadInt32()};          
            _lookup.Add(id, new GameAssociation {UniqueID = id, Name = name, TextureSet = charMap});
            return true;
        }

    }
}
