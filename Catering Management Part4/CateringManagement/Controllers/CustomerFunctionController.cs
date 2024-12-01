using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CateringManagement.Data;
using CateringManagement.Models;
using Microsoft.AspNetCore.Authorization;
using CateringManagement.CustomControllers;
using CateringManagement.Utilities;

namespace CateringManagement.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]

    public class CustomerFunctionController : ElephantController
    {
        private readonly CateringContext _context;

        public CustomerFunctionController(CateringContext context)
        {
            _context = context;
        }

        // GET: CustomerFunction
        //public async Task<IActionResult> Index()
        //{
        //      return View(await _context.Customers.ToListAsync());
        //}

        // GET: CustomerFunction
        public async Task<IActionResult> Index(int? CustomerID, int? page, int? pageSizeID, int? FunctionReasonID, string actionButton,
            string SearchString, string sortDirection = "desc", string sortField = "Function")
        {
            //Get the URL with the last filter, sort and page parameters from THE PATIENTS Index View
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, "Customer");

            if (!CustomerID.HasValue)
            {
                //Go back to the proper return URL for the Customers controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            PopulateDropDownLists();

            //Count the number of filters applied - start by assuming no filters
            ViewData["Filtering"] = "btn-outline-secondary";
            int numberFilters = 0;
            //Then in each "test" for filtering, add to the count of Filters applied

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Function", "Appt. Reason", "Extra Fees" };

            var appts = from a in _context.Functions
                        .Include(a => a.FunctionType)
                        .Include(a => a.Customer)
                        .Include(a => a.FunctionDocuments)
                        where a.CustomerID == CustomerID.GetValueOrDefault()
                        select a;

            if (FunctionReasonID.HasValue)
            {
                appts = appts.Where(p => p.CustomerID == FunctionReasonID);
                numberFilters++;
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                appts = appts.Where(p => p.SetupNotes.ToUpper().Contains(SearchString.ToUpper()));
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
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted so lets sort!
            {
                page = 1;//Reset back to first page when sorting or filtering

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }
            //Now we know which field and direction to sort by.
            if (sortField == "Appt. Reason")
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderBy(p => p.FunctionRooms);
                }
                else
                {
                    appts = appts
                        .OrderByDescending(p => p.FunctionRooms);
                }
            }
            else if (sortField == "Extra Fees")
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderBy(p => p.EstimatedValue);
                }
                else
                {
                    appts = appts
                        .OrderByDescending(p => p.EstimatedValue);
                }
            }
            else //Function Date
            {
                if (sortDirection == "asc")
                {
                    appts = appts
                        .OrderByDescending(p => p.StartTime);
                }
                else
                {
                    appts = appts
                        .OrderBy(p => p.StartTime);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            //Now get the MASTER record, the customer, so it can be displayed at the top of the screen
            Customer customer = await _context.Functions
                //.Include(p => p.CompanyName)
                .Include(p => p.Customer)
                .Include(p => p.FunctionType)
                //.Include(p => p.Customer).ThenInclude(pc => pc.Condition)
                .Where(p => p.ID == CustomerID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            ViewBag.Customer = customer;

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var pagedData = await PaginatedList<Function>.CreateAsync(appts.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }


        // GET: CustomerFunction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Customers == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: CustomerFunction/Create
        // GET: CustomerAppointment/Add
        public IActionResult Add(int? CustomerID, string CustomerName)
        {
            if (!CustomerID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            ViewData["CustomerName"] = CustomerName;
            Function a = new Function()
            {
                CustomerID = CustomerID.GetValueOrDefault()
            };
            PopulateDropDownLists();
            return View(a);
        }


        // POST: CustomerFunction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("ID,FirstName,MiddleName,LastName,CompanyName,Phone,CustomerCode,EMail")] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            PopulateDropDownLists();
            ViewData["CustomerName"] = customer.FullName;
            return View(customer);
        }



        // GET: CustomerFunction/Edit/5
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var appointment = await _context.Functions
               .Include(a => a.FunctionType)
               .Include(a => a.Customer)
               .AsNoTracking()
               .FirstOrDefaultAsync(m => m.ID == id);
            if (appointment == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(appointment);
            return View(appointment);

        }


        // POST: CustomerFunction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("ID,FirstName,MiddleName,LastName,CompanyName,Phone,CustomerCode,EMail")] Customer customer)
        {
            var customerToUpdate = await _context.Functions
                .Include(a => a.FunctionType)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (customerToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Function>(customerToUpdate, "",
                a => a.StartTime, a => a.EndTime, a => a.SetupNotes, a => a.EstimatedValue,
                a => a.CustomerID, a => a.FunctionTypeID))
            {
                try
                {
                    _context.Update(customerToUpdate);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }

                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customerToUpdate.ID))
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
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }
            }
            PopulateDropDownLists(customerToUpdate);
            return View(customerToUpdate);
        }


        // GET: CustomerFunction/Delete/5
        public async Task<IActionResult> Remove(int? id)
        {
            if (id == null || _context.Functions == null)
            {
                return NotFound();
            }

            var appointment = await _context.Functions
                .Include(a => a.FunctionType)
                .Include(a => a.Customer)
                .Include(a => a.FunctionDocuments)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (appointment == null)
            {
                return NotFound();
            }
            return View(appointment);
        }


        // POST: CustomerFunction/Delete/5
        [HttpPost, ActionName("Remove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveConfirmed(int id)
        {
            var appointment = await _context.Functions
                .Include(a => a.FunctionType)
                .Include(a => a.Customer)
                .Include(a => a.FunctionTypeID)
                .FirstOrDefaultAsync(m => m.ID == id);

            try
            {
                _context.Functions.Remove(appointment);
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                    "persists see your system administrator.");
            }

            return View(appointment);
        }

    private SelectList FunctionReasonSelectList(int? id)
        {
            var dQuery = from d in _context.Functions
                         orderby d.FunctionType
                         select d;
            return new SelectList(dQuery, "ID", "ReasonName", id);
        }
        private SelectList FunctionSelectList(int? id)
        {
            var dQuery = from d in _context.Functions
                         orderby d.Customer.FirstName, d.Customer.LastName
                         select d;
            return new SelectList(dQuery, "ID", "FullName", id);
        }
        private void PopulateDropDownLists(Function function = null)
        {
            //ViewData["FunctionReasonID"] = FunctionReasonSelectList(function?.CustomerID);
            ViewData["FunctionID"] = FunctionSelectList(function?.CustomerID);
        }


        private bool CustomerExists(int id)
        {
          return _context.Customers.Any(e => e.ID == id);
        }
    }
}
