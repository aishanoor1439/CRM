using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExcellOnServices.Models;

namespace ExcellOnServices.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSets for our models
        public DbSet<Service> Services { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientService> ClientServices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships

            // Employee -> Department
            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Employee -> Service
            builder.Entity<Employee>()
                .HasOne(e => e.Service)
                .WithMany()
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // ClientService -> Client
            builder.Entity<ClientService>()
                .HasOne(cs => cs.Client)
                .WithMany()
                .HasForeignKey(cs => cs.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ClientService -> Service
            builder.Entity<ClientService>()
                .HasOne(cs => cs.Service)
                .WithMany()
                .HasForeignKey(cs => cs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment -> Client
            builder.Entity<Payment>()
                .HasOne(p => p.Client)
                .WithMany()
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}