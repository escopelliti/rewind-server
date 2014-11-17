using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                using (var stream = new System.IO.StreamWriter("config.json", false))
                {
                    stream.Write(jsonConf);
                    stream.Close();
                }
            }
        }


        public bool ExistConf()
        {
            return System.IO.File.Exists("config.json");
        }

        public Configuration ReadConf()
        {
            try
            {
                if (!ExistConf())
                    return null;
                String jsonConf = System.IO.File.ReadAllText("config.json");
                Configuration conf = JsonConvert.DeserializeObject<Configuration>(jsonConf);
                return conf;
            }
            catch (JsonException ex)
            {
                return null;
            }
        }
    }
}
