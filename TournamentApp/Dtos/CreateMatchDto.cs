using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Dtos
{

    public class CreateMatchDto : IValidatableObject
    {
        [Required]
        public string TeamAName { get; set; }

        [Required]
        public string TeamBName { get; set; }

        [Required]
        public DateTime MatchDate { get; set; }

        public string? WinningTeam { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TeamAName == TeamBName)
            {
                yield return new ValidationResult(
                    errorMessage: "TeamA and TeamB must be different",
                    memberNames: new[] { "TeamBName", "TeamAName" }
               );
            }

            if (WinningTeam != null && (WinningTeam != TeamAName || WinningTeam != TeamBName))
            {
                yield return new ValidationResult(
                    errorMessage: "Winning Team must be in match",
                    memberNames: new[] { "WinningTeam" }
               );
            }

        }
    }
}
