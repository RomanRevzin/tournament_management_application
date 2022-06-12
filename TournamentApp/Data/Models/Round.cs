using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Data.Models
{
    public class Round 
    {
        [Key]
        [MaxLength(450)]
        public string RoundId { get; set; }
        [Required] 
        [Range(1,10)]
        public int RoundNumber { get; set; }
        [Required]
        [ForeignKey("TournamentId")]
        public string TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public List<Match>? Matches { get; set; }

    }
}
