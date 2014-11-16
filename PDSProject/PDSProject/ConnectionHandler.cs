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
    public class ConnectionHandler
    {
        private bool closed = false; 
        private ServerCommunicationManager server;
        private Channel serverChannel;
        private ServerDispatcher dispatcher;
        public ushort DataPort { get; set; }
        public ushort CmdPort { get; set; }
         
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

            Socket cmdSocket = server.Listen(Dns.GetHostName(), CmdPort, serverChannel.GetCmdSocket());
            Socket dataSocket = server.Listen(Dns.GetHostName(), DataPort, serverChannel.GetDataSocket());
            if (cmdSocket == null || dataSocket == null)
            {
                throw new Exception("Problem listening.");
                return;
            }
            serverChannel.SetCmdSocket(cmdSocket);
            serverChannel.SetDataSocket(dataSocket);
            closed = false;
            //faccio partire due thread che ascoltano sulle due socket e passano il Client al dispatcher
            Thread thread1 = new Thread(() => WaitForClientOnDataSocket(serverChannel.GetDataSocket()));
            thread1.Start();
            Thread thread2 = new Thread(() => WaitForClient(serverChannel.GetCmdSocket()));
            thread2.Start();           
        }

        private void WaitForClientOnDataSocket(Socket serverSocket)
        {
            while (!closed)
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
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void StopListening()
        {
            if (serverChannel != null)
            {
                //server.Shutdown(serverChannel.GetCmdSocket(), SocketShutdown.Both);
                server.Close(serverChannel.GetCmdSocket());
                //server.Shutdown(serverChannel.GetDataSocket(), SocketShutdown.Both);
                server.Close(serverChannel.GetDataSocket());
                closed = true;
            }
        }
    }
}
