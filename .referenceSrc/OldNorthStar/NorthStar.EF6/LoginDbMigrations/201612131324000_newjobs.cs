namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newjobs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobStudentRollover",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UploadedFileName = c.String(),
                        UploadedFileUrl = c.String(),
                        ImportLog = c.String(),
                        PotentialIssuesLog = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        RecordsProcessed = c.Int(nullable: false),
                        RecordsSkipped = c.Int(nullable: false),
                        BatchName = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobTeacherRollover",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UploadedFileName = c.String(),
                        UploadedFileUrl = c.String(),
                        ImportLog = c.String(),
                        PotentialIssuesLog = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        RecordsProcessed = c.Int(nullable: false),
                        RecordsSkipped = c.Int(nullable: false),
                        BatchName = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JobTeacherRollover");
            DropTable("dbo.JobStudentRollover");
        }
    }
}
