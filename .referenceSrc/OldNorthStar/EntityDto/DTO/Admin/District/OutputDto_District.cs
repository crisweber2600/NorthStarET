using EntityDto.DTO.Admin.Simple;
using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.District
{
    public class DistrictDto : BaseEntityNoTrack
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public bool? Enabled { get; set; }
        public int? AccessLevel { get; set; }
        public string AzureContainerName { get; set; }
        public string ProfilePicturePrefix { get; set; }
        public string ProfilePictureExtension { get; set; }
        public string Extension1 { get; set; }
        public string Extension2 { get; set; }
        public string Extension3 { get; set; }
        public string Extension4 { get; set; }
        public string Extension5 { get; set; }
        public string Extension6 { get; set; }
        public string Extension7 { get; set; }
        public string Extension8 { get; set; }
        public string Extension9 { get; set; }
        public string Extension10 { get; set; }
    }

    public class OutputDto_District : OutputDto_Base
    {
        public List<DistrictDto> Districts { get; set; }
    }
}
