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
    public class MediaCenterPlugin:IAutomatonPlugin
    {
        private string _initScreenArgs, _revertScreenArgs,_displaySwitchTool;

        public Guid Uid
        {
            get { return new Guid("F1E9CC70-7D67-4185-B9FB-553D22FC2692"); }
        }

        public string Name
        {
            get { return "Start Windows Media Center"; }
        }

        public string Description
        {
            get { return "Starts windows media center and set the display to TV out, then reset the display when complete."; }
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
            p.StartInfo.FileName = "c:\\windows\\ehome\\ehshell.exe";
            p.StartInfo.WorkingDirectory = "c:\\windows\\ehome";
            p.Exited += (sender,e)=>
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
            using (FileStream fs = new FileStream(settingsFile,FileMode.Open,FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs) )
                {
                    XmlDocument document = new XmlDocument();
                    document.Load(reader);

                    _initScreenArgs = document.DocumentElement.SelectSingleNode("initScreenArgs").InnerText;
                    _revertScreenArgs = document.DocumentElement.SelectSingleNode("revertScreenArgs").InnerText;
                    _displaySwitchTool = document.DocumentElement.SelectSingleNode("displaySwitchTool").InnerText;
                }
            }
        }
    }
}
