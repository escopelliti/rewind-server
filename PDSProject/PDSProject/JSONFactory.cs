using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;

namespace JSON
{
    class JSONFactory
    {
        public static string currentDir = ".\\";

        public static string CreateFileTransferJSONRequest(String type, string[] array)
        {
            JObject request = new JObject();
            request.Add(ProtocolUtils.TYPE, type);
            JObject contentJson = new JObject();
            List<ProtocolUtils.FileStruct> fileStructList = new List<ProtocolUtils.FileStruct>();
            string initialDir = currentDir;
            foreach (string file in array)
            {
                
                string name;              
                if (Directory.Exists(file))
                {
                    name = Path.GetFileName(Path.GetFullPath(file));
                    JObject json = new JObject();                    
                    currentDir = currentDir + name + "\\";
                    json = CreateFileTransferRequest(file, json);
                    currentDir = initialDir;
                    contentJson.Add(name, json);
                }
                else
                {
                    name = Path.GetFileName(file);
                    FileInfo fileInfo = new FileInfo(file);
                    ProtocolUtils.FileStruct fileStruct = new ProtocolUtils.FileStruct();
                    fileStruct.name = fileInfo.Name;

                    fileStruct.size = fileInfo.Length;
                    fileStruct.dir = initialDir; 
                    fileStructList.Add(fileStruct);
                }
            }
            contentJson.Add(ProtocolUtils.FILE, JsonConvert.SerializeObject(fileStructList));
            request.Add(ProtocolUtils.CONTENT, contentJson);
            return request.ToString();
        }

        private static JObject CreateFileTransferRequest(string file, JObject json)
        {
            List<ProtocolUtils.FileStruct> fileStructList = new List<ProtocolUtils.FileStruct>();
            foreach (string filename in Directory.GetFiles(file))
            {                   
                FileInfo fileInfo = new FileInfo(filename);
                ProtocolUtils.FileStruct fileStruct = new ProtocolUtils.FileStruct();
                fileStruct.name = fileInfo.Name;
                fileStruct.size = fileInfo.Length;
                fileStruct.dir = currentDir;
                fileStructList.Add(fileStruct);
            }
            if (fileStructList.Count != 0)
            {                  
                json.Add(ProtocolUtils.FILE, JsonConvert.SerializeObject(fileStructList, Formatting.Indented));                    
            }
            if (Directory.GetDirectories(file).Length == 0)
            {                    
                return json;
            }            
            foreach (string dir in Directory.GetDirectories(file))
            {
                string oldCurrentDir = currentDir;
                JObject dirJson = new JObject();
                string directoryName = Path.GetFileName(Path.GetFullPath(dir));
                currentDir = currentDir + directoryName + "\\";
                dirJson = CreateFileTransferRequest(dir, dirJson);
                currentDir = oldCurrentDir;
                json.Add(directoryName, dirJson);              
            }
                return json;
        }
    }
}
