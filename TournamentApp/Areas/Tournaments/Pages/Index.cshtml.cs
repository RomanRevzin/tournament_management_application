#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using TournamentApp.Data.Repos;
using System.Linq.Expressions;
using TournamentApp.Dtos;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TournamentApp.Data.Services;

namespace TournamentApp.Areas.Tournaments.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITournamentService _tournamentService;
        private readonly ITeamService _teamService;


        public IndexModel(UserManager<ApplicationUser> userManager, ITournamentService tournamaentService, ITeamService teamService)
        {
            _userManager = userManager;
            _tournamentService = tournamaentService;
            _teamService = teamService;
        }
        

        public IList<Tournament> AdminTournaments { get; set; }
        public IList<Tournament> ParticipantTournaments { get; set; }

        public async Task OnGetAsync()
        {
            
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                AdminTournaments = await _tournamentService.GetTournamentsAsync(user.Id,Role.admin);
                ParticipantTournaments = await _tournamentService.GetTournamentsAsync(user.Id, Role.participant);
            }

        }

        public async Task<PartialViewResult> OnGetCreateModalPartialAsync()
        {
            CreateViewModel CreateViewModel = new CreateViewModel()
            {
                Tournament = new CreateTournamentDto(),
                TournamentTypes = await _tournamentService.GetTournamentTypesAsync()
            };
            // this handler returns _CreateTournament
            return new PartialViewResult
            {
                ViewName = "_CreateTournament",
                ViewData = new ViewDataDictionary<CreateViewModel>(ViewData, CreateViewModel)
            };
        }

        public async Task OnPostQuit(string trId)
        {
            await _teamService.RemoveParticipantAsync(trId, _userManager.GetUserId(User));
            await OnGetAsync();
        }
        public async Task<PartialViewResult> OnPostCreateModalPartial(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _tournamentService.CreateTournamentAsync(_userManager.GetUserId(User), model.Tournament);
                AdminTournaments = await _tournamentService.GetTournamentsAsync(_userManager.GetUserId(User), Role.admin);
                RedirectToPage("./Index/");
            }
            model.TournamentTypes =  await _tournamentService.GetTournamentTypesAsync();
            return new PartialViewResult
            {
                ViewName = "_CreateTournament",
                ViewData = new ViewDataDictionary<CreateViewModel>(ViewData, model)
            };
        }

    }
    public class CreateViewModel
    {
        public CreateTournamentDto Tournament { get; set; }
        public IList<TournamentType> TournamentTypes { get; set; }
    }
}
