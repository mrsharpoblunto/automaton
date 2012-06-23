using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AutomatonAgent.Config;
using AutomatonContract;
using AutomatonLib;

namespace AutomatonAgent
{
    class TaskFactory
    {
        public static TaskFactory Current { get; set; }

        private readonly Dictionary<string, List<Type>> _pluginTypes = new Dictionary<string, List<Type>>();
        private string _pluginsDirectory;

        public TaskFactory(string pluginsDirectory)
        {
            _pluginsDirectory = pluginsDirectory;
            if (Directory.Exists(pluginsDirectory))
            {
                foreach (string f in Directory.GetFiles(pluginsDirectory))
                {
                    FileInfo fi = new FileInfo(f);
                    if (fi.Extension.ToLower().Equals(".dll"))
                    {
                        LoadPluginTypes(fi.FullName);
                    }
                }
            }
        }

        private void LoadPluginTypes(string pluginFile)
        {
            Assembly assembly;

            if (!File.Exists(pluginFile))
            {
                return;
            }
            assembly = Assembly.LoadFile(pluginFile);

            if (assembly != null)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract) continue;
                    object[] attrs = type.GetCustomAttributes(typeof(AutomatonPluginAttribute), false);
                    if (attrs.Length > 0)
                    {
                        if (!_pluginTypes.ContainsKey(assembly.GetName().Name))
                        {
                            _pluginTypes.Add(assembly.GetName().Name, new List<Type>());
                        }
                        _pluginTypes[assembly.GetName().Name].Add(type);
                    }
                }
            }
        }

        public Task CreateTask(TaskConfig config)
        {
            if (_pluginTypes.ContainsKey(config.PluginAssembly))
            {
                var type = _pluginTypes[config.PluginAssembly].SingleOrDefault(t => t.FullName == config.PluginType);
                if (type != null)
                {
                    Task task = new Task();
                    task.Name = config.Name;
                    task.Description = config.Description;
                    task.Id = Guid.NewGuid();
                    task.Plugin = Activator.CreateInstance(type) as IAutomatonPlugin;
                    task.Plugin.Load(config.Parameters);
                    return task;
                }
            }
            return null;
        }
    }
}