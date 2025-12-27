using TournamentEditor.Core.Data.Entities;
using TournamentEditor.Core.Data.Repositories;

namespace TournamentEditor.Core.Services;

/// <summary>
/// 参加者管理サービス
/// </summary>
public class ParticipantManagementService
{
    private readonly IParticipantRepository _participantRepository;

    public ParticipantManagementService(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository;
    }

    /// <summary>
    /// 参加者を登録
    /// </summary>
    public async Task<ParticipantEntity> RegisterParticipantAsync(
        string name,
        string? email = null,
        string? phoneNumber = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var participant = new ParticipantEntity
        {
            Name = name,
            Email = email,
            PhoneNumber = phoneNumber,
            Notes = notes,
            RegisteredAt = DateTime.UtcNow
        };

        return await _participantRepository.AddAsync(participant, cancellationToken);
    }

    /// <summary>
    /// 複数の参加者を一括登録
    /// </summary>
    public async Task RegisterParticipantsAsync(
        IEnumerable<string> names,
        CancellationToken cancellationToken = default)
    {
        var participants = names.Select(name => new ParticipantEntity
        {
            Name = name,
            RegisteredAt = DateTime.UtcNow
        });

        await _participantRepository.AddRangeAsync(participants, cancellationToken);
    }

    /// <summary>
    /// 参加者情報を更新
    /// </summary>
    public async Task<ParticipantEntity?> UpdateParticipantAsync(
        Guid id,
        string? name = null,
        string? email = null,
        string? phoneNumber = null,
        string? notes = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var participant = await _participantRepository.GetByIdAsync(id, cancellationToken);
        if (participant == null)
        {
            return null;
        }

        if (name != null) participant.Name = name;
        if (email != null) participant.Email = email;
        if (phoneNumber != null) participant.PhoneNumber = phoneNumber;
        if (notes != null) participant.Notes = notes;
        if (isActive.HasValue) participant.IsActive = isActive.Value;

        await _participantRepository.UpdateAsync(participant, cancellationToken);
        return participant;
    }

    /// <summary>
    /// 参加者を削除
    /// </summary>
    public async Task DeleteParticipantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _participantRepository.DeleteByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// 参加者を取得
    /// </summary>
    public async Task<ParticipantEntity?> GetParticipantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _participantRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// すべての参加者を取得
    /// </summary>
    public async Task<IEnumerable<ParticipantEntity>> GetAllParticipantsAsync(CancellationToken cancellationToken = default)
    {
        return await _participantRepository.GetAllAsync(cancellationToken);
    }

    /// <summary>
    /// アクティブな参加者を取得
    /// </summary>
    public async Task<IEnumerable<ParticipantEntity>> GetActiveParticipantsAsync(CancellationToken cancellationToken = default)
    {
        return await _participantRepository.GetActiveParticipantsAsync(cancellationToken);
    }

    /// <summary>
    /// 名前で参加者を検索
    /// </summary>
    public async Task<IEnumerable<ParticipantEntity>> SearchParticipantsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _participantRepository.SearchByNameAsync(name, cancellationToken);
    }

    /// <summary>
    /// 最近登録された参加者を取得
    /// </summary>
    public async Task<IEnumerable<ParticipantEntity>> GetRecentParticipantsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _participantRepository.GetRecentParticipantsAsync(count, cancellationToken);
    }

    /// <summary>
    /// 参加者数を取得
    /// </summary>
    public async Task<int> GetParticipantCountAsync(bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        if (activeOnly)
        {
            return await _participantRepository.CountAsync(p => p.IsActive, cancellationToken);
        }
        return await _participantRepository.CountAsync(cancellationToken);
    }
}
