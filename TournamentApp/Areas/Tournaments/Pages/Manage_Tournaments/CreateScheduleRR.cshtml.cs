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
using MimeKit;

namespace TournamentApp.Areas.Tournaments.Pages.Manage_Tournaments
{
    [Authorize]
    public class CreateScheduleRRModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITeamService _teamService;
        private readonly IMatchService _matchService;
        private readonly IEmailSender _emailSender;

        public CreateScheduleRRModel(UserManager<ApplicationUser> userManager,
            ITeamService teamService, IMatchService matchService, IEmailSender emailSender)
        {
            _userManager = userManager;
            _teamService = teamService;
            _matchService = matchService;
            _emailSender = emailSender;
        }

        [BindProperty]
        public string TournamentId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            TournamentId = HttpContext.Request.Query["trId"];

            await _matchService.CreateScheduleRoundRobinAsync(TournamentId);
            
            return RedirectToPage("./Matches/", null, new { trId = TournamentId });
        }
    }
}
