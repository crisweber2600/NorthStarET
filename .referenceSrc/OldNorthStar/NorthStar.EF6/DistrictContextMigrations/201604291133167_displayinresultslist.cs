namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class displayinresultslist : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentField", "DisplayInLineGraphSummaryTable", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssessmentField", "DisplayInLineGraphSummaryTable");
        }
    }
}
