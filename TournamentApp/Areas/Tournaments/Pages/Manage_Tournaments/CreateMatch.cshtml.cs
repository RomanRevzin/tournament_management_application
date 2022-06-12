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
    [Authorize]
    public class CreateMatchModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMatchService _matchService;
        private readonly ITeamService _teamService;
        public CreateMatchModel(IMatchService matchService,ITeamService teamService,
           UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _matchService = matchService;
            _teamService = teamService;
        }
        [BindProperty]
        public CreateMatchDto CreateRequest { get; set; }
        [BindProperty]
        public string TournamentId { get; set; }
        [BindProperty]
        public List<string> TeamNames { get; set; }

        public async Task<IActionResult> OnGet()
        {
            TournamentId = HttpContext.Request.Query["trId"];
            if(TournamentId == null)
                return NotFound();
            TeamNames = await _teamService.GetTeamNamesAsync(TournamentId);
            return Page();
        }




        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return await OnGet();
            }

            await _matchService.CreateMatchAsync(CreateRequest, TournamentId);
            return RedirectToPage("./Matches/",null,new { trId = TournamentId } );
            
         
        }
    }
}
