using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NorthStar4.CrossPlatform.Entity;
using EntityDto.Entity;

namespace NorthStar4.PCL.Entity
{
	public class Intervention : BaseEntity
	{
        public Intervention()
        {
            this.InterventionGrades = new HashSet<InterventionGrade>();
            this.InterventionToolInterventions = new HashSet<InterventionToolIntervention>();
           // this.InterventionVideoInterventions = new HashSet<InterventionVideoIntervention>();
          //  this.DistrictInterventionTypes = new HashSet<DistrictInterventionType>();
            this.InterventionGroups = new HashSet<InterventionGroup>();
        }

        public string InterventionType { get; set; }
		public bool bDisplay { get; set; }
		public string Description { get; set; }
		public int? DefaultTextLevelType { get; set; }
		public int? InterventionCardinalityID { get; set; }
		public string ExitCriteria { get; set; }
		public string EntranceCriteria { get; set; }
		public string LearnerNeed { get; set; }
		public string DetailedDescription { get; set; }
		public string TimeOfYear { get; set; }
		public int? InterventionTierID { get; set; }
		public int? CategoryID { get; set; }
		public string BriefDescription { get; set; }
		public int? FrameworkID { get; set; }
		public int? UnitOfStudyID { get; set; }
		public int? WorkshopID { get; set; }

        public virtual InterventionCardinality InterventionCardinality { get; set; }
        public virtual InterventionCategory InterventionCategory { get; set; }
        public virtual ICollection<InterventionGrade> InterventionGrades { get; set; }
        public virtual InterventionTier InterventionTier { get; set; }
        public virtual ICollection<InterventionToolIntervention> InterventionToolInterventions { get; set; }
       // public virtual ICollection<InterventionVideoIntervention> InterventionVideoInterventions { get; set; }
        //public virtual ICollection<DistrictInterventionType> DistrictInterventionTypes { get; set; }
        public virtual ICollection<InterventionGroup> InterventionGroups { get; set; }
        public virtual InterventionFramework InterventionFramework { get; set; }
        public virtual InterventionUnitOfStudy InterventionUnitOfStudy { get; set; }
        public virtual InterventionWorkshop InterventionWorkshop { get; set; }
    }
}
