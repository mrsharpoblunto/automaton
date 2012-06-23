using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using AutomatonContract;

namespace ToggleServicePlugin
{
    [AutomatonPlugin]
    public class ToggleServicePlugin : IAutomatonPlugin
    {
        private ServiceController _controller;

        public string Name
        {
            get { return "Toggle Windows Service"; }
        }

        public string Description
        {
            get { return "Starts and stops windows services"; }
        }

        public void Run()
        {
            _controller.Refresh();
            if (_controller.Status==ServiceControllerStatus.Running)
            {
                _controller.Stop();
            }
            else if (_controller.Status==ServiceControllerStatus.Stopped)
            {
                _controller.Start();
            }
        }

        public void Load(List<string> parameters)
        {
            _controller = new ServiceController(parameters[0]);
        }
    }
}
