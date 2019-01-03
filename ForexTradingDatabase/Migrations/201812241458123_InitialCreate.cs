namespace ForexTradingDatabase.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assets",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.TraidingPairDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdOfPair = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TradingPairs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstAsset_Name = c.String(maxLength: 128),
                        SecondAsset_Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assets", t => t.FirstAsset_Name)
                .ForeignKey("dbo.Assets", t => t.SecondAsset_Name)
                .Index(t => t.FirstAsset_Name)
                .Index(t => t.SecondAsset_Name);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Email = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        SureName = c.String(),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Email);
            
            CreateTable(
                "dbo.PortFolioDatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdTradingPair = c.Int(nullable: false),
                        DateOfBuy = c.DateTime(),
                        DateOfSold = c.DateTime(),
                        User_Email = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Email)
                .Index(t => t.User_Email);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PortFolioDatas", "User_Email", "dbo.Users");
            DropForeignKey("dbo.TradingPairs", "SecondAsset_Name", "dbo.Assets");
            DropForeignKey("dbo.TradingPairs", "FirstAsset_Name", "dbo.Assets");
            DropIndex("dbo.PortFolioDatas", new[] { "User_Email" });
            DropIndex("dbo.TradingPairs", new[] { "SecondAsset_Name" });
            DropIndex("dbo.TradingPairs", new[] { "FirstAsset_Name" });
            DropTable("dbo.PortFolioDatas");
            DropTable("dbo.Users");
            DropTable("dbo.TradingPairs");
            DropTable("dbo.TraidingPairDatas");
            DropTable("dbo.Assets");
        }
    }
}
