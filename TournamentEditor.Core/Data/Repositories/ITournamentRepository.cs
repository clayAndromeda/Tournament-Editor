using TournamentEditor.Core.Data.Entities;

namespace TournamentEditor.Core.Data.Repositories;

/// <summary>
/// トーナメントリポジトリインターフェース
/// </summary>
public interface ITournamentRepository : IRepository<TournamentEntity>
{
    /// <summary>
    /// ユーザーのトーナメント一覧を取得
    /// </summary>
    Task<IEnumerable<TournamentEntity>> GetUserTournamentsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ユーザーのアクティブなトーナメントを取得
    /// </summary>
    Task<IEnumerable<TournamentEntity>> GetUserActiveTournamentsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ユーザーの最新トーナメントを取得
    /// </summary>
    Task<TournamentEntity?> GetUserLatestTournamentAsync(string userId, CancellationToken cancellationToken = default);
}
