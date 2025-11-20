namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class interventions1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NSInterventionGrade", "InterventionType_Id", "dbo.NSIntervention");
            DropForeignKey("dbo.NSInterventionToolIntervention", "InterventionType_Id", "dbo.NSIntervention");
            DropForeignKey("dbo.NSInterventionVideoDistrict", "InterventionVideo_Id", "dbo.NSInterventionVideo");
            DropIndex("dbo.NSInterventionGrade", new[] { "InterventionType_Id" });
            DropIndex("dbo.NSInterventionToolIntervention", new[] { "InterventionType_Id" });
            DropIndex("dbo.NSInterventionVideoDistrict", new[] { "InterventionVideo_Id" });
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionCategory_Id", newName: "InterventionCategoryId");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionFramework_Id", newName: "InterventionFrameworkId");
            RenameColumn(table: "dbo.NSInterventionGrade", name: "InterventionType_Id", newName: "InterventionTypeId");
            RenameColumn(table: "dbo.NSInterventionToolIntervention", name: "InterventionType_Id", newName: "InterventionTypeId");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionUnitOfStudy_Id", newName: "InterventionUnitOfStudyId");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionWorkshop_Id", newName: "InterventionWorkshopId");
            RenameColumn(table: "dbo.NSInterventionTool", name: "InterventionToolType_Id", newName: "InterventionToolTypeId");
            RenameColumn(table: "dbo.NSInterventionVideo", name: "ParentVideo_Id", newName: "ParentVideoId");
            RenameColumn(table: "dbo.NSInterventionVideoDistrict", name: "InterventionVideo_Id", newName: "InterventionVideoId");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionCategory_Id", newName: "IX_InterventionCategoryId");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionFramework_Id", newName: "IX_InterventionFrameworkId");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionUnitOfStudy_Id", newName: "IX_InterventionUnitOfStudyId");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionWorkshop_Id", newName: "IX_InterventionWorkshopId");
            RenameIndex(table: "dbo.NSInterventionTool", name: "IX_InterventionToolType_Id", newName: "IX_InterventionToolTypeId");
            RenameIndex(table: "dbo.NSInterventionVideo", name: "IX_ParentVideo_Id", newName: "IX_ParentVideoId");
            AlterColumn("dbo.NSInterventionGrade", "InterventionTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.NSInterventionToolIntervention", "InterventionTypeId", c => c.Int(nullable: false));
            AlterColumn("dbo.NSInterventionVideoDistrict", "InterventionVideoId", c => c.Int(nullable: false));
            CreateIndex("dbo.NSInterventionGrade", "InterventionTypeId");
            CreateIndex("dbo.NSInterventionToolIntervention", "InterventionTypeId");
            CreateIndex("dbo.NSInterventionVideoDistrict", "InterventionVideoId");
            AddForeignKey("dbo.NSInterventionGrade", "InterventionTypeId", "dbo.NSIntervention", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NSInterventionToolIntervention", "InterventionTypeId", "dbo.NSIntervention", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NSInterventionVideoDistrict", "InterventionVideoId", "dbo.NSInterventionVideo", "Id", cascadeDelete: true);
            DropColumn("dbo.NSIntervention", "CategoryID");
            DropColumn("dbo.NSIntervention", "FrameworkId");
            DropColumn("dbo.NSIntervention", "UnitOfStudyId");
            DropColumn("dbo.NSIntervention", "WorkshopId");
            DropColumn("dbo.NSInterventionGrade", "InterventionID");
            DropColumn("dbo.NSInterventionToolIntervention", "InterventionId");
            DropColumn("dbo.NSInterventionTool", "ToolTypeId");
            DropColumn("dbo.NSInterventionVideo", "VideoParentId");
            DropColumn("dbo.NSInterventionVideoDistrict", "VideoId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NSInterventionVideoDistrict", "VideoId", c => c.Int(nullable: false));
            AddColumn("dbo.NSInterventionVideo", "VideoParentId", c => c.Int());
            AddColumn("dbo.NSInterventionTool", "ToolTypeId", c => c.Int());
            AddColumn("dbo.NSInterventionToolIntervention", "InterventionId", c => c.Int(nullable: false));
            AddColumn("dbo.NSInterventionGrade", "InterventionID", c => c.Int(nullable: false));
            AddColumn("dbo.NSIntervention", "WorkshopId", c => c.Int());
            AddColumn("dbo.NSIntervention", "UnitOfStudyId", c => c.Int());
            AddColumn("dbo.NSIntervention", "FrameworkId", c => c.Int());
            AddColumn("dbo.NSIntervention", "CategoryID", c => c.Int());
            DropForeignKey("dbo.NSInterventionVideoDistrict", "InterventionVideoId", "dbo.NSInterventionVideo");
            DropForeignKey("dbo.NSInterventionToolIntervention", "InterventionTypeId", "dbo.NSIntervention");
            DropForeignKey("dbo.NSInterventionGrade", "InterventionTypeId", "dbo.NSIntervention");
            DropIndex("dbo.NSInterventionVideoDistrict", new[] { "InterventionVideoId" });
            DropIndex("dbo.NSInterventionToolIntervention", new[] { "InterventionTypeId" });
            DropIndex("dbo.NSInterventionGrade", new[] { "InterventionTypeId" });
            AlterColumn("dbo.NSInterventionVideoDistrict", "InterventionVideoId", c => c.Int());
            AlterColumn("dbo.NSInterventionToolIntervention", "InterventionTypeId", c => c.Int());
            AlterColumn("dbo.NSInterventionGrade", "InterventionTypeId", c => c.Int());
            RenameIndex(table: "dbo.NSInterventionVideo", name: "IX_ParentVideoId", newName: "IX_ParentVideo_Id");
            RenameIndex(table: "dbo.NSInterventionTool", name: "IX_InterventionToolTypeId", newName: "IX_InterventionToolType_Id");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionWorkshopId", newName: "IX_InterventionWorkshop_Id");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionUnitOfStudyId", newName: "IX_InterventionUnitOfStudy_Id");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionFrameworkId", newName: "IX_InterventionFramework_Id");
            RenameIndex(table: "dbo.NSIntervention", name: "IX_InterventionCategoryId", newName: "IX_InterventionCategory_Id");
            RenameColumn(table: "dbo.NSInterventionVideoDistrict", name: "InterventionVideoId", newName: "InterventionVideo_Id");
            RenameColumn(table: "dbo.NSInterventionVideo", name: "ParentVideoId", newName: "ParentVideo_Id");
            RenameColumn(table: "dbo.NSInterventionTool", name: "InterventionToolTypeId", newName: "InterventionToolType_Id");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionWorkshopId", newName: "InterventionWorkshop_Id");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionUnitOfStudyId", newName: "InterventionUnitOfStudy_Id");
            RenameColumn(table: "dbo.NSInterventionToolIntervention", name: "InterventionTypeId", newName: "InterventionType_Id");
            RenameColumn(table: "dbo.NSInterventionGrade", name: "InterventionTypeId", newName: "InterventionType_Id");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionFrameworkId", newName: "InterventionFramework_Id");
            RenameColumn(table: "dbo.NSIntervention", name: "InterventionCategoryId", newName: "InterventionCategory_Id");
            CreateIndex("dbo.NSInterventionVideoDistrict", "InterventionVideo_Id");
            CreateIndex("dbo.NSInterventionToolIntervention", "InterventionType_Id");
            CreateIndex("dbo.NSInterventionGrade", "InterventionType_Id");
            AddForeignKey("dbo.NSInterventionVideoDistrict", "InterventionVideo_Id", "dbo.NSInterventionVideo", "Id");
            AddForeignKey("dbo.NSInterventionToolIntervention", "InterventionType_Id", "dbo.NSIntervention", "Id");
            AddForeignKey("dbo.NSInterventionGrade", "InterventionType_Id", "dbo.NSIntervention", "Id");
        }
    }
}
