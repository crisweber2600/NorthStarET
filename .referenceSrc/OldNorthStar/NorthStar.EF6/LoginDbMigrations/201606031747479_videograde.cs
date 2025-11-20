namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class videograde : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InterventionVideoGrade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionVideoId = c.Int(nullable: false),
                        GradeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSGrade", t => t.GradeId, cascadeDelete: true)
                .ForeignKey("dbo.NSInterventionVideo", t => t.InterventionVideoId, cascadeDelete: true)
                .Index(t => t.InterventionVideoId)
                .Index(t => t.GradeId);
            
            AddColumn("dbo.District", "AzureContainerName", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InterventionVideoGrade", "InterventionVideoId", "dbo.NSInterventionVideo");
            DropForeignKey("dbo.InterventionVideoGrade", "GradeId", "dbo.NSGrade");
            DropIndex("dbo.InterventionVideoGrade", new[] { "GradeId" });
            DropIndex("dbo.InterventionVideoGrade", new[] { "InterventionVideoId" });
            DropColumn("dbo.District", "AzureContainerName");
            DropTable("dbo.InterventionVideoGrade");
        }
    }
}
