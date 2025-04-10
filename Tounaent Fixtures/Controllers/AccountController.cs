using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly IConfiguration _config;
    private readonly IConverter _converter;

    public AccountController(IConfiguration config, IConverter converter)
    {
        _config = config;
        _converter = converter;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        byte[] photoBytes = null;

        if (model.Photo != null && model.Photo.Length > 0)
        {
            using (var ms = new MemoryStream())
            {
                await model.Photo.CopyToAsync(ms);
                photoBytes = ms.ToArray(); // Now you can use this to embed in the ID card
            }
        }



        // Generate ID Card as a PDF
        byte[] idCardPdf = GenerateIdCardPdf(model, photoBytes);

        // Send email with ID Card attached
        await SendEmailAsync(model.Email, idCardPdf);
        TempData["Message"] = "Registration successful! A confirmation email with your ID card has been sent.";


        return RedirectToAction("Register");
    }

    private byte[] GenerateIdCardPdf(RegisterViewModel model, byte[] photoBytes)
    {
        string base64Image = photoBytes != null
            ? $"data:image/jpeg;base64,{Convert.ToBase64String(photoBytes)}"
            : ""; // fallback if no image uploaded

        var htmlContent = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; text-align: center; }}
            .id-card {{
                width: 300px;
                border: 1px solid #000;
                padding: 20px;
                margin: auto;
            }}
            .name {{ font-size: 18px; font-weight: bold; }}
            .photo {{
                width: 100px;
                height: 100px;
                border: 1px solid #000;
                margin-bottom: 10px;
                object-fit: cover;
            }}
        </style>
    </head>
    <body>
        <div class='id-card'>
            <h2>ID Card</h2>
            {(string.IsNullOrEmpty(base64Image) ? "" : $"<img class='photo' src='{base64Image}' alt='Photo' />")}
            <p class='name'>{model.Name}</p>
            <p>Gender: {model.Gender}</p>
            <p>Age: {model.Age}</p>
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
            }
        };

        doc.Objects.Add(new ObjectSettings
        {
            PagesCount = true,
            HtmlContent = htmlContent,
            WebSettings = { DefaultEncoding = "utf-8" }
        });

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

        // Convert PDF to MemoryStream (IMPORTANT FIX)
        using (var stream = new MemoryStream(pdfBytes))
        {
            stream.Position = 0; // Reset stream position
            var attachment = new Attachment(stream, "ID_Card.pdf", "application/pdf");
            mailMessage.Attachments.Add(attachment);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

}
