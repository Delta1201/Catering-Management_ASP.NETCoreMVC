using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CateringManagement.Data;
using CateringManagement.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Function = CateringManagement.Models.Function;
using CateringManagement.CustomControllers;
using CateringManagement.Utilities;
using String = System.String;
using CateringManagement.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;

namespace CateringManagement.Controllers
{
    [Authorize]
    public class FunctionController : ElephantController
    {
        private readonly CateringContext _context;

        public FunctionController(CateringContext context)
        {
            _context = context;
        }

        // GET: Function
        public async Task<IActionResult> Index(string SearchString, int? FunctionTypeID, int? CustomerID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Function")
        {
            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Function", "Guar. No.", "Customer" };

            PopulateDropDownLists();

            var functions = _context.Functions
                .Include(f => f.Customer)
                .Include(f => f.FunctionType)
                .Include(f => f.MealType)
                .Include(f=>f.FunctionRooms).ThenInclude(fr=>fr.Room)
                .Include(f => f.FunctionDocuments)
                .AsNoTracking();

            //Add as many filters as needed
            if (CustomerID.HasValue)
            {
                functions = functions.Where(p => p.CustomerID == CustomerID);
                numberFilters++;
            }
            if (FunctionTypeID.HasValue)
            {
                functions = functions.Where(p => p.FunctionTypeID == FunctionTypeID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                functions = functions.Where(p => p.Customer.LastName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.Customer.FirstName.ToUpper().Contains(SearchString.ToUpper())
                                       || p.Name.ToUpper().Contains(SearchString.ToUpper())
                                       || p.LobbySign.ToUpper().Contains(SearchString.ToUpper()));
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

            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

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
            if (sortField == "Guar. No.")
            {
                if (sortDirection == "asc")
                {
                    functions = functions
                        .OrderBy(p => p.GuaranteedNumber);
                }
                else
                {
                    functions = functions
                        .OrderByDescending(p => p.GuaranteedNumber);
                }
            }
            else if (sortField == "Customer")
            {
                if (sortDirection == "asc")
                {
                    functions = functions
                        .OrderBy(p => p.Customer.LastName)
                        .ThenBy(p => p.Customer.FirstName);
                }
                else
                {
                    functions = functions
                        .OrderByDescending(p => p.Customer.LastName)
                        .ThenByDescending(p => p.Customer.FirstName);
                }
            }
            else //Sorting by Function Date
            {
                if (sortDirection == "asc")
                {
                    functions = functions
                        .OrderBy(p => p.StartTime);
                }
                else
                {
                    functions = functions
                        .OrderByDescending(p => p.StartTime);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Function>.CreateAsync(functions.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);

        }

        // GET: Function/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var function = await _context.Functions
                .Include(f => f.Customer)
                .Include(f => f.FunctionType)
                .Include(f => f.MealType)
                .Include(f => f.FunctionRooms).ThenInclude(fr => fr.Room)
                .Include(f => f.FunctionDocuments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (function == null)
            {
                return NotFound();
            }

            return View(function);
        }

        // GET: Functions/Create
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public IActionResult Create()
        {
            Function function = new Function();
            PopulateDropDownLists(function);
            PopulateAssignedRoomCheckboxes(function);
            return View(function);
        }

        // POST: Functions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor, Staff")]

        public async Task<IActionResult> Create([Bind("Name,LobbySign,StartTime,EndTime,SetupNotes,BaseCharge," +
            "PerPersonCharge,GuaranteedNumber,SOCAN,Deposit,Alcohol,DepositPaid,NoHST,NoGratuity," +
            "CustomerID,FunctionTypeID,MealTypeID")] Function function, string[] selectedOptions, List<IFormFile> theFiles)
        {
            try
            {
                //Add the selected conditions
                if (selectedOptions != null)
                {
                    foreach (var condition in selectedOptions)
                    {
                        var roomToAdd = new FunctionRoom { FunctionID = function.ID, RoomID = int.Parse(condition) };
                        function.FunctionRooms.Add(roomToAdd);
                    }
                }
                if (ModelState.IsValid)
                {
                    await AddDocumentsAsync(function, theFiles);
                    _context.Add(function);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { function.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            PopulateDropDownLists(function);
            PopulateAssignedRoomCheckboxes(function);
            return View(function);
        }

        // GET: Functions/Edit/5
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var function = await _context.Functions
                .Include(f => f.FunctionRooms).ThenInclude(fr => fr.Room)
                .Include(f=>f.FunctionDocuments)
                .FirstOrDefaultAsync(f => f.ID == id);

            if (function == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(function);
            PopulateAssignedRoomLists(function);
            return View(function);
        }

        // POST: Functions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor, Staff")]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion, 
            string[] selectedOptions, List<IFormFile> theFiles)
        {
            // Go get the function to update
            var functionToUpdate = await _context.Functions
                .Include(f => f.FunctionRooms).ThenInclude(fr => fr.Room)
                .Include(f => f.FunctionDocuments)
                .FirstOrDefaultAsync(f => f.ID == id);

            // Check that we got the function or exit with a not found error
            if (functionToUpdate == null)
            {
                return NotFound();
            }

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(functionToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            //Update the Function Rooms
            UpdateFunctionRoomsListboxes(selectedOptions, functionToUpdate);    

            if (await TryUpdateModelAsync<Function>(functionToUpdate, "",
                f => f.Name, f => f.LobbySign, f => f.StartTime, f => f.EndTime, f => f.SetupNotes,
                f => f.BaseCharge, f => f.PerPersonCharge, f => f.GuaranteedNumber,
                f => f.SOCAN, f => f.Deposit, f => f.DepositPaid, f => f.NoHST,
                f => f.NoGratuity, f => f.Alcohol, f => f.MealTypeID,
                f => f.CustomerID, f => f.FunctionTypeID))
            {
                try
                {
                    await AddDocumentsAsync(functionToUpdate, theFiles);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { functionToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException ex)// Added for concurrency
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Function)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("",
                            "Unable to save changes. The Function was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Function)databaseEntry.ToObject();
                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", "Current value: "
                                + databaseValues.Name);
                        if (databaseValues.LobbySign != clientValues.LobbySign)
                            ModelState.AddModelError("LobbySign", "Current value: "
                                + databaseValues.LobbySign);
                        if (databaseValues.SetupNotes != clientValues.SetupNotes)
                            ModelState.AddModelError("SetupNotes", "Current value: "
                                + databaseValues.SetupNotes);
                        if (databaseValues.StartTime != clientValues.StartTime)
                            ModelState.AddModelError("StartTime", "Current value: "
                                + String.Format("{0:F}", databaseValues.StartTime));
                        if (databaseValues.EndTime != clientValues.EndTime)
                            ModelState.AddModelError("EndTime", "Current value: "
                                + String.Format("{0:F}", databaseValues.EndTime));
                        if (databaseValues.BaseCharge != clientValues.BaseCharge)
                            ModelState.AddModelError("BaseCharge", "Current value: "
                                + String.Format("{0:c}", databaseValues.BaseCharge));
                        if (databaseValues.PerPersonCharge != clientValues.PerPersonCharge)
                            ModelState.AddModelError("PerPersonCharge", "Current value: "
                                + String.Format("{0:c}", databaseValues.PerPersonCharge));
                        if (databaseValues.GuaranteedNumber != clientValues.GuaranteedNumber)
                            ModelState.AddModelError("GuaranteedNumber", "Current value: "
                                + databaseValues.GuaranteedNumber);
                        if (databaseValues.SOCAN != clientValues.SOCAN)
                            ModelState.AddModelError("SOCAN", "Current value: "
                                + String.Format("{0:c}", databaseValues.SOCAN));
                        if (databaseValues.Deposit != clientValues.Deposit)
                            ModelState.AddModelError("Deposit", "Current value: "
                                + String.Format("{0:c}", databaseValues.Deposit));
                        if (databaseValues.Alcohol != clientValues.Alcohol)
                            ModelState.AddModelError("Alcohol", "Current value: "
                                + databaseValues.Alcohol.ToString());
                        if (databaseValues.DepositPaid != clientValues.DepositPaid)
                            ModelState.AddModelError("DepositPaid", "Current value: "
                                + databaseValues.DepositPaid.ToString());
                        if (databaseValues.NoHST != clientValues.NoHST)
                            ModelState.AddModelError("NoHST", "Current value: "
                                + databaseValues.NoHST.ToString());
                        if (databaseValues.NoGratuity != clientValues.NoGratuity)
                            ModelState.AddModelError("NoGratuity", "Current value: "
                                + databaseValues.NoGratuity.ToString());
                        //For the foreign key, we need to go to the database to get the information to show
                        if (databaseValues.CustomerID != clientValues.CustomerID)
                        {
                            Customer databaseCustomer = await _context.Customers.FirstOrDefaultAsync(i => i.ID == databaseValues.CustomerID);
                            ModelState.AddModelError("CustomerID", $"Current value: {databaseCustomer?.FullName}");
                        }
                        if (databaseValues.FunctionTypeID != clientValues.FunctionTypeID)
                        {
                            FunctionType databaseFunctionType = await _context.FunctionTypes.FirstOrDefaultAsync(i => i.ID == databaseValues.FunctionTypeID);
                            ModelState.AddModelError("FunctionTypeID", $"Current value: {databaseFunctionType?.Name}");
                        }
                        //A little extra work for the nullable foreign key.  No sense going to the database and asking for something
                        //we already know is not there.
                        if (databaseValues.MealTypeID != clientValues.MealTypeID)
                        {
                            if (databaseValues.MealTypeID.HasValue)
                            {
                                MealType databaseMealType = await _context.MealTypes.FirstOrDefaultAsync(i => i.ID == databaseValues.MealTypeID);
                                ModelState.AddModelError("MealTypeID", $"Current value: {databaseMealType?.Name}");
                            }
                            else

                            {
                                ModelState.AddModelError("MealTypeID", $"Current value: No Food Service");
                            }
                        }
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you received your values. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to save your version of this record, click "
                                + "the Save button again. Otherwise click the 'Back to Function List' hyperlink.");
                        functionToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateDropDownLists(functionToUpdate);
            PopulateAssignedRoomLists(functionToUpdate);
            return View(functionToUpdate);
        }

        // GET: Functions/Delete/5
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var function = await _context.Functions
                .Include(f => f.Customer)
                .Include(f => f.FunctionType)
                .Include(f => f.FunctionRooms).ThenInclude(fr => fr.Room)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (function == null)
            {
                return NotFound();
            }
            if (User.IsInRole("Supervisor"))
            {
                if (function.CreatedBy != User.Identity.Name)
                {
                    ModelState.AddModelError("", "As a Supervisor, you cannot delete this " +
                        "Function because you did not enter them into the system.");
                    ViewData["NoSubmit"] = "disabled=disabled";
                }
            }

            return View(function);
        }

        // POST: Functions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Functions == null)
            {
                return Problem("There are no Functions to delete.");
            }
            var function = await _context.Functions
                .Include(f => f.Customer)
                .Include(f => f.FunctionType)
                .Include(f => f.FunctionRooms).ThenInclude(fr => fr.Room)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (User.IsInRole("Supervisor"))
            {
                if (function.CreatedBy != User.Identity.Name)
                {
                    ModelState.AddModelError("", "As a Supervisor, you cannot delete this " +
                        "Function because you did not enter them into the system.");
                    ViewData["NoSubmit"] = "disabled=disabled";
                }
            }

            try
            {
                if (function != null)
                {
                    _context.Functions.Remove(function);
                }

                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException)
            {
                //Note: there is really no reason a delete should fail if you can "talk" to the database.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(function);
        }

        //Note that we will use checkbox approach for create.
        #region Maintain Checkboxes
        private void PopulateAssignedRoomCheckboxes(Function function)
        {
            //For this to work, you must have Included the FunctionRooms 
            //in the Function
            var allOptions = _context.Rooms;
            var currentOptionIDs = new HashSet<int>(function.FunctionRooms.Select(b => b.RoomID));
            var checkBoxes = new List<CheckOptionVM>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new CheckOptionVM
                {
                    ID = option.ID,
                    DisplayText = option.Summary,
                    Assigned = currentOptionIDs.Contains(option.ID)
                });
            }
            ViewData["RoomOptions"] = checkBoxes;
        }
        //Note: we are not using UpdateFunctionRoomCheckboxes becuase we don
        //not need it for Create and we are going to demo the Two Listbox approach
        //for Edit.  However, the code is still here for reference.
        private void UpdateFunctionRoomCheckboxes(string[] selectedOptions, Function functionToUpdate)
        {
            if (selectedOptions == null)
            {
                functionToUpdate.FunctionRooms = new List<FunctionRoom>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var functionOptionsHS = new HashSet<int>
                (functionToUpdate.FunctionRooms.Select(c => c.RoomID));//IDs of the currently selected conditions
            foreach (var option in _context.Rooms)
            {
                if (selectedOptionsHS.Contains(option.ID.ToString())) //It is checked
                {
                    if (!functionOptionsHS.Contains(option.ID))  //but not currently in the history
                    {
                        functionToUpdate.FunctionRooms.Add(new FunctionRoom { FunctionID = functionToUpdate.ID, RoomID = option.ID });
                    }
                }
                else
                {
                    //Checkbox Not checked
                    if (functionOptionsHS.Contains(option.ID)) //but it is currently in the history - so remove it
                    {
                        FunctionRoom conditionToRemove = functionToUpdate.FunctionRooms.SingleOrDefault(c => c.RoomID == option.ID);
                        _context.Remove(conditionToRemove);
                    }
                }
            }
        }
        #endregion

        //Note that we will use the two list box approach for edit.
        #region Maintain List Boxes
        private void PopulateAssignedRoomLists(Function function)
        {
            //For this to work, you must have Included the child collection in the parent object
            var allOptions = _context.Rooms;
            var currentOptionsHS = new HashSet<int>(function.FunctionRooms.Select(b => b.RoomID));
            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            foreach (var r in allOptions)
            {
                if (currentOptionsHS.Contains(r.ID))
                {
                    selected.Add(new ListOptionVM
                    {
                        ID = r.ID,
                        DisplayText = r.Summary
                    });
                }
                else
                {
                    available.Add(new ListOptionVM
                    {
                        ID = r.ID,
                        DisplayText = r.Summary
                    });
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }
        private void UpdateFunctionRoomsListboxes(string[] selectedOptions, Function functionToUpdate)
        {
            if (selectedOptions == null)
            {
                functionToUpdate.FunctionRooms = new List<FunctionRoom>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var currentOptionsHS = new HashSet<int>(functionToUpdate.FunctionRooms.Select(b => b.RoomID));
            foreach (var r in _context.Rooms)
            {
                if (selectedOptionsHS.Contains(r.ID.ToString()))//it is selected
                {
                    if (!currentOptionsHS.Contains(r.ID))//but not currently in the Function's collection - Add it!
                    {
                        functionToUpdate.FunctionRooms.Add(new FunctionRoom
                        {
                            RoomID = r.ID,
                            FunctionID = functionToUpdate.ID
                        });
                    }
                }
                else //not selected
                {
                    if (currentOptionsHS.Contains(r.ID))//but is currently in the Function's collection - Remove it!
                    {
                        FunctionRoom roomToRemove = functionToUpdate.FunctionRooms.FirstOrDefault(d => d.RoomID == r.ID);
                        _context.Remove(roomToRemove);
                    }
                }
            }
        }
        #endregion

        private async Task AddDocumentsAsync(Function function, List<IFormFile> theFiles)
        {
            foreach (var f in theFiles)
            {
                if (f != null)
                {
                    string mimeType = f.ContentType;
                    string fileName = Path.GetFileName(f.FileName);
                    long fileLength = f.Length;
                    //Note: you could filter for mime types if you only want to allow
                    //certain types of files.  I am allowing everything.
                    if (!(fileName == "" || fileLength == 0))//Looks like we have a file!!!
                    {
                        FunctionDocument d = new FunctionDocument();
                        using (var memoryStream = new MemoryStream())
                        {
                            await f.CopyToAsync(memoryStream);
                            d.FileContent.Content = memoryStream.ToArray();
                        }
                        d.MimeType = mimeType;
                        d.FileName = fileName;
                        function.FunctionDocuments.Add(d);
                    };
                }
            }
        }

        //This is a twist on the PopulateDropDownLists approach
        //  Create methods that return each SelectList separately
        //  and one method to put them all into ViewData.
        //This approach allows for AJAX requests to refresh
        //DDL Data at a later date.
        private SelectList CustomerSelectList(int? selectedId)
        {
            return new SelectList(_context.Customers
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName), "ID", "Summary", selectedId);
        }
        private SelectList FunctionTypeList(int? selectedId)
        {
            return new SelectList(_context
                .FunctionTypes
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private SelectList MealTypeList(int? selectedId)
        {
            return new SelectList(_context
                .MealTypes
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Function function = null)
        {
            ViewData["CustomerID"] = CustomerSelectList(function?.CustomerID);
            ViewData["FunctionTypeID"] = FunctionTypeList(function?.FunctionTypeID);
            ViewData["MealTypeID"] = MealTypeList(function?.MealTypeID);
        }
        private bool FunctionExists(int id)
        {
          return _context.Functions.Any(e => e.ID == id);
        }
    }
}
