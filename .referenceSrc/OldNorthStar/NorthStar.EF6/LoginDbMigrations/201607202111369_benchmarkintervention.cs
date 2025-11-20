namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class benchmarkintervention : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobBenchmarkDataImport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        UploadedFileName = c.String(),
                        SchoolStartYear = c.Int(nullable: false),
                        UploadedFileUrl = c.String(),
                        ImportLog = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        BenchmarkDateId = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobInterventionDataImport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        UploadedFileName = c.String(),
                        SchoolStartYear = c.Int(nullable: false),
                        UploadedFileUrl = c.String(),
                        ImportLog = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        InterventionGroupId = c.Int(nullable: false),
                        StintId = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JobInterventionDataImport");
            DropTable("dbo.JobBenchmarkDataImport");
        }
    }
}
