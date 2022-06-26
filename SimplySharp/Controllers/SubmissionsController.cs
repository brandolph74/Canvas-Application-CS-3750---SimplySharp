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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Newtonsoft.Json;

namespace SimplySharp.Controllers
{
    public class SubmissionsController : Controller
    {
        private readonly SubmissionContext _context;
        private IWebHostEnvironment _environment;

        //private readonly AssignmentContext _acontext;
        private readonly SimplySharpDBContext _db;
        private readonly UserManager<SimplySharpUser> _userManager; 
        private readonly SubmissionContext _subContext;
        public SubmissionsController(SubmissionContext context, UserManager<SimplySharpUser> userManager, SimplySharpDBContext db, SubmissionContext subContext, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _db = db;

            _subContext = subContext;   //Colton
           _environment = environment;

        }


        // GET: Submissions
        public async Task<IActionResult> Index(int? id)
        {
            TempData["assignId"] = id;
            TempData["assignTitle"] = _db.Assignment.Where(u => u.Id == id).Select(u => u.Title).SingleOrDefault();
            TempData["assignDesc"] = _db.Assignment.Where(u => u.Id == id).Select(u => u.Description).SingleOrDefault();
            TempData["assignPoints"] = _db.Assignment.Where(u => u.Id == id).Select(u => u.MaxPoints).SingleOrDefault();

            //Get the username and score for every submission in the assignment
            var ScoreListGrab = await (from sub in _db.Submission
                                       join stu in _db.Users on sub.UserId equals stu.Id
                                       where sub.AssignmentId == id
                                       select sub.Score).ToListAsync();

            //build the 'dictionary' to send to the browser.
            int scoreCount = ScoreListGrab.Count();
            TempData["ScoreCount"] = scoreCount;
            TempData["MaxPoints"] = _db.Assignment.Where(u => u.Id == id).Select(u => u.MaxPoints).SingleOrDefault();
            List<int> scoreList = new List<int>();
            foreach (var item in ScoreListGrab)
            {
                int notNullScore = Convert.ToInt32(item);
                scoreList.Add(notNullScore);

            }

            int[] ScoreArray = scoreList.ToArray();


            //TempData["ScoreList"] = JsonConvert.SerializeObject(ScoreArray);  //THIS LINE CAUSED HTTP 500: needed to serialize the object first.
            TempData["ScoreList"] = ScoreArray;
            if (id == null)
            {
                return View(await _context.Submission.ToListAsync());
            }


            return View((from a in _db.Submission
                         join us in _db.Users on a.UserId equals us.Id
                         where a.AssignmentId == id
                         orderby a.AssignmentId
                         ascending
                         select new
                         {
                             AssignmentId = a.AssignmentId,
                             Userid = us.Id,
                             SubmissionDate = a.SubmissionDate,
                             Id = a.Id,
                             FirstName = us.FirstName,
                             LastName = us.LastName,
                             Score = a.Score
                         }).ToList()
                                .Select(x => new UserAssignmentViewModel()
                                {
                                    AssignmentId = x.AssignmentId,
                                    UserId = x.Userid,
                                    SubmissionDate = x.SubmissionDate,
                                    Id = x.Id,
                                    FirstName = x.FirstName,
                                    LastName = x.LastName,
                                    Score = x.Score
                                }));
        }

        // GET: Submissions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var submission = await _context.Submission
                .FirstOrDefaultAsync(m => m.Id == id);
            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        // GET: Submissions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Submissions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<IFormFile> postedFiles, [Bind("Id,AssignmentId,UserId,Text,File,SubmissionDate,Score,InstructorFeedback")] Submission submission, bool isUnitTest, string unitTestUser)
        {
            if (!isUnitTest)
            {
                ViewBag.classid = @TempData["classid"];
                TempData.Keep("classid");
            }
            
            string fileNames = "";

            if (postedFiles != null && postedFiles.Count > 0)
            {
                TempData["MissingFile"] = "";
                string wwwPath = this._environment.WebRootPath; // path to wwwroot
                string path = Path.Combine(this._environment.WebRootPath, "StudentFileSubmissions"); // Directory for student file submissions
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, submission.UserId); // Directory for the student
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = Path.Combine(path, submission.AssignmentId.ToString()); // Directory for the assignment
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                else if (true)
                {
                    // Delete all files in a directory to avoid obsolete files scenario where
                    // a new submission has fewer files or different file names    
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                List<string> uploadedFiles = new List<string>();
                foreach (IFormFile postedFile in postedFiles)
                {
                    string fileName = Path.GetFileName(postedFile.FileName);
                    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                        uploadedFiles.Add(fileName);
                        fileNames += string.Format("{0}, ", fileName);
                    }
                }
                fileNames = fileNames.Remove(fileNames.Length - 2, 2);
                ViewBag.Message = $"- {fileNames} uploaded.";
            }

