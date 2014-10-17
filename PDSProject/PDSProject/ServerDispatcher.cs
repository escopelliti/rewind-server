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
using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerTest;

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
        }

        private void MoveByteToFiles(Object source, Object param)
        {
            RequestState request = (RequestState)param;
            if (currentFileNum < filesToReceive.Count)
            {
                ProtocolUtils.FileStruct currentFile = filesToReceive.ElementAt(currentFileNum);             
                using (var stream = new FileStream(".\\tmp\\" + currentFile.dir + currentFile.name, FileMode.Append))
                {
                    stream.Write(request.data, 0, request.data.Length);
                    stream.Close();
                    server.Send(Convert.FromBase64String(request.token), request.client.GetSocket());
                }
                if (new FileInfo(".\\tmp\\" + currentFile.dir + currentFile.name).Length == currentFile.size)
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

        private static void DeleteTmpContent() {

            foreach (string dir in Directory.GetDirectories(".\\tmp")) {
                DirectoryInfo dirInfo = new DirectoryInfo(dir);
                dirInfo.Delete(true);
            }
            foreach (string file in Directory.GetFiles(".\\tmp"))
            {
                FileInfo fileInfo = new FileInfo(file);
                fileInfo.Delete();
            }

        }


        private void NewClipboardFileToPaste(Object source, Object param)
        {   
            //leggere il JSON e preparare il lavoro per i file e le cartelle che arrivano
            DeleteTmpContent();
            RequestState requestState = (RequestState) param;

            JObject contentJson = (JObject)requestState.stdRequest[ProtocolUtils.CONTENT];

            List<ProtocolUtils.FileStruct> files = new List<ProtocolUtils.FileStruct>();
            files = JsonConvert.DeserializeObject<List<ProtocolUtils.FileStruct>>(contentJson[ProtocolUtils.FILE].ToString());
            filesToReceive.AddRange(files);
            foreach (ProtocolUtils.FileStruct fileStruct in files)
            {
                fileDropList.Add(".\\tmp\\" + fileStruct.name);
            }

            foreach (var prop in contentJson) {     
                if (prop.Key != ProtocolUtils.FILE)
                {                   
                    Directory.CreateDirectory(".\\tmp\\" + prop.Key);
                    CreateClipboardContent((JObject)contentJson[prop.Key], prop.Key);
                    fileDropList.Add(".\\tmp\\" + prop.Key);
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
                    Directory.CreateDirectory(".\\tmp\\" + dir + "\\" + prop.Key);
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
                //XDocument xRequest = new XDocument();
                //MemoryStream ms = new MemoryStream(newRequest.data);
                //xRequest = XDocument.Load(ms);
                //string requestType = xRequest.Descendants(ProtocolUtils.TYPE).ElementAt(0).Value.ToString();
                //StandardRequest receivedRequest = JsonConvert.DeserializeObject<StandardRequest>(Encoding.Unicode.GetString(newRequest.data));
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
