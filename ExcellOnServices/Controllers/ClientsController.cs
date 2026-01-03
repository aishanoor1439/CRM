using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clients with search
        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["CurrentSort"] = sortOrder;

            // Set up sorting parameters
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";

            var clients = from c in _context.Clients
                          select c;

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                clients = clients.Where(c =>
                    c.CompanyName.Contains(searchString) ||
                    c.ContactPerson.Contains(searchString) ||
                    c.Email.Contains(searchString) ||
                    c.Phone.Contains(searchString) ||
                    c.Address.Contains(searchString));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "active";
                clients = clients.Where(c => c.IsActive == isActive);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    clients = clients.OrderByDescending(c => c.CompanyName);
                    break;
                case "date":
                    clients = clients.OrderBy(c => c.RegistrationDate);
                    break;
                case "date_desc":
                    clients = clients.OrderByDescending(c => c.RegistrationDate);
                    break;
                case "status":
                    clients = clients.OrderBy(c => c.IsActive);
                    break;
                case "status_desc":
                    clients = clients.OrderByDescending(c => c.IsActive);
                    break;
                default:
                    clients = clients.OrderBy(c => c.CompanyName);
                    break;
            }

            // Get counts for filter display
            ViewData["TotalCount"] = await _context.Clients.CountAsync();
            ViewData["ActiveCount"] = await _context.Clients.CountAsync(c => c.IsActive);
            ViewData["InactiveCount"] = await _context.Clients.CountAsync(c => !c.IsActive);

            return View(await clients.AsNoTracking().ToListAsync());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName,ContactPerson,Email,Phone,Address,RegistrationDate,IsActive")] Client client)
        {
            // Debug: Log model state
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                // Log all validation errors
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Key}");
                    foreach (var err in error.Errors)
                    {
                        Console.WriteLine($"  - {err}");
                    }
                }

                return View(client);
            }

            try
            {
                // Ensure RegistrationDate has a value
                if (client.RegistrationDate == default)
                {
                    client.RegistrationDate = DateTime.Now;
                }

                _context.Add(client);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Client created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error saving client: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"An error occurred while saving the client: {ex.Message}");
                return View(client);
            }
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,ContactPerson,Email,Phone,Address,RegistrationDate,IsActive")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}