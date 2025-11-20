using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Personal
{
    public class InputDto_UpdateFieldForUser
    {
        public int FieldId { get; set; }
        public int AssessmentId { get; set; }
        public string HideFieldFrom { get; set; }
        public bool HiddenStatus { get; set; }
    }
}
