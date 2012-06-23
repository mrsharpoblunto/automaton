using System;
using System.Runtime.Serialization;

namespace AutomatonContract.Messages
{
    [DataContract]
    public class RunTaskRequest: AuthenticatedRequestBase
    {
        [DataMember]
        public Guid TaskId { get; set; }

    }
}