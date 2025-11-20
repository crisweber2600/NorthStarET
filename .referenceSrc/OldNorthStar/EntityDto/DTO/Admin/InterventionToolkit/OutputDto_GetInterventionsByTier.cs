using System.Collections.Generic;
using NorthStar4.CrossPlatform.Entity;
using NorthStar4.PCL.Entity;
using EntityDto.DTO.Admin.Simple;
using EntityDto.DTO.Admin.InterventionToolkit;
using EntityDto.LoginDB.Entity;

namespace NorthStar4.CrossPlatform.DTO.Admin.InterventionToolkit
{
    public class OutputDto_GetInterventionsByTier : OutputDto_Base
    {
        public List<InterventionDto> Interventions { get; set; }
       // public List<InterventionCardinality> GroupSizes { get; set; }
        public List<InterventionCategoryDto> Categories { get; set; }
        public List<GradeDto> Grades { get; set; }  
        public List<InterventionWorkshopDto> Workshops { get; set; }
        //public List<InterventionUnitOfStudy> UnitOfStudies { get; set; }
        //public List<InterventionFramework> Frameworks { get; set; }   
        public InterventionTierDto Tier { get; set; }
    }
}