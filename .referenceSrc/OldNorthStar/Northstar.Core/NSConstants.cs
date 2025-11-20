using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.Core
{
    public static class NSConstants
    {        
        public static class ClaimTypes
        {
            public const string DistrictId = "district_id";
            public const string AuthenticatedAccount = "AuthenticatedAccount";
            public const string DistrictAdmin = "district_admin";
        }

        public static class Azure
        {
            public const string JobQueue = "northstarjobs";// "northstarjobsdev";
            public const string AssessmentImportContainer = "assessmentimports";
            public const string AssessmentDataExportContainer = "assessmentdataexports";
            public const string RolloverContainer = "rollover";
            public enum JobType { StateTestImport, BenchmarkTestImport, InterventionTestImport, PrintBatch, DataExport, AttendanceExport, FullRollover, StudentRollover, TeacherRollover, RolloverValidation, InterventionDataExport, StudentRolloverValidation, TeacherRolloverValidation, StudentAttributeExport, TeacherAttributeExport, AllFieldsBenchmarkExport};
        }

        public static class BatchProcessing
        {
            public const string StateTestStudentLastName = "Student Last Name";
            public const string StateTestStudentFirstName = "Student First Name";
        }

        public static class StaffSettingTypes
        {
            public const string TddMultiRange = "tddmultirange";
            public const string BenchmarkDate = "benchmarkdate";
            public const string ClassroomAssessmentField = "classroomassessmentfield";
            public const string InterventionGroupAssessmentField = "interventiongroupassessmentfield";
            public const string SchoolYear = "schoolyear";
            public const string School = "school";
            public const string Grade = "grade";
            public const string Teacher = "teacher";
            public const string Section = "section";
            public const string SectionStudent = "sectionstudent";
            public const string Interventionist = "interventionist";
            public const string InterventionGroup = "interventiongroup";
            public const string InterventionStudent = "interventionstudent";
            public const string Stint = "stint";
            public const string TeamMeeting = "teammeeting";
            public const string TeamMeetingStaff = "teammeetingstaff";
            public const string LineGraphField = "linegraphfield";
            public const string HFWMultiRange = "hfwmultirange";
            public const string HFWSingleRange = "hfwsinglerange";
            public const string HFWSortOrder = "hfwsortorder";
            public const string HRSForm = "hrsform";
            public const string HRSForm2 = "hrsform2";
            public const string HRSForm3 = "hrsform3";
            public const string OSSchoolColumn = "osschoolcolumn";
            public const string OSGradeColumn = "osgradecolumn";
            public const string OSTeacherColumn = "osteachercolumn";
            public const string TurnOffCommentBubbles = "turnOffCommentBubbles";
        }
    }
}
