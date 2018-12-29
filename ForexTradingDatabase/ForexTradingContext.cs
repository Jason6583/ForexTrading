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
            var databaseLocation = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent?.Parent?.Parent.Parent.FullName + "\\Database\\ForexTradingDb.mdf";
            Database.Connection.ConnectionString =
                "Data Source=(LocalDB)\\MSSQLLocalDB;" +
                $"AttachDbFilename={databaseLocation};" +
                "Integrated Security=True;"+
                "MultipleActiveResultSets=true";

            var pom = (from x in Assets select x).ToList();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ForexTradingContext, Configuration>());
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
