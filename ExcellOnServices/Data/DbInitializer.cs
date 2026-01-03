using ExcellOnServices.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace ExcellOnServices.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Look for any services
                if (context.Services.Any())
                {
                    return; // DB has been seeded
                }

                // Add Services
                var services = new Service[]
                {
                    new Service { Name = "In-bound", Description = "Receive calls from customers", DailyChargePerEmployee = 4500, IsActive = true },
                    new Service { Name = "Out-bound", Description = "Call customers for promotions", DailyChargePerEmployee = 6000, IsActive = true },
                    new Service { Name = "Tele Marketing", Description = "Sales and marketing calls", DailyChargePerEmployee = 5500, IsActive = true }
                };
                context.Services.AddRange(services);

                // Add Departments
                var departments = new Department[]
                {
                    new Department { Name = "HR Management", Description = "Human Resources Department" },
                    new Department { Name = "Administration", Description = "Administrative Operations" },
                    new Department { Name = "Service", Description = "Service Delivery Department" },
                    new Department { Name = "Training", Description = "Employee Training Department" },
                    new Department { Name = "Internet Security", Description = "IT Security and Support" },
                    new Department { Name = "Auditors", Description = "Auditing and Compliance" }
                };
                context.Departments.AddRange(departments);

                context.SaveChanges();
            }
        }
    }
}