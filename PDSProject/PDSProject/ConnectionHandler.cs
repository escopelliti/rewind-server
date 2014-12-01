using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using MainApp;
using System.Windows;
using ConnectionModule.CommunicationLibrary;
using GenericDataStructure;
using System.Collections.Generic;

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
        public List<Client> clients;
         
        private const String HOUSTON_PROBLEM = "Sembra esserci qualche problema, prova a riavviare l'applicazione";
 
        public ConnectionHandler(MainForm mainForm, Configuration.Configuration conf)
        {
            this.mainForm = mainForm;
            ServerChannel = new Channel();
            this.server = new ServerCommunicationManager();
            dispatcher = new ServerDispatcher(server, mainForm, conf);
            clients = new List<Client>();

            Socket cmdSocket = InitSocket();
            if (cmdSocket == null)
            {
                System.Windows.MessageBox.Show(StringConst.HOUSTON_PROBLEM, StringConst.HOUSTON_PROBLEM_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show(StringConst.HOUSTON_PROBLEM, StringConst.HOUSTON_PROBLEM_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
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
                System.Windows.MessageBox.Show(StringConst.HOUSTON_PROBLEM, StringConst.HOUSTON_PROBLEM_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }            
            ServerChannel.SetDataSocket(dataSocket);            
            dataSocket = server.Listen(Dns.GetHostName(), DataPort, dataSocket);
            if (dataSocket == null)
            {
                System.Windows.MessageBox.Show(StringConst.HOUSTON_PROBLEM, StringConst.HOUSTON_PROBLEM_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
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
                Socket clientSocket = Accept(serverDataSocket);
                string ipAddressOnDataSocket = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                Client newClient = clients.Find( x => (((IPEndPoint)x.CmdSocket.RemoteEndPoint).Address.ToString()).Equals(ipAddressOnDataSocket));
                if (!(clientSocket == null))
                {
                    newClient.DataSocket = clientSocket;
                    try
                    {
                        //Thread checkThread = new Thread(() => IsDstReacheable(newClient));
                        //checkThread.Start();
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
                if (!(clientSocket == null))
                {
                    newClient.CmdSocket = clientSocket;
                    dispatcher.StartListeningTo(newClient);
                    clients.Add(newClient);
                }
            }
        }

        private void IsDstReacheable(Client client)
        {
            Socket s = client.CmdSocket;
            int timeToWait = 10;
            bool channelIsOpened = true;

            Socket checkSocket = server.CreateSocket(ProtocolType.Tcp);
            if (checkSocket == null) 
            {
                //destination unreacheable
                channelIsOpened = false;
                CloseChannel(client);                
            }
            else
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(((IPEndPoint)s.RemoteEndPoint).Address.ToString()), 40000);
                try
                {
                    checkSocket.Connect(remoteEP);
                }
                catch (Exception)
                {
                    //destination unreacheable
                    channelIsOpened = false;
                    CloseChannel(client);                
                }

                while (channelIsOpened)
                {
                    int byteRead = 0;
                    try
                    {
                        byte[] BytesTosend = new byte[1];
                        checkSocket.Send(BitConverter.GetBytes(timeToWait));
                        checkSocket.ReceiveTimeout = timeToWait*1000;
                        byteRead = checkSocket.Receive(new byte[1]);                        
                    }
                    catch (Exception)
                    {
                        //destination unreacheable
                        CloseChannel(client);  
                        CloseSocket(checkSocket);
                        break;
                    }

                    if (byteRead > 0)
                    {
                        //destination reacheable
                        if (timeToWait <= 30)
                        {
                            timeToWait += 10;
                        }
                    }
                    else
                    {
                        //destination unreacheable
                        channelIsOpened = false;
                        CloseChannel(client);
                        CloseSocket(checkSocket);
                        break;
                    }
                    Thread.Sleep(10000);
                }
            }
        }


        private void CloseChannel(Client client)
        {

            CloseSocket(client.CmdSocket);
            if (client.DataSocket != null)
            {
                CloseSocket(client.DataSocket);
            }
            this.clients.Remove(client);
        }

        private void CloseSocket(Socket s)
        {
            try
            {
                server.Shutdown(s, SocketShutdown.Both);
                server.Close(s);
            }
            catch (Exception)
            {
                return;
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
