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

        }

        public static ForexDbContext GetInstance()
        {
            var context = new ForexDbContext();

            context.EnsureDatabase();

            return context;
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
