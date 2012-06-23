using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using AutomatonLib;
using AutomatonServerLib;
using AutomatonServerLib.Config;

namespace AutomatonServer
{
    public partial class AutomatonServer : ServiceBase
    {
        private readonly AutomatonHttpServer _server;

        public AutomatonServer()
        {
            InitializeComponent();
            Logger.Current = new Logger(Path.Combine(Resources.ApplicationDirectory, "serverLog.txt"));
            AgentManager.Current = new AgentManager();
            _server = new AutomatonHttpServer(Config.Current.ServicePort);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Current.Write(LogInfoLevel.Error, "Unexpected error in Automaton Server - " + e.ExceptionObject);
            Environment.Exit(0);
        }

        protected override void OnStart(string[] args)
        {
            AgentManager.Current.Start();
            _server.Start();
        }

        protected override void OnStop()
        {
            AgentManager.Current.Stop();
            _server.Stop();
        }
    }
}
