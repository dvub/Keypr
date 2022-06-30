using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    
    public class Password
    {
        public int Id { get; set; } //Primary key for DB, although not really necessary
        public string Name { get; set; }
        public byte[] Username { get; set; }
        public byte[] Pass { get; set; }
        public byte[]? Notes { get; set; }
    }
}
