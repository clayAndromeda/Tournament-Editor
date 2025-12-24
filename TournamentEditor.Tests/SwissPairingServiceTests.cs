using TournamentEditor.Models;
using TournamentEditor.Services;

namespace TournamentEditor.Tests;

public class SwissPairingServiceTests
{
    /// <summary>
    /// 有効な参加者数（2の冪乗）でラウンド数が正しく計算されることを確認
    /// </summary>
    [Theory]
    [InlineData(2, 1)]
    [InlineData(4, 2)]
    [InlineData(8, 3)]
    [InlineData(16, 4)]
    [InlineData(32, 5)]
    [InlineData(64, 6)]
    [InlineData(128, 7)]
    [InlineData(256, 8)]
    public void CalculateRounds_WithValidParticipantCount_ReturnsCorrectRounds(int participantCount, int expectedRounds)
    {
        // Act
        var rounds = SwissPairingService.CalculateRounds(participantCount);

        // Assert
        Assert.Equal(expectedRounds, rounds);
    }

    /// <summary>
    /// 無効な参加者数（2の冪乗でない、または範囲外）でArgumentExceptionが発生することを確認
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(257)]
    public void CalculateRounds_WithInvalidParticipantCount_ThrowsArgumentException(int participantCount)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SwissPairingService.CalculateRounds(participantCount));
    }

    /// <summary>
    /// 2の冪乗判定が正しく機能することを確認
    /// </summary>
    [Theory]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(8, true)]
    [InlineData(16, true)]
    [InlineData(32, true)]
    [InlineData(64, true)]
    [InlineData(128, true)]
    [InlineData(256, true)]
    [InlineData(0, false)]
    [InlineData(3, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    public void IsPowerOfTwo_ReturnsCorrectResult(int n, bool expected)
    {
        // Act
        var result = SwissPairingService.IsPowerOfTwo(n);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// 第1ラウンドのペアリング生成が正しく行われることを確認
    /// </summary>
    [Fact]
    public void GeneratePairings_FirstRound_CreatesCorrectMatches()
    {
        // Arrange
        var tournament = new Tournament
        {
            Participants = new List<Participant>
            {
                new Participant { Id = Guid.NewGuid(), Name = "Player1", DisplayOrder = 1 },
                new Participant { Id = Guid.NewGuid(), Name = "Player2", DisplayOrder = 2 },
                new Participant { Id = Guid.NewGuid(), Name = "Player3", DisplayOrder = 3 },
                new Participant { Id = Guid.NewGuid(), Name = "Player4", DisplayOrder = 4 }
            }
        };

        // Act
        var matches = SwissPairingService.GeneratePairings(tournament, 1);

        // Assert
        Assert.Equal(2, matches.Count);
        Assert.All(matches, m => Assert.Equal(1, m.RoundNumber));
        Assert.All(matches, m => Assert.NotNull(m.Player1Id));
        Assert.All(matches, m => Assert.NotNull(m.Player2Id));
    }

    /// <summary>
    /// 第2ラウンドで過去の対戦相手との再戦を回避することを確認
    /// </summary>
    [Fact]
    public void GeneratePairings_SecondRound_AvoidsPreviousOpponents()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player3Id = Guid.NewGuid();
        var player4Id = Guid.NewGuid();

        var tournament = new Tournament
        {
            Participants = new List<Participant>
            {
                new Participant { Id = player1Id, Name = "Player1", DisplayOrder = 1, Points = 1, OpponentIds = new List<Guid> { player2Id } },
                new Participant { Id = player2Id, Name = "Player2", DisplayOrder = 2, Points = 0, OpponentIds = new List<Guid> { player1Id } },
                new Participant { Id = player3Id, Name = "Player3", DisplayOrder = 3, Points = 1, OpponentIds = new List<Guid> { player4Id } },
                new Participant { Id = player4Id, Name = "Player4", DisplayOrder = 4, Points = 0, OpponentIds = new List<Guid> { player3Id } }
            }
        };

        // Act
        var matches = SwissPairingService.GeneratePairings(tournament, 2);

        // Assert
        Assert.Equal(2, matches.Count);

        // Player1 (1pt) should not face Player2 (0pt) again
        // Player3 (1pt) should not face Player4 (0pt) again
        var match1 = matches.First(m => m.Player1Id == player1Id || m.Player2Id == player1Id);
        Assert.True((match1.Player1Id == player1Id && match1.Player2Id == player3Id) ||
                    (match1.Player1Id == player3Id && match1.Player2Id == player1Id));
    }

    /// <summary>
    /// プレイヤー1が勝利した場合、参加者の統計情報が正しく更新されることを確認
    /// </summary>
    [Fact]
    public void UpdateParticipantStats_Player1Wins_UpdatesCorrectly()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var tournament = new Tournament
        {
            Participants = new List<Participant>
            {
                new Participant { Id = player1Id, Name = "Player1" },
                new Participant { Id = player2Id, Name = "Player2" }
            }
        };

        var match = new Match
        {
            Player1Id = player1Id,
            Player2Id = player2Id,
            Result = MatchResult.Player1Win
        };

        // Act
        SwissPairingService.UpdateParticipantStats(tournament, match);

        // Assert
        var player1 = tournament.Participants.First(p => p.Id == player1Id);
        var player2 = tournament.Participants.First(p => p.Id == player2Id);

        Assert.Equal(1, player1.Wins);
        Assert.Equal(0, player1.Losses);
        Assert.Equal(1, player1.Points);
        Assert.Contains(player2Id, player1.OpponentIds);

        Assert.Equal(0, player2.Wins);
        Assert.Equal(1, player2.Losses);
        Assert.Equal(0, player2.Points);
        Assert.Contains(player1Id, player2.OpponentIds);
    }

    /// <summary>
    /// 両負け（両者不戦敗・失格）の場合、参加者の統計情報が正しく更新されることを確認
    /// </summary>
    [Fact]
    public void UpdateParticipantStats_BothLoss_UpdatesCorrectly()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var tournament = new Tournament
        {
            Participants = new List<Participant>
            {
                new Participant { Id = player1Id, Name = "Player1" },
                new Participant { Id = player2Id, Name = "Player2" }
            }
        };

        var match = new Match
        {
            Player1Id = player1Id,
            Player2Id = player2Id,
            Result = MatchResult.BothLoss
        };

        // Act
        SwissPairingService.UpdateParticipantStats(tournament, match);

        // Assert
        var player1 = tournament.Participants.First(p => p.Id == player1Id);
        var player2 = tournament.Participants.First(p => p.Id == player2Id);

        Assert.Equal(0, player1.Wins);
        Assert.Equal(1, player1.Losses);
        Assert.Equal(0, player1.Points);

        Assert.Equal(0, player2.Wins);
        Assert.Equal(1, player2.Losses);
        Assert.Equal(0, player2.Points);
    }

    /// <summary>
    /// ソネボーン・ベルガースコアが正しく計算されることを確認
    /// </summary>
    [Fact]
    public void CalculateSonnebornBerger_CalculatesCorrectly()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player3Id = Guid.NewGuid();

        var tournament = new Tournament
        {
            Participants = new List<Participant>
            {
                new Participant { Id = player1Id, Name = "Player1", Points = 2, OpponentIds = new List<Guid> { player2Id, player3Id } },
                new Participant { Id = player2Id, Name = "Player2", Points = 1, OpponentIds = new List<Guid> { player1Id } },
                new Participant { Id = player3Id, Name = "Player3", Points = 0, OpponentIds = new List<Guid> { player1Id } }
            },
            Matches = new List<Match>
            {
                new Match { Player1Id = player1Id, Player2Id = player2Id, Result = MatchResult.Player1Win },
                new Match { Player1Id = player1Id, Player2Id = player3Id, Result = MatchResult.Player1Win }
            }
        };

        // Act
        SwissPairingService.CalculateSonnebornBerger(tournament);

        // Assert
        var player1 = tournament.Participants.First(p => p.Id == player1Id);
        // Player1 beat Player2 (1pt) and Player3 (0pt)
        // SB = 1 + 0 = 1.0
        Assert.Equal(1.0m, player1.SonnebornBerger);
    }
}
