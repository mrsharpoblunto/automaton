using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AutomatonContract;

namespace ShutDownPlugin
{
    [AutomatonPlugin]
    public class ShutDownPlugin: IAutomatonPlugin
    {
        public Guid Uid
        {
            get { return new Guid("D7AAD4A4-77A1-4c08-80B9-E3AE206699B8"); }
        }

        public string Name
        {
            get { return "Shut Down PC"; }
        }

        public string Description
        {
            get { return "Shut down the PC"; }
        }

        public void Run()
        {
            Thread shutdownThread = new Thread(() =>
            {
                Thread.Sleep(5000);
                ShutDownUtils.ExitWindows(1, 0);
            });
            shutdownThread.Start();
        }

        public void Load(List<string> parameters)
        {
        }
    }
}
