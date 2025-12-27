using TournamentEditor.Core.Data.Entities;

namespace TournamentEditor.Core.Data.Repositories;

/// <summary>
/// 参加者リポジトリインターフェース
/// </summary>
public interface IParticipantRepository : IRepository<ParticipantEntity>
{
    /// <summary>
    /// アクティブな参加者を取得
    /// </summary>
    Task<IEnumerable<ParticipantEntity>> GetActiveParticipantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 名前で参加者を検索
    /// </summary>
    Task<IEnumerable<ParticipantEntity>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// 最近登録された参加者を取得
    /// </summary>
    Task<IEnumerable<ParticipantEntity>> GetRecentParticipantsAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// 参加者をランダムに抽選
    /// </summary>
    Task<IEnumerable<ParticipantEntity>> DrawRandomParticipantsAsync(int count, bool activeOnly = true, CancellationToken cancellationToken = default);
}
