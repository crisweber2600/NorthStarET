namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class staff_studentatts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StaffStudentAttribute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeId = c.Int(nullable: false),
                        StaffId = c.Int(nullable: false),
                        Visible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StudentAttributeType", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.AttributeId)
                .Index(t => t.StaffId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StaffStudentAttribute", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffStudentAttribute", "AttributeId", "dbo.StudentAttributeType");
            DropIndex("dbo.StaffStudentAttribute", new[] { "StaffId" });
            DropIndex("dbo.StaffStudentAttribute", new[] { "AttributeId" });
            DropTable("dbo.StaffStudentAttribute");
        }
    }
}
