using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Assessment;
using NorthStar4.CrossPlatform.DTO.Reports.ObservationSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.ImportExport
{
    public class InputDto_ExportRequest
    {
        public int AssessmentId { get; set; }
        public int SectionId { get; set; }
        public int BenchmarkDateId { get; set; }
    }

    public class InputDto_InterventionExportRequest
    {
        public int AssessmentId { get; set; }
        public int InterventionGroupId { get; set; }
        public int StudentId { get; set; }
    }

    public class InputDto_DataExportRequest
    {
        public InputDto_GetFilteredObservationSummaryOptions ReportOptions { get; set; }
        public List<AssessmentDto> Assessments { get; set; }
    }

    public class InputDto_BatchPrintRequest
    {
        public InputDto_GetFilteredPrintBatchOptions ReportOptions { get; set; }
        public List<AssessmentDto> Assessments { get; set; }
    }

    public class InputDto_AttendanceExportRequest
    {
        public int SchoolStartYear { get; set; }
    }

    public class OutputDto_SimpleString : OutputDto_Base
    {
        public string Result { get; set; }
    }
}
