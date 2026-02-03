using InControl.Core.Connectivity;

namespace InControl.Core.Plugins;

/// <summary>
/// Creates isolated execution contexts for plugins.
/// </summary>
public interface IPluginSandbox
{
    /// <summary>
    /// Creates a sandboxed context for a plugin based on its manifest.
    /// </summary>
    IPluginContext CreateContext(PluginManifest manifest);
}

/// <summary>
/// Sandboxed execution context for a plugin.
/// All resource access is mediated through this context.
/// </summary>
public interface IPluginContext : IDisposable
{
    /// <summary>
    /// The plugin's manifest.
    /// </summary>
    PluginManifest Manifest { get; }

    /// <summary>
    /// Mediated file access.
    /// </summary>
    IPluginFileAccess Files { get; }

    /// <summary>
    /// Mediated network access.
    /// </summary>
    IPluginNetworkAccess Network { get; }

    /// <summary>
    /// Mediated memory access.
    /// </summary>
    IPluginMemoryAccess Memory { get; }

    /// <summary>
    /// Plugin-specific storage.
    /// </summary>
    IPluginStorage Storage { get; }

    /// <summary>
    /// Checks if a permission is granted.
    /// </summary>
    bool HasPermission(PermissionType type, PermissionAccess access, string? scope = null);
}

/// <summary>
/// Mediated file access for plugins.
/// All operations are scoped to declared permissions.
/// </summary>
public interface IPluginFileAccess
{
    /// <summary>
    /// Reads a file if permitted.
    /// </summary>
    Task<PluginFileResult> ReadAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Writes to a file if permitted.
    /// </summary>
    Task<PluginFileResult> WriteAsync(string path, string content, CancellationToken ct = default);

    /// <summary>
    /// Lists files in a directory if permitted.
    /// </summary>
    Task<PluginFileListResult> ListAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Checks if a path is accessible.
    /// </summary>
    bool IsPathPermitted(string path, PermissionAccess access);
}

/// <summary>
/// Mediated network access for plugins.
/// All requests go through ConnectivityManager.
/// </summary>
public interface IPluginNetworkAccess
{
    /// <summary>
    /// Whether network access is currently available.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Makes an HTTP request if permitted.
    /// </summary>
    Task<PluginNetworkResult> RequestAsync(
        string endpoint,
        string method,
        string? body,
        string intent,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if an endpoint is permitted.
    /// </summary>
    bool IsEndpointPermitted(string endpoint);
}

/// <summary>
/// Mediated memory access for plugins.
/// </summary>
public interface IPluginMemoryAccess
{
    /// <summary>
    /// Stores a memory item.
    /// </summary>
    Task<bool> StoreAsync(string key, string content, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a memory item.
    /// </summary>
    Task<string?> RetrieveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Searches memory.
    /// </summary>
    Task<IReadOnlyList<string>> SearchAsync(string query, int limit = 10, CancellationToken ct = default);
}

/// <summary>
/// Isolated storage for plugin data.
/// </summary>
public interface IPluginStorage
{
    /// <summary>
    /// Stores data in plugin-specific storage.
    /// </summary>
    Task SetAsync(string key, string value, CancellationToken ct = default);

    /// <summary>
    /// Retrieves data from plugin-specific storage.
    /// </summary>
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes data from plugin-specific storage.
    /// </summary>
    Task<bool> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Lists all keys in plugin-specific storage.
    /// </summary>
    Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default);

    /// <summary>
    /// Clears all plugin-specific storage.
    /// </summary>
    Task ClearAsync(CancellationToken ct = default);
}

/// <summary>
/// Plugin instance interface - what plugins implement.
/// </summary>
public interface IPluginInstance
{
    /// <summary>
    /// Initializes the plugin with its sandboxed context.
    /// </summary>
    Task InitializeAsync(IPluginContext context, CancellationToken ct = default);

    /// <summary>
    /// Executes an action within the plugin.
    /// </summary>
    Task<PluginActionResult> ExecuteAsync(
        string actionId,
        IReadOnlyDictionary<string, object?> parameters,
        IPluginContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the capabilities (tools) this plugin provides.
    /// </summary>
    IReadOnlyList<PluginCapability> GetCapabilities();
}

/// <summary>
/// Result of a plugin action.
/// </summary>
public sealed record PluginActionResult(
    bool Success,
    object? Output,
    string? Error
)
{
    public static PluginActionResult Succeeded(object? output) =>
        new(true, output, null);

    public static PluginActionResult Failed(string error) =>
        new(false, null, error);
}

/// <summary>
/// Result of a plugin file operation.
/// </summary>
public sealed record PluginFileResult(
    bool Success,
    string? Content,
    string? Error
)
{
    public static PluginFileResult Succeeded(string? content) =>
        new(true, content, null);

    public static PluginFileResult Failed(string error) =>
        new(false, null, error);
}

/// <summary>
/// Result of a plugin file list operation.
/// </summary>
public sealed record PluginFileListResult(
    bool Success,
    IReadOnlyList<string> Files,
    string? Error
)
{
    public static PluginFileListResult Succeeded(IReadOnlyList<string> files) =>
        new(true, files, null);

    public static PluginFileListResult Failed(string error) =>
        new(false, [], error);
}

/// <summary>
/// Result of a plugin network operation.
/// </summary>
public sealed record PluginNetworkResult(
    bool Success,
    int StatusCode,
    string? Data,
    string? Error
)
{
    public static PluginNetworkResult Succeeded(int statusCode, string? data) =>
        new(true, statusCode, data, null);

    public static PluginNetworkResult Failed(string error) =>
        new(false, 0, null, error);
}

/// <summary>
/// Default sandbox implementation.
/// </summary>
public sealed class PluginSandbox : IPluginSandbox
{
    private readonly ConnectivityManager _connectivity;
    private readonly string _storageBasePath;

