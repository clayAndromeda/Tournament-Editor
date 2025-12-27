using TournamentEditor.Core.Services;
using TournamentEditor.Models;

namespace TournamentEditor.Services;

/// <summary>
/// トーナメントサービス（DB永続化対応版）
/// </summary>
public class TournamentServiceV2
{
    private readonly TournamentPersistenceService _persistenceService;
    private readonly UserContextService _userContextService;
    private Tournament? _currentTournament;

    public TournamentServiceV2(
        TournamentPersistenceService persistenceService,
        UserContextService userContextService)
    {
        _persistenceService = persistenceService;
        _userContextService = userContextService;
    }

    private string UserId => _userContextService.GetUserId();

    public Tournament? CurrentTournament => _currentTournament;

    /// <summary>
    /// 新しいトーナメントを作成
    /// </summary>
    public async Task<Tournament> CreateTournamentAsync(string name, List<string> participantNames)
    {
        if (!SwissPairingService.IsPowerOfTwo(participantNames.Count))
        {
            throw new ArgumentException("参加者数は2の冪乗（2, 4, 8, 16, 32, 64, 128, 256）である必要があります。");
        }

        if (participantNames.Count > 256)
        {
            throw new ArgumentException("参加者数は256人以下である必要があります。");
        }

        var tournament = new Tournament
        {
            Name = name,
            TotalRounds = SwissPairingService.CalculateRounds(participantNames.Count),
            CurrentRound = 0
        };

        // 参加者を作成
        for (int i = 0; i < participantNames.Count; i++)
        {
            tournament.Participants.Add(new Participant
            {
                Name = participantNames[i],
                DisplayOrder = i + 1
            });
        }

        _currentTournament = tournament;

        // DBに保存
        await _persistenceService.SaveTournamentAsync(tournament, UserId);

        return tournament;
    }

    /// <summary>
    /// トーナメントを読み込み
    /// </summary>
    public async Task<Tournament?> LoadTournamentAsync(Guid tournamentId)
    {
        var tournament = await _persistenceService.LoadTournamentAsync(tournamentId);
        _currentTournament = tournament;
        return tournament;
    }

    /// <summary>
    /// 最新のトーナメントを読み込み
    /// </summary>
    public async Task<Tournament?> LoadLatestTournamentAsync()
    {
        var tournament = await _persistenceService.GetUserLatestTournamentAsync(UserId);
        _currentTournament = tournament;
        return tournament;
    }

    /// <summary>
    /// 次のラウンドを開始
    /// </summary>
    public async Task<List<Match>> StartNextRoundAsync()
    {
        if (_currentTournament == null)
        {
            throw new InvalidOperationException("トーナメントが作成されていません。");
        }

        if (_currentTournament.CurrentRound >= _currentTournament.TotalRounds)
        {
            throw new InvalidOperationException("すべてのラウンドが完了しています。");
        }

        _currentTournament.CurrentRound++;
        var newMatches = SwissPairingService.GeneratePairings(_currentTournament, _currentTournament.CurrentRound);
        _currentTournament.Matches.AddRange(newMatches);

        // DBに保存
        await _persistenceService.SaveTournamentAsync(_currentTournament, UserId);

        return newMatches;
    }

    /// <summary>
    /// 試合結果を記録
    /// </summary>
    public async Task RecordMatchResultAsync(Guid matchId, MatchResult result)
    {
        if (_currentTournament == null)
        {
            throw new InvalidOperationException("トーナメントが作成されていません。");
        }

        var match = _currentTournament.Matches.FirstOrDefault(m => m.Id == matchId);
        if (match == null)
        {
            throw new ArgumentException("試合が見つかりません。", nameof(matchId));
        }

        match.Result = result;
        match.CompletedAt = DateTime.UtcNow;

        SwissPairingService.UpdateParticipantStats(_currentTournament, match);
        SwissPairingService.CalculateSonnebornBerger(_currentTournament);

        // DBに保存
        await _persistenceService.SaveTournamentAsync(_currentTournament, UserId);
    }

    /// <summary>
    /// 現在のラウンドが完了しているか確認
    /// </summary>
    public bool IsCurrentRoundComplete()
    {
        if (_currentTournament == null)
        {
            return false;
        }

        var currentRoundMatches = _currentTournament.Matches
            .Where(m => m.RoundNumber == _currentTournament.CurrentRound);

        return currentRoundMatches.All(m => m.Result != MatchResult.NotPlayed);
    }

    /// <summary>
    /// 順位表を取得
    /// </summary>
    public List<Participant> GetStandings()
    {
        if (_currentTournament == null)
        {
            return new List<Participant>();
        }

        return _currentTournament.Participants
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Points)
            .ThenByDescending(p => p.SonnebornBerger)
            .ThenBy(p => p.DisplayOrder)
            .ToList();
    }

    /// <summary>
    /// トーナメントが完了しているか確認
    /// </summary>
    public bool IsTournamentComplete()
    {
        if (_currentTournament == null)
        {
            return false;
        }

        return _currentTournament.CurrentRound >= _currentTournament.TotalRounds &&
               IsCurrentRoundComplete();
    }

    /// <summary>
    /// トーナメントを完了としてマーク
    /// </summary>
    public async Task CompleteTournamentAsync()
    {
        if (_currentTournament == null)
        {
            throw new InvalidOperationException("トーナメントが作成されていません。");
        }

        if (!IsTournamentComplete())
        {
            throw new InvalidOperationException("すべてのラウンドが完了していません。");
        }

        _currentTournament.IsComplete = true;

        // DBに保存
        await _persistenceService.SaveTournamentAsync(_currentTournament, UserId);
    }

    /// <summary>
    /// ユーザーのトーナメント一覧を取得
    /// </summary>
    public async Task<IEnumerable<Core.Data.Entities.TournamentEntity>> GetUserTournamentsAsync()
    {
        return await _persistenceService.GetUserTournamentsAsync(UserId);
    }
}
