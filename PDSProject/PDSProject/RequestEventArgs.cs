using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSProject
{
    public class RequestEventArgs : EventArgs
    {
        public RequestState requestState { get; set; }
        
        public RequestEventArgs(RequestState reqState) {
            this.requestState = reqState;
        }

    }
}
