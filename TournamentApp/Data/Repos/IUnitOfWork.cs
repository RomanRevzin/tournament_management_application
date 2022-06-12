namespace TournamentApp.Data.Repos
{
    public interface IUnitOfWork
    {
        ITournamentRepo TournamentRepo { get; }
        IParticipantRepo ParticipantRepo { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TournamentAppDbContext _dbContext;
        private readonly Lazy<ITournamentRepo> _tournamentRepo;
        private readonly Lazy<IParticipantRepo> _participantRepo;

        public UnitOfWork(TournamentAppDbContext dbContext)
        {
            _dbContext = dbContext;
            _tournamentRepo = new Lazy<ITournamentRepo>(() => new TournamentRepo(dbContext));
            _participantRepo = new Lazy<IParticipantRepo>(() => new ParticipantRepo(dbContext));
        }
      
        public ITournamentRepo TournamentRepo => _tournamentRepo.Value;
        public IParticipantRepo ParticipantRepo => _participantRepo.Value;

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _dbContext.SaveChangesAsync(cancellationToken);
    }
}
