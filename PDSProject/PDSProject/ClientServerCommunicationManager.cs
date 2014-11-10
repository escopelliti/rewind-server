using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace CommunicationLibrary
{
    class ClientServerCommunicationManager
    {
        public Socket CreateSocket(ProtocolType protocolType)
        {
            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, protocolType);
            socket.SendBufferSize = 64 * 1024;
            socket.ReceiveBufferSize = 64 * 1024;
            return socket;
        }

        public void Send(byte[] toSend, Socket socket)
        {
            socket.Send(toSend);
        }

        public void SendFiles(List<string> filepathList, Socket socket)
        {
            foreach (string fileName in filepathList)
            {
                socket.SendFile(fileName);
            }
        }

        public int Receive(byte[] bytes, Socket socket)
        {
            return socket.Receive(bytes);
        }
    }
}
