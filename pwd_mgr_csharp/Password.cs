using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
   public class Password
    {
        public string name { get; set; }
        public byte[] user { get; set; }
        public byte[] pwd { get; set; }
        public Password(string name, byte[] user, byte[] pwd)
        {
            this.name = name;
            this.user = user;
            this.pwd = pwd;
        }
        public Password()
        {

        }
    }

}
