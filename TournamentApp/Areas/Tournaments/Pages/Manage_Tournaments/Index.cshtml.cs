using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;
using TournamentApp.Dtos;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITournamentService _tournamentService;

        public IndexModel(UserManager<ApplicationUser> userManager,
            ITournamentService tournamentService)
        {
            _userManager = userManager;
            _tournamentService = tournamentService;
        }

        [BindProperty]
        public TournamentDetailsDto Tournament { get; set; }

        [BindProperty]
        public string TournamentId { get; set; }

        public async Task<IActionResult> OnGetAsync(string? trId)
        {
            if (trId == null)
            {
                return NotFound();
            }
            TournamentId = trId;
            Tournament = await _tournamentService.GetTournamentDetailAsync(trId);

            if (Tournament == null)
            {
                return NotFound();
            }
            return Page();
        }

       
        public async Task<IActionResult> OnPostEditAsync(string trId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _tournamentService.UpdateAsync(trId,_userManager.GetUserId(User),Tournament);

            
            return RedirectToPage("./Index", null,new {trId = trId});
        }
        public async Task<IActionResult> OnPostStartAsync(string trId)
        {
            await _tournamentService.SetOngoingAsync(trId, _userManager.GetUserId(User));
            return RedirectToPage("./Index", null, new { trId = trId });
        }

    }

}
