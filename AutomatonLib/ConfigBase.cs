using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AutomatonLib
{
    public abstract class ConfigBase
    {
        private readonly XmlNode _section;

        public ConfigBase(XmlNode node)
        {
            _section = node;
        }

        public string this[string key]
        {
            get
            {
                XmlAttribute attr = _section.Attributes[key];
                if (attr != null)
                    return attr.Value;
                else
                    return null;
            }
        }

        public XmlNode Section
        {
            get { return _section; }
        }
    }
}
