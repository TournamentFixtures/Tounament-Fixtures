//namespace Tounaent_Fixtures.Models
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

    public string Gender { get; set; }
    public int Age { get; set; }
    public string Aadhaar { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string Address { get; set; }
    public string PinCode { get; set; }
    public string Phone { get; set; }

    public IFormFile Photo { get; set; } 

}
