using Aspire.Hosting;

namespace JobCommandCenter.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        // PostgreSQL database container - managed by Aspire
        var postgres = builder.AddPostgres("postgres")
            .WithDataVolume("jobcommandcenter-postgres-data");

        var db = postgres.AddDatabase("jobcommandcenterdb");

        // IMPORTANT: The Harvester runs as a native host process (not containerized)
        // This allows it to access localhost:9222 for Chrome CDP connection
        var harvester = builder.AddProject<Projects.JobCommandCenter_Harvester>("harvester")
            .WithReference(db)
            .WithEnvironment("CHROME_DEBUG_PORT", "9222");

        // Web dashboard (can be containerized for production)
        var web = builder.AddProject<Projects.JobCommandCenter_Web>("web")
            .WithReference(db);

        builder.Build().Run();
    }
}
