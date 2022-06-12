using System.ComponentModel.DataAnnotations;
using TournamentApp.Data.Models;

namespace TournamentApp.Dtos
{
    public class TeamDto
    {
        [Required]
        public string TeamId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; }
        [Required]
        public bool IsActive { get; set; }

        public int? TeamSize { get; set; }
    }
}
