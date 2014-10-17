using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Collections.Concurrent;

namespace ServerTest
{
    static class ProtocolUtils
    {
        public static ConcurrentDictionary<string, string> protocolDictionary = new ConcurrentDictionary<string, string>();

        public const string REQUEST = "request";
        public const string CONTENT = "content";
        public const string TYPE = "type";
        public const string FILE = "file";
        public const string NAME = "name";
        public const string SIZE = "size";

        public const string SET_CLIPBOARD_TEXT = "CLIPBOARD_TEXT";
        public const string SET_CLIPBOARD_FILES = "CLIPBOARD_FILES";
        public const string TRANSFER_FILES = "BEGIN_TRANSFER";
        public struct FileStruct
        {
            public string name;
            public long size;
            public string dir;
        }

        public static void InitProtocolDictionary()
        {
            protocolDictionary[SET_CLIPBOARD_TEXT] = SET_CLIPBOARD_TEXT;
            protocolDictionary[SET_CLIPBOARD_FILES] = TRANSFER_FILES;            
        }

        public static byte[] ConvertXDocumentToByteArray(XDocument document)
        {

            byte[] byteToSend;
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            using (var memoryStream = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
            {
                document.WriteTo(xmlWriter);
                xmlWriter.Flush();
                byteToSend = memoryStream.ToArray();
            }

            return byteToSend;
        }
    }
}
