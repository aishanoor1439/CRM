using System;
using System.ComponentModel.DataAnnotations;

namespace ExcellOnServices.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Service Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Daily Charge per Employee ($)")]
        [Range(0, double.MaxValue, ErrorMessage = "Charge must be a positive value")]
        public decimal DailyChargePerEmployee { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}