using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("openmessagebox", 2)]
    public class OpenMessageBox : Command
    {
        public override void Execute(string[] args)
        {
            Thread t = new Thread(new ThreadStart(delegate () { MessageBox.Show(args[1], args[0]); })); // Create the thread
            t.Start();
        }
    }
}
