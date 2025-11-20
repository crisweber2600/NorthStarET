namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assessmentname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobInterventionDataExport", "AssessmentName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobInterventionDataExport", "AssessmentName");
        }
    }
}