            if (!isUnitTest && TempData["FileUpload"] != null && postedFiles.Count == 0)
            {
                TempData["MissingFile"] = "You must choose at least one file";
                return RedirectToAction(nameof(Details), controllerName: "Assignments", new { id = submission.AssignmentId });
            }

            if (ModelState.IsValid)
            {
                var existingsubmission = _context.Submission.Where(x => x.AssignmentId == submission.AssignmentId && x.UserId == (isUnitTest ? unitTestUser : _userManager.GetUserAsync(User).Result.Id)).FirstOrDefault();
                if (existingsubmission == null)
                {
                    submission.File = fileNames;
                    _context.Add(submission); 
                }
                else
                {
                    existingsubmission.Text = submission.Text;
                    existingsubmission.File = fileNames;
                    existingsubmission.SubmissionDate = submission.SubmissionDate;
                }
                await _context.SaveChangesAsync();

                if (!isUnitTest)
                {
                    TempData["Referrer"] = "SaveSubmission";
                    TempData["FileReferrer"] = ViewBag.Message;
                    return RedirectToAction(nameof(Index), controllerName: "Assignments", new { id = ViewBag.classid });
                }                
            }
            return View(submission);
        }

        public FileResult DownloadFile(string filePath)
        {
            string fullName = Path.GetFileName(filePath);

            //Read the File data into Byte Array.
            byte[] bytes = System.IO.File.ReadAllBytes(filePath);

            //Send the File to Download.
            return File(bytes, "application/octet-stream", fullName);
        }

        // GET: Submissions/Edit/5
        public async Task<IActionResult> Edit(int? id, int assignmentid)
        {
            if (id == null)
            {
                return NotFound();
            }

            TempData["assignPoints"] = _db.Assignment.Where(u => u.Id == assignmentid).Select(u => u.MaxPoints).SingleOrDefault();

            var submission = await _context.Submission.FindAsync(id);
            if (submission == null)
            {
                return NotFound();
            }
            TempData["fileSubmission"] = !String.IsNullOrWhiteSpace(submission.File);

            if (!String.IsNullOrWhiteSpace(submission.File))
            {
                //Fetch all files in the Folder (Directory).
                string wwwPath = this._environment.WebRootPath; // path to wwwroot
                string path = Path.Combine(this._environment.WebRootPath, "StudentFileSubmissions"); // Directory for student file submissions
                path = Path.Combine(path, submission.UserId); // Directory for the student            
                path = Path.Combine(path, submission.AssignmentId.ToString()); // Directory for the assignment
                string[] filePaths = Directory.GetFiles(path);

                Dictionary<string, string> submittedFiles = new Dictionary<string, string>();
                foreach (string filePath in filePaths)
                {
                    submittedFiles.Add(Path.GetFileName(filePath), filePath);
                }
                TempData["submittedFiles"] = submittedFiles; // pass them to the view
            }
            return View(submission);
        }

        // POST: Submissions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AssignmentId,UserId,Text,File,SubmissionDate,Score,InstructorFeedback")] Submission submission, bool isUnitTest)
        {
            if (id != submission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(submission);
                    await _context.SaveChangesAsync();

                    if (!isUnitTest) 
                    { 
                            TempData["Referrer"] = "SaveGrade"; 

                        // Add a new notification when assignment is graded.
                        Notification newNoti = new Notification();
                        newNoti.ClassID = (from s in _db.Submission
                                           join a in _db.Assignment on s.AssignmentId equals a.Id
                                           join c in _db.Class on a.ClassId equals c.Id
                                           where (s.AssignmentId == submission.AssignmentId) && (c.Id == a.ClassId)
                                           select c.ClassId
                                           ).FirstOrDefaultAsync().Result;

                        newNoti.AssignmentTitle = (from s in _db.Submission
                                                   join a in _db.Assignment on s.AssignmentId equals a.Id
                                                   where s.AssignmentId == submission.AssignmentId
                                                   select a.Title
                                           ).FirstOrDefaultAsync().Result;

                        newNoti.NotiType = "Graded";
                        newNoti.NotiDate = DateTime.Now;
                        newNoti.UserID = submission.UserId;

                        await _subContext.Notification.AddAsync(newNoti);
                        await _subContext.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubmissionExists(submission.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (!isUnitTest) { return RedirectToAction(nameof(Index), new { id = TempData["assignId"] }); }
            }
            return View(submission);
        }

        // GET: Submissions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var submission = await _context.Submission
                .FirstOrDefaultAsync(m => m.Id == id);
            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        // POST: Submissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var submission = await _context.Submission.FindAsync(id);
            _context.Submission.Remove(submission);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubmissionExists(int id)
        {
            return _context.Submission.Any(e => e.Id == id);
        }
    }
}
