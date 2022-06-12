#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TournamentApp.Data.Models;
using TournamentApp.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using TournamentApp.Dtos;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data.Services;

namespace TournamentApp.Areas.Tournaments.Pages
{
    [Authorize]
    public class TournamentCreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITournamentService _tournamentService;
        private readonly ITeamService _teamService;

        public TournamentCreateModel(ITournamentService tournamentService,
            UserManager<ApplicationUser> userManager, ITeamService teamService)
        {
            _userManager = userManager;
            _tournamentService = tournamentService;
            _teamService = teamService;
        }
       

        public async Task OnGetAsync()
        {
        
            TournamentTypes = await _tournamentService.GetTournamentTypesAsync();
        }

        [BindProperty]
        public CreateTournamentDto CreateRequest { get; set; }
        [BindProperty(SupportsGet =true)]
        public IList<TournamentType> TournamentTypes { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TournamentTypes = await _tournamentService.GetTournamentTypesAsync();
                return Page();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var tournamentId = await _tournamentService.CreateTournamentAsync(user.Id,CreateRequest);

                /* Added default team - admin  */
                await _teamService.CreateTeamAsync(tournamentId, user.Id, user.UserName);

                return RedirectToPage("./Index/");
            }
            return Page();
            
        }

    }
}
