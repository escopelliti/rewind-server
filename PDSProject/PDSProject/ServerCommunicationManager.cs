using System;
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
        private IPEndPoint localEP;
        private AsyncCallback acceptingSocketHandler;
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        public ServerCommunicationManager(string host, int port, ProtocolType protocolType)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            localEP = new IPEndPoint(ipAddress, port);
            socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, protocolType);
            acceptingSocketHandler += AcceptSocketCallback;
        }

        private static void AcceptSocketCallback(IAsyncResult iar)
        {

            allDone.Set();
            Socket serverSocket = (Socket)iar.AsyncState;
            socket = serverSocket.EndAccept(iar);
            SocketObject socketObj = new SocketObject();
            socketObj.receiveBuffer = new byte[SocketObject.bufferSize];
            socketObj.socket = socket;
            bytesRead = new byte[SocketObject.bufferSize];
            socket.BeginReceive(socketObj.receiveBuffer, 0, SocketObject.bufferSize, 0, receiveHandler, socketObj);
        }

        public void Listen()
        {
            try
            {
                socket.Bind(localEP);
                socket.Listen(100);
                while (true)
                {
                    allDone.Reset();
                    socket.BeginAccept(acceptingSocketHandler, socket);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
