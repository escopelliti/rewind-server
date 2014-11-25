using System;
using Newtonsoft.Json;

namespace Configuration
{
    public class ConfigurationMgr
    {
        public void WriteConf(ushort dataPort, ushort cmdPort, string pswDigest)
        {
            {
                Configuration conf = new Configuration();
                conf.CmdPort = cmdPort.ToString();
                conf.DataPort = dataPort.ToString();
                conf.Psw = pswDigest;
                String jsonConf = JsonConvert.SerializeObject(conf);
                using (var stream = new System.IO.StreamWriter(CONFIG_FILE, false))
                {
                    stream.Write(jsonConf);
                    stream.Close();
                }
            }
        }

        public bool ExistConf()
        {
            return System.IO.File.Exists(CONFIG_FILE);
        }

        public Configuration ReadConf()
        {
            try
            {
                if (!ExistConf())
                    return null;
                String jsonConf = System.IO.File.ReadAllText(CONFIG_FILE);
                Configuration conf = JsonConvert.DeserializeObject<Configuration>(jsonConf);
                return conf;
            }
            catch (JsonException ex)
            {
                return null;
            }
        }
        private const String CONFIG_FILE = "config.json";
    }    
}
