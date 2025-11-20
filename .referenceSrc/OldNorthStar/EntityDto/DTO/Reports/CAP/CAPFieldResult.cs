using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar4.PCL.DTO
{
	public class CAPFieldResult
    {
		public string Comment { get; set; }
		public int GroupId { get; set; }
		public bool? Checked { get; set; }
		public string DbColumn { get; set; }
        public int FieldId { get; set; }
        public bool? FF1 { get; set; }
        public bool? FF2 { get; set; }
        public bool? FF3 { get; set; }
        public bool? FF4 { get; set; }
        public bool? FF5 { get; set; }
    }
}
