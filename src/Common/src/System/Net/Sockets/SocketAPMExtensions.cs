// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
    internal static class TcpClientAPMExtensions
    {
        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The remote host
        //     is specified by an System.Net.IPAddress array and a port number (System.Int32).
        //
        // Parameters:
        //   addresses:
        //     At least one System.Net.IPAddress that designates the remote hosts.
        //
        //   port:
        //     The port number of the remote hosts.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The addresses parameter is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        public static IAsyncResult BeginConnect(this TcpClient client, IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ConnectAsync(addresses, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The remote host
        //     is specified by an System.Net.IPAddress and a port number (System.Int32).
        //
        // Parameters:
        //   address:
        //     The System.Net.IPAddress of the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The address parameter is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        public static IAsyncResult BeginConnect(this TcpClient client, IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ConnectAsync(address, port), requestCallback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous request for a remote host connection. The remote host
        //     is specified by a host name (System.String) and a port number (System.Int32).
        //
        // Parameters:
        //   host:
        //     The name of the remote host.
        //
        //   port:
        //     The port number of the remote host.
        //
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the connect operation.
        //     This object is passed to the requestCallback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult object that references the asynchronous connection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The host parameter is null.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        //
        //   T:System.Security.SecurityException:
        //     A caller higher in the call stack does not have permission for the requested
        //     operation.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The port number is not valid.
        public static IAsyncResult BeginConnect(this TcpClient client, string host, int port, AsyncCallback requestCallback, object state)
        {
            return TaskToApm.Begin(client.ConnectAsync(host, port), requestCallback, state);
        }

        //
        // Summary:
        //     Ends a pending asynchronous connection attempt.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult object returned by a call to Overload:System.Net.Sockets.TcpClient.BeginConnect.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The asyncResult parameter is null.
        //
        //   T:System.ArgumentException:
        //     The asyncResult parameter was not returned by a call to a Overload:System.Net.Sockets.TcpClient.BeginConnect
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.TcpClient.EndConnect(System.IAsyncResult) method was previously
        //     called for the asynchronous connection.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred when attempting to access the System.Net.Sockets.Socket. See
        //     the Remarks section for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The underlying System.Net.Sockets.Socket has been closed.
        public static void EndConnect(this TcpClient client, IAsyncResult asyncResult)
        {
            TaskToApm.End(asyncResult);
        }
    }

    internal static class TcpListenerAPMExtensions
    {
        //
        // Summary:
        //     Begins an asynchronous operation to accept an incoming connection attempt.
        //
        // Parameters:
        //   callback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object containing information about the accept operation. This
        //     object is passed to the callback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous creation of the System.Net.Sockets.Socket.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred while attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginAcceptSocket(this TcpListener listener, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(listener.AcceptSocketAsync(), callback, state);
        }

        //
        // Summary:
        //     Begins an asynchronous operation to accept an incoming connection attempt.
        //
        // Parameters:
        //   callback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object containing information about the accept operation. This
        //     object is passed to the callback delegate when the operation is complete.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous creation of the System.Net.Sockets.TcpClient.
        //
        // Exceptions:
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred while attempting to access the socket. See the Remarks section
        //     for more information.
        //
        //   T:System.ObjectDisposedException:
        //     The System.Net.Sockets.Socket has been closed.
        public static IAsyncResult BeginAcceptTcpClient(this TcpListener listener, AsyncCallback callback, object state)
        {
            return TaskToApm.Begin(listener.AcceptTcpClientAsync(), callback, state);
        }

        //
        // Summary:
        //     Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.Socket
        //     to handle remote host communication.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult returned by a call to the System.Net.Sockets.TcpListener.BeginAcceptSocket(System.AsyncCallback,System.Object)
        //     method.
        //
        // Returns:
        //     A System.Net.Sockets.Socket.The System.Net.Sockets.Socket used to send and receive
        //     data.
        //
        // Exceptions:
        //   T:System.ObjectDisposedException:
        //     The underlying System.Net.Sockets.Socket has been closed.
        //
        //   T:System.ArgumentNullException:
        //     The asyncResult parameter is null.
        //
        //   T:System.ArgumentException:
        //     The asyncResult parameter was not created by a call to the System.Net.Sockets.TcpListener.BeginAcceptSocket(System.AsyncCallback,System.Object)
        //     method.
        //
        //   T:System.InvalidOperationException:
        //     The System.Net.Sockets.TcpListener.EndAcceptSocket(System.IAsyncResult) method
        //     was previously called.
        //
        //   T:System.Net.Sockets.SocketException:
        //     An error occurred while attempting to access the System.Net.Sockets.Socket. See
        //     the Remarks section for more information.
        public static Socket EndAcceptSocket(this TcpListener listener, IAsyncResult asyncResult)
        {
            return TaskToApm.End<Socket>(asyncResult);
        }

        //
        // Summary:
        //     Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.TcpClient
        //     to handle remote host communication.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult returned by a call to the System.Net.Sockets.TcpListener.BeginAcceptTcpClient(System.AsyncCallback,System.Object)
        //     method.
        //
        // Returns:
        //     A System.Net.Sockets.TcpClient.The System.Net.Sockets.TcpClient used to send
        //     and receive data.
        public static TcpClient EndAcceptTcpClient(this TcpListener listener, IAsyncResult asyncResult)
        {
            return TaskToApm.End<TcpClient>(asyncResult);
        }
    }
}
