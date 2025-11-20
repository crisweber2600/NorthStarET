namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class interventions4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PrintSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(),
                        PrintLandscape = c.Boolean(),
                        PrintMultiPage = c.Boolean(),
                        FitHeight = c.Boolean(),
                        FitWidth = c.Boolean(),
                        StretchToFit = c.Boolean(),
                        HtmlViewerWidth = c.Int(),
                        HtmlViewerHeight = c.Int(),
                        ForcePortraitPageSize = c.Boolean(),
                        ConversionDelay = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PrintSetting");
        }
    }
}
