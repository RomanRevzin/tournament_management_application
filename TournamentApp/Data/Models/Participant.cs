using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Data.Models
{
    public enum Role { admin, participant}
    public class Participant
    {
        public Role PariticpantRole { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required]
        public string TournamentId { get; set; }
        [ForeignKey("TournamentId")]
        public Tournament? Tournament {get;set;}
        public string? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public Team? Team { get; set; }
    }
}
