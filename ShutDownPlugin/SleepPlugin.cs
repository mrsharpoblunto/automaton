using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AutomatonContract;

namespace ShutDownPlugin
{
    [AutomatonPlugin]
    class SleepPlugin: IAutomatonPlugin
    {
        [DllImport("powrprof.dll")]
        private static extern bool SetSuspendState(bool Hibernate, bool
        ForceCritical, bool DisableWakeEvent);

        public Guid Uid
        {
            get { return new Guid("036B620E-5A13-4f24-87AC-43E2B33F6E36"); }
        }

        public string Name
        {
            get { return "Send PC to sleep"; }
        }

        public string Description
        {
            get { return "Power off the PC and go to sleep"; }
        }

        public void Run()
        {
            Thread shutdownThread = new Thread(() =>
            {
                Thread.Sleep(5000);
                SetSuspendState(false, true, false);
            });
            shutdownThread.Start();
        }

        public void Load(List<string> parameters)
        {
        }
    }
}
