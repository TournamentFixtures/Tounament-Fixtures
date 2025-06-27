using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using Tounaent_Fixtures.Models;
//using IronPdf;
using System;
using DocumentFormat.OpenXml.Packaging;

using System.Threading.Tasks;
//using PuppeteerSharp;
//using MimeKit;
//using MailKit.Net.Smtp;
//using DinkToPdf.Contracts;
//using DinkToPdf;


namespace Tounaent_Fixtures.Controllers
{
    public class PlayerRegistration : Controller
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private static readonly object _pdfLock = new();
		// private readonly IConverter _converter;

        public PlayerRegistration(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
           // _converter = converter; IConverter converter,
            _context = context;
        }

        public async Task<IActionResult> Register(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest("Missing token");

            int tr_id;
            try
            {
                var decrypted = UrlEncryptionHelper.Decrypt(token);
                tr_id = int.Parse(decrypted);
            }
            catch
            {
                return BadRequest("Invalid or tampered token.");
            }
            var tournament = await _context.TblTournament
                .Where(t => t.TournamentId == tr_id)
                .FirstOrDefaultAsync();

            if (tournament == null)
            {
                return NotFound("Tournament not found.");
            }

            ViewData["TournamentName"] = tournament.TournamentName;
            ViewData["Organization"] = tournament.OrganizedBy;
            ViewData["Venue"] = tournament.Venue;
            ViewData["Logo1"] = tournament.Logo1 != null ? $"data:image/png;base64,{Convert.ToBase64String(tournament.Logo1)}" : null;
            ViewData["Logo2"] = tournament.Logo2 != null ? $"data:image/png;base64,{Convert.ToBase64String(tournament.Logo2)}" : null;

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
            if (age < 7) categoryName = "Kids";
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
       .Where(t => t.TournamentId == model.TournamentId).FirstOrDefaultAsync();

            if (tournament != null)
            {
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
                .Where(d => d.DistictId == tournament.DistictId).FirstOrDefaultAsync();
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
                DistrictId = district.DistictId,
                ClubName = model.ClubName,
                AdharNumb = model.AdharNumb,
                Address = model.Address,
                Remarks = Convert.ToString("TNTA_SLM_"+model.Id+1),
                IsVerified = false,
                IsActive = model.IsActive,
                AddedDt = DateTime.Now,
                AddedBy = User.Identity?.Name ?? "admin",
                CategoryName = category.CategoryName,
                District = district.DistictName,
                Gender = gender.GenderName,
                WeighCatName = weightcategory.WeightCatName,
                Photo = photoBytes,
                
            };

            _context.TblTournamentUserRegs.Add(entity);
            await _context.SaveChangesAsync();
            string token = UrlEncryptionHelper.Encrypt(model.TournamentId.ToString());


            //byte[] idCardPdf = GenerateIdCardPdf(model, photoBytes, tournament.Logo1, tournament.Logo2,
            //    weightcategory.WeightCatName, gender.GenderName);
            await SendEmailAsync(model.Email, model, tournament.TournamentName);

            TempData["Success"] = "Player registered successfully!";

            return RedirectToAction("Register", new { token = token });


        }

        private string GenerateIdCardPdf(PlayerViewModel model, byte[] photoBytes, byte[] Logo1, byte[] Logo2,
            string argWeightCat, string argGender)
        {
            string base64Image = photoBytes != null
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(photoBytes)}"
            : "";
            string base64ImageLogo1 = Logo1 != null
                ? $"data:image/jpeg;base64,{Convert.ToBase64String(Logo1)}"
                : "";
            string base64ImageLogo2 = Logo2 != null
                ? $"data:image/jpeg;base64,{Convert.ToBase64String(Logo2)}"
                : "";
            var htmlContent = $@"
       <html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <title>{ViewData["TournamentName"]}</title>
  <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"" rel=""stylesheet"">
  <script src=""https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js""></script>
  <style>
    table {{
      width: 100%;
      border-collapse: collapse;
    }}
    table td {{
      border: none;
      padding: 6px;
      vertical-align: top;
    }}
    .photo-box {{
      border: 1px solid black;
      width: 100px;
      height: 120px;
      text-align: center;
      line-height: 120px;
      margin-left: auto;
    }}
    .form-control {{
      border: none;
      border-bottom: solid 1px black;
      border-radius: 0 !important;
    }}
    textarea {{
      resize: none;
    }}
    input[type=""checkbox""] {{
  width: 18px;
  height: 18px;
  margin-right: 6px;
  vertical-align: middle;
  accent-color: #0d6efd;
}}
label.checkbox-label {{
  margin-right: 15px;
  display: inline-flex;
  align-items: center;
  font-weight: normal;
}}

