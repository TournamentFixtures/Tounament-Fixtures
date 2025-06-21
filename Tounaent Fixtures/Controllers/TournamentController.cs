using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Tounaent_Fixtures.Models;

namespace Tounaent_Fixtures.Controllers
{
    public class TournamentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TournamentController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Register()
        {
            var model = new TournamentViewModel
            {
                DistrictOptions = await GetDistrictsAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(TournamentViewModel model)
        {
            var district = await _context.TblDistricts
                .Where(d => d.DistictId == model.DistictId).FirstOrDefaultAsync();
            byte[]? logo1bytes = null;
            if (model.Logo1 != null && model.Logo1.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.Logo1.CopyToAsync(memoryStream);
                    logo1bytes = memoryStream.ToArray();
                }
            }
            byte[]? logo2bytes = null;
            if (model.Logo2 != null && model.Logo2.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await model.Logo2.CopyToAsync(memoryStream);
                    logo2bytes = memoryStream.ToArray();
                }
            }
            var tournament = new TblTournament
            {
                TournamentName = model.TournamentName,
                OrganizedBy = model.OrganizedBy,
                Venue = model.Venue,
                FromDt = model.From_dt,
                ToDt = model.To_dt,
                AddedDt = DateTime.Now,
                AddedBy = User.Identity?.Name ?? "admin",
                IsActive = model.IsActive,
                DistictId = model.DistictId,
                DistictName = district.DistictName,
                Logo1 = logo1bytes,
                Logo2 = logo2bytes
            };


            // 1. Save tournament to get generated TournamentId
            _context.TblTournament.Add(tournament);
            await _context.SaveChangesAsync(); // now tournament.TournamentId is populated

            // 2. Generate token and update URL
            var token = UrlEncryptionHelper.Encrypt(tournament.TournamentId.ToString());
            tournament.URL = $"{Request.Scheme}://{Request.Host}/PlayerRegistration/Register?token={token}";

            // 3. Save updated URL
            _context.TblTournament.Update(tournament);
            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] = "Tournament successfully registered!";

            return View(new TournamentViewModel());


        }
    }

}
