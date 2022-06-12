using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Data.Models
{
    public class Team
    {
        [Key]
        public string TeamId { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [StringLength(450)]
        public string TournamentId { get; set; }
        public List<Participant> Participants { get; set; }
        public List<Match>? Matches { get; set; }
        //public int Score { get; set; }        MAKE ACTIVE
    }
}
