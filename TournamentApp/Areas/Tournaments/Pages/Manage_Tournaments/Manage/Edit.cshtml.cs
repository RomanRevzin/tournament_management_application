using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;
using TournamentApp.Dtos;
using MimeKit;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments.Manage_Teams 
{
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITeamService _teamService;
        private readonly IMailService _mailService;
        private readonly IMatchService _matchService;
        public EditModel(UserManager<ApplicationUser> userManager, IMatchService matchService,
            ITeamService teamService, IMailService mailService)
        {
            _userManager = userManager;
            _teamService = teamService;
            _mailService = mailService;
            _matchService = matchService;
        }

        [BindProperty]
        public TeamDto Team { get; set; } = default!;
        [BindProperty]
        public IList<ParticipantDetailsDto> Participants { get; set; }
        [BindProperty]
        public string? NewParticipant { get; set; }

        public async Task<IActionResult> OnGetAsync(string tmId)
        {
            if (tmId == null)
            {
                return NotFound();  
            }

            var team =  await _teamService.GetTeamDetailsAsync(tmId);
            if (team == null)
            {
                return NotFound();
            }
            Team = team;
            Participants = await _teamService.GetTeamParticipants(tmId);
            return Page();
        }

 
        public async Task<IActionResult> OnPostEditAsync(string trId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            await _teamService.UpdateTeamAsync(Team.TeamId,Team.TeamName,Team.IsActive);
            if (Team.IsActive == false)
                await _matchService.ForfeitMatchesInactiveTeam(Team.TeamId);
            return RedirectToPage("./Index", null, new { trId = trId });
        }

        public async Task<IActionResult> OnPostRemoveParticipantAsync(string trId, string tmId, string pId)
        {
            try
            {
                await _teamService.RemoveParticipantAsync(_userManager.GetUserId(User), tmId, pId);
            }
            catch
            {
                return RedirectToPage("./Index", null, new { trId = trId });
            }
            return RedirectToPage("", null, new { tmId = tmId, trId = trId });
        }
        public async Task<IActionResult> OnPostAddParticipantAsync(string trId, string tmId)
        {
            try
            {
                await _teamService.AddParticiapntAsync(_userManager.GetUserId(User), tmId, NewParticipant);

            }
            catch(ArgumentException e)
            {
                ModelState.AddModelError("NewParticipant", e.Message);
                Participants = await _teamService.GetTeamParticipants(tmId);
                return Page();
            }
            return RedirectToPage("", null, new { tmId = tmId,trId=trId }); 
           
        }

        public async Task<IActionResult> OnPostAutoCompleteAsync(string prefix, string trId )
        {
            if (trId == null)
                return RedirectToPage("./Index/", null, new { trId = trId });

            var users = _userManager.Users.Where(u => (u.UserName.StartsWith(prefix) || u.Email.StartsWith(prefix)) && !u.UserTournaments.Any(p => p.TournamentId == trId))
                .Select(u => u.UserName.StartsWith(prefix) ? u.UserName : u.Email).Take(10).ToListAsync();


            return new JsonResult(await users);
        }
    }
}
