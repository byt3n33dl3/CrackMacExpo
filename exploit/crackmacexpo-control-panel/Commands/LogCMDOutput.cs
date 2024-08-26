using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeScammerControlPanel.API;
using System.Windows.Forms;

namespace DeScammerControlPanel.Commands
{
    [Command("logcmdoutput", 1)]
    public class LogCMDOutput : Command
    {
        public override void Execute(string[] args)
        {
            Variables.CMDForm.AppeandLog(args[0]); // Appeand the log
        }
    }
}
