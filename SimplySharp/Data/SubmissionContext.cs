using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Models;

namespace SimplySharp.Data
{
    public class SubmissionContext : DbContext
    {
        public SubmissionContext (DbContextOptions<SubmissionContext> options)
            : base(options)
        {
        }

        public DbSet<SimplySharp.Models.Submission> Submission { get; set; }
        public DbSet<SimplySharp.Models.Notification> Notification { get; set; }
    }
}
