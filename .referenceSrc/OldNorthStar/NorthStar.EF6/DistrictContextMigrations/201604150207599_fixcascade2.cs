namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixcascade2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.StudentAttributeData", "AttributeValueID", "dbo.StudentAttributeLookupValue");
            DropIndex("dbo.StudentAttributeData", new[] { "AttributeValueID" });
            AddColumn("dbo.StudentAttributeData", "LookupValue_Id", c => c.Int());
            CreateIndex("dbo.StudentAttributeData", "LookupValue_Id");
            AddForeignKey("dbo.StudentAttributeData", "LookupValue_Id", "dbo.StudentAttributeLookupValue", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentAttributeData", "LookupValue_Id", "dbo.StudentAttributeLookupValue");
            DropIndex("dbo.StudentAttributeData", new[] { "LookupValue_Id" });
            DropColumn("dbo.StudentAttributeData", "LookupValue_Id");
            CreateIndex("dbo.StudentAttributeData", "AttributeValueID");
            AddForeignKey("dbo.StudentAttributeData", "AttributeValueID", "dbo.StudentAttributeLookupValue", "Id");
        }
    }
}
