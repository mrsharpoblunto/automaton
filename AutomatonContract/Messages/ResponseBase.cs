using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AutomatonContract.Messages
{
    [DataContract]
    public class ResponseBase
    {
        private List<string> _errors = new List<string>();

        [DataMember]
        public List<string> Errors
        {
            get { return _errors; }
            set { _errors = value; }
        }
    }
}