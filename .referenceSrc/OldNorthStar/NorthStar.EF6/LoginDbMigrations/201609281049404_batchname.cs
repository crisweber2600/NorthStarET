namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class batchname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobPrintBatch", "BatchName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobPrintBatch", "BatchName");
        }
    }
}
