using Discord.Commands;
using NAudio.Wave;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WaveOutEvent = NAudio.Wave.WaveOutEvent;

public class PlayAudioCommand : ModuleBase<SocketCommandContext>
{
    private static readonly List<IWavePlayer> activeWavePlayers = new List<IWavePlayer>();

    [Command("playaudio")]
    public async Task PlayAudioCmd()
    {
        // Get the attachments from the message
        var attachments = Context.Message.Attachments;

        foreach (var attachment in attachments)
        {
            // Download the attached audio file
            string audioPath = Path.Combine(Path.GetTempPath(), attachment.Filename);
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(attachment.Url, audioPath);
            }

            // Play the audio file
            var wavePlayer = new WaveOutEvent();
            var audioFileReader = new AudioFileReader(audioPath);
            wavePlayer.Init(audioFileReader);
            wavePlayer.Play();
            activeWavePlayers.Add(wavePlayer);
            await ReplyAsync("Playing Audio");
        }
    }

    [Command("stopaudio")]
    public async Task StopAudioCmd()
    {
        // Stop all active wave players
        foreach (var wavePlayer in activeWavePlayers)
        {
            wavePlayer.Stop();
            wavePlayer.Dispose();
        }

        // Clear the list of active wave players
        activeWavePlayers.Clear();
        await ReplyAsync("Audio Player has been stopped");
        
    }
}
