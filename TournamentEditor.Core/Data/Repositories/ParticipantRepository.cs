using Microsoft.EntityFrameworkCore;
using TournamentEditor.Core.Data.Entities;

namespace TournamentEditor.Core.Data.Repositories;

/// <summary>
/// 参加者リポジトリ実装
/// </summary>
public class ParticipantRepository : Repository<ParticipantEntity>, IParticipantRepository
{
    public ParticipantRepository(TournamentDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ParticipantEntity>> GetActiveParticipantsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ParticipantEntity>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ParticipantEntity>> GetRecentParticipantsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(p => p.RegisteredAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ParticipantEntity>> DrawRandomParticipantsAsync(int count, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var allParticipants = await query.ToListAsync(cancellationToken);

        if (allParticipants.Count <= count)
        {
            return allParticipants;
        }

        // Fisher-Yatesシャッフルアルゴリズムでランダム抽選（スレッドセーフ）
        var shuffled = allParticipants.OrderBy(x => Random.Shared.Next()).ToList();
        return shuffled.Take(count);
    }

    public override async Task<ParticipantEntity> AddAsync(ParticipantEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        return await base.AddAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(ParticipantEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(entity, cancellationToken);
    }
}
