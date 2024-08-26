using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeScammer.API;

namespace DeScammer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Variables.Program = new Program(); // Create a new instance of the program
            Variables.Program.EntryPoint(); // Run the entry point
        }

        public void EntryPoint()
        {
            CommandManager.Load(); // Load the command manager

            Console.Title = "DeScammer";
            Console.WriteLine("Welcome to DeScammer! The best scammer troller around!");
            Console.WriteLine();
            Console.WriteLine("We are starting the server for you!");
            Console.WriteLine("Please enter the IP of this virtual machine into your control panel!");
            WaitForServer(); // Wait for the server to run

            Console.Clear();
            Console.WriteLine("You may press any key to activate control mode! This will allow you to control the program from it\'s control panel");
            Console.WriteLine("Thank you for using DeScammer for your scammer trolling needs :D");
            Console.ReadKey();
            Console.Clear();
            //Kernel.FreeConsole(); // Remove the console
            Start(); // Start the troll
        }

        #region Functions
        private void WaitForServer()
        {
            try
            {
                Variables.Server = Server.Create(); // Create the server

                while (!Variables.Server.IsClientConnected)
                    Variables.Server.Update(); // Start accepting client connections
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating the server! Please report the following error message!");
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private void Start()
        {
            try
            {
                while (Variables.Server != null)
                    Variables.Server.Update(); // Update the packets and stuff as long as the server is active
                Environment.Exit(0); // Close if the server crashes
            }
            catch (Exception ex)
            {
                Environment.Exit(0); // No errors
            }
        }
        #endregion
    }
}
