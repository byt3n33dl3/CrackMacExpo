using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("sendkey", 1)]
    public class SendKey : Command
    {
        public override void Execute(string[] args)
        {
            SendKeys.SendWait(args[0]);
        }
    }
}
