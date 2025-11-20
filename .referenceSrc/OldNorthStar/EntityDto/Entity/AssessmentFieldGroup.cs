using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.Entity
{
	public class AssessmentFieldGroup : BaseEntity, ICloneable
	{
        public object Clone()
        {
            AssessmentFieldGroup p = new AssessmentFieldGroup();
            p.SortOrder = this.SortOrder;
            p.AltOrder = this.AltOrder;
            p.DisplayName = this.DisplayName;
            p.AltDisplayLabel = this.AltDisplayLabel;
            p.Description = this.Description;
            p.IsKdg = this.IsKdg;
            //p.PreviousId = this.PreviousId;
            //p.AssessmentFieldGroupContainerId = this.AssessmentFieldGroupContainerId;
            p.Flag1 = this.Flag1;
            p.Flag2 = this.Flag2;
            p.Flag3 = this.Flag3;
            p.Flag4 = this.Flag4;
            p.Flag5 = this.Flag5;

            return p;
        }
        //public virtual Assessment ParentAssessment { get; set; }
        public int AssessmentId { get; set; }
		public int SortOrder { get; set; }
        public int? AltOrder { get; set; }
		public string DisplayName { get; set; }
        public string AltDisplayLabel { get; set; }
        public string Description { get; set; }
        public int? PreviousId { get; set; }
        public bool IsKdg { get; set; }
        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }
        public int? AssessmentFieldGroupContainerId { get; set; }
        public virtual AssessmentFieldGroupContainer Container { get; set; }
        public virtual ICollection<AssessmentField> AssessmentFields { get; set; }
    }

    public class AssessmentFieldGroupContainer: BaseEntity, ICloneable
    {

        public object Clone()
        {
            AssessmentFieldGroupContainer p = new AssessmentFieldGroupContainer();
            p.SortOrder = this.SortOrder;
            p.AltOrder = this.AltOrder;
            p.DisplayName = this.DisplayName;
            p.AltDisplayLabel = this.AltDisplayLabel;
            p.Description = this.Description;
            p.Flag1 = this.Flag1;
            p.Flag2 = this.Flag2;
            p.Flag3 = this.Flag3;
            p.Flag4 = this.Flag4;
            p.Flag5 = this.Flag5;

            return p;
        }

        public int AssessmentId { get; set; }
        public int SortOrder { get; set; }
        public int? AltOrder { get; set; }
        public string DisplayName { get; set; }
        public string AltDisplayLabel { get; set; }
        public string Description { get; set; }
        public int? PreviousId { get; set; }
        public bool? Flag1 { get; set; }
        public bool? Flag2 { get; set; }
        public bool? Flag3 { get; set; }
        public bool? Flag4 { get; set; }
        public bool? Flag5 { get; set; }

        public virtual ICollection<AssessmentFieldGroup> AssessmentFieldGroups { get; set; }
    }
}
