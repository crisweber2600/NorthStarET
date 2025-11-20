using AutoMapper;
using EntityDto.DTO.Admin.InterventionGroup;
using EntityDto.DTO.Admin.Section;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Reports.StackedBarGraphs;
using EntityDto.Entity;
using NorthStar.EF6.Infrastructure;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using NorthStar4.CrossPlatform.DTO.Reports.StackedBarGraphs;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.DTO;
using NorthStar4.PCL.Entity;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.EF6
{
    public class DistrictContext : DbContext
    {
        public DistrictContext() : base(@"Data Source=localhost;Initial Catalog=196Q4_BeforeSnafu;User ID=remote;Password=Passw0rd;MultipleActiveResultSets=True")
        {
            this.Database.CommandTimeout = 30000;
        }

        public DistrictContext(string connectionString) : base(connectionString)
        {
            this.Database.CommandTimeout = 30000;
        }
        public DbSet<Staff> Staffs { get; set; }
        //public DbSet<WeeklyAttendanceResult> WeeklyAttendanceResults { get; set; }

        public DbSet<StaffStudentAttribute> StaffStudentAttributes { get; set; }
        public DbSet<StaffObservationSummaryAssessment> StaffObservationSummaryAssessments { get; set; }
        public DbSet<StaffObservationSummaryAssessmentField> StaffObservationSummaryAssessmentFields { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AttendeeGroup> AttendeeGroups { get; set; }
        public DbSet<AttendeeGroupStaff> AttendeeGroupStaffs { get; set; }
        public DbSet<Assessment_Benchmarks> AssessmentBenchmarks { get; set; }
        public DbSet<District_Benchmark> DistrictBenchmarks { get; set; }
        public DbSet<District_YearlyAssessmentBenchmark> DistrictYearlyAssessmentBenchmarks { get; set; }
        public DbSet<FPComparison> FPComparisons { get; set; }
        public DbSet<AssessmentLookupField> LookupFields { get; set; }
        public DbSet<AssessmentField> AssessmentFields { get; set; }
        public DbSet<AssessmentFieldGroup> AssessmentFieldGroups { get; set; }
        public DbSet<AssessmentFieldGroupContainer> AssessmentFieldGroupContainers { get; set; }
        public DbSet<AssessmentFieldCategory> AssessmentFieldCategories { get; set; }
        public DbSet<AssessmentFieldSubCategory> AssessmentFieldSubCategories { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<StudentInterventionGroup> StudentInterventionGroups { get; set; }
        public DbSet<StudentSection> StudentSections { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<TestDueDate> TestDueDates { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<StaffSetting> StaffSettings { get; set; }
        public DbSet<SchoolAssessment> SchoolAssessments { get; set; }
        public DbSet<Intervention> Interventions { get; set; }
        public DbSet<InterventionTool> InterventionTools { get; set; }
        public DbSet<TestLevelPeriod> TestLevelPeriods { get; set; }
        public DbSet<InterventionCardinality> InterventionCardinalities { get; set; }
        public DbSet<InterventionUnitOfStudy> InterventionUnitOfStudies { get; set; }
        public DbSet<InterventionCategory> InterventionCategories { get; set; }
        public DbSet<InterventionFramework> InterventionFrameworks { get; set; }
        public DbSet<InterventionToolIntervention> InterventionToolInterventions { get; set; }
        public DbSet<InterventionGrade> InterventionGrades { get; set; }
        public DbSet<InterventionGroup> InterventionGroups { get; set; }
        public DbSet<InterventionWorkshop> InterventionWorkshops { get; set; }
        public DbSet<InterventionTier> InterventionTiers { get; set; }
        public DbSet<SchoolYear> SchoolYears { get; set; }
        public DbSet<StaffAssessment> StaffAssessments { get; set; }
        public DbSet<StaffInterventionGroup> StaffInterventionGroups { get; set; }
        public DbSet<StaffSchool> StaffSchools { get; set; }
        public DbSet<StaffSchoolGrade> StaffSchoolGrades { get; set; }
        public DbSet<StaffSection> StaffSections { get; set; }
        public DbSet<StudentSchool> StudentSchools { get; set; }
        public DbSet<StudentNote> StudentNotes { get; set; }
        public DbSet<SchoolCalendar> SchoolCalendars { get; set; }
        public DbSet<DistrictCalendar> DistrictCalendars { get; set; }
        public DbSet<InterventionAttendance> InterventionAttendances { get; set; }
        public DbSet<AttendanceReason> AttendanceReasons { get; set; }
        public DbSet<StaffAssessmentFieldVisibility> StaffAssessmentFieldVisibilities { get; set; }
        public DbSet<StudentAttributeLookupValue> StudentAttributeLookupValues { get; set; }
        public DbSet<StudentAttributeData> StudentAttributeDatas { get; set; }
        public DbSet<StudentAttributeType> StudentAttributeTypes { get; set; }
        public DbSet<TeamMeeting> TeamMeetings { get; set; }
        public DbSet<TeamMeetingAttendance> TeamMeetingAttendances { get; set; }
        public DbSet<TeamMeetingStudent> TeamMeetingStudents { get; set; }
        public DbSet<TeamMeetingStudentNote> TeamMeetingStudentNotes { get; set; }
        public DbSet<TeamMeetingManager> TeamMeetingManagers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Assessment>()
                .HasMany(e => e.Fields);
            //.WithRequired(e => e.ParentAssessment)
            //.WillCascadeOnDelete(false);
            modelBuilder.Entity<Assessment>()
                .Property(e => e.BaseType)
                .IsOptional();

            modelBuilder.Entity<Assessment>()
                .HasMany(e => e.FieldCategories);

            modelBuilder.Entity<Assessment>()
                .HasMany(e => e.FieldGroupContainers);
            //.WithRequired(e => e.ParentAssessment)
            //.WillCascadeOnDelete(false);

            modelBuilder.Entity<Assessment>()
                .HasMany(e => e.FieldGroups);
            //.WithRequired(e => e.ParentAssessment)
            //.WillCascadeOnDelete(false);

            modelBuilder.Entity<Assessment>()
                .HasMany(e => e.FieldSubCategories);

            modelBuilder.Entity<Assessment>()
                .HasMany(e => e.AssessmentBenchmarks);
            //.WithRequired(e => e.ParentAssessment)
            //.WillCascadeOnDelete(false);

            modelBuilder.Entity<AssessmentField>()
                .HasOptional(e => e.Category)
                .WithMany(e => e.AssessmentFields)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<AssessmentField>()
                .HasOptional(e => e.SubCategory)
                .WithMany(e => e.AssessmentFields)
                .HasForeignKey(e => e.SubcategoryId);

            modelBuilder.Entity<AssessmentField>()
                .HasOptional(e => e.Group)
                .WithMany(e => e.AssessmentFields)
                .HasForeignKey(e => e.GroupId);

            modelBuilder.Entity<StaffAssessmentFieldVisibility>()
            .HasRequired(e => e.AssessmentField)
            .WithMany(e => e.StaffAssessmentFieldVisibilities)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssessmentField>()
            .HasMany(e => e.StaffAssessmentFieldVisibilities)
            .WithRequired(e => e.AssessmentField)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<StaffObservationSummaryAssessmentField>()
                .HasRequired(e => e.Assessment)
                .WithMany(e => e.StaffObservationSummaryAssessmentFields)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StaffObservationSummaryAssessmentField>()
                .HasRequired(e => e.AssessmentField)
                .WithMany(e => e.StaffObservationSummaryAssessmentFields)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AssessmentFieldGroupContainer>()
                .HasMany(e => e.AssessmentFieldGroups)
                .WithOptional(e => e.Container)
                .WillCascadeOnDelete(false);
                

            //modelBuilder.Entity<AssessmentFieldGroup>()
            //    .HasMany(e => e.AssessmentFields);
            //.WithOptional(e => e.Group)
            //.HasForeignKey(e => e.GroupId);

            //modelBuilder.Entity<AssessmentFieldSubCategory>()
            //    .HasMany(e => e.AssessmentFields);
            //.WithOptional(e => e.SubCategory)
            //.HasForeignKey(e => e.SubcategoryId);




            modelBuilder.Entity<Intervention>().ToTable("InterventionType");
            modelBuilder.Entity<Assessment_Benchmarks>()
                .Property(e => e.AssessmentField)
                .IsUnicode(false);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .HasRequired(e => e.TestLevelPeriod)
                .WithMany(e => e.AssessmentBenchmarks)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .HasRequired(e => e.Grade)
                .WithMany(e => e.AssessmentBenchmarks)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .Property(e => e.DoesNotMeet)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .Property(e => e.Approaches)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .Property(e => e.Meets)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .Property(e => e.Exceeds)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Assessment_Benchmarks>()
                .HasRequired(e => e.Assessment)
                .WithMany(e => e.AssessmentBenchmarks)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AttendeeGroup>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<AttendeeGroup>()
                .HasRequired(e => e.Staff)
                .WithMany(e => e.AttendeeGroups)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AttendeeGroupStaff>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Grade>()
                .HasMany(e => e.Sections)
                .WithRequired(e => e.Grade)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InterventionCardinality>()
                .Property(e => e.CardinalityName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionCategory>()
                .Property(e => e.CategoryName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionCategory>()
                .Property(e => e.CategoryDescription)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionCategory>()
                .HasMany(e => e.InterventionTypes)
                .WithOptional(e => e.InterventionCategory)
                .HasForeignKey(e => e.CategoryID);

            modelBuilder.Entity<InterventionFramework>()
                .Property(e => e.FrameworkName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionFramework>()
                .Property(e => e.FreameworkDescription)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionFramework>()
                .HasMany(e => e.InterventionTypes)
                .WithOptional(e => e.InterventionFramework)
                .HasForeignKey(e => e.FrameworkID);

            modelBuilder.Entity<InterventionTier>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTier>()
                .Property(e => e.TierName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTier>()
                .Property(e => e.TierLabel)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTier>()
                .Property(e => e.TierColor)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTool>()
                .Property(e => e.ToolName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTool>()
                .Property(e => e.ToolFileName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTool>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTool>()
                .Property(e => e.FileSystemFileName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTool>()
                .Property(e => e.FileExtension)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionTool>()
                .HasMany(e => e.InterventionToolInterventions)
                .WithRequired(e => e.InterventionTool)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InterventionToolType>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionToolType>()
                .HasMany(e => e.InterventionTools)
                .WithOptional(e => e.InterventionToolType)
                .HasForeignKey(e => e.ToolTypeID);

            modelBuilder.Entity<Intervention>()
               .Property(e => e.InterventionType)
               .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.ExitCriteria)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.EntranceCriteria)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.LearnerNeed)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.DetailedDescription)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.TimeOfYear)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .Property(e => e.BriefDescription)
                .IsUnicode(false);

            modelBuilder.Entity<Intervention>()
                .HasMany(e => e.InterventionToolInterventions)
                .WithRequired(e => e.InterventionType)
                .HasForeignKey(e => e.InterventionID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InterventionGroup>()
                .HasMany(e => e.StaffInterventionGroups)
                .WithRequired(e => e.InterventionGroup)
                .HasForeignKey(e => e.InterventionGroupId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<InterventionGroup>()
                .HasMany(e => e.StudentInterventionGroups)
                .WithRequired(e => e.InterventionGroup)
                .HasForeignKey(e => e.InterventionGroupId)
                .WillCascadeOnDelete(false);

            // hook up section and entities
            modelBuilder.Entity<InterventionGroup>()
                .HasOptional(e => e.InterventionType)
                .WithMany(e => e.InterventionGroups)
                .HasForeignKey(e => e.InterventionTypeID)
                .WillCascadeOnDelete(false);
            //modelBuilder.Entity<Intervention>()
            //    .HasMany(e => e.InterventionVideoInterventions)
            //    .WithRequired(e => e.InterventionType)
            //    .HasForeignKey(e => e.InterventionID)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<InterventionUnitOfStudy>()
                .Property(e => e.UnitName)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionUnitOfStudy>()
                .Property(e => e.UnitDescription)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionUnitOfStudy>()
                .HasMany(e => e.InterventionTypes)
                .WithOptional(e => e.InterventionUnitOfStudy)
                .HasForeignKey(e => e.UnitOfStudyID);

            modelBuilder.Entity<InterventionWorkshop>()
               .Property(e => e.WorkshopName)
               .IsUnicode(false);

            modelBuilder.Entity<InterventionWorkshop>()
                .Property(e => e.WorkshopDescription)
                .IsUnicode(false);

            modelBuilder.Entity<InterventionWorkshop>()
                .HasMany(e => e.InterventionTypes)
                .WithOptional(e => e.InterventionWorkshop)
                .HasForeignKey(e => e.WorkshopID);



            modelBuilder.Entity<School>()
                .HasMany(e => e.Sections)
                .WithRequired(e => e.School)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<School>()
                .HasMany(e => e.InterventionGroups)
                .WithRequired(e => e.School)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<School>()
                .HasMany(e => e.SchoolCalendars)
                .WithRequired(e => e.School)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<School>()
            //    .HasMany(e => e.SchoolTests)
            //    .WithRequired(e => e.School)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<School>()
                .HasMany(e => e.StaffSchools)
                .WithRequired(e => e.School)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<School>()
                .HasMany(e => e.StudentSchools)
                .WithRequired(e => e.School)
                .WillCascadeOnDelete(false);

            //modelBuilder.Entity<School>()
            //    .HasMany(e => e.TeamMeetingStudents)
            //    .WithRequired(e => e.School)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<SchoolCalendar>()
                .Property(e => e.Subject)
                .IsUnicode(false);

            //modelBuilder.Entity<SchoolCalendar>()
            //    .Property(e => e.RecurrenceRule)
            //    .IsUnicode(false);

            modelBuilder.Entity<SchoolYear>()
                .HasKey(e => e.SchoolStartYear);

            modelBuilder.Entity<SchoolYear>()
                .Property(s => s.SchoolStartYear)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            //modelBuilder.Entity<Section>()
            //    .HasMany(e => e.InterventionAttendances)
            //    .WithRequired(e => e.Section)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Section>()
                .HasMany(e => e.StaffSections)
                .WithRequired(e => e.Section)
                .HasForeignKey(e => e.ClassID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Section>()
                .HasMany(e => e.StudentSections)
                .WithRequired(e => e.Section)
                .HasForeignKey(e => e.ClassID)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<Staff>()
                .Ignore(p => p.DistrictId);

            modelBuilder.Entity<Staff>()
                .Ignore(p => p.FullName);

            modelBuilder.Entity<Staff>()
                .Property(e => e.TeacherIdentifier)
                .IsUnicode(false);

            modelBuilder.Entity<Staff>()
                .Property(e => e.LoweredUserName)
                .IsUnicode(false);

            modelBuilder.Entity<Staff>()
                .Property(e => e.FirstName)
                .IsUnicode(false);

            modelBuilder.Entity<Staff>()
                .Property(e => e.MiddleName)
                .IsUnicode(false);

            modelBuilder.Entity<Staff>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<Staff>()
                .Property(e => e.Notes)
                .IsUnicode(false);

            modelBuilder.Entity<Staff>()
                .Property(e => e.Email)
                .IsUnicode(false);

            //modelBuilder.Entity<Staff>()
            //   .HasMany(e => e.InterventionAttendances)
            //   .WithRequired(e => e.Staff)
            //   .HasForeignKey(e => e.RecorderID)
            //   .WillCascadeOnDelete(false);
            modelBuilder.Entity<Staff>()
               .HasMany(e => e.Sections)
               .WithRequired(e => e.Staff)
               .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Staff>()
            //    .HasMany(e => e.StaffCalendars)
            //    .WithRequired(e => e.Staff)
            //    .WillCascadeOnDelete(false);

            modelBuilder.Entity<Staff>()
                .HasMany(e => e.StaffSections)
                .WithRequired(e => e.Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Staff>()
                .HasMany(e => e.StaffSchools)
                .WithRequired(e => e.Staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Staff>()
                .HasMany(e => e.AttendeeGroups)
                .WithRequired(e => e.Staff)
                .WillCascadeOnDelete(false);

            

            modelBuilder.Entity<StaffAssessmentFieldVisibility>()
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Student>()
               .Property(e => e.FirstName)
               .IsUnicode(false);

            modelBuilder.Entity<Student>()
                .Property(e => e.MiddleName)
                .IsUnicode(false);

            modelBuilder.Entity<Student>()
                .Property(e => e.LastName)
                .IsUnicode(false);

            modelBuilder.Entity<Student>()
                .Property(e => e.StudentIdentifier)
                .IsUnicode(false);



            modelBuilder.Entity<Student>()
                .HasMany(e => e.StudentSections)
                .WithRequired(e => e.Student)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<Student>()
                .HasMany(e => e.StudentSchools)
                .WithRequired(e => e.Student)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Student>()
                .HasMany(e => e.StudentAttributeDatas)
                .WithRequired(e => e.Student)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StudentAttributeData>()
                .HasRequired(e => e.AttributeType)
                .WithMany(e => e.StudentAttributeDatas)
                .HasForeignKey(e => e.AttributeID);

            modelBuilder.Entity<Student>()
                .HasMany(e => e.StudentNotes)
                .WithRequired(e => e.Student)
                .WillCascadeOnDelete(true);

            //modelBuilder.Entity<StudentAttributeData>()
            //    .HasRequired(e => e.LookupValue)
            //    .WithMany(e => e.StudentAttributeDatas)
            //    .HasForeignKey(e => e.AttributeValueID);

            modelBuilder.Entity<StudentAttributeType>()
                .Property(p => p.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<StudentAttributeType>()
                .HasMany(e => e.LookupValues)
                .WithRequired(e => e.AttributeType)
                .HasForeignKey(e => e.AttributeID);

            modelBuilder.Entity<InterventionAttendance>()
                .HasRequired(e => e.Recorder)
                .WithMany(e => e.InterventionAttendances)
                .HasForeignKey(e => e.RecorderID)
                .WillCascadeOnDelete(false);



            modelBuilder.Entity<TeamMeeting>()
               .HasMany(e => e.TeamMeetingAttendances)
               .WithRequired(e => e.TeamMeeting)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeamMeeting>()
                .HasMany(e => e.TeamMeetingStudents)
                .WithRequired(e => e.TeamMeeting)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeamMeeting>()
                .HasMany(e => e.TeamMeetingStudentNotes)
                .WithRequired(e => e.TeamMeeting)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeamMeeting>()
                .HasMany(e => e.TeamMeetingManagers)
                .WithRequired(e => e.TeamMeeting)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TeamMeetingManager>()
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TeamMeeting>()
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TeamMeetingStudent>()
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TeamMeetingStudentNote>()
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TeamMeetingAttendance>()
                .Property(e => e.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<TeamMeetingStudentNote>()
                .Property(e => e.Note)
                .IsUnicode(false);

            modelBuilder.Entity<TestDueDate>()
                .Ignore(p => p.PeriodName);

            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<WeeklyAttendanceResult>().HasKey(e => e.RecordKey);
        }

        #region Functions

        public List<OutputDto_StackBarGraphGroupData> GetGroupedStackBarGraphResults(InputDto_GetStackedBarGraphGroupingUpdatedOptions input)
        {
            // check if we should bother or not
            if (input.AssessmentField == null || input.AssessmentField.AssessmentId == 0)
            {
                return new List<OutputDto_StackBarGraphGroupData>();
            }

            var assessment = Assessments.First(p => p.Id == input.AssessmentField.AssessmentId);

            Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
            var attributeParameterArray = new SqlParameter[10];
            var attributeIdParameterArray = new SqlParameter[10];

            // hackfix, empty params
            for (var i = 0; i < 10; i++)
            {
                attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
            }

            // combine the student attributevalues into a single list
            List<string> attributeValues = new List<string>();
            foreach (var attributeType in input.DropdownDataList)
            {
                // hackfix, and yes, I did regret it... thanks jerkface
                attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
            }

            //attributeValues.AddRange(input.TitleOneTypes.Select(p => p.id).ToList());
            //attributeValues.AddRange(input.Ethnicities.Select(p => p.id).ToList());
            //attributeValues.AddRange(input.EducationLabels.Select(p => p.id).ToList());
            //attributeValues.AddRange(input..Select(p => p.id).ToList());

            var procSuffix = assessment.TestType == 3 ? "_YearlyAssessment" : "";

            // call stored procedure and pass parameters
            var results = Database.SqlQuery<OutputDto_StackBarGraphGroupData>(String.Format(@"EXEC [_ns4_report_FilteredStackedBarGraph{0}] @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections, 
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                    @InterventionTypes, @SpecialEd, @AssessmentID, @FieldToRetrieve, @IsDecimalField, @TestDbTable, @TestDueDateID", procSuffix),
                new SqlParameter("SchoolStartYear", input.SchoolStartYear),
                new SqlParameter("Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())),
                new SqlParameter("Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())),
                new SqlParameter("Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())),
                new SqlParameter("Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())),
                attributeParameterArray[0],
                attributeIdParameterArray[0],
                                attributeParameterArray[1],
                attributeIdParameterArray[1],
                                attributeParameterArray[2],
                attributeIdParameterArray[2],
                                attributeParameterArray[3],
                attributeIdParameterArray[3],
                                attributeParameterArray[4],
                attributeIdParameterArray[4],
                                attributeParameterArray[5],
                attributeIdParameterArray[5],
                                attributeParameterArray[6],
                attributeIdParameterArray[6],
                                attributeParameterArray[7],
                attributeIdParameterArray[7],
                                attributeParameterArray[8],
                attributeIdParameterArray[8],
                                attributeParameterArray[9],
                attributeIdParameterArray[9],
                new SqlParameter("InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())),
                new SqlParameter("SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value},
                new SqlParameter("AssessmentID", assessment.Id),
                new SqlParameter("FieldToRetrieve", input.AssessmentField.DatabaseColumn),
                new SqlParameter("IsDecimalField", false),
                new SqlParameter("TestDbTable", assessment.StorageTable),
                new SqlParameter("TestDueDateID", SqlDbType.Int) { Value = (object)input.TestDueDateID ?? DBNull.Value });

            return results.ToList();
        }

        public OutputDto_StackBarGraphSummaryData GetStackedBarGraphGroupSummary(InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions input, int staffId)
        {
            var result = new OutputDto_StackBarGraphSummaryData();

            // check if we should bother or not
            if (input.AssessmentField == null || input.AssessmentField.AssessmentId == 0)
            {
                return result;
            }

            // get all the attributes
            var allAttributes = this.StudentAttributeTypes.Where(p => p.Id != 4).OrderBy(p => p.Id).ToList();

            for (var i = 0; i < allAttributes.Count; i++)
            {
                var Id = allAttributes[i].Id;
                switch (i)
                {
                    case 0:
                        result.Att1Header = allAttributes[i].AttributeName;
                        result.Att1Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 1:
                        result.Att2Header = allAttributes[i].AttributeName;
                        result.Att2Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 2:
                        result.Att3Header = allAttributes[i].AttributeName;
                        result.Att3Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 3:
                        result.Att4Header = allAttributes[i].AttributeName;
                        result.Att4Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 4:
                        result.Att5Header = allAttributes[i].AttributeName;
                        result.Att5Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 5:
                        result.Att6Header = allAttributes[i].AttributeName;
                        result.Att6Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 6:
                        result.Att7Header = allAttributes[i].AttributeName;
                        result.Att7Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 7:
                        result.Att8Header = allAttributes[i].AttributeName;
                        result.Att8Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 8:
                        result.Att9Header = allAttributes[i].AttributeName;
                        result.Att9Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                }
            }

            // combine the student attributevalues into a single list
            //List<int> attributeValues = new List<int>();
            //attributeValues.AddRange(input.TitleOneTypes.Select(p => p.id).ToList());
            //attributeValues.AddRange(input.Ethnicities.Select(p => p.id).ToList());
            //attributeValues.AddRange(input.EducationLabels.Select(p => p.id).ToList());
            Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
            var attributeParameterArray = new SqlParameter[10];
            var attributeIdParameterArray = new SqlParameter[10];

            // hackfix, empty params
            for (var i = 0; i < 10; i++)
            {
                attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
            }

            // combine the student attributevalues into a single list
            List<string> attributeValues = new List<string>();
            foreach (var attributeType in input.DropdownDataList)
            {
                // hackfix, and yes, I did regret it... thanks jerkface
                attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
            }

            var assessment = this.Assessments.Include(p => p.Fields).First(p => p.Id == input.AssessmentField.AssessmentId);
            IEnumerable<AssessmentField> fieldsToRetrieve = null;
            fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInObsSummary == true && p.DisplayInStackedBarGraphSummary == true);
            result.Fields = Mapper.Map<List<AssessmentFieldDto>>(fieldsToRetrieve);
            //attributeValues.AddRange(input..Select(p => p.id).ToList());

            /*
            // call stored procedure and pass parameters
            //var scores = Database.SqlQuery<StackedBarGraphSummaryRecord>("EXEC [_ns4_report_FilteredStackedBarGraph_Summary] @SchoolStartYear, @Schools, @Grades, @Teachers, @Sections, @StudentAttributeValues, @InterventionTypes, @AssessmentID, @FieldToRetrieve, @IsDecimalField, @TestDbTable, @ScoreGrouping, @TestDueDate",
            //    new SqlParameter("SchoolStartYear", input.SchoolStartYear),
            //    new SqlParameter("Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())),
            //    new SqlParameter("Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())),
            //    new SqlParameter("Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())),
            //    new SqlParameter("Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())),
            //    new SqlParameter("StudentAttributeValues", string.Join<int>(",", attributeValues)),
            //    new SqlParameter("InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())),
            //    new SqlParameter("AssessmentID", 1),
            //    new SqlParameter("FieldToRetrieve", "FPValueID"),
            //    new SqlParameter("IsDecimalField", false),
            //    new SqlParameter("TestDbTable", "FPTextLeveling"),
            //    new SqlParameter("ScoreGrouping", input.ScoreGrouping),
            //    new SqlParameter("TestDueDate", input.TestDueDate)).ToList();


            //return new OutputDto_StackBarGraphSummaryData { TestDueDates = Mapper.Map<List<TestDueDateDto>>(tdds), Status = new OutputDto_Status { StatusCode = StatusCode.Ok }, SummaryRecords = scores };

            // TODO: REFACTOR ASAP

            */

            List<TestDueDate> tdds = null;

            if(assessment.TestType == 3)
            {
                tdds = new List<TestDueDate>() { new TestDueDate() { Id = -1, TestLevelPeriodID = -1, DueDate = DateTime.MaxValue } };   
            }
            else
            {
                tdds = this.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolStartYear).OrderBy(p => p.DueDate).ToList();
            }
            result.TestDueDates = Mapper.Map<List<TestDueDateDto>>(tdds);
            List<StackedBarGraphSummaryRecord> studentResults = new List<StackedBarGraphSummaryRecord>();

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                command.Parameters.Add(new SqlParameter("@SchoolStartYear", input.SchoolStartYear));
                command.Parameters.Add(new SqlParameter("@Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())));
                //command.Parameters.Add(new SqlParameter("@StudentAttributeValues", string.Join<int>(",", attributeValues)));
                command.Parameters.Add(attributeParameterArray[0]);
                command.Parameters.Add(attributeIdParameterArray[0]);
                command.Parameters.Add(attributeParameterArray[1]);
                command.Parameters.Add(attributeIdParameterArray[1]);
                command.Parameters.Add(attributeParameterArray[2]);
                command.Parameters.Add(attributeIdParameterArray[2]);
                command.Parameters.Add(attributeParameterArray[3]);
                command.Parameters.Add(attributeIdParameterArray[3]);
                command.Parameters.Add(attributeParameterArray[4]);
                command.Parameters.Add(attributeIdParameterArray[4]);
                command.Parameters.Add(attributeParameterArray[5]);
                command.Parameters.Add(attributeIdParameterArray[5]);
                command.Parameters.Add(attributeParameterArray[6]);
                command.Parameters.Add(attributeIdParameterArray[6]);
                command.Parameters.Add(attributeParameterArray[7]);
                command.Parameters.Add(attributeIdParameterArray[7]);
                command.Parameters.Add(attributeParameterArray[8]);
                command.Parameters.Add(attributeIdParameterArray[8]);
                command.Parameters.Add(attributeParameterArray[9]);
                command.Parameters.Add(attributeIdParameterArray[9]);
                command.Parameters.Add(new SqlParameter("@InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@AssessmentID", assessment.Id));
                command.Parameters.Add(new SqlParameter("@FieldToRetrieve", input.AssessmentField.DatabaseColumn));
                command.Parameters.Add(new SqlParameter("@IsDecimalField", false));
                command.Parameters.Add(new SqlParameter("@TestDbTable", assessment.StorageTable));
                command.Parameters.Add(new SqlParameter("@ScoreGrouping", input.ScoreGrouping));
                command.Parameters.Add(new SqlParameter("@TestDueDate", input.TestDueDate == null ? DateTime.Now : input.TestDueDate));
                try
                {
                    var procSuffix = assessment.TestType == 3 ? "_YearlyAssessment" : "";

                    Database.Connection.Open();
                    command.CommandText = String.Format("_ns4_report_FilteredStackedBarGraph_Summary{0}", procSuffix);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;


                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            var resultTddId = Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString());
                            var resultPeriodId = Int32.Parse(dt.Rows[i]["TestLevelPeriodID"].ToString());
                            var studentResult = studentResults.FirstOrDefault(p => p.StudentID == studentId);
                            // first make sure this student isn't already added, if so, just add to TDD list
                            if (studentResult == null)
                            {
                                studentResult = new StackedBarGraphSummaryRecord();
                                studentResult.StudentID = studentId;
                                studentResult.StudentIdentifier = dt.Rows[i]["StudentIdentifier"] != DBNull.Value ? dt.Rows[i]["StudentIdentifier"].ToString() : "";
                                studentResult.Services = dt.Rows[i]["Services"] != DBNull.Value ? dt.Rows[i]["Services"].ToString() : "";
                                studentResult.SpecialED = dt.Rows[i]["SpecialED"] != DBNull.Value ? dt.Rows[i]["SpecialED"].ToString() : "";
                                studentResult.Att1 = dt.Rows[i]["Att1"] != DBNull.Value ? dt.Rows[i]["Att1"].ToString() : "";
                                studentResult.Att2 = dt.Rows[i]["Att2"] != DBNull.Value ? dt.Rows[i]["Att2"].ToString() : "";
                                studentResult.Att3 = dt.Rows[i]["Att3"] != DBNull.Value ? dt.Rows[i]["Att3"].ToString() : "";
                                studentResult.Att4 = dt.Rows[i]["Att4"] != DBNull.Value ? dt.Rows[i]["Att4"].ToString() : "";
                                studentResult.Att5 = dt.Rows[i]["Att5"] != DBNull.Value ? dt.Rows[i]["Att5"].ToString() : "";
                                studentResult.Att6 = dt.Rows[i]["Att6"] != DBNull.Value ? dt.Rows[i]["Att6"].ToString() : "";
                                studentResult.Att7 = dt.Rows[i]["Att7"] != DBNull.Value ? dt.Rows[i]["Att7"].ToString() : "";
                                studentResult.Att8 = dt.Rows[i]["Att8"] != DBNull.Value ? dt.Rows[i]["Att8"].ToString() : "";
                                studentResult.Att9 = dt.Rows[i]["Att9"] != DBNull.Value ? dt.Rows[i]["Att9"].ToString() : "";
                                studentResult.School = dt.Rows[i]["School"] != DBNull.Value ? dt.Rows[i]["School"].ToString() : "";
                                studentResult.Grade = dt.Rows[i]["Grade"] != DBNull.Value ? dt.Rows[i]["Grade"].ToString() : "";
                                studentResult.GradeOrder = dt.Rows[i]["GradeOrder"] != DBNull.Value ? Int32.Parse(dt.Rows[i]["GradeOrder"].ToString()) : 0;
                                studentResult.HomeLanguage = dt.Rows[i]["HomeLanguage"] != DBNull.Value ? dt.Rows[i]["HomeLanguage"].ToString() : "";
                                studentResult.Teacher = dt.Rows[i]["Teacher"] != DBNull.Value ? dt.Rows[i]["Teacher"].ToString() : "";
                                studentResult.Student = dt.Rows[i]["Student"].ToString();
                                studentResult.SchoolsAndSections = dt.Rows[i]["SchoolsAndSections"].ToString(); //TODO: watch out for this field;
                                studentResult.TestDueDateID = resultTddId;
                                studentResults.Add(studentResult);

                                // add a resultbytdd for each tdd
                                foreach (var tdd in result.TestDueDates)
                                {
                                    studentResult.ResultsByTDD.Add(new SummaryResultByTDD { TDDID = tdd.Id, PeriodId = tdd.TestLevelPeriodID.Value }); // TODO: should be INT, not int?
                                }
                            }

                            //if (studentResult.ResultsByTDD.FirstOrDefault(p => p.TDDID == resultTddId) == null)
                            //{
                            //    studentResult.ResultsByTDD.Add(new SummaryResultByTDD { TDDID = resultTddId, PeriodId = resultPeriodId });
                            //}

                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();


                                    var currentTdResult = studentResult.ResultsByTDD.First(p => p.TDDID == resultTddId);
                                    currentTdResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    SetFieldResultValueBasedOnType(fieldResult, field, dt.Rows[i]);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            result.SummaryRecords = studentResults.OrderBy(p => p.Student).ToList();
            result.Status = new OutputDto_Status { StatusCode = StatusCode.Ok };
            return result; ; 
        }

        public OutputDto_StackBarGraphHistoricalSummaryData GetStackedBarGraphGroupHistoricalSummary(InputDto_GetStackedBarGraphGroupingSummaryUpdatedOptions input, int staffId)
        {
            var result = new OutputDto_StackBarGraphHistoricalSummaryData();

            // check if we should bother or not
            if (input.AssessmentField == null || input.AssessmentField.AssessmentId == 0)
            {
                return result;
            }

            // get all the attributes
            var allAttributes = this.StudentAttributeTypes.Where(p => p.Id != 4).OrderBy(p => p.Id).ToList();

            for (var i = 0; i < allAttributes.Count; i++)
            {
                var Id = allAttributes[i].Id;
                switch (i)
                {
                    case 0:
                        result.Att1Header = allAttributes[i].AttributeName;
                        result.Att1Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 1:
                        result.Att2Header = allAttributes[i].AttributeName;
                        result.Att2Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 2:
                        result.Att3Header = allAttributes[i].AttributeName;
                        result.Att3Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 3:
                        result.Att4Header = allAttributes[i].AttributeName;
                        result.Att4Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 4:
                        result.Att5Header = allAttributes[i].AttributeName;
                        result.Att5Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 5:
                        result.Att6Header = allAttributes[i].AttributeName;
                        result.Att6Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 6:
                        result.Att7Header = allAttributes[i].AttributeName;
                        result.Att7Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 7:
                        result.Att8Header = allAttributes[i].AttributeName;
                        result.Att8Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 8:
                        result.Att9Header = allAttributes[i].AttributeName;
                        result.Att9Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                }
            }

            // combine the student attributevalues into a single list
            //List<int> attributeValues = new List<int>();
            //attributeValues.AddRange(input.TitleOneTypes.Select(p => p.id).ToList());
            //attributeValues.AddRange(input.Ethnicities.Select(p => p.id).ToList());
            //attributeValues.AddRange(input.EducationLabels.Select(p => p.id).ToList());
            Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
            var attributeParameterArray = new SqlParameter[10];
            var attributeIdParameterArray = new SqlParameter[10];

            // hackfix, empty params
            for (var i = 0; i < 10; i++)
            {
                attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
            }

            // combine the student attributevalues into a single list
            List<string> attributeValues = new List<string>();
            foreach (var attributeType in input.DropdownDataList)
            {
                // hackfix, and yes, I did regret it... thanks jerkface
                attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
            }

            var assessment = this.Assessments.Include(p => p.Fields).First(p => p.Id == input.AssessmentField.AssessmentId);
            IEnumerable<AssessmentField> fieldsToRetrieve = null;
            fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInObsSummary == true && p.DisplayInStackedBarGraphSummary == true);
            result.Fields = Mapper.Map<List<AssessmentFieldDto>>(fieldsToRetrieve);
            //attributeValues.AddRange(input..Select(p => p.id).ToList());

            var tdds = this.TestDueDates.Where(p => p.SchoolStartYear == input.SchoolStartYear).OrderBy(p => p.DueDate).ToList();
            result.TestDueDates = Mapper.Map<List<TestDueDateDto>>(tdds);
            List<StackedBarGraphHistoricalSummaryRecord> studentResults = new List<StackedBarGraphHistoricalSummaryRecord>();

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();
                command.Parameters.Add(new SqlParameter("@SchoolStartYear", input.SchoolStartYear));
                command.Parameters.Add(new SqlParameter("@Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())));
                //command.Parameters.Add(new SqlParameter("@StudentAttributeValues", string.Join<int>(",", attributeValues)));
                command.Parameters.Add(attributeParameterArray[0]);
                command.Parameters.Add(attributeIdParameterArray[0]);
                command.Parameters.Add(attributeParameterArray[1]);
                command.Parameters.Add(attributeIdParameterArray[1]);
                command.Parameters.Add(attributeParameterArray[2]);
                command.Parameters.Add(attributeIdParameterArray[2]);
                command.Parameters.Add(attributeParameterArray[3]);
                command.Parameters.Add(attributeIdParameterArray[3]);
                command.Parameters.Add(attributeParameterArray[4]);
                command.Parameters.Add(attributeIdParameterArray[4]);
                command.Parameters.Add(attributeParameterArray[5]);
                command.Parameters.Add(attributeIdParameterArray[5]);
                command.Parameters.Add(attributeParameterArray[6]);
                command.Parameters.Add(attributeIdParameterArray[6]);
                command.Parameters.Add(attributeParameterArray[7]);
                command.Parameters.Add(attributeIdParameterArray[7]);
                command.Parameters.Add(attributeParameterArray[8]);
                command.Parameters.Add(attributeIdParameterArray[8]);
                command.Parameters.Add(attributeParameterArray[9]);
                command.Parameters.Add(attributeIdParameterArray[9]);
                command.Parameters.Add(new SqlParameter("@InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())));
                command.Parameters.Add(new SqlParameter("@AssessmentID", assessment.Id));
                command.Parameters.Add(new SqlParameter("@FieldToRetrieve", input.AssessmentField.DatabaseColumn));
                command.Parameters.Add(new SqlParameter("@IsDecimalField", false));
                command.Parameters.Add(new SqlParameter("@TestDbTable", assessment.StorageTable));
                command.Parameters.Add(new SqlParameter("@ScoreGrouping", input.ScoreGrouping));
                command.Parameters.Add(new SqlParameter("@SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value });
                command.Parameters.Add(new SqlParameter("@TestDueDate", input.TestDueDate == null ? DateTime.Now : input.TestDueDate));
                try
                {
                    Database.Connection.Open();
                    command.CommandText = "_ns4_report_FilteredStackedBarGraph_Summary_Historical";
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;


                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            var studentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            var resultTddId = Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString());
                            var section = dt.Rows[i]["Section"] != DBNull.Value ? dt.Rows[i]["Section"].ToString() : "";
                            var resultPeriodId = Int32.Parse(dt.Rows[i]["TestLevelPeriodID"].ToString());
                            var studentResult = studentResults.FirstOrDefault(p => p.StudentID == studentId && p.Section == section);
                            // first make sure this student and section isn't already added, if so, just add to TDD list
                            if (studentResult == null)
                            {
                                studentResult = new StackedBarGraphHistoricalSummaryRecord();
                                studentResult.StudentID = studentId;
                                studentResult.StudentIdentifier = dt.Rows[i]["StudentIdentifier"] != DBNull.Value ? dt.Rows[i]["StudentIdentifier"].ToString() : "";
                                studentResult.Services = dt.Rows[i]["Services"] != DBNull.Value ? dt.Rows[i]["Services"].ToString() : "";
                                studentResult.SpecialED = dt.Rows[i]["SpecialED"] != DBNull.Value ? dt.Rows[i]["SpecialED"].ToString() : "";
                                studentResult.Att1 = dt.Rows[i]["Att1"] != DBNull.Value ? dt.Rows[i]["Att1"].ToString() : "";
                                studentResult.Att2 = dt.Rows[i]["Att2"] != DBNull.Value ? dt.Rows[i]["Att2"].ToString() : "";
                                studentResult.Att3 = dt.Rows[i]["Att3"] != DBNull.Value ? dt.Rows[i]["Att3"].ToString() : "";
                                studentResult.Att4 = dt.Rows[i]["Att4"] != DBNull.Value ? dt.Rows[i]["Att4"].ToString() : "";
                                studentResult.Att5 = dt.Rows[i]["Att5"] != DBNull.Value ? dt.Rows[i]["Att5"].ToString() : "";
                                studentResult.Att6 = dt.Rows[i]["Att6"] != DBNull.Value ? dt.Rows[i]["Att6"].ToString() : "";
                                studentResult.Att7 = dt.Rows[i]["Att7"] != DBNull.Value ? dt.Rows[i]["Att7"].ToString() : "";
                                studentResult.Att8 = dt.Rows[i]["Att8"] != DBNull.Value ? dt.Rows[i]["Att8"].ToString() : "";
                                studentResult.Att9 = dt.Rows[i]["Att9"] != DBNull.Value ? dt.Rows[i]["Att9"].ToString() : "";
                                studentResult.Student = dt.Rows[i]["Student"].ToString();
                                studentResult.Section = section;
                                studentResult.SchoolName = dt.Rows[i]["SchoolName"] != DBNull.Value ? dt.Rows[i]["SchoolName"].ToString() : "";
                                studentResult.TestDueDateID = resultTddId;
                                studentResults.Add(studentResult);

                            }

                            if (studentResult.ResultsByTDD.FirstOrDefault(p => p.TDDID == resultTddId) == null)
                            {
                                studentResult.ResultsByTDD.Add(new SummaryResultByTDD { TDDID = resultTddId, PeriodId = resultPeriodId });
                            }

                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();


                                    var currentTdResult = studentResult.ResultsByTDD.First(p => p.TDDID == resultTddId);
                                    currentTdResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    SetFieldResultValueBasedOnType(fieldResult, field, dt.Rows[i]);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            result.SummaryRecords = studentResults.OrderBy(p => p.Student).ToList();
            result.Status = new OutputDto_Status { StatusCode = StatusCode.Ok };
            return result; ;
        }

        public void SetFieldResultValueBasedOnType(AssessmentFieldResult fieldResult, AssessmentField field, DataRow row)
        {
            switch (field.FieldType)
            {
                case "Textfield":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "DecimalRange":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.DecimalValue = Decimal.Parse(row[field.DatabaseColumn].ToString());
                    }
                    break;
                case "DropdownRange":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.IntValue = Int32.Parse(row[field.DatabaseColumn].ToString());
                    }
                    break;
                case "DropdownFromDB":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.IntValue = Int32.Parse(row[field.DatabaseColumn].ToString());
                    }
                    break;
                case "checklist":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "CalculatedFieldDbBacked":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.IntValue = Int32.Parse(row[field.DatabaseColumn].ToString());
                    }
                    break;
                case "CalculatedFieldDbBackedString":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "Checkbox":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.BoolValue = Boolean.Parse(row[field.DatabaseColumn].ToString());
                    }
                    break;
                case "CalculatedFieldDbOnly":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "CalculatedFieldClientOnly":
                    // no-op
                    break;
                default:
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
            }
        }

        public void SetFieldDisplayValueBasedOnType(AssessmentFieldResultDisplayOnly fieldResult, AssessmentField field, DataRow row)
        {
            switch (field.FieldType)
            {
                case "Textfield":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "DecimalRange":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = String.Format("{0:0.#}", Decimal.Parse(row[field.DatabaseColumn].ToString()));
                    }
                    break;
                case "DropdownRange":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = Int32.Parse(row[field.DatabaseColumn].ToString()).ToString();
                    }
                    break;
                case "DropdownFromDB":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        var lookupId = Int32.Parse(row[field.DatabaseColumn].ToString());
                        fieldResult.StringValue = LookupFields.FirstOrDefault(p => p.FieldSpecificId == lookupId && p.FieldName == field.LookupFieldName)?.FieldValue;
                    }
                    break;
                case "checklist":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        var joinedResult = new List<string>();
                        if (!String.IsNullOrEmpty(row[field.DatabaseColumn].ToString()))
                        {
                            var arySplit = row[field.DatabaseColumn].ToString().Split(Char.Parse(","));
                            foreach (var stringInt in arySplit)
                            {
                                joinedResult.Add(LookupFields.FirstOrDefault(p => p.FieldSpecificId == Int32.Parse(stringInt) && p.FieldName == field.LookupFieldName)?.FieldValue);
                            }
                        }
                        fieldResult.StringValue = String.Join(",", joinedResult);
                    }
                    break;
                case "CalculatedFieldDbBacked":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = Int32.Parse(row[field.DatabaseColumn].ToString()).ToString();
                    }
                    break;
                case "CalculatedFieldDbBackedString":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "Checkbox":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = Boolean.Parse(row[field.DatabaseColumn].ToString()).ToString();
                    }
                    break;
                case "CalculatedFieldDbOnly":
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
                case "CalculatedFieldClientOnly":
                    // no-op
                    break;
                default:
                    if (row[field.DatabaseColumn] != DBNull.Value)
                    {
                        fieldResult.StringValue = row[field.DatabaseColumn].ToString();
                    }
                    break;
            }
        }

        private SqlParameter CreateNullableVarCharSqlParameter(string paramValue, string paramName)
        {
            SqlParameter newParam = new SqlParameter();
            newParam.SqlDbType = SqlDbType.VarChar;
            newParam.ParameterName = paramName;

            if (paramValue == null)
            {
                newParam.Value = DBNull.Value;
            }
            else
            {
                newParam.Value = paramValue;
            }

            return newParam;
        }
        private SqlParameter CreateNullableIntSqlParameter(int? paramValue, string paramName)
        {
            SqlParameter newParam = new SqlParameter();
            newParam.SqlDbType = SqlDbType.Int;
            newParam.ParameterName = paramName;

            if (paramValue == null)
            {
                newParam.Value = DBNull.Value;
            }
            else
            {
                newParam.Value = paramValue;
            }

            return newParam;
        }


        /// <summary>
        /// This is used for data entry screens and returns the data for a single TDD
        /// </summary>
        /// <param name="assessment"></param>
        /// <param name="classId"></param>
        /// <param name="benchmarkDateId"></param>
        /// <param name="testDate"></param>
        /// <returns></returns>
        public List<AssessmentStudentResult> GetAssessmentStudentResults(Assessment assessment, int classId,
            int benchmarkDateId, DateTime testDate, bool summaryFieldsOnly)
        {
            List<AssessmentStudentResult> lstStudentData = new List<AssessmentStudentResult>();

            try
            {
                
                var results = Database.DynamicSqlQuery(String.Format("exec ns4_GetAssessmentStudentResults '{0}',{1},{2}", assessment.StorageTable, classId, benchmarkDateId), assessment, this, summaryFieldsOnly, false);
                lstStudentData = results;

                return lstStudentData;
            } catch(Exception ex)
            {
                Log.Error("Trapping GetAssessmentStudentResultsException -- Assessment: {0}, ClassId: {1}, BenchmarkDateId: {2}, TestDate: {3}, Message: {4}", assessment, classId, benchmarkDateId, testDate, ex.Message);
                return lstStudentData;
            }
        }

        /// <summary>
        /// This is used for data entry screens and returns the data for a single TDD
        /// </summary>
        /// <param name="assessment"></param>
        /// <param name="classId"></param>
        /// <param name="benchmarkDateId"></param>
        /// <param name="testDate"></param>
        /// <returns></returns>
        public List<AssessmentStudentResult> GetIGAssessmentStudentResults(Assessment assessment, int interventionGroupId, int studentId)
        {
            List<AssessmentStudentResult> lstStudentData = new List<AssessmentStudentResult>();

            // TODO: Incorporate StudentID

            var results = Database.DynamicSqlQuery(String.Format("SELECT s.ID as StudentID, s.StudentIdentifier, s.FirstName, s.LastName, s.MiddleName, t.*, {1} as InputSectionId FROM  Student s INNER JOIN {0} t ON s.ID = t.StudentID AND t.InterventionGroupId = {1} WHERE t.StudentID = {2} ORDER BY t.TestDueDate DESC", assessment.StorageTable, interventionGroupId, studentId), assessment, this, true, true);
            lstStudentData = results;

            return lstStudentData;
        }

        public void GetAllStudentsAssesmentResultsForLetterIdUpdate(Assessment assessment)
        {
            var numRows = 0;
            string letterIdCount = "select count(*) from data_LetterId";
            // Define the ADO.NET Objects
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {

                    Database.Connection.Open();
                    command.CommandText = letterIdCount;
                    numRows = (int)command.ExecuteScalar();
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }


            var sql = string.Empty;

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                for (var r = 0; r <= (numRows / 500); r++)
                {
                    try
                    {

                        sql = String.Format("SELECT * from data_LetterId ORDER BY ID ASC OFFSET {0} ROWS FETCH NEXT 500 ROWS ONLY", r * 500);


                        Database.Connection.Open();
                        command.CommandText = sql;
                        command.CommandTimeout = command.Connection.ConnectionTimeout;


                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            // load datatable
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            for (int i = 0; i < dt.Rows.Count; i++) // TODO: Make this a proc or something, this is stupid 
                            {
                                AssessmentStudentResult studentResult = new AssessmentStudentResult();

                                studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                                studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
                                studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
                                studentResult.ClassId = Int32.Parse(dt.Rows[i]["SectionId"].ToString()); //result.GetPropValue<int>("SectionID");
                                studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                                studentResult.Recorder.id = studentResult.StaffId.HasValue ? studentResult.StaffId.Value : -1;
                                studentResult.TestDate = (dt.Rows[i]["DateTestTaken"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DateTestTaken"].ToString()) : (DateTime?)null;

                                int fieldIndex = 0;
                                IEnumerable<AssessmentField> fieldsToRetrieve = null;
   
                                fieldsToRetrieve = assessment.Fields;

                                foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                                {
                                    if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                    {
                                        AssessmentFieldResult fieldResult = new AssessmentFieldResult();
                                        studentResult.FieldResults.Add(fieldResult);
                                        fieldResult.DbColumn = field.DatabaseColumn;
                                        fieldResult.FieldIndex = fieldIndex;
                                        switch (field.FieldType)
                                        {
                                            case "Textfield":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                                }
                                                break;
                                            case "DecimalRange":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                                }
                                                break;
                                            case "DropdownRange":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                                }
                                                break;
                                            case "DropdownFromDB":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                                }
                                                break;
                                            case "checklist":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                                }
                                                break;
                                            case "CalculatedFieldDbBacked":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                                }
                                                break;
                                            case "CalculatedFieldDbBackedString":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                                }
                                                break;
                                            case "Checkbox":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                                }
                                                break;
                                            case "CalculatedFieldDbOnly":
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                                }
                                                break;
                                            case "CalculatedFieldClientOnly":
                                                // no-op
                                                break;
                                            default:
                                                if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                                {
                                                    fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                                }
                                                break;
                                        }
                                    }
                                    fieldIndex++;
                                }
                                SaveLIDBulkAssessmentResult(assessment, studentResult);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Log.Error("Error while processing Letter ID updates: {0}", ex.Message);
                    }
                    finally
                    {
                        Database.Connection.Close();
                        command.Parameters.Clear();
                    }
                }
            }
        }

        private void SaveLIDBulkAssessmentResult(Assessment assessment, AssessmentStudentResult input)
        {
            var insertUpdateSql = new StringBuilder();


            using (System.Data.IDbCommand command = this.Database.Connection.CreateCommand())
            {
                try
                {
                    // connection should already be open
                    //this.Database.Connection.Open();
                    insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);


                    // don't include fields that we don't have fields for
                    foreach (var field in input.FieldResults)
                    {
                        // don't try to update fields that don't have a dbcolumn
                        var control = assessment.Fields.FirstOrDefault(p => p.DatabaseColumn == field.DbColumn);
                        if (control != null && control.FieldType != "CalculatedFieldClientOnly")
                        {
                            if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
                            {
                                insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, input));
                            }
                        }
                    }
                    // remove trailing comma
                    insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
                    insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND SectionID = {1} and TestDueDateId = {2}", input.StudentId, input.ClassId, input.TestDueDateId); // or testdate = {4}

                    command.CommandText = insertUpdateSql.ToString();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Error("Error saving LID Data: {0}", ex.Message);
                    //throw ex;
                }
            }
        }

        public AssessmentStudentResult GetStudentAssesmentResult(Assessment assessment, int classId,
    int benchmarkDateId, DateTime testDate, int studentId, bool editModeFieldsOnly)
        {
            AssessmentStudentResult studentResult = new AssessmentStudentResult();

            var sql = String.Format("SELECT TOP 1 s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, ISNULL(fp.FPText, 'N/A') as FPText, ISNULL(fp.FPValueID, 0) as FPValueSortOrder, t.*, {1} as InputSectionId FROM  dbo.nset_udf_GetStudentIDsForSummaryPages({1}, '{3}', {2}) a LEFT OUTER JOIN Student s on s.ID = a.StudentID  LEFT OUTER JOIN [{0}] t ON a.StudentID = t.StudentID and t.testduedateid = {2} LEFT OUTER JOIN dbo.nset_udf_GetStudentFPText(NULL, {1}, {2}, NULL) fp ON fp.StudentID = s.ID WHERE s.id = {4} ORDER BY s.LastName, s.FirstName", assessment.StorageTable, classId, benchmarkDateId, DateTime.Now, studentId);

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    Database.Connection.Open();
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;


                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < 1; i++) // TODO: Make this a proc or something, this is stupid 
                        {
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
                            //studentResult.IsCopied = (dt.Rows[i]["IsCopied"] != DBNull.Value) ? Boolean.Parse(dt.Rows[i]["IsCopied"].ToString()) : false;
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.FPText = dt.Rows[i]["FPText"].ToString();
                            studentResult.FPValueID = (dt.Rows[i]["FPValueSortOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FPValueSortOrder"].ToString()) : 0;
                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputSectionId"].ToString()); //result.GetPropValue<int>("SectionID");
                            studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                            studentResult.Recorder.id = studentResult.StaffId.HasValue ? studentResult.StaffId.Value : -1;
                            studentResult.Recorder.text = Staffs.FirstOrDefault(p => p.Id == studentResult.Recorder.id)?.FullName ?? String.Empty;
                            studentResult.TestDate = (dt.Rows[i]["DateTestTaken"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["DateTestTaken"].ToString()) : (DateTime?)null;

                            int fieldIndex = 0;
                            IEnumerable<AssessmentField> fieldsToRetrieve = null;
                            if (editModeFieldsOnly)
                            {
                                fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInEditResultList == true);
                            }
                            else
                            {
                                fieldsToRetrieve = assessment.Fields;
                            }

                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();
                                    studentResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    fieldResult.FieldIndex = fieldIndex;
                                    switch (field.FieldType)
                                    {
                                        case "checklist":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();

                                                // convert list of strings to array of ints
                                                fieldResult.ChecklistValues = fieldResult.StringValue.Split(',').Select(int.Parse).ToList();
                                            }
                                            break;
                                        case "Textfield":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "Checkbox":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                                fieldIndex++;
                            }
                        }
                    }
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return studentResult;
        }

        public AssessmentStudentResult GetStudentProgressMonResult(Assessment assessment, int interventionGroupId, int StudentResultId, int studentId)
        {
            AssessmentStudentResult studentResult = new AssessmentStudentResult();

            var sql = String.Format("SELECT TOP 1 s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, t.*, {1} as InputInterventionGroupId FROM  dbo.StudentInterventionGroup a LEFT OUTER JOIN Student s on s.ID = a.StudentID and a.StudentID =  {3} and a.InterventionGroupID = {1}  LEFT OUTER JOIN {0} t ON a.StudentID = t.StudentID and t.id = {2} WHERE s.id = {3} ORDER BY t.TestDueDate DESC", assessment.StorageTable, interventionGroupId, StudentResultId, studentId);

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                SqlDataAdapter da = new SqlDataAdapter();

                try
                {
                    Database.Connection.Open();
                    command.CommandText = sql;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;


                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < 1; i++) // TODO: Make this a proc or something, this is stupid 
                        {
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["StudentID"].ToString());
                            studentResult.ResultId = (dt.Rows[i]["ID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["ID"].ToString()) : -1;
                            //studentResult.IsCopied = (dt.Rows[i]["IsCopied"] != DBNull.Value) ? Boolean.Parse(dt.Rows[i]["IsCopied"].ToString()) : false;
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            //studentResult.FPText = dt.Rows[i]["FPText"].ToString();
                            //studentResult.FPValueID = (dt.Rows[i]["FPValueSortOrder"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["FPValueSortOrder"].ToString()) : 0;
                            studentResult.StaffId = (dt.Rows[i]["RecorderID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["RecorderID"].ToString()) : -1;
                            studentResult.ClassId = Int32.Parse(dt.Rows[i]["InputInterventionGroupId"].ToString()); //result.GetPropValue<int>("SectionID");
                            //studentResult.TestDueDateId = (dt.Rows[i]["TestDueDateID"] != DBNull.Value) ? Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString()) : -1;//result.GetPropValue<int>("TestDueDateID");
                            studentResult.Recorder.id = studentResult.StaffId.HasValue ? studentResult.StaffId.Value : -1;
                            studentResult.Recorder.text = Staffs.FirstOrDefault(p => p.Id == studentResult.Recorder.id)?.FullName ?? String.Empty;
                            studentResult.TestDate = (dt.Rows[i]["TestDueDate"] != DBNull.Value) ? DateTime.Parse(dt.Rows[i]["TestDueDate"].ToString()) : (DateTime?)null;

                            int fieldIndex = 0;
                            IEnumerable<AssessmentField> fieldsToRetrieve = null;
                            if (false)
                            {
                                fieldsToRetrieve = assessment.Fields.Where(p => p.DisplayInEditResultList == true);
                            }
                            else
                            {
                                fieldsToRetrieve = assessment.Fields;
                            }

                            foreach (var field in fieldsToRetrieve.OrderBy(p => p.FieldOrder))
                            {
                                if (!String.IsNullOrEmpty(field.DatabaseColumn))
                                {
                                    AssessmentFieldResult fieldResult = new AssessmentFieldResult();
                                    studentResult.FieldResults.Add(fieldResult);
                                    fieldResult.DbColumn = field.DatabaseColumn;
                                    fieldResult.FieldIndex = fieldIndex;
                                    switch (field.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.DecimalValue = Decimal.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.IntValue = Int32.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "Checkbox":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.BoolValue = Boolean.Parse(dt.Rows[i][field.DatabaseColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][field.DatabaseColumn] != DBNull.Value)
                                            {
                                                fieldResult.StringValue = dt.Rows[i][field.DatabaseColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                                fieldIndex++;
                            }
                        }
                    }
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return studentResult;
        }
        #endregion

        //public AssessmentStudentResult SaveStudentData(AssessmentStudentResult result, int assessmentId)
        //{
        //    var testduedateid = 374;
        //    var recorderid = 1570;

        //    var assessment = Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == assessmentId);

        //    // first determine if there's already a result for the current student/class/date
        //    var resultExistSql = new StringBuilder();
        //    resultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} and SectionID = {2} and (TestDueDateID = {3})", assessment.StorageTable, result.StudentId.ToString(), result.ClassId.ToString(), testduedateid, result.TestDate.HasValue ? result.TestDate.Value.ToShortDateString() : null);

        //    var insertUpdateSql = new StringBuilder();
        //    bool isInsert = true;

        //    // replace this crap with just a check for the result ID > 0
        //    using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
        //    {
        //        try
        //        {
        //            Database.Connection.Open();
        //            command.CommandText = resultExistSql.ToString();
        //            command.CommandTimeout = command.Connection.ConnectionTimeout;

        //            using (System.Data.IDataReader reader = command.ExecuteReader())
        //            {

        //                if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
        //                {
        //                    isInsert = false;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //        finally
        //        {
        //            Database.Connection.Close();
        //            command.Parameters.Clear();
        //        }
        //    }

        //    using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
        //    {
        //        try
        //        {
        //            Database.Connection.Open();
        //            command.CommandText = resultExistSql.ToString();
        //            command.CommandTimeout = command.Connection.ConnectionTimeout;

        //            // need to detect null on testdate
        //            // also need to update TestDueDateID and Recorder

        //            if (isInsert)
        //            {
        //                insertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, SectionId, RecorderId, TestDueDateId, TestDueDate", assessment.StorageTable);
        //                // for each
        //                foreach (var field in result.FieldResults)
        //                {
        //                    var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
        //                    if (control.FieldType != "CalculatedFieldClientOnly")
        //                    {
        //                        insertUpdateSql.AppendFormat(",{0}", field.DbColumn);
        //                    }
        //                }
        //                insertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}, {3}, '{4}'", result.StudentId, result.ClassId, recorderid, testduedateid, result.TestDate);
        //                //for each
        //                foreach (var field in result.FieldResults)
        //                {
        //                    var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
        //                    if (control.FieldType != "CalculatedFieldClientOnly")
        //                    {
        //                        if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
        //                        {
        //                            insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, result));

        //                        }
        //                        else
        //                        {
        //                            insertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

        //                        }
        //                    }
        //                }
        //                insertUpdateSql.AppendFormat(")");
        //            }
        //            else
        //            {
        //                insertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.StorageTable);
        //                // don't include fields that we don't have fields for
        //                foreach (var field in result.FieldResults)
        //                {
        //                    // don't try to update fields that don't have a dbcolumn
        //                    var control = assessment.Fields.FirstOrDefault(p => p.DatabaseColumn == field.DbColumn);
        //                    if (control != null && control.FieldType != "CalculatedFieldClientOnly")
        //                    {
        //                        if (control.FieldType == "CalculatedFieldDbBacked" || control.FieldType == "CalculatedFieldDbOnly" || control.FieldType == "CalculatedFieldDbBackedString")
        //                        {
        //                            insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateStringCalculatedFields(assessment, field, control, result));
        //                        }
        //                        else
        //                        {
        //                            insertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
        //                        }
        //                    }
        //                }
        //                // remove trailing comma
        //                insertUpdateSql.Remove(insertUpdateSql.Length - 1, 1);
        //                insertUpdateSql.AppendFormat(" WHERE StudentId = {0} AND SectionID = {1} and TestDueDateId = {2}", result.StudentId, result.ClassId, testduedateid); // or testdate = {4}
        //            }

        //            command.CommandText = insertUpdateSql.ToString();
        //            command.ExecuteNonQuery();
        //        }
        //        catch (Exception ex)
        //        {
        //            //log
        //            throw ex;
        //        }
        //        finally
        //        {
        //            Database.Connection.Close();
        //            command.Parameters.Clear();
        //        }
        //    }

        //    return result;

        //}

        public AssessmentHFWStudentResult SaveHFWStudentData(AssessmentHFWStudentResult result, int assessmentId, int recorderid)
        {
            var assessment = Assessments.Include(p => p.Fields).Include(p => p.FieldGroups).Include(p => p.FieldCategories).Include(p => p.FieldSubCategories).First(p => p.Id == assessmentId);

            // first determine if there's already a result for the current student/class/date
            var readResultExistSql = new StringBuilder();
            readResultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} ", assessment.StorageTable, result.StudentId.ToString());

            var readInsertUpdateSql = new StringBuilder();
            bool isReadInsert = true;
            bool doReadChange = false;

            // should not need to do this
            //var writeResultExistSql = new StringBuilder();
            //writeResultExistSql.AppendFormat("SELECT * FROM {0} WHERE StudentID = {1} ", assessment.TertiaryStorageTable, result.StudentId.ToString());

            var writeInsertUpdateSql = new StringBuilder();
            bool doWriteChange = false;
            //bool isWriteInsert = true;

            // replace this crap with just a check for the result ID > 0
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    Database.Connection.Open();
                    command.CommandText = readResultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (((System.Data.SqlClient.SqlDataReader)(reader)).HasRows)
                        {
                            isReadInsert = false;
                        } 
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error Updating HFW Data: {0}", ex.Message);
                    throw ex;
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    Database.Connection.Open();
                    command.CommandText = readResultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    var comments = result.TotalFieldResults.FirstOrDefault(p => p.DbColumn == "comments")?.StringValue;
                    if (!String.IsNullOrEmpty(comments))
                    {
                        comments = comments.Replace("'", "''");
                    }

                    if (!isReadInsert)
                    {
                        command.CommandText = String.Format("UPDATE {0} SET Comments = '{2}', RecorderId = {3}, SectionId = {4} WHERE StudentID = {1}", assessment.StorageTable, result.StudentId, comments, recorderid, result.ClassId);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // insert a record for the total table
                        var totalInsertSql = new StringBuilder();
                        totalInsertSql.AppendFormat("INSERT INTO {0} (StudentId,  SectionId, RecorderId, comments) VALUES ({1},{2},{3},'{4}')", assessment.StorageTable, result.StudentId, result.ClassId, recorderid, comments);
                        command.CommandText = totalInsertSql.ToString();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error Updating HFW Data: {0}", ex.Message);
                    throw ex;
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    Database.Connection.Open();
                    command.CommandText = readResultExistSql.ToString();
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    // need to detect null on testdate
                    // also need to update TestDueDateID and Recorder

                    if (isReadInsert)
                    {
                        readInsertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId,  SectionId, RecorderId ", assessment.SecondaryStorageTable);
                        // for each
                        foreach (var field in result.ReadFieldResults.Where(p => p.IsModified))
                        {
                            doReadChange = true;
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

                            readInsertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                        }
                        readInsertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}", result.StudentId, result.ClassId, recorderid);
                        //for each
                        foreach (var field in result.ReadFieldResults.Where(p => p.IsModified))
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

                            readInsertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                        }
                        readInsertUpdateSql.AppendFormat(")");

                        writeInsertUpdateSql.AppendFormat("INSERT INTO {0} (StudentId, SectionId, RecorderId ", assessment.TertiaryStorageTable);
                        // for each
                        foreach (var field in result.WriteFieldResults.Where(p => p.IsModified))
                        {
                            doWriteChange = true;
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

                            writeInsertUpdateSql.AppendFormat(",{0}", field.DbColumn);
                        }
                        writeInsertUpdateSql.AppendFormat(") VALUES ({0}, {1}, {2}", result.StudentId, result.ClassId, recorderid);
                        //for each
                        foreach (var field in result.WriteFieldResults.Where(p => p.IsModified))
                        {
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);

                            writeInsertUpdateSql.AppendFormat(",{0}", GetFieldInsertUpdateString(field, control.FieldType));

                        }
                        writeInsertUpdateSql.AppendFormat(")");
                    }
                    else
                    {
                        readInsertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.SecondaryStorageTable);
                        // don't include fields that we don't have fields for
                        foreach (var field in result.ReadFieldResults.Where(p => p.IsModified))
                        {
                            doReadChange = true;
                            // don't try to update fields that don't have a dbcolumn
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            readInsertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
                        }
                        // remove trailing comma
                        readInsertUpdateSql.Remove(readInsertUpdateSql.Length - 1, 1);
                        readInsertUpdateSql.AppendFormat(" WHERE StudentId = {0}", result.StudentId); // or testdate = {4}

                        writeInsertUpdateSql.AppendFormat("UPDATE {0} SET ", assessment.TertiaryStorageTable);
                        // don't include fields that we don't have fields for
                        foreach (var field in result.WriteFieldResults.Where(p => p.IsModified))
                        {
                            doWriteChange = true;
                            // don't try to update fields that don't have a dbcolumn
                            var control = assessment.Fields.First(p => p.DatabaseColumn == field.DbColumn);
                            writeInsertUpdateSql.AppendFormat("{0} = {1},", field.DbColumn, GetFieldInsertUpdateString(field, control.FieldType));
                        }
                        // remove trailing comma
                        writeInsertUpdateSql.Remove(writeInsertUpdateSql.Length - 1, 1);
                        writeInsertUpdateSql.AppendFormat(" WHERE StudentId = {0}", result.StudentId); // or testdate = {4}
                    }

                    // TODO: Make this a transaction
                    if (doReadChange || isReadInsert)
                    {
                        command.CommandText = readInsertUpdateSql.ToString();
                        command.ExecuteNonQuery();
                    }
                    if (doWriteChange || isReadInsert)
                    {
                        command.CommandText = writeInsertUpdateSql.ToString();
                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    //log
                    throw ex;
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return result;

        }

        //public List<OutputDto_StudentQuickSearch> GetStudentQuickSearchResults(InputDto_StudentQuickSearch input)
        //{
        //    List<OutputDto_StudentQuickSearch> results = new List<OutputDto_StudentQuickSearch>();

        //    using (System.Data.IDbCommand command = Database.AsSqlServer().Connection.DbConnection.CreateCommand())
        //    {
        //        try
        //        {
        //            //Database.AsSqlServer().sq
        //            //Database.AsRelational().s
        //            Database.AsSqlServer().Connection.DbConnection.Open();
        //            command.CommandText =
        //                String.Format(
        //                    "EXEC dbo.nset_StudentQuickSearch @StaffID='163',@SearchString='{0}',@IsSysAdmin=1,@RequiresSchool=0,@RequiresClass=0,@SchoolYear=2014,@IsInterventionGroup=0",
        //                    input.searchString);
        //            command.CommandType = CommandType.Text;
        //            command.CommandTimeout = command.Connection.ConnectionTimeout;

        //            using (System.Data.IDataReader reader = command.ExecuteReader())
        //            {
        //                // load datatable
        //                DataTable dt = new DataTable();
        //                dt.Load(reader);

        //                for (int i = 0; i < dt.Rows.Count; i++)
        //                {
        //                    OutputDto_StudentQuickSearch result = new OutputDto_StudentQuickSearch();
        //                    result.StudentId = Int32.Parse(dt.Rows[i]["ID"].ToString());
        //                    result.FirstName = dt.Rows[i]["FirstName"].ToString();
        //                    result.LastName = dt.Rows[i]["LastName"].ToString();
        //                    result.StudentIdentifier = dt.Rows[i]["StudentIdentifier"].ToString();
        //                    result.CurrentSchoolName = dt.Rows[i]["SchoolName"].ToString();
        //                    result.CurrentTeacherName = dt.Rows[i]["TeacherName"].ToString();
        //                    result.CurrentSectionName = dt.Rows[i]["ClassName"].ToString();
        //                    results.Add(result);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //        finally
        //        {
        //            Database.AsSqlServer().Connection.DbConnection.Close();
        //            command.Parameters.Clear();
        //        }
        //    }

        //    return results;
        //}

        public string GetFieldInsertUpdateString(AssessmentFieldResult field, string controlType)
        {
            switch (controlType)
            {
                case "DropdownRange":
                    return field.IntValue?.ToString() ?? "null";
                case "Textfield":
                case "Textarea":
                case "checklist":
                    return String.Format("'{0}'", field.StringValue == null ? String.Empty : field.StringValue.Replace("'", "''"));
                case "Checkbox":
                    return String.Format("{0}", field.BoolValue.HasValue ? (field.BoolValue.Value == true ? 1 : 0) : 0);
                case "DecimalRange":
                    return field.DecimalValue?.ToString() ?? "null";
                case "DropdownFromDB":
                    return field.IntValue?.ToString() ?? "null";
                case "CalculatedFieldDbBacked":
                    return field.IntValue?.ToString() ?? "null";
                case "CalculatedFieldDbBackedString":
                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
                case "CalculatedFieldDbOnly":
                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
                case "DateCheckbox":
                case "Date":
                    // if null, set null
                    return String.Format("{0}", field.DateValue.HasValue ? "'" + field.DateValue.Value.ToShortDateString() + "'" : "null");
                default:
                    return String.Format("'{0}'", field.StringValue.Replace("'", "''"));
            }
        }

        public string GetFieldInsertUpdateStringCalculatedFields(Assessment assessment, AssessmentFieldResult field, AssessmentField asmntField, AssessmentStudentResult result)
        {
            switch (asmntField.FieldType)
            {
                case "CalculatedFieldDbBacked":
                case "CalculatedFieldDbBackedString":
                    int sum = 0;

                    if (asmntField.CalculationFunction == "Sum")
                    {
                        var fieldsToSum = asmntField.CalculationFields.Split(Char.Parse(","));

                        foreach (var currentResult in result.FieldResults)
                        {
                            foreach (var fieldNameToSum in fieldsToSum)
                            {
                                if (currentResult.DbColumn.Trim().ToLower() == fieldNameToSum.Trim().ToLower())
                                {
                                    sum += currentResult.IntValue ?? 0;
                                }
                            }
                        }
                        field.IntValue = sum;
                        return sum.ToString();
                    }
                    if (asmntField.CalculationFunction == "SumBool")
                    {
                        var fieldsToSum = asmntField.CalculationFields.Split(Char.Parse(","));

                        foreach (var currentResult in result.FieldResults)
                        {
                            foreach (var fieldNameToSum in fieldsToSum)
                            {
                                if (currentResult.DbColumn.Trim().ToLower() == fieldNameToSum.Trim().ToLower())
                                {
                                    sum += currentResult.BoolValue.HasValue ? (currentResult.BoolValue.Value ? 1 : 0) : 0;
                                }
                            }
                        }
                        field.IntValue = sum;
                        return sum.ToString();
                    }
                    if (asmntField.CalculationFunction == "SumBoolByGroup")
                    {
                        foreach (var group in assessment.FieldGroups)
                        {
                            var groupId = group.Id;

                            // i don't like having this reference to the field... need to figure out
                            // if it makes more sense to pass the additional data for each field
                            // or to just join them on the client
                            foreach (var currentResult in result.FieldResults)
                            {
                                // fix this later so that field properties are part of the fieldresults
                                // it is really getting stupid to keep looking this up
                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == currentResult.DbColumn);

                                if (currentField.GroupId == groupId && currentResult.DbColumn.Substring(0, 3) == "chk")
                                {
                                    // only add each groupid once

                                    if (currentResult.BoolValue.Value)
                                    {
                                        sum++;
                                        break;
                                    }
                                }
                            }
                        }


                        field.IntValue = sum;
                        return sum.ToString();
                    }
                    if (asmntField.CalculationFunction == "ConcatenatedMissingLetters")
                    {
                        var unknownLetters = "";
                        var unknownLettersList = new List<string>();

                        foreach (var group in assessment.FieldGroups)
                        {
                            var groupId = group.Id;

                            var foundInGroup = false;

                            foreach (var currentResult in result.FieldResults)
                            {
                                var currentField = assessment.Fields.First(p => p.DatabaseColumn == currentResult.DbColumn);

                                if (currentField.GroupId == groupId && currentResult.DbColumn.Substring(0, 3) == "chk")
                                {
                                    if (currentResult.BoolValue.Value)
                                    {
                                        foundInGroup = true;
                                        break;
                                    }
                                }
                            }
                            if (!foundInGroup)
                            {
                                // how to get the letter? find the field with the same groupid and print its DisplayLabel
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {

                                    if (currentField.GroupId == groupId && currentField.FieldType == "Label")
                                    {
                                        unknownLettersList.Add(currentField.DisplayLabel);
                                        //unknownLetters += currentField.DisplayLabel + ",";
                                        break;
                                    }
                                }
                            }
                        }
                        //remove trailing comma
                        if (unknownLettersList.Count > 0)
                        {
                            unknownLettersList.Sort();
                            unknownLetters = string.Join(",", unknownLettersList);
                            //unknownLetters = unknownLetters.Substring(0, unknownLetters.Length - 1);
                        }
                        else
                        {
                            unknownLetters = "none";
                        }
                        field.StringValue = unknownLetters;
                        return String.Format("'{0}'", unknownLetters); ;
                    }
                    return "0";
                case "CalculatedFieldDbOnly":
                    string stringValue = String.Empty;
                    if (asmntField.CalculationFunction == "BenchmarkLevel")
                    {
                        // calculate benchmark value and return
                        using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
                        {
                            try
                            {
                                // FPValueId, Accuracy, CompScore (may need to calculate, since it might have changed)
                                //Database.AsSqlServer().Connection.DbConnection.Open();
                                command.CommandText = "dbo.ns4_udf_CalculateBenchmarkLevel";
                                command.CommandType = CommandType.StoredProcedure;
                                command.CommandTimeout = command.Connection.ConnectionTimeout;
                                var outParameter = command.CreateParameter();
                                outParameter.Direction = ParameterDirection.ReturnValue;
                                outParameter.ParameterName = "@RETURN_VALUE";
                                outParameter.DbType = DbType.String;
                                outParameter.Size = 50;

                                var fpValueId = command.CreateParameter();
                                fpValueId.Direction = ParameterDirection.Input;
                                fpValueId.DbType = DbType.Int32;
                                fpValueId.ParameterName = "@FPValueID";
                                var fpField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "FPValueID");
                                fpValueId.Value = fpField == null ? 0 : fpField.IntValue ?? 0;

                                var accuracy = command.CreateParameter();
                                accuracy.Direction = ParameterDirection.Input;
                                accuracy.DbType = DbType.Int32;
                                accuracy.ParameterName = "@Accuracy";
                                var accuracyField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Accuracy");
                                accuracy.Value = accuracyField == null ? 0 : accuracyField.DecimalValue ?? 0;

                                var compScore = command.CreateParameter();
                                compScore.Direction = ParameterDirection.Input;
                                compScore.DbType = DbType.Int32;
                                compScore.ParameterName = "@CompScore";
                                int newCompScoreSum = 0;
                                var aboutField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "About");
                                var withinField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Within");
                                var beyondField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "Beyond");
                                var extraPtField = result.FieldResults.FirstOrDefault(p => p.DbColumn == "ExtraPt");

                                compScore.Value = (aboutField == null ? 0 : aboutField.IntValue ?? 0) +
                                    (withinField == null ? 0 : withinField.IntValue ?? 0) +
                                    (beyondField == null ? 0 : beyondField.IntValue ?? 0) +
                                    (extraPtField == null ? 0 : extraPtField.IntValue ?? 0);

                                command.Parameters.Add(outParameter);
                                command.Parameters.Add(fpValueId);
                                command.Parameters.Add(accuracy);
                                command.Parameters.Add(compScore);
                                command.ExecuteNonQuery();
                                stringValue = ((System.Data.SqlClient.SqlParameter)(command.Parameters["@RETURN_VALUE"])).Value.ToString();
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        field.StringValue = stringValue;
                    }
                    return String.Format("'{0}'", stringValue);

            }

            return String.Empty;
        }

        /// <summary>
        /// This method doesn't care about test due dates and returns data for ALL test due dates.  This is used for section reports.
        /// </summary>
        /// <param name="assessment"></param>
        /// <param name="classId"></param>
        /// <returns></returns>
        public List<AssessmentStudentResult> GetBASAssessmentStudentResults(Assessment assessment, int classId, int schoolYear)
        {

            // USE year to get TDD List for school year
            var tddsForYear = TestDueDates.Where(p => p.SchoolStartYear == schoolYear).Select(t => t.Id).ToList();
            var tddString = string.Join(",", tddsForYear);

            List<AssessmentStudentResult> lstStudentData = new List<AssessmentStudentResult>();
            try
            {
                // Removed on 9/28/2019 due to automated rollover removing old sections
                // INNER JOIN dbo.Section sec ON sec.SchoolStartYear = {3} AND t.SectionID = sec.Id

                var results = Database.GetBASClassReportData(String.Format(@"SELECT s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, t.*, {1} as InputSectionId, ss.GradeId FROM {0} t 

                    
                    RIGHT OUTER JOIN Student s on s.ID = t.StudentID AND t.TestDueDateID IN ({4}) INNER JOIN dbo.StudentSchool ss ON s.Id = ss.StudentID AND ss.SchoolStartYear = {3} 
                    WHERE 
                    s.ID IN (SELECT Studentid FROM  dbo.nset_udf_GetStudentIDsForSummaryPages({1}, '{2}', null))
                    AND ss.Id IN ( SELECT TOP 1
                                Id
                       FROM     dbo.StudentSchool
                       WHERE    dbo.StudentSchool.StudentID = s.Id
                                AND dbo.StudentSchool.SchoolStartYear = {3}
                       ORDER BY Id DESC )
                     ORDER BY s.LastName, s.FirstName, s.Id, t.TestDueDateID ASC", assessment.StorageTable, classId, DateTime.Now, schoolYear, tddString), assessment, this);
                lstStudentData = results;
            }
            catch (Exception ex)
            {
                Log.Error("Error during F & P Results: {0}", ex.Message);
            }

            return lstStudentData;
        }

        /// <summary>
        /// This is used for data entry screens and returns the data for a single TDD
        /// </summary>
        /// <param name="assessment"></param>
        /// <param name="classId"></param>
        /// <param name="benchmarkDateId"></param>
        /// <param name="testDate"></param>
        /// <returns></returns>
        public AssessmentHFWStudentResult GetHFWAssessmentStudentResults(Assessment assessment, int classId,
            int benchmarkDateId, DateTime testDate, int studentId)
        {
            AssessmentHFWStudentResult studentData = null;
            try
            {
                studentData = Database.HFWStudentDataEntryResults(String.Format("SELECT TOP 1 s.ID as StudentID, s.FirstName, s.LastName, s.MiddleName, t.*, r.*, w.*, {1} as InputSectionId, dbo.ns4_udf_GetHFwREADSCOREForStudentDate(s.ID, '{3}') AS ReadScore, dbo.ns4_udf_GetHFwWRITESCOREForStudentDate(s.ID, '{3}') AS WriteScore, dbo.ns4_udf_GetHFwTOTALSCOREForStudentDate(s.ID, '{3}') AS TotalScore FROM Student s LEFT OUTER JOIN {0} t ON s.ID = t.StudentID LEFT JOIN {5} r on r.StudentID = {4} LEFT JOIN {6} w on w.StudentID = {4} WHERE s.id = {4} ORDER BY s.LastName, s.FirstName", assessment.StorageTable, classId, benchmarkDateId, DateTime.Now, studentId, assessment.SecondaryStorageTable, assessment.TertiaryStorageTable), assessment, this);
            }
            catch (Exception ex)
            {
                //TODO:LOG
            }

            return studentData;
        }

        #region Observation Summary Methods
        public List<AssessmentField> GetViewableFields(Assessment assessment, string viewType, int staffId)
        {
            var viewableFields = new List<AssessmentField>();

            

            // remove fields based on type...
            switch(viewType)
            {
                case "observationsummary":
                    var fieldsToRemove = new List<StaffObservationSummaryAssessmentField>();
                    viewableFields = assessment.Fields.Where(p => p.DisplayInObsSummary == true).ToList();
                    fieldsToRemove = this.StaffObservationSummaryAssessmentFields.Where(p => p.StaffId == staffId && p.AssessmentId == assessment.Id).ToList();
                    viewableFields.RemoveAll(p => fieldsToRemove.Any(g => g.AssessmentFieldId == p.Id));
                    //fieldsToRemove = this.StaffAssessmentFieldVisibilities.Where(p => p.StaffId == staffId && p.AssessmentId == assessment.Id && p.DisplayInObsSummary == false).ToList();
                    break;
                case "linegraphs":
                    var fieldsToRemoveL = new List<StaffAssessmentFieldVisibility>();
                    viewableFields = assessment.Fields.Where(p => p.DisplayInLineGraphs == true).ToList();
                    fieldsToRemoveL = this.StaffAssessmentFieldVisibilities.Where(p => p.StaffId == staffId && p.AssessmentId == assessment.Id && p.DisplayInObsSummary == false).ToList();
                    viewableFields.RemoveAll(p => fieldsToRemoveL.Any(g => g.AssessmentFieldId == p.Id));

                    break;
                case "editresults":
                    var fieldsToRemoveE = new List<StaffAssessmentFieldVisibility>();
                    viewableFields = assessment.Fields.Where(p => p.DisplayInEditResultList == true).ToList();
                    fieldsToRemoveE = this.StaffAssessmentFieldVisibilities.Where(p => p.StaffId == staffId && p.AssessmentId == assessment.Id && p.DisplayInObsSummary == false).ToList();
                    viewableFields.RemoveAll(p => fieldsToRemoveE.Any(g => g.AssessmentFieldId == p.Id));

                    break;
            }


            return viewableFields;
        }

        public List<Assessment> GetObservationSummaryVisibleAssessments(int staffId)
        {
            var visibleAssessments = new List<Assessment>();

            var allAssessmentsICanAccess = this.Assessments
                 .Include(p => p.FieldGroups)
                    .Include(p => p.FieldCategories)
                    .Include(p => p.FieldSubCategories)
                    .Include(p => p.Fields)
                    .Where(p => ((p.AssessmentIsAvailable.HasValue && p.AssessmentIsAvailable.Value) || (p.AssessmentIsAvailable == null)) && (p.TestType == 1 || p.TestType == 3)).OrderBy(p => p.SortOrder).ToList();

            // remove any that are removed by the schools
            // get all of the schoolIds that I have access to
            var schoolIds = this.StaffSchools.Where(p => p.StaffID == staffId).Select(p => p.SchoolID).ToList();
            var schoolAssessmentsICantAccess = new List<Assessment>();

            foreach (var districtAccesssibleAssessment in allAssessmentsICanAccess)
            {
                var schoolAssessments = this.SchoolAssessments.Where(p => schoolIds.Contains(p.SchoolId) && p.AssessmentId == districtAccesssibleAssessment.Id);
                if (schoolAssessments.Count() > 0)
                {
                    if (schoolAssessments.All(p => !p.AssessmentIsAvailable))
                    {
                        schoolAssessmentsICantAccess.Add(districtAccesssibleAssessment);
                    }
                }
            }

            // remove assessments that ALL schools have said are not available
            allAssessmentsICanAccess.RemoveAll(p => schoolAssessmentsICantAccess.Contains(p));

            // remove any that are hidden by the user
            var staffAssessmentsICantAccess = this.StaffAssessments.Where(p => p.StaffId == staffId && !p.AssessmentIsAvailable).Select(p => p.Assessment).ToList();
            allAssessmentsICanAccess.RemoveAll(p => staffAssessmentsICantAccess.Contains(p));


            allAssessmentsICanAccess.ForEach(p =>
            {
                if(this.StaffObservationSummaryAssessments.FirstOrDefault(g => g.AssessmentId == p.Id && g.StaffId == staffId) == null)
                {
                    visibleAssessments.Add(p);
                }
            });

            return visibleAssessments;
        }

        public DataTable GetDistrictAttendance(int schoolStartYear)
        {
            var dataTable = new DataTable();

            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                command.CommandTimeout = 3000;
                try
                {
                    // set up headers

                    var results = Database.SqlQuery<AttendanceExportInfo>("EXEC [ns4_GetAttendanceExportInfo] @SchoolYear",
                    new SqlParameter("SchoolYear", schoolStartYear)).ToList();

                    var uniqueGroups = results.Select(p => p.InterventionGroupId).Distinct();

                    // speed... get all current records for all groups we care about
                    var existingAttendanceInfoForGroups = this.InterventionAttendances.Include(p => p.AttendanceReason).Where(p => uniqueGroups.Contains(p.SectionID)).ToList();


                    var aCalc = new AttendanceCalculator(this);
                    // now get attendance data for every day for each unique group
                    // get meeting days, etc
                    var detailedAttendance = aCalc.GetAttendanceDetail(results, existingAttendanceInfoForGroups);

                    dataTable = ImportTestDataService.AttendanceResultsToDataTable(detailedAttendance, this);

                }
                catch (Exception ex)
                {
                    Log.Error("Error getting attendance export: {0}", ex.Message);
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }
            return dataTable;
        }

        public DataTable GetStaffExportInfo()
        {
            DataTable dataTable = null;

            try
            {
                var allStaff = StaffDataService.GetAllStaff(this);

                dataTable = StaffDataService.ConvertStaffToDataTable(allStaff, this);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting student export: {0}", ex.Message);
            }
            
            return dataTable;
        }

        public DataTable GetStudentExportInfo()
        {
            DataTable dataTable = null;

            try
            {
                // set up headers
                var studentAttributes = StudentDataService.GetStudentAttributeLookups(this);
                var allStudents = StudentDataService.GetAllStudents(this);

                dataTable = StudentDataService.ConvertStudentsToDataTable(allStudents, studentAttributes);

            }
            catch (Exception ex)
            {
                Log.Error("Error getting student export: {0}", ex.Message);
            }

            return dataTable;
        }

        public ObservationSummaryGroupResults GetFilteredDataExportData(List<Assessment> assessments, InputDto_GetFilteredObservationSummaryOptions input, int staffId, bool usePreAttachedFields)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                command.CommandTimeout = 45;
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // remove fields that shouldn't be shown in this view
                        if (!usePreAttachedFields)
                        {
                            assessment.Fields = GetViewableFields(assessment, "observationsummary", staffId);
                        }
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };
                        groupResults.HeaderGroups.Add(currentHeaderGroup);
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }
                    }

                    Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
                    var attributeParameterArray = new SqlParameter[10];
                    var attributeIdParameterArray = new SqlParameter[10];

                    // hackfix, empty params
                    for (var i = 0; i < 10; i++)
                    {
                        attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                        attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
                    }

                    // combine the student attributevalues into a single list
                    List<string> attributeValues = new List<string>();
                    foreach (var attributeType in input.DropdownDataList)
                    {
                        // hackfix, and yes, I did regret it... thanks jerkface
                        attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                        attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                        attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
                    }

                    //attributeValues.AddRange(input.TitleOneTypes.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input.Ethnicities.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input.EducationLabels.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input..Select(p => p.id).ToList());

                    // call stored procedure and pass parameters

                    Database.Connection.Open();
                    command.CommandText = @"EXEC dbo.ns4_GetFilteredDataExportScores @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd,@TeamMeeting,@TextLevelZone";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("TestDueDateID", SqlDbType.Int) { Value = (object)input.TestDueDateID ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("SchoolStartYear", input.SchoolStartYear));
                    command.Parameters.Add(new SqlParameter("Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())));
                    command.Parameters.Add(attributeParameterArray[0]);
                    command.Parameters.Add(attributeIdParameterArray[0]);
                    command.Parameters.Add(attributeParameterArray[1]);
                    command.Parameters.Add(attributeIdParameterArray[1]);
                    command.Parameters.Add(attributeParameterArray[2]);
                    command.Parameters.Add(attributeIdParameterArray[2]);
                    command.Parameters.Add(attributeParameterArray[3]);
                    command.Parameters.Add(attributeIdParameterArray[3]);
                    command.Parameters.Add(attributeParameterArray[4]);
                    command.Parameters.Add(attributeIdParameterArray[4]);
                    command.Parameters.Add(attributeParameterArray[5]);
                    command.Parameters.Add(attributeIdParameterArray[5]);
                    command.Parameters.Add(attributeParameterArray[6]);
                    command.Parameters.Add(attributeIdParameterArray[6]);
                    command.Parameters.Add(attributeParameterArray[7]);
                    command.Parameters.Add(attributeIdParameterArray[7]);
                    command.Parameters.Add(attributeParameterArray[8]);
                    command.Parameters.Add(attributeIdParameterArray[8]);
                    command.Parameters.Add(attributeParameterArray[9]);
                    command.Parameters.Add(attributeIdParameterArray[9]);
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("TeamMeeting", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("TextLevelZone", DBNull.Value));
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 45;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = dt.Rows[i]["GradeID"] == DBNull.Value ? 0 : Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.GradeOrder = dt.Rows[i]["GradeOrder"] == DBNull.Value ? 0 : Int32.Parse(dt.Rows[i]["GradeOrder"].ToString());
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.GradeName = dt.Rows[i]["GradeName"].ToString();
                            studentResult.StudentIdentifier = dt.Rows[i]["StudentIdentifier"].ToString();
                            studentResult.SchoolName = dt.Rows[i]["SchoolName"].ToString();
                            studentResult.DelimitedTeacherSections = dt.Rows[i]["DelimitedTeacherSections"].ToString();
                            //studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
                            //studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
                            //studentResult.LastName = dt.Rows[i]["LastName"].ToString();
                            studentResult.TestDueDateId = input.TestDueDateID.Value;//result.GetPropValue<int>("TestDueDateID");

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.TestTypeId = assessment.TestType.HasValue ? assessment.TestType.Value : 1;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;

                                    // add state test data grade
                                    if (assessment.TestType == 3)
                                    {
                                        currentFieldScore.StateGradeId = dt.Rows[i][assessment.StorageTable + "_GradeId"] == DBNull.Value ? "0" : dt.Rows[i][assessment.StorageTable + "_GradeId"].ToString();
                                    }
                                    else
                                    {
                                        currentFieldScore.StateGradeId = "0";

                                        if (dt.Columns.Contains(assessment.StorageTable + "_IsCopied"))
                                        {
                                            if (dt.Rows[i][assessment.StorageTable + "_IsCopied"] != DBNull.Value)
                                            {
                                                currentFieldScore.IsCopied = Boolean.Parse(dt.Rows[i][assessment.StorageTable + "_IsCopied"].ToString());
                                            }
                                        }
                                    }

                                    switch (currentField.FieldType)
                                    {
                                        case "Checkbox":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.BoolValue = Boolean.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "Textfield":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error loading filtered observation summary results: {0} - SQL: {1}", ex.Message, command.GetGeneratedQuery());
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return groupResults;
        }

        public ObservationSummaryGroupResults GetFilteredObservationSummaryData(List<Assessment> assessments,  InputDto_GetFilteredObservationSummaryOptions input, int staffId, bool usePreAttachedFields)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

            // get all the attributes
            var allAttributes = this.StudentAttributeTypes.Where(p => p.Id != 4).OrderBy(p => p.Id).ToList();

            for (var i = 0; i < allAttributes.Count; i++)
            {
                var Id = allAttributes[i].Id;
                switch (i)
                {
                    case 0:
                        groupResults.Att1Header = allAttributes[i].AttributeName;
                        groupResults.Att1Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 1:
                        groupResults.Att2Header = allAttributes[i].AttributeName;
                        groupResults.Att2Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 2:
                        groupResults.Att3Header = allAttributes[i].AttributeName;
                        groupResults.Att3Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 3:
                        groupResults.Att4Header = allAttributes[i].AttributeName;
                        groupResults.Att4Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 4:
                        groupResults.Att5Header = allAttributes[i].AttributeName;
                        groupResults.Att5Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 5:
                        groupResults.Att6Header = allAttributes[i].AttributeName;
                        groupResults.Att6Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 6:
                        groupResults.Att7Header = allAttributes[i].AttributeName;
                        groupResults.Att7Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 7:
                        groupResults.Att8Header = allAttributes[i].AttributeName;
                        groupResults.Att8Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 8:
                        groupResults.Att9Header = allAttributes[i].AttributeName;
                        groupResults.Att9Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                }
            }

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                command.CommandTimeout = 45;
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // remove fields that shouldn't be shown in this view
                        if (!usePreAttachedFields)
                        {
                            assessment.Fields = GetViewableFields(assessment, "observationsummary", staffId);
                        }
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };
                        groupResults.HeaderGroups.Add(currentHeaderGroup);
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }
                    }

                    Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
                    var attributeParameterArray = new SqlParameter[10];
                    var attributeIdParameterArray = new SqlParameter[10];

                    // hackfix, empty params
                    for (var i = 0; i < 10; i++)
                    {
                        attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                        attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
                    }

                    // combine the student attributevalues into a single list
                    List<string> attributeValues = new List<string>();
                    foreach (var attributeType in input.DropdownDataList)
                    {
                        // hackfix, and yes, I did regret it... thanks jerkface
                        attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                        attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                        attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
                    }

                    // TODO:  Fix this god-awful hack.  This depends on the IDs of the Attributes being less than 9... i really don't like this at all
                    // and it WILL come back to haunt me some day
                    // #YouWillRegretThis
                    //for (var i = 0; i < 10; i++)
                    //{
                    //    attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                    //    attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);

                    //    // if this key exists, set a value
                    //    if (attributeDictionary.ContainsKey(i + 1))
                    //    {
                    //        attributeParameterArray[i].Value = string.Join<int>(",", attributeDictionary[i + 1]);
                    //        attributeIdParameterArray[i].Value = i + 1;
                    //    }
                    //}

                    //attributeValues.AddRange(input.TitleOneTypes.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input.Ethnicities.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input.EducationLabels.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input..Select(p => p.id).ToList());

                    // call stored procedure and pass parameters

                    Database.Connection.Open();
                    command.CommandText = @"EXEC dbo.ns4_GetFilteredObservationSummaryScores @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd,@TeamMeeting,@TextLevelZone";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("TestDueDateID", SqlDbType.Int) { Value = (object)input.TestDueDateID ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("SchoolStartYear", input.SchoolStartYear));
                    command.Parameters.Add(new SqlParameter("Schools", string.Join<int>(",", input.Schools.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Sections", string.Join<int>(",", input.Sections.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Grades", string.Join<int>(",", input.Grades.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Teachers", string.Join<int>(",", input.Teachers.Select(p => p.id).ToList())));
                    command.Parameters.Add(attributeParameterArray[0]);
                    command.Parameters.Add(attributeIdParameterArray[0]);
                    command.Parameters.Add(attributeParameterArray[1]);
                    command.Parameters.Add(attributeIdParameterArray[1]);
                    command.Parameters.Add(attributeParameterArray[2]);
                    command.Parameters.Add(attributeIdParameterArray[2]);
                    command.Parameters.Add(attributeParameterArray[3]);
                    command.Parameters.Add(attributeIdParameterArray[3]);
                    command.Parameters.Add(attributeParameterArray[4]);
                    command.Parameters.Add(attributeIdParameterArray[4]);
                    command.Parameters.Add(attributeParameterArray[5]);
                    command.Parameters.Add(attributeIdParameterArray[5]);
                    command.Parameters.Add(attributeParameterArray[6]);
                    command.Parameters.Add(attributeIdParameterArray[6]);
                    command.Parameters.Add(attributeParameterArray[7]);
                    command.Parameters.Add(attributeIdParameterArray[7]);
                    command.Parameters.Add(attributeParameterArray[8]);
                    command.Parameters.Add(attributeIdParameterArray[8]);
                    command.Parameters.Add(attributeParameterArray[9]);
                    command.Parameters.Add(attributeIdParameterArray[9]);
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", string.Join<int>(",", input.InterventionTypes.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("TeamMeeting", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("TextLevelZone", DBNull.Value));
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 45;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = dt.Rows[i]["GradeID"] == DBNull.Value ? 0 : Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.GradeOrder = dt.Rows[i]["GradeOrder"] == DBNull.Value ? 0 : Int32.Parse(dt.Rows[i]["GradeOrder"].ToString());
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.GradeName = dt.Rows[i]["GradeName"].ToString();
                            studentResult.StudentIdentifier = dt.Rows[i]["StudentIdentifier"].ToString();
                            studentResult.SchoolName = dt.Rows[i]["SchoolName"].ToString();
                            studentResult.DelimitedTeacherSections = dt.Rows[i]["DelimitedTeacherSections"].ToString();

                            // added on 9/26/2020 - need to show all attributes
                            studentResult.Services = dt.Rows[i]["StudentServices"] != DBNull.Value ? dt.Rows[i]["StudentServices"].ToString() : "";
                            studentResult.SpecialED = dt.Rows[i]["SPEDLabels"] != DBNull.Value ? dt.Rows[i]["SPEDLabels"].ToString() : "";
                            studentResult.Att1 = dt.Rows[i]["Att1"] != DBNull.Value ? dt.Rows[i]["Att1"].ToString() : "";
                            studentResult.Att2 = dt.Rows[i]["Att2"] != DBNull.Value ? dt.Rows[i]["Att2"].ToString() : "";
                            studentResult.Att3 = dt.Rows[i]["Att3"] != DBNull.Value ? dt.Rows[i]["Att3"].ToString() : "";
                            studentResult.Att4 = dt.Rows[i]["Att4"] != DBNull.Value ? dt.Rows[i]["Att4"].ToString() : "";
                            studentResult.Att5 = dt.Rows[i]["Att5"] != DBNull.Value ? dt.Rows[i]["Att5"].ToString() : "";
                            studentResult.Att6 = dt.Rows[i]["Att6"] != DBNull.Value ? dt.Rows[i]["Att6"].ToString() : "";
                            studentResult.Att7 = dt.Rows[i]["Att7"] != DBNull.Value ? dt.Rows[i]["Att7"].ToString() : "";
                            studentResult.Att8 = dt.Rows[i]["Att8"] != DBNull.Value ? dt.Rows[i]["Att8"].ToString() : "";
                            studentResult.Att9 = dt.Rows[i]["Att9"] != DBNull.Value ? dt.Rows[i]["Att9"].ToString() : "";
                            //studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
                            //studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
                            //studentResult.LastName = dt.Rows[i]["LastName"].ToString();
                            studentResult.TestDueDateId = input.TestDueDateID.Value;//result.GetPropValue<int>("TestDueDateID");

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.TestTypeId = assessment.TestType.HasValue ? assessment.TestType.Value : 1;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;

                                    // add state test data grade
                                    if (assessment.TestType == 3)
                                    {
                                        currentFieldScore.StateGradeId = dt.Rows[i][assessment.StorageTable + "_GradeId"] == DBNull.Value ? "0" : dt.Rows[i][assessment.StorageTable + "_GradeId"].ToString();
                                    }
                                    else
                                    {
                                        currentFieldScore.StateGradeId = "0";

                                        if (dt.Columns.Contains(assessment.StorageTable + "_IsCopied"))
                                        {
                                            if (dt.Rows[i][assessment.StorageTable + "_IsCopied"] != DBNull.Value)
                                            {
                                                currentFieldScore.IsCopied = Boolean.Parse(dt.Rows[i][assessment.StorageTable + "_IsCopied"].ToString());
                                            }
                                        }
                                    }

                                    switch (currentField.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error loading filtered observation summary results: {0} - SQL: {1}", ex.Message, command.GetGeneratedQuery());
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return groupResults;
        }

        public ObservationSummaryGroupResults GetFilteredInterventionSummaryData(List<Assessment> assessments, InputDto_GetFilteredObservationSummaryOptions input, int staffId)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray());

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                command.CommandTimeout = 45;
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };
                        groupResults.HeaderGroups.Add(currentHeaderGroup);
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }
                    }

                    Dictionary<int, List<int>> attributeDictionary = new Dictionary<int, List<int>>();
                    var attributeParameterArray = new SqlParameter[10];
                    var attributeIdParameterArray = new SqlParameter[10];

                    // hackfix, empty params
                    for (var i = 0; i < 10; i++)
                    {
                        attributeParameterArray[i] = new SqlParameter("StudentAttributeValues" + i, DBNull.Value);
                        attributeIdParameterArray[i] = new SqlParameter("StudentAttributeIdValue" + i, DBNull.Value);
                    }

                    // combine the student attributevalues into a single list
                    List<string> attributeValues = new List<string>();
                    foreach (var attributeType in input.DropdownDataList)
                    {
                        // hackfix, and yes, I did regret it... thanks jerkface
                        attributeParameterArray[attributeDictionary.Count].Value = string.Join<int>(",", attributeType.DropDownData.Select(p => p.id).ToList());
                        attributeIdParameterArray[attributeDictionary.Count].Value = attributeType.AttributeTypeId;

                        attributeDictionary.Add(attributeType.AttributeTypeId, attributeType.DropDownData.Select(p => p.id).ToList());
                    }

                    //attributeValues.AddRange(input.TitleOneTypes.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input.Ethnicities.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input.EducationLabels.Select(p => p.id).ToList());
                    //attributeValues.AddRange(input..Select(p => p.id).ToList());

                    // call stored procedure and pass parameters

                    Database.Connection.Open();
                    command.CommandText = @"EXEC dbo.ns4_GetFilteredInterventionDataScores @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd,@TeamMeeting,@TextLevelZones,@Interventionists,@InterventionGroups";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("TestDueDateID", SqlDbType.Int) { Value = (object)input.TestDueDateID ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("SchoolStartYear", input.SchoolStartYear));
                    command.Parameters.Add(new SqlParameter("Schools", string.Join<int>(",", input.Schools == null ? new List<int>() : input.Schools.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Sections", string.Join<int>(",", input.Sections == null ? new List<int>() : input.Sections.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Grades", string.Join<int>(",", input.Grades == null ? new List<int>() : input.Grades.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("Teachers", string.Join<int>(",", input.Teachers == null ? new List<int>() : input.Teachers.Select(p => p.id).ToList())));
                    command.Parameters.Add(attributeParameterArray[0]);
                    command.Parameters.Add(attributeIdParameterArray[0]);
                    command.Parameters.Add(attributeParameterArray[1]);
                    command.Parameters.Add(attributeIdParameterArray[1]);
                    command.Parameters.Add(attributeParameterArray[2]);
                    command.Parameters.Add(attributeIdParameterArray[2]);
                    command.Parameters.Add(attributeParameterArray[3]);
                    command.Parameters.Add(attributeIdParameterArray[3]);
                    command.Parameters.Add(attributeParameterArray[4]);
                    command.Parameters.Add(attributeIdParameterArray[4]);
                    command.Parameters.Add(attributeParameterArray[5]);
                    command.Parameters.Add(attributeIdParameterArray[5]);
                    command.Parameters.Add(attributeParameterArray[6]);
                    command.Parameters.Add(attributeIdParameterArray[6]);
                    command.Parameters.Add(attributeParameterArray[7]);
                    command.Parameters.Add(attributeIdParameterArray[7]);
                    command.Parameters.Add(attributeParameterArray[8]);
                    command.Parameters.Add(attributeIdParameterArray[8]);
                    command.Parameters.Add(attributeParameterArray[9]);
                    command.Parameters.Add(attributeIdParameterArray[9]);
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", string.Join<int>(",", input.InterventionTypes == null ? new List<int>() : input.InterventionTypes.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("SpecialEd", SqlDbType.VarChar) { Value = (object)input.SpecialEd?.id ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("TeamMeeting", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("TextLevelZones", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("Interventionists", string.Join<int>(",", input.Interventionists == null ? new List<int>() : input.Interventionists.Select(p => p.id).ToList())));
                    command.Parameters.Add(new SqlParameter("InterventionGroups", string.Join<int>(",", input.InterventionGroups == null ? new List<int>() : input.InterventionGroups.Select(p => p.id).ToList())));
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 45;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = dt.Rows[i]["GradeID"] == DBNull.Value ? 0 : Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.GradeOrder = dt.Rows[i]["GradeOrder"] == DBNull.Value ? 0 : Int32.Parse(dt.Rows[i]["GradeOrder"].ToString());
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.GradeName = dt.Rows[i]["GradeName"].ToString();
                            studentResult.StudentIdentifier = dt.Rows[i]["StudentIdentifier"].ToString();
                            studentResult.SchoolName = dt.Rows[i]["SchoolName"].ToString();
                            studentResult.DelimitedTeacherSections = dt.Rows[i]["DelimitedTeacherSections"].ToString();

                            studentResult.SPEDLables = dt.Rows[i]["SPEDLabels"]?.ToString();
                            studentResult.StudentServices = dt.Rows[i]["StudentServices"]?.ToString();
                            studentResult.ELL = dt.Rows[i]["ELL"]?.ToString();
                            studentResult.FPScore = dt.Rows[i]["FPScore"]?.ToString();
                            studentResult.Interventionist = dt.Rows[i]["Interventionist"].ToString();
                            studentResult.InterventionGroupName = dt.Rows[i]["InterventionGroupName"].ToString();
                            studentResult.BenchmarkDate = dt.Rows[i]["BenchmarkDate"] == DBNull.Value ? (DateTime?)null : DateTime.Parse(dt.Rows[i]["BenchmarkDate"].ToString());
                            studentResult.DateTestTaken = dt.Rows[i]["DateTestTaken"] == DBNull.Value ? (DateTime?)null : DateTime.Parse(dt.Rows[i]["DateTestTaken"].ToString());
                            //studentResult.FirstName = dt.Rows[i]["FirstName"].ToString();
                            //studentResult.MiddleName = dt.Rows[i]["MiddleName"].ToString();
                            //studentResult.LastName = dt.Rows[i]["LastName"].ToString();
                            //studentResult.TestDueDateId = input.TestDueDateID.Value;//result.GetPropValue<int>("TestDueDateID");

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.TestTypeId = assessment.TestType.HasValue ? assessment.TestType.Value : 1;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;

                                    // add state test data grade
                                    if (assessment.TestType == 3)
                                    {
                                        currentFieldScore.StateGradeId = dt.Rows[i][assessment.StorageTable + "_GradeId"] == DBNull.Value ? "0" : dt.Rows[i][assessment.StorageTable + "_GradeId"].ToString();
                                    }
                                    else
                                    {
                                        currentFieldScore.StateGradeId = "0";
                                    }

                                    switch (currentField.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error loading filtered intervention data summary results: {0}", ex.Message);
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return groupResults;
        }

        public ObservationSummaryGroupResults GetClassObservationSummaryMultipleData(List<Assessment> assessments, int classId,
                List<OutputDto_DropdownData> testDueDates, int staffId, bool isMultiColumn)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
            int gradeId = 0;
            int testLevelPeriodId = 0;
            string commaTerminatedAssessments = string.Empty;
            
            string stringTdds = string.Join<int>(",", testDueDates.Select(p => p.id).ToList());

            // get all the attributes
            var allAttributes = this.StudentAttributeTypes.Where(p => p.Id != 4).OrderBy(p => p.Id).ToList();

            for (var i = 0; i < allAttributes.Count; i++)
            {
                var Id = allAttributes[i].Id;
                switch (i)
                {
                    case 0:
                        groupResults.Att1Header = allAttributes[i].AttributeName;
                        groupResults.Att1Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 1:
                        groupResults.Att2Header = allAttributes[i].AttributeName;
                        groupResults.Att2Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 2:
                        groupResults.Att3Header = allAttributes[i].AttributeName;
                        groupResults.Att3Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 3:
                        groupResults.Att4Header = allAttributes[i].AttributeName;
                        groupResults.Att4Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 4:
                        groupResults.Att5Header = allAttributes[i].AttributeName;
                        groupResults.Att5Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 5:
                        groupResults.Att6Header = allAttributes[i].AttributeName;
                        groupResults.Att6Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 6:
                        groupResults.Att7Header = allAttributes[i].AttributeName;
                        groupResults.Att7Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 7:
                        groupResults.Att8Header = allAttributes[i].AttributeName;
                        groupResults.Att8Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 8:
                        groupResults.Att9Header = allAttributes[i].AttributeName;
                        groupResults.Att9Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                }
            }

            var assessmentsToRemove = new List<Assessment>();
            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // remove fields that shouldn't be shown in this view
                        assessment.Fields = GetViewableFields(assessment, "observationsummary", staffId);
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };

                        
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            // if thisis multicolumn, we don't need to check for primary field
                            if (!isMultiColumn)
                            {
                                // make sure this field is set as the primary one
                                if (currentField.IsPrimaryFieldForAssessment != true)
                                {
                                    continue;
                                }
                            }
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = isMultiColumn ? currentField.DisplayLabel  : currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId,
                                CalculationFields = currentField.CalculationFields,
                                CalculationFunction = currentField.CalculationFunction
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }

                        // don't add header group if no fields for that assessment
                        if(currentHeaderGroup.FieldCount > 0)
                        {
                            groupResults.HeaderGroups.Add(currentHeaderGroup);
                        } else
                        {
                            assessmentsToRemove.Add(assessment);
                        }
                    }

                    // remove assessments that don't have any fields
                    assessments.RemoveAll(p => assessmentsToRemove.Contains(p) || p.TestType != 1);
                    commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";


                    Database.Connection.Open();
                    //command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryScores @TableNames='{0}',@TestDueDate='{1}',@TestDueDateID={2},@SectionID={3}", commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    //command.CommandType = CommandType.Text;
                    //command.CommandTimeout = command.Connection.ConnectionTimeout;

                    // use first benchmark date, doesn't matter which one... only one school year is allowed
                    var benchmarkDateId = testDueDates.First().id;
                    var schoolStartYear = TestDueDates.First(p => p.Id == benchmarkDateId).SchoolStartYear;

                    command.CommandText = @"EXEC dbo.ns4_GetFilteredObservationSummaryScores_Multiple @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd,@TeamMeeting, @TextLevelZone, @TestDueDateIDs";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("@TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("@TestDueDateIDs", stringTdds));
                    command.Parameters.Add(new SqlParameter("@TestDueDateID", SqlDbType.Int) { Value = (object)benchmarkDateId ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@SchoolStartYear", schoolStartYear));
                    command.Parameters.Add(new SqlParameter("@Schools", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Sections", classId));
                    command.Parameters.Add(new SqlParameter("@Grades", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Teachers", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues0", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue0", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues1", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue1", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues2", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue2", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues3", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue3", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues4", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue4", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues5", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue5", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues6", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue6", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues7", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue7", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues8", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue8", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues9", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue9", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@SpecialEd", SqlDbType.VarChar) { Value = DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@TeamMeeting", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@TextLevelZone", DBNull.Value));
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 45;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.TestDueDateId = Int32.Parse(dt.Rows[i]["TestDueDateID"].ToString());//benchmarkDateId;//result.GetPropValue<int>("TestDueDateID");
                            studentResult.TestLevelPeriodId = Int32.Parse(dt.Rows[i]["TestLevelPeriodId"].ToString());
                            // added on 9/26/2020 - need to show all attributes
                            studentResult.Services = dt.Rows[i]["StudentServices"] != DBNull.Value ? dt.Rows[i]["StudentServices"].ToString() : "";
                            studentResult.SpecialED = dt.Rows[i]["SPEDLabels"] != DBNull.Value ? dt.Rows[i]["SPEDLabels"].ToString() : "";
                            studentResult.Att1 = dt.Rows[i]["Att1"] != DBNull.Value ? dt.Rows[i]["Att1"].ToString() : "";
                            studentResult.Att2 = dt.Rows[i]["Att2"] != DBNull.Value ? dt.Rows[i]["Att2"].ToString() : "";
                            studentResult.Att3 = dt.Rows[i]["Att3"] != DBNull.Value ? dt.Rows[i]["Att3"].ToString() : "";
                            studentResult.Att4 = dt.Rows[i]["Att4"] != DBNull.Value ? dt.Rows[i]["Att4"].ToString() : "";
                            studentResult.Att5 = dt.Rows[i]["Att5"] != DBNull.Value ? dt.Rows[i]["Att5"].ToString() : "";
                            studentResult.Att6 = dt.Rows[i]["Att6"] != DBNull.Value ? dt.Rows[i]["Att6"].ToString() : "";
                            studentResult.Att7 = dt.Rows[i]["Att7"] != DBNull.Value ? dt.Rows[i]["Att7"].ToString() : "";
                            studentResult.Att8 = dt.Rows[i]["Att8"] != DBNull.Value ? dt.Rows[i]["Att8"].ToString() : "";
                            studentResult.Att9 = dt.Rows[i]["Att9"] != DBNull.Value ? dt.Rows[i]["Att9"].ToString() : "";

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.TestTypeId = assessment.TestType.HasValue ? assessment.TestType.Value : 1;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    currentFieldScore.ResultGradeId = Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;

                                    // add state test data grade
                                    if (assessment.TestType == 3)
                                    {
                                        currentFieldScore.StateGradeId = dt.Rows[i][assessment.StorageTable + "_GradeId"] == DBNull.Value ? "0" : dt.Rows[i][assessment.StorageTable + "_GradeId"].ToString();
                                    }
                                    else
                                    {
                                        currentFieldScore.StateGradeId = "0";

                                        if (dt.Columns.Contains(assessment.StorageTable + "_IsCopied"))
                                        {
                                            if (dt.Rows[i][assessment.StorageTable + "_IsCopied"] != DBNull.Value)
                                            {
                                                currentFieldScore.IsCopied = Boolean.Parse(dt.Rows[i][assessment.StorageTable + "_IsCopied"].ToString());
                                            }
                                        }
                                    }

                                    switch (currentField.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error in GetClassObservationSummaryMultipleData {0} : SQL is {1}", ex.Message, command.GetGeneratedQuery());
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            groupResults.StudentResults = groupResults.StudentResults.OrderBy(p => p.StudentName).ToList();
            return groupResults;
        }

        public ObservationSummaryGroupResults GetClassObservationSummaryData(List<Assessment> assessments, int classId,
                int benchmarkDateId, DateTime testDate, int staffId)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();
            int gradeId = 0;
            int testLevelPeriodId = 0;
            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

            // get all the attributes
            var allAttributes = this.StudentAttributeTypes.Where(p => p.Id != 4).OrderBy(p => p.Id).ToList();

            for (var i = 0; i < allAttributes.Count; i++)
            {
                var Id = allAttributes[i].Id;
                switch (i)
                {
                    case 0:
                        groupResults.Att1Header = allAttributes[i].AttributeName;
                        groupResults.Att1Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 1:
                        groupResults.Att2Header = allAttributes[i].AttributeName;
                        groupResults.Att2Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 2:
                        groupResults.Att3Header = allAttributes[i].AttributeName;
                        groupResults.Att3Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 3:
                        groupResults.Att4Header = allAttributes[i].AttributeName;
                        groupResults.Att4Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 4:
                        groupResults.Att5Header = allAttributes[i].AttributeName;
                        groupResults.Att5Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 5:
                        groupResults.Att6Header = allAttributes[i].AttributeName;
                        groupResults.Att6Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 6:
                        groupResults.Att7Header = allAttributes[i].AttributeName;
                        groupResults.Att7Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 7:
                        groupResults.Att8Header = allAttributes[i].AttributeName;
                        groupResults.Att8Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                    case 8:
                        groupResults.Att9Header = allAttributes[i].AttributeName;
                        groupResults.Att9Visible = this.StaffStudentAttributes.FirstOrDefault(p => p.StaffId == staffId && p.AttributeId == Id) == null ? false : true;
                        break;
                }
            }

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // remove fields that shouldn't be shown in this view
                        assessment.Fields = GetViewableFields(assessment, "observationsummary", staffId);
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };
                        groupResults.HeaderGroups.Add(currentHeaderGroup);
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId,
                                CalculationFields = currentField.CalculationFields,
                                CalculationFunction = currentField.CalculationFunction
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }
                    }


                    Database.Connection.Open();
                    //command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryScores @TableNames='{0}',@TestDueDate='{1}',@TestDueDateID={2},@SectionID={3}", commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    //command.CommandType = CommandType.Text;
                    //command.CommandTimeout = command.Connection.ConnectionTimeout;

                    var schoolStartYear = TestDueDates.First(p => p.Id == benchmarkDateId).SchoolStartYear;

                    command.CommandText = @"EXEC dbo.ns4_GetFilteredObservationSummaryScores @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd,@TeamMeeting, @TextLevelZone";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("@TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("@TestDueDateID", SqlDbType.Int) { Value = (object)benchmarkDateId ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@SchoolStartYear", schoolStartYear));
                    command.Parameters.Add(new SqlParameter("@Schools", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Sections", classId));
                    command.Parameters.Add(new SqlParameter("@Grades", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Teachers", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues0", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue0", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues1", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue1", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues2", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue2", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues3", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue3", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues4", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue4", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues5", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue5", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues6", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue6", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues7", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue7", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues8", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue8", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeValues9", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@StudentAttributeIdValue9", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@SpecialEd", SqlDbType.VarChar) { Value = DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@TeamMeeting", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@TextLevelZone", DBNull.Value));
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 45;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.TestDueDateId = benchmarkDateId;//result.GetPropValue<int>("TestDueDateID");
                                                                          
                            // added on 9/26/2020 - need to show all attributes
                            studentResult.Services = dt.Rows[i]["StudentServices"] != DBNull.Value ? dt.Rows[i]["StudentServices"].ToString() : "";
                            studentResult.SpecialED = dt.Rows[i]["SPEDLabels"] != DBNull.Value ? dt.Rows[i]["SPEDLabels"].ToString() : "";
                            studentResult.Att1 = dt.Rows[i]["Att1"] != DBNull.Value ? dt.Rows[i]["Att1"].ToString() : "";
                            studentResult.Att2 = dt.Rows[i]["Att2"] != DBNull.Value ? dt.Rows[i]["Att2"].ToString() : "";
                            studentResult.Att3 = dt.Rows[i]["Att3"] != DBNull.Value ? dt.Rows[i]["Att3"].ToString() : "";
                            studentResult.Att4 = dt.Rows[i]["Att4"] != DBNull.Value ? dt.Rows[i]["Att4"].ToString() : "";
                            studentResult.Att5 = dt.Rows[i]["Att5"] != DBNull.Value ? dt.Rows[i]["Att5"].ToString() : "";
                            studentResult.Att6 = dt.Rows[i]["Att6"] != DBNull.Value ? dt.Rows[i]["Att6"].ToString() : "";
                            studentResult.Att7 = dt.Rows[i]["Att7"] != DBNull.Value ? dt.Rows[i]["Att7"].ToString() : "";
                            studentResult.Att8 = dt.Rows[i]["Att8"] != DBNull.Value ? dt.Rows[i]["Att8"].ToString() : "";
                            studentResult.Att9 = dt.Rows[i]["Att9"] != DBNull.Value ? dt.Rows[i]["Att9"].ToString() : "";

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.TestTypeId = assessment.TestType.HasValue ? assessment.TestType.Value : 1;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;

                                    // add state test data grade
                                    if (assessment.TestType == 3)
                                    {
                                        currentFieldScore.StateGradeId = dt.Rows[i][assessment.StorageTable + "_GradeId"] == DBNull.Value ? "0" : dt.Rows[i][assessment.StorageTable + "_GradeId"].ToString();
                                    }
                                    else
                                    {
                                        currentFieldScore.StateGradeId = "0";

                                        if (dt.Columns.Contains(assessment.StorageTable + "_IsCopied"))
                                        {
                                            if (dt.Rows[i][assessment.StorageTable + "_IsCopied"] != DBNull.Value)
                                            {
                                                currentFieldScore.IsCopied = Boolean.Parse(dt.Rows[i][assessment.StorageTable + "_IsCopied"].ToString());
                                            }
                                        }
                                    }

                                    switch (currentField.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error in GetClassObservationSummaryData {0} : SQL is {1}", ex.Message, command.GetGeneratedQuery());
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            groupResults.StudentResults = groupResults.StudentResults.OrderBy(p => p.StudentName).ToList();
            return groupResults;
        }

        public ObservationSummaryGroupResults GetTeamMeetingObservationSummaryData(List<Assessment> assessments, int teamMeetingId,
                int benchmarkDateId, int? staffId, int currentStaffId)
        {
            ObservationSummaryGroupResults groupResults = new ObservationSummaryGroupResults();
            groupResults.StudentResults = new List<ObservationSummaryStudentResult>();

            string commaTerminatedAssessments = string.Empty;
            commaTerminatedAssessments = string.Join(",", assessments.Select(p => p.StorageTable).ToArray()) + ",";

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    // set up headers
                    foreach (Assessment assessment in assessments)
                    {
                        // remove fields that shouldn't be shown in this view
                        assessment.Fields = GetViewableFields(assessment, "observationsummary", currentStaffId);
                        // TODO: Add a sort order for Assessments
                        var currentHeaderGroup = new ObservationSummaryAssessmentHeaderGroup()
                        {
                            AssessmentId = assessment.Id,
                            AssessmentName =
                                                         assessment.AssessmentName,
                            AssessmentOrder = 5
                        };
                        groupResults.HeaderGroups.Add(currentHeaderGroup);
                        foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                        {
                            var currentHeader = new ObservationSummaryAssessmentHeader()
                            {
                                AssessmentName = currentHeaderGroup.AssessmentName,
                                FieldName = currentField.ObsSummaryLabel,
                                FieldOrder = currentField.FieldOrder,
                                LookupFieldName = currentField.LookupFieldName,
                                DatabaseColumn = currentField.DatabaseColumn,
                                FieldType = currentField.FieldType,
                                Id = currentField.Id,
                                AssessmentId = currentHeaderGroup.AssessmentId
                            };
                            groupResults.Fields.Add(currentHeader);
                            currentHeaderGroup.FieldCount++;
                        }
                    }


                    Database.Connection.Open();
                    //command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryScores_ForTM @TableNames='{0}',@TeamMeetingID={1},@TestDueDateID={2},@StaffID={3}", commaTerminatedAssessments, teamMeetingId, benchmarkDateId, staffId.HasValue && staffId.Value != 0 ? staffId.Value.ToString() : "NULL");
                    //command.CommandType = CommandType.Text;
                    //command.CommandTimeout = command.Connection.ConnectionTimeout;

                    var schoolStartYear = TestDueDates.First(p => p.Id == benchmarkDateId).SchoolStartYear;

                    command.CommandText = @"EXEC dbo.ns4_GetFilteredObservationSummaryScores @TableNames, @TestDueDateID, @SchoolStartYear, 
                        @Schools, @Grades, @Teachers, @Sections,
                        @StudentAttributeIdValue0, @StudentAttributeValues0,
                        @StudentAttributeIdValue1, @StudentAttributeValues1,
                        @StudentAttributeIdValue2, @StudentAttributeValues2,
                        @StudentAttributeIdValue3, @StudentAttributeValues3,
                        @StudentAttributeIdValue4, @StudentAttributeValues4,
                        @StudentAttributeIdValue5, @StudentAttributeValues5,
                        @StudentAttributeIdValue6, @StudentAttributeValues6,
                        @StudentAttributeIdValue7, @StudentAttributeValues7,
                        @StudentAttributeIdValue8, @StudentAttributeValues8,
                        @StudentAttributeIdValue9, @StudentAttributeValues9,
                         @InterventionTypes,@SpecialEd, @TeamMeeting, @TextLevelZone";//, commaTerminatedAssessments, DateTime.Now.ToShortDateString(), benchmarkDateId, classId);
                    command.Parameters.Add(new SqlParameter("TableNames", commaTerminatedAssessments));
                    command.Parameters.Add(new SqlParameter("TestDueDateID", SqlDbType.Int) { Value = (object)benchmarkDateId ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("SchoolStartYear", schoolStartYear));
                    command.Parameters.Add(new SqlParameter("Schools", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("Sections", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("Grades", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("Teachers", staffId.HasValue && staffId.Value != 0 ? (object)staffId.Value.ToString() : DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues0", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue0", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues1", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue1", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues2", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue2", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues3", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue3", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues4", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue4", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues5", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue5", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues6", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue6", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues7", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue7", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues8", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue8", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeValues9", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("StudentAttributeIdValue9", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@InterventionTypes", DBNull.Value));
                    command.Parameters.Add(new SqlParameter("SpecialEd", SqlDbType.VarChar) { Value = DBNull.Value });
                    command.Parameters.Add(new SqlParameter("TeamMeeting", teamMeetingId));
                    command.Parameters.Add(new SqlParameter("TextLevelZone", DBNull.Value));

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryStudentResult studentResult = new ObservationSummaryStudentResult();
                            groupResults.StudentResults.Add(studentResult);
                            studentResult.StudentId = Int32.Parse(dt.Rows[i]["RealStudentID"].ToString());
                            studentResult.GradeId = Int32.Parse(dt.Rows[i]["GradeID"].ToString());
                            studentResult.GradeOrder = Int32.Parse(dt.Rows[i]["GradeOrder"].ToString());
                            studentResult.GradeName = dt.Rows[i]["GradeName"]?.ToString();
                            studentResult.StudentName = dt.Rows[i]["LastName"].ToString() + ", " + dt.Rows[i]["FirstName"].ToString();
                            studentResult.DelimitedTeachers = dt.Rows[i]["DelimitedTeachers"]?.ToString();
                            studentResult.NoteCount = Int32.Parse(dt.Rows[i]["NoteCount"].ToString());
                            studentResult.TestDueDateId = benchmarkDateId;//result.GetPropValue<int>("TestDueDateID");

                            // now create the fields that hold the scores for each assessment
                            List<ObservationSummaryFieldScore> fieldScores = new List<ObservationSummaryFieldScore>();
                            studentResult.OSFieldResults = fieldScores;

                            // not right now, but think through the case of assessment with the same field names... like accuracy in two assessments
                            // need to prefix the field names
                            foreach (Assessment assessment in assessments)
                            {
                                foreach (var currentField in assessment.Fields.OrderBy(p => p.FieldOrder))
                                {
                                    var currentFieldScore = new ObservationSummaryFieldScore();
                                    fieldScores.Add(currentFieldScore);
                                    currentFieldScore.LookupFieldName = currentField.LookupFieldName;
                                    currentFieldScore.AssessmentId = assessment.Id;
                                    currentFieldScore.DbColumn = currentField.DatabaseColumn;
                                    currentFieldScore.ColumnType = currentField.FieldType;
                                    currentFieldScore.FieldOrder = currentField.FieldOrder;
                                    var currentColumn = assessment.StorageTable + "_" + currentField.DatabaseColumn;
                                    switch (currentField.FieldType)
                                    {
                                        case "Textfield":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "DecimalRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.DecimalValue = Decimal.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownRange":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "DropdownFromDB":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "checklist":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbBacked":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.IntValue = Int32.Parse(dt.Rows[i][currentColumn].ToString());
                                            }
                                            break;
                                        case "CalculatedFieldDbBackedString":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldDbOnly":
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                        case "CalculatedFieldClientOnly":
                                            // no-op
                                            break;
                                        default:
                                            if (dt.Rows[i][currentColumn] != DBNull.Value)
                                            {
                                                currentFieldScore.StringValue = dt.Rows[i][currentColumn].ToString();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Error loading Attend Team Meeting Observation Summary: {0} - SQL: {1}", ex.Message, command.GetGeneratedQuery());
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            groupResults.StudentResults = groupResults.StudentResults.OrderBy(p => p.StudentName).ToList();
            return groupResults;
        }

        public List<ObservationSummaryBenchmark> GetClassObservationSummaryBenchmarks(List<Assessment> assessments, int benchmarkDateId, int gradeId)
        {
            int testLevelPeriodId = this.TestDueDates.First(p => p.Id == benchmarkDateId).TestLevelPeriodID.Value;

            return GetClassObservationSummaryBenchmarks(assessments, benchmarkDateId, gradeId, testLevelPeriodId);
        }

        public List<ObservationSummaryBenchmark> GetAllObservationSummaryBenchmarks(List<Assessment> assessments)
        {
            // get grade, testlevelperiod and delimited string of assessementId|fieldName,
            List<ObservationSummaryBenchmark> lstBenchmarkData = new List<ObservationSummaryBenchmark>();
            StringBuilder delimitedAssessmentFields = new StringBuilder();
            foreach (var assessment in assessments)
            {
                foreach (var field in assessment.Fields)
                {
                    delimitedAssessmentFields.AppendFormat("{0}|{1},", assessment.Id, field.DatabaseColumn);
                }
            }

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    Database.Connection.Open();
                    command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryAllBenchmarks @FieldsWithAssessments='{0}'", delimitedAssessmentFields.ToString());
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryBenchmark result = new ObservationSummaryBenchmark();
                            result.AssessmentId = Int32.Parse(dt.Rows[i]["AssessmentID"].ToString());
                            result.GradeId = Int32.Parse(dt.Rows[i]["GradeId"].ToString());
                            result.TestLevelPeriodId = Int32.Parse(dt.Rows[i]["TestLevelPeriodId"].ToString());
                            result.DbColumn = dt.Rows[i]["FieldName"].ToString();
                            result.Exceeds = dt.Rows[i]["Exceeds"] as decimal?;
                            result.Meets = dt.Rows[i]["Meets"] as decimal?;
                            result.Approaches = dt.Rows[i]["Approaches"] as decimal?;
                            result.DoesNotMeet = dt.Rows[i]["DoesNotMeet"] as decimal?;
                            lstBenchmarkData.Add(result);
                        }
                    }

                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return lstBenchmarkData;
        }



        public List<ObservationSummaryBenchmark> GetClassObservationSummaryBenchmarks(List<Assessment> assessments, int benchmarkDateId, int gradeId, int? testLevelPeriodId)
        {
            // get grade, testlevelperiod and delimited string of assessementId|fieldName,
            List<ObservationSummaryBenchmark> lstBenchmarkData = new List<ObservationSummaryBenchmark>();
            StringBuilder delimitedAssessmentFields = new StringBuilder();
            foreach (var assessment in assessments)
            {
                foreach (var field in assessment.Fields)
                {
                    delimitedAssessmentFields.AppendFormat("{0}|{1},", assessment.Id, field.DatabaseColumn);
                }
            }

            // calculate benchmark value and return
            using (System.Data.IDbCommand command = Database.Connection.CreateCommand())
            {
                try
                {
                    Database.Connection.Open();
                    command.CommandText = String.Format("EXEC dbo.ns4_GetObservationSummaryBenchmarks @FieldsWithAssessments='{0}',@GradeID={1},@TestLevelPeriodID={2}", delimitedAssessmentFields.ToString(), gradeId, testLevelPeriodId.HasValue ? testLevelPeriodId.ToString() : "NULL");
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = command.Connection.ConnectionTimeout;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        // load datatable
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            ObservationSummaryBenchmark result = new ObservationSummaryBenchmark();
                            result.AssessmentId = Int32.Parse(dt.Rows[i]["AssessmentID"].ToString());
                            result.GradeId = Int32.Parse(dt.Rows[i]["GradeId"].ToString());
                            result.DbColumn = dt.Rows[i]["FieldName"].ToString();
                            result.Exceeds = dt.Rows[i]["Exceeds"] as decimal?;
                            result.Meets = dt.Rows[i]["Meets"] as decimal?;
                            result.Approaches = dt.Rows[i]["Approaches"] as decimal?;
                            result.DoesNotMeet = dt.Rows[i]["DoesNotMeet"] as decimal?;
                            result.TestLevelPeriodId = Int32.Parse(dt.Rows[i]["TestLevelPeriodId"].ToString()); ;
                            lstBenchmarkData.Add(result);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Error in creating Bencmarks: {0}", ex.Message);
                }
                finally
                {
                    Database.Connection.Close();
                    command.Parameters.Clear();
                }
            }

            return lstBenchmarkData;
        }


        #endregion
        //private SqlParameter CreateNullableVarCharSqlParameter(string paramValue, string paramName)
        //{
        //    SqlParameter newParam = new SqlParameter();
        //    newParam.SqlDbType = SqlDbType.VarChar;
        //    newParam.ParameterName = paramName;

        //    if (paramValue == null)
        //    {
        //        newParam.Value = DBNull.Value;
        //    }
        //    else
        //    {
        //        newParam.Value = paramValue;
        //    }

        //    return newParam;
        //}
        //private SqlParameter CreateNullableIntSqlParameter(int? paramValue, string paramName)
        //{
        //    SqlParameter newParam = new SqlParameter();
        //    newParam.SqlDbType = SqlDbType.Int;
        //    newParam.ParameterName = paramName;

        //    if (paramValue == null)
        //    {
        //        newParam.Value = DBNull.Value;
        //    }
        //    else
        //    {
        //        newParam.Value = paramValue;
        //    }

        //    return newParam;
        //}

    }
}
