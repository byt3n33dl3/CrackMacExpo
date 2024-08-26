using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RAT
{
    public partial class StartUp : Form
    {
        public StartUp()
        {
            InitializeComponent();
        }



        private void StartUp_Load(object sender, EventArgs e)
        {

        }



        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("aurakaleidoscope@gmail.com", "Hacker Doll", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
