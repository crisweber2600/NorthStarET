namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class recordsskipped : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobBenchmarkDataImport", "RecordsProcessed", c => c.Int(nullable: false));
            AddColumn("dbo.JobBenchmarkDataImport", "RecordsSkipped", c => c.Int(nullable: false));
            AddColumn("dbo.JobFullRollover", "RecordsProcessed", c => c.Int(nullable: false));
            AddColumn("dbo.JobFullRollover", "RecordsSkipped", c => c.Int(nullable: false));
            AddColumn("dbo.JobInterventionDataImport", "RecordsProcessed", c => c.Int(nullable: false));
            AddColumn("dbo.JobInterventionDataImport", "RecordsSkipped", c => c.Int(nullable: false));
            AddColumn("dbo.JobStateTestDataImport", "RecordsProcessed", c => c.Int(nullable: false));
            AddColumn("dbo.JobStateTestDataImport", "RecordsSkipped", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStateTestDataImport", "RecordsSkipped");
            DropColumn("dbo.JobStateTestDataImport", "RecordsProcessed");
            DropColumn("dbo.JobInterventionDataImport", "RecordsSkipped");
            DropColumn("dbo.JobInterventionDataImport", "RecordsProcessed");
            DropColumn("dbo.JobFullRollover", "RecordsSkipped");
            DropColumn("dbo.JobFullRollover", "RecordsProcessed");
            DropColumn("dbo.JobBenchmarkDataImport", "RecordsSkipped");
            DropColumn("dbo.JobBenchmarkDataImport", "RecordsProcessed");
        }
    }
}
