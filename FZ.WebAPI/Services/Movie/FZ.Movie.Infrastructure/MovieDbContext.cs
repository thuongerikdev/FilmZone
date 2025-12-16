using System;
using System.Linq;
using FZ.Movie.Domain.Catalog;
using FZ.Movie.Domain.Interactions;
using FZ.Movie.Domain.Media;
using FZ.Movie.Domain.People;
using FZ.Movie.Domain.Taxonomy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FZ.Movie.Infrastructure
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        public DbSet<Movies> Movies => Set<Movies>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<MovieTag> MovieTags => Set<MovieTag>();
        public DbSet<MovieSource> MovieSources => Set<MovieSource>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<EpisodeSource> EpisodeSources => Set<EpisodeSource>();
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<MoviePerson> MoviePersons => Set<MoviePerson>();
        public DbSet<SavedMovie> SavedMovies => Set<SavedMovie>();
        public DbSet<WatchProgress> WatchProgresses => Set<WatchProgress>();
        public DbSet<EpisodeWatchProgress> EpisodeWatchProgresses => Set<EpisodeWatchProgress>();
        public DbSet<UserRating> UserRatings => Set<UserRating>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<ImageSource> ImageSources => Set<ImageSource>();
        public DbSet<MovieImage> MovieImages => Set<MovieImage>();
        public DbSet<Region> Regions => Set<Region>();
        public DbSet<MovieSubTitle> MovieSubTitles => Set<MovieSubTitle>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ========= Index & Keys =========
            mb.Entity<EpisodeSource>()
              .HasIndex(x => new { x.episodeID, x.sourceType, x.sourceID, x.language, x.quality })
              .IsUnique();

            mb.Entity<MovieSource>()
              .HasIndex(x => new { x.movieID, x.sourceType, x.sourceID, x.language, x.quality })
              .IsUnique();

            mb.Entity<Movies>()
              .HasIndex(x => x.slug)
              .IsUnique();

            mb.Entity<Episode>()
              .HasIndex(x => new { x.movieID, x.seasonNumber, x.episodeNumber })
              .IsUnique();

            mb.Entity<MovieTag>()
              .HasIndex(x => new { x.movieID, x.tagID })
              .IsUnique();

            mb.Entity<MoviePerson>()
              .HasKey(x => new { x.movieID, x.personID, x.role });

            mb.Entity<UserRating>()
              .HasKey(x => new { x.userID, x.movieID });

            mb.Entity<WatchProgress>()
              .HasIndex(x => new { x.userID, x.movieID, x.sourceID })
              .IsUnique();

            mb.Entity<EpisodeWatchProgress>()
              .HasIndex(x => new { x.userID, x.episodeID, x.episodeSourceID })
              .IsUnique();

            mb.Entity<Comment>()
              .HasIndex(x => new { x.movieID, x.createdAt });

            // ========= Self reference Comment =========
            mb.Entity<Comment>()
              .HasOne(x => x.parent)
              .WithMany(x => x.replies)
              .HasForeignKey(x => x.parentID)
              .OnDelete(DeleteBehavior.Restrict);

            // ========= Region (break cascades) =========
            mb.Entity<Movies>()
              .HasOne(m => m.regions)
              .WithMany(r => r.Movies)
              .HasForeignKey(m => m.regionID)
              .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<Person>()
              .HasOne(p => p.region)
              .WithMany(r => r.People)
              .HasForeignKey(p => p.regionID)
              .OnDelete(DeleteBehavior.NoAction);

            // ========= MoviePerson (bridge) =========
            mb.Entity<MoviePerson>()
              .HasOne(mp => mp.movie)
              .WithMany(m => m.credits)
              .HasForeignKey(mp => mp.movieID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<MoviePerson>()
              .HasOne(mp => mp.person)
              .WithMany(p => p.credits)
              .HasForeignKey(mp => mp.personID)
              .OnDelete(DeleteBehavior.Cascade);

            // ========= MovieTag (bridge) =========
            mb.Entity<MovieTag>()
              .HasOne(mt => mt.movie)
              .WithMany(m => m.movieTags)
              .HasForeignKey(mt => mt.movieID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<MovieTag>()
              .HasOne(mt => mt.tag)
              .WithMany(t => t.movieTags)
              .HasForeignKey(mt => mt.tagID)
              .OnDelete(DeleteBehavior.Cascade);

            // ========= Sources =========
            mb.Entity<MovieSource>()
              .HasOne(s => s.movie)
              .WithMany(m => m.sources)
              .HasForeignKey(s => s.movieID)
              .OnDelete(DeleteBehavior.Cascade);
            mb.Entity<MovieSubTitle>()
              .HasOne(s => s.movieSource)
              .WithMany(ms => ms.movieSubTitles)
              .HasForeignKey(s => s.movieSourceID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<EpisodeSource>()
              .HasOne(s => s.episode)
              .WithMany(e => e.sources)
              .HasForeignKey(s => s.episodeID)
              .OnDelete(DeleteBehavior.Cascade);

            // ========= Images =========
            mb.Entity<MovieImage>()
              .HasOne(mi => mi.Movie)
              .WithMany(m => m.movieImages)
              .HasForeignKey(mi => mi.movieID)
              .OnDelete(DeleteBehavior.Cascade);

            // ========= Saved / Ratings / Comments =========
            mb.Entity<SavedMovie>()
              .HasOne(sm => sm.movie)
              .WithMany(m => m.savedBy)
              .HasForeignKey(sm => sm.movieID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<UserRating>()
              .HasOne(r => r.movie)
              .WithMany(m => m.ratings)
              .HasForeignKey(r => r.movieID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Comment>()
              .HasOne(c => c.movie)
              .WithMany(m => m.comments)
              .HasForeignKey(c => c.movieID)
              .OnDelete(DeleteBehavior.Cascade);

            // ========= WatchProgress / EpisodeWatchProgress =========
            mb.Entity<WatchProgress>()
              .HasOne(wp => wp.movie)
              .WithMany()
              .HasForeignKey(wp => wp.movieID)
              .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<WatchProgress>()
              .HasOne(wp => wp.source)
              .WithMany()
              .HasForeignKey(wp => wp.sourceID)
              .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<EpisodeWatchProgress>()
              .HasOne(wp => wp.episode)
              .WithMany()
              .HasForeignKey(wp => wp.episodeID)
              .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<EpisodeWatchProgress>()
              .HasOne(wp => wp.episodeSource)
              .WithMany()
              .HasForeignKey(wp => wp.episodeSourceID)
              .OnDelete(DeleteBehavior.Cascade);

            // ========= UTC converters (global, expression-tree safe) =========
            // Non-nullable DateTime
            var utcDateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), // -> DB
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)             // <- DB
            );

            // Nullable DateTime?
            var utcNullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v == null
                        ? (DateTime?)null
                        : (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()),
                v => v == null ? null : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
            );

            foreach (var entity in mb.Model.GetEntityTypes())
            {
                foreach (var prop in entity.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTime))
                        prop.SetValueConverter(utcDateTimeConverter);

                    if (prop.ClrType == typeof(DateTime?))
                        prop.SetValueConverter(utcNullableDateTimeConverter);
                }
            }
            // ======== end UTC converters =========
        }
    }
}
