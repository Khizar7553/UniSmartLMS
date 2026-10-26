using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionD_People;

namespace UniSmartLMS.Controllers
{
    [Authorize(Roles = "Librarian")]
    public class LibrarianUserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public LibrarianUserController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: LibrarianUser/Dashboard
        public IActionResult Dashboard()
        {

            ViewBag.TotalStudents = _userManager.Users.Count(u => u.UserType == "Student");
            ViewBag.TotalFaculty = _userManager.Users.Count(u => u.UserType == "Faculty");
            ViewBag.TotalStaff = _userManager.Users.Count(u => u.UserType == "Staff");
            ViewBag.TotalVisitors = _userManager.Users.Count(u => u.UserType == "Visitor");
            return View();
        }

        // GET: LibrarianUser/UserManagement
        public async Task<IActionResult> UserManagement()
        {
            var users = _userManager.Users
                .Where(u => u.UserType == "Student" || u.UserType == "Faculty" || u.UserType == "Staff" || u.UserType == "Visitor")
                .ToList();

            foreach (var user in users)
            {
                if (user.PartyId.HasValue)
                {
                    user.Party = await _context.Parties.FindAsync(user.PartyId.Value);
                }
            }

            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(user.Id, roles);
            }

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        // GET: LibrarianUser/CreateUser
        // GET: LibrarianUser/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Student", Text = "Student" },
        new SelectListItem { Value = "Faculty", Text = "Faculty" },
        new SelectListItem { Value = "Staff", Text = "Staff" },
        new SelectListItem { Value = "Visitor", Text = "Visitor" }
    };
            ViewBag.RoleList = new SelectList(roles, "Value", "Text");
            await LoadDepartments(); // ✅ ADD THIS
            return View();
        }

        // POST: LibrarianUser/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password,
            string contact, string userType, string preferredLanguage, int departmentId, string registrationNumber = null)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "All fields are required");
                await LoadDepartments();
                return View();
            }

            // Restrict to allowed types
            if (userType != "Student" && userType != "Faculty" && userType != "Staff" && userType != "Visitor")
            {
                TempData["Error"] = "Librarian can only create Student, Faculty, Staff, and Visitor accounts.";
                return RedirectToAction(nameof(CreateUser));
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already registered");
                await LoadDepartments();
                return View();
            }

            try
            {
                // Create Party
                var party = new Party
                {
                    PartyName = fullName,
                    PartyContact = contact,
                    Email = email,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _context.Parties.Add(party);
                await _context.SaveChangesAsync();

                // Create User
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    PartyId = party.Id,
                    UserType = userType,
                    PreferredLanguage = string.IsNullOrEmpty(preferredLanguage) ? "English" : preferredLanguage,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, userType);

                    // Create Student record
                    if (userType == "Student" && !string.IsNullOrEmpty(registrationNumber))
                    {
                        var student = new Student
                        {
                            RegistrationNumber = registrationNumber,
                            PartyId = party.Id,
                            CandidateId = 0,
                            DegreeProgramId = departmentId > 0 ? departmentId : 1,
                            DegreeLevelId = 1,
                            Quota = "Open Merit",
                            TimingId = 1,
                            ProgramSessionId = 1,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                    }

                    // Create Faculty record
                    if (userType == "Faculty")
                    {
                        var faculty = new Employee
                        {
                            Name = fullName,
                            Contact = contact,
                            Code = $"FAC-{party.Id}",
                            JoiningDate = DateTime.Now,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };
                        _context.Employees.Add(faculty);
                        await _context.SaveChangesAsync();
                    }

                    // Create Library Member
                    var libraryMember = new LibraryMember
                    {
                        PartyId = party.Id,
                        LibraryBranchId = 1,
                        CardNo = $"LIB-{party.Id:D6}",
                        IssuedOn = DateTime.Now,
                        ExpiredOn = DateTime.Now.AddYears(1),
                        MembershipType = userType == "Student" ? 1 : userType == "Faculty" ? 2 : 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.LibraryMembers.Add(libraryMember);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"User {fullName} created successfully as {userType}!";
                    return RedirectToAction(nameof(UserManagement));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            await LoadDepartments();
            return View();
        }

        // Helper method
        private async Task LoadDepartments()
        {
            var departments = await _context.Departments.ToListAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name");
        }

        // GET: LibrarianUser/EditUser
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.PartyId.HasValue)
            {
                user.Party = await _context.Parties.FindAsync(user.PartyId.Value);
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault();

            var roleList = new List<SelectListItem>
            {
                new SelectListItem { Value = "Student", Text = "Student", Selected = ViewBag.CurrentRole == "Student" },
                new SelectListItem { Value = "Faculty", Text = "Faculty", Selected = ViewBag.CurrentRole == "Faculty" },
                new SelectListItem { Value = "Staff", Text = "Staff", Selected = ViewBag.CurrentRole == "Staff" },
                new SelectListItem { Value = "Visitor", Text = "Visitor", Selected = ViewBag.CurrentRole == "Visitor" }
            };
            ViewBag.RoleList = new SelectList(roleList, "Value", "Text");

            return View(user);
        }

        // POST: LibrarianUser/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string fullName, string contact, string userType, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FullName = fullName;
            user.UserType = userType;
            user.IsActive = isActive;

            if (user.PartyId.HasValue)
            {
                var party = await _context.Parties.FindAsync(user.PartyId.Value);
                if (party != null)
                {
                    party.PartyName = fullName;
                    party.PartyContact = contact;
                    _context.Update(party);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
                await _context.SaveChangesAsync();

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, userType);

                TempData["Success"] = "User updated successfully!";
                return RedirectToAction(nameof(UserManagement));
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }

        // POST: LibrarianUser/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete user";
                }
            }
            return RedirectToAction(nameof(UserManagement));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match";
                return RedirectToAction(nameof(UserManagement));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction(nameof(UserManagement));
            }

            // Librarian can only reset Student, Faculty, Staff, Visitor passwords
            if (user.UserType != "Student" && user.UserType != "Faculty" && user.UserType != "Staff" && user.UserType != "Visitor")
            {
                TempData["Error"] = "You don't have permission to reset this user's password";
                return RedirectToAction(nameof(UserManagement));
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
            {
                TempData["Error"] = "Password must be at least 4 characters";
                return RedirectToAction(nameof(UserManagement));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Password reset successfully for {user.Email}!";
            }
            else
            {
                TempData["Error"] = "Failed to reset password";
            }

            return RedirectToAction(nameof(UserManagement));
        }
    }
}