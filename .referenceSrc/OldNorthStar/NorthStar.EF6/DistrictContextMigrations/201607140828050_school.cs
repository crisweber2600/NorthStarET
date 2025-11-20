namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class school : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.School", "IsPreK", c => c.Boolean());
            AddColumn("dbo.School", "IsK2", c => c.Boolean());
            AddColumn("dbo.School", "Is35", c => c.Boolean());
            AddColumn("dbo.School", "IsK8", c => c.Boolean());
            AddColumn("dbo.School", "IsMS", c => c.Boolean());
            AddColumn("dbo.School", "IsHS", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.School", "IsHS");
            DropColumn("dbo.School", "IsMS");
            DropColumn("dbo.School", "IsK8");
            DropColumn("dbo.School", "Is35");
            DropColumn("dbo.School", "IsK2");
            DropColumn("dbo.School", "IsPreK");
        }
    }
}
