//namespace Tounaent_Fixtures.Models
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;


public class RegisterViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Name { get; set; }

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; }

    [Required, Compare("Password"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Please select a gender")]
    [Display(Name = "Gender")]
    public int? GenderId { get; set; }  // The selected gender ID

    public List<SelectListItem> GenderOptions { get; set; } = new(); // For the dropdown

    public string Gender { get; set; }  // This can be used to display the name if needed

    [DataType(DataType.Date)]

    public DateTime DateOfBirth { get; set; } = DateTime.Today; 
    public string Category
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;

            if (age < 12) return "Sub Junior";
            if (age >= 12 && age < 16) return "Junior";
            if (age >= 16 && age < 20) return "Cadet";
            return "Senior";
        }
    }
    public int Age { get; set; }
    public string Aadhaar { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string Address { get; set; }
    public string PinCode { get; set; }
    public string Phone { get; set; }

    public IFormFile Photo { get; set; } 

}

