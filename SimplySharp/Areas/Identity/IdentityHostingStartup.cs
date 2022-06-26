using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Data;

[assembly: HostingStartup(typeof(SimplySharp.Areas.Identity.IdentityHostingStartup))]
namespace SimplySharp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<SimplySharpDBContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("SimplySharpDBContextConnection")));

                services.AddDefaultIdentity<SimplySharpUser>(options => options.SignIn.RequireConfirmedAccount = false)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<SimplySharpDBContext>();                    
            });            
        }
    }
}