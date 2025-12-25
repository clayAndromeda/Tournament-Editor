using Microsoft.AspNetCore.Components;
using TournamentEditor.Models;
using TournamentEditor.Services;

namespace TournamentEditor.Components.Pages;

public partial class Simulation
{
    [Inject]
    private TournamentSimulationService SimulationService { get; set; } = default!;

    private int participantCount = 8;
    private int iterations = 1000;
    private double bothLossPercent = 10.0;
    private bool isRunning = false;
    private int currentIteration = 0;
    private double progressPercentage = 0;
    private CancellationTokenSource? cancellationTokenSource;

    private SimulationResult? result;

    private bool IsProbabilityValid()
    {
        return bothLossPercent >= 0 && bothLossPercent <= 100;
    }

    private void SetDefaultProbability()
    {
        bothLossPercent = 33.0;
    }

    private void SetRealisticProbability()
    {
        bothLossPercent = 10.0;
    }

    private async Task RunSimulation()
    {
        isRunning = true;
        result = null;
        currentIteration = 0;
        progressPercentage = 0;
        cancellationTokenSource = new CancellationTokenSource();
        StateHasChanged();

        await Task.Delay(10); // UI更新のための短い遅延

        var participantNames = Enumerable.Range(1, participantCount)
            .Select(i => $"Player{i}")
            .ToList();

        var winLossPercent = (100.0 - bothLossPercent) / 2.0;
        var config = new SimulationConfig
        {
            Iterations = iterations,
            Player1WinProbability = winLossPercent / 100.0,
            Player2WinProbability = winLossPercent / 100.0,
            BothLossProbability = bothLossPercent / 100.0
        };

        try
        {
            var progress = new Progress<int>(iteration =>
            {
                currentIteration = iteration;
                progressPercentage = (double)iteration / iterations * 100.0;
                InvokeAsync(StateHasChanged);
            });

            // 非同期でシミュレーションを実行
            result = await Task.Run(() =>
                SimulationService.RunSimulation(participantNames, config, progress, cancellationTokenSource.Token),
                cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合は何もしない
            result = null;
        }
        finally
        {
            isRunning = false;
            currentIteration = 0;
            progressPercentage = 0;
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            StateHasChanged();
        }
    }

    private void CancelSimulation()
    {
        cancellationTokenSource?.Cancel();
    }
}
