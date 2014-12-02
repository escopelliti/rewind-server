using System;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using GenericDataStructure;

namespace Configuration
{
    public class ConfigurationMgr
    {
        public Configuration WriteConf(ushort dataPort, ushort cmdPort, string pswDigest, bool delete)
        {
            {
                Configuration conf = new Configuration();
                conf.CmdPort = cmdPort.ToString();
                conf.DataPort = dataPort.ToString();
                conf.Psw = pswDigest;
                conf.Delete = delete;
                try
                {
                    String jsonConf = JsonConvert.SerializeObject(conf);
                    using (var stream = new System.IO.StreamWriter(GenericDataStructure.StringConst.CONFIG_FILE, false))
                    {
                        stream.Write(jsonConf);
                        stream.Close();
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                return conf;
            }
        }

        public bool ExistConf()
        {
            return System.IO.File.Exists(GenericDataStructure.StringConst.CONFIG_FILE);
        }

        public Configuration ReadConf()
        {
            try
            {
                if (!ExistConf())
                {
                    String psw = StringConst.DEFAULT_PSW;
                    String hashString = CreateHashString(psw);
                    System.Windows.Forms.MessageBox.Show(StringConst.DEFAULT_CONF, StringConst.OPERATION_COMPLETED, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    return WriteConf(Protocol.ProtocolUtils.DEFAULT_DATA_PORT, Protocol.ProtocolUtils.DEFAULT_CMD_PORT, hashString, false);                    
                }                    
                String jsonConf = System.IO.File.ReadAllText(GenericDataStructure.StringConst.CONFIG_FILE);
                Configuration conf = JsonConvert.DeserializeObject<Configuration>(jsonConf);
                return conf;
            }
            catch (JsonException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void DeleteCurrentConf()
        {
            try
            {
                System.IO.File.Delete(StringConst.CONFIG_FILE);
            }
            catch (Exception)
            {
                return;
            }
                        
        }

        public String CreateHashString(string psw)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(psw);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:X2}", b);
            }
            string hashString = stringBuilder.ToString();
            return hashString;
        }
    }    
}
