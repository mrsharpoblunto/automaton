using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AutomatonLib
{
    public static class Resources
    {
        public static string ApplicationDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
            }
        }

        public static string UserDirectory
        {
            get
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (!Directory.Exists(Path.Combine(localAppData, "Automaton")))
                {
                    Directory.CreateDirectory(Path.Combine(localAppData, "Automaton"));
                }
                return Path.Combine(localAppData, "Automaton");
            }
        }
    }
}
