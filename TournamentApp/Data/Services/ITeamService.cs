using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data.Models;
using TournamentApp.Dtos;

namespace TournamentApp.Data.Services
{
    public interface ITeamService
    {
        Task AddParticiapntAsync(string userId, string teamId, string participantName);
        Task CreateTeamAsync(string tournamentId, string userId, string TeamName);
        Task DeleteTeamAsync(string userId, string teamId);
        Task<IList<Team>> GetAllTeamsAsync();
        Task<TeamDto?> GetTeamDetailsAsync(string teamId);
        Task<IList<Match?>> GetTeamMatches(string teamId);
        Task<List<string>> GetTeamNamesAsync(string tournamentId);
        Task<IList<ParticipantDetailsDto>> GetTeamParticipants(string teamId);
        Task<int> GetTeamPointsAsync(string teamId);
        Task<IList<Team>> GetTeamsAsync(string tournamentId);
        Task<IList<Team>> GetUserTeamsAsync(string userId);
        Task<bool> IsExistAsync(string tournamentId, string teamName);
        Task RemoveParticipantAsync(string tournamentId, string participantId);
        Task RemoveParticipantAsync(string userId, string teamId, string participantId);
        Task UpdateTeamAsync(string teamId, string teamName, bool isActive);
        Task<Team?> GetTeamAsync(string teamId);
        Task<IList<TournamentTableDto>> GetTeamTable(string tournamentId);
    }


    public class TeamService : ITeamService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITournamentService _tournamentService;
        private readonly TournamentAppDbContext _context;
        private readonly IMailService _mailService;

        public TeamService(TournamentAppDbContext context, IMailService mailService,
           UserManager<ApplicationUser> userManager, ITournamentService tournamentService)
        {
            _userManager = userManager;
            _tournamentService = tournamentService;
            _context = context;
            _mailService = mailService;
        }

        public async Task CreateTeamAsync(string tournamentId, string userId, string TeamName)
        {
            if (tournamentId == null || userId == null)
                throw new ArgumentNullException();

            if (!_tournamentService.IsAdmin(tournamentId, userId))
                throw new ArgumentException($"User is not authorized to make such changes");

            if (!_tournamentService.IsExist(tournamentId))
                throw (new ArgumentException($"No Tournament with id {tournamentId}"));
            if (await _tournamentService.GetStatusAsync(tournamentId) != TournamentStatus.creation)
                throw new ArgumentException($"Cannot add Teams to tournament if not in creation");
            _context.Teams.Add(new Team
            {
                TeamId = Guid.NewGuid().ToString(),
                TeamName = TeamName,
                IsActive = false,
                TournamentId = tournamentId,
            });

            await _context.SaveChangesAsync();
        }
        public async Task AddParticiapntAsync(string userId, string teamId, string participantName)
        {
            Team team = await GetTeamAsync(teamId);
            if (team == null)
                throw new ArgumentException($"No team with id {teamId}");
            // check if adding user is tournament admin
            if (!_tournamentService.IsAdmin(team.TournamentId, userId))
                throw new ArgumentException($"Only admin may add participants");
            // check if team capacity is full
            if (_tournamentService.GetByIdAsync(team.TournamentId).Result.TeamSize == GetTeamSize(teamId))
                throw new ArgumentException($"Team at full capacity");
            var participant = await _userManager.FindByNameAsync(participantName);
            // check if new participant user exits
            if (participant == null)
                throw new ArgumentException($"No user with known by {participantName}");
            //check if in team
            if (GetParticipantTeamAsync(participant.Id, team.TournamentId).Result != null)
                throw new ArgumentException($"Participant already in team");
            if (_tournamentService.IsAdmin(team.TournamentId, participant.Id))
            {
                _context.Participants.FirstOrDefault(p => p.UserId == participant.Id).TeamId = teamId;
                team.IsActive = true;
            }
            else
            {
                _context.Participants.Add(new Participant
                {
                    PariticpantRole = Role.participant,
                    UserId = participant.Id,
                    TournamentId = team.TournamentId,
                    TeamId = teamId
                });
                team.IsActive = true;
            }

            var mail = await _userManager.GetEmailAsync(participant);

            if(mail != null)
                await _mailService.SendEmailOnAddedToTournamentAsync(mail);

            await _context.SaveChangesAsync();
        }

