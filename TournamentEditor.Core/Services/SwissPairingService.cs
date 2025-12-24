using TournamentEditor.Models;

namespace TournamentEditor.Services;

public class SwissPairingService
{
    /// <summary>
    /// スイス式トーナメントのラウンド数を計算
    /// </summary>
    public static int CalculateRounds(int participantCount)
    {
        if (participantCount <= 0 || !IsPowerOfTwo(participantCount))
        {
            throw new ArgumentException("参加者数は2の冪乗である必要があります。", nameof(participantCount));
        }

        if (participantCount > 256)
        {
            throw new ArgumentException("参加者数は256人以下である必要があります。", nameof(participantCount));
        }

        return (int)Math.Log2(participantCount);
    }

    /// <summary>
    /// 2の冪乗かどうかを判定
    /// </summary>
    public static bool IsPowerOfTwo(int n)
    {
        return n > 0 && (n & (n - 1)) == 0;
    }

    /// <summary>
    /// 次のラウンドのペアリングを生成（完全スイス式）
    /// </summary>
    public static List<Match> GeneratePairings(Tournament tournament, int roundNumber)
    {
        var activePlayers = tournament.Participants
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Points)
            .ThenByDescending(p => p.SonnebornBerger)
            .ThenBy(p => p.DisplayOrder)
            .ToList();

        if (activePlayers.Count % 2 != 0)
        {
            throw new InvalidOperationException("アクティブな参加者数が奇数です。スイス式では偶数が必要です。");
        }

        var matches = new List<Match>();
        var paired = new HashSet<Guid>();
        int matchNumber = 1;

        // すべてのプレイヤーをペアリングするまで繰り返す
        while (paired.Count < activePlayers.Count)
        {
            // まだペアになっていないプレイヤーを取得
            var unpaired = activePlayers.Where(p => !paired.Contains(p.Id)).ToList();

            if (unpaired.Count < 2)
            {
                break;
            }

            var player1 = unpaired[0];
            Participant? player2 = null;

            // 過去に対戦していない相手を探す（最も近いランク）
            for (int i = 1; i < unpaired.Count; i++)
            {
                var candidate = unpaired[i];
                if (!player1.OpponentIds.Contains(candidate.Id))
                {
                    player2 = candidate;
                    break;
                }
            }

            // どうしても見つからない場合のみ、過去の対戦相手とマッチ（再マッチ）
            if (player2 == null && unpaired.Count >= 2)
            {
                player2 = unpaired[1];
            }

            if (player2 != null)
            {
                matches.Add(new Match
                {
                    RoundNumber = roundNumber,
                    MatchNumber = matchNumber++,
                    Player1Id = player1.Id,
                    Player2Id = player2.Id
                });

                paired.Add(player1.Id);
                paired.Add(player2.Id);
            }
            else
            {
                break;
            }
        }

        return matches;
    }

    /// <summary>
    /// 試合結果を反映して参加者のスタッツを更新
    /// </summary>
    public static void UpdateParticipantStats(Tournament tournament, Match match)
    {
        if (match.Result == MatchResult.NotPlayed ||
            match.Player1Id == null ||
            match.Player2Id == null)
        {
            return;
        }

        var player1 = tournament.Participants.FirstOrDefault(p => p.Id == match.Player1Id);
        var player2 = tournament.Participants.FirstOrDefault(p => p.Id == match.Player2Id);

        if (player1 == null || player2 == null)
        {
            return;
        }

        // 対戦履歴を記録
        if (!player1.OpponentIds.Contains(player2.Id))
        {
            player1.OpponentIds.Add(player2.Id);
        }
        if (!player2.OpponentIds.Contains(player1.Id))
        {
            player2.OpponentIds.Add(player1.Id);
        }

        // 結果に応じてスタッツを更新
        switch (match.Result)
        {
            case MatchResult.Player1Win:
                player1.Wins++;
                player1.Points += 1;
                player2.Losses++;
                break;

            case MatchResult.Player2Win:
                player2.Wins++;
                player2.Points += 1;
                player1.Losses++;
                break;

            case MatchResult.BothLoss:
                player1.Losses++;
                player2.Losses++;
                break;
        }
    }

    /// <summary>
    /// ソネボーン・ベルガー方式でタイブレークスコアを計算
    /// </summary>
    public static void CalculateSonnebornBerger(Tournament tournament)
    {
        foreach (var participant in tournament.Participants)
        {
            decimal sbScore = 0;

            foreach (var opponentId in participant.OpponentIds)
            {
                var opponent = tournament.Participants.FirstOrDefault(p => p.Id == opponentId);
                if (opponent != null)
                {
                    // その対戦相手との試合結果を取得
                    var match = tournament.Matches.FirstOrDefault(m =>
                        (m.Player1Id == participant.Id && m.Player2Id == opponentId) ||
                        (m.Player2Id == participant.Id && m.Player1Id == opponentId));

                    if (match != null)
                    {
                        bool won = (match.Player1Id == participant.Id && match.Result == MatchResult.Player1Win) ||
                                   (match.Player2Id == participant.Id && match.Result == MatchResult.Player2Win);

                        if (won)
                        {
                            sbScore += opponent.Points;
                        }
                    }
                }
            }

            participant.SonnebornBerger = sbScore;
        }
    }
}
