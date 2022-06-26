using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Data;
using SimplySharp.Models;

namespace SimplySharp.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly AssignmentContext _context;
        private readonly ClassContext _class;

        private readonly SubmissionContext _subContext;


        /// <summary>
        /// A UserManager object to access current user information.
        /// </summary>
        private readonly UserManager<SimplySharpUser> _userManager;

        /// <summary>
        /// A DBContext object used to access the database.
        /// </summary>
        private readonly SimplySharpDBContext _db;
        public int classID;


        public AssignmentsController(AssignmentContext context, UserManager<SimplySharpUser> userManager, SimplySharpDBContext db, ClassContext class1, SubmissionContext subContext)
        {
            _context = context;
            _userManager = userManager;
            _db = db;
            _class = class1;
            _subContext = subContext;
        }

        // GET: Assignments

        public async Task<IActionResult> Index(int id)
        {
            //return View(await _context.Assignment.ToListAsync()); This will return all of the classes

            ViewBag.ID = id;

            string classname = _class.Class.Where(u => u.Id == id).Select(u => u.ClassId).SingleOrDefault();


            TempData["className"] = classname;

            TempData["classid"] = id;

            var classAssignments = await _class.Assignment.Where(x => x.ClassId == id).ToListAsync();
            int totalPossiblePoints = 0;
            int userTotalEarnedPoints = 0;
            
            foreach (var assign in classAssignments)
            {
                var userScore = _db.Submission.Where(u => u.AssignmentId == assign.Id && u.UserId == _userManager.GetUserAsync(User).Result.Id).Select(u => u.Score).FirstOrDefault();
                
                if (userScore.HasValue)
                {
                    userTotalEarnedPoints += (int)userScore;
                    totalPossiblePoints += assign.MaxPoints;
                }
                
                
            }
            double userTotalGrade = ((double)userTotalEarnedPoints / (double)totalPossiblePoints) * 100.0;
            TempData["UserTotalGrade"] = userTotalGrade;

            var studentsInClass = await _class.ClassRegistration.Where(x => x.ClassId == id).ToListAsync();
            int studentIndex = 0;
            double[] classTotalPossibleScoreArray = new double[studentsInClass.Count];
            double[] classTotalEarnedScoreArray = new double[studentsInClass.Count];
            foreach (var student in studentsInClass)
            {
                foreach (var assign in classAssignments)
                {
                    var studentScore = _db.Submission.Where(u => u.AssignmentId == assign.Id && u.UserId == student.StudentId).Select(u => u.Score).FirstOrDefault();

                    if (studentScore.HasValue)
                    {
                        classTotalEarnedScoreArray[studentIndex] += (double)studentScore;
                        classTotalPossibleScoreArray[studentIndex] += (double)assign.MaxPoints;
                    }
                }
                studentIndex++;
            }
            double[] classTotalGrades = new double[studentsInClass.Count];
            for (int i = 0; i < studentsInClass.Count; i++)
            {
                classTotalGrades[i] = (classTotalEarnedScoreArray[i] / classTotalPossibleScoreArray[i]) * 100.0;
            }
            TempData["ClassTotalGradeArray"] = classTotalGrades;

            

            //  UPDDATE TO ONLY SHOW ASSIGNMENTS THAT MATCH THE CLASS ID
            return View(await (from a in _db.Assignment
                               where a.ClassId == id
                               orderby a.ClassId
                               ascending
                               select a).ToListAsync());

        }



        // GET: Assignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            /*Get submission object to pull score, text, and file names*/
            var Score = _db.Submission.Where(u => u.AssignmentId == id && u.UserId == _userManager.GetUserAsync(User).Result.Id).Select(u => u.Score).FirstOrDefault();
            var submission = _db.Submission.Where(u => u.AssignmentId == id && u.UserId == _userManager.GetUserAsync(User).Result.Id).FirstOrDefault();
            
            //Grade histogram for students
            if (_userManager.GetUserAsync(User).Result.UserType == "S" && Score.HasValue)
            {
                //Get the username and score for every submission in the assignment
                var ScoreListGrab = await (from sub in _db.Submission
                                           join stu in _db.Users on sub.UserId equals stu.Id
                                           where sub.AssignmentId == id
                                           select sub.Score).ToListAsync();

                //conversion away from null-ints
                List<int> scoreList = new List<int>();
                foreach (var item in ScoreListGrab)
                {
                    if (item.HasValue)
                    {
                        int notNullScore = (int)item;
                        scoreList.Add(notNullScore);
                    }
                }

                TempData["ScoreListLength"] = scoreList.Count();

                //make the list an array; easier to work with in JS and less data surfaced to browser
                int[] scoreArray = scoreList.ToArray();
                TempData["StudentScore"] = (int)Score;   //FOR CHART USE - Useful for highlighting the histogram bucket the submission belongs to
                TempData["ScoreList"] = scoreArray;
            }

            TempData["UserScore"] = Score;
            TempData["MaxPoints"] = _context.Assignment.Where(x => x.Id == id).Select(x => x.MaxPoints).FirstOrDefault();
            if (submission != null)
            {
                TempData["UserText"] = submission.Text;
                TempData["UserFiles"] = submission.File;
            }         
           
            /*Get the due date for the assignment*/
            TempData["DueDate"] = _context.Assignment.Where(u => u.Id == id).Select(u => u.DueDate).FirstOrDefault();

            string classid;
            if (TempData.ContainsKey("classid"))
            {
                classid = TempData["classid"] as string;
            }  
            TempData.Keep("classid");

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        // GET: Assignments/Create
        [Authorize(Roles ="Teacher")]
        public IActionResult Create()
        {

            string classid;

            if (TempData.ContainsKey("classid"))
                classid = TempData["classid"] as string;

            TempData.Keep("classid");
            return View();

        }

        // POST: Assignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClassId,Title,Description,DueDate,MaxPoints,SubmissionType")] Assignment assignment, bool isUnitTest)
        {
            if (!isUnitTest) { ViewBag.classid = @TempData["classid"]; TempData.Keep("classid"); }

            if (ModelState.IsValid)
            {
                _context.Add(assignment);
                await _context.SaveChangesAsync();

                // Get a list of students registered in the class the new assignment was created for.
                var lstStudentIDs = (from u in _db.Users
                                     join cr in _db.ClassRegistration on u.Id equals cr.StudentId
                                     where u.Id == cr.StudentId && cr.ClassId == assignment.ClassId
                                     select u.Id).ToListAsync();

                // Get classId for the class in which the assignment was created.
                var classId = (from c in _class.Class
                               where c.Id == assignment.ClassId
                               select c.ClassId).FirstOrDefaultAsync();

                // Add a new notification for everyone in the class when assignment is created.
                foreach (var userId in lstStudentIDs.Result)
                {
                    Notification newNoti = new Notification();
                    newNoti.ClassID = classId.Result;
                    newNoti.AssignmentTitle = assignment.Title;
                    newNoti.NotiType = "Created";
                    newNoti.NotiDate = DateTime.Now;
                    newNoti.UserID = userId;

                    await _subContext.Notification.AddAsync(newNoti);
                    await _subContext.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index), new { id = ViewBag.classid });        
            }            
            return View(assignment);
        }

        // GET: Assignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.classid = @TempData["classid"];
            TempData.Keep("classid");

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            return View(assignment);
        }

        // POST: Assignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClassId,Title,Description,DueDate,MaxPoints,SubmissionType")] Assignment assignment)
        {
            ViewBag.classid = @TempData["classid"];
            TempData.Keep("classid");

            if (id != assignment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = ViewBag.classid });
            }
            return View(assignment);
        }

        // GET: Assignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.classid = @TempData["classid"];
            TempData.Keep("classid");

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool isUnitTest)
        {
            if (!isUnitTest) { ViewBag.classid = @TempData["classid"]; TempData.Keep("classid"); }          
            
            var assignment = await _context.Assignment.FindAsync(id);
            _context.Assignment.Remove(assignment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = ViewBag.classid });
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignment.Any(e => e.Id == id);
        }
    }
}
