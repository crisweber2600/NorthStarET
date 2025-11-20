namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assessmentfieldupdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentField", "ObsSummaryLabel", c => c.String());
            AddColumn("dbo.AssessmentField", "DisplayInStackedBarGraphSummary", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssessmentField", "DisplayInStackedBarGraphSummary");
            DropColumn("dbo.AssessmentField", "ObsSummaryLabel");
        }
    }
}
