#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments 
{
    public class RemoveMatchModel : PageModel
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMatchService _matchService;


        public RemoveMatchModel(IMatchService matchService,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _matchService = matchService;
        }

        [BindProperty]
        public Match match { get; set; }
        public async Task<IActionResult> OnGetAsync(string? mcId, string? trId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();
                await _matchService.DeleteMatchAsync(trId, user.Id, mcId);
            }
            catch(Exception e)
            {

            }
            return RedirectToPage("./Matches/", null, new { trId = trId });
        }
    }
}
