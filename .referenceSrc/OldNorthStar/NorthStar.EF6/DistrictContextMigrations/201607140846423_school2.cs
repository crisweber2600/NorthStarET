namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class school2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.School", "IsK5", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.School", "IsK5");
        }
    }
}
