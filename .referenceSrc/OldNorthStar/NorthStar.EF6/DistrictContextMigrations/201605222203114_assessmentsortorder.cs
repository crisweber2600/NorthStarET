namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assessmentsortorder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Assessment", "SortOrder", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Assessment", "SortOrder");
        }
    }
}
