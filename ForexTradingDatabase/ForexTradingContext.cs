using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForexTradingDatabase.Migrations;

namespace ForexTradingDatabase
{
    public class ForexTradingContext : DbContext
    {
        public ForexTradingContext() : base("name=ForexTradingContext")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ForexTradingContext,Configuration>());
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<TradingPair> TraidingPairs { get; set; }
        public virtual DbSet<TraidingPairData> TraidingPairDatas { get; set; }
    }
}
