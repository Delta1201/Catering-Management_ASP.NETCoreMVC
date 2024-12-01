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
    public class MealTypeController : LookupsController
    {
        private readonly CateringContext _context;

        public MealTypeController(CateringContext context)
        {
            _context = context;
        }

        // GET: MealType
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: MealType/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MealType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] MealType mealType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(mealType);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            return View(mealType);
        }

        // GET: MealType/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MealTypes == null)
            {
                return NotFound();
            }

            var mealType = await _context.MealTypes.FindAsync(id);
            if (mealType == null)
            {
                return NotFound();
            }
            return View(mealType);
        }

        // POST: MealType/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the MealType to update
            var mealTypeToUpdate = await _context.MealTypes.FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (mealTypeToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<MealType>(mealTypeToUpdate, "",
                d => d.Name))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MealTypeExists(mealTypeToUpdate.ID))
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
            return View(mealTypeToUpdate);
        }

        // GET: MealType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MealTypes == null)
            {
                return NotFound();
            }

            var mealType = await _context.MealTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (mealType == null)
            {
                return NotFound();
            }

            return View(mealType);
        }

        // POST: MealType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MealTypes == null)
            {
                return Problem("There are no Function Types to delete.");
            }
            var mealType = await _context.MealTypes.FindAsync(id);
            try
            {
                if (mealType != null)
                {
                    _context.MealTypes.Remove(mealType);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Meal Type. Remember, you cannot delete a Meal Type that is used in the system.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(mealType);
        }

        private bool MealTypeExists(int id)
        {
          return _context.MealTypes.Any(e => e.ID == id);
        }
    }
}
