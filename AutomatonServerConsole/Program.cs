using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutomatonLib;
using AutomatonServerLib;
using AutomatonServerLib.Config;

namespace AutomatonServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Current = new Logger(Path.Combine(Resources.ApplicationDirectory, "serverLog.txt"));
            AgentManager.Current = new AgentManager();
            var server = new AutomatonHttpServer(Config.Current.ServicePort);

            AgentManager.Current.Start();
            server.Start();

            while (true)
            {
            }
        }
    }
}
