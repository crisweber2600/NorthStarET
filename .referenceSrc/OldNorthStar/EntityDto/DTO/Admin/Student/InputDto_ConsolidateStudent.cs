using NorthStar4.PCL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class InputDto_ConsolidateStudent
    {
        public StudentQuickSearchResult PrimaryStudent { get; set; }
        public StudentQuickSearchResult SecondaryStudent { get; set; }

    }

    public class InputDto_ConsolidateStudentServices
    {
        public StudentServiceDto PrimaryService { get; set; }
        public List<StudentServiceDto> SecondaryServices { get; set; }

    }
}