    public PluginSandbox(ConnectivityManager connectivity, string storageBasePath)
    {
        _connectivity = connectivity;
        _storageBasePath = storageBasePath;

        // Ensure storage directory exists
        Directory.CreateDirectory(storageBasePath);
    }

    public IPluginContext CreateContext(PluginManifest manifest)
    {
        return new PluginContext(manifest, _connectivity, _storageBasePath);
    }
}

/// <summary>
/// Default plugin context implementation.
/// </summary>
internal sealed class PluginContext : IPluginContext
{
    private readonly ConnectivityManager _connectivity;
    private readonly string _storageBasePath;
    private bool _disposed;

    public PluginManifest Manifest { get; }
    public IPluginFileAccess Files { get; }
    public IPluginNetworkAccess Network { get; }
    public IPluginMemoryAccess Memory { get; }
    public IPluginStorage Storage { get; }

    public PluginContext(
        PluginManifest manifest,
        ConnectivityManager connectivity,
        string storageBasePath)
    {
        Manifest = manifest;
        _connectivity = connectivity;
        _storageBasePath = storageBasePath;

        // Create mediated access objects
        Files = new PluginFileAccessImpl(manifest);
        Network = new PluginNetworkAccessImpl(manifest, connectivity);
        Memory = new PluginMemoryAccessImpl(manifest);
        Storage = new PluginStorageImpl(manifest, storageBasePath);
    }

    public bool HasPermission(PermissionType type, PermissionAccess access, string? scope = null)
    {
        return Manifest.Permissions.Any(p =>
            p.Type == type &&
            p.Access >= access &&
            (scope == null || p.Scope == null || MatchesScope(scope, p.Scope)));
    }

