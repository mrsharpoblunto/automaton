using System.Runtime.Serialization;

namespace AutomatonContract.Messages
{
    [DataContract]
    public class AuthenticatedRequestBase
    {
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string HMAC { get; set; }

        [DataMember]
        public string Timestamp { get; set; }    
    }
}