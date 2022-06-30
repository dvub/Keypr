using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    public class PwdsContext : DbContext
    {
        public DbSet<Password> Passwords { get; set; } //creating a DbSet with password class
        public string DbPath { get; }
        public PwdsContext() //constructor
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "data.db");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
