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

using ServerTest;

namespace PDSProject
{
    class ServerDispatcher
    {

        private static ServerCommunicationManager server;
        public delegate void ChangeClipboardEventHandler(Object sender, Object param);
        public event ChangeClipboardEventHandler clipboardHandler;
        private static Dictionary<string, Delegate> dispatch;

        public ServerDispatcher (ServerCommunicationManager runningServer) {
            server = runningServer;
            SetupDispatcher();
        }

        private void SetupDispatcher()
        {
            dispatch = new Dictionary<string, Delegate>();
            dispatch[ProtocolUtils.SET_CLIPBOARD_TEXT] = new Action<Object>(obj => clipboardHandler(this, obj));
            this.clipboardHandler += NewClipboardDataToPaste;
        }

        private void NewClipboardDataToPaste(Object source, Object param)
        {
            MainForm.mainForm.Invoke(MainForm.clipboardTextDelegate,((XDocument) param).Descendants(ProtocolUtils.CONTENT).ElementAt(0).Value);
        }

        public void StartListeningTo(Client client)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ListenToRequest), client);
        }

        public void StartListeningToData(Client client)
        {
            while (true)
            {
                byte[] data = new byte[1024];
                int bytesReadNum = server.Receive(data, client.GetSocket());
                if (bytesReadNum > 0)
                {
                    byte[] actualData = new byte[bytesReadNum];
                    System.Buffer.BlockCopy(data, 0, actualData, 0, bytesReadNum);
                    //ricostruisci l'INPUT dal buffer di byte e utilizza la SendInput;
                }
            }
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
            string requestType = doc.Descendants(ProtocolUtils.TYPE).ElementAt(0).Value.ToString();
            dispatch[requestType].DynamicInvoke(doc);

            //interpreta l'XML (request) e a seconda del tipo richiama il corretto metodo (attenzione all'invio file)
        }
    }
}
