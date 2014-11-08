﻿using System;
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
        private int fileOffset;
        private ClipboardPOCO clipboardContent;
        private long currentClipboardDimension;
        private List<String> filesToSend;

        public ClipboardMgr()
        {
            filesToSend = new List<string>();
            currentClipboardDimension = 0;
            currentFileNum = 0;
            fileOffset = 0;
        }

        public void OnGetDimensionRequest(Object sender, Object param)
        {
            //RequestEventArgs rea = (RequestEventArgs)param;
            RequestState requestState = (RequestState)param;
            //RequestState requestState = (RequestState)rea.requestState;
            filesToSend.Clear();
            clipboardContent = MainForm.GetClipboardContent();
            

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
            }
            byte[] byteToSend = BitConverter.GetBytes(currentClipboardDimension);
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
           
            switch (clipboardContent.contentType)
            {
                case ClipboardPOCO.FILE_DROP_LIST:
                    byte[] byteToSend = FileDropListToByteToSend();
                    ServerDispatcher.server.Send(byteToSend, requestState.client.GetSocket());
                    ResetClassValue();
                    SetFilesToSendFromDropList();
                    OnFileToTransferEvent(this, new RequestEventArgs(requestState));                    
                    break;
            }
        }

        private void ResetClassValue()
        {
            currentFileNum = 0;
            fileOffset= 0;
            currentClipboardDimension= 0;
        }

        private void SetFilesToSendFromDropList()
        {
            foreach (String s in fileDropListArray)
            {
                if (File.Exists(s))
                {
                    this.filesToSend.Add(s);
                }
                else
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
                return;
            }
            String file = this.filesToSend[currentFileNum];
            FileInfo fileInfo = new FileInfo(file);
            long fileSize = fileInfo.Length;            
            long dim = 0;
            if (File.Exists(file))
                {
                    byte[] bytesFile = new byte[1024];
                    //using (var stream = new FileStream(file, FileMode.Open))
                    using (var input = File.OpenRead(file))
                    {
                        int bytesReadNum;
                        dim = bytesFile.Length;
                        if ((fileSize - fileOffset) < bytesFile.Length)
                        {
                            dim = (fileSize - fileOffset);
                        }
                        input.Position = fileOffset;
                        //bytesReadNum = stream.Read(bytesFile, fileOffset,(int) dim);
                        bytesReadNum = input.Read(bytesFile, 0, (int)dim);
                        if (bytesReadNum > 0)
                        {
                            byte[] bytesFileToSend = new byte[bytesReadNum];
                            System.Buffer.BlockCopy(bytesFile, 0, bytesFileToSend, 0, bytesReadNum);
                            ServerDispatcher.server.Send(bytesFileToSend, rs.client.GetSocket());
                            fileOffset += bytesReadNum;
                            
                            if (fileOffset >= new FileInfo(file).Length)
                            {
                                currentFileNum++;
                                fileOffset = 0;
                            }                                                        
                        }
                        input.Close();
                        //stream.Close();
                    }
                
            }
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
