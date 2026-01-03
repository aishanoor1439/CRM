using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ExcellOnServices.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Display(Name = "Designation")]
        [StringLength(100)]
        public string Designation { get; set; }

        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        public int? DepartmentId { get; set; }


        public int? ServiceId { get; set; }

        [ValidateNever]
        public virtual Service Service { get; set; }
        [ValidateNever]
        public virtual Department Department { get; set; }


    }
}