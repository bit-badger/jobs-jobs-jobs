namespace JobsJobsJobs.DataMigrate

/// Converters used to translate between database and domain types
module Converters =
  open JobsJobsJobs.Shared
  open Microsoft.EntityFrameworkCore.Storage.ValueConversion

  /// Citizen ID converter
  let CitizenIdConverter = ValueConverter<CitizenId, string> ((fun v -> string v), fun v -> CitizenId.Parse v)

  /// Continent ID converter
  let ContinentIdConverter = ValueConverter<ContinentId, string> ((fun v -> string v), fun v -> ContinentId.Parse v)

  /// Markdown converter
  let MarkdownStringConverter = ValueConverter<MarkdownString, string> ((fun v -> v.Text), fun v -> MarkdownString v)

  /// Markdown converter for possibly-null values
  let OptionalMarkdownStringConverter : ValueConverter<MarkdownString, string> =
    ValueConverter<MarkdownString, string>
      ((fun v -> match v with null -> null | _ -> v.Text), fun v -> match v with null -> null | _ -> MarkdownString v)

  /// Skill ID converter
  let SkillIdConverter = ValueConverter<SkillId, string> ((fun v -> string v), fun v -> SkillId.Parse v)

  /// Success ID converter
  let SuccessIdConverter = ValueConverter<SuccessId, string> ((fun v -> string v), fun v -> SuccessId.Parse v)


open JobsJobsJobs.Shared
open Microsoft.EntityFrameworkCore
open System.Collections.Generic

/// Data context for Jobs, Jobs, Jobs
type JobsDbContext (options : DbContextOptions<JobsDbContext>) =
  inherit DbContext (options)

  /// Citizens (users known to us)
  member val Citizens : DbSet<Citizen> = Unchecked.defaultof<DbSet<Citizen>> with get, set

  /// Continents (large land masses - 7 of them!)
  member val Continents : DbSet<Continent> =  Unchecked.defaultof<DbSet<Continent>> with get, set

  /// Employment profiles
  member val Profiles : DbSet<Profile> =  Unchecked.defaultof<DbSet<Profile>> with get, set

  /// Skills held by citizens of Gitmo Nation
  member val Skills : DbSet<Skill> =  Unchecked.defaultof<DbSet<Skill>> with get, set

  /// Success stories from the site
  member val Successes : DbSet<Success> =  Unchecked.defaultof<DbSet<Success>> with get, set

  override _.OnModelCreating (mb : ModelBuilder) =
    base.OnModelCreating(mb)
    
    mb.Entity<Citizen>(fun m ->
        m.ToTable("citizen", "jjj").HasKey(fun e -> e.Id :> obj) |> ignore
        m.Property(fun e -> e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.CitizenIdConverter) |> ignore
        m.Property(fun e -> e.NaUser).HasColumnName("na_user").IsRequired().HasMaxLength(50) |> ignore
        m.Property(fun e -> e.DisplayName).HasColumnName("display_name").HasMaxLength(255) |> ignore
        m.Property(fun e -> e.ProfileUrl).HasColumnName("profile_url").IsRequired().HasMaxLength(1_024) |> ignore
        m.Property(fun e -> e.JoinedOn).HasColumnName("joined_on").IsRequired() |> ignore
        m.Property(fun e -> e.LastSeenOn).HasColumnName("last_seen_on").IsRequired() |> ignore
        m.Property(fun e -> e.RealName).HasColumnName("real_name").HasMaxLength(255) |> ignore
        m.HasIndex(fun e -> e.NaUser :> obj).IsUnique() |> ignore
        m.Ignore(fun e -> e.CitizenName :> obj) |> ignore)
    |> ignore

    mb.Entity<Continent>(fun m ->
        m.ToTable("continent", "jjj").HasKey(fun e -> e.Id :> obj) |> ignore
        m.Property(fun e -> e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.ContinentIdConverter) |> ignore
        m.Property(fun e -> e.Name).HasColumnName("name").IsRequired().HasMaxLength(255) |> ignore)
    |> ignore

    mb.Entity<Profile>(fun m ->
        m.ToTable("profile", "jjj").HasKey(fun e -> e.Id :> obj) |> ignore
        m.Property(fun e -> e.Id).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.CitizenIdConverter) |> ignore
        m.Property(fun e -> e.SeekingEmployment).HasColumnName("seeking_employment").IsRequired() |> ignore
        m.Property(fun e -> e.IsPublic).HasColumnName("is_public").IsRequired() |> ignore
        m.Property(fun e -> e.ContinentId).HasColumnName("continent_id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.ContinentIdConverter) |> ignore
        m.Property(fun e -> e.Region).HasColumnName("region").IsRequired().HasMaxLength(255) |> ignore
        m.Property(fun e -> e.RemoteWork).HasColumnName("remote_work").IsRequired() |> ignore
        m.Property(fun e -> e.FullTime).HasColumnName("full_time").IsRequired() |> ignore
        m.Property(fun e -> e.Biography).HasColumnName("biography").IsRequired()
            .HasConversion(Converters.MarkdownStringConverter) |> ignore
        m.Property(fun e -> e.LastUpdatedOn).HasColumnName("last_updated_on").IsRequired() |> ignore
        m.Property(fun e -> e.Experience).HasColumnName("experience")
            .HasConversion(Converters.OptionalMarkdownStringConverter) |> ignore
        m.HasOne(fun e -> e.Continent :> obj)
            .WithMany()
            .HasForeignKey(fun e -> e.ContinentId :> obj) |> ignore
        m.HasMany(fun e -> e.Skills :> IEnumerable<Skill>)
            .WithOne()
            .HasForeignKey(fun e -> e.CitizenId :> obj) |> ignore)
    |> ignore

    mb.Entity<Skill>(fun m ->
        m.ToTable("skill", "jjj").HasKey(fun e -> e.Id :> obj) |> ignore
        m.Property(fun e -> e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.SkillIdConverter) |> ignore
        m.Property(fun e -> e.CitizenId).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.CitizenIdConverter) |> ignore
        m.Property(fun e -> e.Description).HasColumnName("skill").IsRequired().HasMaxLength(100) |> ignore
        m.Property(fun e -> e.Notes).HasColumnName("notes").HasMaxLength(100) |> ignore)
    |> ignore

    mb.Entity<Success>(fun m ->
        m.ToTable("success", "jjj").HasKey(fun e -> e.Id :> obj) |> ignore
        m.Property(fun e -> e.Id).HasColumnName("id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.SuccessIdConverter) |> ignore
        m.Property(fun e -> e.CitizenId).HasColumnName("citizen_id").IsRequired().HasMaxLength(12)
            .HasConversion(Converters.CitizenIdConverter) |> ignore
        m.Property(fun e -> e.RecordedOn).HasColumnName("recorded_on").IsRequired() |> ignore
        m.Property(fun e -> e.FromHere).HasColumnName("from_here").IsRequired() |> ignore
        // m.Property(fun e -> e.Source).HasColumnName("source").IsRequired().HasMaxLength(7);
        m.Property(fun e -> e.Story).HasColumnName("story")
            .HasConversion(Converters.OptionalMarkdownStringConverter) |> ignore)
    |> ignore
