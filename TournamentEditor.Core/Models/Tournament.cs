namespace TournamentEditor.Models;

public class Tournament
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TotalRounds { get; set; }
    public int CurrentRound { get; set; }

    public List<Participant> Participants { get; set; } = new();
    public List<Match> Matches { get; set; } = new();

    public bool IsComplete { get; set; }
}
