using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace CommunicationLibrary
{
    public class ClientServerCommunicationManager
    {
        public Socket CreateSocket(ProtocolType protocolType)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, protocolType);
            socket.SendBufferSize = 64 * 1024;
            socket.ReceiveBufferSize = 64 * 1024;
            return socket;
        }

        public void Send(byte[] toSend, Socket socket)
        {
            socket.Send(toSend);
        }

        public int Receive(byte[] bytes, Socket socket)
        {
            return socket.Receive(bytes);
        }

        public void Shutdown(Socket socket, SocketShutdown shutdownMode)
        {
            socket.Shutdown(shutdownMode);
        }

        public void Close(Socket socket)
        {
            socket.Close();
        }
    }
}
