using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.AssessmentDataImport
{
    public class AssessmentImportViewModel
    {

            public string Name { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public long Size { get; set; }
            public string Url { get; set; }

    }
}
