using System;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace Forex.Models
{
    internal class ForexDbContext : DbContext
    {
        public DbSet<RateItem> RateItems { get; set; }
        public DbSet<RateSummary> RateSummaries { get; set; }

        private ForexDbContext()
            : base(new SQLiteConnection
            {
                ConnectionString = new SQLiteConnectionStringBuilder
                {
                    DataSource = Path.Combine(Configs.DATA_ROOT, "cache.dat"),
                    ForeignKeys = true
                }.ConnectionString
            }, true)
        {
            // Turn off the Migrations, (NOT a code first Db)
            //Database.SetInitializer(new CreateDatabaseIfNotExists<CashierDbContext>());
        }

        public static ForexDbContext GetInstance()
        {
            var context = new ForexDbContext();

            context.EnsureDatabase();

            return context;
        }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Database does not pluralize table names
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        private void EnsureDatabase()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Forex.Scripts.Schema.sql"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();

                    Database.ExecuteSqlCommand(result);
                    SaveChanges();
                }
            }
        }
    }
}
