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
        protected static Socket socket;
        protected AsyncCallback connectHandler;
        protected AsyncCallback sendHandler;
        public static AsyncCallback receiveHandler;
        protected static byte[] bytesRead;
        public static ManualResetEvent dataReady = new ManualResetEvent(false);

        public void Send(byte[] toSend)
        {
            socket.BeginSend(toSend, 0, toSend.Length, 0, sendHandler, socket);
        }

        public void SendObject(object toSend)
        {
            //Serialization and then send
            //clientSocket.BeginSend(toSend, 0, toSend.Length, 0, sendHandler, clientSocket);
        }

        protected static void SendCallback(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;
            client.EndSend(iar);
        }

        public void SendFiles(List<string> filepathList)
        {
            foreach (string fileName in filepathList)
            {
                socket.SendFile(fileName);
            }
        }

        public void Receive()
        {
            SocketObject socketObj = new SocketObject();
            socketObj.socket = socket;
            socketObj.receiveBuffer = new byte[SocketObject.bufferSize];
            socket.BeginReceive(socketObj.receiveBuffer, 0, SocketObject.bufferSize, 0, receiveHandler, socketObj);
        }

        private void ReceiveObject()
        {

            //deserializzazione
        }
        public struct SocketObject
        {
            public Socket socket;
            public const int bufferSize = 1024;
            public byte[] receiveBuffer;
        }
    }
}
