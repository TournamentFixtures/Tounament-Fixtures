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

        public async Task<IActionResult> Register()
        {
            var model = new PlayerViewModel
            {
                GenderOptions = await GetGendersAsync(),
                DistrictOptions = await GetDistrictsAsync(),
                ClubOptions = new List<SelectListItem>() // initially empty
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
        [HttpGet]
        public async Task<IActionResult> GetClubsByDistrict(int districtId)
        {
            var clubs = await _context.TblDistLocalClubs
                .Where(c => c.DistictId == districtId && c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.ClubId.ToString(),
                    Text = c.LocalClubName
                })
                .ToListAsync();

            return Json(clubs);
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
            if (age < 6) categoryName = "PeeWee";
            else if (age < 14) categoryName = "SubJunior";
            else if (age < 17) categoryName = "Junior";
            else if (age < 20) categoryName = "Cadet";
            else categoryName = "Senior";

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
        public async Task<IActionResult> Register(PlayerViewModel model)
        {
            // Reload dropdowns on POST
            model.GenderOptions = await GetGendersAsync();
            model.DistrictOptions = await GetDistrictsAsync();

            var category = await _context.TblCategory
                .Where(c => c.GenId == model.GenderId && c.CategoryName == model.CategoryName && c.IsActive)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                ModelState.AddModelError("CatId", "No matching category found.");
                return View(model);
            }

            model.CatId = category.CatId;

            if (ModelState.IsValid)
            {
                var entity = new TblTournamentUserReg
                {
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
                    AddedBy = User.Identity?.Name ?? "admin"
                };

                _context.TblTournamentUserRegs.Add(entity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Player registered successfully!";
                return RedirectToAction("Register");
            }

            return View(model);
        }
    }
}
