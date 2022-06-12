using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments 
{
    public class ParticipantsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TournamentAppDbContext _context;

        public ParticipantsModel(UserManager<ApplicationUser> userManager,
            TournamentAppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [BindProperty]
        public List<Participant> Participants { get; set; }
        [BindProperty]
        public string NewParticipant { get; set; }
        [BindProperty]
        public string TournamentId { get; set; }
        public IActionResult OnGet(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TournamentId = id;

            if (!_context.Tournaments.Any(t => t.TournamentId == id))
            {
                return NotFound();
            }
            Participants = _context.Participants.Where(p => p.TournamentId == id).Include(p => p.User).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = _context.Users.Where(t => t.UserName == NewParticipant).SingleOrDefault();
            if (user != null)
            {
                string teamId = Guid.NewGuid().ToString();
                Participant p = new Participant()
                {
                    PariticpantRole = Role.participant,
                    UserId = user.Id,
                    TournamentId = TournamentId,
                    TeamId = teamId
                };
                _context.Participants.Add(p);

                _context.Teams.Add(new Team()
                {
                    TeamId = teamId,
                    TeamName = user.UserName,
                    IsActive = true,
                    TournamentId = TournamentId,
                    Participants = new List<Participant>
                    {
                        p
                    }
                });

                await _context.SaveChangesAsync();

                return RedirectToPage("./Index/", null, new { id = TournamentId });
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAutoCompleteAsync(string prefix, string tid)  
        {
            if (tid == null)
                return RedirectToPage("./Index/", null, new { id = TournamentId });

            var users = _context.AppUsers.Where(u => (u.UserName.StartsWith(prefix) || u.Email.StartsWith(prefix)) && !u.UserTournaments.Any(p => p.TournamentId == tid))
                .Select(u => u.UserName.StartsWith(prefix) ? u.UserName : u.Email).Take(10).ToListAsync();


            return new JsonResult(await users);
        }

    }
}
