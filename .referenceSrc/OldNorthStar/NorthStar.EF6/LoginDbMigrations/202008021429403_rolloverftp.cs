namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rolloverftp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AutomatedRolloverDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FtpSite = c.String(),
                        FtpUsername = c.String(),
                        FtpPassword = c.String(),
                        RelativeUri = c.String(),
                        RolloverEmail = c.String(),
                        AdminEmail = c.String(),
                        ForceLoad = c.Boolean(nullable: false),
                        DistrictId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AutomatedRolloverDetail");
        }
    }
}
