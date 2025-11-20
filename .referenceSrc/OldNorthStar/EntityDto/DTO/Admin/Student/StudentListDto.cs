using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Student
{
    public class StudentListDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string DOBText { get; set; }
        public bool RegisteredForCurrentYear { get; set; }

        public string StudentIdentifier { get; set; }
    }
}
