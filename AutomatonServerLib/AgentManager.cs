using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using ACorns.WCF.DynamicClientProxy;
using AutomatonContract;
using AutomatonContract.Entities;
using AutomatonContract.Messages;
using AutomatonLib;

namespace AutomatonServerLib
{
    public class Agent
    {
        public string Name { get; set;}
        public List<Task> Tasks { get; private set;}
        public Agent()
        {
            Tasks = new List<Task>();
        }
    }

    class AgentProxy: Agent
    {
        public IAgentService Service { get; set; }
    }


    public class AgentManager
    {
        public static AgentManager Current { get; set; }

        private readonly object _lock = new object();
        private readonly Thread _locatorThread;
        private readonly Thread _pingThread;
        private readonly List<AgentProxy> _agents = new List<AgentProxy>();

        public AgentManager()
        {
            _locatorThread = new Thread(AnswerAgentNotifications);
            _pingThread = new Thread(PingAgents);
        }

        public static T Build<T>() where T : AuthenticatedRequestBase, new()
        {
            var request = new T { Timestamp = Cryptography.GenerateTimestamp(DateTime.Now) };
            request.HMAC = Cryptography.GenerateHMac(Config.Config.Current.ServicePassword, request.Timestamp);
            request.UserName = Config.Config.Current.ServiceUserName;
            return request;
        }

        public List<Agent> GetAgents()
        {
            var agents = new List<Agent>();
            lock (_lock)
            {
                foreach (var agent in _agents)
                {
                    var copy = new Agent { Name = agent.Name};
                    copy.Tasks.AddRange(agent.Tasks);
                    agents.Add(copy);
                }
            }
            return agents;
        }

        public void RunTask(Guid id)
        {
            IAgentService service = null;
            lock (_lock)
            {
                foreach (var agent in _agents)
                {
                    var task = agent.Tasks.SingleOrDefault(t => t.Id == id);
                    if (task != null)
                    {
                        service = agent.Service;
                        break;
                    }
                }
            }

            if (service != null)
            {
                try 
                {
                    var request = Build<RunTaskRequest>();
                    request.TaskId = id;
                    var response = service.RunTask(request);
                    if (response.Errors.Count>0)
                    {
                        Logger.Current.Write(LogInfoLevel.Error, "Error Running task '" + id + "' on agent - "+response.Errors[0]);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Write(ex, "Error Running task '"+id+"' on agent");
                }
            }
        }

        private void PingAgents()
        {
            while (true)
            {
                var agents = new List<AgentProxy>();
                lock (_lock)
                {
                    agents.AddRange(_agents);
                }

                foreach (var agent in agents)
                {
                    bool remove = false;
                    try
                    {
                        var response = agent.Service.Ping(Build<PingRequest>());
                        if (response.Errors.Count != 0)
                        {
                            Logger.Current.Write(LogInfoLevel.Error, "Error Pinging agent, removing from active agents list");
                            remove = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Write(ex, "Error Pinging agent, removing from active agents list");
                        remove = true;
                    }

                    if (remove)
                    {
                        lock (_lock)
                        {
                            _agents.Remove(agent);
                        }
                    }
                }
                Thread.Sleep(30000);
            }
        }

        private void AnswerAgentNotifications()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(IPAddress.Any, Config.Config.Current.ServerLocatorRequestPort));
            socket.ReceiveTimeout = 5000;
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    EndPoint remoteClient = new IPEndPoint(IPAddress.Any, Config.Config.Current.ServerLocatorRequestPort);

                    try
                    {
                        int messageSize = socket.ReceiveFrom(buffer, ref remoteClient);

                        string requestMessage = Encoding.ASCII.GetString(buffer, 0, messageSize);
                        if (requestMessage.StartsWith("AgentStarted|"))
                        {
                            string agentServiceUrl = requestMessage.Substring(requestMessage.IndexOf("|") + 1);

                            try
                            {
                                IAgentService agentService = WCFClientProxy<IAgentService>.GetReusableFaultUnwrappingInstance(new BasicHttpBinding(), new EndpointAddress(agentServiceUrl));
                                var response = agentService.QueryTasks(Build<QueryTasksRequest>());
                                if (response.Errors.Count == 0)
                                {
                                    lock (_lock)
                                    {
                                        if (_agents.Exists(a=>a.Name==response.AgentName))
                                        {
                                            _agents.RemoveAll(a => a.Name == response.AgentName);
                                        }

                                        var newAgent = new AgentProxy{Name = response.AgentName,Service = agentService };
                                        foreach (var task in response.Tasks)
                                        {
                                            newAgent.Tasks.Add(task);
                                        }
                                        _agents.Add(newAgent);
                                    }
                                }
                                else
                                {
                                    Logger.Current.Write(LogInfoLevel.Error, "Error Pinging agent at '" + agentServiceUrl + "' - " + response.Errors[0]);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Write(ex, "Error Pinging agent at '" + agentServiceUrl + "'");
                            }

                            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            clientSocket.SendTo(Encoding.ASCII.GetBytes("NotificationReceived"), new IPEndPoint(((IPEndPoint)remoteClient).Address, Config.Config.Current.ServerLocatorResponsePort));
                            clientSocket.Close();
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.TimedOut)
                        {
                            continue;
                        }
                        else
                        {
                            Logger.Current.Write(ex, "Error in Agent Notifications Thread");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Write(ex, "Error in ServerLocator Thread");                        
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Logger.Current.Write(ex, "Error in ServerLocator Thread");
            }
            finally
            {
                socket.Close();
            }
        }

        public void Start()
        {
            _locatorThread.Start();
            _pingThread.Start();
        }

        public void Stop()
        {
            _locatorThread.Abort();
            _locatorThread.Join();
            _pingThread.Abort();
            _pingThread.Join();
        }
    }
}
