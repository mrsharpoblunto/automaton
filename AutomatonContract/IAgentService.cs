using System.Linq;
using System.ServiceModel;
using System.Text;
using AutomatonContract.Messages;

namespace AutomatonContract
{
    [ServiceContract(Name = "AgentService", Namespace = "http://schemas.sharpoblunto.com/2010")]
    public interface IAgentService
    {
        [OperationContract]
        PingResponse Ping(PingRequest request);

        [OperationContract]
        RunTaskResponse RunTask(RunTaskRequest request);

        [OperationContract]
        QueryTasksResponse QueryTasks(QueryTasksRequest request);
    }
}
