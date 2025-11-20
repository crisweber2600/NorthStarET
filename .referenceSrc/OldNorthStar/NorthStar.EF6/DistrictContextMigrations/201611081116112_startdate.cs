namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class startdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StudentInterventionGroup", "StartDate", c => c.DateTime(nullable: false));
            DropTable("dbo.PrintSetting");
        }
        
        public override void Down()
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
            
            AlterColumn("dbo.StudentInterventionGroup", "StartDate", c => c.DateTime());
        }
    }
}
