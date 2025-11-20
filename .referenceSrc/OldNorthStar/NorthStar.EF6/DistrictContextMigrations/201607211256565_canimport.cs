namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class canimport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assessment", "CanImport", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Assessment", "CanImport");
        }
    }
}
