using System;
using System.ComponentModel.DataAnnotations;
namespace Tounaent_Fixtures.Models
{

    public class TournamentViewModel
    {
        [Key]
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

        public bool? IsActive { get; set; }
    }

}