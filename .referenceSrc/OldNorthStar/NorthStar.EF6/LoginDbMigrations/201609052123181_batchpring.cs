namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class batchpring : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobPrintBatch",
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
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JobPrintBatch");
        }
    }
}
