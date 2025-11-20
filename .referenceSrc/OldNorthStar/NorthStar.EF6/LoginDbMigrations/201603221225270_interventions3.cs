namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class interventions3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NSInterventionVideo", "ChapterStartTime", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NSInterventionVideo", "ChapterStartTime", c => c.Int(nullable: false));
        }
    }
}
