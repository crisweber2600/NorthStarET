namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class url : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.JobStateTestDataImport", "UploadedFileUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.JobStateTestDataImport", "UploadedFileUrl");
        }
    }
}
