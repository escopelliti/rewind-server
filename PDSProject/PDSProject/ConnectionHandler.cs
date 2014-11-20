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
        public bool closed = false; 
        private ServerCommunicationManager server;
        public Channel ServerChannel { get; set; }
        private ServerDispatcher dispatcher;
        public ushort DataPort { get; set; }
        public ushort CmdPort { get; set; }
         
        //ci dovra essere una struttura che gestisce tutte le connessioni/socket in entrata
        public ConnectionHandler(MainForm mainForm, Configuration.Configuration conf)
        {
            ServerChannel = new Channel();
            this.server = new ServerCommunicationManager();
            dispatcher = new ServerDispatcher(server, mainForm, conf);
            ServerChannel.SetCmdSocket(server.CreateSocket(ProtocolType.Tcp));          
        }

        public void ListenCmd()
        {

            Socket cmdSocket = server.Listen(Dns.GetHostName(), CmdPort, ServerChannel.GetCmdSocket());
            //Socket dataSocket = server.Listen(Dns.GetHostName(), DataPort, ServerChannel.GetDataSocket());
            if (cmdSocket == null /*|| dataSocket == null*/)
            {
                throw new Exception("Problem listening.");
            }
            ServerChannel.SetCmdSocket(cmdSocket);
            //ServerChannel.SetDataSocket(dataSocket);
            closed = false;
            //faccio partire due thread che ascoltano sulle due socket e passano il Client al dispatcher            
            Thread thread2 = new Thread(() => WaitForClient(ServerChannel.GetCmdSocket()));
            thread2.Start();           
        }

        public void ListenData()
        {            
            ServerChannel.SetDataSocket(server.CreateSocket(ProtocolType.Tcp));
            Socket dataSocket = server.Listen(Dns.GetHostName(), DataPort, ServerChannel.GetDataSocket());
            if (dataSocket == null)
            {
                throw new Exception("Problem listening.");
                return;
            }
            ServerChannel.SetDataSocket(dataSocket);
            closed = false;
            Thread thread1 = new Thread(() => WaitForClientOnDataSocket(ServerChannel.GetDataSocket()));
            thread1.Start();
        }

        public void WaitForClientOnDataSocket(Socket serverDataSocket)
        {
            while (!closed)
            {                
                Client newClient = new Client();
                Socket clientSocket = Accept(serverDataSocket);
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

        public void StopListeningCmd()
        {
            if (ServerChannel != null)
            {                
                server.Close(ServerChannel.GetCmdSocket());                                
                closed = true;
            }
        }

        public void StopListeningData()
        {
            try
            {
                Socket socket = ServerChannel.GetDataSocket();                
                //socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (SocketException ex)
            {
                return;
            }
            
        }
    }
}
