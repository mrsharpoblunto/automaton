using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows.Forms;
using AutomatonAgent.Config;
using AutomatonAgent.Properties;
using AutomatonLib;
using Resources=AutomatonLib.Resources;

namespace AutomatonAgent
{
    static class Program
    {
        private static NotifyIcon _appIcon;
        private static ServiceHost _serviceHost;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.Current = new Logger(Path.Combine(Resources.UserDirectory, "agentLog.txt"));
            TaskFactory.Current = new TaskFactory(Path.Combine(Resources.ApplicationDirectory, "plugins"));

            InitTasks();

            _serviceHost = new ServiceHost(typeof (AgentService));
            _serviceHost.Open();
            ServerLocator.Start(_serviceHost);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CreateSystemTrayMenu();
            Application.Run();
        }

        private static void InitTasks()
        {
            List<Task> tasks = new List<Task>();
            foreach (var t in Config.Config.Current.Tasks)
            {
                var task = TaskFactory.Current.CreateTask(t);
                if (task != null)
                {
                    tasks.Add(task);
                }
            }

            Tasks.Current = new Tasks(tasks);
        }

        private static void CreateSystemTrayMenu()
        {
            ContextMenu sysTrayMenu = new ContextMenu();

            foreach (var task in Tasks.Current.LoadedTasks)
            {
                MenuItem pluginMenuItem = new MenuItem(task.Name);
                Task task1 = task;
                pluginMenuItem.Click += (s, e) => task1.Plugin.Run();
                sysTrayMenu.MenuItems.Add(pluginMenuItem);
            }

            MenuItem splitterItem = new MenuItem("-");
            sysTrayMenu.MenuItems.Add(splitterItem);

            MenuItem exitMenuItem = new MenuItem("Exit");
            exitMenuItem.Click += Exit_Click;

            sysTrayMenu.MenuItems.Add(exitMenuItem);

            _appIcon = new NotifyIcon
            {
                Icon = Properties.Resources.trayIcon,
                Text = "Automaton Server",
                ContextMenu = sysTrayMenu,
                Visible = true
            };
        }

        static void Exit_Click(object sender, EventArgs e)
        {
            _appIcon.Dispose();
            _serviceHost.Close();
            ServerLocator.Stop();
            Application.Exit();
        }
    }
}
