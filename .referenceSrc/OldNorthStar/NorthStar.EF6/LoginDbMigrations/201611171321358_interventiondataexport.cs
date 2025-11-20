namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class interventiondataexport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobInterventionDataExport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SerializedRequest = c.String(),
                        UploadedFileName = c.String(),
                        SchoolStartYear = c.Int(nullable: false),
                        UploadedFileUrl = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        BenchmarkDateId = c.Int(nullable: false),
                        RecordsProcessed = c.Int(nullable: false),
                        BatchName = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.JobAssessmentDataExport", "BatchName", c => c.String());
            AddColumn("dbo.JobBenchmarkDataImport", "BatchName", c => c.String());
            AddColumn("dbo.JobFullRollover", "BatchName", c => c.String());
            AddColumn("dbo.JobInterventionDataImport", "BatchName", c => c.String());
            AddColumn("dbo.JobStateTestDataImport", "BatchName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStateTestDataImport", "BatchName");
            DropColumn("dbo.JobInterventionDataImport", "BatchName");
            DropColumn("dbo.JobFullRollover", "BatchName");
            DropColumn("dbo.JobBenchmarkDataImport", "BatchName");
            DropColumn("dbo.JobAssessmentDataExport", "BatchName");
            DropTable("dbo.JobInterventionDataExport");
        }
    }
}
