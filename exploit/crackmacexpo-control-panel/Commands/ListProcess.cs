using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeScammerControlPanel.API;

namespace DeScammerControlPanel.Commands
{
    [Command("listprocess", 1)]
    public class ListProcess : Command
    {
        public override void Execute(string[] args)
        {
            for(int i = 0; i < args.Length; i = i + 2)
                Variables.TaskManagerForm.AddProcess(args[i], args[i + 1]);
        }
    }
}
