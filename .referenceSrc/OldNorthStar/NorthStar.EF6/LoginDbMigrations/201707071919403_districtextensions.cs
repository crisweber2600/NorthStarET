namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class districtextensions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.District", "ProfilePicturePrefix", c => c.String());
            AddColumn("dbo.District", "ProfilePictureExtension", c => c.String());
            AddColumn("dbo.District", "Extension1", c => c.String());
            AddColumn("dbo.District", "Extension2", c => c.String());
            AddColumn("dbo.District", "Extension3", c => c.String());
            AddColumn("dbo.District", "Extension4", c => c.String());
            AddColumn("dbo.District", "Extension5", c => c.String());
            AddColumn("dbo.District", "Extension6", c => c.String());
            AddColumn("dbo.District", "Extension7", c => c.String());
            AddColumn("dbo.District", "Extension8", c => c.String());
            AddColumn("dbo.District", "Extension9", c => c.String());
            AddColumn("dbo.District", "Extension10", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.District", "Extension10");
            DropColumn("dbo.District", "Extension9");
            DropColumn("dbo.District", "Extension8");
            DropColumn("dbo.District", "Extension7");
            DropColumn("dbo.District", "Extension6");
            DropColumn("dbo.District", "Extension5");
            DropColumn("dbo.District", "Extension4");
            DropColumn("dbo.District", "Extension3");
            DropColumn("dbo.District", "Extension2");
            DropColumn("dbo.District", "Extension1");
            DropColumn("dbo.District", "ProfilePictureExtension");
            DropColumn("dbo.District", "ProfilePicturePrefix");
        }
    }
}
