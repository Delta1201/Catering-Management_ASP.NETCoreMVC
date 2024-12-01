using CateringManagement.CustomControllers;
using CateringManagement.Data;
using CateringManagement.Utilities;
using CateringManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Runtime.Intrinsics.X86;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CateringManagement.Controllers
{

    public class ReportController : CognizantController
    {
        private readonly CateringContext _context;

        public ReportController(CateringContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> FunctionRevenue(int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Function Type")
        {
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Function Type", "Total Number", "Avg. Per Person Charge", 
                "Avg. Guar. No.",  "Total Value", "Avg. Value", "Highest Value"};

            var sumQ =_context.FunctionRevenueSummary
                .AsNoTracking();

            //Before we sort, see if we have called for a different sort
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "Total Number")
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderBy(p => p.TotalNumber);
                }
                else
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.TotalNumber);
                }
            }
            else if (sortField == "Avg. Per Person Charge")
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.AveragePPCharge);
                }
                else
                {
                    sumQ = sumQ
                        .OrderBy(p => p.AveragePPCharge);
                }
            }
            else if (sortField == "Avg. Guar. No.")
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderBy(p => p.AverageGuarNo);
                }
                else
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.AverageGuarNo);
                }
            }
            else if (sortField == "Total Value")
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderBy(p => p.TotalValue);
                }
                else
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.TotalValue);
                }
            }
            else if (sortField == "Avg. Value")
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderBy(p => p.AvgValue);
                }
                else
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.AvgValue);
                }
            }
            else if (sortField == "Highest Value")
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderBy(p => p.MaxValue);
                }
                else
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.MaxValue);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    sumQ = sumQ
                        .OrderBy(p => p.Name);
                }
                else
                {
                    sumQ = sumQ
                        .OrderByDescending(p => p.Name);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;


            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "FunctionRevenue");//Remember for this View
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<FunctionRevenueVM>.CreateAsync(sumQ.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        [Authorize(Roles = "Admin, Supervisor")]
        public IActionResult DownloadFunctionRevenue()
        {
            //Get the data from the View
            var sumQ = _context.FunctionRevenueSummary
                .OrderBy(r=>r.Name)
                .Select(r=> new
                {
                    Function_Type=r.Name,
                    Total_Number=r.TotalNumber,
                    Average_Per_Person_Charge=r.AveragePPCharge,
                    Average_Guaranteed_Number=r.AverageGuarNo,
                    Total_Value=r.TotalValue,
                    Average_Value=r.AvgValue,
                    Highest_Value=r.MaxValue
                })
                .AsNoTracking();

            //How many rows?
            int numRows = sumQ.Count();

            if (numRows > 0) //We have data
            {
                //Create a new spreadsheet from scratch.
                using (ExcelPackage excel = new ExcelPackage())
                {

                    var workSheet = excel.Workbook.Worksheets.Add("Revenue by Type");

                    //Note: Cells[row, column]
                    workSheet.Cells[3, 1].LoadFromCollection(sumQ, true);

                    //Style second column for Total
                    workSheet.Column(2).Style.Numberformat.Format = "###,##0";

                    //Style columns for currency
                    workSheet.Column(3).Style.Numberformat.Format = "###,##0.00";
                    workSheet.Column(5).Style.Numberformat.Format = "###,##0.00";
                    workSheet.Column(6).Style.Numberformat.Format = "###,##0.00";
                    workSheet.Column(7).Style.Numberformat.Format = "###,##0.00";

                    //Style Avg. Guar. No.
                    workSheet.Column(4).Style.Numberformat.Format = "###,##0.0";

                    //Note: You can define a BLOCK of cells: Cells[startRow, startColumn, endRow, endColumn]
                    //Make Date and Patient Bold
                    workSheet.Cells[4, 1, numRows + 3, 1].Style.Font.Bold = true;

                    //Total Estimated Value of all Functions
                    using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 5])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[4, 5].Address + ":" + workSheet.Cells[numRows + 3, 5].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "$###,##0.00";
                    }
                    //Total number of Functions
                    using (ExcelRange totalfees = workSheet.Cells[numRows + 4, 2])//
                    {
                        totalfees.Formula = "Sum(" + workSheet.Cells[4, 2].Address + ":" + workSheet.Cells[numRows + 3, 2].Address + ")";
                        totalfees.Style.Font.Bold = true;
                        totalfees.Style.Numberformat.Format = "###,##0";
                    }

                    //Set Style and backgound colour of headings
                    using (ExcelRange headings = workSheet.Cells[3, 1, 3, 7])
                    {
                        headings.Style.Font.Bold = true;
                        var fill = headings.Style.Fill;
                        fill.PatternType = ExcelFillStyle.Solid;
                        fill.BackgroundColor.SetColor(Color.LightBlue);
                    }


                    //Autofit columns
                    workSheet.Cells.AutoFitColumns();
                    //Note: You can manually set width of columns as well
                    //workSheet.Column(7).Width = 10;

                    //Add a title and timestamp at the top of the report
                    workSheet.Cells[1, 1].Value = "Revenue Report by Function Type";
                    using (ExcelRange Rng = workSheet.Cells[1, 1, 1, 7])
                    {
                        Rng.Merge = true; //Merge columns start and end range
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 18;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    //Since the time zone where the server is running can be different, adjust to 
                    //Local for us.
                    DateTime utcDate = DateTime.UtcNow;
                    TimeZoneInfo esTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    DateTime localDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, esTimeZone);
                    using (ExcelRange Rng = workSheet.Cells[2, 7])
                    {
                        Rng.Value = "Created: " + localDate.ToShortTimeString() + " on " +
                            localDate.ToShortDateString();
                        Rng.Style.Font.Bold = true; //Font should be bold
                        Rng.Style.Font.Size = 12;
                        Rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }

                    //Ok, time to download the Excel

                    try
                    {
                        Byte[] theData = excel.GetAsByteArray();
                        string filename = "RevenueByType.xlsx";
                        string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(theData, mimeType, filename);
                    }
                    catch (Exception)
                    {
                        return BadRequest("Could not build and download the file.");
                    }
                }
            }
            return NotFound("No data.");
        }

    }
}
