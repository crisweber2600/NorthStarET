using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar.Core.FileUpload
{
    public class AssessmentImportViewModel
    {

            public string Name { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public long Size { get; set; }
            public string Url { get; set; }

    }

    public class ImportTestDataViewModel
    {
        public IEnumerable<AssessmentImportViewModel> Files { get; set; }
        public NameValueCollection FormData { get; set; }
    }
}
