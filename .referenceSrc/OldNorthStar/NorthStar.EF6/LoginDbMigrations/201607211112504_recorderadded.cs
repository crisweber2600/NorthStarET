namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recorderadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobBenchmarkDataImport", "RecorderId", c => c.Int(nullable: false));
            AddColumn("dbo.JobInterventionDataImport", "RecorderId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobInterventionDataImport", "RecorderId");
            DropColumn("dbo.JobBenchmarkDataImport", "RecorderId");
        }
    }
}
