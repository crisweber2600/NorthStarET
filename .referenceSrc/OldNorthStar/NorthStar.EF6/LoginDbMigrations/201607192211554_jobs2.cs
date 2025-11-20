namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class jobs2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStateTestDataImport", "StaffEmail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStateTestDataImport", "StaffEmail");
        }
    }
}
