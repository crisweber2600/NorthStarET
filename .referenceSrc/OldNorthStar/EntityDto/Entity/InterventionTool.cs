using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.CrossPlatform.Entity
{
    public class InterventionTool : BaseEntityNoTrack
    {
        public InterventionTool()
        {
            this.InterventionToolInterventions = new HashSet<InterventionToolIntervention>();
        }
        public string ToolName { get; set; }
        public string ToolFileName { get; set; }
        public string Description { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public string FileSystemFileName { get; set; }
        public Nullable<System.DateTime> LastModified { get; set; }
        public Nullable<int> FileSize { get; set; }
        public string FileExtension { get; set; }
        public Nullable<int> ToolTypeID { get; set; }

        public virtual ICollection<InterventionToolIntervention> InterventionToolInterventions { get; set; }
        public virtual InterventionToolType InterventionToolType { get; set; }
    }
}
