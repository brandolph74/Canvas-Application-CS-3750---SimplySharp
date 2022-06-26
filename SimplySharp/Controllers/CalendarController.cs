using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Data;
using SimplySharp.Models;


namespace SimplySharp.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ClassContext _context;

        private readonly UserManager<SimplySharpUser> _userManager;

        private readonly SimplySharpDBContext _db;

        public CalendarController(ClassContext context, UserManager<SimplySharpUser> userManager, SimplySharpDBContext db)
        {
            _context = context;
            _userManager = userManager;
            _db = db;
        }
        public async Task<IActionResult> Calendar()
        {
            DateTime summerSemesterStart = new DateTime(2021, 6, 1);
            DateTime summerSemesterEnd = new DateTime(2021, 8, 31);
            List<Event> evn = new List<Event>();
            string[] courseColors = new string[20] { "blue", "purple", "green", "red", "#19452F", "#9506AB", "#AB7206", "#3D9309", "#9AAB06", "#800424",
                                                     "#692C73", "#2C7339", "#7E1005", "#1F06AB", "#8238C2", "#B0120A", "#044B28", "#72045B", "#E30606", "#045451" };
            int counter = 0;

            //Instructor block
            if (_userManager.GetUserAsync(User).Result.UserType == "T")
            {
                var classEvents = await _db.Class.Where(x => x.Instructor == (_userManager.GetUserAsync(User).Result.FirstName + " " + _userManager.GetUserAsync(User).Result.LastName)).ToListAsync();
                foreach (var sClassEvent in classEvents)
                {
                    var course = await _db.Class.FirstOrDefaultAsync(x => x.Id == sClassEvent.Id);
                    string[] meetDays = course.MeetingDays.Split(" ");
                    DayOfWeek[] daysOfWeek = new DayOfWeek[meetDays.Length];
                    for (int i = 0; i < meetDays.Length; i++)
                    {
                        switch (meetDays[i])
                        {
                            case "M": daysOfWeek[i] = DayOfWeek.Monday; break;
                            case "T": daysOfWeek[i] = DayOfWeek.Tuesday; break;
                            case "W": daysOfWeek[i] = DayOfWeek.Wednesday; break;
                            case "TH": daysOfWeek[i] = DayOfWeek.Thursday; break;
                            case "F": daysOfWeek[i] = DayOfWeek.Friday; break;
                            case "S": daysOfWeek[i] = DayOfWeek.Saturday; break;
                            case "SU": daysOfWeek[i] = DayOfWeek.Sunday; break;
                        }
                    }

                    for (DateTime theDay = summerSemesterStart; theDay <= summerSemesterEnd; theDay.AddDays(1))
                    {
                        theDay = theDay.AddDays(1);
                        foreach (DayOfWeek courseDay in daysOfWeek)
                        {
                            if (theDay.DayOfWeek == courseDay)
                            {
                                evn.Add(new Event
                                {
                                    ClassId = course.ClassId,
                                    Title = course.ClassName,
                                    StartDate = new DateTime(theDay.Year, theDay.Month, theDay.Day, course.StartTime.Hour, course.StartTime.Minute, 00).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    EndDate = new DateTime(theDay.Year, theDay.Month, theDay.Day, course.EndTime.Hour, course.EndTime.Minute, 00).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    CourseColor = counter < 20 ? courseColors[counter] : "blue"
                                });
                            }
                        }
                    }
                    counter++;
                };

                ViewData["events"] = evn;
            }

            //Student block
            if (_userManager.GetUserAsync(User).Result.UserType == "S")
            {
                // Weekly Classes
                var regEvents = await _db.ClassRegistration.Where(x => x.StudentId == (_userManager.GetUserAsync(User).Result.Id)).ToListAsync();
                foreach (var sRegEvent in regEvents)
                {
                    var course = await _db.Class.FirstOrDefaultAsync(x => x.Id == sRegEvent.ClassId);
                    string[] meetDays = course.MeetingDays.Split(" ");
                    DayOfWeek[] daysOfWeek = new DayOfWeek[meetDays.Length];
                    for (int i = 0; i < meetDays.Length; i++)
                    {
                        switch (meetDays[i])
                        {
                            case "M": daysOfWeek[i] = DayOfWeek.Monday; break;
                            case "T": daysOfWeek[i] = DayOfWeek.Tuesday; break;
                            case "W": daysOfWeek[i] = DayOfWeek.Wednesday; break;
                            case "TH": daysOfWeek[i] = DayOfWeek.Thursday; break;
                            case "F": daysOfWeek[i] = DayOfWeek.Friday; break;
                            case "S": daysOfWeek[i] = DayOfWeek.Saturday; break;
                            case "SU": daysOfWeek[i] = DayOfWeek.Sunday; break;
                        }
                    }

                    for (DateTime theDay = summerSemesterStart; theDay <= summerSemesterEnd; theDay.AddDays(1))
                    {
                        theDay = theDay.AddDays(1);
                        foreach (DayOfWeek courseDay in daysOfWeek)
                        {
                            if (theDay.DayOfWeek == courseDay)
                            {
                                evn.Add(new Event
                                {
                                    ClassId = course.ClassId,
                                    Title = course.ClassName,
                                    StartDate = new DateTime(theDay.Year, theDay.Month, theDay.Day, course.StartTime.Hour, course.StartTime.Minute, 00).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    EndDate = new DateTime(theDay.Year, theDay.Month, theDay.Day, course.EndTime.Hour, course.EndTime.Minute, 00).ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    CourseColor = counter < 20 ? courseColors[counter] : "blue"
                                });
                            }
                        }
                    }
                    counter++;
                };
                ViewData["events"] = evn;

                // Assignments
                List<AssignmentEvent> assgnEvents = new List<AssignmentEvent>();

                var regAssgns = await (from a in _db.Assignment
                                          join cr in _db.ClassRegistration on a.ClassId equals cr.ClassId
                                          where cr.StudentId == (_userManager.GetUserAsync(User).Result.Id) && a.Id >=0
                                          orderby a.Id
                                          ascending
                                          select a).ToListAsync();

                foreach (var assgn in regAssgns)
                {
                    var assignment = await _db.Assignment.FirstOrDefaultAsync(x => x.Id == assgn.Id);
                    var course = await _db.Class.FirstOrDefaultAsync(x => x.Id == assgn.ClassId);

                    assgnEvents.Add(new AssignmentEvent
                    {
                        Id = assignment.Id,
                        CourseName = course.ClassId,
                        AssignmentTitle = assignment.Title,
                        DueDate = assignment.DueDate.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        // If an assignment is due at 11:59 pm, DueDateOffset allows the assignment to be visible on the calendar.
                        DueDateOffset = (assignment.DueDate.AddHours(-1)).ToString("yyyy-MM-dd HH:mm:ss.fff")
                    }) ;
                }
                ViewData["assgnEvents"] = assgnEvents;


            }
            return View();
        }

    }
}
