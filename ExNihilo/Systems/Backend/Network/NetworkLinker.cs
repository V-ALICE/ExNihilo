using System.Collections.Generic;
using System.Linq;
using ExNihilo.Sectors;
using Lidgren.Network;
using Microsoft.Xna.Framework;

using MessageStruct = ExNihilo.Systems.Backend.Network.NetworkManager.MessageStruct;

namespace ExNihilo.Systems.Backend.Network
{
    public static class NetworkLinker
    {
        private static GameContainer _gameRef;
        private static PlayerBasedSector _outer, _void;
        public static byte MyMiniID;
        private static long MyUniqueID => NetworkManager.MyUniqueID;

        private class GameAssociation
        {
            public string AssignedName => Name + (MiniID > 0 ? "-" + MiniID : "");

            public long UniqueID;
            public byte MiniID;
            public string Name;
            public int[] charSet;
        }
        private static readonly Dictionary<long, GameAssociation> _lookup = new Dictionary<long, GameAssociation>();

        public static long GetUniqueIDByName(string name)
        {
            if (_lookup.Any(p => p.Value.AssignedName == name))
            {
                return _lookup.First(p => p.Value.AssignedName == name).Value.UniqueID;
            }
            return -1;
        }

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
                //I need to disconnect
                _lookup.Clear();
                _void.ClearPlayers();
                _outer.ClearPlayers();
                if (NetworkManager.Active)
                {
                    GameContainer.Console.ForceMessage("<Asura>", "Disconnected", Color.Purple, Color.White);
                }
            }
            else if (_lookup.TryGetValue(id, out var val))
            {
                //Someone else has disconnected
                GameContainer.Console.ForceMessage("<Asura>", _lookup[id].AssignedName + " has disconnected", Color.Purple, Color.White);
                _void.ClearPlayers(val.UniqueID);
                _outer.ClearPlayers(val.UniqueID);
                _lookup.Remove(id);

                if (NetworkManager.Hosting)
                {
                    //Share disconnect info and name updates with clients
                    NetworkManager.DisconnectClient(id);
                    if (!NetworkManager.Active) return;
                    ReconfigureMiniIDs();
                    ForwardMessage(new MessageStruct((short)NetworkMessageType.Disconnect, MyUniqueID, id));
                    ForwardMessage(new PlayerIntroduction(MyUniqueID, _gameRef.Player.Name, MyMiniID, _gameRef.Player.TextureSet));
                    foreach (var p in _lookup.Values)
                    {
                        ForwardMessage(new PlayerIntroduction(p.UniqueID, p.Name, p.MiniID, p.charSet));
                    }
                }
            }
        }

        private static void ReconfigureMiniIDs()
        {
            foreach (var player in _lookup.Values)
            {
                player.MiniID = 0;
                while ((_gameRef.Player.Name == player.Name && MyMiniID == player.MiniID) ||
                       _lookup.Values.Any(p => p.UniqueID != player.UniqueID && p.Name == player.Name && p.MiniID == player.MiniID))
                {
                    player.MiniID++;
                }
            }
        }

        public static void ForwardMessage(MessageStruct data)
        {
            if (NetworkManager.Hosting) NetworkManager.SendMessage(data);
        }

        public static bool InterpretIncomingMessage(NetIncomingMessage message)
        {
            switch ((NetworkMessageType)message.PeekInt16())
            {
                case NetworkMessageType.NewConnection: //These are taken care of in the manager
                case NetworkMessageType.Heartbeat:
                    return true;
                case NetworkMessageType.BLANK: //This usually means something went wrong with an in-progress message and can be ignored
                    return true;
                case NetworkMessageType.ConsoleMessage:
                    ReadConsoleMessage(message);
                    break;
                case NetworkMessageType.PlayerIntroduction:
                    ReadPlayerIntroduction(message);
                    break;
                case NetworkMessageType.StandardUpdate:
                    ReadStandardUpdate(message);
                    break;
                case NetworkMessageType.Disconnect:
                    ReadDisconnect(message);
                    break;
                default:
                    GameContainer.Console.ForceMessage("<error>", "Received network message of unknown type", Color.DarkRed, Color.White);
                    return false;
            }
            return true;
        }
        private static void ReadPlayerIntroduction(NetIncomingMessage message)
        {
            var data = new PlayerIntroduction(message);
            if (data.SenderUniqueID == MyUniqueID)
            {
                if (!NetworkManager.Hosting) MyMiniID = data.SubID; //Take assigned miniID from host
                return;
            }

            //Either update the existing entry or create a new one if this is a new player
            GameAssociation obj;
            if (_lookup.ContainsKey(data.SenderUniqueID))
            {
                obj = _lookup[data.SenderUniqueID];
                obj.Name = data.Name;
                obj.charSet = data.CharSet;
                obj.MiniID = data.SubID;
            }
            else
            {
                obj = new GameAssociation { UniqueID = data.SenderUniqueID, Name = data.Name, charSet = data.CharSet, MiniID = data.SubID };
                _lookup.Add(data.SenderUniqueID, obj);
            }

            //If hosting make sure there are no name clashes
            if (NetworkManager.Hosting) ReconfigureMiniIDs();
            //Use the incoming data to update/add the player in game
            _outer.UpdatePlayers(obj.UniqueID, obj.AssignedName, obj.charSet);

            //If hosting, inform all clients of all players to keep everyone on the same page
            if (NetworkManager.Hosting)
            {
                ForwardMessage(new PlayerIntroduction(MyUniqueID, _gameRef.Player.Name, MyMiniID, _gameRef.Player.TextureSet));
                foreach (var p in _lookup.Values)
                {
                    ForwardMessage(new PlayerIntroduction(p.UniqueID, p.Name, p.MiniID, p.charSet));
                }
            }
        }
        private static void ReadConsoleMessage(NetIncomingMessage message)
        {
            var data = new ConsoleMessage(message);
            if (!data.IsRecipient(MyUniqueID)) return;

            if (_lookup.TryGetValue(data.SenderUniqueID, out var val))
            {
                GameContainer.Console.ForceMessage("<" + val.AssignedName + ">", data.Message, Color.ForestGreen, Color.White);
                ForwardMessage(data);
            }
        } 
        private static void ReadStandardUpdate(NetIncomingMessage message)
        {
            var data = new StandardUpdate(message);
            if (!data.IsRecipient(MyUniqueID)) return;

            var player = _outer.OtherPlayers.FirstOrDefault(p => p.ID == data.SenderUniqueID);
            player?.ForceValues(data.X, data.Y, data.State);
        }
        private static void ReadDisconnect(NetIncomingMessage message)
        {
            var data = new MessageStruct(message);
            if (data.ReceiverUniqueID == -1 || data.ReceiverUniqueID == NetworkManager.MyUniqueID) NetworkManager.CloseConnections();
            else OnDisconnect(data.ReceiverUniqueID);
        }
    }
}
