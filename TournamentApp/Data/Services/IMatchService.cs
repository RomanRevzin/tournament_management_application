using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using TournamentApp.Data.Models;
using TournamentApp.Dtos;

namespace TournamentApp.Data.Services
{
    public interface IMatchService
    {
        Task ClearMatches(Round rounds);
        Task ClearRounds(Round rounds);
        Task ClearSchedule(string tournamentId);
        Task CreateMatchAsync(CreateMatchDto newMatch, string tournamentId, Round round);
        Task CreateMatchAsync(CreateMatchDto newMatch, string tournamentId);
        Task CreateScheduleDefaultAsync(string tournamentId);
        Task<IList<Match>> GetRoundMatchesAsync(Round round);
        Task<Round> GetRoundsAsync(string tournamentId);
        Task<List<Match>> GetTeamMatchesListAsync(string teamId);
        Task<IList<Match>> GetUserMatchListAsync(string userId);
        Task DeleteMatchAsync(string tournamentId, string userId, string gameId);
        Task UpdateMatchAsync(string tournamentId, string userId, string gameId, CreateMatchDto CreateRequest);
        Task<Match> GetMatchAsync(string gameId);
        Task CreateScheduleRoundRobinAsync(string tournamentId);
        Task<IList<Match>> GetTournamentMatchesAsync(string tournamentId);
        Task<IList<Match>> GetTournamentMatchesSortedAsync(string tournamentId);
        Task<List<Match>> GetTeamMatchesNamesListAsync(string teamId);
        Task ForfeitMatchesInactiveTeam(string teamId);

    }

    public class MatchService : IMatchService
    {
        private readonly ITournamentService _tournamentService;
        private readonly ITeamService _teamService;
        private readonly TournamentAppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailService _mailService;

        public MatchService(TournamentAppDbContext context, ITournamentService tournamentService,
            ITeamService teamService, UserManager<ApplicationUser> userManager, IMailService mailService)
        {
            _tournamentService = tournamentService;
            _context = context;
            _teamService = teamService;
            _userManager = userManager;
            _mailService = mailService;
        }

        public async Task<IList<Match>> GetRoundMatchesAsync(Round round)
        {
            return await _context.Matches.Where(m => m.RoundId == round.RoundId).ToListAsync();

        }
        //public async Task<IList<Match>> GetMatchesAsync(string tournamentId)
        //{
        //    var matches = 
        //    return await _context.Matches.Where(m => m.T == tournamentId).ToListAsync();

        //}

        public async Task CreateScheduleRoundRobinAsync(string tournamentId)
        {
            DateTime tDate = (await _tournamentService.GetTournamentDetailAsync(tournamentId)).StartDate;
            await ClearSchedule(tournamentId);
            Round round = new Round()
            {
                RoundId = Guid.NewGuid().ToString(),
                RoundNumber = 1,
                TournamentId = tournamentId,
                Tournament = await _tournamentService.GetByIdAsync(tournamentId)
            };
            _context.Add(round);
            await _context.SaveChangesAsync();

            // Create teams hash table with (teamNumber - incrementing, TeamName) as (K,V) pairs
            IDictionary<int, string> teamsTable = new Dictionary<int, string>();
            var teams = await _teamService.GetTeamsAsync(tournamentId);
            if (teams.Count < 4)
            {
                await CreateSmallRR(teams, tDate, tournamentId, round);
                return;
            }
            int i = 0;
            foreach (var team in teams)
            {
                teamsTable.Add(i++, team.TeamName);
            }
            // Init round matches matrix
            int[,] roundMat = new int[2, teamsTable.Count / 2];
            for (int j = 0; j < teamsTable.Count / 2; j++)
            {
                //if (j < teamsTable.Count / 2)
                    roundMat[0, j] = j;
                //else
                    roundMat[1, j] = j + (teamsTable.Count / 2);
            }
            // Create the matches
            var today = tDate;
            for (int j = 0; j < 2 * teamsTable.Count - 2; j++)
            {
                for (int k = 0; k < teamsTable.Count / 2; k++)
                {
                    var date = today.AddDays(j); //Monotonic rising series for each team
                    if(j< teamsTable.Count - 1)
                    {
                        await CreateMatchAsync(new CreateMatchDto()
                        {
                            TeamAName = teamsTable[roundMat[0, k]],
                            TeamBName = teamsTable[roundMat[1, k]],
                            MatchDate = date
                        }, tournamentId, round);
                    }
                    else
                    {
                        await CreateMatchAsync(new CreateMatchDto()
                        {
                            TeamBName = teamsTable[roundMat[0, k]],
                            TeamAName = teamsTable[roundMat[1, k]],
                            MatchDate = date
                        }, tournamentId, round);
                    }
                }
                // Rotate
                RotateArray(roundMat);
            }
            await _mailService.SendEmailOnMatchCreationAsync(await _teamService.GetTeamsAsync(tournamentId), "rr");
            
        }

