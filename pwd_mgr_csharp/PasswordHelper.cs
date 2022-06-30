using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    public static class PasswordHelper
    {
        public static Password findPassword(string file, string name)
        {
            List<Password> pwds = readPwds(file);

            Password pwd = pwds.Find(x => x.Name == name);
            if (pwd == null)
            {
                return null;
            }
            else
            {
                return pwd;
            }
        }

        public static List<Password> readPwds(string file)
        {
            using (StreamReader r = new StreamReader(file))
            {
                string json = r.ReadToEnd();
                return JsonSerializer.Deserialize<List<Password>>(json);
            }
        }
        public static void writePwds(List<Password> allPwds, string file)
        {
            string json = JsonSerializer.Serialize(allPwds, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(file, json);
        }

    }
}
