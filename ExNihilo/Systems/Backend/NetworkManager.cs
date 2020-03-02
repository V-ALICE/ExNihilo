using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Lidgren.Network;

namespace ExNihilo.Systems.Backend
{
    public static class NetworkManager
    {
        private class ClientInfoPackage
        {
            public long UserID;
            public float TimeSinceLastHeartbeat;

            public static float operator +(ClientInfoPackage me, float add)
            {
                me.TimeSinceLastHeartbeat += add;
                return me.TimeSinceLastHeartbeat;
            }
        }

        private static int _maxConnections;
        private static float _connectionTimeout; //How long to try to connect to the host initially
        private static float _heartbeatTimeout; //How long between heartbeats to consider the connection dead

        private static bool Active => _host != null || _client != null;
        private static bool Connected => _client?.ConnectionStatus == NetConnectionStatus.Connected || _host?.ConnectionsCount > 0;
        private static NetPeer Connection => _hosting ? (NetPeer) _host : (NetPeer) _client;

        private static long _myUniqueID;
        private static bool _hosting, _attemptingToConnect;
        private static float _connectionTimer, _heartbeatTimer;
        private static string _ipInUse = "";
        private static NetServer _host;
        private static NetClient _client;
        private static NetConnection _hostRef;
        private static List<ClientInfoPackage> _link;

        public static string LastError, LastMessage;

        public static void Initialize(int maxConnections, float connectionTimeout, float heartbeatTimeout)
        {
            _maxConnections = maxConnections;
            _connectionTimeout = connectionTimeout;
            _heartbeatTimeout = heartbeatTimeout;

            _link = new List<ClientInfoPackage>(_maxConnections);
        }

        public static void CloseConnections()
        {
            Connection?.Shutdown("bye");
            _host = null;
            _client = null;
            _hostRef = null;
            _hosting = _attemptingToConnect = false;
            _ipInUse = "";
            _myUniqueID = 0;
            _connectionTimer = _heartbeatTimer = 0;
            _link = new List<ClientInfoPackage>(_maxConnections);
        }

        public static bool StartNewHost(string id, int port)
        {
            if (Active) return false;

            var config = new NetPeerConfiguration(id)
            {
                Port = port,
                MaximumConnections = _maxConnections
            };
            _hosting = true;
            _myUniqueID = -1;

            try
            {
                _host = new NetServer(config);
                _host.Start();
            }
            catch (NetException e)
            {
                LastError = e.Message;
                CloseConnections();
                return false;
            }

            return true;
        }

        public static bool ConnectToHost(string id, string hostIP, int port)
        {
            if (Active) return false;

            var config = new NetPeerConfiguration(id);
            _hosting = false;
            _ipInUse = hostIP;

            try
            {
                _client = new NetClient(config);
                _attemptingToConnect = true;
                _client.Connect(hostIP, port);
            }
            catch (NetException e)
            {
                LastError = e.Message;
                CloseConnections();
                return false;
            }

            return true;
        }

        public static void Update(float elapsedTimeSec, Action<short, NetIncomingMessage> output)
        {
            if (!Active) return;

            if (_attemptingToConnect)
            {
                _connectionTimer += elapsedTimeSec;
                if (_connectionTimer > _connectionTimeout)
                {
                    //Connection Timeout
                    CloseConnections();
                    LastError = "Failed to connect to host";
                    return;
                }
            }

            if (!Connected) return;

            if (_hosting)
            {
                for (int i = 0; i < _link.Count; i++)
                {
                    if (_link[i].TimeSinceLastHeartbeat + elapsedTimeSec > _heartbeatTimeout)
                    {
                        //Lost Connection to Client
                        LastError = "Lost connection to client with ID " + _link[i].UserID;
                        _link[i] = new ClientInfoPackage();
                    }
                }
            }
            else
            {
                _heartbeatTimer += elapsedTimeSec;
                if (_heartbeatTimer > _heartbeatTimeout)
                {
                    //Lost Connection to Host
                    CloseConnections();
                    LastError = "Lost connection to host";
                }
            }

            NetIncomingMessage message;
            while ((message = Connection.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Error:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                        LastError = message.ReadString();
                        break;
                    case NetIncomingMessageType.Data:
                        var messageID = message.ReadInt16();
                        if (_hosting)
                        {
                            var client = _link.FirstOrDefault(c => c.UserID == message.SenderConnection.RemoteUniqueIdentifier);
                            if (client is null) break;
                            client.TimeSinceLastHeartbeat = 0;
                            output.Invoke(messageID, message);
                        }
                        else
                        {
                            _heartbeatTimer = 0;
                            if (messageID == 0)
                            {
                                if (_hosting) break;
                                _myUniqueID = message.ReadInt64();
                            }
                            else output.Invoke(messageID, message);
                        }

                        break;
                    case NetIncomingMessageType.StatusChanged:
                        var connectionID = message.SenderConnection.RemoteUniqueIdentifier;
                        switch ((NetConnectionStatus) message.ReadByte())
                        {
                            case NetConnectionStatus.Connected:
                                if (_hosting)
                                {
                                    LastMessage = "New client with ID " + connectionID + " has connected";
                                    var newConnection = new ClientInfoPackage {UserID = connectionID};
                                    _link.Add(newConnection);
                                    var msg = _host.CreateMessage();
                                    msg.Write((short) 0);
                                    msg.Write(connectionID);
                                    _host.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                }
                                else
                                {
                                    _attemptingToConnect = false;
                                    _hostRef = message.SenderConnection;
                                    LastMessage = "Successfully connected to host";
                                }

                                break;
                            case NetConnectionStatus.Disconnected:
                                LastMessage = "Client with ID " + connectionID + " has disconnected";
                                for (int i = 0; i < _link.Count; i++)
                                {
                                    if (_link[i].UserID == connectionID) _link[i] = new ClientInfoPackage();
                                }

                                break;
                        }

                        break;
                }

                Connection.Recycle(message);
            }
        }

        public static void SendMessage(short id, Action<NetOutgoingMessage> filler)
        {
            if (!Connected) return;
            var message = Connection.CreateMessage();
            message.Write(id);
            filler.Invoke(message);
            if (_hosting) _host.SendToAll(message, NetDeliveryMethod.ReliableOrdered);
            else _client.SendMessage(message, _hostRef, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
