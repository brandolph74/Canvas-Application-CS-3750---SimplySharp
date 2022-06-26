using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Data;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SimplySharp.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly SimplySharpDBContext _db;
        private readonly ClassContext _clsContext;
        
        
        public StatisticsController(ClassContext clsContext, SimplySharpDBContext db)
        {
            _clsContext = clsContext;
            _db = db;
            
        }
        public async Task <IActionResult> Index()
        {
            //Right now, categoryList just grabs the number of distinct Credit counts in the Class table.
            var categoryList = await (from li in _clsContext.Class select li.Credits).Distinct().ToListAsync(); 
            
            ViewBag.CategoryList = categoryList;
            
            return View();
        }
    }
}
