namespace InControl.Core.Plugins;

/// <summary>
/// Audit logging for plugin operations.
/// All plugin activity is logged and cannot be disabled.
/// </summary>
public interface IPluginAuditLog
{
    /// <summary>
    /// Logs that a plugin was loaded.
    /// </summary>
    void LogPluginLoaded(string pluginId, string version);

    /// <summary>
    /// Logs that a plugin was unloaded.
    /// </summary>
    void LogPluginUnloaded(string pluginId);

    /// <summary>
    /// Logs that a plugin was enabled.
    /// </summary>
    void LogPluginEnabled(string pluginId);

    /// <summary>
    /// Logs that a plugin was disabled.
    /// </summary>
    void LogPluginDisabled(string pluginId);

    /// <summary>
    /// Logs a plugin error.
    /// </summary>
    void LogPluginError(string pluginId, string action, string error);

    /// <summary>
    /// Logs that an action started.
    /// </summary>
    void LogActionStarted(string pluginId, string actionId, Guid executionId);

    /// <summary>
    /// Logs that an action completed.
    /// </summary>
    void LogActionCompleted(string pluginId, string actionId, Guid executionId, bool success, TimeSpan duration);

    /// <summary>
    /// Logs that an action failed.
    /// </summary>
    void LogActionFailed(string pluginId, string actionId, Guid executionId, string error, TimeSpan duration);

    /// <summary>
    /// Gets recent audit entries.
    /// </summary>
    IReadOnlyList<PluginAuditEntry> GetRecentEntries(int count = 100);

    /// <summary>
    /// Gets audit entries for a specific plugin.
    /// </summary>
    IReadOnlyList<PluginAuditEntry> GetEntriesForPlugin(string pluginId, int count = 100);

    /// <summary>
    /// Clears the audit log (operator action only).
    /// </summary>
    void Clear();
}

/// <summary>
/// A single audit log entry.
/// </summary>
public sealed record PluginAuditEntry(
    DateTimeOffset Timestamp,
    string PluginId,
    PluginAuditEventType EventType,
    string? ActionId,
    Guid? ExecutionId,
    bool? Success,
    TimeSpan? Duration,
    string? Details
);

/// <summary>
/// Types of plugin audit events.
/// </summary>
public enum PluginAuditEventType
{
    Loaded,
    Unloaded,
    Enabled,
    Disabled,
    Error,
    ActionStarted,
    ActionCompleted,
    ActionFailed
}

/// <summary>
/// In-memory implementation of plugin audit log.
/// </summary>
public sealed class InMemoryPluginAuditLog : IPluginAuditLog
{
    private readonly List<PluginAuditEntry> _entries = [];
    private readonly object _lock = new();
    private readonly int _maxEntries;

    public InMemoryPluginAuditLog(int maxEntries = 10000)
    {
        _maxEntries = maxEntries;
    }

    public void LogPluginLoaded(string pluginId, string version)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.Loaded,
            null,
            null,
            null,
            null,
            $"Version: {version}"));
    }

    public void LogPluginUnloaded(string pluginId)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.Unloaded,
            null,
            null,
            null,
            null,
            null));
    }

    public void LogPluginEnabled(string pluginId)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.Enabled,
            null,
            null,
            null,
            null,
            null));
    }

    public void LogPluginDisabled(string pluginId)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.Disabled,
            null,
            null,
            null,
            null,
            null));
    }

    public void LogPluginError(string pluginId, string action, string error)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.Error,
            action,
            null,
            false,
            null,
            error));
    }

    public void LogActionStarted(string pluginId, string actionId, Guid executionId)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.ActionStarted,
            actionId,
            executionId,
            null,
            null,
            null));
    }

    public void LogActionCompleted(string pluginId, string actionId, Guid executionId, bool success, TimeSpan duration)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.ActionCompleted,
            actionId,
            executionId,
            success,
            duration,
            null));
    }

    public void LogActionFailed(string pluginId, string actionId, Guid executionId, string error, TimeSpan duration)
    {
        AddEntry(new PluginAuditEntry(
            DateTimeOffset.UtcNow,
            pluginId,
            PluginAuditEventType.ActionFailed,
            actionId,
            executionId,
            false,
            duration,
            error));
    }

    public IReadOnlyList<PluginAuditEntry> GetRecentEntries(int count = 100)
    {
        lock (_lock)
        {
            return _entries
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    public IReadOnlyList<PluginAuditEntry> GetEntriesForPlugin(string pluginId, int count = 100)
    {
        lock (_lock)
        {
            return _entries
                .Where(e => e.PluginId == pluginId)
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }

    private void AddEntry(PluginAuditEntry entry)
    {
        lock (_lock)
        {
            _entries.Add(entry);

            // Trim if over limit
            while (_entries.Count > _maxEntries)
            {
                _entries.RemoveAt(0);
            }
        }
    }
}
