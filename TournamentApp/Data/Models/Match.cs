using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Data.Models
{
    public partial class Match 
    {
        [Key]
        public string GameId { get; set; }
        public string TeamAId { get; set; } 
        public string TeamBId { get; set; }
        
        [Display(Name = " Match Date")]
        public DateTime MatchDate { get; set; }
        [StringLength(450)]
        public string?  WinningTeam{ get; set; }
        [Required]
        [ForeignKey("RoundId")]
        public string RoundId { set; get; }
        public Round? Round { get; set; }
        public  Team TeamA { get; set; }
        public  Team TeamB { get; set; }
    }

    public partial class Match : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ( TeamAId == TeamBId)
            {
                yield return new ValidationResult(
                    errorMessage: "TeamA and TeamB must be different",
                    memberNames: new[] { "TeamBId" }
               );
            }

            if (WinningTeam != null && (WinningTeam != TeamAId || WinningTeam != TeamBId))
            {
                yield return new ValidationResult(
                    errorMessage: "Winning Team must be in match",
                    memberNames: new[] { "WinningTeam" }
               );
            }

        }
    }
    
}