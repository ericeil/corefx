// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Xunit.Performance;
using Xunit.Abstractions;
using System.Net.Test.Common;

namespace System.Net.Sockets.Tests
{
    public class Perf
    {
        private readonly ITestOutputHelper _log;

        public Perf(ITestOutputHelper output)
        {
            _log = TestLogging.GetInstance();
        }

        private static IDisposable ReserveUDPLoopbackPort(out EndPoint endPoint)
        {
            Socket s = new Socket(SocketType.Dgram, ProtocolType.Udp);
            int port = s.BindToAnonymousPort(IPAddress.Loopback);
            endPoint = new IPEndPoint(IPAddress.Loopback, port);
            return s;
        }

        [Benchmark]
        public void UDPSendSinglePacket()
        {
            EndPoint endPoint;
            using (ReserveUDPLoopbackPort(out endPoint))
            {
                byte[] buf = new byte[1024];
                using (Socket s = new Socket(SocketType.Dgram, ProtocolType.Udp))
                {
                    foreach (var iteration in Benchmark.Iterations)
                    {
                        using (iteration.StartMeasurement())
                        {
                            s.SendTo(buf, endPoint);
                        }
                    }
                }
            }
        }
    }
}
