using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Models;

namespace SimplySharp.Data
{
    public class SimplySharpDBContext : IdentityDbContext<SimplySharpUser>
    {
        public SimplySharpDBContext(DbContextOptions<SimplySharpDBContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Db set of classes in database.
        /// </summary>
        public DbSet<Class> Class { get; set; }
        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<ClassRegistration> ClassRegistration { get; set; }

        public DbSet<Submission> Submission { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
