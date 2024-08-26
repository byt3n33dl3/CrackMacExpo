using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeScammer.API
{
    public static class Functions
    {
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];

            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];

            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);

            return new string(chars);
        }

        public static string GetString(byte[] bytes, int length)
        {
            char[] chars = new char[length / sizeof(char)];

            Buffer.BlockCopy(bytes, 0, chars, 0, length);

            return new string(chars);
        }
    }
}
