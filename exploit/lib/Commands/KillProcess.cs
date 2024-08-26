using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("killprocess", 1)]
    public class KillProcess : Command
    {
        public override void Execute(string[] args)
        {
            foreach(Process p in Process.GetProcessesByName(args[0]))
                p.Kill();

            /*List<string> processes = new List<string>();

            foreach (Process p in Process.GetProcesses())
            {
                if(p.ProcessName == args[0])
                {
                    p.Kill(); // Kill the process
                    continue; // Skip the rest of the code
                }

                processes.Add(p.Id.ToString()); // Add process id
                processes.Add(p.ProcessName); // Add process name
            }

            Variables.Server.Send(CommandManager.FormatCommand("listprocess", processes.ToArray())); // Send the processes*/
        }
    }
}
