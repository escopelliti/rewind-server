using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clipboard
{
    public class ClipboardPOCO
    {
        public string contentType { get; set; }
        public Object content { get; set; }

        public const string FILE_DROP_LIST = "FILE_DROP_LIST";
        public const string IMAGE = "IMAGE";
        public const string AUDIO = "AUDIO";
        public const string TEXT = "TEXT";
    }
}
