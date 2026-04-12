using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories
{
    public class TournamentSponsorRepository : GenericRepository<TournamentSponsor>, ITournamentSponsorRepository
    {
        public TournamentSponsorRepository(LeagueDbContext context) : base(context)
        {
        }

        public async Task<TournamentSponsor?> GetByTournamentAndSponsorAsync(int tournamentId, int sponsorId)
        {
            return await _dbSet
                .Where(tt => tt.TournamentId == tournamentId && tt.SponsorId == sponsorId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TournamentSponsor>> GetBySponsorAsync(
            int sponsorId)
        {
            return await _dbSet
                .Where(tt => tt.SponsorId == sponsorId)
                .Include(tt => tt.Tournament)
                .ToListAsync();
        }

        public async Task UnregisterTournamentAsync(int tournamentId, int sponsorId)
        {
            var sponsorTournament = await _context .TournamentSponsors
            .FirstOrDefaultAsync(ts => ts.TournamentId == tournamentId && ts.SponsorId == sponsorId);

            if (sponsorTournament != null)
            {
                _dbSet.Remove(sponsorTournament);
                await _context.SaveChangesAsync();
            }

        }
    }
}
