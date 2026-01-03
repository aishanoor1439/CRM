using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ExcellOnServices.Data;
using ExcellOnServices.Models;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Service)
                .ToListAsync();

            return View(employees);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name");
            ViewBag.Services = new SelectList(
                _context.Services.Where(s => s.IsActive),
                "Id",
                "Name"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            ModelState.Remove("Department");
            ModelState.Remove("Service");

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
                ViewBag.Services = new SelectList(_context.Services.Where(s => s.IsActive), "Id", "Name", employee.ServiceId);
                return View(employee);
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            ViewBag.Departments = new SelectList(
                _context.Departments,
                "Id",
                "Name",
                employee.DepartmentId
            );

            ViewBag.Services = new SelectList(
                _context.Services.Where(s => s.IsActive),
                "Id",
                "Name",
                employee.ServiceId
            );

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                        return NotFound();

                    throw;
                }
            }

            ViewBag.Departments = new SelectList(
                _context.Departments,
                "Id",
                "Name",
                employee.DepartmentId
            );

            ViewBag.Services = new SelectList(
                _context.Services.Where(s => s.IsActive),
                "Id",
                "Name",
                employee.ServiceId
            );

            return View(employee);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}