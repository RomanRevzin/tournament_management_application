using System.ComponentModel.DataAnnotations;
using TournamentApp.Data.Models;

namespace TournamentApp.Dtos
{
    public class TournamentDetailsDto : IValidatableObject
    {
        [Display(Name = "Tournament Name")]
        public string TournamentName { get; set; }
        public TournamentStatus? Status { get; set; }
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        [ Display(Name = "Team size")]
        public int? TeamSize { get; set; }
        public string? TypeName { get; set; }

        public List<ParticipantDetailsDto>? Participants { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (StartDate < DateTime.UtcNow)
            {
                yield return new ValidationResult(
                    errorMessage: "StartDate must be a future Date",
                    memberNames: new[] { "StartDate" }
               );
            }
            if (EndDate < StartDate)
            {
                yield return new ValidationResult(
                    errorMessage: "EndDate must be greater than StartDate",
                    memberNames: new[] { "EndDate" }
               );
            }
            if (EndDate < DateTime.UtcNow)
            {
                yield return new ValidationResult(
                    errorMessage: "EndDate must be a future Date",
                    memberNames: new[] { "EndDate" }
               );
            }
        }

    }
}
