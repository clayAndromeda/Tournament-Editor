using TournamentEditor.Models;

namespace TournamentEditor.Services;

public class TournamentService
{
    private Tournament? _currentTournament;

    public Tournament? CurrentTournament => _currentTournament;

    /// <summary>
    /// 新しいトーナメントを作成
    /// </summary>
    public Tournament CreateTournament(string name, List<string> participantNames)
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
        return tournament;
    }

    /// <summary>
    /// 次のラウンドを開始
    /// </summary>
    public List<Match> StartNextRound()
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

        return newMatches;
    }

    /// <summary>
    /// 試合結果を記録
    /// </summary>
    public void RecordMatchResult(Guid matchId, MatchResult result)
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
    public void CompleteTournament()
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
    }
}
