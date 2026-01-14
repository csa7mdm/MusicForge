using Microsoft.EntityFrameworkCore;
using MusicForge.Domain.Entities;
using MusicForge.Domain.ValueObjects;

namespace MusicForge.Infrastructure.Persistence;

public class MusicForgeDbContext : DbContext
{
    public MusicForgeDbContext(DbContextOptions<MusicForgeDbContext> options) : base(options)
    {
    }

    protected MusicForgeDbContext()
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Stem> Stems => Set<Stem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(builder =>
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ProjectId(value));

            builder.Property(p => p.Status);
            builder.Property(p => p.Name).HasMaxLength(100);
            builder.Property(p => p.MasterFilePath);
            builder.Property(p => p.FailureReason);
            builder.Property(p => p.CreatedAt);
            builder.Property(p => p.UpdatedAt);

            // Configure SongSpecification as embedded/owned
            builder.OwnsOne(p => p.Specification, spec =>
            {
                spec.Property(s => s.Description).HasMaxLength(500);
                spec.Property(s => s.Genre);
                spec.Property(s => s.Mood);
                spec.Property(s => s.DurationSeconds);
                spec.Property(s => s.HasVocals);
                spec.Property(s => s.Lyrics).HasMaxLength(2000);

                // Value Objects flattening
                spec.OwnsOne(s => s.Tempo, t => t.Property(x => x.Value).HasColumnName("TempoBpm"));
                spec.OwnsOne(s => s.Key, k =>
                {
                    k.Property(x => x.Root).HasColumnName("KeyRoot");
                    k.Property(x => x.Mode).HasColumnName("KeyMode");
                });
                spec.OwnsOne(s => s.TimeSignature, t =>
                {
                    t.Property(x => x.Numerator).HasColumnName("TimeSigNum");
                    t.Property(x => x.Denominator).HasColumnName("TimeSigDenom");
                });

                // Store StyleTags as JSON
                spec.PrimitiveCollection(s => s.StyleTags);
            });

            // Configure Arrangement as JSON
            builder.OwnsOne(p => p.Arrangement, arrangement =>
            {
                arrangement.ToJson();

                arrangement.Property(a => a.Id)
                    .HasConversion(id => id.Value, value => new ArrangementId(value));

                arrangement.OwnsOne(a => a.ChordProgression, cp =>
                {
                    cp.PrimitiveCollection(c => c.Chords);
                    cp.PrimitiveCollection(c => c.Durations);
                });

                arrangement.OwnsMany(a => a.Sections, section =>
                {
                    section.Property(s => s.Id)
                        .HasConversion(id => id.Value, value => new SectionId(value));
                    section.PrimitiveCollection(s => s.Elements);
                });
            });

            // Configure AgentContext as JSON
            builder.OwnsOne(p => p.Context, context =>
            {
                context.ToJson();

                context.OwnsMany(c => c.History);
                context.PrimitiveCollection(c => c.FocusCheckpoints); // Use accessing backing field via property if possible, or just public property

                context.OwnsOne(c => c.EstablishedKey);
                context.OwnsOne(c => c.EstablishedTempo);
            });

            // Stems relationship
            builder.HasMany(p => p.Stems)
                .WithOne()
                .HasForeignKey("ProjectId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Stem>(builder =>
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasConversion(
                    id => id.Value,
                    value => new StemId(value));

            builder.Property(s => s.Name);
            builder.Property(s => s.Type);
            builder.Property(s => s.Path);
        });
    }
}
