using TournamentEditor.Models;
using TournamentEditor.Services;

namespace TournamentEditor.Tests;

public class TournamentServiceTests
{
    /// <summary>
    /// 有効な入力でトーナメントが正常に作成されることを確認
    /// </summary>
    [Fact]
    public void CreateTournament_WithValidInput_CreatesTournamentSuccessfully()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };

        // Act
        var tournament = service.CreateTournament("Test Tournament", participantNames);

        // Assert
        Assert.NotNull(tournament);
        Assert.Equal("Test Tournament", tournament.Name);
        Assert.Equal(4, tournament.Participants.Count);
        Assert.Equal(2, tournament.TotalRounds); // log2(4) = 2
        Assert.Equal(0, tournament.CurrentRound);
    }

    /// <summary>
    /// 無効な参加者数でArgumentExceptionが発生することを確認
    /// </summary>
    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void CreateTournament_WithInvalidParticipantCount_ThrowsArgumentException(int count)
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = Enumerable.Range(1, count).Select(i => $"Player{i}").ToList();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.CreateTournament("Test Tournament", participantNames));
    }

    /// <summary>
    /// 第1ラウンド開始時に試合が正しく作成されることを確認
    /// </summary>
    [Fact]
    public void StartNextRound_FirstRound_CreatesMatches()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        service.CreateTournament("Test Tournament", participantNames);

        // Act
        var matches = service.StartNextRound();

        // Assert
        Assert.NotNull(matches);
        Assert.Equal(2, matches.Count);
        Assert.Equal(1, service.CurrentTournament!.CurrentRound);
    }

    /// <summary>
    /// 試合結果の記録時に参加者の統計情報が更新されることを確認
    /// </summary>
    [Fact]
    public void RecordMatchResult_UpdatesParticipantStats()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        service.CreateTournament("Test Tournament", participantNames);
        var matches = service.StartNextRound();
        var firstMatch = matches.First();

        // Act
        service.RecordMatchResult(firstMatch.Id, MatchResult.Player1Win);

        // Assert
        Assert.Equal(MatchResult.Player1Win, firstMatch.Result);
        Assert.NotNull(firstMatch.CompletedAt);

        var player1 = service.CurrentTournament!.Participants
            .First(p => p.Id == firstMatch.Player1Id);
        Assert.Equal(1, player1.Wins);
        Assert.Equal(1, player1.Points);
    }

    /// <summary>
    /// 全試合が終了した場合、現在のラウンドが完了とみなされることを確認
    /// </summary>
    [Fact]
    public void IsCurrentRoundComplete_AllMatchesPlayed_ReturnsTrue()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        service.CreateTournament("Test Tournament", participantNames);
        var matches = service.StartNextRound();

        // Act
        foreach (var match in matches)
        {
            service.RecordMatchResult(match.Id, MatchResult.Player1Win);
        }

        // Assert
        Assert.True(service.IsCurrentRoundComplete());
    }

    /// <summary>
    /// 一部の試合が未実施の場合、現在のラウンドが未完了とみなされることを確認
    /// </summary>
    [Fact]
    public void IsCurrentRoundComplete_SomeMatchesNotPlayed_ReturnsFalse()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        service.CreateTournament("Test Tournament", participantNames);
        var matches = service.StartNextRound();

        // Act
        service.RecordMatchResult(matches.First().Id, MatchResult.Player1Win);

        // Assert
        Assert.False(service.IsCurrentRoundComplete());
    }

    /// <summary>
    /// 順位表が正しい順序（ポイント降順）で返されることを確認
    /// </summary>
    [Fact]
    public void GetStandings_ReturnsCorrectOrder()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2", "Player3", "Player4" };
        service.CreateTournament("Test Tournament", participantNames);
        var matches = service.StartNextRound();

        service.RecordMatchResult(matches[0].Id, MatchResult.Player1Win);
        service.RecordMatchResult(matches[1].Id, MatchResult.Player1Win);

        // Act
        var standings = service.GetStandings();

        // Assert
        Assert.Equal(4, standings.Count);
        // Winners should be at the top
        Assert.True(standings[0].Points >= standings[1].Points);
        Assert.True(standings[1].Points >= standings[2].Points);
        Assert.True(standings[2].Points >= standings[3].Points);
    }

    /// <summary>
    /// 全ラウンドが完了した場合、トーナメントが完了とみなされることを確認
    /// </summary>
    [Fact]
    public void IsTournamentComplete_AllRoundsComplete_ReturnsTrue()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2" }; // 1 round only
        service.CreateTournament("Test Tournament", participantNames);

        var matches = service.StartNextRound();
        service.RecordMatchResult(matches.First().Id, MatchResult.Player1Win);

        // Act
        var isComplete = service.IsTournamentComplete();

        // Assert
        Assert.True(isComplete);
    }

    /// <summary>
    /// 全ラウンド完了時にトーナメントを完了状態にマークできることを確認
    /// </summary>
    [Fact]
    public void CompleteTournament_WhenAllRoundsComplete_MarksAsComplete()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "Player1", "Player2" };
        service.CreateTournament("Test Tournament", participantNames);

        var matches = service.StartNextRound();
        service.RecordMatchResult(matches.First().Id, MatchResult.Player1Win);

        // Act
        service.CompleteTournament();

        // Assert
        Assert.True(service.CurrentTournament!.IsComplete);
    }

    /// <summary>
    /// 複数ラウンドにわたって状態が正しく維持されることを確認
    /// </summary>
    [Fact]
    public void MultipleRounds_MaintainsCorrectState()
    {
        // Arrange
        var service = new TournamentService();
        var participantNames = new List<string> { "P1", "P2", "P3", "P4" };
        service.CreateTournament("Test Tournament", participantNames);

        // Round 1
        var round1Matches = service.StartNextRound();
        foreach (var match in round1Matches)
        {
            service.RecordMatchResult(match.Id, MatchResult.Player1Win);
        }

        // Act - Round 2
        var round2Matches = service.StartNextRound();

        // Assert
        Assert.Equal(2, service.CurrentTournament!.CurrentRound);
        Assert.Equal(2, round2Matches.Count);
        Assert.All(round2Matches, m => Assert.Equal(2, m.RoundNumber));

        // Verify that winners from round 1 are paired together in round 2
        var winners = service.CurrentTournament.Participants
            .Where(p => p.Points == 1)
            .Select(p => p.Id)
            .ToList();

        var round2Match1 = round2Matches[0];
        Assert.Contains(round2Match1.Player1Id!.Value, winners);
        Assert.Contains(round2Match1.Player2Id!.Value, winners);
    }
}
