using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Client)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(payments);
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            var activeClients = _context.Clients.Where(c => c.IsActive).ToList();

            Console.WriteLine($"Active Clients: {activeClients.Count}");

            if (activeClients.Count == 0)
            {
                TempData["ErrorMessage"] = "No active clients found. Please create a client first.";
            }

            ViewData["ClientId"] = new SelectList(activeClients, "Id", "CompanyName");
            return View();
        }

        // POST: Payments/Create - ONLY ONE CREATE ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            try
            {
                Console.WriteLine("=== Starting Payment Creation ===");

                // Parse form values manually
                int clientId = 0;
                decimal amount = 0;
                DateTime paymentDate = DateTime.Now;
                string paymentMethod = "Bank Transfer";
                string notes = "";

                // Parse ClientId
                if (int.TryParse(form["ClientId"], out var cid))
                {
                    clientId = cid;
                    Console.WriteLine($"ClientId: {clientId}");
                }

                // Parse Amount
                if (decimal.TryParse(form["Amount"], out var amt))
                {
                    amount = amt;
                    Console.WriteLine($"Amount: {amount}");
                }

                // Parse PaymentDate
                if (!string.IsNullOrEmpty(form["PaymentDate"]) && DateTime.TryParse(form["PaymentDate"], out var pd))
                {
                    paymentDate = pd;
                    Console.WriteLine($"PaymentDate: {paymentDate}");
                }

                // Parse PaymentMethod
                if (!string.IsNullOrEmpty(form["PaymentMethod"]))
                {
                    paymentMethod = form["PaymentMethod"];
                    Console.WriteLine($"PaymentMethod: {paymentMethod}");
                }

                // Parse Notes
                if (!string.IsNullOrEmpty(form["Notes"]))
                {
                    notes = form["Notes"];
                    Console.WriteLine($"Notes: {notes}");
                }

                // Validate required fields
                if (clientId <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a client";
                    ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName");
                    return View();
                }

                if (amount <= 0)
                {
                    TempData["ErrorMessage"] = "Please enter a valid amount greater than 0";
                    ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                    return View();
                }

                if (string.IsNullOrEmpty(paymentMethod))
                {
                    TempData["ErrorMessage"] = "Please select a payment method";
                    ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                    return View();
                }

                Console.WriteLine($"Creating Payment with:");
                Console.WriteLine($"  ClientId: {clientId}");
                Console.WriteLine($"  Amount: {amount}");
                Console.WriteLine($"  PaymentDate: {paymentDate}");
                Console.WriteLine($"  PaymentMethod: {paymentMethod}");
                Console.WriteLine($"  Notes: {notes}");

                // Create entity
                var payment = new Payment
                {
                    ClientId = clientId,
                    Amount = amount,
                    PaymentDate = paymentDate,
                    PaymentMethod = paymentMethod,
                    Notes = notes
                };

                _context.Add(payment);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== Payment Created Successfully ===");
                TempData["SuccessMessage"] = "Payment recorded successfully!";
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

                // Repopulate dropdown
                int clientId = 0;
                int.TryParse(form["ClientId"], out clientId);

                ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                return View();
            }
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", payment.ClientId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,Amount,PaymentDate,PaymentMethod,Notes")] Payment payment)
        {
            if (id != payment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id))
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
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", payment.ClientId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}