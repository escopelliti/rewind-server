using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace CommunicationLibrary
{
    class ClientCommunicationManager : ClientServerCommunicationManager
    {

        private IPEndPoint remoteEP;
        private static ManualResetEvent connectionCompleted = new ManualResetEvent(false);

        public ClientCommunicationManager(string host, int port, ProtocolType protocolType)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(host);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            remoteEP = new IPEndPoint(ipAddress, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            sendHandler += SendCallback;
            connectHandler += ConnectCallback;
        }

        public void Connect()
        {
            socket.BeginConnect(remoteEP, connectHandler, socket);
            connectionCompleted.WaitOne();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectionCompleted.Set();
            }
            catch (System.ArgumentNullException ex)
            {
                throw;
            }
            catch (SocketException ex)
            {
                throw;
            }
            catch (Exception e)
            {
                throw;
            }
        }


    }
}
