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
        private int fileOffset;

        public ClipboardMgr()
        {
            currentFileNum = 0;
            fileOffset = 0;
        }

        public void OnGetDimensionRequest(Object sender, Object param)
        {
            RequestEventArgs rea = (RequestEventArgs)param;
            RequestState requestState = (RequestState)rea.requestState;

            // TO BE IMPLEME.
        }

        public void OnGetContentRequest(Object sender, Object param)
        {
            RequestEventArgs rea = (RequestEventArgs)param;
            RequestState requestState = (RequestState)rea.requestState;
            ClipboardPOCO clipboardContent = MainForm.GetClipboardContent();
            switch (clipboardContent.contentType)
            {
                case ClipboardPOCO.FILE_DROP_LIST:
                    byte[] byteToSend = GetFileDropListFromClipboard(clipboardContent);
                    ServerDispatcher.server.Send(byteToSend, requestState.client.GetSocket());
                    OnFileToTransferEvent(this, new EventArgs());                    
                    break;
            }
        }

        public void OnFileToTransferEvent(Object sender, Object ea)
        {
            RequestEventArgs rea = (RequestEventArgs) ea;
            RequestState rs = (RequestState) rea.requestState;
            if (currentFileNum == this.fileDropListArray.Length)
            {
                return;
            }
            String file = this.fileDropListArray[currentFileNum];
            if (File.Exists(file))
                {
                    byte[] bytesFile = new byte[1024];
                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        int bytesReadNum;
                        bytesReadNum = stream.Read(bytesFile, fileOffset, bytesFile.Length);
                        if (bytesReadNum > 0)
                        {
                            byte[] bytesFileToSend = new byte[bytesReadNum];
                            System.Buffer.BlockCopy(bytesFile, 0, bytesFileToSend, 0, bytesReadNum);
                            ServerDispatcher.server.Send(bytesFileToSend, rs.client.GetSocket());
                            fileOffset += bytesReadNum;
                            if (fileOffset >= new FileInfo(file).Length)
                            {
                                currentFileNum++;
                            }                                                        
                        }
                        stream.Close();
                    }
                
            }
        }
       
        private byte[] GetFileDropListFromClipboard(ClipboardPOCO clipboardContent)
        {
            StringCollection fileDropList = (StringCollection)clipboardContent.content;
            this.fileDropListArray = new String[fileDropList.Count];
            fileDropList.CopyTo(fileDropListArray, 0);
            string toSend = JSON.JSONFactory.CreateFileTransferJSONRequest(Protocol.ProtocolUtils.SET_CLIPBOARD_FILES, fileDropListArray);
            return Encoding.Unicode.GetBytes(toSend);
        }
    }
}
