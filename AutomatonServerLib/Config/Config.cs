using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AutomatonLib;

namespace AutomatonServerLib.Config
{
    public class Config: ConfigBase
    {
        private static Config _current;

        public static Config Current
        {
            get
            {
                if (_current == null)
                {
                    _current = (Config)System.Configuration.ConfigurationManager.GetSection("server");
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

        public Config(XmlNode node) : base(node)
        {
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

        public int ServicePort
        {
            get
            {
                return int.Parse(this["serviceport"]);
            }
        }

        private readonly Guid _sessionId = Guid.NewGuid();

        public Guid SessionId
        {
            get
            {
                return _sessionId;
            }
        }
    }
}