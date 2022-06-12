#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using TournamentApp.Data.Repos;
using System.Linq.Expressions;
using TournamentApp.Data.Services;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments 
{
    public class MatchesModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMatchService _matchService;
        private readonly ITournamentService _tournamentService;

        public MatchesModel(UserManager<ApplicationUser> userManager, IMatchService matchService, ITournamentService tournamentService)
        {
            _userManager = userManager;
            _matchService = matchService;
            _tournamentService = tournamentService;
        }

        public IList<Match> Matches { get; set; }
        
        [BindProperty]
        public string TournamentId { get; set; }
        [BindProperty]
        public TournamentStatus TournamentStatus { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            TournamentId = HttpContext.Request.Query["trId"];
            if(TournamentId == null)
                return NotFound();
            TournamentStatus = await _tournamentService.GetStatusAsync(TournamentId);
            Matches = await _matchService.GetTournamentMatchesSortedAsync(TournamentId);

            return Page();
        }
    }
}
