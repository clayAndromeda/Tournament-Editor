namespace TournamentEditor.Core.Data.Entities;

/// <summary>
/// 参加者エンティティ（データベース用）
/// </summary>
public class ParticipantEntity
{
    /// <summary>
    /// 参加者ID（主キー）
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 参加者名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 登録日時
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// メールアドレス（オプション）
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 電話番号（オプション）
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// メモ（オプション）
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// アクティブ状態
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