    private static bool MatchesScope(string requested, string permitted)
    {
        // Simple prefix matching for paths/endpoints
        return requested.StartsWith(permitted, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (Storage is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

/// <summary>
/// File access implementation that enforces permissions.
/// </summary>
internal sealed class PluginFileAccessImpl : IPluginFileAccess
{
    private readonly PluginManifest _manifest;

    public PluginFileAccessImpl(PluginManifest manifest)
    {
        _manifest = manifest;
    }

    public bool IsPathPermitted(string path, PermissionAccess access)
    {
        return _manifest.Permissions.Any(p =>
            p.Type == PermissionType.File &&
            p.Access >= access &&
            !string.IsNullOrEmpty(p.Scope) &&
            path.StartsWith(p.Scope, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PluginFileResult> ReadAsync(string path, CancellationToken ct = default)
    {
        if (!IsPathPermitted(path, PermissionAccess.Read))
        {
            return PluginFileResult.Failed($"No read permission for path: {path}");
        }

        try
        {
            var content = await File.ReadAllTextAsync(path, ct);
            return PluginFileResult.Succeeded(content);
        }
        catch (Exception ex)
        {
            return PluginFileResult.Failed(ex.Message);
        }
    }

    public async Task<PluginFileResult> WriteAsync(string path, string content, CancellationToken ct = default)
    {
        if (!IsPathPermitted(path, PermissionAccess.Write))
        {
            return PluginFileResult.Failed($"No write permission for path: {path}");
        }

        try
        {
            await File.WriteAllTextAsync(path, content, ct);
            return PluginFileResult.Succeeded(null);
        }
        catch (Exception ex)
        {
            return PluginFileResult.Failed(ex.Message);
        }
    }

    public async Task<PluginFileListResult> ListAsync(string path, CancellationToken ct = default)
    {
        if (!IsPathPermitted(path, PermissionAccess.Read))
        {
            return PluginFileListResult.Failed($"No read permission for path: {path}");
        }

        try
        {
            await Task.CompletedTask; // No async file listing in .NET
            var files = Directory.GetFiles(path).ToList();
            return PluginFileListResult.Succeeded(files);
        }
        catch (Exception ex)
        {
            return PluginFileListResult.Failed(ex.Message);
        }
    }
}

/// <summary>
/// Network access implementation that enforces permissions and uses ConnectivityManager.
/// </summary>
internal sealed class PluginNetworkAccessImpl : IPluginNetworkAccess
{
    private readonly PluginManifest _manifest;
    private readonly ConnectivityManager _connectivity;

    public PluginNetworkAccessImpl(PluginManifest manifest, ConnectivityManager connectivity)
    {
        _manifest = manifest;
        _connectivity = connectivity;
    }

    public bool IsAvailable => _connectivity.IsOnline;

    public bool IsEndpointPermitted(string endpoint)
    {
        return _manifest.Permissions.Any(p =>
            p.Type == PermissionType.Network &&
            !string.IsNullOrEmpty(p.Scope) &&
            endpoint.StartsWith(p.Scope, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PluginNetworkResult> RequestAsync(
        string endpoint,
        string method,
        string? body,
        string intent,
        CancellationToken ct = default)
    {
        if (!IsEndpointPermitted(endpoint))
        {
            return PluginNetworkResult.Failed($"No network permission for endpoint: {endpoint}");
        }

        if (!_connectivity.IsOnline)
        {
            return PluginNetworkResult.Failed("Network is offline");
        }

        try
        {
            var request = new NetworkRequest(
                Endpoint: endpoint,
                Method: method,
                Intent: $"[Plugin:{_manifest.Id}] {intent}",
                DataSent: body,
                RequestedAt: DateTimeOffset.UtcNow);

            var response = await _connectivity.RequestAsync(request, ct);

            if (response == null)
            {
                return PluginNetworkResult.Failed("Request was blocked or failed");
            }

            if (response.IsSuccess)
            {
                return PluginNetworkResult.Succeeded(response.StatusCode, response.Data);
            }
            else
            {
                return PluginNetworkResult.Failed(response.Error ?? "Request failed");
            }
        }
        catch (Exception ex)
        {
            return PluginNetworkResult.Failed(ex.Message);
        }
    }
}

/// <summary>
/// Memory access stub (will integrate with actual memory system).
/// </summary>
internal sealed class PluginMemoryAccessImpl : IPluginMemoryAccess
{
    private readonly PluginManifest _manifest;
    private readonly Dictionary<string, string> _memoryStore = new();

    public PluginMemoryAccessImpl(PluginManifest manifest)
    {
        _manifest = manifest;
    }

    private bool HasReadPermission => _manifest.Permissions.Any(p =>
        p.Type == PermissionType.Memory && p.Access >= PermissionAccess.Read);

    private bool HasWritePermission => _manifest.Permissions.Any(p =>
        p.Type == PermissionType.Memory && p.Access >= PermissionAccess.Write);

    public Task<bool> StoreAsync(string key, string content, CancellationToken ct = default)
    {
        if (!HasWritePermission)
            return Task.FromResult(false);

        _memoryStore[$"{_manifest.Id}:{key}"] = content;
        return Task.FromResult(true);
    }

    public Task<string?> RetrieveAsync(string key, CancellationToken ct = default)
    {
        if (!HasReadPermission)
            return Task.FromResult<string?>(null);

        _memoryStore.TryGetValue($"{_manifest.Id}:{key}", out var value);
        return Task.FromResult(value);
    }

    public Task<IReadOnlyList<string>> SearchAsync(string query, int limit = 10, CancellationToken ct = default)
    {
        if (!HasReadPermission)
            return Task.FromResult<IReadOnlyList<string>>([]);

        var prefix = $"{_manifest.Id}:";
        var results = _memoryStore
            .Where(kv => kv.Key.StartsWith(prefix) && kv.Value.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .Select(kv => kv.Value)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(results);
    }
}

/// <summary>
/// Plugin-specific storage implementation.
/// Each plugin has isolated storage in its own directory.
/// </summary>
internal sealed class PluginStorageImpl : IPluginStorage, IDisposable
{
    private readonly string _storagePath;

    public PluginStorageImpl(PluginManifest manifest, string basePath)
    {
        _storagePath = Path.Combine(basePath, manifest.Id);
        Directory.CreateDirectory(_storagePath);
    }

    private string GetFilePath(string key) => Path.Combine(_storagePath, $"{key}.json");

    public async Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        var path = GetFilePath(key);
        await File.WriteAllTextAsync(path, value, ct);
    }

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var path = GetFilePath(key);
        if (!File.Exists(path))
            return null;

        return await File.ReadAllTextAsync(path, ct);
    }

    public Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        var path = GetFilePath(key);
        if (!File.Exists(path))
            return Task.FromResult(false);

        File.Delete(path);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<string>> ListKeysAsync(CancellationToken ct = default)
    {
        var keys = Directory.GetFiles(_storagePath, "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(keys);
    }

    public Task ClearAsync(CancellationToken ct = default)
    {
        foreach (var file in Directory.GetFiles(_storagePath))
        {
            File.Delete(file);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        // Storage persists - don't delete on dispose
    }
}
