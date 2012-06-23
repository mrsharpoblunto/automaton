using System.Runtime.Serialization;

namespace AutomatonContract.Messages
{
    [DataContract]
    public class PingRequest:AuthenticatedRequestBase
    {
    }
}