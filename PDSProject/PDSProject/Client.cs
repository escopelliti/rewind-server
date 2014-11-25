using System;
using System.Net.Sockets;

namespace ConnectionModule
{
    public class Client
    {
        private Socket socket;        
        public Socket GetSocket()
        {
            return socket;
        }
        public void SetSocket(Socket socket)
        {
            this.socket = socket;
        }
    }
}
