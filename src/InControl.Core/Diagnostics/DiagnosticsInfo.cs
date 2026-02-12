using System.Runtime.InteropServices;
using InControl.Core.Storage;
using InControl.Core.Version;

namespace InControl.Core.Diagnostics;

/// <summary>
/// Collects and provides diagnostic information about the application and system.
/// Used for support bundles and "Copy Diagnostics" functionality.
/// </summary>
public static class DiagnosticsInfo
{
    /// <summary>
    /// Gets comprehensive diagnostic information.
    /// </summary>
    public static DiagnosticsReport GetReport()
    {
        return new DiagnosticsReport(
            Application: GetApplicationInfo(),
            Runtime: GetRuntimeInfo(),
            Host: GetSystemInfo(),
            Storage: GetStorageInfo(),
            CollectedAt: DateTimeOffset.UtcNow
        );
    }

    /// <summary>
    /// Gets a compact diagnostic string suitable for clipboard.
    /// Does not include stack traces or sensitive information.
    /// </summary>
    public static string GetCompactDiagnostics()
    {
        var app = GetApplicationInfo();
        var runtime = GetRuntimeInfo();
        var host = GetSystemInfo();

        return $"""
            InControl Diagnostics
            =====================
            Version: {app.Version}
            Configuration: {app.Configuration}

            Runtime: {runtime.FrameworkVersion}
            Platform: {host.OSDescription}
            Architecture: {host.ProcessArchitecture}

            Collected: {DateTimeOffset.UtcNow:u}
            """;
    }

    private static ApplicationInfo GetApplicationInfo()
    {
        return new ApplicationInfo(
            Version: AppVersion.Full,
            Configuration: AppVersion.Configuration,
            ProductName: AppVersion.ProductName
        );
    }

    private static RuntimeInfo GetRuntimeInfo()
    {
        return new RuntimeInfo(
            FrameworkVersion: RuntimeInformation.FrameworkDescription,
            RuntimeIdentifier: RuntimeInformation.RuntimeIdentifier,
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString()
        );
    }

    private static SystemInfo GetSystemInfo()
    {
        return new SystemInfo(
            OSDescription: RuntimeInformation.OSDescription,
            OSArchitecture: RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount: Environment.ProcessorCount,
            MachineName: Environment.MachineName,
            UserName: Environment.UserName,
            SystemDirectory: Environment.SystemDirectory,
            WorkingSet: Environment.WorkingSet
        );
    }

    private static StorageInfo GetStorageInfo()
    {
        var stats = DataPaths.GetStorageStats();
        var config = DataPaths.Configuration;

        return new StorageInfo(
            AppDataRoot: config.AppDataRoot,
            SessionsPath: config.Sessions,
            LogsPath: config.Logs,
            CachePath: config.Cache,
            ExportsPath: config.Exports,
            TotalStorageUsed: stats.TotalFormatted,
            SessionsSize: StorageStats.FormatBytes(stats.SessionsSize),
            LogsSize: StorageStats.FormatBytes(stats.LogsSize),
            CacheSize: StorageStats.FormatBytes(stats.CacheSize)
        );
    }
}

/// <summary>
/// Complete diagnostics report.
/// </summary>
public sealed record DiagnosticsReport(
    ApplicationInfo Application,
    RuntimeInfo Runtime,
    SystemInfo Host,
    StorageInfo Storage,
    DateTimeOffset CollectedAt
)
{
    /// <summary>
    /// Converts to JSON string.
    /// </summary>
    public string ToJson()
    {
        return State.StateSerializer.Serialize(this);
    }
}

/// <summary>
/// Application version and configuration info.
/// </summary>
public sealed record ApplicationInfo(
    string Version,
    string Configuration,
    string ProductName
);

/// <summary>
/// .NET runtime information.
/// </summary>
public sealed record RuntimeInfo(
    string FrameworkVersion,
    string RuntimeIdentifier,
    string ProcessArchitecture
);

/// <summary>
/// Operating system and hardware info.
/// </summary>
public sealed record SystemInfo(
    string OSDescription,
    string OSArchitecture,
    string ProcessArchitecture,
    int ProcessorCount,
    string MachineName,
    string UserName,
    string SystemDirectory,
    long WorkingSet
);

/// <summary>
/// Storage paths and usage info.
/// </summary>
public sealed record StorageInfo(
    string AppDataRoot,
    string SessionsPath,
    string LogsPath,
    string CachePath,
    string ExportsPath,
    string TotalStorageUsed,
    string SessionsSize,
    string LogsSize,
    string CacheSize
);
