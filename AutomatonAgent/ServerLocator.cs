using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace AutomatonAgent
{
    class ServerLocator
    {
        private static Thread _serverLocatorThread;
        private static DateTime _lastRequest;

        public static void Stop()
        {
            _serverLocatorThread.Abort();
            _serverLocatorThread.Join();
        }

        public static void Start(ServiceHost agentService)
        {
            _serverLocatorThread = new Thread(DoNotifyAgentStarted);
            _serverLocatorThread.Start(agentService);
        }

        private static void DoNotifyAgentStarted(object arg)
        {
            ServiceHost agentService = (ServiceHost) arg;
            _lastRequest = DateTime.MinValue;

            while (true)
            {
                if (new TimeSpan(DateTime.Now.Ticks - _lastRequest.Ticks).TotalSeconds<=60)
                {
                    Thread.Sleep(60000);
                }
                //if its been more than a minute since the last message from the server we have probably been disconnected, so reregister.
                else
                {
                    while (true)
                    {
                        try
                        {
                            int serverLocatorServicePort = Config.Config.Current.ServerLocatorRequestPort;
                            int serverLocatorResponsePort = Config.Config.Current.ServerLocatorResponsePort;

                            Socket responseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                                                               ProtocolType.Udp);
                            responseSocket.ReceiveTimeout = 2000;
                            responseSocket.Bind(new IPEndPoint(IPAddress.Any, serverLocatorResponsePort));

                            Socket requestSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                            requestSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                            //tell the server that an agent has started up and tell the server what the agents service address is so
                            //the server can contact the agent in future
                            requestSocket.SendTo(
                                ASCIIEncoding.ASCII.GetBytes("AgentStarted|" + FixAgentServiceUri(agentService.BaseAddresses[0])),
                                new IPEndPoint(IPAddress.Broadcast, serverLocatorServicePort));
                            requestSocket.Close();

                            byte[] buffer = new byte[1024];
                            int responseSize = responseSocket.Receive(buffer, 0, 1024, SocketFlags.None);
                            responseSocket.Close();

                            //whether the server accepted or rejected the agents notification (should always accept)
                            if (Encoding.ASCII.GetString(buffer, 0, responseSize) == "NotificationReceived") break;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        private static string FixAgentServiceUri(Uri agentServiceUri)
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress localIP in localIPs)
            {
                if (!IPAddress.IsLoopback(localIP) && !localIP.IsIPv6LinkLocal)
                {
                    Uri url = new Uri(agentServiceUri.Scheme + "://" + localIP + ":" + agentServiceUri.Port + agentServiceUri.PathAndQuery);
                    return url.ToString();
                }
            }
            return agentServiceUri.ToString();
        }

        public static void ReceivedRequest()
        {
            _lastRequest = DateTime.Now;
        }
    }
}
