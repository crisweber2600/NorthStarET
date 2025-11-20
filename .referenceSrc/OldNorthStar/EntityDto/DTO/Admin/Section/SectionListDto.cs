using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class SectionListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PrimaryTeacher { get; set; }
        public List<string> CoTeachers { get; set; }
        public int NumStudents { get; set; }
        public string Description { get; set; }
        public string Grade { get; set; }
    }
}
