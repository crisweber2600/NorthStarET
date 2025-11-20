namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assessmentfieldunique : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AssessmentField", "UniqueImportColumnName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AssessmentField", "UniqueImportColumnName");
        }
    }
}
