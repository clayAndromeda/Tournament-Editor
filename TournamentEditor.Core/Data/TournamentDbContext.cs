using Microsoft.EntityFrameworkCore;
using TournamentEditor.Core.Data.Entities;

namespace TournamentEditor.Core.Data;

/// <summary>
/// トーナメント管理用データベースコンテキスト
/// </summary>
public class TournamentDbContext : DbContext
{
    public TournamentDbContext(DbContextOptions<TournamentDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 参加者テーブル
    /// </summary>
    public DbSet<ParticipantEntity> Participants => Set<ParticipantEntity>();

    /// <summary>
    /// トーナメントテーブル
    /// </summary>
    public DbSet<TournamentEntity> Tournaments => Set<TournamentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ParticipantEntity の設定
        modelBuilder.Entity<ParticipantEntity>(entity =>
        {
            entity.ToTable("Participants");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .HasMaxLength(200);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.RegisteredAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // インデックス
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.RegisteredAt);
        });

        // TournamentEntity の設定
        modelBuilder.Entity<TournamentEntity>(entity =>
        {
            entity.ToTable("Tournaments");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TotalRounds)
                .IsRequired();

            entity.Property(e => e.CurrentRound)
                .IsRequired();

            entity.Property(e => e.IsComplete)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.Property(e => e.ParticipantsJson)
                .IsRequired()
                .HasColumnType("TEXT");

            entity.Property(e => e.MatchesJson)
                .IsRequired()
                .HasColumnType("TEXT");

            // インデックス
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsComplete);
        });
    }
}
