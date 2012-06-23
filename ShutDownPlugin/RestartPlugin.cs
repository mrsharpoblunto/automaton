using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AutomatonContract;

namespace ShutDownPlugin
{
    [AutomatonPlugin]
    public class RestartPlugin : IAutomatonPlugin
    {
        public Guid Uid
        {
            get { return new Guid("30674AE9-9AAE-4054-82F6-28D43B669302"); }
        }

        public string Name
        {
            get { return "Restart PC"; }
        }

        public string Description
        {
            get { return "Restart the PC"; }
        }

        public void Run()
        {
            Thread shutdownThread = new Thread(() =>
                                                   {
                                                       Thread.Sleep(5000);
                                                       ShutDownUtils.ExitWindows(2, 0);
                                                   });
            shutdownThread.Start();
        }

        public void Load(List<string> parameters)
        {
        }
    }
}
