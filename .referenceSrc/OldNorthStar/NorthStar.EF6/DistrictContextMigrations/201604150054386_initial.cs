namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Assessment_Benchmarks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                        TestLevelPeriodID = c.Int(nullable: false),
                        AssessmentField = c.String(unicode: false),
                        DoesNotMeet = c.Decimal(precision: 10, scale: 2),
                        Approaches = c.Decimal(precision: 10, scale: 2),
                        Meets = c.Decimal(precision: 10, scale: 2),
                        Exceeds = c.Decimal(precision: 10, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentID)
                .ForeignKey("dbo.Grade", t => t.GradeID)
                .ForeignKey("dbo.TestLevelPeriod", t => t.TestLevelPeriodID)
                .Index(t => t.AssessmentID)
                .Index(t => t.GradeID)
                .Index(t => t.TestLevelPeriodID);
            
            CreateTable(
                "dbo.Assessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentIsAvailable = c.Boolean(),
                        StorageTable = c.String(),
                        SecondaryStorageTable = c.String(),
                        TertiaryStorageTable = c.String(),
                        IsStateTest = c.Boolean(),
                        IsHFW = c.Boolean(),
                        IsProgressMonitoring = c.Boolean(),
                        AssessmentName = c.String(),
                        AssessmentDescription = c.String(),
                        DefaultDataEntryPage = c.String(),
                        DataEntryPages = c.String(),
                        DefaultClassReportPage = c.String(),
                        ClassReportPages = c.String(),
                        TestType = c.Int(),
                        BaseType = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AssessmentFieldCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        AltOrder = c.Int(),
                        DisplayName = c.String(),
                        AltDisplayLabel = c.String(),
                        Description = c.String(),
                        PreviousId = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.AssessmentId);
            
            CreateTable(
                "dbo.AssessmentField",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StorageTable = c.String(),
                        DisplayLabel = c.String(),
                        AltDisplayLabel = c.String(),
                        FieldType = c.String(),
                        DefaultValue = c.String(),
                        IsRequired = c.Boolean(nullable: false),
                        CategoryId = c.Int(),
                        SubcategoryId = c.Int(),
                        GroupId = c.Int(),
                        Page = c.Int(nullable: false),
                        FieldOrder = c.Int(nullable: false),
                        AltOrder = c.Int(),
                        Description = c.String(),
                        LookupFieldName = c.String(),
                        RangeHigh = c.Int(nullable: false),
                        RangeLow = c.Int(nullable: false),
                        DatabaseColumn = c.String(),
                        CalculationFunction = c.String(),
                        CalculationFields = c.String(),
                        AssessmentId = c.Int(nullable: false),
                        DisplayInObsSummary = c.Boolean(),
                        DisplayInEditResultList = c.Boolean(),
                        DisplayInLineGraphs = c.Boolean(),
                        OutOfHowMany = c.Int(),
                        PreviousId = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AssessmentFieldCategory", t => t.CategoryId)
                .ForeignKey("dbo.AssessmentFieldGroup", t => t.GroupId)
                .ForeignKey("dbo.AssessmentFieldSubCategory", t => t.SubcategoryId)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.SubcategoryId)
                .Index(t => t.GroupId)
                .Index(t => t.AssessmentId);
            
            CreateTable(
                "dbo.AssessmentFieldGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        AltOrder = c.Int(),
                        DisplayName = c.String(),
                        AltDisplayLabel = c.String(),
                        Description = c.String(),
                        PreviousId = c.Int(),
                        IsKdg = c.Boolean(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.AssessmentId);
            
            CreateTable(
                "dbo.StaffObservationSummaryAssessmentField",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        AssessmentFieldId = c.Int(nullable: false),
                        StaffId = c.Int(nullable: false),
                        Hidden = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .ForeignKey("dbo.AssessmentField", t => t.AssessmentFieldId, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.AssessmentId)
                .Index(t => t.AssessmentFieldId)
                .Index(t => t.StaffId);
            
            CreateTable(
                "dbo.Staff",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherIdentifier = c.String(unicode: false),
                        LoweredUserName = c.String(unicode: false),
                        FirstName = c.String(unicode: false),
                        MiddleName = c.String(unicode: false),
                        LastName = c.String(unicode: false),
                        NorthStarUserTypeID = c.Int(),
                        Notes = c.String(unicode: false),
                        RoleID = c.Int(nullable: false),
                        Email = c.String(unicode: false),
                        IsInterventionSpecialist = c.Boolean(nullable: false),
                        NavigationFavorites = c.String(),
                        RolesLastUpdated = c.DateTime(),
                        IsActive = c.Boolean(),
                        IsDistrictAdmin = c.Boolean(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AttendeeGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupName = c.String(),
                        StaffId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Staff", t => t.StaffId)
                .Index(t => t.StaffId);
            
            CreateTable(
                "dbo.AttendeeGroupStaff",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StaffId = c.Int(nullable: false),
                        AttendeeGroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttendeeGroup", t => t.AttendeeGroupId, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.StaffId)
                .Index(t => t.AttendeeGroupId);
            
            CreateTable(
                "dbo.InterventionAttendance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SectionID = c.Int(nullable: false),
                        Notes = c.String(),
                        AttendanceDate = c.DateTime(nullable: false),
                        AttendanceReasonID = c.Int(nullable: false),
                        StudentID = c.Int(nullable: false),
                        RecorderID = c.Int(nullable: false),
                        Contact = c.Boolean(),
                        ClassStartEndID = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AttendanceReason", t => t.AttendanceReasonID, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.RecorderID)
                .Index(t => t.AttendanceReasonID)
                .Index(t => t.RecorderID);
            
            CreateTable(
                "dbo.AttendanceReason",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Reason = c.String(),
                        ValidForScheduledDays = c.Boolean(nullable: false),
                        CountsAsAbsense = c.Boolean(nullable: false),
                        ValidForNonScheduledDays = c.Boolean(nullable: false),
                        CountsAsBonusDay = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Section",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        StaffID = c.Int(nullable: false),
                        SchoolStartYear = c.Int(nullable: false),
                        SchoolID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                        SectionDataTypeID = c.Int(nullable: false),
                        InterventionTypeID = c.Int(),
                        InterventionTierID = c.Int(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        StaffTimeSlotID = c.Int(),
                        IsInterventionGroup = c.Boolean(nullable: false),
                        InterventionDistrictID = c.Int(),
                        MondayMeet = c.Boolean(),
                        TuesdayMeet = c.Boolean(),
                        WednesdayMeet = c.Boolean(),
                        ThursdayMeet = c.Boolean(),
                        FridayMeet = c.Boolean(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grade", t => t.GradeID)
                .ForeignKey("dbo.School", t => t.SchoolID)
                .ForeignKey("dbo.InterventionType", t => t.InterventionTypeID)
                .ForeignKey("dbo.SchoolYear", t => t.SchoolStartYear, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffID)
                .Index(t => t.StaffID)
                .Index(t => t.SchoolStartYear)
                .Index(t => t.SchoolID)
                .Index(t => t.GradeID)
                .Index(t => t.InterventionTypeID);
            
            CreateTable(
                "dbo.Grade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShortName = c.String(),
                        LongName = c.String(),
                        GradeOrder = c.Int(nullable: false),
                        StateGradeNumber = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionType = c.String(unicode: false),
                        bDisplay = c.Boolean(nullable: false),
                        Description = c.String(unicode: false),
                        DefaultTextLevelType = c.Int(),
                        InterventionCardinalityID = c.Int(),
                        ExitCriteria = c.String(unicode: false),
                        EntranceCriteria = c.String(unicode: false),
                        LearnerNeed = c.String(unicode: false),
                        DetailedDescription = c.String(unicode: false),
                        TimeOfYear = c.String(unicode: false),
                        InterventionTierID = c.Int(),
                        CategoryID = c.Int(),
                        BriefDescription = c.String(unicode: false),
                        FrameworkID = c.Int(),
                        UnitOfStudyID = c.Int(),
                        WorkshopID = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InterventionCardinality", t => t.InterventionCardinalityID)
                .ForeignKey("dbo.InterventionCategory", t => t.CategoryID)
                .ForeignKey("dbo.InterventionFramework", t => t.FrameworkID)
                .ForeignKey("dbo.InterventionTier", t => t.InterventionTierID)
                .ForeignKey("dbo.InterventionUnitOfStudy", t => t.UnitOfStudyID)
                .ForeignKey("dbo.InterventionWorkshop", t => t.WorkshopID)
                .Index(t => t.InterventionCardinalityID)
                .Index(t => t.InterventionTierID)
                .Index(t => t.CategoryID)
                .Index(t => t.FrameworkID)
                .Index(t => t.UnitOfStudyID)
                .Index(t => t.WorkshopID);
            
            CreateTable(
                "dbo.InterventionCardinality",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CardinalityName = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(unicode: false),
                        CategoryDescription = c.String(unicode: false),
                        SortOrder = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionFramework",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FrameworkName = c.String(unicode: false),
                        FreameworkDescription = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionGrade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grade", t => t.GradeID, cascadeDelete: true)
                .ForeignKey("dbo.InterventionType", t => t.InterventionID, cascadeDelete: true)
                .Index(t => t.InterventionID)
                .Index(t => t.GradeID);
            
            CreateTable(
                "dbo.InterventionGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        StaffID = c.Int(nullable: false),
                        SchoolStartYear = c.Int(nullable: false),
                        SchoolID = c.Int(nullable: false),
                        SectionDataTypeID = c.Int(nullable: false),
                        InterventionTypeID = c.Int(),
                        InterventionTierID = c.Int(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        StaffTimeSlotID = c.Int(),
                        IsInterventionGroup = c.Boolean(nullable: false),
                        InterventionDistrictID = c.Int(),
                        MondayMeet = c.Boolean(),
                        TuesdayMeet = c.Boolean(),
                        WednesdayMeet = c.Boolean(),
                        ThursdayMeet = c.Boolean(),
                        FridayMeet = c.Boolean(),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InterventionType", t => t.InterventionTypeID)
                .ForeignKey("dbo.School", t => t.SchoolID)
                .ForeignKey("dbo.SchoolYear", t => t.SchoolStartYear, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .Index(t => t.StaffID)
                .Index(t => t.SchoolStartYear)
                .Index(t => t.SchoolID)
                .Index(t => t.InterventionTypeID);
            
            CreateTable(
                "dbo.School",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SchoolCalendar",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SchoolID = c.Int(nullable: false),
                        Subject = c.String(unicode: false),
                        Description = c.String(),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.School", t => t.SchoolID)
                .Index(t => t.SchoolID);
            
            CreateTable(
                "dbo.StaffSchool",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SchoolID = c.Int(nullable: false),
                        StaffID = c.Int(nullable: false),
                        StaffHierarchyPermissionID = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.School", t => t.SchoolID)
                .ForeignKey("dbo.Staff", t => t.StaffID)
                .Index(t => t.SchoolID)
                .Index(t => t.StaffID);
            
            CreateTable(
                "dbo.StudentSchool",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentID = c.Int(nullable: false),
                        SchoolID = c.Int(nullable: false),
                        SchoolStartYear = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SchoolYear", t => t.SchoolStartYear, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.StudentID)
                .ForeignKey("dbo.School", t => t.SchoolID)
                .Index(t => t.StudentID)
                .Index(t => t.SchoolID)
                .Index(t => t.SchoolStartYear);
            
            CreateTable(
                "dbo.SchoolYear",
                c => new
                    {
                        SchoolStartYear = c.Int(nullable: false),
                        YearVerbose = c.String(),
                        SchoolEndYear = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.SchoolStartYear);
            
            CreateTable(
                "dbo.Student",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(unicode: false),
                        MiddleName = c.String(unicode: false),
                        LastName = c.String(unicode: false),
                        DOB = c.DateTime(),
                        GradYear = c.Int(),
                        StudentIdentifier = c.String(unicode: false),
                        TitleOnetypeID = c.Int(),
                        Comment = c.String(),
                        EthnicityID = c.Int(),
                        Gender = c.String(),
                        IsActive = c.Boolean(),
                        GenderID = c.Int(),
                        DistrictID = c.Int(),
                        ELL = c.Boolean(),
                        ADSIS = c.Boolean(),
                        Gifted = c.Boolean(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudentAttributeData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentID = c.Int(nullable: false),
                        AttributeID = c.Int(nullable: false),
                        AttributeValueID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StudentAttributeType", t => t.AttributeID, cascadeDelete: true)
                .ForeignKey("dbo.StudentAttributeLookupValue", t => t.AttributeValueID, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.StudentID)
                .Index(t => t.StudentID)
                .Index(t => t.AttributeID)
                .Index(t => t.AttributeValueID);
            
            CreateTable(
                "dbo.StudentAttributeType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudentAttributeLookupValue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttributeID = c.Int(nullable: false),
                        LookupValueID = c.Int(),
                        LookupValue = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StudentAttributeType", t => t.AttributeID, cascadeDelete: true)
                .Index(t => t.AttributeID);
            
            CreateTable(
                "dbo.StudentSection",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentID = c.Int(nullable: false),
                        ClassID = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        LastAssociatedTDDID = c.Int(),
                        Notes = c.String(),
                        LastAssociatedTDD = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Student", t => t.StudentID)
                .ForeignKey("dbo.Section", t => t.ClassID)
                .Index(t => t.StudentID)
                .Index(t => t.ClassID);
            
            CreateTable(
                "dbo.StaffInterventionGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StaffID = c.Int(nullable: false),
                        InterventionGroupId = c.Int(nullable: false),
                        StaffHierarchyPermissionID = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .ForeignKey("dbo.InterventionGroup", t => t.InterventionGroupId)
                .Index(t => t.StaffID)
                .Index(t => t.InterventionGroupId);
            
            CreateTable(
                "dbo.StudentInterventionGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentID = c.Int(nullable: false),
                        InterventionGroupId = c.Int(nullable: false),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        LastAssociatedTDDID = c.Int(),
                        Notes = c.String(),
                        LastAssociatedTDD = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Student", t => t.StudentID, cascadeDelete: true)
                .ForeignKey("dbo.InterventionGroup", t => t.InterventionGroupId)
                .Index(t => t.StudentID)
                .Index(t => t.InterventionGroupId);
            
            CreateTable(
                "dbo.InterventionTier",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TierValue = c.Int(nullable: false),
                        Description = c.String(unicode: false),
                        TierName = c.String(unicode: false),
                        TierLabel = c.String(unicode: false),
                        TierColor = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionToolIntervention",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterventionID = c.Int(nullable: false),
                        InterventionToolID = c.Int(nullable: false),
                        SortOrder = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InterventionTool", t => t.InterventionToolID)
                .ForeignKey("dbo.InterventionType", t => t.InterventionID)
                .Index(t => t.InterventionID)
                .Index(t => t.InterventionToolID);
            
            CreateTable(
                "dbo.InterventionTool",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ToolName = c.String(unicode: false),
                        ToolFileName = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        SortOrder = c.Int(),
                        FileSystemFileName = c.String(unicode: false),
                        LastModified = c.DateTime(),
                        FileSize = c.Int(),
                        FileExtension = c.String(unicode: false),
                        ToolTypeID = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InterventionToolType", t => t.ToolTypeID)
                .Index(t => t.ToolTypeID);
            
            CreateTable(
                "dbo.InterventionToolType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionUnitOfStudy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UnitName = c.String(unicode: false),
                        UnitDescription = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InterventionWorkshop",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkshopName = c.String(unicode: false),
                        WorkshopDescription = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StaffSection",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StaffID = c.Int(nullable: false),
                        ClassID = c.Int(nullable: false),
                        StaffHierarchyPermissionID = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Section", t => t.ClassID)
                .ForeignKey("dbo.Staff", t => t.StaffID)
                .Index(t => t.StaffID)
                .Index(t => t.ClassID);
            
            CreateTable(
                "dbo.StaffObservationSummaryAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        StaffId = c.Int(nullable: false),
                        Hidden = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.AssessmentId)
                .Index(t => t.StaffId);
            
            CreateTable(
                "dbo.StaffSchoolGrade",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SchoolID = c.Int(nullable: false),
                        StaffID = c.Int(nullable: false),
                        StaffHierarchyPermissionID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grade", t => t.GradeID, cascadeDelete: true)
                .ForeignKey("dbo.School", t => t.SchoolID, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .Index(t => t.SchoolID)
                .Index(t => t.StaffID)
                .Index(t => t.GradeID);
            
            CreateTable(
                "dbo.AssessmentFieldSubCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        AltOrder = c.Int(),
                        DisplayName = c.String(),
                        AltDisplayLabel = c.String(),
                        Description = c.String(),
                        PreviousId = c.Int(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.AssessmentId);
            
            CreateTable(
                "dbo.TestLevelPeriod",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        PeriodOrder = c.Int(nullable: false),
                        Notes = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.District_Benchmark",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentID = c.Int(nullable: false),
                        GradeID = c.Int(nullable: false),
                        TestLevelPeriodID = c.Int(nullable: false),
                        AssessmentField = c.String(),
                        DoesNotMeet = c.Decimal(precision: 18, scale: 2),
                        Approaches = c.Decimal(precision: 18, scale: 2),
                        Meets = c.Decimal(precision: 18, scale: 2),
                        Exceeds = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentID, cascadeDelete: true)
                .ForeignKey("dbo.Grade", t => t.GradeID, cascadeDelete: true)
                .ForeignKey("dbo.TestLevelPeriod", t => t.TestLevelPeriodID, cascadeDelete: true)
                .Index(t => t.AssessmentID)
                .Index(t => t.GradeID)
                .Index(t => t.TestLevelPeriodID);
            
            CreateTable(
                "dbo.DistrictCalendar",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Subject = c.String(),
                        Description = c.String(),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FPComparison",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Grades = c.String(),
                        DRAs = c.String(),
                        RR = c.String(),
                        FPs = c.String(),
                        FPID = c.Int(nullable: false),
                        Lexiles = c.String(),
                        FPOrder = c.Int(nullable: false),
                        DRAID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.AssessmentLookupField",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FieldName = c.String(),
                        FieldValue = c.String(),
                        SortOrder = c.Int(nullable: false),
                        FieldSpecificId = c.Int(),
                        Description = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SchoolAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        SchoolId = c.Int(nullable: false),
                        AssessmentIsAvailable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.AssessmentId);
            
            CreateTable(
                "dbo.StaffAssessmentFieldVisibility",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StaffId = c.Int(nullable: false),
                        AssessmentId = c.Int(nullable: false),
                        AssessmentFieldId = c.Int(nullable: false),
                        DisplayInObsSummary = c.Boolean(),
                        DisplayInEditResultList = c.Boolean(),
                        DisplayInLineGraphs = c.Boolean(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .ForeignKey("dbo.AssessmentField", t => t.AssessmentFieldId, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.StaffId)
                .Index(t => t.AssessmentId)
                .Index(t => t.AssessmentFieldId);
            
            CreateTable(
                "dbo.StaffAssessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssessmentId = c.Int(nullable: false),
                        StaffId = c.Int(nullable: false),
                        AssessmentIsAvailable = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Assessment", t => t.AssessmentId, cascadeDelete: true)
                .Index(t => t.AssessmentId);
            
            CreateTable(
                "dbo.StaffSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StaffId = c.Int(nullable: false),
                        SelectedValueId = c.Int(nullable: false),
                        Attribute = c.String(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Staff", t => t.StaffId, cascadeDelete: true)
                .Index(t => t.StaffId);
            
            CreateTable(
                "dbo.TeamMeetingAttendance",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TeamMeetingID = c.Int(nullable: false),
                        StaffID = c.Int(nullable: false),
                        NoticeSent = c.DateTime(),
                        Attended = c.Boolean(nullable: false),
                        SchoolID = c.Int(),
                        IncludeAllStudents = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.School", t => t.SchoolID)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .ForeignKey("dbo.TeamMeeting", t => t.TeamMeetingID)
                .Index(t => t.TeamMeetingID)
                .Index(t => t.StaffID)
                .Index(t => t.SchoolID);
            
            CreateTable(
                "dbo.TeamMeeting",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TestDueDateId = c.Int(),
                        Title = c.String(),
                        Comments = c.String(),
                        MeetingTime = c.DateTime(nullable: false),
                        StaffID = c.Int(nullable: false),
                        SchoolYear = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .Index(t => t.StaffID);
            
            CreateTable(
                "dbo.TeamMeetingManager",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TeamMeetingID = c.Int(nullable: false),
                        StaffID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .ForeignKey("dbo.TeamMeeting", t => t.TeamMeetingID)
                .Index(t => t.TeamMeetingID)
                .Index(t => t.StaffID);
            
            CreateTable(
                "dbo.TeamMeetingStudentNote",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TeamMeetingID = c.Int(nullable: false),
                        StudentID = c.Int(nullable: false),
                        NoteDate = c.DateTime(nullable: false),
                        Note = c.String(unicode: false),
                        StaffID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.StudentID, cascadeDelete: true)
                .ForeignKey("dbo.TeamMeeting", t => t.TeamMeetingID)
                .Index(t => t.TeamMeetingID)
                .Index(t => t.StudentID)
                .Index(t => t.StaffID);
            
            CreateTable(
                "dbo.TeamMeetingStudent",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TeamMeetingID = c.Int(nullable: false),
                        StaffID = c.Int(nullable: false),
                        SectionID = c.Int(nullable: false),
                        StudentID = c.Int(nullable: false),
                        Notes = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Section", t => t.SectionID, cascadeDelete: true)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.StudentID, cascadeDelete: true)
                .ForeignKey("dbo.TeamMeeting", t => t.TeamMeetingID)
                .Index(t => t.TeamMeetingID)
                .Index(t => t.StaffID)
                .Index(t => t.SectionID)
                .Index(t => t.StudentID);
            
            CreateTable(
                "dbo.TestDueDate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SchoolStartYear = c.Int(),
                        DueDate = c.DateTime(),
                        TestLevelPeriodID = c.Int(),
                        Notes = c.String(),
                        Hex = c.String(),
                        IsSupplemental = c.Boolean(),
                        StartDate = c.DateTime(),
                        EndDate = c.DateTime(),
                        ModifiedDate = c.DateTime(),
                        Ip = c.String(),
                        ModifiedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamMeetingStudent", "TeamMeetingID", "dbo.TeamMeeting");
            DropForeignKey("dbo.TeamMeetingStudent", "StudentID", "dbo.Student");
            DropForeignKey("dbo.TeamMeetingStudent", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingStudent", "SectionID", "dbo.Section");
            DropForeignKey("dbo.TeamMeetingStudentNote", "TeamMeetingID", "dbo.TeamMeeting");
            DropForeignKey("dbo.TeamMeetingStudentNote", "StudentID", "dbo.Student");
            DropForeignKey("dbo.TeamMeetingStudentNote", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingManager", "TeamMeetingID", "dbo.TeamMeeting");
            DropForeignKey("dbo.TeamMeetingManager", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingAttendance", "TeamMeetingID", "dbo.TeamMeeting");
            DropForeignKey("dbo.TeamMeeting", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingAttendance", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingAttendance", "SchoolID", "dbo.School");
            DropForeignKey("dbo.StaffSetting", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentFieldId", "dbo.AssessmentField");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.SchoolAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.District_Benchmark", "TestLevelPeriodID", "dbo.TestLevelPeriod");
            DropForeignKey("dbo.District_Benchmark", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.District_Benchmark", "AssessmentID", "dbo.Assessment");
            DropForeignKey("dbo.Assessment_Benchmarks", "TestLevelPeriodID", "dbo.TestLevelPeriod");
            DropForeignKey("dbo.Assessment_Benchmarks", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.Assessment_Benchmarks", "AssessmentID", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldSubCategory", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentField", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldGroup", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldCategory", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentField", "SubcategoryId", "dbo.AssessmentFieldSubCategory");
            DropForeignKey("dbo.StaffSection", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StaffSchool", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StaffSchoolGrade", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StaffSchoolGrade", "SchoolID", "dbo.School");
            DropForeignKey("dbo.StaffSchoolGrade", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.StaffObservationSummaryAssessment", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffObservationSummaryAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.Section", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StudentSection", "ClassID", "dbo.Section");
            DropForeignKey("dbo.StaffSection", "ClassID", "dbo.Section");
            DropForeignKey("dbo.Section", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.Section", "InterventionTypeID", "dbo.InterventionType");
            DropForeignKey("dbo.InterventionType", "WorkshopID", "dbo.InterventionWorkshop");
            DropForeignKey("dbo.InterventionType", "UnitOfStudyID", "dbo.InterventionUnitOfStudy");
            DropForeignKey("dbo.InterventionToolIntervention", "InterventionID", "dbo.InterventionType");
            DropForeignKey("dbo.InterventionTool", "ToolTypeID", "dbo.InterventionToolType");
            DropForeignKey("dbo.InterventionToolIntervention", "InterventionToolID", "dbo.InterventionTool");
            DropForeignKey("dbo.InterventionType", "InterventionTierID", "dbo.InterventionTier");
            DropForeignKey("dbo.StudentInterventionGroup", "InterventionGroupId", "dbo.InterventionGroup");
            DropForeignKey("dbo.StudentInterventionGroup", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StaffInterventionGroup", "InterventionGroupId", "dbo.InterventionGroup");
            DropForeignKey("dbo.StaffInterventionGroup", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.InterventionGroup", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.InterventionGroup", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.StudentSchool", "SchoolID", "dbo.School");
            DropForeignKey("dbo.StudentSection", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StudentSchool", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StudentAttributeData", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StudentAttributeData", "AttributeValueID", "dbo.StudentAttributeLookupValue");
            DropForeignKey("dbo.StudentAttributeData", "AttributeID", "dbo.StudentAttributeType");
            DropForeignKey("dbo.StudentAttributeLookupValue", "AttributeID", "dbo.StudentAttributeType");
            DropForeignKey("dbo.StudentSchool", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.StaffSchool", "SchoolID", "dbo.School");
            DropForeignKey("dbo.Section", "SchoolID", "dbo.School");
            DropForeignKey("dbo.SchoolCalendar", "SchoolID", "dbo.School");
            DropForeignKey("dbo.InterventionGroup", "SchoolID", "dbo.School");
            DropForeignKey("dbo.InterventionGroup", "InterventionTypeID", "dbo.InterventionType");
            DropForeignKey("dbo.InterventionGrade", "InterventionID", "dbo.InterventionType");
            DropForeignKey("dbo.InterventionGrade", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.InterventionType", "FrameworkID", "dbo.InterventionFramework");
            DropForeignKey("dbo.InterventionType", "CategoryID", "dbo.InterventionCategory");
            DropForeignKey("dbo.InterventionType", "InterventionCardinalityID", "dbo.InterventionCardinality");
            DropForeignKey("dbo.Section", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.InterventionAttendance", "RecorderID", "dbo.Staff");
            DropForeignKey("dbo.InterventionAttendance", "AttendanceReasonID", "dbo.AttendanceReason");
            DropForeignKey("dbo.AttendeeGroup", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.AttendeeGroupStaff", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.AttendeeGroupStaff", "AttendeeGroupId", "dbo.AttendeeGroup");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentFieldId", "dbo.AssessmentField");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentField", "GroupId", "dbo.AssessmentFieldGroup");
            DropForeignKey("dbo.AssessmentField", "CategoryId", "dbo.AssessmentFieldCategory");
            DropIndex("dbo.TeamMeetingStudent", new[] { "StudentID" });
            DropIndex("dbo.TeamMeetingStudent", new[] { "SectionID" });
            DropIndex("dbo.TeamMeetingStudent", new[] { "StaffID" });
            DropIndex("dbo.TeamMeetingStudent", new[] { "TeamMeetingID" });
            DropIndex("dbo.TeamMeetingStudentNote", new[] { "StaffID" });
            DropIndex("dbo.TeamMeetingStudentNote", new[] { "StudentID" });
            DropIndex("dbo.TeamMeetingStudentNote", new[] { "TeamMeetingID" });
            DropIndex("dbo.TeamMeetingManager", new[] { "StaffID" });
            DropIndex("dbo.TeamMeetingManager", new[] { "TeamMeetingID" });
            DropIndex("dbo.TeamMeeting", new[] { "StaffID" });
            DropIndex("dbo.TeamMeetingAttendance", new[] { "SchoolID" });
            DropIndex("dbo.TeamMeetingAttendance", new[] { "StaffID" });
            DropIndex("dbo.TeamMeetingAttendance", new[] { "TeamMeetingID" });
            DropIndex("dbo.StaffSetting", new[] { "StaffId" });
            DropIndex("dbo.StaffAssessment", new[] { "AssessmentId" });
            DropIndex("dbo.StaffAssessmentFieldVisibility", new[] { "AssessmentFieldId" });
            DropIndex("dbo.StaffAssessmentFieldVisibility", new[] { "AssessmentId" });
            DropIndex("dbo.StaffAssessmentFieldVisibility", new[] { "StaffId" });
            DropIndex("dbo.SchoolAssessment", new[] { "AssessmentId" });
            DropIndex("dbo.District_Benchmark", new[] { "TestLevelPeriodID" });
            DropIndex("dbo.District_Benchmark", new[] { "GradeID" });
            DropIndex("dbo.District_Benchmark", new[] { "AssessmentID" });
            DropIndex("dbo.AssessmentFieldSubCategory", new[] { "AssessmentId" });
            DropIndex("dbo.StaffSchoolGrade", new[] { "GradeID" });
            DropIndex("dbo.StaffSchoolGrade", new[] { "StaffID" });
            DropIndex("dbo.StaffSchoolGrade", new[] { "SchoolID" });
            DropIndex("dbo.StaffObservationSummaryAssessment", new[] { "StaffId" });
            DropIndex("dbo.StaffObservationSummaryAssessment", new[] { "AssessmentId" });
            DropIndex("dbo.StaffSection", new[] { "ClassID" });
            DropIndex("dbo.StaffSection", new[] { "StaffID" });
            DropIndex("dbo.InterventionTool", new[] { "ToolTypeID" });
            DropIndex("dbo.InterventionToolIntervention", new[] { "InterventionToolID" });
            DropIndex("dbo.InterventionToolIntervention", new[] { "InterventionID" });
            DropIndex("dbo.StudentInterventionGroup", new[] { "InterventionGroupId" });
            DropIndex("dbo.StudentInterventionGroup", new[] { "StudentID" });
            DropIndex("dbo.StaffInterventionGroup", new[] { "InterventionGroupId" });
            DropIndex("dbo.StaffInterventionGroup", new[] { "StaffID" });
            DropIndex("dbo.StudentSection", new[] { "ClassID" });
            DropIndex("dbo.StudentSection", new[] { "StudentID" });
            DropIndex("dbo.StudentAttributeLookupValue", new[] { "AttributeID" });
            DropIndex("dbo.StudentAttributeData", new[] { "AttributeValueID" });
            DropIndex("dbo.StudentAttributeData", new[] { "AttributeID" });
            DropIndex("dbo.StudentAttributeData", new[] { "StudentID" });
            DropIndex("dbo.StudentSchool", new[] { "SchoolStartYear" });
            DropIndex("dbo.StudentSchool", new[] { "SchoolID" });
            DropIndex("dbo.StudentSchool", new[] { "StudentID" });
            DropIndex("dbo.StaffSchool", new[] { "StaffID" });
            DropIndex("dbo.StaffSchool", new[] { "SchoolID" });
            DropIndex("dbo.SchoolCalendar", new[] { "SchoolID" });
            DropIndex("dbo.InterventionGroup", new[] { "InterventionTypeID" });
            DropIndex("dbo.InterventionGroup", new[] { "SchoolID" });
            DropIndex("dbo.InterventionGroup", new[] { "SchoolStartYear" });
            DropIndex("dbo.InterventionGroup", new[] { "StaffID" });
            DropIndex("dbo.InterventionGrade", new[] { "GradeID" });
            DropIndex("dbo.InterventionGrade", new[] { "InterventionID" });
            DropIndex("dbo.InterventionType", new[] { "WorkshopID" });
            DropIndex("dbo.InterventionType", new[] { "UnitOfStudyID" });
            DropIndex("dbo.InterventionType", new[] { "FrameworkID" });
            DropIndex("dbo.InterventionType", new[] { "CategoryID" });
            DropIndex("dbo.InterventionType", new[] { "InterventionTierID" });
            DropIndex("dbo.InterventionType", new[] { "InterventionCardinalityID" });
            DropIndex("dbo.Section", new[] { "InterventionTypeID" });
            DropIndex("dbo.Section", new[] { "GradeID" });
            DropIndex("dbo.Section", new[] { "SchoolID" });
            DropIndex("dbo.Section", new[] { "SchoolStartYear" });
            DropIndex("dbo.Section", new[] { "StaffID" });
            DropIndex("dbo.InterventionAttendance", new[] { "RecorderID" });
            DropIndex("dbo.InterventionAttendance", new[] { "AttendanceReasonID" });
            DropIndex("dbo.AttendeeGroupStaff", new[] { "AttendeeGroupId" });
            DropIndex("dbo.AttendeeGroupStaff", new[] { "StaffId" });
            DropIndex("dbo.AttendeeGroup", new[] { "StaffId" });
            DropIndex("dbo.StaffObservationSummaryAssessmentField", new[] { "StaffId" });
            DropIndex("dbo.StaffObservationSummaryAssessmentField", new[] { "AssessmentFieldId" });
            DropIndex("dbo.StaffObservationSummaryAssessmentField", new[] { "AssessmentId" });
            DropIndex("dbo.AssessmentFieldGroup", new[] { "AssessmentId" });
            DropIndex("dbo.AssessmentField", new[] { "AssessmentId" });
            DropIndex("dbo.AssessmentField", new[] { "GroupId" });
            DropIndex("dbo.AssessmentField", new[] { "SubcategoryId" });
            DropIndex("dbo.AssessmentField", new[] { "CategoryId" });
            DropIndex("dbo.AssessmentFieldCategory", new[] { "AssessmentId" });
            DropIndex("dbo.Assessment_Benchmarks", new[] { "TestLevelPeriodID" });
            DropIndex("dbo.Assessment_Benchmarks", new[] { "GradeID" });
            DropIndex("dbo.Assessment_Benchmarks", new[] { "AssessmentID" });
            DropTable("dbo.TestDueDate");
            DropTable("dbo.TeamMeetingStudent");
            DropTable("dbo.TeamMeetingStudentNote");
            DropTable("dbo.TeamMeetingManager");
            DropTable("dbo.TeamMeeting");
            DropTable("dbo.TeamMeetingAttendance");
            DropTable("dbo.StaffSetting");
            DropTable("dbo.StaffAssessment");
            DropTable("dbo.StaffAssessmentFieldVisibility");
            DropTable("dbo.SchoolAssessment");
            DropTable("dbo.AssessmentLookupField");
            DropTable("dbo.FPComparison");
            DropTable("dbo.DistrictCalendar");
            DropTable("dbo.District_Benchmark");
            DropTable("dbo.TestLevelPeriod");
            DropTable("dbo.AssessmentFieldSubCategory");
            DropTable("dbo.StaffSchoolGrade");
            DropTable("dbo.StaffObservationSummaryAssessment");
            DropTable("dbo.StaffSection");
            DropTable("dbo.InterventionWorkshop");
            DropTable("dbo.InterventionUnitOfStudy");
            DropTable("dbo.InterventionToolType");
            DropTable("dbo.InterventionTool");
            DropTable("dbo.InterventionToolIntervention");
            DropTable("dbo.InterventionTier");
            DropTable("dbo.StudentInterventionGroup");
            DropTable("dbo.StaffInterventionGroup");
            DropTable("dbo.StudentSection");
            DropTable("dbo.StudentAttributeLookupValue");
            DropTable("dbo.StudentAttributeType");
            DropTable("dbo.StudentAttributeData");
            DropTable("dbo.Student");
            DropTable("dbo.SchoolYear");
            DropTable("dbo.StudentSchool");
            DropTable("dbo.StaffSchool");
            DropTable("dbo.SchoolCalendar");
            DropTable("dbo.School");
            DropTable("dbo.InterventionGroup");
            DropTable("dbo.InterventionGrade");
            DropTable("dbo.InterventionFramework");
            DropTable("dbo.InterventionCategory");
            DropTable("dbo.InterventionCardinality");
            DropTable("dbo.InterventionType");
            DropTable("dbo.Grade");
            DropTable("dbo.Section");
            DropTable("dbo.AttendanceReason");
            DropTable("dbo.InterventionAttendance");
            DropTable("dbo.AttendeeGroupStaff");
            DropTable("dbo.AttendeeGroup");
            DropTable("dbo.Staff");
            DropTable("dbo.StaffObservationSummaryAssessmentField");
            DropTable("dbo.AssessmentFieldGroup");
            DropTable("dbo.AssessmentField");
            DropTable("dbo.AssessmentFieldCategory");
            DropTable("dbo.Assessment");
            DropTable("dbo.Assessment_Benchmarks");
        }
    }
}
