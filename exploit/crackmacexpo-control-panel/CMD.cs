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
    public partial class CMD : Form
    {
        public CMD()
        {
            InitializeComponent();
        }

        #region Public Functions
        public void AppeandLog(string text)
        {
            txtOutput.Text += "\n" + text; // Add the text
        }
        #endregion

        #region Form Functions
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("runcommand", new string[] { txtCommand.Text })); // Send the command
            txtCommand.Text = ""; // Clear the command
        }
        #endregion
    }
}
