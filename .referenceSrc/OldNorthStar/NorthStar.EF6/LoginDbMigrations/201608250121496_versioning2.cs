namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class versioning2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StaffDistrict", "CurrentVersion", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.StaffDistrict", "VersionLastUpdated", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StaffDistrict", "VersionLastUpdated", c => c.DateTime(nullable: false));
            AlterColumn("dbo.StaffDistrict", "CurrentVersion", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
