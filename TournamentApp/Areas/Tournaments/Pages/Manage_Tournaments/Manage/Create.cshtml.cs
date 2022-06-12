using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments.Manage_Teams 
{
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITeamService _teamService;

        public CreateModel(UserManager<ApplicationUser> userManager,
            ITeamService teamService)
        {
            _userManager = userManager;
            _teamService = teamService; 
        } 

        public IActionResult OnGet(string trId)
        {
            TournamentId = trId;
            return Page();
        }

        [BindProperty]
        [RegularExpression(@"^\S(?!.*\s{2}).*?\S$", ErrorMessage = "Illegal name of team")]
        [PageRemote(ErrorMessage ="Team name already exists in Tournament",HttpMethod="post",PageHandler="Validate", AdditionalFields = "__RequestVerificationToken,TournamentId")]
        public string TeamName { get; set; } = default!;
        [BindProperty]
        public string TournamentId { get; set; }
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (TeamName == null)
            {
                return Page();
            }
            var userId = _userManager.GetUserId(User);
        await _teamService.CreateTeamAsync(TournamentId,userId,TeamName);


            return RedirectToPage("./Index", null, new { trId = TournamentId });
        }

        public async Task<IActionResult> OnPostValidate(string TeamName,string tournamentId)
        {
            var exists = await _teamService.IsExistAsync(tournamentId, TeamName);

            return new JsonResult(!exists);
        }
    }
}
