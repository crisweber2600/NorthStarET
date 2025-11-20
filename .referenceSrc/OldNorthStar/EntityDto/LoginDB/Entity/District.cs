using NorthStar4.CrossPlatform.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.LoginDB.Entity
{
    public class District : BaseEntityNoTrack
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
    public class AutomatedRolloverDetail : BaseEntityNoTrack
    {
        public string FtpSite { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public string RelativeUri { get; set; }
        public string RolloverEmail { get; set; }
        public string AdminEmail { get; set; }
        public bool ForceLoad { get; set; }
        public int DistrictId { get; set; }
        public bool IsActive { get; set; }
    }
}
