using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace ShutDownPlugin
{
    class ShutDownUtils
    {
        public static void ExitWindows(uint uFlags, uint dwReason)
        {
            ManagementBaseObject outParams = null;
            ManagementClass os = new ManagementClass("Win32_OperatingSystem");
            os.Get();
            os.Scope.Options.EnablePrivileges = true; // enables required security privilege.
            ManagementBaseObject inParams = os.GetMethodParameters("Win32Shutdown");
            inParams["Flags"] = uFlags.ToString();
            inParams["Reserved"] = "0";
            foreach (ManagementObject mo in os.GetInstances())
            {
                outParams = mo.InvokeMethod("Win32Shutdown",inParams, null);
            }
        }
    }
}
