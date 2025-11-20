namespace NorthStar.EF6.DistrictContextMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentnotes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AssessmentFieldCategory", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldGroup", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentField", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldSubCategory", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffObservationSummaryAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffObservationSummaryAssessment", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffSchoolGrade", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.AttendeeGroupStaff", "AttendeeGroupId", "dbo.AttendeeGroup");
            DropForeignKey("dbo.AttendeeGroupStaff", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.InterventionAttendance", "AttendanceReasonID", "dbo.AttendanceReason");
            DropForeignKey("dbo.Section", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.InterventionGrade", "InterventionID", "dbo.InterventionType");
            DropForeignKey("dbo.InterventionGrade", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.InterventionGroup", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.InterventionGroup", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StudentSchool", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.StudentAttributeData", "AttributeID", "dbo.StudentAttributeType");
            DropForeignKey("dbo.StudentAttributeLookupValue", "AttributeID", "dbo.StudentAttributeType");
            DropForeignKey("dbo.StaffInterventionGroup", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StudentInterventionGroup", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StaffSchoolGrade", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.StaffSchoolGrade", "SchoolID", "dbo.School");
            DropForeignKey("dbo.District_Benchmark", "AssessmentID", "dbo.Assessment");
            DropForeignKey("dbo.District_Benchmark", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.District_Benchmark", "TestLevelPeriodID", "dbo.TestLevelPeriod");
            DropForeignKey("dbo.SchoolAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentFieldId", "dbo.AssessmentField");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffSetting", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingAttendance", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeeting", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingManager", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingStudentNote", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingStudentNote", "StudentID", "dbo.Student");
            DropForeignKey("dbo.TeamMeetingStudent", "SectionID", "dbo.Section");
            DropForeignKey("dbo.TeamMeetingStudent", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingStudent", "StudentID", "dbo.Student");
            CreateTable(
                "dbo.StudentNote",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentID = c.Int(nullable: false),
                        NoteDate = c.DateTime(nullable: false),
                        Note = c.String(),
                        StaffID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Staff", t => t.StaffID, cascadeDelete: true)
                .ForeignKey("dbo.Student", t => t.StudentID, cascadeDelete: true)
                .Index(t => t.StudentID)
                .Index(t => t.StaffID);
            
            AddForeignKey("dbo.AssessmentFieldCategory", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AssessmentFieldGroup", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AssessmentField", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AssessmentFieldSubCategory", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffObservationSummaryAssessment", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffObservationSummaryAssessmentField", "StaffId", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffObservationSummaryAssessment", "StaffId", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffSchoolGrade", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AttendeeGroupStaff", "AttendeeGroupId", "dbo.AttendeeGroup", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AttendeeGroupStaff", "StaffId", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.InterventionAttendance", "AttendanceReasonID", "dbo.AttendanceReason", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Section", "SchoolStartYear", "dbo.SchoolYear", "SchoolStartYear", cascadeDelete: true);
            AddForeignKey("dbo.InterventionGrade", "InterventionID", "dbo.InterventionType", "Id", cascadeDelete: true);
            AddForeignKey("dbo.InterventionGrade", "GradeID", "dbo.Grade", "Id", cascadeDelete: true);
            AddForeignKey("dbo.InterventionGroup", "SchoolStartYear", "dbo.SchoolYear", "SchoolStartYear", cascadeDelete: true);
            AddForeignKey("dbo.InterventionGroup", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StudentSchool", "SchoolStartYear", "dbo.SchoolYear", "SchoolStartYear", cascadeDelete: true);
            AddForeignKey("dbo.StudentAttributeData", "AttributeID", "dbo.StudentAttributeType", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StudentAttributeLookupValue", "AttributeID", "dbo.StudentAttributeType", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffInterventionGroup", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StudentInterventionGroup", "StudentID", "dbo.Student", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffSchoolGrade", "GradeID", "dbo.Grade", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffSchoolGrade", "SchoolID", "dbo.School", "Id", cascadeDelete: true);
            AddForeignKey("dbo.District_Benchmark", "AssessmentID", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.District_Benchmark", "GradeID", "dbo.Grade", "Id", cascadeDelete: true);
            AddForeignKey("dbo.District_Benchmark", "TestLevelPeriodID", "dbo.TestLevelPeriod", "Id", cascadeDelete: true);
            AddForeignKey("dbo.SchoolAssessment", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentFieldId", "dbo.AssessmentField", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffAssessmentFieldVisibility", "StaffId", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffAssessment", "AssessmentId", "dbo.Assessment", "Id", cascadeDelete: true);
            AddForeignKey("dbo.StaffSetting", "StaffId", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingAttendance", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeeting", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingManager", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingStudentNote", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingStudentNote", "StudentID", "dbo.Student", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingStudent", "SectionID", "dbo.Section", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingStudent", "StaffID", "dbo.Staff", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TeamMeetingStudent", "StudentID", "dbo.Student", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeamMeetingStudent", "StudentID", "dbo.Student");
            DropForeignKey("dbo.TeamMeetingStudent", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingStudent", "SectionID", "dbo.Section");
            DropForeignKey("dbo.TeamMeetingStudentNote", "StudentID", "dbo.Student");
            DropForeignKey("dbo.TeamMeetingStudentNote", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingManager", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeeting", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.TeamMeetingAttendance", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StaffSetting", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentFieldId", "dbo.AssessmentField");
            DropForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.SchoolAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.District_Benchmark", "TestLevelPeriodID", "dbo.TestLevelPeriod");
            DropForeignKey("dbo.District_Benchmark", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.District_Benchmark", "AssessmentID", "dbo.Assessment");
            DropForeignKey("dbo.StaffSchoolGrade", "SchoolID", "dbo.School");
            DropForeignKey("dbo.StaffSchoolGrade", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.StudentInterventionGroup", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StaffInterventionGroup", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StudentAttributeLookupValue", "AttributeID", "dbo.StudentAttributeType");
            DropForeignKey("dbo.StudentAttributeData", "AttributeID", "dbo.StudentAttributeType");
            DropForeignKey("dbo.StudentSchool", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.InterventionGroup", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.InterventionGroup", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.InterventionGrade", "GradeID", "dbo.Grade");
            DropForeignKey("dbo.InterventionGrade", "InterventionID", "dbo.InterventionType");
            DropForeignKey("dbo.Section", "SchoolStartYear", "dbo.SchoolYear");
            DropForeignKey("dbo.InterventionAttendance", "AttendanceReasonID", "dbo.AttendanceReason");
            DropForeignKey("dbo.AttendeeGroupStaff", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.AttendeeGroupStaff", "AttendeeGroupId", "dbo.AttendeeGroup");
            DropForeignKey("dbo.StaffSchoolGrade", "StaffID", "dbo.Staff");
            DropForeignKey("dbo.StaffObservationSummaryAssessment", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffObservationSummaryAssessmentField", "StaffId", "dbo.Staff");
            DropForeignKey("dbo.StaffObservationSummaryAssessment", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldSubCategory", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentField", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldGroup", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.AssessmentFieldCategory", "AssessmentId", "dbo.Assessment");
            DropForeignKey("dbo.StudentNote", "StudentID", "dbo.Student");
            DropForeignKey("dbo.StudentNote", "StaffID", "dbo.Staff");
            DropIndex("dbo.StudentNote", new[] { "StaffID" });
            DropIndex("dbo.StudentNote", new[] { "StudentID" });
            DropTable("dbo.StudentNote");
            AddForeignKey("dbo.TeamMeetingStudent", "StudentID", "dbo.Student", "Id");
            AddForeignKey("dbo.TeamMeetingStudent", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.TeamMeetingStudent", "SectionID", "dbo.Section", "Id");
            AddForeignKey("dbo.TeamMeetingStudentNote", "StudentID", "dbo.Student", "Id");
            AddForeignKey("dbo.TeamMeetingStudentNote", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.TeamMeetingManager", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.TeamMeeting", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.TeamMeetingAttendance", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.StaffSetting", "StaffId", "dbo.Staff", "Id");
            AddForeignKey("dbo.StaffAssessment", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.StaffAssessmentFieldVisibility", "StaffId", "dbo.Staff", "Id");
            AddForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentFieldId", "dbo.AssessmentField", "Id");
            AddForeignKey("dbo.StaffAssessmentFieldVisibility", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.SchoolAssessment", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.District_Benchmark", "TestLevelPeriodID", "dbo.TestLevelPeriod", "Id");
            AddForeignKey("dbo.District_Benchmark", "GradeID", "dbo.Grade", "Id");
            AddForeignKey("dbo.District_Benchmark", "AssessmentID", "dbo.Assessment", "Id");
            AddForeignKey("dbo.StaffSchoolGrade", "SchoolID", "dbo.School", "Id");
            AddForeignKey("dbo.StaffSchoolGrade", "GradeID", "dbo.Grade", "Id");
            AddForeignKey("dbo.StudentInterventionGroup", "StudentID", "dbo.Student", "Id");
            AddForeignKey("dbo.StaffInterventionGroup", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.StudentAttributeLookupValue", "AttributeID", "dbo.StudentAttributeType", "Id");
            AddForeignKey("dbo.StudentAttributeData", "AttributeID", "dbo.StudentAttributeType", "Id");
            AddForeignKey("dbo.StudentSchool", "SchoolStartYear", "dbo.SchoolYear", "SchoolStartYear");
            AddForeignKey("dbo.InterventionGroup", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.InterventionGroup", "SchoolStartYear", "dbo.SchoolYear", "SchoolStartYear");
            AddForeignKey("dbo.InterventionGrade", "GradeID", "dbo.Grade", "Id");
            AddForeignKey("dbo.InterventionGrade", "InterventionID", "dbo.InterventionType", "Id");
            AddForeignKey("dbo.Section", "SchoolStartYear", "dbo.SchoolYear", "SchoolStartYear");
            AddForeignKey("dbo.InterventionAttendance", "AttendanceReasonID", "dbo.AttendanceReason", "Id");
            AddForeignKey("dbo.AttendeeGroupStaff", "StaffId", "dbo.Staff", "Id");
            AddForeignKey("dbo.AttendeeGroupStaff", "AttendeeGroupId", "dbo.AttendeeGroup", "Id");
            AddForeignKey("dbo.StaffSchoolGrade", "StaffID", "dbo.Staff", "Id");
            AddForeignKey("dbo.StaffObservationSummaryAssessment", "StaffId", "dbo.Staff", "Id");
            AddForeignKey("dbo.StaffObservationSummaryAssessmentField", "StaffId", "dbo.Staff", "Id");
            AddForeignKey("dbo.StaffObservationSummaryAssessment", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.AssessmentFieldSubCategory", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.AssessmentField", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.AssessmentFieldGroup", "AssessmentId", "dbo.Assessment", "Id");
            AddForeignKey("dbo.AssessmentFieldCategory", "AssessmentId", "dbo.Assessment", "Id");
        }
    }
}
