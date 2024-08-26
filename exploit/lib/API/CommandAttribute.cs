using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeScammer.API
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : Attribute
    {
        #region Properties
        public string Command { get; private set; } // The command
        public int ArgCount { get; private set; } // Minimum amount of arguments
        #endregion

        public CommandAttribute(string command, int argcount)
        {
            this.Command = command; // Set the command
            this.ArgCount = argcount; // Set the arg count
        }
    }
}
