using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellOnServices.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Client is required")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount ($)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Bank Transfer";

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        // Navigation property
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }
    }
}