namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentattributelookup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentAttributeLookupValue", "IsSpecialEd", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentAttributeLookupValue", "IsSpecialEd");
        }
    }
}
