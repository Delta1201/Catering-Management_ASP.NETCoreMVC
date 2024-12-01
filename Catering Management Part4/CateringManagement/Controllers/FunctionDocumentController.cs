using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CateringManagement.Data;
using CateringManagement.Models;
using CateringManagement.Utilities;
using System.Numerics;
using CateringManagement.CustomControllers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Microsoft.AspNetCore.Authorization;

namespace CateringManagement.Controllers
{
    [Authorize(Roles = "Admin, Supervisor, Staff")]

    public class FunctionDocumentController : ElephantController
    {
        private readonly CateringContext _context;

        public FunctionDocumentController(CateringContext context)
        {
            _context = context;
        }

        // GET: FunctionDocument
        public async Task<IActionResult> Index(string SearchString, int? FunctionID, string SearchFileName, int? page, int? pageSizeID)
        {
            //Supply SelectList for Functions
            var functions = _context.Functions.ToList();
            var functionList = functions
                .OrderBy(f => f.Summary)
                .ThenBy(f => f.StartDateSummary)
                .Select(f => new {
                    ID = f.ID,
                    Sum = f.Summary + " - " + f.StartDateSummary
                });

            ViewData["FunctionID"] = new SelectList(functionList, "ID", "Sum");

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            var functionDocuments = _context.FunctionDocuments
                .Include(f => f.Function)
                .AsNoTracking();

            //Add as many filters as needed
            if (FunctionID.HasValue)
            {
                functionDocuments = functionDocuments.Where(p => p.FunctionID == FunctionID);
                numberFilters++;
            }
            if (!System.String.IsNullOrEmpty(SearchFileName))
            {
                functionDocuments = functionDocuments.Where(p => p.FileName.ToUpper().Contains(SearchFileName.ToUpper()));
                numberFilters++;
            }
            if (!System.String.IsNullOrEmpty(SearchString))
            {
                functionDocuments = functionDocuments.Where(p=>p.Function.Name.ToUpper().Contains(SearchString.ToUpper())
                                       || p.Function.LobbySign.ToUpper().Contains(SearchString.ToUpper()));
                numberFilters++;
            }
            //Give feedback about the state of the filters
            if (numberFilters != 0)
            {
                //Toggle the Open/Closed state of the collapse depending on if we are filtering
                ViewData["Filtering"] = " btn-danger";
                //Show how many filters have been applied
                ViewData["numberFilters"] = "(" + numberFilters.ToString()
                    + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
                //Keep the Bootstrap collapse open
                //@ViewData["ShowFilter"] = " show";
            }
            // Always sort by FunctionDocument Name
            functionDocuments = functionDocuments
                        .OrderBy(p => p.FileName);

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<FunctionDocument>.CreateAsync(functionDocuments, page ?? 1, pageSize);
            return View(pagedData);
        }
 
        public async Task<FileContentResult> Download(int id)
        {
            var theFile = await _context.UploadedFiles
                .Include(d => d.FileContent)
                .Where(f => f.ID == id)
                .FirstOrDefaultAsync();
            return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
        }

        // GET: FunctionDocument/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FunctionDocuments == null)
            {
                return NotFound();
            }

            var functionDocument = await _context.FunctionDocuments
                .Include(d=>d.Function).FirstOrDefaultAsync(d=>d.ID==id);

            if (functionDocument == null)
            {
                return NotFound();
            }
            return View(functionDocument);
        }

        // POST: FunctionDocument/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var functionDocumentToUpdate = await _context.FunctionDocuments
                    .Include(d => d.Function).FirstOrDefaultAsync(d => d.ID == id);

            //Check that you got it or exit with a not found error
            if (functionDocumentToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<FunctionDocument>(functionDocumentToUpdate, "",
                    d => d.FileName, d => d.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FunctionDocumentExists(functionDocumentToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }

            }
            
            return View(functionDocumentToUpdate);
        }

        // GET: FunctionDocument/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FunctionDocuments == null)
            {
                return NotFound();
            }

            var functionDocument = await _context.FunctionDocuments
                .Include(f => f.Function)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (functionDocument == null)
            {
                return NotFound();
            }

            return View(functionDocument);
        }

        // POST: FunctionDocument/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FunctionDocuments == null)
            {
                return Problem("Entity set 'CateringContext.FunctionDocuments'  is null.");
            }
            var functionDocument = await _context.FunctionDocuments
                .Include(f => f.Function)
                .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (functionDocument != null)
                {
                    _context.FunctionDocuments.Remove(functionDocument);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(functionDocument);
        }

        private bool FunctionDocumentExists(int id)
        {
          return _context.FunctionDocuments.Any(e => e.ID == id);
        }
    }
}
