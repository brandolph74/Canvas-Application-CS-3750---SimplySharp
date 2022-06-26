using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Models;


namespace SimplySharp.Data
{
    public class ClassContext : DbContext
    {
        public ClassContext (DbContextOptions<ClassContext> options)
            : base(options)
        {

        }

        public DbSet<SimplySharp.Models.Class> Class { get; set; }
        public DbSet<SimplySharp.Models.Assignment> Assignment { get; set; }

        public DbSet<SimplySharp.Models.ClassRegistration> ClassRegistration { get; set; }

        public DbSet<SimplySharp.Models.Submission> Submission { get; set; }

        public DbSet<SimplySharp.Models.Payment> Payment { get; set; }

        /// <summary>
        /// This method specifies the type for the decimal column "PaymentAmount" in the Payment database.
        /// </summary>
        /// <param name="builder">Instance of a ModelBuilder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Payment>().Property(x => x.PaymentAmount).HasPrecision(16, 2);
        }
    }
}
