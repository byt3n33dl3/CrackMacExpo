using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeScammerControlPanel.API;

namespace DeScammerControlPanel.Commands
{
    [Command("listfiles", 2)]
    public class ListFiles : Command
    {
        public override void Execute(string[] args)
        {
            Variables.FileManagerForm.path = args[0]; // Set the path

            for(int i = 1; i < args.Length; i++)
                Variables.FileManagerForm.AddFile(args[i]);
        }
    }
}
