using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /UserManagement/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Check if user is admin
            var userId = HttpContext.Session.GetInt32("UserId");
            var roles = HttpContext.Session.GetString("Roles");
            
            if (userId == null || roles == null || !roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            var users = _context.AppUsers
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    Roles = u.UserRoles.Select(ur => ur.Role!.Name).ToList()
                })
                .ToList();

            return View(users);
        }

        // GET: /UserManagement/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roles = HttpContext.Session.GetString("Roles");
            
            if (userId == null || roles == null || !roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.AppUsers
                .Include(u => u.UserRoles)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                SelectedRoles = user.UserRoles.Select(ur => ur.RoleId).ToList()
            };

            ViewBag.AllRoles = _context.Roles.Where(r => r.IsActive).ToList();
            return View(model);
        }

        // POST: /UserManagement/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserEditViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roles = HttpContext.Session.GetString("Roles");
            
            if (userId == null || roles == null || !roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var user = _context.AppUsers.Find(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = model.Email;
                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.IsActive = model.IsActive;

                // Update roles
                var existingUserRoles = _context.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
                _context.UserRoles.RemoveRange(existingUserRoles);

                if (model.SelectedRoles != null)
                {
                    foreach (var roleId in model.SelectedRoles)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = roleId,
                            AssignedAt = DateTime.Now
                        });
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "User updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.AllRoles = _context.Roles.Where(r => r.IsActive).ToList();
            return View(model);
        }

        // GET: /UserManagement/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var roles = HttpContext.Session.GetString("Roles");
            
            if (userId == null || roles == null || !roles.Contains("Admin"))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.AppUsers.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.AppUsers.Remove(user);
            _context.SaveChanges();
            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction("Index");
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UserEditViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<int> SelectedRoles { get; set; } = new List<int>();
    }
}
