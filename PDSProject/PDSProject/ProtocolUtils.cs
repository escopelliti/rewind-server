using System;
using System.Collections.Concurrent;

namespace Protocol
{
    public class ProtocolUtils
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
        public const string SET_CLIPBOARD_AUDIO = "CLIPBOARD_AUDIO";
        public const string TRANSFER_FILES = "BEGIN_TRANSFER_FILES";
        public const string TRANSFER_IMAGE = "BEGIN_TRANSFER_IMAGE";
        public const string TRANSFER_AUDIO = "BEGIN_TRANSFER_AUDIO";
        public const string GET_CLIPBOARD_DIMENSION = "GET_CLIPBOARD_DIMENSION";
        public const string GET_CLIPBOARD_CONTENT = "GET_CLIPBOARD_CONTENT";
        public const string GET_CLIPBOARD_FILES = "GET_CLIPBOARD_FILES";
        public const string GET_CLIPBOARD_IMG = "GET_CLIPBOARD_IMG";
        public const string GET_CLIPBOARD_AUDIO = "GET_CLIPBOARD_AUDIO";
        public const string GET_CLIPBOARD_DATA = "GET_CLIPBOARD_DATA";
        public const string SET_RESET_FOCUS = "SET_RESET_FOCUS";
        public const string FOCUS_ON = "FOCUS_ON";
        public const string FOCUS_OFF = "FOCUS_OFF";
        public const string TRY_AUTHENTICATE = "TRY_AUTHENTICATE";
        public const string REMOTE_PASTE = "REMOTE_PASTE";

        public const string TMP_IMAGE_FILE = ".\\tmp.jpg";
        public const string TMP_AUDIO_FILE = ".\\tmp.wav";
        public const string TMP_DIR = ".\\resources\\tmp\\";

        public const ushort DEFAULT_DATA_PORT = 12001;
        public const ushort DEFAULT_CMD_PORT = 12000;

        public const ushort TOKEN_DIM = 4;

        public const long CLIBPOARD_DIM_THRESHOLD = 1024 * 1024;

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
            protocolDictionary[SET_CLIPBOARD_AUDIO] = TRANSFER_AUDIO;
            protocolDictionary[TRANSFER_IMAGE] = TMP_IMAGE_FILE;
            protocolDictionary[TRANSFER_AUDIO] = TMP_AUDIO_FILE;

            protocolDictionary[GET_CLIPBOARD_DIMENSION] = GET_CLIPBOARD_DIMENSION;
            protocolDictionary[GET_CLIPBOARD_CONTENT] = GET_CLIPBOARD_CONTENT;
            protocolDictionary[GET_CLIPBOARD_FILES] = GET_CLIPBOARD_FILES;
            protocolDictionary[GET_CLIPBOARD_IMG] = GET_CLIPBOARD_IMG;
            protocolDictionary[GET_CLIPBOARD_DATA] = GET_CLIPBOARD_DATA;
            protocolDictionary[SET_RESET_FOCUS] = SET_RESET_FOCUS;
            protocolDictionary[FOCUS_ON] = FOCUS_ON;
            protocolDictionary[FOCUS_OFF] = FOCUS_OFF;
            protocolDictionary[TRY_AUTHENTICATE] = TRY_AUTHENTICATE;
            protocolDictionary[REMOTE_PASTE] = REMOTE_PASTE;
        }
    }
}
