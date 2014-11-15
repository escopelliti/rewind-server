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

using Clipboard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PDSProject
{
    class ServerDispatcher
    {

        public static ServerCommunicationManager server { get; set; }
        private ClipboardMgr clipboardMgr;
        private MainForm mainForm;
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

        public delegate void GetClipboardContentEventHandler(Object sender, Object param);
        public event GetClipboardContentEventHandler getClipboardContentHandler;

        public delegate void GetClipboardDimensionEventHandler(Object sender, Object param);
        public event GetClipboardDimensionEventHandler getClipboardDimensionHandler;

        public delegate void FileToTransferEventHandler(Object sender, Object param);
        public event FileToTransferEventHandler fileTotransferHandler;

        public delegate void ImgToTransferEventHandler(Object sender, Object param);
        public event ImgToTransferEventHandler imgTotransferHandler;

        public delegate void SetActiveServerEventHandler(Object sender, Object param);
        public event SetActiveServerEventHandler setActiveServerHandler;

        public ServerDispatcher (ServerCommunicationManager runningServer, MainForm mainWindow) {
            server = runningServer;
            this.mainForm = mainWindow;
            filesToReceive = new List<ProtocolUtils.FileStruct>();
            fileDropList = new System.Collections.Specialized.StringCollection();
            currentFileNum = 0;
            requestDictionary = new ConcurrentDictionary<string, RequestState>();           
            clipboardMgr = new ClipboardMgr();
            ProtocolUtils.InitProtocolDictionary();
            SetupDispatcher();
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

            getClipboardDimensionHandler += clipboardMgr.OnGetDimensionRequest;
            dispatch[ProtocolUtils.GET_CLIPBOARD_DIMENSION] = new Action<Object>(obj => OnGetClipboardDimension(new RequestEventArgs((RequestState) obj)));

            getClipboardContentHandler += clipboardMgr.OnGetContentRequest;
            dispatch[ProtocolUtils.GET_CLIPBOARD_CONTENT] = new Action<Object>(obj => OnGetClipboardContent(new RequestEventArgs((RequestState) obj)));

            fileTotransferHandler += clipboardMgr.OnFileToTransferEvent;
            dispatch[ProtocolUtils.GET_CLIPBOARD_FILES] = new Action<Object>(obj => OnFileToTransfer(new RequestEventArgs((RequestState) obj)));

            imgTotransferHandler += clipboardMgr.OnImageToTransfer;
            dispatch[ProtocolUtils.GET_CLIPBOARD_IMG] = new Action<Object>(obj => OnImgToTransfer(new RequestEventArgs((RequestState)obj)));

            setActiveServerHandler += mainForm.OnSetServerFocus;
            dispatch[ProtocolUtils.SET_RESET_FOCUS] = new Action<Object>(obj => OnSetServerFocus(new RequestEventArgs((RequestState)obj)));
        }

        private void OnSetServerFocus(RequestEventArgs ea)
        {
            SetActiveServerEventHandler handler = this.setActiveServerHandler;
            if (handler != null)
            {
                handler(this, ea);
                server.Send(new byte[16], ea.requestState.client.GetSocket());
            }
        }

        private void OnImgToTransfer(RequestEventArgs ea)
        {
            ImgToTransferEventHandler handler = this.imgTotransferHandler;
            if (handler != null)
            {
                handler(this, ea);
            }
        }

        private void OnFileToTransfer(RequestEventArgs ea)
        {
            FileToTransferEventHandler handler = this.fileTotransferHandler;
            if (handler != null)
            {
                handler(this, ea);
            }
        }

        private void OnGetClipboardContent(RequestEventArgs ea)
        {
            GetClipboardContentEventHandler handler = this.getClipboardContentHandler;
            if (handler != null)
            {
                handler(this, ea.requestState);
                RequestState value = new RequestState();
                if (!requestDictionary.TryRemove(ea.requestState.token, out value))
                {//custom exception would be better than this
                    throw new Exception("Request not present in the dictionary");
                }
            }
        }

        private void OnGetClipboardDimension(RequestEventArgs ea)
        {
            GetClipboardDimensionEventHandler handler = this.getClipboardDimensionHandler;
            if (handler != null)
            {
                handler(this, ea.requestState);
            }
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
            byte[] actualData = new byte[request.data.Length - ProtocolUtils.TOKEN_DIM];
            System.Buffer.BlockCopy(request.data, ProtocolUtils.TOKEN_DIM, actualData, 0, actualData.Length); 
            if (currentFileNum < filesToReceive.Count)
            {
                ProtocolUtils.FileStruct currentFile = filesToReceive.ElementAt(currentFileNum);             
                using (var stream = new FileStream(ProtocolUtils.TMP_DIR + currentFile.dir + currentFile.name, FileMode.Append))
                {
                    stream.Write(actualData, 0, actualData.Length);
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
                        currentFileNum = 0;
                        filesToReceive.Clear();
                        fileDropList.Clear();
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
            String fullTmpPath = Path.GetFullPath(ProtocolUtils.TMP_DIR);
            //leggere il JSON e preparare il lavoro per i file e le cartelle che arrivano
            DeleteFileDirContent(ProtocolUtils.TMP_DIR);
            RequestState requestState = (RequestState) param;

            JObject contentJson = (JObject)requestState.stdRequest[ProtocolUtils.CONTENT];

            List<ProtocolUtils.FileStruct> files = new List<ProtocolUtils.FileStruct>();

            try
            {
                files = JsonConvert.DeserializeObject<List<ProtocolUtils.FileStruct>>(contentJson[ProtocolUtils.FILE].ToString());
                filesToReceive.AddRange(files);
            }
            catch (NullReferenceException ex)
            {
            }
            foreach (ProtocolUtils.FileStruct fileStruct in files)
            {
                fileDropList.Add(fullTmpPath + fileStruct.name);
            }

            foreach (var prop in contentJson) {     
                if (prop.Key != ProtocolUtils.FILE)
                {
                    Directory.CreateDirectory(ProtocolUtils.TMP_DIR + prop.Key);
                    CreateClipboardContent((JObject)contentJson[prop.Key], prop.Key);
                    fileDropList.Add(fullTmpPath + prop.Key);
                }
            }
            server.Send(new byte[16], requestState.client.GetSocket());
        }

        private void CreateClipboardContent(JObject contentJson, string dir) 
        {
            List<ProtocolUtils.FileStruct> files = new List<ProtocolUtils.FileStruct>();
            try
            {
                files = JsonConvert.DeserializeObject<List<ProtocolUtils.FileStruct>>(contentJson[ProtocolUtils.FILE].ToString());
                filesToReceive.AddRange(files);
            }
            catch (NullReferenceException ex)
            {

            }
            foreach (var prop in contentJson) {
                if(prop.Key != ProtocolUtils.FILE) {
                    Directory.CreateDirectory(ProtocolUtils.TMP_DIR + dir + "\\" + prop.Key);
                    CreateClipboardContent((JObject)contentJson[prop.Key], dir + "\\" + prop.Key);                    
                }
            }
        }


        private void NewClipboardDataToPaste(Object source, Object param)
        {            
            JObject stdRequest = ((RequestState)param).stdRequest;
            MainForm.mainForm.Invoke(MainForm.clipboardTextDelegate, stdRequest[ProtocolUtils.CONTENT].ToString());
            RequestState value = new RequestState();
            if (!requestDictionary.TryRemove(((RequestState)param).token, out value))
            {//custom exception would be better than this
                throw new Exception("Request not present in the dictionary");
            }
            server.Send(new byte[16], ((RequestState)param).client.GetSocket());
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
                Console.WriteLine();
                INPUT input = JsonConvert.DeserializeObject<INPUT>(json);
                if (input.type == 0) { 
                    float screenW = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                    float screenH = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                    input.mi.dx = (UInt16)Math.Round(input.mi.dx * (65535 / screenW), 0);
                    input.mi.dy = (UInt16)Math.Round(input.mi.dy * (65535 / screenH), 0);
                }
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
                byte[] data = new byte[64*1024];                
                int bytesReadNum = server.Receive(data, client.GetSocket());
                if (bytesReadNum > 0)
                {
                    byte[] actualData = new byte[bytesReadNum];
                    System.Buffer.BlockCopy(data, 0, actualData, 0, bytesReadNum);
                    data = null;
                    byte[] byteToken = new byte[ProtocolUtils.TOKEN_DIM];
                    System.Buffer.BlockCopy(actualData, 0, byteToken, 0, ProtocolUtils.TOKEN_DIM);
                    byte[] requestData = new byte[bytesReadNum - ProtocolUtils.TOKEN_DIM];
                    System.Buffer.BlockCopy(actualData, ProtocolUtils.TOKEN_DIM, requestData, 0, bytesReadNum - ProtocolUtils.TOKEN_DIM);
                    string token = Convert.ToBase64String(byteToken);
                                       
                   
                    RequestState request = new RequestState();
                    request.client = client;
                    request.data = requestData;
                    request.token = token;


                    Thread thread = new Thread(() => DispatchRequest(request));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(DispatchRequest), request);       
                }                   
            } 
        }
 

        private static void DispatchRequest(object request)
        {
            RequestState newRequest = (RequestState)request;
            Console.WriteLine(newRequest.data.Length);          
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
                Console.WriteLine(newRequest.data.Length);
                string type = receivedJson[ProtocolUtils.TYPE].ToString();
                string requestType = ProtocolUtils.protocolDictionary[type];
                newRequest.type = requestType;
                newRequest.stdRequest = receivedJson;
                requestDictionary[newRequest.token] = newRequest;
                dispatch[type].DynamicInvoke(newRequest);
            }
        }
    }

    public struct RequestState
    {
        public Client client;
        public byte[] data;
        public JObject stdRequest;
        public string type;
        public string token;
    }
}
