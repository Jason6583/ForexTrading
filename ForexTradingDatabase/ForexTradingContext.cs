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
    /// <summary>
    /// Database context for forex trading
    /// </summary>
    public class ForexTradingContext : DbContext
    {
        /// <summary>
        /// Context constructor
        /// </summary>
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

            }

            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<ForexTradingContext, Configuration>());
        }
        /// <summary>
        /// Fiding actual solution directory
        /// </summary>
        /// <param name="currentPath"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Overloading constructor for context, if connection string is in App.config
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        public ForexTradingContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {

        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<TradingPair> TraidingPairs { get; set; }
        public virtual DbSet<TradingPairData> TraidingPairDatas { get; set; }
        public virtual DbSet<PortFolioData> PortFolioDatas { get; set; }
    }
}
