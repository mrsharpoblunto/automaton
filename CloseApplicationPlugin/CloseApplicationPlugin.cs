using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AutomatonContract;

namespace CloseApplicationPlugin
{
    [AutomatonPlugin]
    public class CloseApplicationPlugin: IAutomatonPlugin
    {
        [DllImport("user32.dll")]
        public static extern Int32 GetForegroundWindow();

        [DllImport("user32", EntryPoint = "SendMessageA")]
        public static extern Int32 SendMessage(Int32 hwnd, Int32 wMsg, Int32 wParam, Int32 lParam);

        public Guid Uid
        {
            get { return new Guid("8EC2D314-EC2F-44b4-A645-2F81CFE6C8F0"); }
        }

        public string Name
        {
            get { return "Close current application"; }
        }

        public string Description
        {
            get { return "Closes the currently active application"; }
        }

        public void Run()
        {
            Int32 hWnd = GetForegroundWindow();
            SendMessage(hWnd, 0x0112, 0xF060, 0);
        }

        public void Load(List<string> parameters)
        {
        }
    }
}
