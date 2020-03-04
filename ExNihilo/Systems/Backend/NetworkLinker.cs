
using System;
using System.Collections.Generic;
using System.Linq;
using ExNihilo.Sectors;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace ExNihilo.Systems.Backend
{
    public static class NetworkLinker
    {
        public enum NetworkMessageType : short
        {
            NewConnection = -2,   //int_version, long_id
            Heartbeat = -1,       //None
            BLANK,                //None
            InitialPlayerUpdate,  //long_id, string_name, int[4]_charSet, byte_subid
            ConsoleMessage,       //long_id, string_message
            StandardPlayerUpdate, //long_id, float_xpos, float_ypos, float scale, sbyte_state
            SetAscended,          //long_id, bool_ascend
            IdentifyRequest,      //long_id
            Disconnect,           //long_id
        }

        private static GameContainer _gameRef;
        private static PlayerBasedSector _outer, _void;
        public static byte _myMiniID;

        private class GameAssociation
        {
            public string AssignedName => Name + (MiniID > 0 ? "-" + MiniID : "");

            public long UniqueID;
            public byte MiniID;
            public string Name;
            public int[] charSet;
        }
        private static readonly Dictionary<long, GameAssociation> _lookup = new Dictionary<long, GameAssociation>();

        public static void Initialize(GameContainer reference, VoidSector voids, OuterworldSector outer)
        {
            _gameRef = reference;
            _void = voids;
            _outer = outer;
        }

        public static void OnDisconnect(long id)
        {
            if (id == -1 || id == NetworkManager.MyUniqueID)
            {
                _lookup.Clear();
                _void.ClearPlayers();
                _outer.ClearPlayers();
            }
            else if (_lookup.TryGetValue(id, out var val))
            {
                GameContainer.Console.ForceMessage("<Asura>", _lookup[id].AssignedName + " has disconnected", Color.Purple, Color.White);
                _void.ClearPlayers(val.AssignedName);
                _outer.ClearPlayers(val.AssignedName);
                _lookup.Remove(id);
                
                if (NetworkManager.Hosting)
                {
                    NetworkManager.DisconnectClient(id);
                    ReconfigureMiniIDs();
                    NetworkManager.SendMessage(new object[] {id}, Disconnect);
                    foreach (var p in _lookup.Values)
                    {
                        ForwardMessage(new object[] { p.UniqueID, p.Name, p.charSet, p.MiniID }, InitialPlayerUpdate);
                    }
                }
            }
        }

        private static void ReconfigureMiniIDs()
        {
            foreach (var player in _lookup.Values)
            {
                while ((_gameRef.Player.Name == player.Name && _myMiniID == player.MiniID) ||
                       _lookup.Values.Any(p => p.UniqueID != player.UniqueID && p.Name == player.Name && p.MiniID == player.MiniID))
                {
                    player.MiniID++;
                }
            }
        }

        public static void ForwardMessage(object[] data, Action<object[], NetOutgoingMessage> action)
        {
            if (NetworkManager.Hosting) NetworkManager.SendMessage(data, action);
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
                case NetworkMessageType.BLANK: //This usually means something went wrong with an in-progress message and can be ignored
                    return true;
                case NetworkMessageType.ConsoleMessage:
                    return ReadConsoleMessage(message);
                case NetworkMessageType.InitialPlayerUpdate:
                    return ReadInitialPlayerUpdate(message);
                case NetworkMessageType.StandardPlayerUpdate:
                    return ReadStandardPlayerUpdate(message);
                case NetworkMessageType.Disconnect:
                    return ReadDisconnect(message);
                default:
                    GameContainer.Console.ForceMessage("<error>", "Received network message of unknown type", Color.DarkRed, Color.White);
                    return false;
            }
        }

        //Sent on first connect and whenever someone changes characters
        public static void InitialPlayerUpdate(object[] data, NetOutgoingMessage message)
        {
            message.Write((short) NetworkMessageType.InitialPlayerUpdate);
            message.Write((long)data[0]);
            message.Write((string)data[1]);
            var charSet = (int[])data[2];
            message.Write(charSet[0]);
            message.Write(charSet[1]);
            message.Write(charSet[2]);
            message.Write(charSet[3]);
            message.Write((byte)data[3]);
        }
        private static bool ReadInitialPlayerUpdate(NetIncomingMessage message)
        {
            //long_id, string_name, int[4]_charSet, int_subid
            var id = message.ReadInt64();
            var name = message.ReadString();
            var charMap = new[] { message.ReadInt32(), message.ReadInt32(), message.ReadInt32(), message.ReadInt32() };
            var miniID = message.ReadByte();

            if (id == NetworkManager.MyUniqueID)
            {
                if (!NetworkManager.Hosting) _myMiniID = miniID; //Take assigned miniID from host
                return true;
            }

            if (_lookup.ContainsKey(id))
            {
                _lookup[id].Name = name;
                _lookup[id].charSet = charMap;
                _lookup[id].MiniID = miniID;
            }
            else
            {
                _lookup.Add(id, new GameAssociation {UniqueID = id, Name = name, charSet = charMap, MiniID = miniID});
            }

            if (NetworkManager.Hosting) ReconfigureMiniIDs();
            _outer.UpdatePlayers(_lookup[id].AssignedName, charMap);

            if (NetworkManager.Hosting)
            {
                NetworkManager.SendMessage(new object[] { NetworkManager.MyUniqueID, _gameRef.Player.Name, _gameRef.Player.TextureSet, _myMiniID }, InitialPlayerUpdate);
                foreach (var p in _lookup.Values)
                {
                    ForwardMessage(new object[] { p.UniqueID, p.Name, p.charSet, p.MiniID }, InitialPlayerUpdate);
                }
            }
            
            return true;
        }

        //Sent whenever someone writes a message in the chat console (not commands)
        public static void ConsoleMessage(object[] data, NetOutgoingMessage message)
        {
            message.Write((short)NetworkMessageType.ConsoleMessage);
            message.Write((long)data[0]);
            message.Write((string)data[1]);
        }
        private static bool ReadConsoleMessage(NetIncomingMessage message)
        {
            //long_id, string_message
            var id = message.ReadInt64();
            if (id == NetworkManager.MyUniqueID) return true;

            if (_lookup.TryGetValue(id, out var val))
            {
                var name = val.AssignedName;
                var data = message.ReadString();
                GameContainer.Console.ForceMessage("<" + name + ">", data, Color.ForestGreen, Color.White);
                ForwardMessage(new object[] { id, data }, ConsoleMessage);
            }

            return true;
        }

        //Sent constantly to update position and direction of players
        public static void StandardPlayerUpdate(object[] data, NetOutgoingMessage message)
        {
            message.Write((short)NetworkMessageType.StandardPlayerUpdate);
            message.Write((long)data[0]);
            message.Write((float)data[1]);
            message.Write((float)data[2]);
            message.Write((float)data[3]);
            message.Write((sbyte)data[4]);
        }
        private static bool ReadStandardPlayerUpdate(NetIncomingMessage message)
        {
            //long_id, float_xpos, float_ypos, float scale, sbyte_state
            var id = message.ReadInt64();
            if (id == NetworkManager.MyUniqueID) return true;

            if (_lookup.TryGetValue(id, out var val))
            {
                var player = _outer.OtherPlayers.FirstOrDefault(p => p.Name == val.AssignedName);
                player?.ForceValues(message.ReadFloat(), message.ReadFloat(), message.ReadFloat(), message.ReadSByte());
            }

            return true;
        }

        //Sent by host when someone disconnects, or by a anyone when they're disconnecting
        public static void Disconnect(object[] data, NetOutgoingMessage message)
        {
            message.Write((short)NetworkMessageType.Disconnect);
            message.Write((long)data[0]);
        }
        private static bool ReadDisconnect(NetIncomingMessage message)
        {
            //long_id
            var id = message.ReadInt64();
            if (id == NetworkManager.MyUniqueID || id == -1) NetworkManager.CloseConnections();
            else OnDisconnect(id);
            return true;
        }
    }
}
