using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tounaent_Fixtures.Models
{
    public class PlayerViewModel 
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string FatherName { get; set; }


        [Required(ErrorMessage = "Please select a gender")]
        [Display(Name = "Gender")]
        public int GenderId { get; set; }

        public List<SelectListItem> GenderOptions { get; set; } = new();

        public string Gender => GenderId == 1 ? "Male" : GenderId == 2 ? "Female" : "Other";

        [Required]
        public string MobileNo { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime Dob { get; set; } = DateTime.Today;
        public int Age => Dob != null ? (int)((DateTime.Now - Dob).TotalDays / 365.25) : 0;


        [Required]
        public int CatId { get; set; }
        public string CategoryName
        {
            get
            {
                var age = Age;
                if (age < 6) return "PeeWee";
                if (age < 12) return "SubJunior";
                if (age < 16) return "Junior";
                if (age < 20) return "Cadet";
                return "Senior";
            }
        }


        [Required]
        public int WeightCatId { get; set; }

        [NotMapped]
        public List<SelectListItem> WeightCatOptions { get; set; } = new();


        [Required]
        public int DistictId { get; set; }

        public List<SelectListItem> DistrictOptions { get; set; } = new();

        [Required]
        public int ClubId { get; set; }

        [NotMapped]
        public List<SelectListItem> ClubOptions { get; set; } = new();
        public string ClubName {  get; set; }

        [Required]
        public string AdharNumb { get; set; }

        [Required]
        public string Address { get; set; }

        public string? Remarks { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
