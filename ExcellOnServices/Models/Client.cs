using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcellOnServices.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(200, ErrorMessage = "Company Name cannot exceed 200 characters")]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Contact Person is required")]
        [StringLength(100, ErrorMessage = "Contact Person cannot exceed 100 characters")]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Registration Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;

        // Navigation property to ClientServices
        public virtual ICollection<ClientService> ClientServices { get; set; } = new List<ClientService>();
    }
}