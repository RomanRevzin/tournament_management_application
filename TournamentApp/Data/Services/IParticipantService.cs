using Microsoft.AspNetCore.Identity;
using TournamentApp.Data.Models;
using TournamentApp.Data.Repos;

namespace TournamentApp.Data.Services
{
    public interface IParticipantService
    {
        Task AddParticipant(string tournamentId, string userId, Role role = Role.admin);
        Task<IList<Tournament>> GetTournamentsAsync(string participantId, Role role);
        Task<IList<Participant>> GetParticipants(string tournamentId);
        Task<IList<Participant>> GetParticipants(string tournamentId, Role role);
        Task RemoveParticipant(string tournamentId, string userId);

    }
    
    public class ParticipantService : IParticipantService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public ParticipantService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        } 
        public async Task AddParticipant(string tournamentId, string userId, Role role=Role.admin)
        {
            //check that tournament exists
            if (!await _unitOfWork.TournamentRepo.IsExist(tournamentId))
                throw new InvalidOperationException($"Tournament with Id {tournamentId} does not exits");

            //check that participant isnt already in tournament
            if (await _unitOfWork.ParticipantRepo.IsExist(tournamentId, userId))
                throw new InvalidOperationException($"Participant with id {userId} already in tournament");

            //check if user exists await ;
            if (await _userManager.FindByIdAsync(userId) == null)
                throw new InvalidOperationException($"User with id {userId} doesnt exist");

            _unitOfWork.ParticipantRepo.Add(new Participant { 
                UserId = userId,
                TournamentId = tournamentId,
                PariticpantRole = role,
            });

             await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IList<Tournament>> GetTournamentsAsync(string participantId, Role role)
        {
            return await _unitOfWork.ParticipantRepo.GetParticipantTournamentsAsync(participantId,role);
        }
        public async Task<IList<Participant>> GetParticipants(string tournamentId)
        {
            return await _unitOfWork.ParticipantRepo.GetParticipantsAsync(tournamentId);
        }
        public async Task<IList<Participant>> GetParticipants(string tournamentId, Role role)
        {
            return await _unitOfWork.ParticipantRepo.GetParticipantsAsync(tournamentId,role);
        }
        public async Task RemoveParticipant(string tournamentId, string userId)
        {
            Participant participant = await _unitOfWork.ParticipantRepo.ReadAsync(userId, tournamentId);
            if (participant == null)
                throw new InvalidOperationException($"Participant with user id {userId} does not exist in tournament with id {tournamentId}");
            _unitOfWork.ParticipantRepo.Remove(participant);
            await _unitOfWork.SaveChangesAsync();
        }
    }


}
