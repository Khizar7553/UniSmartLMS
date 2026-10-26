using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using UniSmartLMS.Data;
using UniSmartLMS.Models;
using UniSmartLMS.Models.SectionA_University;
using UniSmartLMS.Models.SectionD_People;
using UniSmartLMS.Repositories.Interfaces;

namespace UniSmartLMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IGenericRepository<Department> _deptRepo;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IGenericRepository<Department> deptRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _deptRepo = deptRepo;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Your account is deactivated. Please contact administrator.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Redirect based on role
                if (roles.Contains("Admin")) return RedirectToAction("Index", "Dashboard");
                if (roles.Contains("Librarian")) return RedirectToAction("Index", "Dashboard");
                if (roles.Contains("Faculty")) return RedirectToAction("Index", "Dashboard");
                if (roles.Contains("Student")) return RedirectToAction("Index", "Dashboard");
                if (roles.Contains("Visitor")) return RedirectToAction("Index", "Dashboard");
                if (roles.Contains("Staff")) return RedirectToAction("Index", "Dashboard");

                return LocalRedirect(returnUrl ?? "/");
            }

            if (result.IsLockedOut)
            {
                ViewBag.Error = "Your account is locked out. Please try again later.";
            }
            else if (result.IsNotAllowed)
            {
                ViewBag.Error = "Login not allowed. Please confirm your email.";
            }
            else
            {
                ViewBag.Error = "Invalid email or password";
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var departments = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name");

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword,
    string contact, string userType, string registrationNumber, string preferredLanguage, int departmentId, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Validation
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "All fields are required");
                await LoadDepartments();
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                await LoadDepartments();
                return View();
            }

            if (password.Length < 4)
            {
                ModelState.AddModelError("", "Password must be at least 4 characters");
                await LoadDepartments();
                return View();
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already registered. Please login.");
                await LoadDepartments();
                return View();
            }

            try
            {
                // Create Party first
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

                // Then create user with PartyId
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
                    // Assign role
                    string role = userType == "Student" ? "Student" : "Faculty";

                    // Create role if it doesn't exist
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }

                    await _userManager.AddToRoleAsync(user, role);

                    // Create Student record if needed
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

                    // Create Faculty record if needed
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

                    // Create Library Member record
                    var libraryMember = new LibraryMember
                    {
                        PartyId = party.Id,
                        LibraryBranchId = 1, // Default branch
                        CardNo = $"LIB-{party.Id:D6}",
                        IssuedOn = DateTime.Now,
                        ExpiredOn = DateTime.Now.AddYears(1),
                        MembershipType = userType == "Student" ? 1 : 2,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    _context.LibraryMembers.Add(libraryMember);
                    await _context.SaveChangesAsync();

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["Success"] = "Registration successful! Welcome to UniSmartLMS.";

                    return RedirectToAction("Index", "Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
            }

            await LoadDepartments();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task LoadDepartments()
        {
            var departments = await _deptRepo.GetAllAsync();
            ViewBag.DepartmentList = new SelectList(departments, "Id", "Name");
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Load Party data
            if (user.PartyId.HasValue)
            {
                user.Party = await _context.Parties.FindAsync(user.PartyId.Value);
            }

            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.PartyId.HasValue)
            {
                user.Party = await _context.Parties.FindAsync(user.PartyId.Value);
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string fullName, string contact)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Update User
            user.FullName = fullName;
            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                // Update Party
                if (user.PartyId.HasValue)
                {
                    var party = await _context.Parties.FindAsync(user.PartyId.Value);
                    if (party != null)
                    {
                        party.PartyName = fullName;
                        party.PartyContact = contact;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Create Party if doesn't exist
                    var party = new Party
                    {
                        PartyName = fullName,
                        PartyContact = contact,
                        IsActive = true
                    };
                    _context.Parties.Add(party);
                    await _context.SaveChangesAsync();

                    user.PartyId = party.Id;
                    await _userManager.UpdateAsync(user);
                }

                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Message"] = "If an account exists with this email, a reset link has been sent.";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { email = email, token = token }, Request.Scheme);

            // In production, send email here. For now, show link in console.
            Console.WriteLine($"Reset Link: {resetLink}");
            TempData["Message"] = $"Password reset link: {resetLink} (Check console output)";

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string token, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password reset successfully! Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Failed to reset password";
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SetLanguage([FromBody] LanguageRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.PreferredLanguage = request.Language;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // ✅ Refresh the user's claims to update the navbar immediately
                    await _signInManager.RefreshSignInAsync(user);
                }
            }
            return Ok();
        }

        public class LanguageRequest { public string Language { get; set; } }
    }
}