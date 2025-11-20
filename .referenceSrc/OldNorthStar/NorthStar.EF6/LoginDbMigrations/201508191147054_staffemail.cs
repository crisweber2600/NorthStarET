namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class staffemail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StaffDistrict", "StaffEmail", c => c.String());
            DropColumn("dbo.StaffDistrict", "StaffId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StaffDistrict", "StaffId", c => c.Int(nullable: false));
            DropColumn("dbo.StaffDistrict", "StaffEmail");
        }
    }
}
