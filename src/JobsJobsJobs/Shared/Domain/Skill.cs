namespace JobsJobsJobs.Shared
{
    /// <summary>
    /// A skill the job seeker possesses
    /// </summary>
    public record Skill(SkillId Id, CitizenId CitizenId, string Description, string? Notes);
}
