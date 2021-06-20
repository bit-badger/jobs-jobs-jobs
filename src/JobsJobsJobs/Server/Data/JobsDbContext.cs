using JobsJobsJobs.Shared;
using Microsoft.EntityFrameworkCore;

namespace JobsJobsJobs.Server.Data
{
    /// <summary>
    /// Data context for Jobs, Jobs, Jobs
    /// </summary>
    public class JobsDbContext : DbContext
    {
        /// <summary>
        /// Citizens (users known to us)
        /// </summary>
        public DbSet<Citizen> Citizens { get; set; } = default!;

        /// <summary>
        /// Continents (large land masses - 7 of them!)
        /// </summary>
        public DbSet<Continent> Continents { get; set; } = default!;

        /// <summary>
        /// Job listings
        /// </summary>
        public DbSet<Listing> Listings { get; set; } = default!;

        /// <summary>
        /// Employment profiles
        /// </summary>
        public DbSet<Profile> Profiles { get; set; } = default!;

        /// <summary>
        /// Skills held by citizens of Gitmo Nation
        /// </summary>
        public DbSet<Skill> Skills { get; set; } = default!;

        /// <summary>
        /// Success stories from the site
        /// </summary>
        public DbSet<Success> Successes { get; set; } = default!;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">The options to use to configure this instance</param>
        public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Citizen>(m =>
            {
                m.ToTable("citizen", "jjj").HasKey(e => e.Id);
                m.Property(e => e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.CitizenIdConverter);
                m.Property(e => e.NaUser).HasColumnName("na_user").IsRequired().HasMaxLength(50);
                m.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(255);
                m.Property(e => e.ProfileUrl).HasColumnName("profile_url").IsRequired().HasMaxLength(1_024);
                m.Property(e => e.JoinedOn).HasColumnName("joined_on").IsRequired();
                m.Property(e => e.LastSeenOn).HasColumnName("last_seen_on").IsRequired();
                m.Property(e => e.RealName).HasColumnName("real_name").HasMaxLength(255);
                m.HasIndex(e => e.NaUser).IsUnique();
                m.Ignore(e => e.CitizenName);
            });

            modelBuilder.Entity<Continent>(m =>
            {
                m.ToTable("continent", "jjj").HasKey(e => e.Id);
                m.Property(e => e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.ContinentIdConverter);
                m.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            });

            modelBuilder.Entity<Listing>(m =>
            {
                m.ToTable("listing", "jjj").HasKey(e => e.Id);
                m.Property(e => e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.ListingIdConverter);
                m.Property(e => e.CitizenId).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.CitizenIdConverter);
                m.Property(e => e.CreatedOn).HasColumnName("created_on").IsRequired();
                m.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(100);
                m.Property(e => e.ContinentId).HasColumnName("continent_id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.ContinentIdConverter);
                m.Property(e => e.Region).HasColumnName("region").IsRequired().HasMaxLength(255);
                m.Property(e => e.RemoteWork).HasColumnName("remote_work").IsRequired();
                m.Property(e => e.IsExpired).HasColumnName("expired").IsRequired();
                m.Property(e => e.UpdatedOn).HasColumnName("updated_on").IsRequired();
                m.Property(e => e.Text).HasColumnName("listing").IsRequired()
                    .HasConversion(Converters.MarkdownStringConverter);
                m.Property(e => e.NeededBy).HasColumnName("needed_by");
                m.Property(e => e.WasFilledHere).HasColumnName("filled_here");
                m.HasOne(e => e.Citizen)
                    .WithMany()
                    .HasForeignKey(e => e.CitizenId);
                m.HasOne(e => e.Continent)
                    .WithMany()
                    .HasForeignKey(e => e.ContinentId);
            });

            modelBuilder.Entity<Profile>(m =>
            {
                m.ToTable("profile", "jjj").HasKey(e => e.Id);
                m.Property(e => e.Id).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.CitizenIdConverter);
                m.Property(e => e.SeekingEmployment).HasColumnName("seeking_employment").IsRequired();
                m.Property(e => e.IsPublic).HasColumnName("is_public").IsRequired();
                m.Property(e => e.ContinentId).HasColumnName("continent_id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.ContinentIdConverter);
                m.Property(e => e.Region).HasColumnName("region").IsRequired().HasMaxLength(255);
                m.Property(e => e.RemoteWork).HasColumnName("remote_work").IsRequired();
                m.Property(e => e.FullTime).HasColumnName("full_time").IsRequired();
                m.Property(e => e.Biography).HasColumnName("biography").IsRequired()
                    .HasConversion(Converters.MarkdownStringConverter);
                m.Property(e => e.LastUpdatedOn).HasColumnName("last_updated_on").IsRequired();
                m.Property(e => e.Experience).HasColumnName("experience")
                    .HasConversion(Converters.OptionalMarkdownStringConverter);
                m.HasOne(e => e.Continent)
                    .WithMany()
                    .HasForeignKey(e => e.ContinentId);
                m.HasMany(e => e.Skills)
                    .WithOne()
                    .HasForeignKey(e => e.CitizenId);
            });

            modelBuilder.Entity<Skill>(m =>
            {
                m.ToTable("skill", "jjj").HasKey(e => e.Id);
                m.Property(e => e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.SkillIdConverter);
                m.Property(e => e.CitizenId).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.CitizenIdConverter);
                m.Property(e => e.Description).HasColumnName("skill").IsRequired().HasMaxLength(100);
                m.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(100);
            });

            modelBuilder.Entity<Success>(m =>
            {
                m.ToTable("success", "jjj").HasKey(e => e.Id);
                m.Property(e => e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.SuccessIdConverter);
                m.Property(e => e.CitizenId).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
                    .HasConversion(Converters.CitizenIdConverter);
                m.Property(e => e.RecordedOn).HasColumnName("recorded_on").IsRequired();
                m.Property(e => e.FromHere).HasColumnName("from_here").IsRequired();
                m.Property(e => e.Source).HasColumnName("source").IsRequired().HasMaxLength(7);
                m.Property(e => e.Story).HasColumnName("story")
                    .HasConversion(Converters.OptionalMarkdownStringConverter);
            });
        }
    }
}
