namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixcascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentFieldId", "dbo.AssessmentField");
            AddForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentFieldId", "dbo.AssessmentField", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentFieldId", "dbo.AssessmentField");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentId", "dbo.Assessment");
            AddForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentFieldId", "dbo.AssessmentField", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
        }
    }
}
