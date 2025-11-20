namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class helppages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NSPageHelp",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(),
                        HelpPlaceHolder1 = c.String(),
                        HelpPlaceHolder2 = c.String(),
                        HelpPlaceHolder3 = c.String(),
                        HelpPlaceHolder4 = c.String(),
                        HelpPlaceHolder5 = c.String(),
                        HelpPlaceHolder6 = c.String(),
                        HelpPlaceHolder7 = c.String(),
                        HelpPlaceHolder8 = c.String(),
                        HelpPlaceHolder9 = c.String(),
                        HelpPlaceHolder10 = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NSPageHelp");
        }
    }
}
