using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeScammerControlPanel.API;

namespace DeScammerControlPanel
{
    static class Variables
    {
        public static Main MainForm = null;
        public static TaskManager TaskManagerForm = null;
        public static Run RunForm = null;
        public static OpenMessageBox OpenMessageBoxForm = null;
        public static FileManager FileManagerForm = null;
        public static CMD CMDForm = null;

        public static Client Client = null;

        public static readonly int Port = 634;
    }
}
