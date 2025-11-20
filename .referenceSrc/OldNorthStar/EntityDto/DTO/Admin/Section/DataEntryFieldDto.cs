using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Admin.Section
{
    public class DataEntryFieldDto
    {
        public DataEntryFieldDto()
        {

        }
        public string StringValue { get; set; }
        public int? IntValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public DateTime? DateValue { get; set; }
        public bool? BoolValue { get; set; }
        public string DatabaseColumn { get; set; }
        public bool IsModified { get; set; }
        public string DisplayLabel { get; set; }
        public string AltDisplayLabel { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public int? GroupId { get; set; }
        public int Page { get; set; }
        public string CalculationFunction { get; set; }
        public string CalculationFields { get; set; }
    }
}
