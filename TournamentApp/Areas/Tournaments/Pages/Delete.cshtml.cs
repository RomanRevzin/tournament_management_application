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

namespace TournamentApp.Areas.Tournaments.Pages
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ITournamentService _tournamentService;
        private readonly UserManager<ApplicationUser> _userManager;
        public DeleteModel(ITournamentService tournamentService, UserManager<ApplicationUser> userManager )
        {
            _tournamentService = tournamentService;
            _userManager = userManager;
        }

        [BindProperty]
        public Tournament Tournament { get; set; }

        public async Task<IActionResult> OnGetAsync(string? trId)
        {
            if (trId == null)
            {
                return NotFound();
            }

           Tournament = await _tournamentService.GetByIdAsync(trId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? trId)
        {
            if (trId == null)
            {
                return NotFound();
            }
            await _tournamentService.DeleteAsync(trId,_userManager.GetUserId(User));

            return RedirectToPage("./Index/");
        }
    }
}
