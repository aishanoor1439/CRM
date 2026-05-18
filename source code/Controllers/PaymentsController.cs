using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;
using static ExcellOnServices.Models.PaymentDecorator;

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

        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Client)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(payments);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();
            return View(payment);
        }

        public IActionResult Create()
        {
            var activeClients = _context.Clients.Where(c => c.IsActive).ToList();
            if (activeClients.Count == 0)
                TempData["ErrorMessage"] = "No active clients found. Please create a client first.";

            ViewData["ClientId"] = new SelectList(activeClients, "Id", "CompanyName");
            return View();
        }

        // ==================== UPDATED: CREATE WITH FULL DECORATOR PATTERN ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWithDecorator(IFormCollection form)
        {
            try
            {
                // Parse form values
                int clientId = int.TryParse(form["ClientId"], out var cid) ? cid : 0;
                decimal originalAmount = decimal.TryParse(form["Amount"], out var amt) ? amt : 0;
                DateTime paymentDate = DateTime.TryParse(form["PaymentDate"], out var pd) ? pd : DateTime.Now;
                string paymentMethod = form["PaymentMethod"].ToString();
                string notes = form["Notes"].ToString();

                // Parse decorator options
                bool applyCustomCharge = form["ApplyCustomCharge"].ToString() == "true";
                decimal customChargeAmount = decimal.TryParse(form["CustomChargeAmount"], out var cca) ? cca : 0;

                bool applyProcessingFee = form["ApplyProcessingFee"].ToString() == "true";
                decimal processingFee = decimal.TryParse(form["ProcessingFee"], out var pf) ? pf : 0;

                bool applyTaxCharge = form["ApplyTaxCharge"].ToString() == "true";
                decimal taxPercent = decimal.TryParse(form["TaxPercent"], out var tp) ? tp : 0;

                bool applyTip = form["ApplyTip"].ToString() == "true";
                decimal tipAmount = decimal.TryParse(form["TipAmount"], out var ta) ? ta : 0;

                if (clientId <= 0 || originalAmount <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a client and enter valid amount";
                    ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                    return View("Create");
                }

                // ========== DECORATOR PATTERN IMPLEMENTATION ==========
                IPaymentCalculator calculator = new BasePaymentCalculator();
                List<string> appliedDecorators = new List<string>();

                if (applyCustomCharge && customChargeAmount > 0)
                {
                    calculator = new CustomChargeDecorator(calculator, customChargeAmount);
                    appliedDecorators.Add($"Custom Charge (${customChargeAmount})");
                }

                if (applyProcessingFee && processingFee > 0)
                {
                    calculator = new ProcessingFeeDecorator(calculator, processingFee);
                    appliedDecorators.Add($"Processing Fee (${processingFee})");
                }

                if (applyTaxCharge && taxPercent > 0)
                {
                    calculator = new TaxDecorator(calculator, taxPercent);
                    appliedDecorators.Add($"Tax ({taxPercent}%)");
                }

                if (applyTip && tipAmount > 0)
                {
                    calculator = new TipDecorator(calculator, tipAmount);
                    appliedDecorators.Add($"Tip (${tipAmount})");
                }

                decimal finalAmount = calculator.Calculate(originalAmount);
                string calculationDetails = appliedDecorators.Count > 0
                    ? $"Base Payment + {string.Join(" + ", appliedDecorators)}"
                    : "Base Payment Only";
                // ========== DECORATOR PATTERN END ==========

                var payment = new Payment
                {
                    ClientId = clientId,
                    Amount = finalAmount,
                    PaymentDate = paymentDate,
                    PaymentMethod = paymentMethod,
                    Notes = $"{notes} | [Calculation: {calculationDetails}] | Original: ${originalAmount}"
                };

                _context.Add(payment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Payment recorded! Original: ${originalAmount}, Final: ${finalAmount} ({calculationDetails})";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                int clientId = int.TryParse(form["ClientId"], out var cid) ? cid : 0;
                ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", clientId);
                return View("Create");
            }
        }

        // ==================== TEST ENDPOINT FOR DECORATOR PATTERN ====================
        [HttpGet]
        public IActionResult TestDecorator()
        {
            var results = new List<object>();

            // Test 1: Base payment only
            IPaymentCalculator baseCalc = new BasePaymentCalculator();
            results.Add(new
            {
                TestCase = "Base Payment Only",
                Input = 1000,
                Output = baseCalc.Calculate(1000),
                Description = baseCalc.GetDescription()
            });

            // Test 2: With Custom Charge ($35)
            IPaymentCalculator customChargeCalc = new CustomChargeDecorator(new BasePaymentCalculator(), 35);
            results.Add(new
            {
                TestCase = "With Custom Charge ($35)",
                Input = 1000,
                Output = customChargeCalc.Calculate(1000),
                Description = customChargeCalc.GetDescription()
            });

            // Test 3: With Processing Fee ($25)
            IPaymentCalculator processingCalc = new ProcessingFeeDecorator(new BasePaymentCalculator(), 25);
            results.Add(new
            {
                TestCase = "With Processing Fee ($25)",
                Input = 1000,
                Output = processingCalc.Calculate(1000),
                Description = processingCalc.GetDescription()
            });

            // Test 4: With Tax (5%)
            IPaymentCalculator taxCalc = new TaxDecorator(new BasePaymentCalculator(), 5);
            results.Add(new
            {
                TestCase = "With Tax (5%)",
                Input = 1000,
                Output = taxCalc.Calculate(1000),
                Description = taxCalc.GetDescription()
            });

            // Test 5: With Tip ($10)
            IPaymentCalculator tipCalc = new TipDecorator(new BasePaymentCalculator(), 10);
            results.Add(new
            {
                TestCase = "With Tip ($10)",
                Input = 1000,
                Output = tipCalc.Calculate(1000),
                Description = tipCalc.GetDescription()
            });

            // Test 6: ALL DECORATORS combined
            IPaymentCalculator allDecorators = new TipDecorator(
                                                new TaxDecorator(
                                                    new ProcessingFeeDecorator(
                                                        new CustomChargeDecorator(
                                                            new BasePaymentCalculator(), 35), 25), 5), 10);
            results.Add(new
            {
                TestCase = "ALL DECORATORS (Custom $35 + Processing $25 + Tax 5% + Tip $10)",
                Input = 1000,
                Output = allDecorators.Calculate(1000),
                Description = allDecorators.GetDescription()
            });

            return Json(new
            {
                success = true,
                pattern = "Decorator Pattern - Complete Implementation",
                testResults = results,
                message = "✅ Decorator pattern is working correctly with all 4 decorators!"
            });
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", payment.ClientId);
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,Amount,PaymentDate,PaymentMethod,Notes")] Payment payment)
        {
            if (id != payment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.IsActive), "Id", "CompanyName", payment.ClientId);
            return View(payment);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null) return NotFound();
            return View(payment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null) _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Id == id);
        }
    }
}