  </style>
</head>
<body>
<div class=""container my-4"" id=""form-content"">
  <table>
    <tr>
      <td style=""width: 25%; text-align: center;"">
 {(string.IsNullOrEmpty(base64ImageLogo1) ? "" : $"<img class='photo' src='{base64ImageLogo1}' alt='Photo' height='100px' widht='120px' />")}</td>
      <td style=""width: 50%; text-align: center;"">
        <h4>{ViewData["TournamentName"]}</h4>
        <p>Date: {Convert.ToString(ViewData["Date"])}</p>
        <p> {ViewData["Organization"]} </p>
      </td>
      <td style=""width: 25%; text-align: center;"">
 {(string.IsNullOrEmpty(base64ImageLogo2) ? "" : $"<img class='photo' src='{base64ImageLogo2}' alt='Photo' height='100px' widht='120px' />")}</td>
    </tr>
  </table>

  <p style=""border-top: 2px solid black; border-bottom: 2px solid black; padding: 10px; text-align:center;"">
    <strong>Organised by:</strong> {ViewData["Organization"]}<br>
    <strong>Promoted by:</strong> SALEM DISTRICT AMATEUR TAEKWONDO ASSOCIATION (R)<br>
    <strong>Under the Auspicious of:</strong> TAMILNADU TAEKWONDO ASSOCIATION (R)
  </p>

  <h5 class=""text-center mt-4"">INDIVIDUAL ENTRY FORM</h5>

  <table class=""mb-3"">
    <tr>
      <td style=""width: 75%""> <strong>GENDER - {model.Gender} </strong>
        
<br>
        <strong>CATEGORY - {model.CategoryName} </strong>
<br>
      </td>
      <td><div class=""photo-box""> {(string.IsNullOrEmpty(base64Image) ? "" : $"<img class='photo' src='{base64Image}' alt='Photo' height='100px' widht='120px' />")}</div></td>
    </tr>
  </table>

  <table>
    <tr>
      <td>Weight Category</td>
      <td><input type=""text"" class=""form-control"" name=""weight_category"" value=""{Convert.ToString(argWeightCat)}""></td>
      <td>Weight</td>
      <td><input type=""text"" class=""form-control"" name=""weight""></td>
    </tr>
    <tr>
      <td>Name (in capital letter)</td>
      <td colspan=""3""><input type=""text"" class=""form-control"" value=""{Convert.ToString(model.Name)}""></td>
    </tr>
    <tr>
      <td>Date of Birth</td>
      <td><input type=""date"" class=""form-control"" name=""dob"" value={Convert.ToString(model.Dob)}></td>
      <td>Age</td>
      <td><input type=""text"" class=""form-control"" name=""age""></td>
    </tr>
    <tr>
      <td>Parent / Guardian Name</td>
      <td colspan=""3""><input type=""text"" class=""form-control"" value=""{Convert.ToString(model.FatherName)}""></td>
    </tr>
    <tr>
      <td>Name of the School</td>
      <td colspan=""3""><input type=""text"" class=""form-control"" name=""school""></td>
    </tr>
    <tr>
      <td>Name of the Club</td>
      <td colspan=""3""><input type=""text"" class=""form-control"" name=""club"" value=""{Convert.ToString(model.ClubName)}"" ></td>
    </tr>
    <tr>
      <td>Address</td>
      <td colspan=""3""><textarea class=""form-control"" name=""address"">  {Convert.ToString(model.Address)} </textarea></td>
    </tr>
    <tr>
      <td>Present Belt Grade</td>
      <td><input type=""text"" class=""form-control"" name=""belt_grade""></td>
      <td>TFI.I.C. No.</td>
      <td><input type=""text"" class=""form-control"" name=""tfi_lic_no""></td>
    </tr>
  </table>

