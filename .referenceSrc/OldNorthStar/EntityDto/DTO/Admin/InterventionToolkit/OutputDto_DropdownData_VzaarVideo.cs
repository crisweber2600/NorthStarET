using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.InterventionToolkit
{
    public class OutputDto_DropdownData_VzaarVideo
    {
        public long id { get; set; }
        public string text { get; set; }
        public DateTime createdAt { get; set; }
        public string description { get; set; }
        public decimal duration { get; set; }
        public int height { get; set; }
        public long playCount { get; set; }
        public string status { get; set; }
        public int statusId { get; set; }
        public string thumbnail { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        // public VideoAuthor user;
        public string version { get; set; }
        public int width { get; set; }

    }
}
