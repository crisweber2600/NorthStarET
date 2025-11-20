namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mtss : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NSPage",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(),
                        BriefDescription = c.String(),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PagePresentation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PageId = c.Int(nullable: false),
                        PresentationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSPage", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.Presentation", t => t.PresentationId, cascadeDelete: true)
                .Index(t => t.PageId)
                .Index(t => t.PresentationId);
            
            CreateTable(
                "dbo.Presentation",
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PageTool",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PageId = c.Int(nullable: false),
                        ToolId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSPage", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.Tool", t => t.ToolId, cascadeDelete: true)
                .Index(t => t.PageId)
                .Index(t => t.ToolId);
            
            CreateTable(
                "dbo.Tool",
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
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PageVideo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PageId = c.Int(nullable: false),
                        VideoId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NSPage", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.Video", t => t.VideoId, cascadeDelete: true)
                .Index(t => t.PageId)
                .Index(t => t.VideoId);
            
            CreateTable(
                "dbo.Video",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VideoName = c.String(),
                        ChapterStartTime = c.Int(),
                        ParentVideoId = c.Int(),
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
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PageVideo", "VideoId", "dbo.Video");
            DropForeignKey("dbo.PageVideo", "PageId", "dbo.NSPage");
            DropForeignKey("dbo.PageTool", "ToolId", "dbo.Tool");
            DropForeignKey("dbo.PageTool", "PageId", "dbo.NSPage");
            DropForeignKey("dbo.PagePresentation", "PresentationId", "dbo.Presentation");
            DropForeignKey("dbo.PagePresentation", "PageId", "dbo.NSPage");
            DropIndex("dbo.PageVideo", new[] { "VideoId" });
            DropIndex("dbo.PageVideo", new[] { "PageId" });
            DropIndex("dbo.PageTool", new[] { "ToolId" });
            DropIndex("dbo.PageTool", new[] { "PageId" });
            DropIndex("dbo.PagePresentation", new[] { "PresentationId" });
            DropIndex("dbo.PagePresentation", new[] { "PageId" });
            DropTable("dbo.Video");
            DropTable("dbo.PageVideo");
            DropTable("dbo.Tool");
            DropTable("dbo.PageTool");
            DropTable("dbo.Presentation");
            DropTable("dbo.PagePresentation");
            DropTable("dbo.NSPage");
        }
    }
}
