using System;
using System.Threading;
using System.Windows.Forms;

namespace RAT
{
    static class Program
    {
        [STAThread]
        static void Main() 
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "RAT_Application", out createdNew))
            {
                if (createdNew)
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Create and show the message form
                    using (var startupForm = new StartUp())
                    {
                        Application.Run(startupForm);
                    }

                    // Start the main form
                    Application.Run(new Main());
                }
                else
                {
                    MessageBox.Show("Another instance of my application is already running. Be patient!","Hacker Doll", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
