using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;

namespace TournamentApp.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITournamentService _tournamentService;
        private readonly ITeamService _teamService;
        private readonly IMatchService _matchService;

        public IndexModel(UserManager<ApplicationUser> userManager, 
            ITournamentService tournamaentService, ITeamService teamService, IMatchService matchService)
        {
            _userManager = userManager;
            _tournamentService = tournamaentService;
            _teamService = teamService;
            _matchService = matchService;
        }

        public IList<Match> FutureMatches { get; set; }
        public IList<Match> PastMatches { get; set; }
        public IList<Tournament> UserTournaments { get; set; }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                UserTournaments = await _tournamentService.GetTournamentsAsync(user.Id);
                var matches = await _matchService.GetUserMatchListAsync(user.Id);
                FutureMatches = matches.Where(m => m.MatchDate > DateTime.UtcNow).ToList();
                PastMatches = matches.Where(m => m.MatchDate <= DateTime.UtcNow).ToList();
            }
        }
    }
}