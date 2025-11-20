namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class passwordresetrequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PasswordResetRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResetRequestDateStamp = c.DateTime(nullable: false),
                        UserName = c.String(),
                        UID = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.PasswordResetRequest");
        }
    }
}
