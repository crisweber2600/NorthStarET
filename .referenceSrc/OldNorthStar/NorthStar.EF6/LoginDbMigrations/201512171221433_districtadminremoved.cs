namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class districtadminremoved : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.StaffDistrict", "PermissionLevel");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StaffDistrict", "PermissionLevel", c => c.Int(nullable: false));
        }
    }
}
