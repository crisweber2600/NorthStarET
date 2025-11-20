using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.CrossPlatform.Entity;
using EntityDto.Entity;
using NorthStar4.PCL.Entity;

namespace EntityDto.LoginDB.Entity
{
	public class NSIntervention : BaseEntity
	{
        public NSIntervention()
        {
            this.InterventionGrades = new HashSet<NSInterventionGrade>();
            this.InterventionToolInterventions = new HashSet<NSInterventionToolIntervention>();
            this.InterventionVideoInterventions = new HashSet<NSInterventionVideoNSIntervention>();
            this.Description = "New";
            this.InterventionType = "Empty";
        }

        public string InterventionType { get; set; }
		public bool bDisplay { get; set; }
		public string Description { get; set; }
		public int? DefaultTextLevelType { get; set; }
		public int? InterventionCardinalityId { get; set; }
		public string ExitCriteria { get; set; }
		public string EntranceCriteria { get; set; }
		public string LearnerNeed { get; set; }
		public string DetailedDescription { get; set; }
		public string TimeOfYear { get; set; }
		public int? InterventionTierId { get; set; }
		public int? InterventionCategoryId { get; set; }
		public string BriefDescription { get; set; }
		public int? InterventionFrameworkId { get; set; }
		public int? InterventionUnitOfStudyId { get; set; }
		public int? InterventionWorkshopId { get; set; }

        public virtual NSInterventionCardinality InterventionCardinality { get; set; }
        public virtual NSInterventionCategory InterventionCategory { get; set; }
        public virtual ICollection<NSInterventionGrade> InterventionGrades { get; set; }
        public virtual NSInterventionTier InterventionTier { get; set; }
        public virtual ICollection<NSInterventionToolIntervention> InterventionToolInterventions { get; set; }
        public virtual ICollection<NSInterventionVideoNSIntervention> InterventionVideoInterventions { get; set; }
        public virtual NSInterventionFramework InterventionFramework { get; set; }
        public virtual NSInterventionUnitOfStudy InterventionUnitOfStudy { get; set; }
        public virtual NSInterventionWorkshop InterventionWorkshop { get; set; }
    }
}
