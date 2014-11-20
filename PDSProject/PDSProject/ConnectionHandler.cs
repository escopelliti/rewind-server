using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using PDSProject;
using System.Windows;

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

            Socket cmdSocket = InitSocket();
            if (cmdSocket == null)
            {
                System.Windows.MessageBox.Show("Sembra esserci qualche problema, prova a riavviare l'applicazione", "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ServerChannel.SetCmdSocket(cmdSocket);          
            }            
        }

        public void ListenCmd()
        {
            Socket cmdSocket = server.Listen(Dns.GetHostName(), CmdPort, ServerChannel.GetCmdSocket());            
            if (cmdSocket == null)
            {
                System.Windows.MessageBox.Show("Sembra esserci qualche problema, prova a riavviare l'applicazione", "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ServerChannel.SetCmdSocket(cmdSocket);            
            closed = false;
            //faccio partire due thread che ascoltano sulle due socket e passano il Client al dispatcher            
            try
            {
                Thread cmdThread = new Thread(() => WaitForClient(ServerChannel.GetCmdSocket()));
                cmdThread.Start();
            }
            catch (System.Threading.ThreadStateException ex)
            {
                throw ex;
            }
            catch (System.OutOfMemoryException ex)
            {
                throw ex;
            }                  
        }

        private Socket InitSocket()
        {
            short retry = 3;
            Socket socket = null;
            while (retry > 0)
            {
                socket = server.CreateSocket(ProtocolType.Tcp);
                if (socket != null)
                    break;
                retry--;
            }
            return socket;
        }                           

        public void ListenData()
        {
            Socket dataSocket = InitSocket();
            if (dataSocket == null)
            {
                System.Windows.MessageBox.Show("Sembra esserci qualche problema, prova a riavviare l'applicazione", "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }            
            ServerChannel.SetDataSocket(dataSocket);            
            dataSocket = server.Listen(Dns.GetHostName(), DataPort, dataSocket);
            if (dataSocket == null)
            {
                System.Windows.MessageBox.Show("Sembra esserci qualche problema, prova a riavviare l'applicazione", "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new NullReferenceException("no server socket available");
            }
            ServerChannel.SetDataSocket(dataSocket);
            closed = false;
            try
            {
                Thread thread1 = new Thread(() => WaitForClientOnDataSocket(ServerChannel.GetDataSocket()));
                thread1.Start();
            }
            catch (System.Threading.ThreadStateException ex)
            {
                throw ex;
            }
            catch (System.OutOfMemoryException ex)
            {
                throw ex;
            }
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
            return server.Accept(serverSock);           
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
