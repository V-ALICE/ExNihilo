using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Lidgren.Network;

namespace ExNihilo.Systems.Backend
{
    public static class NetworkManager
    {
        private class ClientInfoPackage
        {
            public long UserID;
            public double TimeSinceLastHeartbeat;
            public double LastPingTime;

            public static double operator +(ClientInfoPackage me, double add)
            {
                me.TimeSinceLastHeartbeat += add;
                return me.TimeSinceLastHeartbeat;
            }
        }

        private enum BasicMessageTypes : short
        {
            UniqueID = -2,
            Heartbeat = -1
        }

        private static int _maxConnections;
        private static float _connectionTimeout; //How long to try to connect to the host initially
        private static float _heartbeatTimeout;  //How long between heartbeats to consider the connection dead
        private static float _heartbeatRate;     //How long between sending heartbeats
        private static float _hostUpdateRate;    //How long between state updates from host to clients

        public static bool Active => _host != null || _client != null;
        public static bool Connected => _client?.ConnectionStatus == NetConnectionStatus.Connected || _host?.ConnectionsCount > 0;
        private static NetPeer Connection => _hosting ? (NetPeer) _host : (NetPeer) _client;

        private static long _myUniqueID;
        private static bool _hosting;
        private static double _heartbeatTimer, _sendHeartbeatTimer, _updateTimer;
        private static double _lastPingTime;
        private static NetServer _host;
        private static NetClient _client;
        private static List<ClientInfoPackage> _link;

        private static string _lastError, _lastNotice;
        public static string LastError
        {
            get => _lastError;
            set
            {
                _lastError = value;
                Console.WriteLine("NETWORK ERROR: " + _lastError);
            }
        }
        public static string LastNotice
        {
            get => _lastNotice;
            set
            {
                _lastNotice = value;
                Console.WriteLine("NETWORK NOTICE: " + _lastNotice);
            }
        }

        public static void Initialize(int maxConnections, float connectionTimeout, float heartbeatTimeout, float heartbeatRate, float hostUpdateRate)
        {
            _maxConnections = maxConnections;
            _connectionTimeout = connectionTimeout;
            _heartbeatTimeout = heartbeatTimeout;
            _heartbeatRate = heartbeatRate;
            _hostUpdateRate = hostUpdateRate;

            _link = new List<ClientInfoPackage>(_maxConnections);
        }

        public static void CloseConnections()
        {
            Connection?.Shutdown("bye");
            _host = null;
            _client = null;
            _hosting = false;
            _myUniqueID = 0;
            _heartbeatTimer = _sendHeartbeatTimer = _updateTimer = 0;
            _lastPingTime = 0;
            _link = new List<ClientInfoPackage>(_maxConnections);
        }

        public static bool StartNewHost(string name, int port)
        {
            if (Active) CloseConnections();

            var config = new NetPeerConfiguration(name)
            {
                Port = port,
                MaximumConnections = _maxConnections
            };
            _hosting = true;

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

        public static bool ConnectToHost(string name, string hostIP, int port)
        {
            if (Active) CloseConnections();

            var config = new NetPeerConfiguration(name) {ConnectionTimeout = _connectionTimeout};
            //config.AutoFlushSendQueue = false;
            _hosting = false;

            try
            {
                _client = new NetClient(config);
                _client.Start();

                var hail = _client.CreateMessage("HAIL");
                _client.Connect(hostIP, port, hail);
            }
            catch (NetException e)
            {
                LastError = e.Message;
                CloseConnections();
                return false;
            }

            return true;
        }

        public static double GetLatestPing()
        {
            if (!Connected) return -1;
            if (_hosting)
            {
                double max = -1;
                foreach (var c in _link)
                {
                    if (c.LastPingTime > max) max = c.LastPingTime;
                }

                return max;
            }
            return _lastPingTime;
        }

        public static string GetErrorAndClear()
        {
            var error = _lastError;
            _lastError = "";
            return error;
        }

        private static short HandleBasicMessage(NetIncomingMessage message, short id, double time)
        {
            if (id == (short) BasicMessageTypes.UniqueID)
            {
                if (_hosting)
                {
                    //Client is asking for uniqueID
                    var msg = _host.CreateMessage();
                    msg.WriteTime(false);
                    msg.Write((short)BasicMessageTypes.UniqueID);
                    msg.Write(message.SenderConnection.RemoteUniqueIdentifier);
                    _host.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                    LastNotice = "Client with ID " + message.SenderConnection.RemoteUniqueIdentifier + " requested UniqueID";
                }
                else
                {
                    //Unique ID from host
                    _myUniqueID = message.ReadInt64();
                    LastNotice = "Received unique ID " + _myUniqueID + " from host";
                }

                return id;
            }

            if (id == (short) BasicMessageTypes.Heartbeat)
            {
                if (_hosting)
                {
                    //Heartbeat from a client
                    var client = _link.FirstOrDefault(c => c.UserID == message.SenderConnection.RemoteUniqueIdentifier);
                    if (client is null)
                    {
                        LastError = "Received heartbeat from unknown client with ID " + message.SenderConnection.RemoteUniqueIdentifier;
                    }
                    else
                    {
                        client.TimeSinceLastHeartbeat = 0;
                        client.LastPingTime = time;
                    }
                }
                else
                {
                    //Heartbeat from host
                    _heartbeatTimer = 0;
                    _lastPingTime = time;
                }

                return id;
            }

            return 0;
        }

        private static int HandleTimerChecks(double elapsedTimeSec)
        {
            if (!Active) return -1;
            if (!Connected) return 0;

            //Check for heartbeat timeout for connections
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
                    return -4;
                }
            }

