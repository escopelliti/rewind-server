using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using PDSProject;

namespace CommunicationLibrary
{
    class ConnectionHandler
    {
        
        private ServerCommunicationManager server;
        private Channel serverChannel;
        private ServerDispatcher dispatcher;
         
        //ci dovra essere una struttura che gestisce tutte le connessioni/socket in entrata
        public ConnectionHandler(MainForm mainForm)
        {
            serverChannel = new Channel();
            this.server = new ServerCommunicationManager();
            dispatcher = new ServerDispatcher(server, mainForm);
            serverChannel.SetDataSocket(server.CreateSocket(ProtocolType.Tcp));
            serverChannel.SetCmdSocket(server.CreateSocket(ProtocolType.Tcp));
        }

        public void Listen()
        {
            
            // TODO to be changed - we need a configuration file or something like that
            serverChannel.SetCmdSocket(server.Listen(Dns.GetHostName(), 12000, serverChannel.GetCmdSocket()));
            serverChannel.SetDataSocket(server.Listen(Dns.GetHostName(), 12001, serverChannel.GetDataSocket()));

            //faccio partire due thread che ascoltano sulle due socket e passano il Client al dispatcher
            Thread thread1 = new Thread(() => WaitForClientOnDataSocket(serverChannel.GetDataSocket()));
            thread1.Start();
            Thread thread2 = new Thread(() => WaitForClient(serverChannel.GetCmdSocket()));
            thread2.Start();
           
        }

        private void WaitForClientOnDataSocket(Socket serverSocket)
        {
            while (true)
            {
                Client newClient = new Client();
                Socket clientSocket = Accept(serverSocket);
                if (!(clientSocket == null))
                {
                    newClient.SetSocket(clientSocket);
                    dispatcher.StartListeningToData(newClient);
                }
            }

        }

        private void WaitForClient(Socket serverSocket)
        {
            while (true)
            {
                Client newClient = new Client();
                Socket clientSocket = Accept(serverSocket);
                if (!(clientSocket == null))
                {
                    newClient.SetSocket(clientSocket);
                    dispatcher.StartListeningTo(newClient);
                }
            }
        }

        private Socket Accept(Socket serverSock)
        {
            //mi memorizzo la socket del client in quache struttura e passo il controllo al dispatcher che ascolterà le richieste
            try
            {
                return server.Accept(serverSock);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("Non e' possibile stabilire una connessione.");
                //System.Windows.Forms.Application.Exit();
                return null;
            }
        }

        public Channel GetChannel()
        {
            return serverChannel;
        }

    }
}
