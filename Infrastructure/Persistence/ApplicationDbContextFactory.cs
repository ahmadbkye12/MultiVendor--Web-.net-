using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> design-time tools so migrations work without running the web app.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveWebApiContentRoot();

        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' not found. Use WebApi/appsettings*.json, user secrets, or env var ConnectionStrings__DefaultConnection (required for Azure SQL).");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveWebApiContentRoot()
    {
        var cwd = Directory.GetCurrentDirectory();
        if (File.Exists(Path.Combine(cwd, "appsettings.json")))
            return cwd;

        var viaInfrastructure = Path.GetFullPath(Path.Combine(cwd, "..", "WebApi"));
        if (File.Exists(Path.Combine(viaInfrastructure, "appsettings.json")))
            return viaInfrastructure;

        var viaSolution = Path.GetFullPath(Path.Combine(cwd, "WebApi"));
        if (File.Exists(Path.Combine(viaSolution, "appsettings.json")))
            return viaSolution;

        throw new InvalidOperationException(
            $"Could not locate WebApi/appsettings.json (cwd was '{cwd}'). Run EF commands from the repo root or WebApi folder, or use -s WebApi.");
    }
}
