using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AutomatonServerLib.Config
{
    public class SectionHandler : System.Configuration.IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            return new Config(section);
        }
    }
}