﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CommunicationLibrary
{
    class ServerCommunicationManager : ClientServerCommunicationManager
    {

        public Socket Listen(string host, int port, Socket socket)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEP = new IPEndPoint(ipAddress, port);
            try
            {
                socket.Bind(localEP);
                socket.Listen(100);
                return socket;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public Socket Accept(Socket serverSocket)
        {
            try
            {
                return serverSocket.Accept();
            }
            catch (SocketException se)
            {
                return null;
            }
        }
    }
}
