using Bonjour;
using System;
using MainApp;
using GenericDataStructure;

namespace Discovery
{
    public class ServiceRegister
    {

        private Bonjour.DNSSDService service = null;
        private short serviceNum;
        
        private Bonjour.DNSSDService cmdRegister = null;
        private Bonjour.DNSSDService dataRegister = null;

        private Bonjour.DNSSDEventManager eventMgr = null;

        public delegate void ServiceRegisteredEventHandler(Object sender, EventArgs param);
        public ServiceRegisteredEventHandler serviceRegisteredHandler;

        public ushort CurrentDataPort { get; set; }
        public ushort CurrentCmdPort { get; set; }

        public ServiceRegister(ushort dataPort, ushort cmdPort, MainForm mainWin)
        {
            eventMgr = new DNSSDEventManager();
            eventMgr.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(ServiceRegistered);
            service = new DNSSDService();
            
            this.CurrentDataPort = dataPort;
            this.CurrentCmdPort = cmdPort;
            this.serviceRegisteredHandler += mainWin.OnServiceRegisterd;
            
            try
            {
                cmdRegister = service.Register(0, 0, System.Net.Dns.GetHostName() + StringConst.CMD_SERVICE_INSTANCE, StringConst.CMD_SERVICE, null, null, CurrentCmdPort, null, eventMgr);
                dataRegister = service.Register(0, 0, System.Net.Dns.GetHostName() + StringConst.DATA_SERVICE_INSTANCE, StringConst.DATA_SERVICE, null, null, CurrentDataPort, null, eventMgr);
            }
            catch (Exception) {
                System.Windows.Forms.MessageBox.Show(StringConst.HOUSTON_PROBLEM, StringConst.HOUSTON_PROBLEM_TITLE, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                Environment.Exit(-1);
            }            
        }
      
        public void Stop()
        {
            try
            {
                service.Stop();
                service = null;
                cmdRegister.Stop();
                cmdRegister = null;
            }
            catch (Exception)
            {
                return;
            }
        }

        public void ServiceRegistered(Bonjour.DNSSDService srvc, Bonjour.DNSSDFlags flags, string s1, string s2, string s3)
        {
            serviceNum++;
            if (serviceNum.Equals(2))
            {
                OnServicesRegistered();
            }            
        }

        private void OnServicesRegistered()
        {
            ServiceRegisteredEventHandler handler = this.serviceRegisteredHandler;
            if (handler != null)
            {
                handler(this, new EventArgs());                
            }
        }
    }
}
