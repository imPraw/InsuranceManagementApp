using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Controllers
{
    public class InsuranceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InsuranceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.InsurancePolicies.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InsurancePolicy policy)
        {
            if (!ModelState.IsValid) return View(policy);

            _context.Add(policy);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InsurancePolicy policy)
        {
            if (!ModelState.IsValid) return View(policy);

            _context.Update(policy);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);

            if (policy != null)
            {
                _context.InsurancePolicies.Remove(policy);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
