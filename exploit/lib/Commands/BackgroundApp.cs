using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("backgroundapp", 1)]
    public class BackgroundApp : Command
    {
        public override void Execute(string[] args)
        {
            foreach (Process p in Process.GetProcessesByName(args[0]))
                Kernel.ShowWindow(p.MainWindowHandle, 2);
        }
    }
}
