using InsuranceManagement.Web.Models;

namespace InsuranceManagement.Web.Data
{
    public static class DbSeeder
    {
        public static void SeedData(ApplicationDbContext context)
        {
            // Seed Roles if they don't exist
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "Admin", Description = "Administrator with full access", CreatedAt = DateTime.Now, IsActive = true },
                    new Role { Name = "User", Description = "Regular user with limited access", CreatedAt = DateTime.Now, IsActive = true },
                    new Role { Name = "Manager", Description = "Manager with moderate access", CreatedAt = DateTime.Now, IsActive = true }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // Seed Menus if they don't exist
            if (!context.Menus.Any())
            {
                var menus = new List<Menu>
                {
                    // Main menus
                    new Menu { Name = "Dashboard", DisplayName = "Dashboard", Url = "/Dashboard", Icon = "bi-speedometer2", SortOrder = 1, CreatedAt = DateTime.Now, IsActive = true },
                    new Menu { Name = "Insurance", DisplayName = "Insurance Policies", Url = "/Insurance", Icon = "bi-file-earmark-text", SortOrder = 2, CreatedAt = DateTime.Now, IsActive = true },
                    new Menu { Name = "Claims", DisplayName = "Claims", Url = "/Insurance", Icon = "bi-clipboard-check", SortOrder = 3, CreatedAt = DateTime.Now, IsActive = true },
                    new Menu { Name = "Reports", DisplayName = "Reports", Url = "/Home", Icon = "bi-bar-chart", SortOrder = 4, CreatedAt = DateTime.Now, IsActive = true },
                    
                    // Admin menus
                    new Menu { Name = "Users", DisplayName = "User Management", Url = "/Home", Icon = "bi-people", SortOrder = 5, CreatedAt = DateTime.Now, IsActive = true },
                    new Menu { Name = "Roles", DisplayName = "Role Management", Url = "/Home", Icon = "bi-shield", SortOrder = 6, CreatedAt = DateTime.Now, IsActive = true },
                    new Menu { Name = "Settings", DisplayName = "Settings", Url = "/Home", Icon = "bi-gear", SortOrder = 7, CreatedAt = DateTime.Now, IsActive = true }
                };

                context.Menus.AddRange(menus);
                context.SaveChanges();
            }

            // Create admin user if not exists
            if (!context.AppUsers.Any(u => u.Username == "admin"))
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                var adminUser = new AppUser
                {
                    Username = "admin",
                    Password = HashPassword("admin123"),
                    Email = "admin@insurance.com",
                    FullName = "System Administrator",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.AppUsers.Add(adminUser);
                context.SaveChanges();

                // Assign admin role
                if (adminRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                        AssignedAt = DateTime.Now
                    };
                    context.UserRoles.Add(userRole);
                }

                // Assign all menus to admin
                var allMenus = context.Menus.ToList();
                foreach (var menu in allMenus)
                {
                    var userMenu = new UserMenu
                    {
                        UserId = adminUser.Id,
                        MenuId = menu.Id,
                        AssignedAt = DateTime.Now
                    };
                    context.UserMenus.Add(userMenu);
                }

                context.SaveChanges();
            }

            // Create regular user if not exists
            if (!context.AppUsers.Any(u => u.Username == "user"))
            {
                var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");
                var regularUser = new AppUser
                {
                    Username = "user",
                    Password = HashPassword("user123"),
                    Email = "user@insurance.com",
                    FullName = "Regular User",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.AppUsers.Add(regularUser);
                context.SaveChanges();

                // Assign user role
                if (userRole != null)
                {
                    var ur = new UserRole
                    {
                        UserId = regularUser.Id,
                        RoleId = userRole.Id,
                        AssignedAt = DateTime.Now
                    };
                    context.UserRoles.Add(ur);
                }

                // Assign basic menus to user
                var basicMenus = context.Menus.Where(m => m.Name == "Dashboard" || m.Name == "Insurance" || m.Name == "Claims").ToList();
                foreach (var menu in basicMenus)
                {
                    var userMenu = new UserMenu
                    {
                        UserId = regularUser.Id,
                        MenuId = menu.Id,
                        AssignedAt = DateTime.Now
                    };
                    context.UserMenus.Add(userMenu);
                }

                context.SaveChanges();
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
