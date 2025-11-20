namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fieldgroupcategory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AssessmentFieldGroupContainer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        AltOrder = c.Int(),
                        DisplayName = c.String(),
                        AltDisplayLabel = c.String(),
                        Description = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.AssessmentId);
            
            AddColumn("dbo.AssessmentFieldGroup", "AssessmentFieldGroupContainerId", c => c.Int());
            CreateIndex("dbo.AssessmentFieldGroup", "AssessmentFieldGroupContainerId");
            AddForeignKey("dbo.AssessmentFieldGroup", "AssessmentFieldGroupContainerId", "dbo.AssessmentFieldGroupContainer", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AssessmentFieldGroupContainer", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldGroup", "AssessmentFieldGroupContainerId", "dbo.AssessmentFieldGroupContainer");
            DropIndex("dbo.AssessmentFieldGroupContainer", new[] { "AssessmentId" });
            DropIndex("dbo.AssessmentFieldGroup", new[] { "AssessmentFieldGroupContainerId" });
            DropColumn("dbo.AssessmentFieldGroup", "AssessmentFieldGroupContainerId");
            DropTable("dbo.AssessmentFieldGroupContainer");
        }
    }
}
