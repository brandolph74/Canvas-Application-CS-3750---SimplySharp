using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Data;
using SimplySharp.Models;

namespace SimplySharp.Controllers
{
    public class ClassRegistrationController : Controller
    {
        private readonly ClassContext _context;
        private readonly UserManager<SimplySharpUser> _userManager;
        private readonly SimplySharpDBContext _db;

        public ClassRegistrationController(ClassContext context, UserManager<SimplySharpUser> userManager, SimplySharpDBContext db)
        {
            _context = context;
            _userManager = userManager;
            _db = db;
        }
        
        // GET: ClassRegistration/Create
        public async Task<IActionResult> Create(int classId, string studentId)
        {
            if (classId == 0)
            {
                return NotFound();
            }

            var @class = await _context.Class.FirstOrDefaultAsync(m => m.Id == classId);
            if (@class == null)
            {
                return NotFound();
            }
            ClassReg classRegister = new ClassReg
            {
                classId = @class.ClassId,
                className = @class.ClassName,
                credits = @class.Credits,
                location = @class.Location,
                meetingTimes = @class.MeetingDays + " " + @class.StartTime.ToString("HH:mm") + " - " + @class.EndTime.ToString("HH:mm")
            };
            ViewBag.Message = classRegister;

            var @classReg = new ClassRegistration();
            @classReg.ClassId = classId;
            @classReg.StudentId = studentId;
            @classReg.LetterGrade = String.Empty;
            return View(@classReg);
        }

        // POST: ClassRegistration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int classId, string studentId, [Bind("Id,ClassId,StudentId,LetterGrade")] ClassRegistration @classReg)
        {            
            if (ModelState.IsValid)
            {
                _context.Add(@classReg);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }        
            return View();
        }

        // GET: ClassRegistration/Delete
        public async Task<IActionResult> Delete(int classId, string studentId)
        {
            if (classId == 0)
            {
                return NotFound();
            }
            var @class = await _context.Class.FirstOrDefaultAsync(m => m.Id == classId);
            if (@class == null)
            {
                return NotFound();
            }
            ClassReg classRegister = new ClassReg
            {
                classId = @class.ClassId,
                className = @class.ClassName,
                credits = @class.Credits,
                location = @class.Location,
                meetingTimes = @class.MeetingDays + " " + @class.StartTime.ToString("HH:mm") + " - " + @class.EndTime.ToString("HH:mm")
            };
            ViewBag.Message = classRegister;

            var @classReg = await _context.ClassRegistration.FirstOrDefaultAsync(m => m.ClassId == classId && m.StudentId == studentId);
            if (@classReg == null)
            {
                return NotFound();
            }
            return View(@classReg);
        }

        // POST: ClassRegistration/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int Id)
        {
            var @classReg = await _context.ClassRegistration.FindAsync(Id);
            _context.ClassRegistration.Remove(@classReg);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index(string searchString, string Dept)
        {       
            //Step 1: grab the list

            var classList = from c in _db.Class
                            orderby c.ClassId
                            ascending
                            select c;

            //Step 2: narrow by search terms
            

            //Search tutorial: https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/search?view=aspnetcore-5.0
            //Only alter the list if search field or dropdown are not empty!
            if (!String.IsNullOrEmpty(searchString))
            {
                if (!String.IsNullOrEmpty(Dept))
                    classList = (IOrderedQueryable<Class>)classList.Where(s => ((s.ClassId.Contains(searchString) || s.ClassName.Contains(searchString)) && s.Department.Contains(Dept)));
                else
                    classList = (IOrderedQueryable<Class>)classList.Where(s => (s.ClassId.Contains(searchString) || s.ClassName.Contains(searchString))); //VS demanded explicit cast here
            }
            if (String.IsNullOrEmpty(searchString))
            {
                if (!String.IsNullOrEmpty(Dept))
                    classList = (IOrderedQueryable<Class>)classList.Where(s => s.Department.Equals(Dept));
            }

            //Pass list of registered classes to toggle Drop/Register action
            var regClassList = await (from cr in _context.ClassRegistration
                               join c in _context.Class on cr.ClassId equals c.Id
                               where cr.StudentId == (_userManager.GetUserAsync(User).Result.Id)
                               orderby cr.ClassId
                               ascending
                               select cr).ToListAsync();        
            ViewBag.Message = regClassList;

            //Step 3: await the construction of the list, then send to browser

            return View(await classList.ToListAsync());
        }
    }    

    public struct ClassReg
    {
        public string classId;
        public string className;
        public int credits;
        public string location;
        public string meetingTimes;
    }
}
