using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
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
            public float LastPingTime;

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
        private const int _verNum = 2020030310;

        private static int _maxConnections;
        private static double _connectionTimeout; //How long to try to connect to the host initially
        private static double _heartbeatTimeout;  //How long between heartbeats to consider the connection dead
        private static double _heartbeatRate;     //How long between sending heartbeats
        private static double _hostUpdateRate;    //How long between state updates from host to clients

        public static bool Active => _host != null || _client != null;
        public static bool Connected => _client?.ConnectionStatus == NetConnectionStatus.Connected || _host?.ConnectionsCount > 0;
        private static NetPeer Connection => Hosting ? (NetPeer) _host : (NetPeer) _client;

        public static bool Hosting { get; private set; }
        public static long MyUniqueID { get; private set; }

        private static bool _connecting;
        private static double _heartbeatTimer, _sendHeartbeatTimer, _updateTimer, _connectionTimer;
        private static float _lastPingTime;
        private static NetServer _host;
        private static NetClient _client;
        private static List<ClientInfoPackage> _link;

        private static Action _onUpdate;
        private static Action<long> _onDisconnect;

        private static string _lastError, _lastNotice;
        public static string LastError
        {
            get => _lastError;
            private set
            {
                _lastError = value;
                Debug.WriteLine("NETWORK ERROR: " + _lastError);
            }
        }
        public static string LastNotice
        {
            get => _lastNotice;
            private set
            {
                _lastNotice = value;
                Debug.WriteLine("NETWORK NOTICE: " + _lastNotice);
            }
        }

        public static void Initialize(int maxConnections, double connectionTimeout, double heartbeatTimeout, double heartbeatRate, double hostUpdateRate, Action onUpdate, Action<long> onDisconnect=null)
        {
            _maxConnections = maxConnections;
            _connectionTimeout = connectionTimeout;
            _heartbeatTimeout = heartbeatTimeout;
            _heartbeatRate = heartbeatRate;
            _hostUpdateRate = hostUpdateRate;

            _link = new List<ClientInfoPackage>(_maxConnections);
            _onDisconnect = onDisconnect;
            _onUpdate = onUpdate;
        }

        public static void CloseConnections()
        {
            Connection?.Shutdown("bye");
            _onDisconnect?.Invoke(-1);
            _host = null;
            _client = null;
            Hosting = _connecting = false;
            MyUniqueID = 0;
            _heartbeatTimer = _sendHeartbeatTimer = _updateTimer = _connectionTimer = 0;
            _lastPingTime = 0;
            _link = new List<ClientInfoPackage>(_maxConnections);
        }

        public static bool StartNewHost(string name, int port)
        {
            if (Active) CloseConnections();

            var config = new NetPeerConfiguration(name)
            {
                Port = port,
                MaximumConnections = _maxConnections,
                //ConnectionTimeout = (float) _heartbeatTimeout
            };
            Hosting = true;

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
            catch (SocketException e)
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

            var config = new NetPeerConfiguration(name);// {ConnectionTimeout = (float)_heartbeatTimeout};
            Hosting = false;

            try
            {
                _connecting = true;
                _client = new NetClient(config);
                _client.Start();
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

        public static double GetLatestPing()
        {
            if (!Connected) return -1;
            if (Hosting)
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

        private static short HandleBasicMessage(NetIncomingMessage message)
        {
            var id = message.PeekInt16();
            if (id == (short) BasicMessageTypes.UniqueID)
            {
                message.ReadInt16();
                if (Hosting)
                {
                    //Client is asking for uniqueID
                    var version = message.ReadInt32();
                    if (version != _verNum)
                    {
                        message.SenderConnection.Disconnect("bye");
                        LastError = "Client with ID " + message.SenderConnection.RemoteUniqueIdentifier + " attempted to connect using a different application version";
                        return -2;
                    }

                    var msg = _host.CreateMessage();
                    msg.Write((short) BasicMessageTypes.UniqueID);
                    msg.Write(_verNum);
                    msg.Write(message.SenderConnection.RemoteUniqueIdentifier);
                    _host.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                    LastNotice = "Client with ID " + message.SenderConnection.RemoteUniqueIdentifier + " requested UniqueID";
                }
                else
                {
                    //Unique ID from host
                    if (message.ReadInt32() != _verNum)
                    {
                        CloseConnections();
                        LastError = "Host is running a different application version";
                        return -2;
                    }
                    MyUniqueID = message.ReadInt64();
                    LastNotice = "Received unique ID " + MyUniqueID + " from host";
                }

                return id;
            }

            if (id == (short) BasicMessageTypes.Heartbeat)
            {
                message.ReadInt16();
                if (Hosting)
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
                        client.LastPingTime = message.SenderConnection.AverageRoundtripTime;
                    }
                }
                else
                {
                    //Heartbeat from host
                    _heartbeatTimer = 0;
                    _lastPingTime = message.SenderConnection.AverageRoundtripTime;
                }

                return id;
            }

            return 0;
        }

        public static void DisconnectClient(long id)
        {
            if (!Hosting) return;
            var connection = _host.Connections.FirstOrDefault(c => c.RemoteUniqueIdentifier == id);
            connection?.Disconnect("bye");
            _onDisconnect?.Invoke(id);
            var link = _link.FirstOrDefault(c => c.UserID == id);
            if (link != null) _link.Remove(link);
        }

        private static int HandleTimerChecks(double elapsedTimeSec)
        {
            if (!Active) return -1;

            if (_connecting)
            {
                _connectionTimer += elapsedTimeSec;
                if (_connectionTimer > _connectionTimeout)
                {
                    //Failed to connect to host in time
                    LastError = "Timed out attempting to connect to host";
                    CloseConnections();
                    return -1;
                }
            }

            if (!Connected) return 0;

            //Check for heartbeat timeout for connections
            if (Hosting)
            {
                for (int i = _link.Count-1; i>=0; i--)
                {
                    if (_link[i].TimeSinceLastHeartbeat + elapsedTimeSec > _heartbeatTimeout)
                    {
                        //Lost Connection to Client
                        LastError = "Lost connection to client with ID " + _link[i].UserID;
                        DisconnectClient(_link[i].UserID);
                        if (_link.Count == 0)
                        {
                            LastNotice = "All clients have disconnected. Shutting down host";
                            CloseConnections();
                            return -1;
                        }
                    }
                }
            }
            else
            {
                _heartbeatTimer += elapsedTimeSec;
                if (_heartbeatTimer > _heartbeatTimeout)
                {
                    //Lost Connection to Host
                    LastError = "Lost connection to host";
                    CloseConnections();                  
                    return -1;
                }
            }

            //Check if time to send heartbeat to connections
            _sendHeartbeatTimer += elapsedTimeSec;
            if (_sendHeartbeatTimer > _heartbeatRate)
            {
                _sendHeartbeatTimer = 0;

                //Send heartbeat
                var msg = Connection.CreateMessage();
                msg.Write((short)BasicMessageTypes.Heartbeat);
                if (Hosting) _host.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
                else _client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);

                if (!Hosting && MyUniqueID == 0)
                {
                    //Still waiting for UniqueID. Request again
                    var msg2 = _client.CreateMessage();
                    msg2.Write((short)BasicMessageTypes.UniqueID);
                    msg2.Write(_verNum);
                    msg2.Write((long) 0);
                    _client.SendMessage(msg2, NetDeliveryMethod.ReliableOrdered);
                }
            }

            //Check if time to send status updates to connections
            _updateTimer += elapsedTimeSec;
            if (_updateTimer > _hostUpdateRate)
            {
                //Send update to clients
                _updateTimer = 0;
                _onUpdate.Invoke();
            }

            return 0;
        }

        public static int Update(double elapsedTimeSec, Func<NetIncomingMessage, bool> output)
        {
            if (Active && !Connected && !_connecting && !Hosting)
            {
                CloseConnections();
                LastError = "Lost connection to host";
            }

            var anyDataReceived = HandleTimerChecks(elapsedTimeSec);
            if (anyDataReceived != 0) return anyDataReceived;

            NetIncomingMessage message;
            while ((message = Connection?.ReadMessage()) != null)
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
                        var messageID = message.PeekInt16();
                        if (HandleBasicMessage(message) != 0) break;
                        if (output.Invoke(message)) anyDataReceived++;
                        else LastError = "Failed to read unknown message with ID " + messageID;
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        var connectionID = message.SenderConnection.RemoteUniqueIdentifier;
                        switch ((NetConnectionStatus)message.ReadByte())
                        {
                            case NetConnectionStatus.Connected:
                                if (Hosting)
                                {
                                    //Record new connection
                                    LastNotice = "New client with ID " + connectionID + " has connected";
                                    _link.Add(new ClientInfoPackage { UserID = connectionID });
                                }
                                else
                                {
                                    //Record host link
                                    LastNotice = "Connected to host";
                                    _connecting = false;

                                    //Request UniqueID
                                    var msg = _client.CreateMessage();
                                    msg.Write((short)BasicMessageTypes.UniqueID);
                                    msg.Write(_verNum);
                                    msg.Write((long)0);
                                    _client.SendMessage(msg, message.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                }

                                break;
                            case NetConnectionStatus.Disconnecting:
                            case NetConnectionStatus.Disconnected:
                                if (Hosting)
                                {
                                    //Remove disconnected client from links
                                    LastNotice = "Client with ID " + connectionID + " has disconnected";
                                    for (int i = _link.Count - 1; i >= 0; i--)
                                    {
                                        if (_link[i].UserID == connectionID)
                                        {
                                            _link.RemoveAt(i);
                                            _onDisconnect?.Invoke(connectionID);
                                        }
                                    }

                                    if (_link.Count == 0)
                                    {
                                        LastNotice = "All clients have disconnected. Shutting down host";
                                        CloseConnections();
                                    }
                                }
                                else
                                {
                                    LastNotice = "Disconnected from the host";
                                    CloseConnections();
                                }
                                break;
                        }

                        break;
                }

                Connection?.Recycle(message);
            }

            return anyDataReceived;
        }
        
        public static void SendMessage(object[] data, Action<object[], NetOutgoingMessage> filler)
        {
            if (!Connected) return;
            var message = Connection.CreateMessage();
            filler.Invoke(data, message);
            if (Hosting) _host.SendToAll(message, NetDeliveryMethod.ReliableOrdered);
            else _client.SendMessage(message, NetDeliveryMethod.ReliableOrdered);
        }

    }
}
