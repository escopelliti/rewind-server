using System;
using ConnectionModule;

namespace GenericDataStructure
{
    public class RequestEventArgs : EventArgs
    {
        public RequestState requestState { get; set; }
        
        public RequestEventArgs(RequestState reqState) {
            this.requestState = reqState;
        }
    }
}
