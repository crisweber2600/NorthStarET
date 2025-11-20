namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class versioning : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NorthStarVersion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.StaffDistrict", "CurrentVersion", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.StaffDistrict", "VersionLastUpdated", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StaffDistrict", "VersionLastUpdated");
            DropColumn("dbo.StaffDistrict", "CurrentVersion");
            DropTable("dbo.NorthStarVersion");
        }
    }
}
