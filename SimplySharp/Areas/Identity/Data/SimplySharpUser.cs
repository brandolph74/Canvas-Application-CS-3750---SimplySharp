  using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SimplySharp.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the SimplySharpUser class
    public class SimplySharpUser : IdentityUser
    {
        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string FirstName { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string LastName { get; set; }

        [PersonalData]
        [Column(TypeName = "date")]
        public DateTime Birthdate { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string AddressLine1 { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string AddressLine2 { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string City { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string State { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(20)")]
        public string Zip { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(100)")]
        public string Phone { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(1000)")]
        public string Biography { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(500)")]
        public string Link1 { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(500)")]
        public string Link2 { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(500)")]
        public string Link3 { get; set; }


        [PersonalData]
        [Column(TypeName = "nvarchar(1)")]
        public string UserType { get; set; }

        [PersonalData]
        [Column(TypeName ="varbinary(max)")]
        public byte[] ProfilePicture { get; set; }


    }
}
