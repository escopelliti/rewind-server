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
        public const string SET_CLIPBOARD_IMAGE = "CLIPBOARD_IMAGE";
        public const string TRANSFER_FILES = "BEGIN_TRANSFER_FILES";
        public const string TRANSFER_IMAGE = "BEGIN_TRANSFER_IMAGE";
        
        public const string TMP_IMAGE_FILE = ".\\tmp.jpg";
        public const string TMP_DIR = ".\\tmp\\";

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
            protocolDictionary[SET_CLIPBOARD_IMAGE] = TRANSFER_IMAGE;
            protocolDictionary[TRANSFER_IMAGE] = TMP_IMAGE_FILE;
        }
    }
}
