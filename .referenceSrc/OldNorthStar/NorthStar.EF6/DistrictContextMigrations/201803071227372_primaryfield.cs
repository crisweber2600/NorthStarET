namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class primaryfield : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentField", "IsPrimaryFieldForAssessment", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssessmentField", "IsPrimaryFieldForAssessment");
        }
    }
}
