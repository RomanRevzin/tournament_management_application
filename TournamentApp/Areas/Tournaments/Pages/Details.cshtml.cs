#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data;
using TournamentApp.Data.Models;
using TournamentApp.Data.Services;
using TournamentApp.Dtos;

namespace TournamentApp.Areas.Tournaments.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ITournamentService _tournamentService;
        private readonly ITeamService _teamService;

        public DetailsModel(ITournamentService tournamentService, ITeamService teamService)
        {
            _tournamentService = tournamentService;
            _teamService = teamService;

        }
        
        public TournamentDetailsDto TournamentDetials { get; set; }
        public IList<TournamentTableDto> TournamentTable { get; set; }
        public async Task<IActionResult> OnGetAsync(string? trId)
        {
            
            if (trId == null)
            {
                return NotFound();
            }

            TournamentDetials = await _tournamentService.GetTournamentDetailAsync(trId);
            TournamentTable = await _teamService.GetTeamTable(trId);

            if (TournamentDetials == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
