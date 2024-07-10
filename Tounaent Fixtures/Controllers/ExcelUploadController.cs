using ClosedXML.Excel;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Tounaent_Fixtures.Controllers
{
    public class ExcelUploadController : Controller
    {
        private readonly IConverter _converter;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConverter _pdfConverter;

        public ExcelUploadController(IConverter pdfConverter)
        {
            _pdfConverter = pdfConverter;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length <= 0)
            {
                ViewBag.Error = "No file uploaded.";
                return View("Index");
            }

            string filePath = Path.Combine("C:\\Users\\xches\\Downloads", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            List<ExcelData> data = new List<ExcelData>();

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);

                    var headersRow = worksheet.Row(1);
                    var nameColumn = GetColumnIndex(headersRow, "Student Name");
                    var teamColumn = GetColumnIndex(headersRow, "Team");
                    var heightColumn = GetColumnIndex(headersRow, "Height");
                    var weightColumn = GetColumnIndex(headersRow, "Weight");
                    var snoColumn = GetColumnIndex(headersRow, "S.No");
                    var dobColumn = GetColumnIndex(headersRow, "DOB");

                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        if (row.Cell(snoColumn)?.Value != null)
                        {
                            data.Add(new ExcelData
                            {
                                Sno = Convert.ToInt32(row.Cell(snoColumn)?.Value.ToString().Trim()),
                                StudentName = row.Cell(nameColumn)?.Value.ToString().Trim(),
                                Team = row.Cell(teamColumn)?.Value.ToString().Trim(),
                                Height = row.Cell(heightColumn)?.Value.ToString().Trim(),
                                Weight = Convert.ToInt32(row.Cell(weightColumn)?.Value.ToString().Trim()),
                                //DOB = DateTime.ParseExact(row.Cell(dobColumn)?.Value.ToString().Trim(), "dd-MM-yy", null)
                            });
                        }
                    }
                }

                var teamGroups = data.GroupBy(s => s.Team)
                                        .Select(g => g.ToList())
                                        .ToList();
                var shuffledStudents = new List<ExcelData>();
                while (teamGroups.Any(g => g.Count > 0))
                {
                    foreach (var group in teamGroups.ToList())
                    {
                        if (group.Count > 0)
                        {
                            shuffledStudents.Add(group.First());
                            group.RemoveAt(0); // Remove the student from the group

                            // Remove the group if it's empty
                            if (group.Count == 0)
                            {
                                teamGroups.Remove(group);
                            }
                        }
                    }
                }

                TempData["ViewName"] = GetViewName(shuffledStudents.Count);
                TempData["ShuffledStudents"] = JsonConvert.SerializeObject(shuffledStudents);

                return View("Data", data);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error reading Excel file: " + ex.Message;
                return View("Index");
            }
        }

        private int GetColumnIndex(IXLRow headersRow, string columnName)
        {
            foreach (var cell in headersRow.CellsUsed())
            {
                if (cell.Value.ToString().Trim().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return cell.Address.ColumnNumber;
                }
            }
            return -1;
        }
        [HttpPost]
        public IActionResult NavigateToView(string viewName)
        {
            if (!string.IsNullOrEmpty(viewName))
            {
                var shuffledStudentsJson = TempData["ShuffledStudents"] as string;
                var shuffledStudents = JsonConvert.DeserializeObject<List<ExcelData>>(shuffledStudentsJson);
                ViewBag.students=shuffledStudents;
                return View(viewName, shuffledStudents);
            }
            else
            {
                ViewBag.Error = "Error determining the view.";
                return View("Index");
            }
        }

        private string GetViewName(int count)
        {
            switch (count)
            {
                case 2:
                    return "2player";
                case 3:
                    return "3player";
                case 4:
                    return "4player";
                case 5:
                    return "5player";
                case 6:
                    return "6player";
                case 7:
                    return "7player";
                case 8:
                    return "8player";
                case 9:
                    return "9player";
                case 10:
                    return "10player";
                case 11:
                    return "11player";
                case 12:
                    return "12player";
                case 13:
                    return "13player";
                case 14:
                    return "14player";
                default:
                    return "Index";
            }
        }
        public IActionResult GeneratePdfFromHtml(string htmlContent)
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A2,
            };

            var objectSettings = new ObjectSettings
            {
                HtmlContent = htmlContent,
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _pdfConverter.Convert(pdf);

            return File(file, "application/pdf");
        }
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View {viewName} was not found.");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, (ITempDataProvider)TempData),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }


}