        private async Task CreateSmallRR(IList<Team> teams, DateTime today, string tournamentId, Round round)
        {
            int size = teams.Count;
            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    var date = today.AddDays(i + j); //Monotonic rising series for each team
                    await CreateMatchAsync(new CreateMatchDto()
                    {
                        TeamAName = teams[i].TeamName,
                        TeamBName = teams[j].TeamName,
                        MatchDate = date
                    }, tournamentId, round);
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = i + 1; j < size; j++)
                {
                    var date = today.AddDays(i + j); //Monotonic rising series for each team
                    await CreateMatchAsync(new CreateMatchDto()
                    {
                        TeamBName = teams[i].TeamName,
                        TeamAName = teams[j].TeamName,
                        MatchDate = date.AddDays(size + i + j)
                    }, tournamentId, round);
                }
            }
            await _mailService.SendEmailOnMatchCreationAsync(await _teamService.GetTeamsAsync(tournamentId), "default");
        }

        public async Task CreateScheduleDefaultAsync(string tournamentId)
        {
            var teams = await _teamService.GetTeamsAsync(tournamentId);
            int size = teams.Count;
            if (size > 0)
            {
                /* Create a new round for all */
                await ClearSchedule(tournamentId);
                Round round = new Round()
                {
                    RoundId = Guid.NewGuid().ToString(),
                    RoundNumber = 1,
                    TournamentId = tournamentId,
                    Tournament = await _tournamentService.GetByIdAsync(tournamentId)
                };
                _context.Add(round);
                await _context.SaveChangesAsync();

                List<string> teamNames = new List<string>();
                foreach (var team in teams)
                {
                    teamNames.Add(team.TeamName);
                }

                /*  The algorithm  */
                var today = DateTime.Now;
                for (int i = 0; i < size; i++)
                {
                    for (int j = i + 1; j < size; j++)
                    {
                        var date = today.AddDays(i + j); //Monotonic rising series for each team
                        await CreateMatchAsync(new CreateMatchDto()
                        {
                            TeamAName = teamNames[i],
                            TeamBName = teamNames[j],
                            MatchDate = date
                        }, tournamentId, round);
                    }
                }

                await _mailService.SendEmailOnMatchCreationAsync(await _teamService.GetTeamsAsync(tournamentId), "default");
                

            }
        }
        

        public async Task<List<Match>> GetTeamMatchesListAsync(string teamId)
        {
            return await _context.Matches.Where(m => m.TeamAId == teamId || m.TeamBId == teamId).ToListAsync();
        }

        public async Task<List<Match>> GetTeamMatchesNamesListAsync(string teamId)
        {
            var matches =  await _context.Matches.Where(m => m.TeamAId == teamId || m.TeamBId == teamId).ToListAsync();
            foreach(var match in matches)
            {
                var team = await _teamService.GetTeamAsync(teamId);
                if (team != null)
                    match.WinningTeam = team.TeamName;
            }
            return matches;
        }

        public async Task<IList<Match>> GetUserMatchListAsync(string userId)
        {
            var userTeams = await _teamService.GetUserTeamsAsync(userId);
            List<Match> matches = new List<Match>();
            foreach (var team in userTeams)
            {
                var teamMatches = await GetTeamMatchesNamesListAsync(team.TeamId);
                matches.AddRange(teamMatches);
            }
            
            return matches;
        }

        public async Task<IList<Match>> GetTournamentMatchesAsync(string tournamentId)
        {
            Round round = await GetRoundsAsync(tournamentId);
            if(round != null)
            { 
                var matches = await GetRoundMatchesAsync(round);
                foreach (var match in matches)
                {
                    match.TeamA = await _teamService.GetTeamAsync(match.TeamAId);
                    match.TeamB = await _teamService.GetTeamAsync(match.TeamBId);
                }
                return matches;
            }
            return null; 
        }

        public async Task<IList<Match>> GetTournamentMatchesSortedAsync(string tournamentId)
        {
            Round round = await GetRoundsAsync(tournamentId);
            if (round != null)
            {
                var matches = await GetRoundMatchesAsync(round);
                foreach (var match in matches)
                {
                    match.TeamA = await _teamService.GetTeamAsync(match.TeamAId);
                    match.TeamB = await _teamService.GetTeamAsync(match.TeamBId);
                }
                return matches.OrderBy(m => m.MatchDate).ToList();
            }
            return null;
        }
        public async Task ClearSchedule(string tournamentId)
        {
            var rounds = await GetRoundsAsync(tournamentId);
            if (rounds == null)
                return;
            await ClearRounds(rounds);
            await ClearMatches(rounds);
            _mailService.ClearCalendarEvents();
        }

        public async Task ClearMatches(Round rounds)
        {
            var roundId = rounds.RoundId;
            List<Match> matches = await _context.Matches.Where(m => m.RoundId == roundId).ToListAsync();
            foreach (Match match in matches)
            {
                _context.Matches.Remove(match);
            }
            await _context.SaveChangesAsync();
        }

        public async Task ClearRounds(Round rounds)
        {
            _context.Rounds.Remove(rounds);
            await _context.SaveChangesAsync();
        }

        public async Task<Round> GetRoundsAsync(string tournamentId)
        {
            return await _context.Rounds.FirstOrDefaultAsync(r => r.TournamentId == tournamentId);

        }
        public async Task CreateMatchAsync(CreateMatchDto newMatch, string tournamentId, Round round)
        {
            var teamA = _context.Teams.SingleOrDefault(t => t.TeamName == newMatch.TeamAName && t.TournamentId == tournamentId);
            var teamB = _context.Teams.SingleOrDefault(t => t.TeamName == newMatch.TeamBName && t.TournamentId == tournamentId);
            if (teamA == null || teamB == null)
                return;

                Match match = new Match
            {
                GameId = Guid.NewGuid().ToString(),
                TeamAId = teamA.TeamId,
                TeamBId = teamB.TeamId,
                MatchDate = newMatch.MatchDate,
                Round = round,
                RoundId = round.RoundId
            };

            

            _context.Matches.Add(match);


            await _context.SaveChangesAsync();
        }
        public async Task ForfeitMatchesInactiveTeam(string teamId)
        {
            var matches = await GetTeamMatchesListAsync(teamId);
            foreach (Match match in matches)
            {
                match.TeamA = await _teamService.GetTeamAsync(match.TeamAId);
                match.TeamB = await _teamService.GetTeamAsync(match.TeamBId);
                if (match.MatchDate > DateTime.Now)
                {
                    string winningTeam = null;
                    if(match.TeamAId == teamId)
                    {
                        if (match.TeamB.IsActive == true)
                            winningTeam = match.TeamBId;
                    }
                    else
                    {
                        if (match.TeamA.IsActive == true)
                            winningTeam = match.TeamAId;
                    }
                    match.WinningTeam = winningTeam;
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task CreateMatchAsync(CreateMatchDto newMatch, string tournamentId)
        {
            var teamA = _context.Teams.SingleOrDefault(t => t.TeamName == newMatch.TeamAName && t.TournamentId == tournamentId);
            var teamB = _context.Teams.SingleOrDefault(t => t.TeamName == newMatch.TeamBName && t.TournamentId == tournamentId);
            if (teamA == null || teamB == null)
                return;
            Round round = await GetRoundsAsync(tournamentId);
            Match match = new Match
            {
                GameId = Guid.NewGuid().ToString(),
                TeamAId = teamA.TeamId,
                TeamBId = teamB.TeamId,
                MatchDate = newMatch.MatchDate,
                Round = round,
                RoundId = round.RoundId
            };

            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMatchAsync(string tournamentId, string userId, string gameId)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m => m.GameId == gameId);
            if (match == null)
                throw new ArgumentException($"No match with id {gameId}");
            if (!_tournamentService.IsAdmin(tournamentId, userId))
                throw new ArgumentException($"Only admin may add participants");
            _context.Matches.Remove(match);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMatchAsync(string tournamentId, string userId, string gameId, CreateMatchDto CreateRequest)
        {
            var match = await _context.Matches.Include(m => m.TeamA).Include(m => m.TeamB).FirstOrDefaultAsync(m => m.GameId == gameId);
            if (match == null)
                throw new ArgumentException($"No match with id {gameId}");
            if (!_tournamentService.IsAdmin(tournamentId, userId))
                throw new ArgumentException($"Only admin may add participants");
            match.MatchDate = CreateRequest.MatchDate;
            if (CreateRequest.WinningTeam != null)
                match.WinningTeam = CreateRequest.WinningTeam == match.TeamA.TeamName ? match.TeamAId : match.TeamBId;

            await _context.SaveChangesAsync();
        }

        public async Task<Match> GetMatchAsync(string gameId)
        {
            return await _context.Matches.Include(m => m.TeamA).Include(m => m.TeamB).FirstOrDefaultAsync(m => m.GameId == gameId);

        }

        private void RotateArray(int[,] roundMat)
        {
            int len = roundMat.GetLength(1);
            int toRise = roundMat[1, 0];
            int toDrop = roundMat[0, len - 1];
            for (int i = 1; i < len; i++)
            {
                roundMat[0, len- i] = roundMat[0, len - i - 1];
                roundMat[1, i - 1] = roundMat[1, i];

            }
            roundMat[0, 1] = toRise;
            roundMat[1, len - 1] = toDrop;
        }

       
    }
}