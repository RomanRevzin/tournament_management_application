using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments.Manage_Teams 
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITeamService _teamService;
        private readonly ITournamentService _tournamentService;

        public IndexModel(UserManager<ApplicationUser> userManager,
            ITeamService teamService, ITournamentService tournamentService)
        {
            _userManager = userManager;
            _teamService = teamService;
            _tournamentService = tournamentService;
        }
        [BindProperty]
        public IList<Team> Team { get;set; } = default!;
        [BindProperty]
        public bool InCreation { get;set; }

        public async Task OnGetAsync(string trId)
        {  
             Team = await _teamService.GetTeamsAsync(trId);  
             if(Team == null)
                NotFound();
            InCreation = await _tournamentService.GetStatusAsync(trId) == TournamentStatus.creation;
        }

        public async Task OnPostRemoveAsync(string trId,string tmId)
        {
           await _teamService.DeleteTeamAsync(_userManager.GetUserId(User), tmId);
           await OnGetAsync(trId);
        }
    }
}
