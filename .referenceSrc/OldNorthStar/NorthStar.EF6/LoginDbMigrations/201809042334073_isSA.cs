namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isSA : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StaffDistrict", "IsSA", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StaffDistrict", "IsSA");
        }
    }
}
