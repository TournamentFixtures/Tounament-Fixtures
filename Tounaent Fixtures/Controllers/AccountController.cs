using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using DinkToPdf;
using Tounaent_Fixtures.Models;
using Microsoft.EntityFrameworkCore;

public class AccountController : Controller
{
    private readonly IConfiguration _config;
    private readonly IConverter _converter;
    private readonly ApplicationDbContext _context;

    public AccountController(IConfiguration config, IConverter converter, ApplicationDbContext context)
    {
        _config = config;
        _converter = converter;
        _context = context;
    }

    public async Task<IActionResult> Register()
    {
        var model = new RegisterViewModel
        {
            GenderOptions = await GetGendersAsync()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.GenderOptions = await GetGendersAsync();

        if (!ModelState.IsValid)
            return View(model);

        byte[] photoBytes = null;
        if (model.Photo != null && model.Photo.Length > 0)
        {
            using var ms = new MemoryStream();
            await model.Photo.CopyToAsync(ms);
            photoBytes = ms.ToArray();
        }

        var registration = new Registration
        {
            Name = model.Name,
            GenderId = model.GenderId,
            Dob = model.DateOfBirth,
            Aadhaar = model.Aadhaar,
            Height = model.Height,
            Weight = model.Weight,
            Address = model.Address,
            PinCode = model.PinCode,
            Phone = model.Phone,
            Email = model.Email,
            Photo = photoBytes,
            CreatedDate = DateTime.Now
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        byte[] idCardPdf = GenerateIdCardPdf(model, photoBytes);
        await SendEmailAsync(model.Email, idCardPdf);

        TempData["Message"] = "Registration successful! A confirmation email with your ID card has been sent.";
        return RedirectToAction("Register");
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

private byte[] GenerateIdCardPdf(RegisterViewModel model, byte[] photoBytes)
    {
        string base64Image = photoBytes != null
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(photoBytes)}"
            : "";

        var htmlContent = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; text-align: center; }}
                .id-card {{
                    width: 300px; border: 1px solid #000; padding: 20px; margin: auto;
                }}
                .name {{ font-size: 18px; font-weight: bold; }}
                .photo {{
                    width: 100px; height: 100px; border: 1px solid #000; margin-bottom: 10px; object-fit: cover;
                }}
            </style>
        </head>
        <body>
            <div class='id-card'>
                <h2>ID Card</h2>
                {(string.IsNullOrEmpty(base64Image) ? "" : $"<img class='photo' src='{base64Image}' alt='Photo' />")}
                <p class='name'>{model.Name}</p>
                <p>Gender: {model.Gender}</p>
                <p>DOB: {model.DateOfBirth.ToShortDateString()}</p>
                <p>Aadhaar: {model.Aadhaar}</p>
                <p>Height: {model.Height}</p>
                <p>Weight: {model.Weight}</p>
                <p>Address: {model.Address}</p>
                <p>PinCode: {model.PinCode}</p>
                <p>Phone: {model.Phone}</p>
                <p>Email: {model.Email}</p>
                <p>Registration Date: {DateTime.Now.ToShortDateString()}</p>
            </div>
        </body>
        </html>";

        var doc = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        return _converter.Convert(doc);
    }

    private async Task SendEmailAsync(string toEmail, byte[] pdfBytes)
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
            Subject = "Registration Successful - ID Card",
            Body = "Thank you for registering! Your ID card is attached.",
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        using var stream = new MemoryStream(pdfBytes);
        stream.Position = 0;
        var attachment = new Attachment(stream, "ID_Card.pdf", "application/pdf");
        mailMessage.Attachments.Add(attachment);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
