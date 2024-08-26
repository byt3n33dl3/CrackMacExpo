using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("processstart", 1)]
    public class ProcessStart : Command
    {
        public override void Execute(string[] args)
        {
            System.Diagnostics.Process.Start(args[0]);
        }
    }
}
