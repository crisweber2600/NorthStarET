namespace NorthStar.EF6.LoginDbMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class interventions2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NSInterventionVideoNSIntervention", "InterventionType_Id", "dbo.NSIntervention");
            DropIndex("dbo.NSInterventionVideoNSIntervention", new[] { "InterventionType_Id" });
            RenameColumn(table: "dbo.NSInterventionVideoNSIntervention", name: "InterventionType_Id", newName: "InterventionTypeId");
            AlterColumn("dbo.District", "Enabled", c => c.Boolean());
            AlterColumn("dbo.District", "AccessLevel", c => c.Int());
            AlterColumn("dbo.NSInterventionVideoNSIntervention", "InterventionTypeId", c => c.Int(nullable: false));
            CreateIndex("dbo.NSInterventionVideoNSIntervention", "InterventionTypeId");
            AddForeignKey("dbo.NSInterventionVideoNSIntervention", "InterventionTypeId", "dbo.NSIntervention", "Id", cascadeDelete: true);
            DropColumn("dbo.NSInterventionVideoNSIntervention", "InterventionId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.NSInterventionVideoNSIntervention", "InterventionId", c => c.Int(nullable: false));
            DropForeignKey("dbo.NSInterventionVideoNSIntervention", "InterventionTypeId", "dbo.NSIntervention");
            DropIndex("dbo.NSInterventionVideoNSIntervention", new[] { "InterventionTypeId" });
            AlterColumn("dbo.NSInterventionVideoNSIntervention", "InterventionTypeId", c => c.Int());
            AlterColumn("dbo.District", "AccessLevel", c => c.Int(nullable: false));
            AlterColumn("dbo.District", "Enabled", c => c.Boolean(nullable: false));
            RenameColumn(table: "dbo.NSInterventionVideoNSIntervention", name: "InterventionTypeId", newName: "InterventionType_Id");
            CreateIndex("dbo.NSInterventionVideoNSIntervention", "InterventionType_Id");
            AddForeignKey("dbo.NSInterventionVideoNSIntervention", "InterventionType_Id", "dbo.NSIntervention", "Id");
        }
    }
}
