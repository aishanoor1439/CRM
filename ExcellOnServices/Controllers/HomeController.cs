using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ExcellOnServices.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // LANDING PAGE - PUBLIC
        [AllowAnonymous]
        public IActionResult Landing()
        {
            return View();
        }

        // DASHBOARD - REQUIRES LOGIN
        [Authorize]
        public IActionResult Index()
        {
            try
            {
                ViewBag.ServicesCount = _context.Services.Count();
                ViewBag.DepartmentsCount = _context.Departments.Count();
                ViewBag.EmployeesCount = _context.Employees.Count();
                ViewBag.ClientsCount = _context.Clients.Count();
                ViewBag.PaymentsCount = _context.Payments.Count();
                ViewBag.TotalPayments = _context.Payments.Sum(p => p.Amount);
            }
            catch (Exception ex)
            {
                ViewBag.ServicesCount = 0;
                ViewBag.DepartmentsCount = 0;
                ViewBag.EmployeesCount = 0;
                ViewBag.ClientsCount = 0;
                ViewBag.PaymentsCount = 0;
                ViewBag.TotalPayments = 0;
                _logger.LogError(ex, "Error loading dashboard counts");
            }

            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}