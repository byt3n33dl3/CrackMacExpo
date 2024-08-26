using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("deletefile", 1)]
    public class DeleteFile : Command
    {
        public override void Execute(string[] args)
        {
            if (File.Exists(args[0]))
                File.Delete(args[0]);
            if (Directory.Exists(args[0]))
                Directory.Delete(args[0]);
        }
    }
}
