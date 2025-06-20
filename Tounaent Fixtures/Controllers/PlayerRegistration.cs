using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tounaent_Fixtures.Models;

namespace Tounaent_Fixtures.Controllers
{
    public class PlayerRegistration : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlayerRegistration(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Register(int tr_id)
        {
            var tournament = await _context.TblTournament
                .Where(t => t.TournamentId == tr_id)
                .Select(t => new { t.TournamentName, t.OrganizedBy, t.Venue, t.ToDt, t.FromDt, t.DistictName, t.DistictId })
                .FirstOrDefaultAsync();

            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            ViewData["TournamentName"] = tournament.TournamentName;
            ViewData["Organization"] = tournament.OrganizedBy;
            ViewData["Venue"] = tournament.Venue;
            if (tournament.FromDt == tournament.ToDt)
            {
                ViewData["Date"] = tournament.FromDt?.ToString("dd-MM-yyyy");
            }
            else
            {
                ViewData["Date"] = tournament.FromDt?.ToString("dd-MM-yyyy") + " - " + tournament.ToDt?.ToString("dd-MM-yyyy");
            }

            var model = new PlayerViewModel
            {
                TournamentId = tr_id, 
                GenderOptions = await GetGendersAsync(),
                DistrictName = tournament.DistictName,
                DistictId = (int)tournament.DistictId,
                //DistrictOptions = await GetDistrictsAsync(),
                ClubOptions = await GetClubsByDistrict((int)tournament.DistictId) 
            };

            return View(model);
        }


        private async Task<List<SelectListItem>> GetDistrictsAsync()
        {
            return await _context.TblDistricts
                .Where(d => d.IsActive)
                .Select(d => new SelectListItem
                {
                    Value = d.DistictId.ToString(),
                    Text = d.DistictName
                }).ToListAsync();
        }
        public async Task<List<SelectListItem>> GetClubsByDistrict(int districtId)
        {
            return await _context.TblDistLocalClubs
                .Where(c => c.DistictId == districtId && c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.ClubId.ToString(), // or ClubId if you're storing ID
                    Text = c.LocalClubName
                })
                .ToListAsync();

        }




        private async Task<List<SelectListItem>> GetGendersAsync()
        {
            return await _context.Gender
                .Select(g => new SelectListItem
                {
                    Value = g.GenderId.ToString(),
                    Text = g.GenderName
                })
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryByGenderAndAge(int genderId, int age)
        {
            string categoryName;
            if (age < 7) categoryName = "PeeWee";
            else if (age < 11) categoryName = "SubJunior";
            else if (age < 14) categoryName = "Cadet";
            else if (age < 17) categoryName = "Junior";
            else if (age >= 17) categoryName = "Senior";
            else categoryName = "---Select Category---";

            var category = await _context.TblCategory
                .Where(c => c.GenId == genderId && c.CategoryName == categoryName && c.IsActive)
                .FirstOrDefaultAsync();

            if (category != null)
            {
                return Json(new { catId = category.CatId, categoryName = category.CategoryName });
            }

            return Json(new { });
        }

        [HttpGet]
        public async Task<IActionResult> GetWeightCategoriesByCategory(int catId)
        {
            var weights = await _context.TblWeightCategory
                .Where(w => w.CatId == catId && w.IsActive)
                .Select(w => new SelectListItem
                {
                    Value = w.WeightCatId.ToString(),
                    Text = w.WeightCatName
                })
                .ToListAsync();

            return Json(weights);
        }

        [HttpPost]
        public async Task<IActionResult> Register(PlayerViewModel model, int tr_id)
        {

            // Reload dropdowns on POST
            model.GenderOptions = await GetGendersAsync();
            model.DistrictOptions = await GetDistrictsAsync();


            var tournament = await _context.TblTournament
       .Where(t => t.TournamentId == tr_id)
       .Select(t => new { t.TournamentName, t.OrganizedBy, t.Venue, t.DistictId })
       .FirstOrDefaultAsync();

            if (tournament != null)
            {
                ViewData["TournamentName"] = tournament.TournamentName;
                ViewData["Organization"] = tournament.OrganizedBy;
                ViewData["Venue"] = tournament.Venue;
            }
            var gender = await _context.Gender
                .Where(c => c.GenderId == model.GenderId)
                .FirstOrDefaultAsync();
            var category = await _context.TblCategory
                .Where(c => c.GenId == model.GenderId && c.CategoryName == model.CategoryName && c.IsActive)
                .FirstOrDefaultAsync();
            var club = await _context.TblDistLocalClubs
                .Where(c => c.ClubId == model.ClubId).FirstOrDefaultAsync();
            var district = await _context.TblDistricts
                .Where(d => d.DistictId == model.DistictId).FirstOrDefaultAsync();
            var weightcategory = await _context.TblWeightCategory
                .Where(w => w.WeightCatId == model.WeightCatId).FirstOrDefaultAsync();

            if (category == null)
            {
                ModelState.AddModelError("CatId", "No matching category found.");
                return View(model);
            }

            model.CatId = category.CatId;
            model.ClubName = club.LocalClubName;
            byte[]? photoBytes = null;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.PhotoFile.CopyToAsync(memoryStream);
                    photoBytes = memoryStream.ToArray();
                }
            }


            var entity = new TblTournamentUserReg
                {
                    TrId = model.TournamentId,
                    Name = model.Name,
                    FatherName = model.FatherName,
                    GenderId = model.GenderId,
                    MobileNo = model.MobileNo,
                    Email = model.Email,
                    Dob = model.Dob,
                    CatId = model.CatId,
                    WeightCatId = model.WeightCatId,
                    DistrictId = model.DistictId, 
                    ClubName = model.ClubName,
                    AdharNumb = model.AdharNumb,
                    Address = model.Address,
                    Remarks = model.Remarks,
                    IsVerified = false,
                    IsActive = model.IsActive,
                    AddedDt = DateTime.Now,
                    AddedBy = User.Identity?.Name ?? "admin",
                    CategoryName = category.CategoryName,
                    District = district.DistictName,
                    Gender = gender.GenderName,
                    WeighCatName = weightcategory.WeightCatName,
                    Photo = photoBytes
                };

                _context.TblTournamentUserRegs.Add(entity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Player registered successfully!";
                return RedirectToAction("Register", new {tr_id = "4"});

        }
    }
}
