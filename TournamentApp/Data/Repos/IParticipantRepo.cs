using System.Linq.Expressions;
using TournamentApp.Data.Models;

namespace TournamentApp.Data.Repos
{
    public interface IParticipantRepo : IRepo<Participant, string>
    {
        Task<IList<Participant>>  GetParticipantsAsync(string tournamentId);
        Task<IList<Participant>> GetParticipantsAsync(string tournamentId, Role role);
        Task<IList<Tournament>> GetParticipantTournamentsAsync(string participantId, Role role);
        Task<bool> IsExist(string tournamentID, string participantId);
        
    }

    public class ParticipantRepo :  Repo<Participant, string>, IParticipantRepo
    {
        public ParticipantRepo(TournamentAppDbContext dbContext):base(dbContext)
        {
        }
        public async Task<IList<Participant>> GetParticipantsAsync(string tournamentId)
        {
            Expression<Func<Participant, bool>> filter = p => p.TournamentId == tournamentId;
            return await EagerReadAllAsync(filter,"ApplicationUser");
        }
        public async Task<IList<Participant>> GetParticipantsAsync(string tournamentId, Role role)
        {
            Expression<Func<Participant, bool>> filter = p => p.TournamentId == tournamentId && p.PariticpantRole == role;
            return await EagerReadAllAsync(filter, "ApplicationUser");
        }
        public async Task<IList<Tournament>> GetParticipantTournamentsAsync(string participantId, Role role)
        {
            Expression<Func<Participant, bool>> filter = p => p.UserId == participantId && p.PariticpantRole == role;
            var fullParticipant = await EagerReadAllAsync(filter, "Tournamets");
            return fullParticipant.Select(p=> p.Tournament).ToList();
        }
        public async Task<bool> IsExist(string tournamentId, string userId)
        {
            Expression<Func<Participant, bool>> filter = p => p.TournamentId == tournamentId && p.UserId == userId;
            return await IsExist(filter);
        }
    }
}
