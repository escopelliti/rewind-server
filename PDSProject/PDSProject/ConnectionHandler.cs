using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using MainApp;
using System.Windows;
using ConnectionModule.CommunicationLibrary;

namespace ConnectionModule
{
    public class ConnectionHandler
    {
        public bool closed = false; 
        private ServerCommunicationManager server;
        public Channel ServerChannel { get; set; }
        private ServerDispatcher dispatcher;
        public ushort DataPort { get; set; }
        public ushort CmdPort { get; set; }
        private MainForm mainForm;

        private const String HOUSTON_PROBLEM = "Sembra esserci qualche problema, prova a riavviare l'applicazione";
 
        public ConnectionHandler(MainForm mainForm, Configuration.Configuration conf)
        {
            this.mainForm = mainForm;
            ServerChannel = new Channel();
            this.server = new ServerCommunicationManager();
            dispatcher = new ServerDispatcher(server, mainForm, conf);

            Socket cmdSocket = InitSocket();
            if (cmdSocket == null)
            {
                System.Windows.MessageBox.Show(HOUSTON_PROBLEM, "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show(HOUSTON_PROBLEM, "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ServerChannel.SetCmdSocket(cmdSocket);            
            closed = false;
           
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
                System.Windows.MessageBox.Show(HOUSTON_PROBLEM, "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }            
            ServerChannel.SetDataSocket(dataSocket);            
            dataSocket = server.Listen(Dns.GetHostName(), DataPort, dataSocket);
            if (dataSocket == null)
            {
                System.Windows.MessageBox.Show(HOUSTON_PROBLEM, "Attenzione!", MessageBoxButton.OK, MessageBoxImage.Error);
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

                    try
                    {
                        dispatcher.StartListeningToData(newClient);
                    }
                    catch (Exception)
                    {
                        StopListeningData();
                        this.closed = true;
                        mainForm.StopFeedbackIcon();
                    }
                }
            }
        }

        private void WaitForClient(Socket serverSocket)
        {
            while (true)
            {
                Client newClient = new Client();
                Socket clientSocket = Accept(serverSocket);
                clientSocket.ReceiveTimeout = 5000;

                if (!(clientSocket == null))
                {
                    newClient.SetSocket(clientSocket);
                    dispatcher.StartListeningTo(newClient);
                }
            }
        }

        private Socket Accept(Socket serverSock)
        {            
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
                socket.Close();
            }
            catch (SocketException)
            {
                return;
            }
        }
    }
}
