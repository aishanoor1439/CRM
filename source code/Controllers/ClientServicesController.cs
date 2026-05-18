using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ClientServicesController : Controller
    {
        private ApplicationDbContext _context;

        // FIXED: Using Singleton Pattern instead of Dependency Injection
        public ClientServicesController()
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

        public async Task<IActionResult> Index()
        {
            EnsureContext();
            var clientServices = await _context.ClientServices
                .Include(cs => cs.Client)
                .Include(cs => cs.Service)
                .ToListAsync();
            return View(clientServices);
        }

        public async Task<IActionResult> Details(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var clientService = await _context.ClientServices
                .Include(cs => cs.Client)
                .Include(cs => cs.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clientService == null) return NotFound();
            return View(clientService);
        }

        public IActionResult Create()
        {
            EnsureContext();
            var activeClients = _context.Clients.Where(c => c.IsActive).ToList();
            var activeServices = _context.Services.Where(s => s.IsActive).ToList();

            if (activeClients.Count == 0)
                TempData["ErrorMessage"] = "No active clients found. Please create a client first.";

            if (activeServices.Count == 0)
                TempData["ErrorMessage"] = "No active services found. Please create a service first.";

            ViewData["ClientId"] = new SelectList(activeClients, "Id", "CompanyName");
            ViewData["ServiceId"] = new SelectList(activeServices, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            EnsureContext();
            try
            {
                int clientId = 0, serviceId = 0, numberOfEmployees = 1;

                if (int.TryParse(form["ClientId"], out var cid)) clientId = cid;
                if (int.TryParse(form["ServiceId"], out var sid)) serviceId = sid;

                if (clientId <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a client";
                    RepopulateDropDowns();
                    return View();
                }

                if (serviceId <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a service";
                    RepopulateDropDowns(clientId, serviceId);
                    return View();
                }

                DateTime startDate = DateTime.Now;
                if (!string.IsNullOrEmpty(form["StartDate"]) && DateTime.TryParse(form["StartDate"], out var sd))
                    startDate = sd;

                DateTime? endDate = null;
                if (!string.IsNullOrEmpty(form["EndDate"]) && DateTime.TryParse(form["EndDate"], out var ed))
                    endDate = ed;

                if (int.TryParse(form["NumberOfEmployees"], out var noe) && noe >= 1)
                    numberOfEmployees = noe;

                bool isActive = form["IsActive"].ToString() == "true" ||
                               form["IsActive"].ToString().Contains("true") ||
                               form["IsActive"].ToString() == "on";

                var clientService = new ClientService
                {
                    ClientId = clientId,
                    ServiceId = serviceId,
                    StartDate = startDate,
                    EndDate = endDate,
                    NumberOfEmployees = numberOfEmployees,
                    IsActive = isActive
                };

                _context.Add(clientService);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Service assigned successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                int clientId = 0, serviceId = 0;
                int.TryParse(form["ClientId"], out clientId);
                int.TryParse(form["ServiceId"], out serviceId);
                RepopulateDropDowns(clientId, serviceId);
                return View();
            }
        }

        private void RepopulateDropDowns(int clientId = 0, int serviceId = 0)
        {
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
            ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", serviceId);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var clientService = await _context.ClientServices.FindAsync(id);
            if (clientService == null) return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientService.ClientId);
            ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", clientService.ServiceId);
            return View(clientService);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,ServiceId,StartDate,EndDate,NumberOfEmployees,IsActive")] ClientService clientService)
        {
            EnsureContext();
            if (id != clientService.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clientService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientServiceExists(clientService.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientService.ClientId);
            ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", clientService.ServiceId);
            return View(clientService);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var clientService = await _context.ClientServices
                .Include(cs => cs.Client)
                .Include(cs => cs.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clientService == null) return NotFound();
            return View(clientService);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            EnsureContext();
            var clientService = await _context.ClientServices.FindAsync(id);
            if (clientService != null) _context.ClientServices.Remove(clientService);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientServiceExists(int id)
        {
            EnsureContext();
            return _context.ClientServices.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableEmployeesCount(int serviceId)
        {
            EnsureContext();
            var availableEmployeesCount = await _context.Employees
                .Where(e => e.ServiceId == serviceId && e.IsActive)
                .CountAsync();
            return Json(new { count = availableEmployeesCount });
        }
    }
}