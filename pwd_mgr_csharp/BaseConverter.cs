using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    // https://stackoverflow.com/questions/33729397/how-to-convert-a-floating-point-number-to-base36-in-c-sharp-in-the-same-way-as-j
    public class BaseConverter
    {
        private readonly string Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ+/"; //these are the characters in base 64 encoding
        private string ExtraChars = "!@#$%^&*?.-_="; //feel free to add more to this
        // private int nBase = 76;
       

        private string Encode(int nIn, int nBase = 77)
        {
            string total = Chars + ExtraChars;
            if (nBase > total.Length)
            {
                throw new ArgumentException($"Encoding base exceeded {total.Length}.");
            }
            int n = nIn / nBase;
            char c = total[nIn % nBase];
            return n > 0 ? Encode(n, nBase) + c : c.ToString();
        }

        public string EncodeBytes(byte[] bytes, int nBase = 77)
        {
            string encoded = "";
            foreach (byte b in bytes)
            {
                encoded += Encode(b, nBase);
            }
            return encoded;
        }
    }
}
