using System;
using System.Net;
using System.Net.Sockets;

namespace ConnectionModule.CommunicationLibrary
{
    class ServerCommunicationManager : ClientServerCommunicationManager
    {

        public Socket Listen(string host, int port, Socket socket)
        {            
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
                IPAddress ipAddress = FindIPv4Addr(ipHostInfo);
                IPEndPoint localEP = new IPEndPoint(ipAddress, port);
                socket.Bind(localEP);
                socket.Listen(100);
                return socket;
            }
            catch (SocketException ex)
            {
                return null;
            }
            catch (Exception e)
            {                
                return null;
            }
        }

        private IPAddress FindIPv4Addr(IPHostEntry ipHostInfo)
        {
            foreach (IPAddress addr in ipHostInfo.AddressList)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr;
                }
            }
            throw new NullReferenceException("no ipv4 addr found");
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
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
