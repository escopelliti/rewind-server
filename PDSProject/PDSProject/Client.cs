using System;
using System.Net.Sockets;

namespace ConnectionModule
{
    public class Client
    {
        public Socket CmdSocket{ get;set; }
        public Socket DataSocket { get; set; }

    }
}
