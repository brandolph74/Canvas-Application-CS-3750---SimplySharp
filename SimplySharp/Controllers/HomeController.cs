using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Data;
using SimplySharp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimplySharp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<SimplySharpUser> _userManager;
        private readonly SimplySharpDBContext _db;
        private readonly ClassContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(UserManager<SimplySharpUser> userManager, SimplySharpDBContext db, ClassContext context, ILogger<HomeController> logger)
        {
            _userManager = userManager;
            _db = db;
            _context = context;
            _logger = logger;
        }

        public static List<Class> list;

        public static IEnumerable<SimplySharp.Models.Class> classList;

        public static IEnumerable<SimplySharp.Models.Submission> submissionList;

        public static IEnumerable<SimplySharp.Models.Assignment> assignmentList;

        public static bool reload = true; //bool changed when the classes need to be reloaded

        public static string userID;  //save the user ID

        public async Task<IActionResult> Index()
        {
            var model = new StudentAssignmentViewModel();  //create the viewmodel object, ViewModel is used since multiple models(class and assignment) are needed in the view
            if ((_userManager.GetUserAsync(User).Result.UserType == "T" && reload == true) || (_userManager.GetUserAsync(User).Result.Id != userID && _userManager.GetUserAsync(User).Result.UserType == "T"))
            {
                userID = _userManager.GetUserAsync(User).Result.Id;

                model.Classes = await (from c in _context.Class
                                       where c.Instructor ==
                                       (_userManager.GetUserAsync(User).Result.FirstName + " " +
                                       _userManager.GetUserAsync(User).Result.LastName)
                                       orderby c.ClassId
                                       ascending
                                       select c).ToListAsync();

                DateTime currentDate = DateTime.Now;  //grab the current date

                model.Assignment = await (from c in _context.Assignment
                                          join b in _context.Class on c.ClassId equals b.Id
                                          where b.Instructor == (_userManager.GetUserAsync(User).Result.FirstName + " " + _userManager.GetUserAsync(User).Result.LastName)
                                          orderby c.DueDate
                                          ascending
                                          select c).ToListAsync();



                model.Submission = await (from c in _context.Submission
                                          join b in _context.Assignment on c.AssignmentId equals b.Id
                                          join d in _context.Class on b.ClassId equals d.Id
                                          where d.Instructor == (_userManager.GetUserAsync(User).Result.FirstName + " " + _userManager.GetUserAsync(User).Result.LastName)  //check if assignment due date is equal or past
                                          orderby b.DueDate
                                          ascending
                                          select c).ToListAsync();

                reload = false;
                
                classList = model.Classes;
                assignmentList = model.Assignment;
                submissionList = model.Submission;

            }
            else if ((_userManager.GetUserAsync(User).Result.UserType != "T" && reload == true) || (_userManager.GetUserAsync(User).Result.Id != userID && _userManager.GetUserAsync(User).Result.UserType != "T"))
            {
                userID = _userManager.GetUserAsync(User).Result.Id;

                model.Classes = await (from cr in _context.ClassRegistration
                                       join c in _context.Class on cr.ClassId equals c.Id
                                       where cr.StudentId == (_userManager.GetUserAsync(User).Result.Id)
                                       orderby cr.ClassId
                                       ascending
                                       select c).ToListAsync();

                DateTime currentDate = DateTime.Now;  //grab the current date

                model.Assignment = await (from c in _context.Assignment
                                          join b in _context.ClassRegistration on c.ClassId equals b.ClassId
                                          where b.StudentId == (_userManager.GetUserAsync(User).Result.Id) && c.DueDate >= currentDate  //check if assignment due date is equal or past
                                          orderby c.DueDate
                                          ascending
                                          select c).ToListAsync();

                classList = model.Classes;
                assignmentList = model.Assignment;

            }
            else  //if nothing has changed, just set the cached lists to the model
            {
                model.Classes = classList;
                model.Assignment = assignmentList;

                if (_userManager.GetUserAsync(User).Result.UserType == "T")
                {
                    model.Submission = submissionList;
                }

            }



            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
