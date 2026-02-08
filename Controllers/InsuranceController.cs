using InsuranceManagement.Web.Data;
using InsuranceManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Web.Controllers
{
    public class InsuranceController : Controller
    {
        // Dependency injection: ASP.NET Core automatically provides the DbContext.
        // We store it in this private field to use throughout the controller.
        private readonly ApplicationDbContext _context;

        public InsuranceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Insurance
        // Displays a list of all insurance policies.
        public async Task<IActionResult> Index()
        {
            // ToListAsync() fetches all records from the InsurancePolicies table.
            // "await" makes this non-blocking so the app stays responsive.
            return View(await _context.InsurancePolicies.ToListAsync());
        }

        // GET: /Insurance/Details/5
        // Shows detailed information about a specific policy.
        public async Task<IActionResult> Details(int id)
        {
            // FindAsync searches for a record by its primary key (Id).
            var policy = await _context.InsurancePolicies.FindAsync(id);

            // If no policy is found, return a 404 Not Found page.
            if (policy == null) return NotFound();

            return View(policy);
        }

        // GET: /Insurance/Create
        // Shows the form to create a new policy.
        public IActionResult Create()
        {
            // Just returns the empty form view.
            return View();
        }

        // POST: /Insurance/Create
        // Handles form submission when creating a new policy.
        [HttpPost]
        public async Task<IActionResult> Create(InsurancePolicy policy)
        {
            // ModelState.IsValid checks if all validation rules are satisfied.
            // If validation fails, send the user back to the form with error messages.
            if (!ModelState.IsValid) return View(policy);

            // Add the new policy to the database context.
            _context.Add(policy);

            // SaveChangesAsync() commits the changes to the database.
            // Without this, nothing gets saved.
            await _context.SaveChangesAsync();

            // After saving, redirect to the Index page to show the updated list.
            return RedirectToAction(nameof(Index));
        }

        // GET: /Insurance/Edit/5
        // Shows the form to edit an existing policy.
        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();

            // Pass the existing policy data to the view so the form is pre-filled.
            return View(policy);
        }

        // POST: /Insurance/Edit/5
        // Handles form submission when editing a policy.
        [HttpPost]
        public async Task<IActionResult> Edit(InsurancePolicy policy)
        {
            if (!ModelState.IsValid) return View(policy);

            // Update() tells EF Core that this record has been modified.
            _context.Update(policy);

            // Save the changes to the database.
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Insurance/Delete/5
        // Shows a confirmation page before deleting a policy.
        public async Task<IActionResult> Delete(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);
            if (policy == null) return NotFound();

            // Display the policy details so the user can confirm deletion.
            return View(policy);
        }

        // POST: /Insurance/Delete/5
        // Actually deletes the policy after user confirms.
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var policy = await _context.InsurancePolicies.FindAsync(id);

            // Check if the policy exists before trying to delete it.
            if (policy != null)
            {
                // Remove the policy from the database context.
                _context.InsurancePolicies.Remove(policy);

                // Commit the deletion to the database.
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}