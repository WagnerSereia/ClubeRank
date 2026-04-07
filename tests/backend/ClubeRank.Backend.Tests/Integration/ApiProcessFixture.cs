using System.Diagnostics;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ClubeRank.Backend.Tests.Integration;

public sealed class ApiProcessFixture : IAsyncLifetime, IDisposable
{
    private Process? _process;
    private readonly StringBuilder _stdout = new();
    private readonly StringBuilder _stderr = new();

    private readonly string _appDatabaseName = $"ClubeRankAppDb_Tests_{Guid.NewGuid():N}";
    private readonly string _identityDatabaseName = $"ClubeRankIdentityDb_Tests_{Guid.NewGuid():N}";

    public int Port { get; } = Random.Shared.Next(5201, 5999);
    public string BaseUrl => $"http://127.0.0.1:{Port}";

    public HttpClient CreateClient() => new()
    {
        BaseAddress = new Uri(BaseUrl)
    };

    public async Task InitializeAsync()
    {
        var repoRoot = GetRepositoryRoot();
        var apiDllPath = Path.Combine(repoRoot, "src", "backend", "ClubeRank.Api", "bin", "Debug", "net8.0", "ClubeRank.Api.dll");

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{apiDllPath}\"",
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        _process.StartInfo.Environment["ASPNETCORE_URLS"] = BaseUrl;
        _process.StartInfo.Environment["ConnectionStrings__ApplicationConnection"] = BuildConnectionString(_appDatabaseName);
        _process.StartInfo.Environment["ConnectionStrings__IdentityConnection"] = BuildConnectionString(_identityDatabaseName);
        _process.StartInfo.Environment["Jwt__Key"] = "cluberank_process_test_secret_key_for_phase1_validation_2026";
        _process.StartInfo.Environment["Jwt__Issuer"] = "ClubeRank.Process.Tests";
        _process.StartInfo.Environment["Jwt__Audience"] = "ClubeRank.Process.Tests.Client";
        _process.StartInfo.Environment["Jwt__ExpirationInMinutes"] = "60";

        _process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                _stdout.AppendLine(args.Data);
            }
        };

        _process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.Data))
            {
                _stderr.AppendLine(args.Data);
            }
        };

        _process.Start();
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var timeoutAt = DateTime.UtcNow.AddSeconds(45);
        while (DateTime.UtcNow < timeoutAt)
        {
            if (_process.HasExited)
            {
                throw new InvalidOperationException(
                    $"A API encerrou prematuramente. STDOUT: {_stdout} STDERR: {_stderr}");
            }

            if (_stdout.ToString().Contains("Application started", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"A API nao iniciou dentro do tempo esperado. STDOUT: {_stdout} STDERR: {_stderr}");
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        finally
        {
            await DeleteDatabaseAsync(_appDatabaseName);
            await DeleteDatabaseAsync(_identityDatabaseName);
        }
    }

    public void Dispose() => _process?.Dispose();

    private static string GetRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        for (var i = 0; i < 6; i++)
        {
            current = Path.GetFullPath(Path.Combine(current, ".."));
        }

        return current;
    }

    private static string BuildConnectionString(string databaseName) =>
        $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    private static async Task DeleteDatabaseAsync(string databaseName)
    {
        var masterConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $@"
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
BEGIN
    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{databaseName}];
END";

        await command.ExecuteNonQueryAsync();
    }
}
