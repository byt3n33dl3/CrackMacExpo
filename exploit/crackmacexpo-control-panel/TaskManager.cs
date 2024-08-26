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
    public partial class TaskManager : Form
    {
        public TaskManager()
        {
            InitializeComponent();
        }

        #region Public Functions
        public void AddProcess(string ID, string name)
        {
            ListViewItem lvi = new ListViewItem(ID);

            lvi.SubItems.Add(name);

            listView1.Items.Add(lvi);
            listView1.Sort();
        }
        #endregion

        #region Private Functions
        private void RefreshProcesses()
        {
            listView1.Items.Clear(); // Clear the listview

            if(Variables.Client != null)
                Variables.Client.Send(CommandManager.FormatCommand("getprocesses", new string[0])); // Ask for the processes
        }
        #endregion

        #region Form Functions
        private void TaskManager_Load(object sender, EventArgs e)
        {
            listView1.ListViewItemSorter = new ListViewSorter();

            ((ListViewSorter)listView1.ListViewItemSorter).ByColumn = 1;
            listView1.Sorting = SortOrder.Descending;
            RefreshProcesses(); // Refresh the processes
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        private void killToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("killprocess", new string[] { listView1.SelectedItems[0].SubItems[1].Text })); // Kill the process
            RefreshProcesses();
        }

        private void sendToForegroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("foregroundapp", new string[] { listView1.SelectedItems[0].SubItems[1].Text })); // Send the process to foreground
        }

        private void sendToBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            if (Variables.Client == null)
                return;

            Variables.Client.Send(CommandManager.FormatCommand("backgroundapp", new string[] { listView1.SelectedItems[0].SubItems[1].Text })); // Send the process to background
        }
        #endregion
    }

    public class ListViewSorter : System.Collections.IComparer
    {
        // http://www.java2s.com/Code/CSharp/GUI-Windows-Form/SortaListViewbyAnyColumn.htm
        public int Compare(object o1, object o2)
        {
            if (!(o1 is ListViewItem))
                return (0);
            if (!(o2 is ListViewItem))
                return (0);

            ListViewItem lvi1 = (ListViewItem)o2;
            string str1 = lvi1.SubItems[ByColumn].Text;
            ListViewItem lvi2 = (ListViewItem)o1;
            string str2 = lvi2.SubItems[ByColumn].Text;

            int result;
            if (lvi1.ListView.Sorting == SortOrder.Ascending)
                result = String.Compare(str1, str2);
            else
                result = String.Compare(str2, str1);

            LastSort = ByColumn;

            return (result);
        }


        public int ByColumn
        {
            get { return Column; }
            set { Column = value; }
        }
        int Column = 0;

        public int LastSort
        {
            get { return LastColumn; }
            set { LastColumn = value; }
        }
        int LastColumn = 0;
    }

}
