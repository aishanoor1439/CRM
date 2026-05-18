using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private ApplicationDbContext _context;

        // FIXED: Using Singleton Pattern instead of Dependency Injection
        public ClientsController()
        {
            _context = DatabaseHandler.GetContext();
        }

        // Safety method to ensure context is valid
        private void EnsureContext()
        {
            try
            {
                var test = _context.Model;
            }
            catch (ObjectDisposedException)
            {
                _context = DatabaseHandler.GetContext();
            }
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder)
        {
            EnsureContext();
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["CurrentSort"] = sortOrder;

            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";

            var clients = from c in _context.Clients select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                clients = clients.Where(c =>
                    c.CompanyName.Contains(searchString) ||
                    c.ContactPerson.Contains(searchString) ||
                    c.Email.Contains(searchString) ||
                    c.Phone.Contains(searchString) ||
                    c.Address.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "active";
                clients = clients.Where(c => c.IsActive == isActive);
            }

            switch (sortOrder)
            {
                case "name_desc": clients = clients.OrderByDescending(c => c.CompanyName); break;
                case "date": clients = clients.OrderBy(c => c.RegistrationDate); break;
                case "date_desc": clients = clients.OrderByDescending(c => c.RegistrationDate); break;
                case "status": clients = clients.OrderBy(c => c.IsActive); break;
                case "status_desc": clients = clients.OrderByDescending(c => c.IsActive); break;
                default: clients = clients.OrderBy(c => c.CompanyName); break;
            }

            ViewData["TotalCount"] = await _context.Clients.CountAsync();
            ViewData["ActiveCount"] = await _context.Clients.CountAsync(c => c.IsActive);
            ViewData["InactiveCount"] = await _context.Clients.CountAsync(c => !c.IsActive);

            return View(await clients.AsNoTracking().ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }

        public IActionResult Create()
        {
            EnsureContext();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName,ContactPerson,Email,Phone,Address,RegistrationDate,IsActive")] Client client)
        {
            EnsureContext();
            if (!ModelState.IsValid) return View(client);

            try
            {
                if (client.RegistrationDate == default) client.RegistrationDate = DateTime.Now;
                _context.Add(client);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Client created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while saving the client: {ex.Message}");
                return View(client);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,ContactPerson,Email,Phone,Address,RegistrationDate,IsActive")] Client client)
        {
            EnsureContext();
            if (id != client.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            EnsureContext();
            var client = await _context.Clients.FindAsync(id);
            if (client != null) _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            EnsureContext();
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}