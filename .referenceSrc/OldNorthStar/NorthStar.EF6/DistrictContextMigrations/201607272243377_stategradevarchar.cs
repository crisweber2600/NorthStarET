namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stategradevarchar : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Grade", "StateGradeNumber", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Grade", "StateGradeNumber", c => c.Int());
        }
    }
}
