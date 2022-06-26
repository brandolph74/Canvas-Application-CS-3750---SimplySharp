using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SimplySharpUnitTests
{
    [TestClass]
    public class ArpitExampleForUnitTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            SimplySharp.Data.ClassContext context;
            string connectionString = "Server=tcp:lmssimplysharpdbserver.database.windows.net,1433;Initial Catalog=SimplySharp_db;Persist Security Info=False;User ID=SimplySharp;Password=Password$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            //string connectionString = "Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSSimplySharp;User ID=LMSSimplySharp;Password=Password$implysharp;";


            DbContextOptions<SimplySharp.Data.ClassContext> options = new DbContextOptions<SimplySharp.Data.ClassContext>();
            DbContextOptionsBuilder builder = new DbContextOptionsBuilder(options);
            SqlServerDbContextOptionsExtensions.UseSqlServer(builder, connectionString, null);
            context = new SimplySharp.Data.ClassContext((DbContextOptions<SimplySharp.Data.ClassContext>)builder.Options);
            var classList = await context.Class.ToListAsync();
            var classListForAnInstructor = classList.FindAll(x => x.Instructor == "arpit instructor");
            int N = classListForAnInstructor.Count;

            System.Diagnostics.Debug.WriteLine(N);

        }
    }
}
