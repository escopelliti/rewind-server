using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDSProject;
using System.Collections.Specialized;
using Protocol;
using System.IO;

namespace Clipboard
{
    public class ClipboardMgr
    {   
        public String[] fileDropListArray {get; set;}
        private int currentFileNum;
        private long offset;
        private uint offset1;
        private ClipboardPOCO clipboardContent;
        private long currentClipboardDimension;
        private List<String> filesToSend;
        private byte[] imageBytes;

        public ClipboardMgr()
        {
            filesToSend = new List<string>();
            currentClipboardDimension = 0;
            currentFileNum = 0;
            offset = 0;
            offset1 = 0;
        }

        public void OnGetDimensionRequest(Object sender, Object param)
        {            
            RequestState requestState = (RequestState)param;
            filesToSend.Clear();
            clipboardContent = MainForm.GetClipboardContent();
            if (clipboardContent != null)
            {
                switch (clipboardContent.contentType)
                {
                    case ClipboardPOCO.FILE_DROP_LIST:
                        StringCollection strcoll = (StringCollection)clipboardContent.content;
                        foreach (string s in strcoll)
                        {
                            if (File.Exists(s))
                            {
                                FileInfo f = new FileInfo(s);
                                currentClipboardDimension += f.Length;
                            }
                            else
                            {
                                currentClipboardDimension += GetDirectorySize(s);
                            }
                        }
                        break;
                    case ClipboardPOCO.TEXT:
                        String clipboardText = (String)clipboardContent.content;
                        currentClipboardDimension = clipboardText.Length;
                        break;
                    case ClipboardPOCO.IMAGE:
                        byte[] img = (byte[])clipboardContent.content;
                        currentClipboardDimension = img.Length;
                        break;
                }
            }
            else
            {
                currentClipboardDimension = 0;
            }
            byte[] byteToSend = BitConverter.GetBytes(currentClipboardDimension);
            currentClipboardDimension = 0;
            ServerDispatcher.server.Send(byteToSend, requestState.client.GetSocket());
        }

        private long GetDirectorySize(string p)
        {
            string[] a = Directory.GetFiles(p, "*.*", SearchOption.AllDirectories);
            long b = 0;
            foreach (string name in a)
            {
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            return b;
        }
        
        public void OnGetContentRequest(Object sender, Object param)
        {            
            RequestState requestState = (RequestState)param;
            byte[] byteToSend = null;
            switch (clipboardContent.contentType)
            {
                case ClipboardPOCO.FILE_DROP_LIST:
                    byteToSend = FileDropListToByteToSend();                    
                    SetFilesToSendFromDropList();                                        
                    break;
                case ClipboardPOCO.TEXT:
                    byteToSend = TextStandardRequestToSend();
                    break;
                case ClipboardPOCO.IMAGE:
                    imageBytes = (byte[])clipboardContent.content;
                    byteToSend = ImgStandardRequestToSend();
                    break;
            }
            ServerDispatcher.server.Send(byteToSend, requestState.client.GetSocket());
            ResetClassValue();
        }

        public void OnImageToTransfer(Object sender, Object ea)
        {
            RequestEventArgs rea = (RequestEventArgs)ea;
            RequestState rs = (RequestState)rea.requestState;
            ushort chunkLength = 1024;                                               
            if (offset1 < imageBytes.Length)
            {
                int dim = 1024;
                if ((imageBytes.Length - offset1) < dim)
                {
                    dim = (imageBytes.Length - (int)offset1);
                }
                byte[] chunk = new byte[dim];
                System.Buffer.BlockCopy(imageBytes,(int) offset1, chunk, 0, dim);
                offset1 += chunkLength;
                ServerDispatcher.server.Send(chunk, rs.client.GetSocket());
                return;
            }
            offset1-= chunkLength;
            int lastBytes = (imageBytes.Length - (int) offset1); 
            if (lastBytes > 0)
            {
                byte[] chunk = new byte[lastBytes];
                System.Buffer.BlockCopy(imageBytes, (int) offset1, chunk, 0, lastBytes);
                ServerDispatcher.server.Send(chunk, rs.client.GetSocket());
            }
        }

        private void ResetClassValue()
        {
            currentFileNum = 0;
            offset= 0;
            offset1 = 0;
            currentClipboardDimension= 0;
        }

        private void SetFilesToSendFromDropList()
        {
            foreach (String s in fileDropListArray)
            {
                if (File.Exists(s))
                {
                    this.filesToSend.Add(s);
                   //forse si puo rimuovere
                }
            }
            foreach (String s in fileDropListArray) {
                if (!File.Exists(s))
                {
                     String[] subFiles = Directory.GetFiles(s, "*.*", SearchOption.AllDirectories);
                    foreach(String file in subFiles)
                    {
                        this.filesToSend.Add(file);
                    }
                }
            }             
        }

        public void OnFileToTransferEvent(Object sender, Object ea)
        {
            RequestEventArgs rea = (RequestEventArgs) ea;
            RequestState rs = (RequestState) rea.requestState;
            if (currentFileNum == this.filesToSend.Count)
            {
                ServerDispatcher.server.Send(new byte[16], rs.client.GetSocket());
                return;
            }
            String file = this.filesToSend[currentFileNum];
            FileInfo fileInfo = new FileInfo(file);
            long fileSize = fileInfo.Length;            
            long dim = 0;
            if (File.Exists(file))
                {
                    byte[] bytesFile = new byte[1024];                    
                    using (var input = File.OpenRead(file))
                    {
                        int bytesReadNum;
                        dim = bytesFile.Length;
                        if ((fileSize - offset) < bytesFile.Length)
                        {
                            dim = (fileSize - offset);
                        }
                        input.Position = offset;                        
                        bytesReadNum = input.Read(bytesFile, 0, (int)dim);
                        if (bytesReadNum > 0)
                        {
                            byte[] bytesFileToSend = new byte[bytesReadNum];
                            System.Buffer.BlockCopy(bytesFile, 0, bytesFileToSend, 0, bytesReadNum);
                            ServerDispatcher.server.Send(bytesFileToSend, rs.client.GetSocket());
                            offset += bytesReadNum;
                            
                            if (offset >= new FileInfo(file).Length)
                            {
                                currentFileNum++;
                                offset = 0;
                            }                                                        
                        }
                        input.Close();
                    }
                
            }
        }

        private byte[] ImgStandardRequestToSend()
        {
            StandardRequest sr = new StandardRequest();
            sr.type = ProtocolUtils.SET_CLIPBOARD_IMAGE;
            sr.content = imageBytes.Length.ToString();
            String toSend = JSON.JSONFactory.CreateJSONStandardRequest(sr);
            return Encoding.Unicode.GetBytes(toSend);
        }

        private byte[] TextStandardRequestToSend()
        {
            String text = (String)clipboardContent.content;
            StandardRequest sr = new StandardRequest();
            sr.type = ProtocolUtils.SET_CLIPBOARD_TEXT;
            sr.content = text;
            String toSend = JSON.JSONFactory.CreateJSONStandardRequest(sr);
            return Encoding.Unicode.GetBytes(toSend);
        }
       
        private byte[] FileDropListToByteToSend()
        {
            StringCollection fileDropList = (StringCollection)clipboardContent.content;
            this.fileDropListArray = new String[fileDropList.Count];
            fileDropList.CopyTo(fileDropListArray, 0);
            string toSend = JSON.JSONFactory.CreateFileTransferJSONRequest(Protocol.ProtocolUtils.SET_CLIPBOARD_FILES, fileDropListArray);
            return Encoding.Unicode.GetBytes(toSend);
        }
    }
}
