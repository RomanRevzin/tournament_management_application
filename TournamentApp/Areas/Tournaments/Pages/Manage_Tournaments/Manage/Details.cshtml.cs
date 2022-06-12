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
using TournamentApp.Dtos;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments.Manage_Teams 
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITeamService _teamService;

        public DetailsModel(UserManager<ApplicationUser> userManager,
            ITeamService teamService)
        {
            _userManager = userManager;
            _teamService = teamService;
        }
        [BindProperty]
        public string TournamentId { get; set; }
        [BindProperty]
        public TeamDto? Team { get; set; } = default!; 
        [BindProperty]
        public IList<ParticipantDetailsDto> ParticipantDetails { get; set; }

        public async Task<IActionResult> OnGetAsync(string trId,string tmId)
        {
            if (trId == null || tmId == null)
            {
                return NotFound();
            }
            TournamentId = trId;    
            Team = await _teamService.GetTeamDetailsAsync(tmId);
            ParticipantDetails = await _teamService.GetTeamParticipants(tmId);
            if (Team == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
