using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CateringManagement.Data;
using CateringManagement.Models;
using CateringManagement.CustomControllers;

namespace CateringManagement.Controllers
{
    public class FunctionTypeController : LookupsController
    {
        private readonly CateringContext _context;

        public FunctionTypeController(CateringContext context)
        {
            _context = context;
        }

        // GET: FunctionTypes
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: FunctionTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FunctionTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] FunctionType functionType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(functionType);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(functionType);
        }

        // GET: FunctionTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FunctionTypes == null)
            {
                return NotFound();
            }

            var functionType = await _context.FunctionTypes.FindAsync(id);
            if (functionType == null)
            {
                return NotFound();
            }
            return View(functionType);
        }

        // POST: FunctionTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the function type to update
            var functionTypeToUpdate = await _context.FunctionTypes.FirstOrDefaultAsync(ft => ft.ID == id);

            //Check that we got the function type or exit with a not found error
            if (functionTypeToUpdate == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<FunctionType>(functionTypeToUpdate, "",
                    ft => ft.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FunctionTypeExists(functionTypeToUpdate.ID))
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
            return View(functionTypeToUpdate);
        }

        // GET: FunctionTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FunctionTypes == null)
            {
                return NotFound();
            }

            var functionType = await _context.FunctionTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (functionType == null)
            {
                return NotFound();
            }

            return View(functionType);
        }

        // POST: FunctionTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FunctionTypes == null)
            {
                return Problem("There are no Function Types to delete.");
            }
            var functionType = await _context.FunctionTypes.FindAsync(id);
            try
            {
                if (functionType != null)
                {
                    _context.FunctionTypes.Remove(functionType);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Function Type. Remember, you cannot delete a Function Type that is used in the system.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(functionType);

        }

        private bool FunctionTypeExists(int id)
        {
          return _context.FunctionTypes.Any(e => e.ID == id);
        }
    }
}
