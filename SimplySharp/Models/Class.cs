using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    public class Class
    {
        public int Id { get; set; }

        [DisplayName("Class")]
        public string ClassId { get; set; }
        [DisplayName("Name")]
        public string ClassName { get; set; }
        public int Credits { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        [DisplayName("Meeting Days")]
        public string MeetingDays { get; set; }

        [DisplayName("Start Time")]
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }
        [DisplayName("End Time")]
        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }      
        public string Instructor { get; set; }
        public string Department { get; set; }
    }

    public enum Department
    {
        ACTG,
        BTNY,
        CHEM,
        CS,
        EDU
    }
}
