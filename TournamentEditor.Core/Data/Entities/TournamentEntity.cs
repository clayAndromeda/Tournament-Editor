namespace TournamentEditor.Core.Data.Entities;

/// <summary>
/// トーナメントエンティティ（データベース用）
/// </summary>
public class TournamentEntity
{
    /// <summary>
    /// トーナメントID（主キー）
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// トーナメント名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ユーザーID（所有者）
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 総ラウンド数
    /// </summary>
    public int TotalRounds { get; set; }

    /// <summary>
    /// 現在のラウンド番号
    /// </summary>
    public int CurrentRound { get; set; }

    /// <summary>
    /// 完了フラグ
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 参加者リスト（JSON形式で保存）
    /// </summary>
    public string ParticipantsJson { get; set; } = "[]";

    /// <summary>
    /// 試合リスト（JSON形式で保存）
    /// </summary>
    public string MatchesJson { get; set; } = "[]";
}
