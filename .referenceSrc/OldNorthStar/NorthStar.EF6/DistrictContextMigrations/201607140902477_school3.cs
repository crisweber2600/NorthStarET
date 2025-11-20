namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class school3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.School", "IsSS", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.School", "IsSS");
        }
    }
}
