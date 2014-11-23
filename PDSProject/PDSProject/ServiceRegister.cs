using Bonjour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDSProject;

namespace Discovery
{



    public class ServiceRegister
    {

        private Bonjour.DNSSDService m_service = null;
        public short serviceNum { get; set; }

        //A Handle for the registered record
        private Bonjour.DNSSDService m_registrar = null;
        private Bonjour.DNSSDService m_registrar1 = null;

        private Bonjour.DNSSDEventManager m_eventManager = null;

        public delegate void ServiceRegisteredEventHandler(Object sender, EventArgs param);
        public ServiceRegisteredEventHandler serviceRegisteredHandler;

        public ushort CurrentDataPort { get; set; }
        public ushort CurrentCmdPort { get; set; }

        public ServiceRegister(ushort dataPort, ushort cmdPort, MainForm mainWin)
        {
            m_eventManager = new DNSSDEventManager();
            m_eventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(ServiceRegistered);
            m_service = new DNSSDService();
            this.CurrentDataPort = dataPort;
            this.CurrentCmdPort = cmdPort;
            this.serviceRegisteredHandler += mainWin.OnServiceRegisterd;
            m_registrar = m_service.Register(0, 0, System.Net.Dns.GetHostName() + "CmdInstance", "_cmdListening._tcp", null, null, CurrentCmdPort, null, m_eventManager);
            m_registrar1 = m_service.Register(0, 0, System.Net.Dns.GetHostName() + "DataInstance", "_dataListening._tcp", null, null, CurrentDataPort, null, m_eventManager);
        }

        public void RegisterCmdService()
        {            
            m_registrar = m_service.Register(0, 0, System.Net.Dns.GetHostName() + "CmdInstance", "_cmdListening._tcp", null, null, CurrentCmdPort, null, m_eventManager);            
        }

        public void RegisterDataService()
        {
            m_registrar = m_service.Register(0, 0, System.Net.Dns.GetHostName() + "DataInstance", "_dataListening._tcp", null, null, CurrentDataPort, null, m_eventManager);
        }

        public void Stop()
        {
            m_service.Stop();
            m_service = null;
            m_registrar.Stop();
            m_registrar = null;
        }

        public void ServiceRegistered(Bonjour.DNSSDService srvc, Bonjour.DNSSDFlags flags, string s1, string s2, string s3)
        {
            serviceNum++;
            if (serviceNum.Equals(2))
            {
                OnServicesRegistered();
            }
            Console.WriteLine("The service {0} of type {1} successfully registered with the mDNS daemon", s1, s2);
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
