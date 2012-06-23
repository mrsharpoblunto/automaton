using System.Collections.Generic;
using System.Runtime.Serialization;
using AutomatonContract.Entities;

namespace AutomatonContract.Messages
{
    [DataContract]
    public class QueryTasksResponse: ResponseBase
    {
        private List<Task> _tasks = new List<Task>();

        [DataMember]
        public List<Task> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        [DataMember]
        public string AgentName { get; set; }
    }
}