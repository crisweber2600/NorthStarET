namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changestaffresulttostring : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StaffSetting", "SelectedValueId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.StaffSetting", "SelectedValueId", c => c.Int(nullable: false));
        }
    }
}
