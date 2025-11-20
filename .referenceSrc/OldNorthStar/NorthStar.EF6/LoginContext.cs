using EntityDto.Entity;
using EntityDto.LoginDB;
using EntityDto.LoginDB.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public class LoginContext : DbContext
    {
        public LoginContext() : base(@"Data Source=localhost;Initial Catalog=NorthStar4Login;User ID=remote;Password=Passw0rd;MultipleActiveResultSets=True") // only for migrations
        {

        }

        public LoginContext(string connectionString) : base(connectionString)
        {
            Database.SetInitializer<LoginContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();


        }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Presentation> Presentations { get; set; }
        public DbSet<NSPage> NSPages { get; set; }
        public DbSet<PagePresentation> PagePresentations { get; set; }
        public DbSet<PageVideo> PageVideos { get; set; }
        public DbSet<PageTool> PageTools { get; set; }
        public DbSet<Tool> Tools { get; set; }

        public DbSet<AutomatedRolloverDetail> AutomatedRolloverDetails { get; set; }
        public DbSet<PrintSetting> PrintSettings { get; set; }
        public DbSet<NorthStarVersion> NorthStarVersions { get; set; }
        public DbSet<JobStateTestDataImport> JobStateTestDataImports { get; set; }
        public DbSet<JobFullRollover> JobFullRollovers { get; set; }
        public DbSet<JobStudentRollover> JobStudentRollovers { get; set; }
        public DbSet<JobTeacherRollover> JobTeacherRollovers { get; set; }
        public DbSet<JobBenchmarkDataImport> JobBenchmarkDataImports { get; set; }
        public DbSet<JobAssessmentDataExport> JobAssessmentDataExports { get; set; }
        public DbSet<JobAllFieldsAssessmentDataExport> JobAllFieldsAssessmentDataExports { get; set; }
        public DbSet<JobInterventionDataExport> JobInterventionDataExports { get; set; }
        public DbSet<JobPrintBatch> JobPrintBatches { get; set; }
        public DbSet<JobAttendanceExport> JobAttendanceExports { get; set; }
        public DbSet<JobStudentExport> JobStudentExports { get; set; }
        public DbSet<JobStaffExport> JobStaffExports { get; set; }
        public DbSet<JobInterventionDataImport> JobInterventionDataImports { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

        public DbSet<DistrictDb> DistrictDbs { get; set; }
        public DbSet<StaffDistrict> StaffDistricts { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<NSIntervention> NSInterventions { get; set; }
        public DbSet<NSInterventionCardinality> NSInterventionCardinalities { get; set; }
        public DbSet<NSInterventionCategory> NSInterventionCategories { get; set; }
        public DbSet<NSInterventionGrade> NSInterventionGrades { get; set; }
        public DbSet<NSGrade> NSGrades { get; set; }
        public DbSet<NSInterventionVideo> NSInterventionVideos { get; set; }
        public DbSet<NSInterventionVideoNSIntervention> NSInterventionVideoNSInterventions { get; set; }
        public DbSet<NSInterventionWorkshop> NSInterventionWorkshops { get; set; }
        public DbSet<NSInterventionUnitOfStudy> NSInterventionUnitsOfStudy { get; set; }
        public DbSet<NSInterventionToolType> NSInterventionToolTypes { get; set; }
        public DbSet<NSInterventionTier> NSInterventionTiers { get; set; }
        public DbSet<NSInterventionFramework> NSInterventionFrameworks { get; set; }
        public DbSet<NSInterventionTool> NSInterventionTools { get; set; }
        public DbSet<NSInterventionToolIntervention> NSInterventionToolInterventions { get; set; }
        public DbSet<NSInterventionVideoDistrict> NSInterventionVideoDistricts { get; set; }
        public DbSet<InterventionVideoGrade> NSInterventionVideoGrades { get; set; }
        public DbSet<NSPageHelp> HelpPages { get; set; }
    }
}
