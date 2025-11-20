namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class yearlassessments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.District_YearlyAssessmentBenchmark",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                        AssessmentField = c.String(),
                        DoesNotMeet = c.Decimal(precision: 18, scale: 2),
                        Approaches = c.Decimal(precision: 18, scale: 2),
                        Meets = c.Decimal(precision: 18, scale: 2),
                        Exceeds = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentID, cascadeDelete: true)
                .ForeignKey("dbo.Grade", t => t.GradeID, cascadeDelete: true)
                .Index(t => t.AssessmentID)
                .Index(t => t.GradeID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.District_YearlyAssessmentBenchmark", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.District_YearlyAssessmentBenchmark", "AssessmentID", "dbo.Assessment");
            DropIndex("dbo.District_YearlyAssessmentBenchmark", new[] { "GradeID" });
            DropIndex("dbo.District_YearlyAssessmentBenchmark", new[] { "AssessmentID" });
            DropTable("dbo.District_YearlyAssessmentBenchmark");
        }
    }
}
