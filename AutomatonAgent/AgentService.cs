using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AutomatonContract;
using AutomatonContract.Messages;
using AutomatonLib;

namespace AutomatonAgent
{
    public class AgentService: IAgentService
    {
        public PingResponse Ping(PingRequest request)
        {
            return InvokeAuthenticatedMethod<PingRequest, PingResponse>(request, (r, e) => { return; });
        }

        public RunTaskResponse RunTask(RunTaskRequest request)
        {
            return InvokeAuthenticatedMethod<RunTaskRequest, RunTaskResponse>(request, (r, response) => 
            {
                var task = Tasks.Current.GetTask(request.TaskId);
                if (task!=null)
                {
                    task.Plugin.Run();
                }
                else
                {
                    response.Errors.Add("No task found with this Id");
                }
            });
        }

        public QueryTasksResponse QueryTasks(QueryTasksRequest request)
        {
            return InvokeAuthenticatedMethod<QueryTasksRequest, QueryTasksResponse>(request, (r, response) =>
            {
                response.AgentName = Config.Config.Current.Name;
                foreach (var task in Tasks.Current.LoadedTasks)
                {
                    var contractEntity = new AutomatonContract.Entities.Task
                    {
                        Id = task.Id, Name = task.Name, Description = task.Description
                    };
                    response.Tasks.Add(contractEntity);
                }
            });
        }

        private delegate void AuthenticatedMethodHandler<REQUEST, RESPONSE>(REQUEST request, RESPONSE response);

        private static RESPONSE InvokeAuthenticatedMethod<REQUEST, RESPONSE>(REQUEST request, AuthenticatedMethodHandler<REQUEST, RESPONSE> handler)
            where REQUEST : AuthenticatedRequestBase
            where RESPONSE : ResponseBase, new()
        {
            RESPONSE response = new RESPONSE();

            try
            {
                if (Authenticate(request.HMAC, request.UserName, request.Timestamp))
                {
                    ServerLocator.ReceivedRequest();
                    handler(request, response);
                }
                else
                {
                    response.Errors.Add("Incorrect Password/HMAC");
                }
            }
            catch (Exception ex)
            {
                response.Errors.Add("Unknown error");
                Logger.Current.Write(ex, "Error processing " + typeof(RESPONSE).Name);
            }

            return response;
        }

        private static bool Authenticate(string hmac, string userName,string messageTimestamp)
        {
            try
            {
                if (userName != Config.Config.Current.ServiceUserName) return false;

                string computedHMac = Cryptography.GenerateHMac(Config.Config.Current.ServicePassword, messageTimestamp);

                //does the hmac agree with the server generated version.
                if (computedHMac.Equals(hmac))
                {
                    DateTime timeStamp = DateTime.ParseExact(messageTimestamp, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();
                    DateTime now = DateTime.Now;

                    //is the timestamp within the last 15 minutes server time (prevents reuse of timestamps)
                    TimeSpan timeDiff = new TimeSpan(now.Ticks - timeStamp.Ticks);
                    if (Math.Abs(timeDiff.TotalMinutes) <= 15)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
