using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server
{
    public partial class Form1 : Form
    {
        public static Thread th = null;
        public static int n = 0;
        public Form1()
        {
            MaximizeBox = false;
            InitializeComponent();
            //th = new Thread(() => Load_Imgs());
            //th.Start();
            Task.Run(() => Load_Imgs());
        }
        private void Load_Imgs()
        {
            
            TcpListener listener = new TcpListener(IPAddress.Any, 8081);
            listener.Start();
            int s1 = DateTime.Now.Second;
            while (true)
            {
                int s2 = DateTime.Now.Second;
                if(s2 != s1)
                {
                    s1 = s2;
                    label1.Text = n.ToString() + " FPS";
                    n = 0;
                }
                n++;
                
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                view.Image = Bitmap.FromStream(ns);
                ns.Close();
                client.Close();
            }
        }

        private void view_Click(object sender, EventArgs e)
        {

        }

        private void Frm_Closing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
