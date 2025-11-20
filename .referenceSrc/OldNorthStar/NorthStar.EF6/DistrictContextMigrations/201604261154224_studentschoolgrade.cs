namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentschoolgrade : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentSchool", "GradeId", c => c.Int(nullable: false));
            CreateIndex("dbo.StudentSchool", "GradeId");
            AddForeignKey("dbo.StudentSchool", "GradeId", "dbo.Grade", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentSchool", "GradeId", "dbo.Grade");
            DropIndex("dbo.StudentSchool", new[] { "GradeId" });
            DropColumn("dbo.StudentSchool", "GradeId");
        }
    }
}
