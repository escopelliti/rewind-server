using Bonjour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discovery
{



    public class ServiceRegister
    {

        private Bonjour.DNSSDService m_service = null;

        //A Handle for the registered record
        private Bonjour.DNSSDService m_registrar = null;

        private Bonjour.DNSSDEventManager m_eventManager = null;

        public ushort CurrentDataPort { get; set; }
        public ushort CurrentCmdPort { get; set; }

        public ServiceRegister(ushort dataPort, ushort cmdPort)
        {
            m_eventManager = new DNSSDEventManager();
            m_eventManager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(ServiceRegistered);
            m_service = new DNSSDService();
            this.CurrentDataPort = dataPort;
            this.CurrentCmdPort = cmdPort;
        }

        public void RegisterServices()
        {
            m_registrar = m_service.Register(0, 0, System.Net.Dns.GetHostName() + "DataInstance", "_dataListening._tcp", null, null, CurrentDataPort, null, m_eventManager);
            m_registrar = m_service.Register(0, 0, System.Net.Dns.GetHostName() + "CmdInstance", "_cmdListening._tcp", null, null, CurrentCmdPort, null, m_eventManager);            
        }

        public void Stop()
        {
            m_service.Stop();
            m_registrar.Stop();
        }

        public void ServiceRegistered(Bonjour.DNSSDService srvc, Bonjour.DNSSDFlags flags, string s1, string s2, string s3)
        {
            Console.WriteLine("The service {0} of type {1} successfully registered with the mDNS daemon", s1, s2);
        }
    }
}
