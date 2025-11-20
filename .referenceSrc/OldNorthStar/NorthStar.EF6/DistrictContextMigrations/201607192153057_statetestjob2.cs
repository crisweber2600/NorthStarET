namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statetestjob2 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.JobStateTestDataImport");
        }
        
        public override void Down()
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
    }
}
