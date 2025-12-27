using System.Text.Json;
using TournamentEditor.Core.Data.Entities;
using TournamentEditor.Core.Data.Repositories;
using TournamentEditor.Models;

namespace TournamentEditor.Core.Services;

/// <summary>
/// トーナメント永続化サービス
/// </summary>
public class TournamentPersistenceService
{
    private readonly ITournamentRepository _tournamentRepository;

    public TournamentPersistenceService(ITournamentRepository tournamentRepository)
    {
        _tournamentRepository = tournamentRepository;
    }

    /// <summary>
    /// トーナメントをDBに保存
    /// </summary>
    public async Task<TournamentEntity> SaveTournamentAsync(
        Tournament tournament,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var entity = new TournamentEntity
        {
            Id = tournament.Id,
            Name = tournament.Name,
            UserId = userId,
            TotalRounds = tournament.TotalRounds,
            CurrentRound = tournament.CurrentRound,
            IsComplete = tournament.IsComplete,
            ParticipantsJson = JsonSerializer.Serialize(tournament.Participants),
            MatchesJson = JsonSerializer.Serialize(tournament.Matches)
        };

        var existingTournament = await _tournamentRepository.GetByIdAsync(tournament.Id, cancellationToken);

        if (existingTournament == null)
        {
            return await _tournamentRepository.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.CreatedAt = existingTournament.CreatedAt;
            await _tournamentRepository.UpdateAsync(entity, cancellationToken);
            return entity;
        }
    }

    /// <summary>
    /// DBからトーナメントを読み込み
    /// </summary>
    public async Task<Tournament?> LoadTournamentAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _tournamentRepository.GetByIdAsync(tournamentId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return ConvertToTournament(entity);
    }

    /// <summary>
    /// ユーザーのトーナメント一覧を取得
    /// </summary>
    public async Task<IEnumerable<TournamentEntity>> GetUserTournamentsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _tournamentRepository.GetUserTournamentsAsync(userId, cancellationToken);
    }

    /// <summary>
    /// ユーザーのアクティブなトーナメントを取得
    /// </summary>
    public async Task<IEnumerable<TournamentEntity>> GetUserActiveTournamentsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _tournamentRepository.GetUserActiveTournamentsAsync(userId, cancellationToken);
    }

    /// <summary>
    /// ユーザーの最新トーナメントを取得
    /// </summary>
    public async Task<Tournament?> GetUserLatestTournamentAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _tournamentRepository.GetUserLatestTournamentAsync(userId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return ConvertToTournament(entity);
    }

    /// <summary>
    /// トーナメントを削除
    /// </summary>
    public async Task DeleteTournamentAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default)
    {
        await _tournamentRepository.DeleteByIdAsync(tournamentId, cancellationToken);
    }

    /// <summary>
    /// エンティティをモデルに変換
    /// </summary>
    private Tournament ConvertToTournament(TournamentEntity entity)
    {
        var tournament = new Tournament
        {
            Id = entity.Id,
            Name = entity.Name,
            TotalRounds = entity.TotalRounds,
            CurrentRound = entity.CurrentRound,
            IsComplete = entity.IsComplete,
            CreatedAt = entity.CreatedAt
        };

        try
        {
            tournament.Participants = JsonSerializer.Deserialize<List<Participant>>(entity.ParticipantsJson)
                ?? new List<Participant>();
            tournament.Matches = JsonSerializer.Deserialize<List<Match>>(entity.MatchesJson)
                ?? new List<Match>();
        }
        catch (JsonException)
        {
            tournament.Participants = new List<Participant>();
            tournament.Matches = new List<Match>();
        }

        return tournament;
    }
}
