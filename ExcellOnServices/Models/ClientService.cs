using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellOnServices.Models
{
    public class ClientService
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Client is required")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Service is required")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Number of employees is required")]
        [Display(Name = "Number of Employees Assigned")]
        [Range(1, 1000, ErrorMessage = "Number of employees must be between 1 and 1000")]
        public int NumberOfEmployees { get; set; } = 1;

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
}