using TournamentEditor.Core.Data.Entities;
using TournamentEditor.Core.Data.Repositories;

namespace TournamentEditor.Core.Services;

/// <summary>
/// 抽選結果
/// </summary>
public class LotteryResult
{
    /// <summary>
    /// 抽選された参加者
    /// </summary>
    public List<ParticipantEntity> SelectedParticipants { get; set; } = new();

    /// <summary>
    /// 抽選日時
    /// </summary>
    public DateTime DrawnAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 抽選数
    /// </summary>
    public int RequestedCount { get; set; }

    /// <summary>
    /// 全参加者数
    /// </summary>
    public int TotalParticipants { get; set; }

    /// <summary>
    /// アクティブ参加者のみを対象にしたか
    /// </summary>
    public bool ActiveOnly { get; set; }
}

/// <summary>
/// 抽選サービス
/// </summary>
public class LotteryService
{
    private readonly IParticipantRepository _participantRepository;

    public LotteryService(IParticipantRepository participantRepository)
    {
        _participantRepository = participantRepository;
    }

    /// <summary>
    /// 参加者をランダムに抽選
    /// </summary>
    /// <param name="count">抽選する人数</param>
    /// <param name="activeOnly">アクティブな参加者のみを対象にするか</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>抽選結果</returns>
    public async Task<LotteryResult> DrawLotteryAsync(
        int count,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            throw new ArgumentException("抽選数は1以上である必要があります。", nameof(count));
        }

        var selectedParticipants = await _participantRepository.DrawRandomParticipantsAsync(
            count,
            activeOnly,
            cancellationToken);

        var totalCount = await _participantRepository.CountAsync(
            activeOnly ? p => p.IsActive : p => true,
            cancellationToken);

        return new LotteryResult
        {
            SelectedParticipants = selectedParticipants.ToList(),
            RequestedCount = count,
            TotalParticipants = totalCount,
            ActiveOnly = activeOnly,
            DrawnAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 条件付き抽選（カスタムフィルタ）
    /// </summary>
    /// <param name="count">抽選する人数</param>
    /// <param name="filter">フィルタ条件</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>抽選結果</returns>
    public async Task<LotteryResult> DrawLotteryWithFilterAsync(
        int count,
        Func<ParticipantEntity, bool> filter,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            throw new ArgumentException("抽選数は1以上である必要があります。", nameof(count));
        }

        var allParticipants = await _participantRepository.GetAllAsync(cancellationToken);
        var filteredParticipants = allParticipants.Where(filter).ToList();

        var selected = filteredParticipants
            .OrderBy(x => Random.Shared.Next())
            .Take(count)
            .ToList();

        return new LotteryResult
        {
            SelectedParticipants = selected,
            RequestedCount = count,
            TotalParticipants = filteredParticipants.Count,
            ActiveOnly = false,
            DrawnAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 複数回抽選を実行（重複なし）
    /// </summary>
    /// <param name="rounds">抽選回数</param>
    /// <param name="countPerRound">1回あたりの抽選数</param>
    /// <param name="activeOnly">アクティブな参加者のみを対象にするか</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>各ラウンドの抽選結果</returns>
    public async Task<List<LotteryResult>> DrawMultipleRoundsAsync(
        int rounds,
        int countPerRound,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        if (rounds <= 0)
        {
            throw new ArgumentException("抽選回数は1以上である必要があります。", nameof(rounds));
        }

        if (countPerRound <= 0)
        {
            throw new ArgumentException("1回あたりの抽選数は1以上である必要があります。", nameof(countPerRound));
        }

        var results = new List<LotteryResult>();
        var excludedIds = new HashSet<Guid>();

        for (int i = 0; i < rounds; i++)
        {
            var allParticipants = await _participantRepository.GetAllAsync(cancellationToken);
            var availableParticipants = allParticipants
                .Where(p => !excludedIds.Contains(p.Id))
                .Where(p => !activeOnly || p.IsActive)
                .ToList();

            if (availableParticipants.Count == 0)
            {
                break;
            }

            var selected = availableParticipants
                .OrderBy(x => Random.Shared.Next())
                .Take(Math.Min(countPerRound, availableParticipants.Count))
                .ToList();

            foreach (var participant in selected)
            {
                excludedIds.Add(participant.Id);
            }

            results.Add(new LotteryResult
            {
                SelectedParticipants = selected,
                RequestedCount = countPerRound,
                TotalParticipants = availableParticipants.Count,
                ActiveOnly = activeOnly,
                DrawnAt = DateTime.UtcNow
            });
        }

        return results;
    }
}
