namespace TournamentEditor.Models;

public class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Tournament statistics
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal Points { get; set; }
    public decimal SonnebornBerger { get; set; } // タイブレーク用

    public List<Guid> OpponentIds { get; set; } = new();
}
