namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentstaffexport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobStaffExport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UploadedFileName = c.String(),
                        SchoolStartYear = c.Int(nullable: false),
                        UploadedFileUrl = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        RecordsProcessed = c.Int(nullable: false),
                        BatchName = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JobStudentExport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UploadedFileName = c.String(),
                        SchoolStartYear = c.Int(nullable: false),
                        UploadedFileUrl = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        StaffEmail = c.String(),
                        RecordsProcessed = c.Int(nullable: false),
                        BatchName = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.JobAttendanceExport", "BatchName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobAttendanceExport", "BatchName");
            DropTable("dbo.JobStudentExport");
            DropTable("dbo.JobStaffExport");
        }
    }
}
