using System.Linq.Expressions;

namespace TournamentEditor.Core.Data.Repositories;

/// <summary>
/// 汎用リポジトリインターフェース
/// </summary>
/// <typeparam name="T">エンティティ型</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// IDで単一エンティティを取得
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// すべてのエンティティを取得
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 条件に一致するエンティティを取得
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを追加
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のエンティティを追加
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを更新
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを削除
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// IDでエンティティを削除
    /// </summary>
    Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティ数を取得
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 条件に一致するエンティティ数を取得
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティが存在するか確認
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