  <p class=""fst-italic mt-3"">Copy of Corporation / Municipal / School Date of Birth Certificate should be enclosed compulsorily. (Original Birth Certificate should be produced at the time of Weigh-in).</p>

  <h6><strong>DECLARATION</strong></h6>
  <p>I, the undersigned do hereby solemnly affirm, declare and confirm for myself, my heirs, executors & administrators that I indemnify the Promoters / Organiser / Sponsors & its Members, Officials, Participants etc., holding myself personally responsible for all damages, injuries or accidents, claims, demands etc., waiving all prerogative rights, whatsoever related to the above set forth event.</p>

  <table>
    <tr>
      <td>Signature of Parent / Guardian:</td>
      <td><input type=""text"" class=""form-control"" name=""guardian_signature""></td>
      <td>Signature of Participant:</td>
      <td><input type=""text"" class=""form-control"" name=""participant_signature""></td>
    </tr>
  </table>

  <p class=""text-center mt-5"">
    Signature of President / Secretary<br>
    District Club / Organization / Head of the Institution with Seal
  </p>
</div>
</body>
</html>";

            //var Renderer = new IronPdf.HtmlToPdf();
            //var pdf = Renderer.RenderHtmlAsPdf(htmlContent);
            //return pdf.BinaryData;

            //var doc = new HtmlToPdfDocument
            //{
            //    GlobalSettings = new GlobalSettings
            //    {
            //        ColorMode = ColorMode.Color,
            //        Orientation = Orientation.Portrait,
            //        PaperSize = PaperKind.A4
            //    },
            //    Objects = {
            //    new ObjectSettings
            //    {
            //        PagesCount = true,
            //        HtmlContent = htmlContent,
            //        WebSettings = { DefaultEncoding = "utf-8" }
            //    }
            //    }
            //};

          //  return _converter.Convert(doc);

            // Step 1: Read the HTML content
            string htmlBody = htmlContent;

            // Step 2: Prepare the email
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("tournamentfixtures@gmail.com");
            mail.To.Add("gopinathbajaj@gmail.com");
            mail.Subject = "Your Invoice Page";
            mail.Body = htmlBody;
            mail.IsBodyHtml = true; // Important to enable HTML rendering
          

            // Step 3: SMTP settings
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential("tournamentfixtures@gmail.com", "mhsk dylu ohun cqtc");
            smtp.EnableSsl = true;

            // Step 4: Send
            smtp.Send(mail);

            return "Siuccess";

        }

        private async Task SendEmailAsync(string toEmail, PlayerViewModel model, string tournamentName)

        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = int.Parse(_config["EmailSettings:Port"]);
            var fromEmail = _config["EmailSettings:FromEmail"];
            var fromPassword = _config["EmailSettings:FromPassword"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = $"Registration Successful - Online Entry " + model.Name,
                Body = $"Thank you for registering for {tournamentName}. Your Online Entry is successfully created.<br /><br />" +
                $"Register Name : {model.Name} <br /> " +
                $"Father Name   : {model.FatherName} <br />" +
                $"Gender        : {model.Gender} <br />" +
                $"Date Of Birth : {model.Dob} <br />" +
                $"Weigt Category : {model.CategoryName} <br />" +
                $"Local Club Name : {model.ClubName}<br />" +
                $"Email ID        : {model.Email}<br />" +
                $"Mobile No       : {model.MobileNo}<br />" +
                $"Registration Reference : {model.Remarks}<br />",
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            //using var stream = new MemoryStream(pdfBytes);
            //stream.Position = 0;
            //var attachment = new Attachment(stream, "OnlineRegistration_" + model.Name + ".pdf", "application/pdf");
            //mailMessage.Attachments.Add(attachment);


            await smtpClient.SendMailAsync(mailMessage);

        }
    }
}
