using Microsoft.AspNetCore.Identity;
using UniSmartLMS.Models;

namespace UniSmartLMS.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // Create roles
            string[] roles = { "Student", "Faculty", "Staff", "Librarian", "Admin", "Visitor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create Admin user
            var adminEmail = "admin@unismartlms.edu.pk";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    UserType = "Admin",
                    IsActive = true,
                    EmailConfirmed = true,      // ✅ Add this
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    // Debug: Log errors if any
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin: {error.Description}");
                    }
                }
            }

            // Optional: Create a test Librarian
            var librarianEmail = "librarian@unismartlms.edu.pk";
            if (await userManager.FindByEmailAsync(librarianEmail) == null)
            {
                var librarian = new AppUser
                {
                    UserName = librarianEmail,
                    Email = librarianEmail,
                    FullName = "Library Staff",
                    UserType = "Librarian",
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(librarian, "Librarian@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(librarian, "Librarian");
                }
            }
        }
    }
}