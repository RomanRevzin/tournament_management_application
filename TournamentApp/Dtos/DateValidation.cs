using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Dtos
{
    public class CompareDatesVaidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (Dtos.CreateTournamentDto)validationContext.ObjectInstance;
            DateTime _endDate = Convert.ToDateTime(value);
            DateTime _startDate = Convert.ToDateTime(model.StartDate);

            if (_startDate > _endDate)
            {
                return new ValidationResult
                    ("EndDate must be after StartDate.");
            }
            else if (_endDate < DateTime.UtcNow)
            {
                return new ValidationResult
                    ("EndDate must be in the future.");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}
