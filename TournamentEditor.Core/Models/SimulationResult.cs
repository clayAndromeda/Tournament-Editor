namespace TournamentEditor.Models;

/// <summary>
/// シミュレーション結果
/// </summary>
public class SimulationResult
{
    /// <summary>
    /// シミュレーション実行回数
    /// </summary>
    public int TotalIterations { get; set; }

    /// <summary>
    /// 再マッチが発生したシミュレーション回数
    /// </summary>
    public int RematchOccurrences { get; set; }

    /// <summary>
    /// 再マッチ発生率 (0.0 - 1.0)
    /// </summary>
    public double RematchProbability => TotalIterations > 0 ? (double)RematchOccurrences / TotalIterations : 0;

    /// <summary>
    /// 再マッチ回数の分布
    /// Key: 再マッチが発生した回数 (0, 1, 2, ...)
    /// Value: その回数が発生したシミュレーション数
    /// </summary>
    public Dictionary<int, int> RematchCountDistribution { get; set; } = new();

    /// <summary>
    /// 再マッチが発生したペアのリスト（頻度順）
    /// Key: ペア識別子（例: "Player1 vs Player2"）
    /// Value: 再マッチが発生したシミュレーション数
    /// </summary>
    public Dictionary<string, int> RematchPairs { get; set; } = new();

    /// <summary>
    /// 平均再マッチ回数
    /// </summary>
    public double AverageRematchCount { get; set; }

    /// <summary>
    /// 最大再マッチ回数
    /// </summary>
    public int MaxRematchCount { get; set; }

    /// <summary>
    /// ラウンドごとの再マッチ発生数
    /// Key: ラウンド番号 (1, 2, 3, ...)
    /// Value: そのラウンドで発生した再マッチの合計数（全シミュレーション合計）
    /// </summary>
    public Dictionary<int, int> RematchesByRound { get; set; } = new();
}
