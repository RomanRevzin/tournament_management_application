using System.Linq.Expressions;
using TournamentApp.Areas.Tournaments.Pages;
using TournamentApp.Data.Models;

namespace TournamentApp.Data.Repos
{
    public interface ITournamentRepo : IRepo<Tournament,string>
    {
        Task<bool> IsExist(string tournamentId);
    }

    public class TournamentRepo : Repo<Tournament, string>, ITournamentRepo
    {
        public TournamentRepo(TournamentAppDbContext context):base(context)
        {
        }

        public async Task<bool> IsExist(string tournamentId)
        {
            Expression<Func<Tournament, bool>> filter = p => p.TournamentId == tournamentId;
            return await IsExist(filter);
        }

    }
}
