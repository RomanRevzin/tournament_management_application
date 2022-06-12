using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Dtos
{
    public class CreateTournamentDto : IValidatableObject
    {
        [Required]
        public string TournamentName { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        [Range(1, 10)]
        [Display(Name = "Team Size")]
        public int TeamSize { get; set; }
        [Required]
        [Display(Name = "Tournament Type")]
        public int TypeId { get; set; }
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
