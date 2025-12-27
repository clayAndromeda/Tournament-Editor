using Microsoft.EntityFrameworkCore;
using TournamentEditor.Core.Data.Entities;

namespace TournamentEditor.Core.Data.Repositories;

/// <summary>
/// トーナメントリポジトリ実装
/// </summary>
public class TournamentRepository : Repository<TournamentEntity>, ITournamentRepository
{
    public TournamentRepository(TournamentDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TournamentEntity>> GetUserTournamentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TournamentEntity>> GetUserActiveTournamentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && !t.IsComplete)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TournamentEntity?> GetUserLatestTournamentAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public override async Task<TournamentEntity> AddAsync(TournamentEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await base.AddAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(TournamentEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(entity, cancellationToken);
    }
}