            //Check if time to send heartbeat to connections
            _sendHeartbeatTimer += elapsedTimeSec;
            if (_sendHeartbeatTimer > _heartbeatRate)
            {
                //Send heartbeat
                var msg = Connection.CreateMessage();
                msg.WriteTime(false);
                msg.Write((short)BasicMessageTypes.Heartbeat);
                if (_hosting) _host.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                else _client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);

                if (!_hosting)
                {
                    //Still waiting for UniqueID. Request again
                    var msg2 = _client.CreateMessage();
                    msg2.WriteTime(false);
                    msg2.Write((short)BasicMessageTypes.UniqueID);
                    _client.SendMessage(msg2, NetDeliveryMethod.ReliableOrdered);
                }
            }

            //Check if time to send status updates to clients
            if (_hosting)
            {
                _updateTimer += elapsedTimeSec;
                if (_updateTimer > _hostUpdateRate)
                {
                    //Send update to clients
                    _updateTimer -= _hostUpdateRate;
                    //TODO:
                }
            }

            return 0;
        }

        public static int Update(double elapsedTimeSec, Func<NetIncomingMessage, bool> output)
        {
            var anyDataReceived = HandleTimerChecks(elapsedTimeSec);
            if (anyDataReceived != 0) return anyDataReceived;

            NetIncomingMessage message;
            while ((message = Connection.ReadMessage()) != null)
            {
                switch (message.MessageType)
                {
                    case NetIncomingMessageType.Error:
                        CloseConnections();
                        break;
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        LastNotice = message.ReadString();
                        break;
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                        LastError = message.ReadString();
                        break;
                    case NetIncomingMessageType.Data:
                        var time = message.ReadTime(false) - message.ReceiveTime;
                        var messageID = message.PeekInt16();
                        if (HandleBasicMessage(message, messageID, time) != 0) break;
                        if (output.Invoke(message)) anyDataReceived++;
                        else LastError = "Failed to read unknown message with ID " + messageID;
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        var connectionID = message.SenderConnection.RemoteUniqueIdentifier;
                        switch ((NetConnectionStatus)message.ReadByte())
                        {
                            case NetConnectionStatus.Connected:
                                if (_hosting)
                                {
                                    //Record new connection
                                    LastNotice = "New client with ID " + connectionID + " has connected";
                                    _link.Add(new ClientInfoPackage { UserID = connectionID });

                                    //Send connection their UniqueID
                                    var msg = _host.CreateMessage();
                                    msg.WriteTime(false);
                                    msg.Write((short)BasicMessageTypes.UniqueID);
                                    msg.Write(message.SenderConnection.RemoteUniqueIdentifier);
                                    _host.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                }
                                else
                                {
                                    //Record host link
                                    LastNotice = "Connected to host";
                                }

                                break;
                            case NetConnectionStatus.Disconnecting:
                            case NetConnectionStatus.Disconnected:
                                if (!_hosting) break;

                                //Remove disconnected client from links
                                LastNotice = "Client with ID " + connectionID + " has disconnected";
                                for (int i = _link.Count - 1; i >= 0; i--)
                                {
                                    if (_link[i].UserID == connectionID) _link.RemoveAt(i);
                                }

                                break;
                        }

                        break;
                }

                Connection.Recycle(message);
            }

            return anyDataReceived;
        }
        
        public static void SendMessage(Action<NetOutgoingMessage> filler)
        {
            //if (!Connected) return;
            var message = Connection.CreateMessage();
            message.WriteTime(false);
            filler.Invoke(message);
            if (_hosting) _host.SendToAll(message, NetDeliveryMethod.ReliableOrdered);
            else _client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            //Connection.FlushSendQueue();
        }

        public static void SendMessage(short id, string data)
        {
            //if (!Connected) return;
            var message = Connection.CreateMessage();
            message.WriteTime(false);
            message.Write(id);
            message.Write(data);
            if (_hosting) _host.SendToAll(message, NetDeliveryMethod.ReliableOrdered);
            else _client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
            //Connection.FlushSendQueue();
        }

    }
}
