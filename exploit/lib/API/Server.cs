using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeScammer.API
{
    public class Server
    {
        #region Properties
        public TcpListener Listener { get; private set; } // The TCP listener
        public TcpClient Client { get; private set; } // The TCP client
        public NetworkStream Stream { get; private set; } // The client network stream

        public bool IsClientConnected // Returns if a client is connected or not
        {
            get
            {
                return (Client != null && Stream != null) && Client.Connected;
            }
        }
        #endregion

        #region Handlers
        public delegate void MessageSentHandler(string message, bool successful); // Create a message sent handler
        #endregion

        #region Events
        public static event MessageSentHandler OnMessageSent; // Called when a message is sent
        #endregion

        public Server()
        {
            OnMessageSent += new MessageSentHandler(MessageSent); // Add the message sent event

            Listener = new TcpListener(IPAddress.Any, Variables.Port); // Create the TCP Listener with the 634 port
        }

        #region Static Functions
        public static Server Create()
        {
            Server server = new Server(); // Create the new server instance

            server.Start(); // Start the server

            return server; // Return the server instance
        }
        #endregion

        #region Public Functions
        public void Start()
        {
            Listener.Start();
        }

        public void Stop()
        {
            Listener.Stop();
        }

        public void Send(string message)
        {
            try
            {
                if (!IsClientConnected)
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

        public void Update()
        {
            if (!IsClientConnected)
                Update_Client(); // If there is no client connected run the update client
            else
                Update_Packets(); // If a client is already connected wait for the packets
        }

        public void Update_Client()
        {
            if (IsClientConnected)
                return; // Make sure no other client is already connected

            Client = Listener.AcceptTcpClient(); // Accept the TCP request
            Stream = Client.GetStream(); // Get the network stream of the client
        }

        public void Update_Packets()
        {
            if (!IsClientConnected)
                return; // Make sure we actually have a client connected

            try
            {
                byte[] receivedBytes = new byte[Client.ReceiveBufferSize]; // Create the byte array
                int bytesRead = Stream.Read(receivedBytes, 0, receivedBytes.Length); // Read the message

                if (bytesRead <= 0)
                {
                    RemoveClient();
                    return;
                }

                ParseCommand(Functions.GetString(receivedBytes, bytesRead)); // Parse the command
            }
            catch (Exception ex) // Can't be too safe
            {
                // Meh do nothing
            }
        }
        #endregion

        #region Private Functions
        private void RemoveClient()
        {
            try
            {
                Client.Client.Disconnect(false); // Disconnect the socket
                Client.Close(); // Close the client
            }
            catch(Exception ex) { }
            Stream = null; // Remove the client stream
            Client = null; // Set the client to null
        }

        private void ParseCommand(string command)
        {
            CommandManager.ParseCommand(command);
        }
        #endregion

        #region Event Functions
        private void MessageSent(string message, bool success)
        {
            // Maybe add a retry later on?
        }
        #endregion
    }
}
