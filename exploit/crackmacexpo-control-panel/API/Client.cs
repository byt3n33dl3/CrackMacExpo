using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeScammerControlPanel.API
{
    public class Client
    {
        #region Properties
        public TcpClient TClient { get; private set; } // The TCP client
        public NetworkStream Stream { get; private set; } // The server network stream

        public Thread PacketThread { get; private set; } // Packet parsing thread

        public string IP { get; private set; } // The IP to the server
        public bool IsConnected // Returns if the client is connected to the server
        {
            get
            {
                return TClient.Connected;
            }
        }
        #endregion

        #region Handlers
        public delegate void MessageReceivedHandler(string message); // Create a message recieved handler
        public delegate void MessageSentHandler(string message, bool successful); // Create a message sent handler
        #endregion

        #region Events
        public static event MessageReceivedHandler OnMessageReceived; // Called when a message is received
        public static event MessageSentHandler OnMessageSent; // Called when a message is sent
        #endregion

        public Client(string IP)
        {
            this.IP = IP; // Set the IP

            OnMessageReceived += new MessageReceivedHandler(MessageParse); // Add the received event
            OnMessageSent += new MessageSentHandler(MessageSent); // Add the sent event

            TClient = new TcpClient(); // Create a new TCP client
            PacketThread = new Thread(new ThreadStart(PacketUpdate)); // Create a new thread for packet updates
        }

        #region Static Functions
        public static Client Create(string IP)
        {
            Client client = new Client(IP); // Create the client instance

            try
            {
                client.Connect(); // Connect to the client
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while attempting to connect!", "DeScammer Control Panel");
            }

            return client; // Return the client instance
        }
        #endregion

        #region Public Functions
        public void Connect()
        {
            TClient.Connect(IPAddress.Parse(IP), Variables.Port); // Connect to the server

            Stream = TClient.GetStream(); // Get the server stream

            PacketThread.Start(); // Start the packet update thread

            Variables.MainForm.IsClientConnected = true;
        }

        public void Disconnect()
        {
            TClient.Client.Disconnect(false); // Disconnect the client
            TClient.Close(); // Close the client

            Stream = null; // Remove the stream

            PacketThread.Abort(); // Abort the packet update thread

            Variables.MainForm.IsClientConnected = false;
        }

        public void Send(string message)
        {
            try
            {
                if (!IsConnected)
                    return;

                Stream.Write(Functions.GetBytes(message), 0, message.Length * sizeof(char)); // Write to stream
                Stream.Flush(); // Flush the stream

                Thread.Sleep(10); // Sleep

                OnMessageSent(message, true);
            }
            catch (Exception ex)
            {
                OnMessageSent(message, false);
            }
        }
        #endregion

        #region Thread Functions
        public void PacketUpdate()
        {
            while (IsConnected)
            {
                try
                {
                    byte[] receivedBytes = new byte[TClient.ReceiveBufferSize]; // Create the byte array
                    int bytesRead = Stream.Read(receivedBytes, 0, receivedBytes.Length); // Read the message

                    OnMessageReceived(Functions.GetString(receivedBytes, bytesRead)); // Activate the event
                }
                catch (Exception ex) // Can't be too safe
                {
                    // Meh do nothing
                }
            }
            Variables.MainForm.IsClientConnected = false;
        }
        #endregion

        #region Event Functions
        private void MessageParse(string message)
        {
            CommandManager.ParseCommand(message);
        }

        private void MessageSent(string message, bool success)
        {
            // Maybe add a retry later on?
        }
        #endregion
    }
}
