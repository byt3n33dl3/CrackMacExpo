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
    public partial class OpenMessageBox : Form
    {
        public OpenMessageBox()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSM_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("openmessagebox", new string[] { txtTitle.Text, txtMessage.Text })); // Send the message
        }
    }
}
