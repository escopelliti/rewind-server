using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDataStructure
{
    public class StringConst
    {

        /// <summary>
        /// CONFIGURATION
        /// </summary>
        public const String CONFIG_FILE = "config.json";
        public const String DEF_DIR = "resources/";
        public const String DEFAULT_PSW = "default";
        public const String DEFAULT_CONF = "Configurazione di default creata. La psw è \"default\"";
        public const String CONF_CHANGED_MEX = "Le modifiche saranno disponibili al riavvio dell'applicazione";

        /// <summary>
        /// OPERATION COMPLETED/SUCCESS/INFORMATION
        /// </summary>
        public const String OPERATION_COMPLETED = "Operazione completata";
        public const String INFORMATION = "Informazione";
        public const String CONNECTED = "Connesso";
        public const String CONNECTED_INFO = "Un computer si è appena connesso";
        public const String LISTENING = "In attesa...";
        public const String LISTENING_INFO = "In attesa di connessione";
        
        /// <summary>
        /// Problems/Errors
        /// </summary>
        public const String HOUSTON_PROBLEM = "Sembra esserci qualche problema, prova a riavviare l'applicazione";
        public const String HOUSTON_PROBLEM_TITLE = "Attenzione!";
        public const String DUPLICATED_PORT = "Le porte non possono avere lo stesso valore";
        public const String PSW_ERROR = "Inserisci una password per continuare!";

        /// <summary>
        /// Bonjour services
        /// </summary>
        public const String CMD_SERVICE = "_cmdListening._tcp";
        public const String CMD_SERVICE_INSTANCE = "CmdInstance";
        public const String DATA_SERVICE_INSTANCE = "DataInstance";
        public const String DATA_SERVICE = "_dataListening._tcp";
    }
}
