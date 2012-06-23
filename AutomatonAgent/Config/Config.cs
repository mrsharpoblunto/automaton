using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AutomatonLib;

namespace AutomatonAgent.Config
{
    class TaskConfig
    {
        public TaskConfig()
        {
            Parameters = new List<string>();
        }

        public string PluginAssembly { get; set; }
        public string PluginType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Parameters { get; private set; }
    }

    class Config: ConfigBase
    {
        private static Config _current;

        public static Config Current
        {
            get
            {
                if (_current == null)
                {
                    _current = (Config)System.Configuration.ConfigurationManager.GetSection("agent");
                    if (_current == null)
                    {
                        throw new ApplicationException("Failed to get configuration from app.config.");
                    }
                }
                return _current;
            }
            set
            {
                _current = value;
            }
        }

        private readonly List<TaskConfig> _tasks = new List<TaskConfig>();

        public Config(XmlNode node) : base(node)
        {
            var tasks = Section.SelectNodes("task");
            foreach (XmlElement task in tasks)
            {
                TaskConfig r = new TaskConfig();
                r.Name = task.Attributes["name"].Value;
                r.Description = task.Attributes["description"].Value;
                r.PluginAssembly = task.Attributes["pluginassembly"].Value;
                r.PluginType = task.Attributes["plugintype"].Value;
                if (task.Attributes["parameters"] != null)
                {
                    r.Parameters.AddRange(task.Attributes["parameters"].Value.Split(new[]{';'}));
                }
                _tasks.Add(r);
            }
        }

        public int ServerLocatorRequestPort
        {
            get
            {
                return int.Parse(this["serverlocatorrequestport"]);
            }
        }

        public int ServerLocatorResponsePort
        {
            get
            {
                return int.Parse(this["serverlocatorresponseport"]);
            }
        }

        public string Name
        {
            get
            {
                return this["name"];
            }
        }

        public string ServiceUserName
        {
            get
            {
                return this["serviceusername"];
            }
        }


        public string ServicePassword
        {
            get
            {
                return this["servicepassword"];
            }
        }

        public List<TaskConfig> Tasks
        {
            get { return _tasks; }
        }
    }
}