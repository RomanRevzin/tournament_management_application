using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Dtos
{

    public class TournamentTableDto
    {
        [Required]
        public string TeamName { get; set; }

        public int Games { get; set; }

        public int Wins { get; set; }

        public int Loses { get; set; }

        public int Points { get; set; }


    }
}
