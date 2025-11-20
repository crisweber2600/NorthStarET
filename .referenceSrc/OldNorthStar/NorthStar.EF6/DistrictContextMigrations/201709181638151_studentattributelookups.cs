namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentattributelookups : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentAttributeLookupValue", "IsDefaultOption", c => c.Boolean());
            AddColumn("dbo.StudentAttributeLookupValue", "NotGenEd", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentAttributeLookupValue", "NotGenEd");
            DropColumn("dbo.StudentAttributeLookupValue", "IsDefaultOption");
        }
    }
}
