namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rolloverftp_isactive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AutomatedRolloverDetail", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AutomatedRolloverDetail", "IsActive");
        }
    }
}
