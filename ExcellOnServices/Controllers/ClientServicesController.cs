using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class ClientServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ClientServices
        public async Task<IActionResult> Index()
        {
            var clientServices = await _context.ClientServices
                .Include(cs => cs.Client)
                .Include(cs => cs.Service)
                .ToListAsync();
            return View(clientServices);
        }

        // GET: ClientServices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientService = await _context.ClientServices
                .Include(cs => cs.Client)
                .Include(cs => cs.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clientService == null)
            {
                return NotFound();
            }

            return View(clientService);
        }

        // GET: ClientServices/Create
        public IActionResult Create()
        {
            var activeClients = _context.Clients.Where(c => c.IsActive).ToList();
            var activeServices = _context.Services.Where(s => s.IsActive).ToList();
            
            Console.WriteLine($"Active Clients: {activeClients.Count}");
            Console.WriteLine($"Active Services: {activeServices.Count}");
            
            if (activeClients.Count == 0)
            {
                TempData["ErrorMessage"] = "No active clients found. Please create a client first.";
            }
            
            if (activeServices.Count == 0)
            {
                TempData["ErrorMessage"] = "No active services found. Please create a service first.";
            }
            
            ViewData["ClientId"] = new SelectList(activeClients, "Id", "CompanyName");
            ViewData["ServiceId"] = new SelectList(activeServices, "Id", "Name");
            return View();
        }

        // POST: ClientServices/Create - SIMPLIFIED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            try
            {
                Console.WriteLine("=== Starting ClientService Creation ===");
                
                // Parse form values manually
                int clientId = 0;
                int serviceId = 0;
                int numberOfEmployees = 1;
                
                if (int.TryParse(form["ClientId"], out var cid))
                {
                    clientId = cid;
                    Console.WriteLine($"ClientId: {clientId}");
                }
                
                if (int.TryParse(form["ServiceId"], out var sid))
                {
                    serviceId = sid;
                    Console.WriteLine($"ServiceId: {serviceId}");
                }
                
                // Validate required fields
                if (clientId <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a client";
                    ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName");
                    ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name");
                    return View();
                }
                
                if (serviceId <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a service";
                    ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                    ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", serviceId);
                    return View();
                }
                
                // Parse dates
                DateTime startDate = DateTime.Now;
                if (!string.IsNullOrEmpty(form["StartDate"]) && DateTime.TryParse(form["StartDate"], out var sd))
                {
                    startDate = sd;
                }
                
                DateTime? endDate = null;
                if (!string.IsNullOrEmpty(form["EndDate"]) && DateTime.TryParse(form["EndDate"], out var ed))
                {
                    endDate = ed;
                }
                
                // Parse number of employees
                if (int.TryParse(form["NumberOfEmployees"], out var noe) && noe >= 1)
                {
                    numberOfEmployees = noe;
                }
                
                // Parse IsActive - handle checkbox format
                bool isActive = false;
                string isActiveValue = form["IsActive"].ToString();
                if (isActiveValue == "true" || isActiveValue.Contains("true") || isActiveValue == "on")
                {
                    isActive = true;
                }
                
                Console.WriteLine($"Creating ClientService with:");
                Console.WriteLine($"  ClientId: {clientId}");
                Console.WriteLine($"  ServiceId: {serviceId}");
                Console.WriteLine($"  StartDate: {startDate}");
                Console.WriteLine($"  EndDate: {endDate}");
                Console.WriteLine($"  NumberOfEmployees: {numberOfEmployees}");
                Console.WriteLine($"  IsActive: {isActive}");
                
                // Create entity
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
                
                Console.WriteLine("=== ClientService Created Successfully ===");
                TempData["SuccessMessage"] = "Service assigned successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR ===");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                
                // Repopulate dropdowns
                int clientId = 0, serviceId = 0;
                int.TryParse(form["ClientId"], out clientId);
                int.TryParse(form["ServiceId"], out serviceId);
                
                ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", serviceId);
                return View();
            }
        }

        // GET: ClientServices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientService = await _context.ClientServices.FindAsync(id);
            if (clientService == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientService.ClientId);
            ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", clientService.ServiceId);
            return View(clientService);
        }

        // POST: ClientServices/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,ServiceId,StartDate,EndDate,NumberOfEmployees,IsActive")] ClientService clientService)
        {
            if (id != clientService.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clientService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientServiceExists(clientService.Id))
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
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientService.ClientId);
            ViewData["ServiceId"] = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", clientService.ServiceId);
            return View(clientService);
        }

        // GET: ClientServices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clientService = await _context.ClientServices
                .Include(cs => cs.Client)
                .Include(cs => cs.Service)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clientService == null)
            {
                return NotFound();
            }

            return View(clientService);
        }

        // POST: ClientServices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var clientService = await _context.ClientServices.FindAsync(id);
            if (clientService != null)
            {
                _context.ClientServices.Remove(clientService);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientServiceExists(int id)
        {
            return _context.ClientServices.Any(e => e.Id == id);
        }
    }
}