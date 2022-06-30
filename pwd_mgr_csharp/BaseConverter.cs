using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    // non-base64 encoder class
    // allows for integers to be converted to a non-base64 string given a set of characters to encode to

    // https://stackoverflow.com/questions/33729397/how-to-convert-a-floating-point-number-to-base36-in-c-sharp-in-the-same-way-as-j
    public static class BaseConverter
    {
        private static string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static string Numbers = "0123456789";
        private static string Symbols = "!@#$%^&*_+-=?/.,";
        public static string EncodeBytes(byte[] bytes)
        {
            string encoded = "";
            foreach (byte b in bytes)
            {
                encoded += Encode(b);
            }
            return encoded;
        }
        private static string Encode(int value)
        {
            string chars = Letters + Numbers + Symbols;
            int nBase = chars.Length;
            string result = "";

            while (value > 0)
            {
                result = chars [value % nBase] + result;
                value /= nBase;
            }
            return result;
        }
    }
}
