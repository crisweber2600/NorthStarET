namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class previousid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentFieldGroupContainer", "PreviousId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssessmentFieldGroupContainer", "PreviousId");
        }
    }
}