        public async Task<TeamDto?> GetTeamDetailsAsync(string teamId)
        {
            if (teamId == null)
                throw new ArgumentNullException($"Team id is null");

            var team = await _context.Teams.Include(t => t.Participants).FirstOrDefaultAsync(t => t.TeamId == teamId);

            TeamDto teamDetails = new TeamDto()
            {
                TeamId = team.TeamId,
                TeamName = team.TeamName,
                IsActive = team.IsActive,
            };

            return teamDetails;
        }
        public async Task RemoveParticipantAsync(string userId, string teamId, string participantId)
        {
            Team team = await GetTeamAsync(teamId);
            if (team == null)
                throw new ArgumentException($"No team with id {teamId}");
            if (!_tournamentService.IsAdmin(team.TournamentId, userId) && userId != participantId)
                throw new ArgumentException($"Only admin may remove participants");
            if (_tournamentService.IsAdmin(team.TournamentId, participantId))
                _context.Participants.FirstOrDefault(p => p.UserId == participantId && p.TeamId == teamId).TeamId = null;
            else
                _context.Participants.Remove(_context.Participants.FirstOrDefault(p => p.UserId == participantId && p.TeamId == teamId));
            if(team.Participants.Count == 1)
                team.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveParticipantAsync(string tournamentId, string participantId)
        {
            var team = await GetParticipantTeamAsync(participantId, tournamentId);
            if (team == null)
                throw new ArgumentException($"No Team in tournament {tournamentId} with Participant {participantId}");
            await RemoveParticipantAsync(participantId, team.TeamId, participantId);
        }
        public async Task DeleteTeamAsync(string userId, string teamId)
        {
            Team team = await GetTeamAsync(teamId);
            if (team == null)
                throw new ArgumentException($"No team with id {teamId}");
            // check if tournament is in creation status
            if (await _tournamentService.GetStatusAsync(team.TournamentId) != TournamentStatus.creation)
                throw new ArgumentException("Cannot change team from tournament not in creation");
            if (!_tournamentService.IsAdmin(team.TournamentId, userId))
                throw new ArgumentException($"Only admin may add participants");

            team.Participants.ForEach(p =>
            {
                if (p.UserId != userId)
                    _context.Participants.Remove(p);
            });

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
        public async Task<IList<Team>> GetTeamsAsync(string tournamentId)
        {
            return await _context.Teams.Include(t=>t.Participants).Include(t => t.Matches).Where(t => t.TournamentId == tournamentId).ToListAsync();
        }
        public async Task<IList<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams.ToListAsync();
        }
        public async Task<IList<ParticipantDetailsDto>> GetTeamParticipants(string teamId)
        {
            return await _context.Participants.Where(p => p.TeamId == teamId).Include(p => p.User).Include(p => p.Team).Select(p => new ParticipantDetailsDto
            {
                PariticpantRole = p.PariticpantRole,
                UserId = p.UserId,
                UserName = p.User.UserName,
                TeamName = p.Team.TeamName,
                Email = p.User.Email,
            }).ToListAsync();
        }
        public async Task<IList<Match?>> GetTeamMatches(string teamId)
        {
            var matches = await _context.Matches.Where(m => m.TeamAId == teamId || m.TeamBId == teamId).ToListAsync();
            return matches;

        }
        public async Task UpdateTeamAsync(string teamId, string teamName, bool isActive)
        {
            var team = await GetTeamAsync(teamId);
            if (team == null)
                throw new ArgumentNullException(nameof(team));
            team.IsActive = isActive;
            team.TeamName = teamName;
            await _context.SaveChangesAsync();
        }
        public async Task<int> GetTeamPointsAsync(string teamId)
        {
            if (teamId == null)
                throw new ArgumentNullException();
            var matches = await GetTeamMatches(teamId);
            return matches.Count(m => m.WinningTeam == teamId);
        }

        private async Task<string?> GetTeamNameById(string id)
        {
            var team = await GetTeamAsync(id);
            if(team!=null)
                return team.TeamName;
            return null;

        }

        public async Task<List<string>> GetTeamNamesAsync(string tournamentId)
        {
            if (tournamentId == null)
                throw new ArgumentNullException();
            return await _context.Teams.Where(t=>t.TournamentId == tournamentId).Select(t => t.TeamName).ToListAsync();
        }
        public async Task<bool> IsExistAsync(string tournamentId, string teamName)
        {
            if (tournamentId == null || teamName == null)
                throw new ArgumentNullException();
            return await _context.Teams.AnyAsync(t => t.TeamName == teamName && t.TournamentId == tournamentId);
        }
        public Task<Team?> GetTeamAsync(string teamId)
        {
            return _context.Teams.Include(t => t.Participants).FirstOrDefaultAsync(t => t.TeamId == teamId);
        }
        private async Task<Team?> GetParticipantTeamAsync(string participantId, string tournamentId)
        {
            var t = await _context.Teams.FirstOrDefaultAsync(t => t.Participants.Any(p => p.UserId == participantId && p.TournamentId == tournamentId));
            return t;
        }
        private int GetTeamSize(string teamId) => _context.Teams.Include(t => t.Participants).FirstOrDefault(t => t.TeamId == teamId).Participants.Count;

        public async Task<IList<Team>> GetUserTeamsAsync(string userId)
        {
            var teams = await GetAllTeamsAsync();
            List<Team> userTeams = new List<Team>();
            foreach (var team in teams)
            {
                var teamParticipants = await GetTeamParticipants(team.TeamId);
                foreach (var p in teamParticipants)
                {
                    if (p.UserId == userId)
                    {
                        userTeams.Add(team);
                    }
                }
            }
            return userTeams;
        }
        public async Task<IList<TournamentTableDto>> GetTeamTable(string tournamentId)
        {
            List<TournamentTableDto> TournamentTb = new List<TournamentTableDto>();
            if (tournamentId == null)
                throw new ArgumentNullException();
            var teams = await GetTeamsAsync(tournamentId);
            {
                foreach (var team in teams)
                {
                    // do we need to check IsExist?
                    var TeamGames = await GetTeamMatches(team.TeamId);
                    var FinishedGames = TeamGames.Where(g=> g.WinningTeam!= null).ToList();
                    var TeamWins = await GetTeamPointsAsync(team.TeamId);
                    var TeamLoses = FinishedGames.Count - TeamWins;
                    TournamentTableDto teamTableRow = new TournamentTableDto()
                    {
                        TeamName = team.TeamName,
                        Games = FinishedGames.Count,
                        Wins = TeamWins,
                        Loses = TeamLoses,
                        Points = TeamWins,

                    };
                    TournamentTb.Add(teamTableRow);
                }
                return TournamentTb.OrderByDescending(p => p.Points).ToList();
            }
        }
    }
}
