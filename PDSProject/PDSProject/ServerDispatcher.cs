using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using CommunicationLibrary;

using System.Xml.Linq;
using System.IO;

namespace PDSProject
{
    class ServerDispatcher
    {

        private static ServerCommunicationManager server;

        public ServerDispatcher (ServerCommunicationManager runningServer) {
            server = runningServer;
        }

        public void StartListeningTo(Client client)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ListenToRequest), client);
        }

        private static void ListenToRequest(Object newClient)
        {
            Client client = (Client) newClient;
            while (true)
            {
                //qui leggi le richieste e distribuisci ad un thread la richiesta
                byte[] data = new byte[4096];
                int bytesReadNum = server.Receive(data, client.GetSocket());
                if (bytesReadNum > 0)
                {
                    byte[] actualData = new byte[bytesReadNum];
                    System.Buffer.BlockCopy(data, 0, actualData, 0, bytesReadNum);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(DispatchRequest), actualData);
                }
            }
        }

        private static void DispatchRequest(object request)
        {
            XDocument doc = new XDocument();
            MemoryStream ms = new MemoryStream((byte[]) request);
            doc = XDocument.Load(ms);

            Invoke(new MainForm().clipboardTextDelegate();
            System.Windows.Forms.Clipboard.SetText(doc.Descendants("content").ElementAt(0).Value);
            //interpreta l'XML (request) e a seconda del tipo richiama il corretto metodo (attenzione all'invio file)
        }
    }
}
