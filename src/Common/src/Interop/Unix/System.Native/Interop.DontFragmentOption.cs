// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Sys
    {
        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_SetIPv4DontFragmentOption")]
        internal static extern unsafe Error SetIPv4DontFragmentOption(SafeHandle socket, int optionValue);

        [DllImport(Libraries.SystemNative, EntryPoint = "SystemNative_GetIPv4DontFragmentOption")]
        internal static extern unsafe Error GetIPv4DontFragmentOption(SafeHandle socket, out int optionValue);
    }
}
