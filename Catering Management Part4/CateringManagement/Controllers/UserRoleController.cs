using CateringManagement.CustomControllers;
using CateringManagement.Data;
using CateringManagement.Models;
using CateringManagement.Utilities;
using CateringManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CateringManagement.Controllers
{
    [Authorize(Roles = "Security")]
    public class UserRoleController : CognizantController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        ////for sending email
        //private readonly IMyEmailSender _emailSender;

        //public UserRoleController(ApplicationDbContext context, IMyEmailSender emailSender)
        //{
        //    _context = context;
        //    _emailSender = emailSender;
        //}

        public UserRoleController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: User
        [Authorize(Roles = "Security")]
        public async Task<IActionResult> Index()
        {
            var users = await (from u in _context.Users
                               .OrderBy(u => u.UserName)
                               select new UserVM
                               {
                                   ID = u.Id,
                                   UserName = u.UserName
                               }).ToListAsync();
            foreach (var u in users)
            {
                var _user = await _userManager.FindByIdAsync(u.ID);
                u.UserRoles = (List<string>)await _userManager.GetRolesAsync(_user);
                //Note: we needed the explicit cast above because GetRolesAsync() returns an IList<string>
            };
            return View(users);
        }
        // GET: Users/Edit/5
        [Authorize(Roles = "Security")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new BadRequestResult();
            }

            var _user = await _userManager.FindByIdAsync(id);//IdentityRole
            if (_user == null)
            {
                return NotFound();
            }
            UserVM user = new UserVM
            {
                ID = _user.Id,
                UserName = _user.UserName,
                UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
            };

            PopulateAssignedRoleData(user);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Security")]
        public async Task<IActionResult> Edit(string Id, string[] selectedRoles)
        {

            var _user = await _userManager.FindByIdAsync(Id);//IdentityRole
            UserVM user = new UserVM
            {
                ID = _user.Id,
                UserName = _user.UserName,
                UserRoles = (List<string>)await _userManager.GetRolesAsync(_user)
            };

            try
            {
                await UpdateUserRoles(selectedRoles, user);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty,
                                "Unable to save changes.");
            }
            PopulateAssignedRoleData(user);
            return View(user);
        }

        [Authorize(Roles = "Security")]
        private void PopulateAssignedRoleData(UserVM user)
        {//Prepare checkboxes for all Roles
            var allRoles = _context.Roles;
            var currentRoles = user.UserRoles;
            var viewModel = new List<RoleVM>();
            foreach (var r in allRoles)
            {
                viewModel.Add(new RoleVM
                {
                    RoleID = r.Id,
                    RoleName = r.Name,
                    Assigned = currentRoles.Contains(r.Name)
                });
            }
            ViewBag.Roles = viewModel;
        }

        [Authorize(Roles = "Security")]
        private async Task UpdateUserRoles(string[] selectedRoles, UserVM userToUpdate)
        {
            var UserRoles = userToUpdate.UserRoles;//Current roles use is in
            var _user = await _userManager.FindByIdAsync(userToUpdate.ID);//IdentityUser

            if (selectedRoles == null)
            {
                //No roles selected so just remove any currently assigned
                foreach (var r in UserRoles)
                {
                    await _userManager.RemoveFromRoleAsync(_user, r);
                }
            }
            else
            {
                //At least one role checked so loop through all the roles
                //and add or remove as required

                //We need to do this next line because foreach loops don't always work well
                //for data returned by EF when working async.  Pulling it into an IList<>
                //first means we can safely loop over the colleciton making async calls and avoid
                //the error 'New transaction is not allowed because there are other threads running in the session'
                IList<IdentityRole> allRoles = _context.Roles.ToList<IdentityRole>();

                foreach (var r in allRoles)
                {
                    if (selectedRoles.Contains(r.Name))
                    {
                        if (!UserRoles.Contains(r.Name))
                        {
                            await _userManager.AddToRoleAsync(_user, r.Name);
                        }
                    }
                    else
                    {
                        if (UserRoles.Contains(r.Name))
                        {
                            await _userManager.RemoveFromRoleAsync(_user, r.Name);
                        }
                    }
                }
            }
        }

        //// GET/POST: MedicalTrial/Notification/5
        //public async Task<IActionResult> Notification(int? id, string Subject, string emailContent)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    //User t = await _context.Users.FindAsync(id);

        //    ViewData["id"] = id;
        //    //ViewData["TrialName"] = t.TrialName;

        //    if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
        //    {
        //        ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
        //    }
        //    else
        //    {
        //        int folksCount = 0;
        //        try
        //        {
        //            //Send a Notice.
        //            List<EmailAddress> folks = (from p in _context.Users
        //                                        //where p.Id == id
        //                                        where p.Id.Contains(id.ToString())
        //                                        select new EmailAddress
        //                                        {
        //                                            Name = p.UserName,
        //                                            Address = p.Email
        //                                        }).ToList();
        //            folksCount = folks.Count;
        //            if (folksCount > 0)
        //            {
        //                var msg = new EmailMessage()
        //                {
        //                    ToAddresses = folks,
        //                    Subject = Subject,
        //                    Content = "<p>" + emailContent + "</p><p>Please access the <strong>Niagara College</strong> web site to review.</p>"

        //                };
        //                await _emailSender.SendToManyAsync(msg);
        //                ViewData["Message"] = "Message sent to " + folksCount + " Patient"
        //                    + ((folksCount == 1) ? "." : "s.");
        //            }
        //            else
        //            {
        //                ViewData["Message"] = "Message NOT sent!  No Patients in medical trial.";
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string errMsg = ex.GetBaseException().Message;
        //            ViewData["Message"] = "Error: Could not send email message to the " + folksCount + " Patient"
        //                + ((folksCount == 1) ? "" : "s") + " in the trial.";
        //        }
        //    }
        //    return View();
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _userManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
