using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer.Commands
{
    [Command("shutdown", 0)]
    public class Shutdown : Command
    {
        #region Override Functions
        public override void Execute(string[] args)
        {
            Environment.Exit(0);
        }
        #endregion
    }
}
