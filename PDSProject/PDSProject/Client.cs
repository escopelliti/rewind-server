using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace PDSProject
{
    public class Client
    {

        private Socket socket;
        //some other stuff about a client (its name?)

        public Socket GetSocket()
        {
            return socket;
        }

        public void SetSocket(Socket socket)
        {
            this.socket = socket;
        }
    }
}
