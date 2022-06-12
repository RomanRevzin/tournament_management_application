using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TournamentApp.Data.Models
{
    public class TournamentType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TypeId { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public List<Tournament> Tournaments { get; set; }
    }
}