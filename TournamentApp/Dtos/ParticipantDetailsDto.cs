
using TournamentApp.Data.Models;

namespace TournamentApp.Dtos
{
    public class ParticipantDetailsDto
    {
        public Role PariticpantRole { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? TeamName { get; set; }
    }
}
