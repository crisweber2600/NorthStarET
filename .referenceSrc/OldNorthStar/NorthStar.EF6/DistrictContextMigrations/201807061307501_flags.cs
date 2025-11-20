namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class flags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentFieldCategory", "Flag1", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldCategory", "Flag2", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldCategory", "Flag3", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldCategory", "Flag4", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldCategory", "Flag5", c => c.Boolean());
            AddColumn("dbo.AssessmentField", "Flag1", c => c.Boolean());
            AddColumn("dbo.AssessmentField", "Flag2", c => c.Boolean());
            AddColumn("dbo.AssessmentField", "Flag3", c => c.Boolean());
            AddColumn("dbo.AssessmentField", "Flag4", c => c.Boolean());
            AddColumn("dbo.AssessmentField", "Flag5", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroup", "Flag1", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroup", "Flag2", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroup", "Flag3", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroup", "Flag4", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroup", "Flag5", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroupContainer", "Flag1", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroupContainer", "Flag2", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroupContainer", "Flag3", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroupContainer", "Flag4", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldGroupContainer", "Flag5", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldSubCategory", "Flag1", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldSubCategory", "Flag2", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldSubCategory", "Flag3", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldSubCategory", "Flag4", c => c.Boolean());
            AddColumn("dbo.AssessmentFieldSubCategory", "Flag5", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssessmentFieldSubCategory", "Flag5");
            DropColumn("dbo.AssessmentFieldSubCategory", "Flag4");
            DropColumn("dbo.AssessmentFieldSubCategory", "Flag3");
            DropColumn("dbo.AssessmentFieldSubCategory", "Flag2");
            DropColumn("dbo.AssessmentFieldSubCategory", "Flag1");
            DropColumn("dbo.AssessmentFieldGroupContainer", "Flag5");
            DropColumn("dbo.AssessmentFieldGroupContainer", "Flag4");
            DropColumn("dbo.AssessmentFieldGroupContainer", "Flag3");
            DropColumn("dbo.AssessmentFieldGroupContainer", "Flag2");
            DropColumn("dbo.AssessmentFieldGroupContainer", "Flag1");
            DropColumn("dbo.AssessmentFieldGroup", "Flag5");
            DropColumn("dbo.AssessmentFieldGroup", "Flag4");
            DropColumn("dbo.AssessmentFieldGroup", "Flag3");
            DropColumn("dbo.AssessmentFieldGroup", "Flag2");
            DropColumn("dbo.AssessmentFieldGroup", "Flag1");
            DropColumn("dbo.AssessmentField", "Flag5");
            DropColumn("dbo.AssessmentField", "Flag4");
            DropColumn("dbo.AssessmentField", "Flag3");
            DropColumn("dbo.AssessmentField", "Flag2");
            DropColumn("dbo.AssessmentField", "Flag1");
            DropColumn("dbo.AssessmentFieldCategory", "Flag5");
            DropColumn("dbo.AssessmentFieldCategory", "Flag4");
            DropColumn("dbo.AssessmentFieldCategory", "Flag3");
            DropColumn("dbo.AssessmentFieldCategory", "Flag2");
            DropColumn("dbo.AssessmentFieldCategory", "Flag1");
        }
    }
}
