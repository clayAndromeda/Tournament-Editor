namespace TournamentEditor.Models;

public class Match
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RoundNumber { get; set; }
    public int MatchNumber { get; set; }

    public Guid? Player1Id { get; set; }
    public Guid? Player2Id { get; set; }

    public MatchResult Result { get; set; } = MatchResult.NotPlayed;

    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
}
