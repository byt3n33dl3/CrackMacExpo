using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeScammerControlPanel.API;

namespace DeScammerControlPanel
{
    public partial class Run : Form
    {
        public Run()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { textBox1.Text })); // Send the process start command
        }
    }
}
