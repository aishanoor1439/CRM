using ExcellOnServices.Data;
using ExcellOnServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ExcellOnServices.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private ApplicationDbContext _context;

        // FIXED: Using Singleton Pattern instead of Dependency Injection
        public DepartmentsController()
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
            return View(await _context.Departments.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var department = await _context.Departments.FirstOrDefaultAsync(m => m.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        public IActionResult Create()
        {
            EnsureContext();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CreatedDate")] Department department)
        {
            EnsureContext();
            if (ModelState.IsValid)
            {
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var department = await _context.Departments.FindAsync(id);
            if (department == null) return NotFound();
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedDate")] Department department)
        {
            EnsureContext();
            if (id != department.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            EnsureContext();
            if (id == null) return NotFound();

            var department = await _context.Departments.FirstOrDefaultAsync(m => m.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            EnsureContext();
            var department = await _context.Departments.FindAsync(id);
            if (department != null) _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(int id)
        {
            EnsureContext();
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}