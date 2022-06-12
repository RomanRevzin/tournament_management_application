using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
 
namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments.Manage_Teams
{
    public class DeleteModel : PageModel
    {
        private readonly TournamentApp.Data.TournamentAppDbContext _context;

        public DeleteModel(TournamentApp.Data.TournamentAppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Team Team { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FirstOrDefaultAsync(m => m.TeamId == id);

            if (team == null)
            {
                return NotFound();
            }
            else 
            {
                Team = team;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null || _context.Teams == null)
            {
                return NotFound();
            }
            var team = await _context.Teams.FindAsync(id);

            if (team != null)
            {
                Team = team;
                _context.Teams.Remove(Team);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
