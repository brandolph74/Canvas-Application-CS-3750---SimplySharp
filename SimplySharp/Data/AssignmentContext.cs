using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Models;

namespace SimplySharp.Data
{
    public class AssignmentContext : DbContext
    {
        public AssignmentContext (DbContextOptions<AssignmentContext> options)
            : base(options)
        {
        }

        public DbSet<SimplySharp.Models.Assignment> Assignment { get; set; }
    }
}
