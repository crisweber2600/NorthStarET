namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statetestjob : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JobStateTestDataImport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        UploadedFileName = c.String(),
                        ImportLog = c.String(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        Status = c.String(),
                        StaffId = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.JobStateTestDataImport");
        }
    }
}
