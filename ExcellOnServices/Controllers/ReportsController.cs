using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardReport
            {
                TotalServices = await _context.Services.CountAsync(),
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalEmployees = await _context.Employees.CountAsync(),
                TotalClients = await _context.Clients.CountAsync(),
                TotalPayments = await _context.Payments.SumAsync(p => p.Amount),
                ActiveClients = await _context.Clients.CountAsync(c => c.IsActive),
                ActiveServices = await _context.Services.CountAsync(s => s.IsActive)
            };

            // Recent payments
            model.RecentPayments = await _context.Payments
                .Include(p => p.Client)
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .ToListAsync();

            // Service usage stats
            var services = await _context.Services.ToListAsync();
            foreach (var service in services)
            {
                var clientCount = await _context.ClientServices
                    .CountAsync(cs => cs.ServiceId == service.Id && cs.IsActive);
                model.ServiceUsage.Add(service.Name, clientCount);
            }

            return View(model);
        }

        // GET: Reports/LatePayments
        public async Task<IActionResult> LatePayments()
        {
            // This is a placeholder - in a real app, you'd have invoice due dates
            // For now, show clients with no payments in the last 30 days
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            var clientsWithRecentPayments = await _context.Payments
                .Where(p => p.PaymentDate >= thirtyDaysAgo)
                .Select(p => p.ClientId)
                .Distinct()
                .ToListAsync();

            var lateClients = await _context.Clients
                .Where(c => c.IsActive && !clientsWithRecentPayments.Contains(c.Id))
                .ToListAsync();

            ViewBag.ReportDate = DateTime.Now.ToString("yyyy-MM-dd");
            return View(lateClients);
        }

        // GET: Reports/ServiceUsage
        public async Task<IActionResult> ServiceUsage()
        {
            var serviceUsage = await _context.ClientServices
                .Include(cs => cs.Service)
                .Include(cs => cs.Client)
                .Where(cs => cs.IsActive)
                .GroupBy(cs => cs.Service.Name)
                .Select(g => new ServiceUsageReport
                {
                    ServiceName = g.Key,
                    ClientCount = g.Count(),
                    TotalEmployees = g.Sum(cs => cs.NumberOfEmployees)
                })
                .OrderByDescending(r => r.ClientCount)
                .ToListAsync();

            return View(serviceUsage);
        }

        // GET: Reports/ClientSummary
        // GET: Reports/ClientSummary
        public async Task<IActionResult> ClientSummary()
        {
            // Simple version without ClientServices navigation
            var clients = await _context.Clients
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            // Get client services separately
            var clientServices = await _context.ClientServices
                .Include(cs => cs.Service)
                .ToListAsync();

            ViewBag.ClientServices = clientServices;

            return View(clients);
        }
    }

    // View Models for Reports
    public class DashboardReport
    {
        public int TotalServices { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalClients { get; set; }
        public decimal TotalPayments { get; set; }
        public int ActiveClients { get; set; }
        public int ActiveServices { get; set; }
        public List<Payment> RecentPayments { get; set; } = new List<Payment>();
        public Dictionary<string, int> ServiceUsage { get; set; } = new Dictionary<string, int>();
    }

    public class ServiceUsageReport
    {
        public string ServiceName { get; set; }
        public int ClientCount { get; set; }
        public int TotalEmployees { get; set; }
    }
}