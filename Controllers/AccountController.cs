using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AppUser user)
        {
            if (ModelState.IsValid)
            {
                // Check if username or email already exists
                if (_context.AppUsers.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(user);
                }

                if (_context.AppUsers.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(user);
                }

                // Hash password (simple hash for demo - in production use proper hashing)
                user.Password = HashPassword(user.Password);
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                _context.AppUsers.Add(user);
                _context.SaveChanges();

                // Assign default role (User)
                var defaultRole = _context.Roles.FirstOrDefault(r => r.Name == "User");
                if (defaultRole != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = defaultRole.Id,
                        AssignedAt = DateTime.Now
                    };
                    _context.UserRoles.Add(userRole);
                    _context.SaveChanges();
                }

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View();
            }

            var hashedPassword = HashPassword(password);
            var user = _context.AppUsers
                .FirstOrDefault(u => u.Username == username && u.Password == hashedPassword && u.IsActive);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View();
            }

            // Update last login time
            user.LastLoginAt = DateTime.Now;
            _context.SaveChanges();

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

            // Get user's roles
            var roles = _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role!.Name)
                .ToList();

            HttpContext.Session.SetString("Roles", string.Join(",", roles));

            // Get user's menus
            var menus = _context.UserMenus
                .Include(um => um.Menu)
                .Where(um => um.UserId == user.Id && um.Menu!.IsActive)
                .Select(um => um.Menu!)
                .ToList();

            // Store menu IDs in session
            var menuIds = menus.Select(m => m.Id).ToList();
            HttpContext.Session.SetString("MenuIds", string.Join(",", menuIds));

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            // Simple hash for demo - in production use proper hashing like BCrypt
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
