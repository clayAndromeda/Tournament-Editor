using TournamentEditor.Models;

namespace TournamentEditor.Services;

/// <summary>
/// トーナメントシミュレーションサービス
/// </summary>
public class TournamentSimulationService
{
    private readonly Random _random = new();

    /// <summary>
    /// トーナメントをシミュレーションして再マッチの統計を取得
    /// </summary>
    public SimulationResult RunSimulation(
        List<string> participantNames,
        SimulationConfig config,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!config.IsValid())
        {
            throw new ArgumentException("シミュレーション設定が無効です。確率の合計は1.0である必要があります。");
        }

        var result = new SimulationResult
        {
            TotalIterations = config.Iterations
        };

        int totalRematchCount = 0;
        var allRematchCounts = new List<int>();

        // 各シミュレーションを実行
        for (int i = 0; i < config.Iterations; i++)
        {
            // キャンセルチェック
            cancellationToken.ThrowIfCancellationRequested();

            var simulationRematchData = SimulateSingleTournament(participantNames, config);
            int rematchCount = simulationRematchData.RematchCount;

            totalRematchCount += rematchCount;
            allRematchCounts.Add(rematchCount);

            // 再マッチ回数の分布を更新
            if (!result.RematchCountDistribution.ContainsKey(rematchCount))
            {
                result.RematchCountDistribution[rematchCount] = 0;
            }
            result.RematchCountDistribution[rematchCount]++;

            // 再マッチが発生した場合
            if (rematchCount > 0)
            {
                result.RematchOccurrences++;

                // 再マッチペアを記録
                foreach (var pair in simulationRematchData.RematchPairs)
                {
                    if (!result.RematchPairs.ContainsKey(pair))
                    {
                        result.RematchPairs[pair] = 0;
                    }
                    result.RematchPairs[pair]++;
                }
            }

            // ラウンドごとの再マッチを集計
            foreach (var kvp in simulationRematchData.RematchesByRound)
            {
                if (!result.RematchesByRound.ContainsKey(kvp.Key))
                {
                    result.RematchesByRound[kvp.Key] = 0;
                }
                result.RematchesByRound[kvp.Key] += kvp.Value;
            }

            // 進捗を報告（10回ごと、または最後）
            if ((i + 1) % 10 == 0 || i == config.Iterations - 1)
            {
                progress?.Report(i + 1);
            }
        }

        // 統計情報を計算
        result.AverageRematchCount = config.Iterations > 0 ? (double)totalRematchCount / config.Iterations : 0;
        result.MaxRematchCount = allRematchCounts.Count > 0 ? allRematchCounts.Max() : 0;

        return result;
    }

    /// <summary>
    /// 単一のトーナメントをシミュレーション
    /// </summary>
    private SimulationRematchData SimulateSingleTournament(
        List<string> participantNames,
        SimulationConfig config)
    {
        // トーナメントを作成
        var tournament = CreateTournament(participantNames);

        // すべてのラウンドを実行
        for (int round = 1; round <= tournament.TotalRounds; round++)
        {
            var matches = SwissPairingService.GeneratePairings(tournament, round);
            tournament.Matches.AddRange(matches);

            // 各試合にランダムな結果を割り当て
            foreach (var match in matches)
            {
                match.Result = GenerateRandomResult(config);
                SwissPairingService.UpdateParticipantStats(tournament, match);
            }

            SwissPairingService.CalculateSonnebornBerger(tournament);
        }

        // 再マッチを検出
        return DetectRematches(tournament);
    }

    /// <summary>
    /// トーナメントを作成（TournamentServiceを使わずに直接作成）
    /// </summary>
    private Tournament CreateTournament(List<string> participantNames)
    {
        if (!SwissPairingService.IsPowerOfTwo(participantNames.Count))
        {
            throw new ArgumentException("参加者数は2の冪乗である必要があります。");
        }

        var tournament = new Tournament
        {
            Name = "Simulation",
            TotalRounds = SwissPairingService.CalculateRounds(participantNames.Count),
            CurrentRound = 0
        };

        for (int i = 0; i < participantNames.Count; i++)
        {
            tournament.Participants.Add(new Participant
            {
                Name = participantNames[i],
                DisplayOrder = i + 1
            });
        }

        return tournament;
    }

    /// <summary>
    /// 設定に基づいてランダムな試合結果を生成
    /// </summary>
    private MatchResult GenerateRandomResult(SimulationConfig config)
    {
        var random = _random.NextDouble();

        if (random < config.Player1WinProbability)
        {
            return MatchResult.Player1Win;
        }
        else if (random < config.Player1WinProbability + config.Player2WinProbability)
        {
            return MatchResult.Player2Win;
        }
        else
        {
            return MatchResult.BothLoss;
        }
    }

    /// <summary>
    /// トーナメント内の再マッチを検出
    /// </summary>
    private SimulationRematchData DetectRematches(Tournament tournament)
    {
        var data = new SimulationRematchData();
        var matchupHistory = new Dictionary<string, List<int>>(); // ペア -> ラウンド番号のリスト

        // ラウンドごとに試合をチェック
        var roundNumbers = tournament.Matches.Select(m => m.RoundNumber).Distinct().OrderBy(r => r);

        foreach (var roundNumber in roundNumbers)
        {
            var roundMatches = tournament.Matches.Where(m => m.RoundNumber == roundNumber);
            int rematchesInRound = 0;

            foreach (var match in roundMatches)
            {
                if (match.Player1Id == null || match.Player2Id == null)
                {
                    continue;
                }

                var player1 = tournament.Participants.First(p => p.Id == match.Player1Id);
                var player2 = tournament.Participants.First(p => p.Id == match.Player2Id);

                // ペア識別子を作成（名前順で正規化）
                var pairKey = CreatePairKey(player1.Name, player2.Name);

                if (!matchupHistory.ContainsKey(pairKey))
                {
                    matchupHistory[pairKey] = new List<int>();
                }
                else
                {
                    // このペアは過去に対戦済み = 再マッチ
                    rematchesInRound++;
                    data.RematchCount++;
                    if (!data.RematchPairs.Contains(pairKey))
                    {
                        data.RematchPairs.Add(pairKey);
                    }
                }

                matchupHistory[pairKey].Add(roundNumber);
            }

            data.RematchesByRound[roundNumber] = rematchesInRound;
        }

        return data;
    }

    /// <summary>
    /// ペア識別子を作成（名前順で正規化）
    /// </summary>
    private string CreatePairKey(string name1, string name2)
    {
        var names = new[] { name1, name2 }.OrderBy(n => n).ToArray();
        return $"{names[0]} vs {names[1]}";
    }

    /// <summary>
    /// 単一シミュレーションの再マッチデータ
    /// </summary>
    private class SimulationRematchData
    {
        public int RematchCount { get; set; }
        public List<string> RematchPairs { get; set; } = new();
        public Dictionary<int, int> RematchesByRound { get; set; } = new();
    }
}
