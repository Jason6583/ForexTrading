using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForexTradingDatabase.Migrations;

namespace ForexTradingDatabase
{
    public class ForexTradingContext : DbContext
    {
        public ForexTradingContext() : base()
        {
            //For other user they want to try this application with filled database
            var solutionDirectory = TryGetSolutionDirectoryInfo();

            if (solutionDirectory != null)
            {
                var databaseLocation = solutionDirectory.FullName + "\\Database\\ForexTradingDb.mdf";

                Database.Connection.ConnectionString =
                    "Data Source=(LocalDB)\\MSSQLLocalDB;" +
                    $"AttachDbFilename={databaseLocation};" +
                    "Integrated Security=True;" +
                    "MultipleActiveResultSets=true";

                var pom = (from x in Assets select x).ToList();
                Database.SetInitializer<ForexTradingContext>(null);
            }
        }
        public static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }

        public ForexTradingContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {

        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<TradingPair> TraidingPairs { get; set; }
        public virtual DbSet<TraidingPairData> TraidingPairDatas { get; set; }
        public virtual DbSet<PortFolioData> PortFolioDatas { get; set; }
    }
}
