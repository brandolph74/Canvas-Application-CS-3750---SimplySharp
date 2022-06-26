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
    public class ClassesController : Controller
    {
        private readonly ClassContext _context;

        /// <summary>
        /// A UserManager object to access current user information.
        /// </summary>
        private readonly UserManager<SimplySharpUser> _userManager;

        /// <summary>
        /// A DBContext object used to access the database.
        /// </summary>
        private readonly SimplySharpDBContext _db;


        public ClassesController(ClassContext context, UserManager<SimplySharpUser> userManager, SimplySharpDBContext db)
        {
            _context = context;
            _userManager = userManager;
            _db = db;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
          
            // Only return classes the current user is the instructor for the class.
            return View(await (from c in _db.Class 
                               where c.Instructor == 
                               (_userManager.GetUserAsync(User).Result.FirstName + " " + 
                               _userManager.GetUserAsync(User).Result.LastName) 
                               orderby c.ClassId 
                               ascending select c).ToListAsync());
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var @class = await _context.Class
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }

            return View(@class);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClassId,ClassName,Credits,Capacity,Location,MeetingDays,StartTime,EndTime,Department,Instructor")] Class @class)
        {
            if (@class.MeetingDays == null)
            {
                ModelState.AddModelError("", "You must select at least one Meeting Day");
            }

            if (ModelState.IsValid)
            {
                _context.Add(@class);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            
            HomeController.reload = true;  //flag to reload data for student again

            return View(@class);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var @class = await _context.Class.FindAsync(id);
            if (@class == null)
            {
                return NotFound();
            }
            HomeController.reload = true;  //flag to reload data for student again

            return View(@class);
        }

        // POST: Classes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClassId,ClassName,Credits,Capacity,Location,MeetingDays,StartTime,EndTime,Instructor,Department")] Class @class)
        {
            if (id != @class.Id)
            {
                return NotFound();
            }

            if (@class.MeetingDays == null)
            {
                ModelState.AddModelError("", "You must select at least one Meeting Day");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@class);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(@class.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            HomeController.reload = true;  //flag to reload data for student again
            return View(@class);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var @class = await _context.Class
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@class == null)
            {
                return NotFound();
            }
            
            HomeController.reload = true;  //flag to reload data for student again

            return View(@class);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @class = await _context.Class.FindAsync(id);
            _context.Class.Remove(@class);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassExists(int id)
        {
            return _context.Class.Any(e => e.Id == id);
        }
    }
}
