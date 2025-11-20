namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class interventions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.District",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        State = c.String(),
                        Enabled = c.Boolean(nullable: false),
                        AccessLevel = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSGrade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        LongName = c.String(),
                        GradeOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSInterventionCardinality",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CardinalityName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSIntervention",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionType = c.String(),
                        bDisplay = c.Boolean(nullable: false),
                        Description = c.String(),
                        DefaultTextLevelType = c.Int(),
                        InterventionCardinalityId = c.Int(),
                        ExitCriteria = c.String(),
                        EntranceCriteria = c.String(),
                        LearnerNeed = c.String(),
                        DetailedDescription = c.String(),
                        TimeOfYear = c.String(),
                        InterventionTierId = c.Int(),
                        CategoryID = c.Int(),
                        BriefDescription = c.String(),
                        FrameworkId = c.Int(),
                        UnitOfStudyId = c.Int(),
                        WorkshopId = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                        InterventionCategory_Id = c.Int(),
                        InterventionFramework_Id = c.Int(),
                        InterventionUnitOfStudy_Id = c.Int(),
                        InterventionWorkshop_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSInterventionCardinality", t => t.InterventionCardinalityId)
                .ForeignKey("dbo.NSInterventionCategory", t => t.InterventionCategory_Id)
                .ForeignKey("dbo.NSInterventionFramework", t => t.InterventionFramework_Id)
                .ForeignKey("dbo.NSInterventionTier", t => t.InterventionTierId)
                .ForeignKey("dbo.NSInterventionUnitOfStudy", t => t.InterventionUnitOfStudy_Id)
                .ForeignKey("dbo.NSInterventionWorkshop", t => t.InterventionWorkshop_Id)
                .Index(t => t.InterventionCardinalityId)
                .Index(t => t.InterventionTierId)
                .Index(t => t.InterventionCategory_Id)
                .Index(t => t.InterventionFramework_Id)
                .Index(t => t.InterventionUnitOfStudy_Id)
                .Index(t => t.InterventionWorkshop_Id);
            
            CreateTable(
                "dbo.NSInterventionCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(),
                        CategoryDescription = c.String(),
                        SortOrder = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSInterventionFramework",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FrameworkName = c.String(),
                        FreameworkDescription = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSInterventionGrade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                        InterventionType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSGrade", t => t.GradeID, cascadeDelete: true)
                .ForeignKey("dbo.NSIntervention", t => t.InterventionType_Id)
                .Index(t => t.GradeID)
                .Index(t => t.InterventionType_Id);
            
            CreateTable(
                "dbo.NSInterventionTier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TierValue = c.Int(nullable: false),
                        Description = c.String(),
                        TierName = c.String(),
                        TierLabel = c.String(),
                        TierColor = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSInterventionToolIntervention",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionId = c.Int(nullable: false),
                        InterventionToolId = c.Int(nullable: false),
                        SortOrder = c.Int(),
                        InterventionType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSInterventionTool", t => t.InterventionToolId, cascadeDelete: true)
                .ForeignKey("dbo.NSIntervention", t => t.InterventionType_Id)
                .Index(t => t.InterventionToolId)
                .Index(t => t.InterventionType_Id);
            
            CreateTable(
                "dbo.NSInterventionTool",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ToolName = c.String(),
                        ToolFileName = c.String(),
                        Description = c.String(),
                        SortOrder = c.Int(),
                        FileSystemFileName = c.String(),
                        LastModified = c.DateTime(),
                        FileSize = c.Int(),
                        FileExtension = c.String(),
                        ToolTypeId = c.Int(),
                        InterventionToolType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSInterventionToolType", t => t.InterventionToolType_Id)
                .Index(t => t.InterventionToolType_Id);
            
            CreateTable(
                "dbo.NSInterventionToolType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSInterventionUnitOfStudy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UnitName = c.String(),
                        UnitDescription = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NSInterventionVideoNSIntervention",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionId = c.Int(nullable: false),
                        InterventionVideoId = c.Int(nullable: false),
                        SortOrder = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                        InterventionType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSIntervention", t => t.InterventionType_Id)
                .ForeignKey("dbo.NSInterventionVideo", t => t.InterventionVideoId, cascadeDelete: true)
                .Index(t => t.InterventionVideoId)
                .Index(t => t.InterventionType_Id);
            
            CreateTable(
                "dbo.NSInterventionVideo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VideoName = c.String(),
                        ChapterStartTime = c.Int(nullable: false),
                        VideoParentId = c.Int(),
                        EncodedVideoURL = c.String(),
                        VideoFileName = c.String(),
                        Description = c.String(),
                        VideoLength = c.String(),
                        FileExtension = c.String(),
                        FileSize = c.String(),
                        LastModified = c.DateTime(),
                        SortOrder = c.Int(),
                        VideoStreamId = c.String(),
                        ThumbnailURL = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                        ParentVideo_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSInterventionVideo", t => t.ParentVideo_Id)
                .Index(t => t.ParentVideo_Id);
            
            CreateTable(
                "dbo.NSInterventionVideoDistrict",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VideoId = c.Int(nullable: false),
                        DistrictId = c.Int(nullable: false),
                        InterventionVideo_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.District", t => t.DistrictId, cascadeDelete: true)
                .ForeignKey("dbo.NSInterventionVideo", t => t.InterventionVideo_Id)
                .Index(t => t.DistrictId)
                .Index(t => t.InterventionVideo_Id);
            
            CreateTable(
                "dbo.NSInterventionWorkshop",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkshopName = c.String(),
                        WorkshopDescription = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NSIntervention", "InterventionWorkshop_Id", "dbo.NSInterventionWorkshop");
            DropForeignKey("dbo.NSInterventionVideoNSIntervention", "InterventionVideoId", "dbo.NSInterventionVideo");
            DropForeignKey("dbo.NSInterventionVideoDistrict", "InterventionVideo_Id", "dbo.NSInterventionVideo");
            DropForeignKey("dbo.NSInterventionVideoDistrict", "DistrictId", "dbo.District");
            DropForeignKey("dbo.NSInterventionVideo", "ParentVideo_Id", "dbo.NSInterventionVideo");
            DropForeignKey("dbo.NSInterventionVideoNSIntervention", "InterventionType_Id", "dbo.NSIntervention");
            DropForeignKey("dbo.NSIntervention", "InterventionUnitOfStudy_Id", "dbo.NSInterventionUnitOfStudy");
            DropForeignKey("dbo.NSInterventionToolIntervention", "InterventionType_Id", "dbo.NSIntervention");
            DropForeignKey("dbo.NSInterventionTool", "InterventionToolType_Id", "dbo.NSInterventionToolType");
            DropForeignKey("dbo.NSInterventionToolIntervention", "InterventionToolId", "dbo.NSInterventionTool");
            DropForeignKey("dbo.NSIntervention", "InterventionTierId", "dbo.NSInterventionTier");
            DropForeignKey("dbo.NSInterventionGrade", "InterventionType_Id", "dbo.NSIntervention");
            DropForeignKey("dbo.NSInterventionGrade", "GradeID", "dbo.NSGrade");
            DropForeignKey("dbo.NSIntervention", "InterventionFramework_Id", "dbo.NSInterventionFramework");
            DropForeignKey("dbo.NSIntervention", "InterventionCategory_Id", "dbo.NSInterventionCategory");
            DropForeignKey("dbo.NSIntervention", "InterventionCardinalityId", "dbo.NSInterventionCardinality");
            DropIndex("dbo.NSInterventionVideoDistrict", new[] { "InterventionVideo_Id" });
            DropIndex("dbo.NSInterventionVideoDistrict", new[] { "DistrictId" });
            DropIndex("dbo.NSInterventionVideo", new[] { "ParentVideo_Id" });
            DropIndex("dbo.NSInterventionVideoNSIntervention", new[] { "InterventionType_Id" });
            DropIndex("dbo.NSInterventionVideoNSIntervention", new[] { "InterventionVideoId" });
            DropIndex("dbo.NSInterventionTool", new[] { "InterventionToolType_Id" });
            DropIndex("dbo.NSInterventionToolIntervention", new[] { "InterventionType_Id" });
            DropIndex("dbo.NSInterventionToolIntervention", new[] { "InterventionToolId" });
            DropIndex("dbo.NSInterventionGrade", new[] { "InterventionType_Id" });
            DropIndex("dbo.NSInterventionGrade", new[] { "GradeID" });
            DropIndex("dbo.NSIntervention", new[] { "InterventionWorkshop_Id" });
            DropIndex("dbo.NSIntervention", new[] { "InterventionUnitOfStudy_Id" });
            DropIndex("dbo.NSIntervention", new[] { "InterventionFramework_Id" });
            DropIndex("dbo.NSIntervention", new[] { "InterventionCategory_Id" });
            DropIndex("dbo.NSIntervention", new[] { "InterventionTierId" });
            DropIndex("dbo.NSIntervention", new[] { "InterventionCardinalityId" });
            DropTable("dbo.NSInterventionWorkshop");
            DropTable("dbo.NSInterventionVideoDistrict");
            DropTable("dbo.NSInterventionVideo");
            DropTable("dbo.NSInterventionVideoNSIntervention");
            DropTable("dbo.NSInterventionUnitOfStudy");
            DropTable("dbo.NSInterventionToolType");
            DropTable("dbo.NSInterventionTool");
            DropTable("dbo.NSInterventionToolIntervention");
            DropTable("dbo.NSInterventionTier");
            DropTable("dbo.NSInterventionGrade");
            DropTable("dbo.NSInterventionFramework");
            DropTable("dbo.NSInterventionCategory");
            DropTable("dbo.NSIntervention");
            DropTable("dbo.NSInterventionCardinality");
            DropTable("dbo.NSGrade");
            DropTable("dbo.District");
        }
    }
}
