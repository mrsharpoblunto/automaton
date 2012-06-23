using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatonContract
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false,Inherited = false)]
    public class AutomatonPluginAttribute: Attribute
    {
    }
}
