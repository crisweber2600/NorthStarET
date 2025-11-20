using EntityDto.DTO.Admin.Simple;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.ImportExport
{
    public class AssessmentTemplate : OutputDto_Base
    {
        public AssessmentTemplate()
        {
            Fields = new List<AssessmentFieldTemplate>();
        }
        public List<AssessmentFieldTemplate> Fields { get; set; }
    }

    public class AssessmentFieldTemplate
    {
        public string FieldName { get; set; }
        public bool Required { get; set; }
        public string ValidValues { get; set; }
        public string ValidRange { get; set; }
        public string FieldType { get; set; }
        public int SortOrder { get; set; }
        public string UniqueColumnName { get; set; }
    }
}
