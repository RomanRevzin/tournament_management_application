using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Data.Models
{
    public enum TournamentStatus { creation, ongoing, finished}
    public class Tournament
    {
        [Key]
        public string TournamentId { get; set; }
        [Required]
        [MaxLength(50)]
        [Display(Name = "Tournament Name")]
        public string TournamentName { get; set; }
        [Required,DefaultValue(TournamentStatus.creation)]
        public TournamentStatus Status { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [Required, Display(Name = "Team size")]
        [Range(1, 20)]
        public int TeamSize { get; set; } = 1;
        [Required]
        [ForeignKey("TypeId")]
        public int TypeId { get; set; }
        [Required,Display(Name ="Tournament Teams Capacity")]
        public int TeamCapacity { get; set; }
        public TournamentType Type { get; set; }
        public List<Participant>? Participants { get; set; }
        public List<Round>? Rounds { get; set; }

    }
  
}
