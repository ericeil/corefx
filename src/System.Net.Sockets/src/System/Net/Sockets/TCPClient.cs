// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
    // The System.Net.Sockets.TcpClient class provide TCP services at a higher level
    // of abstraction than the System.Net.Sockets.Socket class. System.Net.Sockets.TcpClient
    // is used to create a Client connection to a remote host.
    public partial class TcpClient : IDisposable
    {
        private AddressFamily _family;
        private Socket _clientSocket;
        private NetworkStream _dataStream;
        private bool _cleanedUp = false;
        private bool _active;

        // Initializes a new instance of the System.Net.Sockets.TcpClient class with the specified end point.
        public TcpClient(IPEndPoint localEP)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, nameof(TcpClient), localEP);
            }

            if (localEP == null)
            {
                throw new ArgumentNullException(nameof(localEP));
            }

            // IPv6: Establish address family before creating a socket
            _family = localEP.AddressFamily;

            InitializeClientSocket();
            Client.Bind(localEP);

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, nameof(TcpClient), "");
            }
        }
        
        // Initializes a new instance of the System.Net.Sockets.TcpClient class.
        public TcpClient() : this(AddressFamily.InterNetwork)
        {
        }

        // Initializes a new instance of the System.Net.Sockets.TcpClient class.
        public TcpClient(AddressFamily family)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "TcpClient", family);
            }

            // Validate parameter
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
            {
                throw new ArgumentException(SR.Format(SR.net_protocol_invalid_family, "TCP"), nameof(family));
            }

            _family = family;
            InitializeClientSocket();

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "TcpClient", null);
            }
        }

        // Initializes a new instance of the System.Net.Sockets.TcpClient class and connects to the specified port on 
        // the specified host.
        public TcpClient(string hostname, int port)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, nameof(TcpClient), hostname);
            }

            if (hostname == null)
            {
                throw new ArgumentNullException(nameof(hostname));
            }

            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            //
            // IPv6: Delay creating the client socket until we have
            //       performed DNS resolution and know which address
            //       families we can use.
            //
            //InitializeClientSocket();

            try
            {
                Connect(hostname, port);
            }

            catch
            {
                if (_clientSocket != null)
                {
                    _clientSocket.Close();
                }
                throw;
            }

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, nameof(TcpClient), null);
            }
        }

        // Used by TcpListener.Accept().
        internal TcpClient(Socket acceptedSocket)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "TcpClient", acceptedSocket);
            }

            _clientSocket = acceptedSocket;
            _active = true;

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "TcpClient", null);
            }
        }

        // Used by the class to indicate that a connection has been made.
        protected bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public int Available { get { return AvailableCore; } }

        // Used by the class to provide the underlying network socket.
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // TODO: Remove once https://github.com/dotnet/corefx/issues/5868 is addressed.
        public Socket Client
        {
            get { return ClientCore; }
            set { ClientCore = value; }
        }

        public bool Connected { get { return ConnectedCore; } }

        public bool ExclusiveAddressUse
        {
            get { return ExclusiveAddressUseCore; }
            set { ExclusiveAddressUseCore = value; }
        }

        // Connects the Client to the specified port on the specified host.
        public void Connect(string hostname, int port)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, nameof(Connect), hostname);
            }
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (hostname == null)
            {
                throw new ArgumentNullException(nameof(hostname));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            // Check for already connected and throw here. This check
            // is not required in the other connect methods as they
            // will throw from WinSock. Here, the situation is more
            // complex since we have to resolve a hostname so it's
            // easier to simply block the request up front.
            if (_active)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            // IPv6: We need to process each of the addresses return from
            //       DNS when trying to connect. Use of AddressList[0] is
            //       bad form.

            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            Exception lastex = null;
            Socket ipv6Socket = null;
            Socket ipv4Socket = null;

            try
            {
                if (_clientSocket == null)
                {
                    if (Socket.OSSupportsIPv4)
                    {
                        ipv4Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    }
                }

                foreach (IPAddress address in addresses)
                {
                    try
                    {
                        if (_clientSocket == null)
                        {
                            // We came via the <hostname,port> constructor. Set the
                            // address family appropriately, create the socket and
                            // try to connect.
                            if (address.AddressFamily == AddressFamily.InterNetwork && ipv4Socket != null)
                            {
                                ipv4Socket.Connect(address, port);
                                _clientSocket = ipv4Socket;
                                if (ipv6Socket != null)
                                {
                                    ipv6Socket.Close();
                                }
                            }
                            else if (ipv6Socket != null)
                            {
                                ipv6Socket.Connect(address, port);
                                _clientSocket = ipv6Socket;
                                if (ipv4Socket != null)
                                {
                                    ipv4Socket.Close();
                                }
                            }

                            _family = address.AddressFamily;
                            _active = true;
                            break;
                        }
                        else if (address.AddressFamily == _family)
                        {
                            // Only use addresses with a matching family
                            Connect(new IPEndPoint(address, port));
                            _active = true;
                            break;
                        }
                    }

                    catch (Exception ex)
                    {
                        if (ex is OutOfMemoryException)
                        {
                            throw;
                        }
                        lastex = ex;
                    }
                }
            }

            catch (Exception ex)
            {
                if (ex is OutOfMemoryException)
                {
                    throw;
                }
                lastex = ex;
            }

            finally
            {

                //cleanup temp sockets if failed
                //main socket gets closed when tcpclient gets closed

                //did we connect?
                if (!_active)
                {
                    if (ipv6Socket != null)
                    {
                        ipv6Socket.Close();
                    }

                    if (ipv4Socket != null)
                    {
                        ipv4Socket.Close();
                    }


                    // The connect failed - rethrow the last error we had
                    if (lastex != null)
                    {
                        throw lastex;
                    }
                    else
                    {
                        throw new SocketException((int)SocketError.NotConnected);
                    }
                }
            }

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, nameof(Connect), null);
            }
        }

        // Connects the Client to the specified port on the specified host.
        public void Connect(IPAddress address, int port)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, nameof(Connect), address);
            }
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            IPEndPoint remoteEP = new IPEndPoint(address, port);
            Connect(remoteEP);

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, nameof(Connect), null);
            }
        }

        // Connect the Client to the specified end point.
        public void Connect(IPEndPoint remoteEP)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, nameof(Connect), remoteEP);
            }
            if (_cleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException(nameof(remoteEP));
            }

            Client.Connect(remoteEP);
            _active = true;

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, nameof(Connect), null);
            }
        }

        //methods
        public void Connect(IPAddress[] ipAddresses, int port)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, nameof(Connect), ipAddresses);
            }

            Client.Connect(ipAddresses, port);
            _active = true;

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, nameof(Connect), null);
            }
        }

        public Task ConnectAsync(IPAddress address, int port)
        {
            return Task.Factory.FromAsync(
                (targetAddess, targetPort, callback, state) => ((TcpClient)state).BeginConnect(targetAddess, targetPort, callback, state),
                asyncResult => ((TcpClient)asyncResult.AsyncState).EndConnect(asyncResult),
                address,
                port,
                state: this);
        }

        public Task ConnectAsync(string host, int port)
        {
            return ConnectAsyncCore(host, port);
        }

        public Task ConnectAsync(IPAddress[] addresses, int port)
        {
            return ConnectAsyncCore(addresses, port);
        }

        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "BeginConnect", address);
            }

            IAsyncResult result = Client.BeginConnect(address, port, requestCallback, state);
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "BeginConnect", null);
            }

            return result;
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "BeginConnect", host);
            }

            IAsyncResult result = Client.BeginConnect(host, port, requestCallback, state);
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "BeginConnect", null);
            }

            return result;
        }

        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "BeginConnect", addresses);
            }

            IAsyncResult result = Client.BeginConnect(addresses, port, requestCallback, state);
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "BeginConnect", null);
            }

            return result;
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "EndConnect", asyncResult);
            }

            Socket s = Client;
            if (s == null)
            {
                // Dispose nulls out the client socket field.
                throw new ObjectDisposedException(GetType().Name);
            }
            s.EndConnect(asyncResult);

            _active = true;
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "EndConnect", null);
            }
        }

        // Returns the stream used to read and write data to the remote host.
        public NetworkStream GetStream()
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "GetStream", "");
            }

            if (_cleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!Connected)
            {
                throw new InvalidOperationException(SR.net_notconnected);
            }

            if (_dataStream == null)
            {
                _dataStream = new NetworkStream(Client, true);
            }

            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "GetStream", _dataStream);
            }

            return _dataStream;
        }

        // Disposes the Tcp connection.
        protected virtual void Dispose(bool disposing)
        {
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Enter(NetEventSource.ComponentType.Socket, this, "Dispose", "");
            }

            if (_cleanedUp)
            {
                if (NetEventSource.Log.IsEnabled())
                {
                    NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "Dispose", "");
                }

                return;
            }

            if (disposing)
            {
                IDisposable dataStream = _dataStream;
                if (dataStream != null)
                {
                    dataStream.Dispose();
                }
                else
                {
                    // If the NetworkStream wasn't created, the Socket might
                    // still be there and needs to be closed. In the case in which
                    // we are bound to a local IPEndPoint this will remove the
                    // binding and free up the IPEndPoint for later uses.
                    Socket chkClientSocket = _clientSocket;
                    if (chkClientSocket != null)
                    {
                        try
                        {
                            chkClientSocket.InternalShutdown(SocketShutdown.Both);
                        }
                        finally
                        {
                            chkClientSocket.Dispose();
                            _clientSocket = null;
                        }
                    }
                }

                DisposeCore(); // platform-specific disposal work

                GC.SuppressFinalize(this);
            }

            _cleanedUp = true;
            if (NetEventSource.Log.IsEnabled())
            {
                NetEventSource.Exit(NetEventSource.ComponentType.Socket, this, "Dispose", "");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~TcpClient()
        {
#if DEBUG
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            using (GlobalLog.SetThreadKind(ThreadKinds.System | ThreadKinds.Async))
            {
#endif
                Dispose(false);
#if DEBUG
            }
#endif
        }

        // Gets or sets the size of the receive buffer in bytes.
        public int ReceiveBufferSize
        {
            get { return ReceiveBufferSizeCore; }
            set { ReceiveBufferSizeCore = value; }
        }

        // Gets or sets the size of the send buffer in bytes.
        public int SendBufferSize
        {
            get { return SendBufferSizeCore; }
            set { SendBufferSizeCore = value; }
        }

        // Gets or sets the receive time out value of the connection in milliseconds.
        public int ReceiveTimeout
        {
            get { return ReceiveTimeoutCore; }
            set { ReceiveTimeoutCore = value; }
        }

        // Gets or sets the send time out value of the connection in milliseconds.
        public int SendTimeout
        {
            get { return SendTimeoutCore; }
            set { SendTimeoutCore = value; }
        }

        // Gets or sets the value of the connection's linger option.
        public LingerOption LingerState
        {
            get { return LingerStateCore; }
            set { LingerStateCore = value; }
        }

        // Enables or disables delay when send or receive buffers are full.
        public bool NoDelay
        {
            get { return NoDelayCore; }
            set { NoDelayCore = value; }
        }

        private Socket CreateSocket()
        {
            return new Socket(_family, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
