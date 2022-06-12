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

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments 
{
    public class UpdateMatchModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMatchService _matchService;



        public UpdateMatchModel(IMatchService matchService,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _matchService = matchService;
        }

        [BindProperty]
        public CreateMatchDto CreateRequest { get; set; }

        [BindProperty]
        public Match match { get; set; }
        public async Task<IActionResult> OnGetAsync(string? mcId, string ? trId)
        {
            match = await _matchService.GetMatchAsync(mcId);
            CreateRequest = new CreateMatchDto 
            {
                MatchDate = match.MatchDate ,
                WinningTeam = match.WinningTeam 
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? mcId, string? trId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _matchService.UpdateMatchAsync(trId, user.Id, mcId, CreateRequest);
                return RedirectToPage("./Matches/", null, new { trId = trId });
            }
            return Page();
        }
    }
}
