using System.Diagnostics;
using System.Text.Json;
using World.GOV.Core;
using World.GOV.Tooling;

namespace World.GOV.Cli;

public static class WorldGovCommand
{
    public static Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
        {
            PrintHelp();
            return Task.FromResult(0);
        }

        try
        {
            var root = ReadOption(args, "--root") ?? ContentCatalog.FindRepositoryRoot();
            var catalog = ContentCatalog.Load(root);
            var command = args[0].ToLowerInvariant();
            var exitCode = command switch
            {
                "validate" => Validate(catalog),
                "export" => Export(catalog, args),
                "countries" => ListCountries(catalog, args),
                "incident" => ShowIncident(catalog, args),
                "history" => ShowHistory(catalog, args),
                "trace" or "blame" => ShowTrace(catalog, args),
                "diagnostics" => ShowDiagnostics(catalog, args),
                "standards" => ListStandards(catalog, args),
                "build-report" => ShowBuildReport(root),
                "real-history" => RunGit(root, ["log", "--date=short", "--pretty=format:%h %ad %s"]),
                "real-blame" => RunRealBlame(root, args),
                _ => Unknown(command)
            };
            return Task.FromResult(exitCode);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or DirectoryNotFoundException or JsonException or ArgumentException)
        {
            Console.Error.WriteLine($"worldgov: {exception.Message}");
            return Task.FromResult(1);
        }
    }

    private static int Validate(ContentCatalog catalog)
    {
        var issues = ContentValidator.Validate(catalog);
        foreach (var issue in issues)
        {
            var location = string.IsNullOrWhiteSpace(issue.FilePath) ? string.Empty : $" ({issue.FilePath})";
            Console.WriteLine($"{issue.Severity} {issue.Code}{location}: {issue.Message}");
        }

        var errors = issues.Count(issue => issue.Severity == "ERROR");
        Console.WriteLine(errors == 0
            ? $"Content validation passed: {catalog.Countries.Count} countries, {catalog.Incidents.Count} incidents, {catalog.GetSourceRegistry().Count} registered sources."
            : $"Content validation failed: {errors} error(s), {issues.Count - errors} warning(s).");
        return errors == 0 ? 0 : 1;
    }

    private static int Export(ContentCatalog catalog, string[] args)
    {
        var output = ReadOption(args, "--output");
        if (ReadOption(args, "--format") is string format && !string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return Unknown("export format (supported: json)");
        var buildStatus = ReadOption(args, "--build-status");
        var path = StaticSiteExporter.Export(catalog, output, buildStatus);
        Console.WriteLine($"Static site data exported to {Path.GetRelativePath(catalog.RepositoryRoot, path).Replace(Path.DirectorySeparatorChar, '/')}");
        Console.WriteLine("GOV diagnostics were exported separately; they did not affect build status.");
        return 0;
    }

    private static int ListCountries(ContentCatalog catalog, string[] args)
    {
        if (args.Length < 2 || args[1] != "list") return Unknown("countries (expected: countries list)");
        Console.WriteLine("ID  COUNTRY      NATIVE NAME     INCIDENTS  LOCALES");
        foreach (var country in catalog.Countries.OrderBy(country => country.DisplayName, StringComparer.Ordinal))
        {
            Console.WriteLine($"{country.Id,-3} {country.DisplayName,-12} {country.NativeName,-15} {catalog.GetByCountry(country.Id).Count,9}  {string.Join(", ", country.SupportedLocales)}");
        }
        return 0;
    }

    private static int ShowIncident(ContentCatalog catalog, string[] args)
    {
        if (args.Length < 3 || args[1] != "show") return Unknown("incident (expected: incident show <id>)");
        var incident = catalog.FindById(args[2]);
        if (incident is null)
        {
            Console.Error.WriteLine($"Incident not found: {args[2]}");
            return 1;
        }

        PrintFacts(catalog, incident);
        if (string.Equals(ReadOption(args, "--view"), "developer", StringComparison.OrdinalIgnoreCase))
            PrintSatire(incident);
        return 0;
    }

    private static int ShowHistory(ContentCatalog catalog, string[] args)
    {
        var countryId = args.Length > 1 && !args[1].StartsWith("--", StringComparison.Ordinal) ? args[1] : "DK";
        var history = catalog.GetHistory(countryId);
        if (history.Count == 0)
        {
            Console.Error.WriteLine($"No historical simulation is registered for country {countryId}.");
            return 1;
        }

        Console.WriteLine("HISTORICAL SIMULATION — these entries are not authentic Git commits.");
        var year = ReadOption(args, "--year");
        if (year is not null && (year.Length != 4 || !int.TryParse(year, out var parsedYear) || parsedYear < 1))
            return Unknown("history year (expected: YYYY)");
        foreach (var commit in history.Where(item => year is null || YearOverlaps(item.HistoricalDate, int.Parse(year))))
            Console.WriteLine($"{commit.Date,-10} {commit.Hash,-14} {commit.Message} [{commit.IncidentId}]");
        if (args.Contains("--include-classified", StringComparer.OrdinalIgnoreCase))
            Console.WriteLine("[REDACTED] Fictional presentation effect. No classified information is being revealed.");
        return 0;
    }

    private static int ShowTrace(ContentCatalog catalog, string[] args)
    {
        if (args.Length < 2) return Unknown("trace (expected: trace <incident-id>)");
        var entity = args[1].EndsWith("/Government", StringComparison.OrdinalIgnoreCase) ? args[1].Split('/')[0] : args[1];
        var trace = catalog.GetBlame(entity);
        if (trace.Count == 0)
        {
            Console.Error.WriteLine($"Incident not found: {args[1]}");
            return 1;
        }

        Console.WriteLine("VIRTUAL TRACE — interpretive links, not git blame of source lines.");
        foreach (var commit in trace)
        {
            var tracedIncident = catalog.FindById(commit.IncidentId)!;
            Console.WriteLine($"{commit.Date} {commit.Hash} {tracedIncident.Title}");
            Console.WriteLine($"  Source file: {commit.SourcePath ?? "not recorded"}");
            Console.WriteLine($"  GOV standards: {(tracedIncident.Satire.Standards.Count == 0 ? "none" : string.Join(", ", tracedIncident.Satire.Standards))}");
        }
        return 0;
    }

    private static int ShowDiagnostics(ContentCatalog catalog, string[] args)
    {
        var countryId = args.Length > 1 && !args[1].StartsWith("--", StringComparison.Ordinal) ? args[1] : null;
        var incidents = string.IsNullOrWhiteSpace(countryId) ? catalog.Incidents : catalog.GetByCountry(countryId);
        Console.WriteLine("GOV DOMAIN DIAGNOSTICS — satirical records; not compiler errors.");
        foreach (var incident in incidents.OrderBy(item => item.Date))
        {
            var standard = incident.Satire.Standards.FirstOrDefault() ?? "unmapped";
            Console.WriteLine($"{incident.Satire.DiagnosticId} {incident.Satire.Severity} (ignored by technical build policy) [{incident.Id}/{standard}]");
            Console.WriteLine($"  {incident.Satire.DiagnosticMessage}");
        }
        return 0;
    }

    private static int ListStandards(ContentCatalog catalog, string[] args)
    {
        if (args.Length < 2 || args[1] != "list") return Unknown("standards (expected: standards list)");
        foreach (var standard in catalog.Standards.OrderBy(item => item.Id, StringComparer.Ordinal))
            Console.WriteLine($"{standard.Id} [{standard.Country}] {standard.Title} — {standard.FilePath}");
        return 0;
    }

    private static int ShowBuildReport(string root)
    {
        var reportPath = Path.Combine(root, "publish-data", "build-report.json");
        if (!File.Exists(reportPath))
        {
            Console.WriteLine("No published build report is available. Run 'worldgov export' after a build.");
            return 0;
        }
        var report = JsonSerializer.Deserialize<BuildReport>(File.ReadAllText(reportPath), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (report is null)
        {
            Console.Error.WriteLine("The published build report is invalid.");
            return 1;
        }
        Console.WriteLine($"Technical build: {report.BuildStatus} ({report.StatusSource})");
        Console.WriteLine($"GOV domain diagnostics registered: {report.RegisteredDiagnostics} — {report.DiagnosticStatusLabel}");
        return 0;
    }

    private static int RunRealBlame(string root, string[] args)
    {
        if (args.Length < 2) return Unknown("real-blame (expected: real-blame <repository-relative-file>)");
        var target = Path.GetFullPath(Path.Combine(root, args[1]));
        var rootPrefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!target.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Path must remain inside the repository.");
            return 1;
        }
        var relative = Path.GetRelativePath(root, target).Replace(Path.DirectorySeparatorChar, '/');
        return RunGit(root, ["blame", "--", relative]);
    }

    private static int RunGit(string root, IReadOnlyList<string> arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo("git")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        foreach (var argument in arguments) process.StartInfo.ArgumentList.Add(argument);
        try
        {
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (output.Length > 0) Console.WriteLine(output.TrimEnd());
            if (error.Length > 0) Console.Error.WriteLine(error.TrimEnd());
            return process.ExitCode;
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Console.Error.WriteLine($"Could not run authentic git history: {exception.Message}");
            return 1;
        }
    }

    private static void PrintFacts(ContentCatalog catalog, GovernmentIncident incident)
    {
        Console.WriteLine($"{incident.Date.DisplayText} · {incident.Title} [{incident.Id}]");
        Console.WriteLine($"Country: {incident.Country} | Category: {incident.Category} | Evidence: {incident.Historical.EvidenceStatus}");
        Console.WriteLine("HISTORICAL FACTS");
        Console.WriteLine(incident.Historical.Summary);
        if (!string.IsNullOrWhiteSpace(incident.Historical.Uncertainty)) Console.WriteLine($"Uncertainty: {incident.Historical.Uncertainty}");
        Console.WriteLine("SOURCES");
        foreach (var sourceId in incident.Historical.Sources)
        {
            var source = catalog.FindSource(sourceId);
            if (source is not null) Console.WriteLine($"- {source.Title} — {source.Url} [{source.Id}]");
        }
    }

    private static bool YearOverlaps(HistoricalDate date, int year) =>
        date.StartYear is int start && year >= start && year <= (date.EndYear ?? start);

    private static void PrintSatire(GovernmentIncident incident)
    {
        Console.WriteLine("SATIRICAL IMPLEMENTATION (FICTION)");
        Console.WriteLine($"Simulated commit: {incident.Satire.CommitMessage}");
        Console.WriteLine($"{incident.Satire.DiagnosticId} {incident.Satire.Severity}: {incident.Satire.DiagnosticMessage}");
        Console.WriteLine($"Standards: {(incident.Satire.Standards.Count == 0 ? "none" : string.Join(", ", incident.Satire.Standards))}");
        Console.WriteLine("This is a historical simulation, not an authentic Git commit or historical quotation.");
    }

    private static int Unknown(string command)
    {
        Console.Error.WriteLine($"Unknown or incomplete command: {command}");
        PrintHelp();
        return 2;
    }

    private static string? ReadOption(string[] args, string option)
    {
        for (var index = 0; index < args.Length; index++)
            if (string.Equals(args[index], option, StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 == args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
                    throw new ArgumentException($"{option} requires a value.");
                return args[index + 1];
            }
        return null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("World.GOV CLI — sourced history and clearly labeled satire");
        Console.WriteLine("  worldgov validate [--root PATH]");
        Console.WriteLine("  worldgov export [--root PATH] [--output PATH] [--build-status succeeded|failed]");
        Console.WriteLine("  worldgov countries list [--root PATH]");
        Console.WriteLine("  worldgov incident show <id> [--view developer] [--root PATH]");
        Console.WriteLine("  worldgov history [country-id] [--year YYYY] [--include-classified] [--root PATH]");
        Console.WriteLine("  worldgov blame <incident-id|country-id|COUNTRY/Government> [--root PATH]");
        Console.WriteLine("  worldgov trace <incident-id> [--root PATH]");
        Console.WriteLine("  worldgov diagnostics [country-id] [--root PATH]");
        Console.WriteLine("  worldgov standards list [--root PATH]");
        Console.WriteLine("  worldgov build-report [--root PATH]");
        Console.WriteLine("  worldgov real-history [--root PATH]      authentic local git log");
        Console.WriteLine("  worldgov real-blame <file> [--root PATH] authentic local git blame");
    }
}
