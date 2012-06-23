using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using AutomatonContract;

namespace MediaCenterPlugin
{
    [AutomatonPlugin]
    public class XBMCPlugin: IAutomatonPlugin 
    {
        private string _initScreenArgs, _revertScreenArgs, _displaySwitchTool;
        private string _xbmcPath;
        private string _xbmcArgs;

        public Guid Uid
        {
            get { return new Guid("B698614C-F602-4e66-B695-A68D299338AF"); }
        }

        public string Name
        {
            get { return "Start XBMC Media Center"; }
        }

        public string Description
        {
            get { return "Starts XBMC media center and set the display to TV out, then reset the display when complete."; }
        }

        public void Run()
        {
            Process setScreen = new Process();
            setScreen.StartInfo.FileName = _displaySwitchTool;
            setScreen.StartInfo.Arguments = _initScreenArgs;
            setScreen.StartInfo.UseShellExecute = false;
            setScreen.Start();
            setScreen.WaitForExit();

            Process p = new Process();
            p.EnableRaisingEvents = true;
            p.StartInfo.FileName = _xbmcPath;
            p.StartInfo.Arguments = _xbmcArgs;
            p.Exited += (sender, e) =>
            {
                Process resetScreen = new Process();
                resetScreen.StartInfo.FileName = _displaySwitchTool;
                resetScreen.StartInfo.Arguments = _revertScreenArgs;
                resetScreen.StartInfo.UseShellExecute = false;
                resetScreen.Start();
                resetScreen.WaitForExit();
            };
            p.Start();
            SetForegroundWindow(p.Id);
        }

        [DllImport("user32.dll")]
        public static extern void SetForegroundWindow(Int32 handle);

        public void LoadSettings(string settingsFile)
        {
            using (FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(reader);

                    _xbmcArgs = document.DocumentElement.SelectSingleNode("xbmcArgs").InnerText;
                    _xbmcPath = document.DocumentElement.SelectSingleNode("xbmcPath").InnerText;
                    _initScreenArgs = document.DocumentElement.SelectSingleNode("initScreenArgs").InnerText;
                    _revertScreenArgs = document.DocumentElement.SelectSingleNode("revertScreenArgs").InnerText;
                    _displaySwitchTool = document.DocumentElement.SelectSingleNode("displaySwitchTool").InnerText;
                }
            }
        }
    }
}
