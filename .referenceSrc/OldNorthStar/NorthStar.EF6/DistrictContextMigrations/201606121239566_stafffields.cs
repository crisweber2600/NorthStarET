namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stafffields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Staff", "IsSA", c => c.Boolean());
            AddColumn("dbo.Staff", "IsPowerUser", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Staff", "IsPowerUser");
            DropColumn("dbo.Staff", "IsSA");
        }
    }
}
