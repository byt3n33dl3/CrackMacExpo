using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("foregroundapp", 1)]
    public class ForegroundApp : Command
    {
        public override void Execute(string[] args)
        {
            foreach(Process p in Process.GetProcessesByName(args[0]))
                Kernel.SetForegroundWindow(p.MainWindowHandle);
        }
    }
}
