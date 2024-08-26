using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeScammerControlPanel.API
{
    public static class CommandManager
    {
        #region Properties
        public static AppDomain CurrentDomain { get { return AppDomain.CurrentDomain; } } // Get the current app domain

        public static Dictionary<CommandAttribute, Command> Commands { get; private set; } // The command list

        public static bool IsLoaded { get; private set; } = false; // Is the command manager loaded
        #endregion

        #region Public Functions
        public static void Load()
        {
            Commands = new Dictionary<CommandAttribute, Command>(); // Create the commands list

            foreach (Assembly assembly in CurrentDomain.GetAssemblies()) // Loop all assemblies
            {
                foreach (Type _class in assembly.GetTypes()) // Loop all classes
                {
                    CommandAttribute attribute = (CommandAttribute)Attribute.GetCustomAttribute(_class, typeof(CommandAttribute)); // Get the command attribute

                    if (!_class.IsClass || !typeof(Command).IsAssignableFrom(_class) || attribute == null) // Check if the command is valid
                        continue;

                    Commands.Add(attribute, (Command)Activator.CreateInstance(_class)); // Add the command
                }
            }

            IsLoaded = true; // Set the isloaded
        }

        public static void ParseCommand(string message)
        {
            int commandSize = (int)message[0]; // Get the length of the command
            string command = message.Substring(1, commandSize); // Get the command text

            if (Commands.Keys.Count(a => a.Command == command) <= 0)
                return; // Command doesn't exist

            KeyValuePair<CommandAttribute, Command> info = Commands.First(a => a.Key.Command == command); // Get the command info
            string[] args = ParseArgs(message.Substring(1, message.Length - 1).Replace(command, "")); // Get the args

            try
            {
                info.Value.Execute(args);
            }
            catch (Exception ex) { }
        }

        public static string[] ParseArgs(string message)
        {
            List<string> args = new List<string>();

            if (string.IsNullOrEmpty(message))
                return new string[0]; // No arguments if string is empty

            while (message.Length > 0)
            {
                int len = (int)message[0]; // Get length of argument
                string arg = message.Substring(1, len); // Get the argument

                args.Add(arg);
                message = message.Substring(len + 1, message.Length - len - 1);
            }

            return args.ToArray();
        }

        public static string FormatCommand(string command, string[] args)
        {
            string result = "";

            result += (char)command.Length; // Get the length of the command and change to char
            result += command; // Add the command

            foreach (string arg in args) // Loop through args
            {
                result += (char)arg.Length; // Add the arg lenght and change to char
                result += arg; // Add the arg
            }

            return result;
        }
        #endregion
    }
}
