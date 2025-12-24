namespace TournamentEditor.Models;

public enum MatchResult
{
    NotPlayed,      // 未実施
    Player1Win,     // プレイヤー1勝利
    Player2Win,     // プレイヤー2勝利
    BothLoss        // 両負け（両者不戦敗・失格）
}
