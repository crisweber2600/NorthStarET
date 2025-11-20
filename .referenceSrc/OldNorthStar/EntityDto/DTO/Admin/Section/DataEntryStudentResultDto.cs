using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class DataEntryStudentResultDto
    {
        public DataEntryStudentResultDto()
        {
            FieldResults = new List<DataEntryFieldDto>();
        }

        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string FPText { get; set; }
        public int? FPValueID { get; set; }
        public int? ResultId { get; set; }
        public List<DataEntryFieldDto> FieldResults { get; set; }
        public int? TestDueDateId { get; set; }
        public DateTime? TestDate { get; set; }
        public int? StaffId { get; set; }
        public int? ClassId { get; set; }
    }
}
