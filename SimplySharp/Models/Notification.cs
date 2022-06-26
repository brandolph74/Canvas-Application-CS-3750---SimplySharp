using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string ClassID { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string AssignmentTitle { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string NotiType { get; set; }

        public DateTime NotiDate { get; set; }

        [Column(TypeName = "nvarchar(450)")]
        public string UserID { get; set; }


    }
}
