using AsusFanControl.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsusFanControl.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppSetting> Setting { get; set; }
        public DbSet<Graph> Graph { get; set; }
        public DbSet<GraphPoint> GraphPoints { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbFilePath = Path.Combine(appDataFolder, "Asus Fan Control", "AsusControl.db");

            string dbFolder = Path.GetDirectoryName(dbFilePath);
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            string connectionString = $"Data Source={dbFilePath}";
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
