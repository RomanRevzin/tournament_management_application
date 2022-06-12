using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Dtos
{
    public class CreateTeamDto
    {
        [Required, Display(Name = "Team name")]
        public string TeamName { get; set; }
        public List<string> Participants { get; set; }
    }
}
