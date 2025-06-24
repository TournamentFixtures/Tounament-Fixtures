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
        public async Task<IActionResult> PlayerManagement()
        {
            var players = await _context.TblTournamentUserRegs
                .Select(p => new PlayerExportViewModel
                {
                    TrUserId = p.TrUserId,
                    TrId = p.TrId,
                    Name = p.Name,
                    FatherName = p.FatherName,
                    Gender = p.Gender,
                    MobileNo = p.MobileNo,
                    Email = p.Email,
                    Dob = p.Dob,
                    CategoryName = p.CategoryName,
                    WeighCatName = p.WeighCatName,
                    District = p.District,
                    ClubName = p.ClubName,
                    Address = p.Address
                })
                .ToListAsync();
            return View(players);
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

        public async Task<IActionResult> ExportTournamentsToExcel()
        {
            var tournaments = await _context.TblTournament.ToListAsync();

            var columns = new Dictionary<string, Func<TblTournament, object>>
    {
        { "Tournament Name", t => t.TournamentName },
        { "Organizer", t => t.OrganizedBy },
        { "Venue", t => t.Venue },
        { "From Date", t => t.FromDt?.ToString("dd-MM-yyyy") },
        { "To Date", t => t.ToDt?.ToString("dd-MM-yyyy") },
        {"URL", t => t.URL }
        // Add more if needed
    };

            byte[] excelBytes = ExcelExportHelper.ExportToExcel(tournaments, columns, "Tournaments");

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tournaments.xlsx");
        }

        public async Task<IActionResult> ExportDistrictsToExcel()
        {
            var districts = await _context.TblDistricts.ToListAsync();

            var columns = new Dictionary<string, Func<TblDistrict, object>>
    {
        { "District Name", d => d.DistictName },
        { "Is Active", d => d.IsActive ? "Yes" : "No" }
    };

            byte[] excelBytes = ExcelExportHelper.ExportToExcel(districts, columns, "Districts");

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Districts.xlsx");
        }
        public async Task<IActionResult> ExportGenderToExcel()
        {
            var gender = await _context.Gender.ToListAsync();

            var columns = new Dictionary<string, Func<Gender, object>>
    {
        { "Gender Name", d => d.GenderName },
        {"Gender Id", d => d.GenderId },
        { "Is Active", d => d.IsActive}
    };

            byte[] excelBytes = ExcelExportHelper.ExportToExcel(gender, columns, "Gender");

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Gender.xlsx");
        }

        public async Task<IActionResult> ExportPlayerToExcel()
        {
            var players = await _context.TblTournamentUserRegs
                    .Select(p => new PlayerExportViewModel
                    {
                        TrUserId = p.TrUserId,
                        TrId = p.TrId,
                        Name = p.Name,
                        FatherName = p.FatherName,
                        Gender = p.Gender,
                        MobileNo = p.MobileNo,
                        Email = p.Email,
                        Dob = p.Dob,
                        CategoryName = p.CategoryName,
                        WeighCatName = p.WeighCatName,
                        District = p.District,
                        ClubName = p.ClubName,
                        Address = p.Address
                    })
                    .ToListAsync();
            var columns = new Dictionary<string, Func<PlayerExportViewModel, object>>
    {
        { "User ID", d => d.TrUserId },
        {"Tournament Id", d => d.TrId },
        { "Name", d => d.Name},
                { "Father Name", d => d.FatherName },
                { "Gender", d => d.Gender },
                { "Mobile Number", d => d.MobileNo },
                { "Email", d => d.Email },
                { "DOB", d => d.Dob },
                { "Category Name", d => d.CategoryName },
                { "Weight Category Name", d => d.WeighCatName},
                { "District", d => d.District },
                { "Club Name", d => d.ClubName },
                { "Address", d => d.Address }
    };

            byte[] excelBytes = ExcelExportHelper.ExportToExcel(players, columns, "Players");

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Players.xlsx");
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