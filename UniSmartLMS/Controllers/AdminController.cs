using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;
using UniSmartLMS.Services.Implementations;
using UniSmartLMS.Services.Interfaces;
//using UniSmartLMS.Services.Interfaces;  // ✅ ADD THIS
//using UniSmartLMS.ViewModels;



namespace UniSmartLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;  // ✅ CORRECT

        public AdminController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
        IAuditService auditService)  // ✅ ADD THIS

        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _auditService = auditService;  // ✅ ADD THIS

        }


        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            ViewBag.TotalUsers = _userManager.Users.Count();
            ViewBag.TotalStudents = _userManager.Users.Count(u => u.UserType == "Student");
            ViewBag.TotalFaculty = _userManager.Users.Count(u => u.UserType == "Faculty");
            ViewBag.TotalStaff = _userManager.Users.Count(u => u.UserType == "Staff");        // ✅ ADD
            ViewBag.TotalLibrarians = _userManager.Users.Count(u => u.UserType == "Librarian");
            ViewBag.TotalAdmins = _userManager.Users.Count(u => u.UserType == "Admin");
            ViewBag.TotalVisitors = _userManager.Users.Count(u => u.UserType == "Visitor");    // ✅ ADD
            return View();
        }

        // GET: Admin/UserManagement
        public async Task<IActionResult> UserManagement()
        {
            var users = _userManager.Users.ToList();

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

        

        // GET: Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name");
            await LoadDepartments(); // ✅ ADD THIS
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password,
            string contact, string userType, string preferredLanguage, int departmentId, string registrationNumber = null)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "All fields are required");
                ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name");
                await LoadDepartments();
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already registered");
                ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name");
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

                    // Create role if doesn't exist
                    if (!await _roleManager.RoleExistsAsync(userType))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(userType));
                    }

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

            ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name");
            await LoadDepartments();
            return View();
        }

        // Helper method
        private async Task LoadDepartments()
        {
            var departments = await _context.Departments.ToListAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name");
        }

        // GET: Admin/EditUser/5
        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ✅ Load Party data explicitly
            if (user.PartyId.HasValue)
            {
                user.Party = await _context.Parties.FindAsync(user.PartyId.Value);
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault();
            ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name", ViewBag.CurrentRole);

            // ✅ Return SINGLE user, NOT a list
            return View(user);  // ← This must be single model, not list
        }

        // POST: Admin/EditUser
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

            ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name", userType);
            return View(user);
        }

        // POST: Admin/DeleteUser
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

        // ========================================
        // ASSIGN ROLE METHODS (NEW)
        // ========================================

        // GET: Admin/AssignRole
        // GET: Admin/AssignRole
        public async Task<IActionResult> AssignRole()
        {
            var users = _userManager.Users.ToList();
            var roles = await _roleManager.Roles.ToListAsync();

            // ✅ Load FullName and Party data for each user
            foreach (var user in users)
            {
                if (user.PartyId.HasValue)
                {
                    user.Party = await _context.Parties.FindAsync(user.PartyId.Value);
                }
            }

            // Get current role for each user
            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var rolesList = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = rolesList.FirstOrDefault() ?? "Not Assigned";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.RoleList = new SelectList(roles, "Name", "Name");
            return View(users);
        }

        // POST: Admin/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                TempData["Error"] = "Please select both user and role";
                return RedirectToAction(nameof(AssignRole));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction(nameof(AssignRole));
            }

            // Remove existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Assign new role
            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                // Also update UserType in AppUser
                user.UserType = roleName;
                await _userManager.UpdateAsync(user);

                TempData["Success"] = $"Role '{roleName}' assigned to {user.Email} successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to assign role";
            }

            return RedirectToAction(nameof(AssignRole));
        }

        // GET: Admin/SystemSettings
        public IActionResult SystemSettings()
        {
            ViewData["Title"] = "System Settings";
            return View();
        }

        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _auditService.GetAllAsync();
            if (logs == null || !logs.Any())
            {
                ViewBag.Message = "No audit logs found.";
                return View(new List<AuditLog>());
            }
            return View(logs);
        }
        // POST: Admin/ResetUserPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found";
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
                TempData["Error"] = $"Failed to reset password: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(UserManagement));
        }

    }
}