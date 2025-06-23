using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Tounaent_Fixtures.Models;

namespace Tounaent_Fixtures.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;

        }
        public async Task<IActionResult> TournamentManagement()
        {
            var tournaments = await _context.TblTournament.ToListAsync();
            return View(tournaments);
        }

        public async Task<IActionResult> DistrictManagement()
        {
            var districts = await _context.TblDistricts.ToListAsync();
            return View(districts);
        }


        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GenderManagement()
        {
            var genders = await _context.Gender.ToListAsync();
            return View(genders);  // Return all genders to the view
        }
        // Edit Gender Action (GET)
        public async Task<IActionResult> EditGender(int id)
        {
            var gender = await _context.Gender.FindAsync(id);
            if (gender == null)
            {
                return NotFound();
            }
            return View(gender);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateGender(int genderId, string genderName)
        {
            var gender = await _context.Gender.FindAsync(genderId);
            if (gender == null)
            {
                return NotFound();
            }

            gender.GenderName = genderName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Gender.Any(e => e.GenderId == genderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();  // Return success response
        }

        // Edit Gender Action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGender(int id, Gender gender)
        {
            if (id != gender.GenderId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gender);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Gender.Any(e => e.GenderId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(GenderManagement));
            }
            return View(gender);
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