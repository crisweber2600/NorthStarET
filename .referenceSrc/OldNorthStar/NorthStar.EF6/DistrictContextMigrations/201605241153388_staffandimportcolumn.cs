namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class staffandimportcolumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentField", "ImportColumnName", c => c.String());
            AddColumn("dbo.Staff", "IsDistrictContact", c => c.Boolean());
            AddColumn("dbo.StaffSchool", "IsSchoolContact", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StaffSchool", "IsSchoolContact");
            DropColumn("dbo.Staff", "IsDistrictContact");
            DropColumn("dbo.AssessmentField", "ImportColumnName");
        }
    }
}
