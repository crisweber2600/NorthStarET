namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class schoolstartyearjob : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStateTestDataImport", "SchoolStartYear", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStateTestDataImport", "SchoolStartYear");
        }
    }
}
