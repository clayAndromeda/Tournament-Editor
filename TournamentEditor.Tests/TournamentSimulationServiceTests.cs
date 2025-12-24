using TournamentEditor.Models;
using TournamentEditor.Services;

namespace TournamentEditor.Tests;

public class TournamentSimulationServiceTests
{
    /// <summary>
    /// 有効な設定でシミュレーションを実行すると結果が返される
    /// </summary>
    [Fact]
    public void RunSimulation_WithValidConfig_ReturnsResult()
    {
        // Arrange - 準備
        var service = new TournamentSimulationService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        var config = new SimulationConfig
        {
            Iterations = 10,
            Player1WinProbability = 0.5,
            Player2WinProbability = 0.5,
            BothLossProbability = 0.0
        };

        // Act - 実行
        var result = service.RunSimulation(participantNames, config);

        // Assert - 検証
        Assert.NotNull(result);
        Assert.Equal(10, result.TotalIterations);
        Assert.True(result.RematchCountDistribution.Count > 0);
    }

    /// <summary>
    /// 無効な設定（確率の合計が1.0でない）でシミュレーションを実行すると例外がスローされる
    /// </summary>
    [Fact]
    public void RunSimulation_WithInvalidConfig_ThrowsException()
    {
        // Arrange - 準備
        var service = new TournamentSimulationService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        var config = new SimulationConfig
        {
            Iterations = 10,
            Player1WinProbability = 0.5,
            Player2WinProbability = 0.5,
            BothLossProbability = 0.5 // 合計が1.5で無効
        };

        // Act & Assert - 実行と検証
        Assert.Throws<ArgumentException>(() => service.RunSimulation(participantNames, config));
    }

    /// <summary>
    /// 有効な設定（確率の合計が1.0）の場合、IsValidがtrueを返す
    /// </summary>
    [Fact]
    public void SimulationConfig_IsValid_ReturnsTrueForValidConfig()
    {
        // Arrange - 準備
        var config = new SimulationConfig
        {
            Player1WinProbability = 1.0 / 3.0,
            Player2WinProbability = 1.0 / 3.0,
            BothLossProbability = 1.0 / 3.0
        };

        // Act & Assert - 実行と検証
        Assert.True(config.IsValid());
    }

    /// <summary>
    /// 無効な設定（確率の合計が1.0でない）の場合、IsValidがfalseを返す
    /// </summary>
    [Fact]
    public void SimulationConfig_IsValid_ReturnsFalseForInvalidConfig()
    {
        // Arrange - 準備
        var config = new SimulationConfig
        {
            Player1WinProbability = 0.5,
            Player2WinProbability = 0.5,
            BothLossProbability = 0.5
        };

        // Act & Assert - 実行と検証
        Assert.False(config.IsValid());
    }

    /// <summary>
    /// シミュレーション結果に再マッチ回数の分布が正しく記録される
    /// </summary>
    [Fact]
    public void RunSimulation_TracksRematchDistribution()
    {
        // Arrange - 準備
        var service = new TournamentSimulationService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4", "Player5", "Player6", "Player7", "Player8" };
        var config = new SimulationConfig
        {
            Iterations = 100,
            Player1WinProbability = 0.45,
            Player2WinProbability = 0.45,
            BothLossProbability = 0.10
        };

        // Act - 実行
        var result = service.RunSimulation(participantNames, config);

        // Assert - 検証
        Assert.NotNull(result);
        Assert.Equal(100, result.TotalIterations);

        // 再マッチ回数の分布の合計がシミュレーション回数と一致することを確認
        var totalDistribution = result.RematchCountDistribution.Values.Sum();
        Assert.Equal(100, totalDistribution);

        // 平均再マッチ回数が計算されていることを確認
        Assert.True(result.AverageRematchCount >= 0);
    }

    /// <summary>
    /// 8人のプレイヤーでシミュレーションが正常に完了する
    /// </summary>
    [Fact]
    public void RunSimulation_With8Players_CompletesSuccessfully()
    {
        // Arrange - 準備
        var service = new TournamentSimulationService();
        var participantNames = Enumerable.Range(1, 8).Select(i => $"Player{i}").ToList();
        var config = new SimulationConfig
        {
            Iterations = 50,
            Player1WinProbability = 1.0 / 3.0,
            Player2WinProbability = 1.0 / 3.0,
            BothLossProbability = 1.0 / 3.0
        };

        // Act - 実行
        var result = service.RunSimulation(participantNames, config);

        // Assert - 検証
        Assert.Equal(50, result.TotalIterations);
        Assert.True(result.MaxRematchCount >= 0);
        Assert.True(result.RematchProbability >= 0 && result.RematchProbability <= 1.0);
    }

    /// <summary>
    /// キャンセルトークンでシミュレーションをキャンセルするとOperationCanceledExceptionがスローされる
    /// </summary>
    [Fact]
    public void RunSimulation_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange - 準備
        var service = new TournamentSimulationService();
        var participantNames = Enumerable.Range(1, 16).Select(i => $"Player{i}").ToList();
        var config = new SimulationConfig
        {
            Iterations = 10000,
            Player1WinProbability = 0.45,
            Player2WinProbability = 0.45,
            BothLossProbability = 0.10
        };
        var cts = new CancellationTokenSource();
        cts.Cancel(); // すぐにキャンセル

        // Act & Assert - 実行と検証
        Assert.Throws<OperationCanceledException>(() =>
            service.RunSimulation(participantNames, config, null, cts.Token));
    }

    /// <summary>
    /// IProgressを使用してシミュレーションの進捗が正しく報告される
    /// </summary>
    [Fact]
    public void RunSimulation_WithProgress_ReportsProgress()
    {
        // Arrange - 準備
        var service = new TournamentSimulationService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        var config = new SimulationConfig
        {
            Iterations = 100,
            Player1WinProbability = 0.5,
            Player2WinProbability = 0.5,
            BothLossProbability = 0.0
        };

        var reportedProgress = new List<int>();
        var progress = new Progress<int>(iteration => reportedProgress.Add(iteration));

        // Act - 実行
        var result = service.RunSimulation(participantNames, config, progress);

        // Assert - 検証
        Assert.NotNull(result);
        Assert.True(reportedProgress.Count > 0); // 進捗が報告されている
        Assert.True(reportedProgress.Last() >= 90); // 最後の報告が90以上（10回ごとまたは最終）
        Assert.True(reportedProgress.Last() <= 100);
    }
}
