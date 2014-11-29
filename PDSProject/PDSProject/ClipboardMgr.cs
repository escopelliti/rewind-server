using System;
using System.Collections.Generic;
using System.Text;
using ConnectionModule;
using System.Collections.Specialized;
using Protocol;
using System.IO;
using MainApp;
using GenericDataStructure;

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
        private byte[] dataBytes;

        public String CurrentContentToPaste { get; set; }        
        public String TextToPaste { get; set; }
        public System.Drawing.Image ImgToPaste { get; set; }
        public Stream AudioToPaste { get; set; }

        public ClipboardMgr()
        {
            filesToSend = new List<string>();
            currentClipboardDimension = 0;
            currentFileNum = 0;
            offset = 0;
            offset1 = 0;
            CurrentContentToPaste = "NONE";
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
                    case ClipboardPOCO.AUDIO:
                        byte[] audio = (byte[])clipboardContent.content;
                        currentClipboardDimension = audio.Length;
                        break;
                    default:
                        return;                        
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
                    dataBytes = (byte[])clipboardContent.content;
                    byteToSend = DataStandardRequestToSend(ProtocolUtils.SET_CLIPBOARD_IMAGE);
                    break;
                case ClipboardPOCO.AUDIO:
                    dataBytes = (byte[])clipboardContent.content;
                    byteToSend = DataStandardRequestToSend(ProtocolUtils.SET_CLIPBOARD_AUDIO);
                    break;
                default:
                    ResetClassValue();
                    return;
            }
            if (byteToSend == null)
            {
                ServerDispatcher.server.Shutdown(requestState.client.GetSocket(), System.Net.Sockets.SocketShutdown.Both);
                ServerDispatcher.server.Close(requestState.client.GetSocket());
                ResetClassValue();
                return;
            }
            ServerDispatcher.server.Send(byteToSend, requestState.client.GetSocket());
            ResetClassValue();
        }

        public void OnDataToTransfer(Object sender, Object ea)
        {
            RequestEventArgs rea = (RequestEventArgs)ea;
            RequestState rs = (RequestState)rea.requestState;
            ushort chunkLength = 1024;                                               
            if (offset1 < dataBytes.Length)
            {
                int dim = 1024;
                if ((dataBytes.Length - offset1) < dim)
                {
                    dim = (dataBytes.Length - (int)offset1);
                }
                byte[] chunk = new byte[dim];
                try
                {
                    System.Buffer.BlockCopy(dataBytes, (int)offset1, chunk, 0, dim);
                }
                catch (Exception)
                {
                    //problem during bytes copy
                    ServerDispatcher.server.Shutdown(rs.client.GetSocket(), System.Net.Sockets.SocketShutdown.Both);
                    ServerDispatcher.server.Close(rs.client.GetSocket());
                    return;
                }
                    
                offset1 += chunkLength;
                ServerDispatcher.server.Send(chunk, rs.client.GetSocket());
                return;
            }
            offset1-= chunkLength;
            int lastBytes = (dataBytes.Length - (int) offset1); 
            if (lastBytes > 0)
            {
                byte[] chunk = new byte[lastBytes];
                try
                {
                    System.Buffer.BlockCopy(dataBytes, (int)offset1, chunk, 0, lastBytes);                     
                }
                catch (Exception)
                {
                    ServerDispatcher.server.Shutdown(rs.client.GetSocket(), System.Net.Sockets.SocketShutdown.Both);
                    ServerDispatcher.server.Close(rs.client.GetSocket());
                    //problems during bytes copy
                    return;
                }                   
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
                    String[] subFiles = null;
                    try
                    {
                        subFiles = Directory.GetFiles(s, "*.*", SearchOption.AllDirectories);
                    } 
                    catch (Exception) 
                    {
                        this.filesToSend.Clear();
                        return;
                    }
                    
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
                    try
                    {
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
                    catch (Exception)
                    {
                        ServerDispatcher.server.Shutdown(rs.client.GetSocket(), System.Net.Sockets.SocketShutdown.Both);
                        ServerDispatcher.server.Close(rs.client.GetSocket());
                    }                                      
            }
        }

        private byte[] DataStandardRequestToSend(String requestType)
        {
            StandardRequest sr = new StandardRequest();
            sr.type = requestType;
            sr.content = dataBytes.Length.ToString();
            String toSend = JSON.JSONFactory.CreateJSONStandardRequest(sr);
            if (toSend == null)
            {
                return null;
            }
            return Encoding.Unicode.GetBytes(toSend);
        }

        private byte[] TextStandardRequestToSend()
        {
            String text = (String)clipboardContent.content;
            StandardRequest sr = new StandardRequest();
            sr.type = ProtocolUtils.SET_CLIPBOARD_TEXT;
            sr.content = text;
            String toSend = JSON.JSONFactory.CreateJSONStandardRequest(sr);
            if (toSend == null)
            {
                return null;
            }
            return Encoding.Unicode.GetBytes(toSend);
        }
       
        private byte[] FileDropListToByteToSend()
        {
            StringCollection fileDropList = (StringCollection)clipboardContent.content;
            this.fileDropListArray = new String[fileDropList.Count];
            fileDropList.CopyTo(fileDropListArray, 0);
            string toSend = JSON.JSONFactory.CreateFileTransferJSONRequest(Protocol.ProtocolUtils.SET_CLIPBOARD_FILES, fileDropListArray);
            if (toSend == null)
            {
                return null;
            }
            return Encoding.Unicode.GetBytes(toSend);
        }
    }
}
