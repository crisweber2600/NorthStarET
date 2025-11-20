
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Personal
{
    public class StaffAssessmentFieldVisibilityDto : BaseEntityNoTrack
    {
        public int StaffId { get; set; }
        public int AssessmentId { get; set; }
        public int AssessmentFieldId { get; set; }
        public bool? HideFieldFromObservationSummary { get; set; }
        public bool? HideFieldFromEditResults { get; set; }
        public bool? HideFieldFromLineGraphs { get; set; }

    }
}
