using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ForceApiCall
{
    class WorkplaceContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string userdatajson = File.ReadAllText("Userdata.json");
            dynamic data = JsonConvert.DeserializeObject(userdatajson);
            string connectionString = data.ConnString;
            var serverVersion = new MySqlServerVersion(new Version(5, 7, 20));
            optionsBuilder.UseMySql(connectionString, serverVersion);
        }

        public DbSet<Workplace> Workplaces { get; set; }
        public DbSet<SetupData> Setups { get; set; }
    }
}
