using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class NSInterventionTool : BaseEntityNoTrack
    {
        public NSInterventionTool()
        {
            this.InterventionToolInterventions = new HashSet<NSInterventionToolIntervention>();
        }
        public string ToolName { get; set; }
        public string ToolFileName { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public string FileSystemFileName { get; set; }
        public DateTime? LastModified { get; set; }
        public int? FileSize { get; set; }
        public string FileExtension { get; set; }
        public int? InterventionToolTypeId { get; set; }

        public virtual ICollection<NSInterventionToolIntervention> InterventionToolInterventions { get; set; }
        public virtual NSInterventionToolType InterventionToolType { get; set; }
    }
}
