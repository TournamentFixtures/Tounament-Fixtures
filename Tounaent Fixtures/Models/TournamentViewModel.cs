using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Tounaent_Fixtures.Models
{

    public class TournamentViewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // <-- this is important
        public int Tournament_Id { get; set; }

        [Required]
        public string TournamentName { get; set; }

        [Required]
        public string OrganizedBy { get; set; }

        [Required]
        public string Venue { get; set; }

        public DateTime? From_dt { get; set; }
        public DateTime? To_dt { get; set; }

        public DateTime? Added_dt { get; set; }
        public string Added_by { get; set; }

        public DateTime? Modify_dt { get; set; }
        public string Modify_by { get; set; }

        public bool IsActive { get; set; }
        public int? DistictId { get; set; }

        public string? DistictName { get; set; }

        public List<SelectListItem> DistrictOptions { get; set; } = new();
        [Display(Name = "Upload Logo1")]
        public IFormFile? Logo1 { get; set; }
        [Display(Name = "Upload Logo2")]
        public IFormFile? Logo2 { get; set; }

    }

}