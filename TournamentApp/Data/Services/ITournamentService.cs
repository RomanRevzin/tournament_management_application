using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TournamentApp.Data.Models;
using TournamentApp.Data.Repos;
using TournamentApp.Dtos;

namespace TournamentApp.Data.Services
{
    public interface ITournamentService
    {  
        Task<IList<Tournament>> GetTournamentsAsync(string userId);
        Task<IList<Tournament>> GetTournamentsAsync(string userId,Role role);
        Task<IList<TournamentType>> GetTournamentTypesAsync();
        Task<Tournament?> GetByIdAsync(string tournamentId);
        Task<TournamentDetailsDto?> GetTournamentDetailAsync(string tournamentId);
        Task<string> CreateTournamentAsync(string userId, CreateTournamentDto newTournament);
        Task UpdateAsync(string tournamentId, string userId, TournamentDetailsDto editTournament);
        Task DeleteAsync(string tournamentId,string userId);
        bool IsExist(string tournamentId);
        bool IsAdmin(string tournamentId, string userId);
        Task AddParticipant(string userId, Participant newParticipant);
        Task<TournamentStatus> GetStatusAsync(string tournamentId);
        Task SetOngoingAsync(string tournamentId, string userId);

    }

    public class TournamentService : ITournamentService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TournamentAppDbContext _context;
        
        public TournamentService(TournamentAppDbContext context,
           UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            
        }
        public async Task<IList<Tournament>> GetTournamentsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"No user with id {userId}");

            return _context.Participants.Where(p => p.UserId == userId).Select(p => p.Tournament).ToList();
        }
        public async Task<IList<Tournament>> GetTournamentsAsync(string userId, Role role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"No user with id {userId}");

            return _context.Participants.Where(p => p.UserId == userId && p.PariticpantRole == role).Select(p => p.Tournament).ToList();
        }
        public async Task<IList<TournamentType>> GetTournamentTypesAsync()
        {
            var types = await _context.TournamentTypes.ToListAsync();
            if (types.Count==0)
            {
                TournamentType basicLeague = new TournamentType()
                {
                    
                    Name = "Basic League"
                };
                _context.TournamentTypes.Add(basicLeague);
                _context.SaveChanges();
                types = await _context.TournamentTypes.ToListAsync();
            }
            return types;
        }
        public async Task<TournamentDetailsDto?> GetTournamentDetailAsync(string tournamentId)
        {
            var tournament = await GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new ArgumentNullException();
            var participants = await _context.Participants.Where(p=>p.TournamentId == tournamentId).Select(p=>new ParticipantDetailsDto(){
                UserName = p.User.UserName,
                UserId = p.UserId,
                Email = p.User.Email,
                PariticpantRole = p.PariticpantRole,
                TeamName = p.TeamId != null ? p.Team.TeamName : null 
            }
            ).ToListAsync();
        
            TournamentDetailsDto tournamentDetails = new TournamentDetailsDto()
            {
                TournamentName = tournament.TournamentName,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Status = tournament.Status,
                TeamSize = tournament.TeamSize,
                TypeName = _context.TournamentTypes.FindAsync(tournament.TypeId).Result.Name,
                Participants = participants
            };

            return tournamentDetails;
        }
        public async Task<Tournament?> GetByIdAsync(string tournamentId)
        {
            return await _context.Tournaments.FindAsync(tournamentId);
        }
        public async Task<string> CreateTournamentAsync(string userId, CreateTournamentDto newTournamentDto)
        {
            //check user exists
            if (await _userManager.FindByIdAsync(userId) == null)
                throw new ArgumentException($"No user with id {userId}");

            if (newTournamentDto == null)
                throw new ArgumentNullException(nameof(Tournament));

            Tournament newTournament = new Tournament
            {
                TournamentId = Guid.NewGuid().ToString(),
                TournamentName = newTournamentDto.TournamentName,
                StartDate = newTournamentDto.StartDate,
                EndDate = newTournamentDto.EndDate,
                TeamSize = newTournamentDto.TeamSize,
                TypeId = newTournamentDto.TypeId,
                Status = TournamentStatus.creation
            };
            Participant admin = new Participant
            {
                UserId = userId,
                TournamentId = newTournament.TournamentId,
                PariticpantRole = Role.admin
            };

            newTournament.Participants = new List<Participant>() { admin };
            _context.Add(newTournament);

            await _context.SaveChangesAsync();
            return newTournament.TournamentId;
        }
        public bool IsExist(string tournamentId)
        {
            return _context.Tournaments.Any(t => t.TournamentId == tournamentId);
        }
        public bool IsAdmin(string tournamentId, string userId)
        {
            return _context.Participants
                .Any(p => p.TournamentId == tournamentId && p.UserId == userId && p.PariticpantRole == Role.admin);
        }
        public async Task UpdateAsync(string tournamentId, string userId, TournamentDetailsDto editTournament)
        {
            var tournament = await GetByIdAsync(tournamentId);
            tournament.TournamentName = editTournament.TournamentName;
            tournament.StartDate = editTournament.StartDate;
            tournament.EndDate = editTournament.EndDate;
            _context.Attach(tournament).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IsExist(tournament.TournamentId))
                {
                    throw new ArgumentException($"No tournament with id {tournament.TournamentId}");
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task DeleteAsync(string tournamentId, string userId)
        {
            if (userId == null || tournamentId == null)
                throw new ArgumentNullException("Id must not be null");
            // check if user admin
            if (!IsAdmin(tournamentId, userId))
                throw new ArgumentException($"User with id {userId} is not authorized to make such changes");

            var tournament = await GetByIdAsync(tournamentId);
            if (tournament != null)
                _context.Tournaments.Remove(tournament);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (tournament == null)
                {
                    throw new ArgumentException($"No tournament with id {tournamentId}");
                }
                else
                {
                    throw;
                }
            }
        }
        public async Task AddParticipant(string userId, Participant newParticipant)
        {
            if (userId == null || newParticipant == null)
                throw new ArgumentNullException("Id must not be null");
            // check if user admin
            if (!IsAdmin(newParticipant.TournamentId, userId))
                throw new ArgumentException($"User with id {userId} is not authorized to make such changes");
            if(!IsExist(newParticipant.TournamentId))
                throw new ArgumentException($"No Tournament with id {newParticipant.TournamentId}");
            _context.Attach(newParticipant).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
        public async Task<TournamentStatus> GetStatusAsync(string tournamentId)
        {
            if (tournamentId == null)
                throw new ArgumentNullException();
            if (!IsExist(tournamentId))
                throw new ArgumentException($"No tournament with id {tournamentId}");
            var tournament = await GetByIdAsync(tournamentId);
            return tournament.Status;
        }
        public async Task SetOngoingAsync(string tournamentId, string userId)
        {
            if (tournamentId == null || userId == null)
                throw new ArgumentNullException();
            if (!IsExist(tournamentId))
                throw new ArgumentException($"No tournament with id {tournamentId}");
            if (!IsAdmin(tournamentId, userId))
                throw new ArgumentException($"user is not tournament admin");
            var tournament = await GetByIdAsync(tournamentId);
            if (tournament.Status != TournamentStatus.finished )
            {
                tournament.Status = TournamentStatus.ongoing;
                await _context.SaveChangesAsync();
            }
        }
    }
    
}
