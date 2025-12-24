namespace TournamentEditor.Models;

/// <summary>
/// シミュレーションの設定
/// </summary>
public class SimulationConfig
{
    /// <summary>
    /// シミュレーション実行回数
    /// </summary>
    public int Iterations { get; set; } = 1000;

    /// <summary>
    /// Player1勝利の確率 (0.0 - 1.0)
    /// </summary>
    public double Player1WinProbability { get; set; } = 1.0 / 3.0;

    /// <summary>
    /// Player2勝利の確率 (0.0 - 1.0)
    /// </summary>
    public double Player2WinProbability { get; set; } = 1.0 / 3.0;

    /// <summary>
    /// 両負けの確率 (0.0 - 1.0)
    /// </summary>
    public double BothLossProbability { get; set; } = 1.0 / 3.0;

    /// <summary>
    /// 設定が有効かどうかを検証
    /// </summary>
    public bool IsValid()
    {
        var total = Player1WinProbability + Player2WinProbability + BothLossProbability;
        return Math.Abs(total - 1.0) < 0.0001 &&
               Player1WinProbability >= 0 &&
               Player2WinProbability >= 0 &&
               BothLossProbability >= 0;
    }
}
