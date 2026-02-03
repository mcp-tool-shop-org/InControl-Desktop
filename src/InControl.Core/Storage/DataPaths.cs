namespace InControl.Core.Storage;

/// <summary>
/// Defines all data storage paths for the application.
/// All paths are explicit and bounded to specific roots.
/// </summary>
public static class DataPaths
{
    private static readonly Lazy<DataPathsConfig> _config = new(InitializePaths);

    /// <summary>
    /// Gets the root directory for all application data.
    /// Default: %LOCALAPPDATA%\InControl
    /// </summary>
    public static string AppDataRoot => _config.Value.AppDataRoot;

    /// <summary>
    /// Gets the directory where sessions are stored.
    /// Path: {AppDataRoot}\sessions
    /// </summary>
    public static string Sessions => _config.Value.Sessions;

    /// <summary>
    /// Gets the directory where log files are stored.
    /// Path: {AppDataRoot}\logs
    /// </summary>
    public static string Logs => _config.Value.Logs;

    /// <summary>
    /// Gets the directory where cache files are stored.
    /// Path: {AppDataRoot}\cache
    /// </summary>
    public static string Cache => _config.Value.Cache;

    /// <summary>
    /// Gets the directory where exported files are stored.
    /// Path: {Documents}\InControl\exports
    /// </summary>
    public static string Exports => _config.Value.Exports;

    /// <summary>
    /// Gets the directory where configuration files are stored.
    /// Path: {AppDataRoot}\config
    /// </summary>
    public static string Config => _config.Value.Config;

    /// <summary>
    /// Gets the directory where temporary files are stored.
    /// Path: {AppDataRoot}\temp
    /// </summary>
    public static string Temp => _config.Value.Temp;

    /// <summary>
    /// Gets the directory where support bundles are stored.
    /// Path: {AppDataRoot}\support
    /// </summary>
    public static string Support => _config.Value.Support;

    /// <summary>
    /// Gets all paths configuration.
    /// </summary>
    public static DataPathsConfig Configuration => _config.Value;

    /// <summary>
    /// Validates that a path is within allowed write boundaries.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>True if the path is within allowed boundaries.</returns>
    public static bool IsPathAllowed(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var fullPath = Path.GetFullPath(path);

        // Allowed roots
        var allowedRoots = new[]
        {
            _config.Value.AppDataRoot,
            _config.Value.Exports
        };

        return allowedRoots.Any(root =>
            fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the purpose description for a given path.
    /// </summary>
    /// <param name="path">The path to describe.</param>
    /// <returns>A description of what the path is used for.</returns>
    public static string GetPathPurpose(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "Unknown";

        var fullPath = Path.GetFullPath(path);

        if (fullPath.StartsWith(_config.Value.Sessions, StringComparison.OrdinalIgnoreCase))
            return "Session data (conversations, messages)";

        if (fullPath.StartsWith(_config.Value.Logs, StringComparison.OrdinalIgnoreCase))
            return "Application logs";

        if (fullPath.StartsWith(_config.Value.Cache, StringComparison.OrdinalIgnoreCase))
            return "Cached data (model info, temporary results)";

        if (fullPath.StartsWith(_config.Value.Exports, StringComparison.OrdinalIgnoreCase))
            return "Exported sessions and data";

        if (fullPath.StartsWith(_config.Value.Config, StringComparison.OrdinalIgnoreCase))
            return "User configuration and settings";

        if (fullPath.StartsWith(_config.Value.Temp, StringComparison.OrdinalIgnoreCase))
            return "Temporary files (cleared on startup)";

        if (fullPath.StartsWith(_config.Value.Support, StringComparison.OrdinalIgnoreCase))
            return "Support bundles and diagnostics";

        if (fullPath.StartsWith(_config.Value.AppDataRoot, StringComparison.OrdinalIgnoreCase))
            return "Application data";

        return "Unknown (outside allowed boundaries)";
    }

    /// <summary>
    /// Ensures all required directories exist.
    /// </summary>
    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_config.Value.Sessions);
        Directory.CreateDirectory(_config.Value.Logs);
        Directory.CreateDirectory(_config.Value.Cache);
        Directory.CreateDirectory(_config.Value.Exports);
        Directory.CreateDirectory(_config.Value.Config);
        Directory.CreateDirectory(_config.Value.Temp);
        Directory.CreateDirectory(_config.Value.Support);
    }

    /// <summary>
    /// Clears the temporary directory.
    /// </summary>
    public static void ClearTemp()
    {
        var tempPath = _config.Value.Temp;
        if (Directory.Exists(tempPath))
        {
            foreach (var file in Directory.GetFiles(tempPath))
            {
                try { File.Delete(file); }
                catch { /* Ignore cleanup errors */ }
            }

            foreach (var dir in Directory.GetDirectories(tempPath))
            {
                try { Directory.Delete(dir, recursive: true); }
                catch { /* Ignore cleanup errors */ }
            }
        }
    }

    /// <summary>
    /// Gets storage statistics for all paths.
    /// </summary>
    public static StorageStats GetStorageStats()
    {
        return new StorageStats(
            SessionsSize: GetDirectorySize(_config.Value.Sessions),
            LogsSize: GetDirectorySize(_config.Value.Logs),
            CacheSize: GetDirectorySize(_config.Value.Cache),
            ExportsSize: GetDirectorySize(_config.Value.Exports),
            ConfigSize: GetDirectorySize(_config.Value.Config),
            TempSize: GetDirectorySize(_config.Value.Temp),
            SupportSize: GetDirectorySize(_config.Value.Support)
        );
    }

    private static DataPathsConfig InitializePaths()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        var appDataRoot = Path.Combine(localAppData, "InControl");

        return new DataPathsConfig(
            AppDataRoot: appDataRoot,
            Sessions: Path.Combine(appDataRoot, "sessions"),
            Logs: Path.Combine(appDataRoot, "logs"),
            Cache: Path.Combine(appDataRoot, "cache"),
            Exports: Path.Combine(documents, "InControl", "exports"),
            Config: Path.Combine(appDataRoot, "config"),
            Temp: Path.Combine(appDataRoot, "temp"),
            Support: Path.Combine(appDataRoot, "support")
        );
    }

    private static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        try
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>
/// Configuration record containing all data paths.
/// </summary>
public sealed record DataPathsConfig(
    string AppDataRoot,
    string Sessions,
    string Logs,
    string Cache,
    string Exports,
    string Config,
    string Temp,
    string Support
);

/// <summary>
/// Storage statistics for all paths.
/// </summary>
public sealed record StorageStats(
    long SessionsSize,
    long LogsSize,
    long CacheSize,
    long ExportsSize,
    long ConfigSize,
    long TempSize,
    long SupportSize
)
{
    /// <summary>
    /// Gets the total storage used across all paths.
    /// </summary>
    public long TotalSize => SessionsSize + LogsSize + CacheSize + ExportsSize + ConfigSize + TempSize + SupportSize;

    /// <summary>
    /// Gets a formatted string of the total storage used.
    /// </summary>
    public string TotalFormatted => FormatBytes(TotalSize);

    /// <summary>
    /// Formats bytes to human-readable string.
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:F1} {suffixes[suffixIndex]}";
    }
}
