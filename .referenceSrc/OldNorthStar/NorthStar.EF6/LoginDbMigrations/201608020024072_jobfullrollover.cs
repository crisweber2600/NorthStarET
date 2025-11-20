namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class jobfullrollover : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobFullRollover", "PotentialIssuesLog", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobFullRollover", "PotentialIssuesLog");
        }
    }
}
