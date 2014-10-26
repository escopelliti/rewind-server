using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using CommunicationLibrary;
using System.IO;
using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PDSProject
{
    class ServerDispatcher
    {

        private static ServerCommunicationManager server;
        public delegate void ChangeClipboardEventHandler(Object sender, Object param);
        public event ChangeClipboardEventHandler clipboardHandler;
        private static Dictionary<string, Delegate> dispatch;

        public delegate void ChangeClipboardFilesEventHandler(Object sender, Object param);
        public event ChangeClipboardFilesEventHandler clipboardFilesHandler;

        public delegate void ReceivingFilesEventHandler(Object sender, Object param);
        public event ReceivingFilesEventHandler receivingFiles;

        public delegate void ChangeClipboardImageEventHandler(Object sender, Object param);
        public event ChangeClipboardImageEventHandler clipboardImageHandler;

        public delegate void SetupClipboardDataTransfer(Object sender, Object param);
        public event SetupClipboardDataTransfer setupClipboardDataTransfer;

        private static List<ProtocolUtils.FileStruct> filesToReceive;
        private static int currentFileNum;
   
        private static System.Collections.Specialized.StringCollection fileDropList;

        private static ConcurrentDictionary<string, RequestState> requestDictionary;

        public ServerDispatcher (ServerCommunicationManager runningServer) {
            server = runningServer;
            filesToReceive = new List<ProtocolUtils.FileStruct>();
            SetupDispatcher();
            ProtocolUtils.InitProtocolDictionary();
            fileDropList = new System.Collections.Specialized.StringCollection();
            currentFileNum = 0;
            requestDictionary = new ConcurrentDictionary<string, RequestState>();
        }

        private void SetupDispatcher()
        {
            dispatch = new Dictionary<string, Delegate>();

            this.clipboardHandler += NewClipboardDataToPaste;
            dispatch[ProtocolUtils.SET_CLIPBOARD_TEXT] = new Action<Object>(obj => clipboardHandler(this, obj));

            this.clipboardFilesHandler += NewClipboardFileToPaste;
            dispatch[ProtocolUtils.SET_CLIPBOARD_FILES] = new Action<Object>(obj => clipboardFilesHandler(this, obj));

            this.receivingFiles += MoveByteToFiles;
            dispatch[ProtocolUtils.TRANSFER_FILES] = new Action<Object>(obj => receivingFiles(this, obj));

            setupClipboardDataTransfer += SendAckToClient;
            dispatch[ProtocolUtils.SET_CLIPBOARD_IMAGE] = new Action<Object>(obj => setupClipboardDataTransfer(this, obj));

            clipboardImageHandler += ReceiveDataForClipboard;
            dispatch[ProtocolUtils.TRANSFER_IMAGE] = new Action<Object>(obj => clipboardImageHandler(this, obj));
        }

        private static void SendAckToClient(Object source, Object param)
        {
            RequestState requestState = (RequestState)param;
            server.Send(Encoding.Unicode.GetBytes(requestState.token), requestState.client.GetSocket());
        }

        private static void ReceiveDataForClipboard(Object source, Object param)
        {
            RequestState requestState = (RequestState)param;
            string filename = ProtocolUtils.protocolDictionary[requestState.type];
            using (var stream = new FileStream(filename, FileMode.Append))
            {
                stream.Write(requestState.data, 0, requestState.data.Length);
                stream.Close();
                server.Send(Convert.FromBase64String(requestState.token), requestState.client.GetSocket());
            }
            if (new FileInfo(filename).Length >= Convert.ToInt64(requestState.stdRequest[ProtocolUtils.CONTENT].ToString())) 
            {
                
                RequestState value = new RequestState();
                if (!requestDictionary.TryRemove(requestState.token, out value))
                {//custom exception would be better than this
                    throw new Exception("Request not present in the dictionary");
                }

                //TODO : AVOID CONDITIONAL TEST
                if (requestState.type == ProtocolUtils.TRANSFER_IMAGE)
                {
                    CreateImageForClipboard(filename);
                }
            }            
        }

        private static void CreateImageForClipboard(string filename)
        {
            Image image = null;
            using (var ms = new MemoryStream(File.ReadAllBytes(filename)))
            {
                image = Image.FromStream(ms);

            }
            File.Delete(filename);
            MainForm.mainForm.Invoke(MainForm.clipboardImageDelegate, image);  
        }

        private void MoveByteToFiles(Object source, Object param)
        {
            RequestState request = (RequestState)param;
            if (currentFileNum < filesToReceive.Count)
            {
                ProtocolUtils.FileStruct currentFile = filesToReceive.ElementAt(currentFileNum);             
                using (var stream = new FileStream(ProtocolUtils.TMP_DIR + currentFile.dir + currentFile.name, FileMode.Append))
                {
                    stream.Write(request.data, 0, request.data.Length);
                    stream.Close();
                    server.Send(Convert.FromBase64String(request.token), request.client.GetSocket());
                }
                if (new FileInfo(ProtocolUtils.TMP_DIR + currentFile.dir + currentFile.name).Length == currentFile.size)
                {
                    currentFileNum++;
                    if (currentFileNum == filesToReceive.Count)
                    {
                        RequestState value = new RequestState();
                        if (!requestDictionary.TryRemove(request.token, out value))
                        {//custom exception would be better than this
                            throw new Exception("Request not present in the dictionary");
                        }
                        MainForm.mainForm.Invoke(MainForm.clipboardFilesDelegate, fileDropList);
                    }
                }
            }
        }

        private static void DeleteFileDirContent(string toRemove) {

            foreach (string dir in Directory.GetDirectories(toRemove)) {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                dirInfo.Delete(true);
            }
            foreach (string file in Directory.GetFiles(toRemove))
            {
                FileInfo fileInfo = new FileInfo(file);
                fileInfo.Delete();
            }

        }


        private void NewClipboardFileToPaste(Object source, Object param)
        {   
            //leggere il JSON e preparare il lavoro per i file e le cartelle che arrivano
            DeleteFileDirContent(ProtocolUtils.TMP_DIR);
            RequestState requestState = (RequestState) param;

            JObject contentJson = (JObject)requestState.stdRequest[ProtocolUtils.CONTENT];

            List<ProtocolUtils.FileStruct> files = new List<ProtocolUtils.FileStruct>();
            files = JsonConvert.DeserializeObject<List<ProtocolUtils.FileStruct>>(contentJson[ProtocolUtils.FILE].ToString());
            filesToReceive.AddRange(files);
            foreach (ProtocolUtils.FileStruct fileStruct in files)
            {
                fileDropList.Add(ProtocolUtils.TMP_DIR + fileStruct.name);
            }

            foreach (var prop in contentJson) {     
                if (prop.Key != ProtocolUtils.FILE)
                {
                    Directory.CreateDirectory(ProtocolUtils.TMP_DIR + prop.Key);
                    CreateClipboardContent((JObject)contentJson[prop.Key], prop.Key);
                    fileDropList.Add(ProtocolUtils.TMP_DIR + prop.Key);
                }
            }
            //foreach( IEnumerable<JToken> item in requestState.stdRequest[ProtocolUtils.CONTENT].Values()) {
            //    Console.WriteLine(item);
            //    Console.WriteLine(item.GetType());
            //    Console.WriteLine(item.Values().ElementAt(0));
            //}
            //foreach (var prop in ((JObject) requestState.stdRequest["content"])) 
            //    //Console.WriteLine(prop.Value.Type);
            //    //Console.WriteLine(prop.Key);
            //foreach (var item in inner)
            //{
            //    JProperty asd = item.First.Value<JProperty>();
            //    Console.WriteLine(asd.Name);
            //}
            //JProperty prop = (JProperty)token;
            //Console.WriteLine(prop.Name);

            
            
            //aggiornare per il caso delle directory
            //Request requestWithFiles = (Request)param;
            //IEnumerable<XElement> filesElement = requestWithFiles.xRequest.Descendants(ProtocolUtils.FILE);
            //foreach (XElement fileElement in filesElement)
            //{
            //    ProtocolUtils.FileStruct fileStruct = new ProtocolUtils.FileStruct();
            //    fileStruct.name = fileElement.Attribute(ProtocolUtils.NAME).Value;
            //    fileStruct.size = Convert.ToInt64(fileElement.Descendants(ProtocolUtils.SIZE).ElementAt(0).Value);
            //    filesToReceive.Add(fileStruct);
            //}

        }

        private void CreateClipboardContent(JObject contentJson, string dir) 
        {
            List<ProtocolUtils.FileStruct> files = new List<ProtocolUtils.FileStruct>();
            files = JsonConvert.DeserializeObject<List<ProtocolUtils.FileStruct>>(contentJson[ProtocolUtils.FILE].ToString());
            filesToReceive.Concat(files);
            foreach (var prop in contentJson) {
                if(prop.Key != ProtocolUtils.FILE) {
                    Directory.CreateDirectory(ProtocolUtils.TMP_DIR + dir + "\\" + prop.Key);
                    CreateClipboardContent((JObject) contentJson[prop.Key], prop.Key);                    
                }
            }
        }


        private void NewClipboardDataToPaste(Object source, Object param)
        {
            //XDocument xRequest = ((Request)param).xRequest;
            JObject stdRequest = ((RequestState)param).stdRequest;
            MainForm.mainForm.Invoke(MainForm.clipboardTextDelegate, stdRequest[ProtocolUtils.CONTENT].ToString());
            RequestState value = new RequestState();
            if (!requestDictionary.TryRemove(((RequestState)param).token, out value))
            {//custom exception would be better than this
                throw new Exception("Request not present in the dictionary");
            }
        }


        public void StartListeningTo(Client client)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(ListenToRequest), client);
        }

        public void StartListeningToData(Client client)
        {
            Socket socket = client.GetSocket();
            while (true)
            {
                byte[] data = new byte[1024];
                int bytesReadNum = server.Receive(data, client.GetSocket());
                ThreadPool.QueueUserWorkItem(new WaitCallback(HandleInput), new List<Object>(){bytesReadNum, data, client});
            }
        }


        private static void HandleInput(Object obj)
        {
            List<Object> objList = (List<Object>) obj;
            int bytesReadNum = (int)objList[0];
            byte[] data = (byte[])objList[1];
            Socket socket = ((Client) objList[2]).GetSocket();
            if (bytesReadNum > 0)
            {
                byte[] actualData = new byte[bytesReadNum];
                System.Buffer.BlockCopy(data, 0, actualData, 0, bytesReadNum);
                string json = Encoding.Unicode.GetString(actualData);
                Console.WriteLine(json);
                INPUT input = JsonConvert.DeserializeObject<INPUT>(json);
                float screenW = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                float screenH = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                input.mi.dx = (UInt16)Math.Round(input.mi.dx * (65535 / screenW), 0);
                input.mi.dy = (UInt16)Math.Round(input.mi.dy * (65535 / screenH), 0);

                server.Send(new byte[] { 0 }, socket);
                INPUT[] inputList = { input };
                SendInput(1, inputList, Marshal.SizeOf(input));
            }
        }

        [DllImport("user32.dll", EntryPoint = "SendInput", SetLastError = true)]
        public static extern UInt32 SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        private static void ListenToRequest(Object newClient)
        {
            Client client = (Client) newClient;
            while (true)
            {
                //qui leggi le richieste e distribuisci ad un thread la richiesta
                byte[] data = new byte[1024];                
                int bytesReadNum = server.Receive(data, client.GetSocket());
                if (bytesReadNum > 0)
                {
                    byte[] actualData = new byte[bytesReadNum];
                    System.Buffer.BlockCopy(data, 0, actualData, 0, bytesReadNum);
                    byte[] byteToken = new byte[16];
                    System.Buffer.BlockCopy(actualData, 0, byteToken, 0, 16);
                    byte[] requestData = new byte[bytesReadNum - 16];
                    System.Buffer.BlockCopy(actualData, 16, requestData, 0, bytesReadNum - 16);
                    string token = Convert.ToBase64String(byteToken);
                                       
                   
                    RequestState request = new RequestState();
                    request.client = client;
                    request.data = requestData;
                    request.token = token;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(DispatchRequest), request);       
                }                   
            } 
        }
 

        private static void DispatchRequest(object request)
        {
            RequestState newRequest = (RequestState)request;                       
            if (requestDictionary.ContainsKey(newRequest.token))
            {
                RequestState oldRequest = requestDictionary[newRequest.token];
                newRequest.type = oldRequest.type;
                newRequest.stdRequest = oldRequest.stdRequest;
                requestDictionary[newRequest.token] = newRequest;
                dispatch[newRequest.type].DynamicInvoke(newRequest);
            }
            else
            {                
                JObject receivedJson = JObject.Parse(Encoding.Unicode.GetString(newRequest.data));
                string type = receivedJson[ProtocolUtils.TYPE].ToString();
                string requestType = ProtocolUtils.protocolDictionary[type];
                newRequest.type = requestType;
                newRequest.stdRequest = receivedJson;
                requestDictionary[newRequest.token] = newRequest;
                dispatch[type].DynamicInvoke(newRequest);
            }
        }
    }

    struct RequestState
    {
        public Client client;
        public byte[] data;
        public JObject stdRequest;
        public string type;
        public string token;
    }
}
