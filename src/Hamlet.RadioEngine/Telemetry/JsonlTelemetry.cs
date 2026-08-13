using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Hamlet.RadioEngine.Telemetry;

/// <summary>
/// Writes telemetry as one JSON object per line into daily files
/// (<c>YYYY-MM-DD.jsonl</c>), evicting oldest files first when the folder
/// exceeds its size cap (HM-DEC-018).
/// </summary>
/// <remarks>
/// Never-throw discipline (§8): every failure path increments
/// <see cref="DroppedEventCount"/> and returns. A telemetry writer that can
/// take the app down is worse than no telemetry at all.
/// </remarks>
public sealed class JsonlTelemetry : ITelemetry, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    private readonly string _folder;
    private readonly string _appVersion;
    private readonly string _sessionId;
    private readonly Func<TelemetryCategory, bool> _isEnabled;
    private readonly long _maxTotalBytes;
    private readonly BlockingCollection<string> _queue = new(4096);
    private readonly Thread _writer;
    private long _dropped;

    /// <summary>Create a writer over a folder, creating it if needed.</summary>
    /// <param name="folder">Telemetry folder, e.g. %AppData%\Hamlet\telemetry.</param>
    /// <param name="appVersion">Version stamped on every line.</param>
    /// <param name="isEnabled">Category switch lookup, read per event so
    /// Settings changes take effect immediately.</param>
    /// <param name="maxTotalBytes">Folder size cap; oldest daily files are
    /// deleted first when exceeded.</param>
    public JsonlTelemetry(
        string folder,
        string appVersion,
        Func<TelemetryCategory, bool> isEnabled,
        long maxTotalBytes = 50L * 1024 * 1024)
    {
        _folder = folder;
        _appVersion = appVersion;
        _isEnabled = isEnabled;
        _maxTotalBytes = maxTotalBytes;
        _sessionId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            Directory.CreateDirectory(_folder);
        }
        catch (Exception)
        {
            Interlocked.Increment(ref _dropped);
        }

        _writer = new Thread(WriteLoop)
        {
            IsBackground = true,
            Name = "hamlet-telemetry",
        };
        _writer.Start();
    }

    /// <summary>The id correlating every line from this run.</summary>
    public string SessionId => _sessionId;

    /// <inheritdoc/>
    public long DroppedEventCount => Interlocked.Read(ref _dropped);

    /// <inheritdoc/>
    public void Write(TelemetryCategory category, string eventName,
        IReadOnlyDictionary<string, object?>? data = null,
        TelemetryLevel level = TelemetryLevel.Info)
    {
        try
        {
            if (!_isEnabled(category))
            {
                return;
            }

            var line = Serialize(new TelemetryEvent(
                DateTime.UtcNow, _sessionId, level, _appVersion,
                category, eventName,
                data ?? new Dictionary<string, object?>()));

            if (!_queue.TryAdd(line))
            {
                Interlocked.Increment(ref _dropped);
            }
        }
        catch (Exception)
        {
            Interlocked.Increment(ref _dropped);
        }
    }

    /// <summary>Delete every telemetry file. Returns how many went.</summary>
    public int ClearAll()
    {
        var removed = 0;
        try
        {
            foreach (var file in Directory.GetFiles(_folder, "*.jsonl"))
            {
                try
                {
                    File.Delete(file);
                    removed++;
                }
                catch (Exception)
                {
                    // A file held open elsewhere is skipped, not fatal.
                }
            }
        }
        catch (Exception)
        {
            // Folder gone: nothing to clear.
        }

        return removed;
    }

    /// <summary>Total bytes currently held in the telemetry folder.</summary>
    public long TotalBytes()
    {
        try
        {
            return Directory.GetFiles(_folder, "*.jsonl")
                .Sum(f => new FileInfo(f).Length);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            _queue.CompleteAdding();
            _writer.Join(TimeSpan.FromSeconds(2));
        }
        catch (Exception)
        {
            // Shutdown never throws.
        }

        _queue.Dispose();
    }

    /// <summary>Serialize one event to a JSONL line. Internal for tests.</summary>
    internal static string Serialize(TelemetryEvent e)
    {
        var sb = new StringBuilder(160);
        sb.Append("{\"ts\":\"")
          .Append(e.TimestampUtc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ",
              CultureInfo.InvariantCulture))
          .Append("\",\"sessionId\":\"").Append(e.SessionId)
          .Append("\",\"level\":\"").Append(e.Level.ToString().ToLowerInvariant())
          .Append("\",\"appVersion\":\"").Append(e.AppVersion)
          .Append("\",\"category\":\"").Append(e.Category.ToString().ToLowerInvariant())
          .Append("\",\"event\":\"").Append(e.Event)
          .Append("\",\"data\":")
          .Append(JsonSerializer.Serialize(e.Data, JsonOptions))
          .Append('}');
        return sb.ToString();
    }

    private void WriteLoop()
    {
        foreach (var line in _queue.GetConsumingEnumerable())
        {
            try
            {
                var path = Path.Combine(_folder,
                    DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    + ".jsonl");
                File.AppendAllText(path, line + Environment.NewLine);
                EnforceCap();
            }
            catch (Exception)
            {
                Interlocked.Increment(ref _dropped);
            }
        }
    }

    private void EnforceCap()
    {
        try
        {
            var files = new DirectoryInfo(_folder)
                .GetFiles("*.jsonl")
                .OrderBy(f => f.Name, StringComparer.Ordinal)
                .ToList();

            var total = files.Sum(f => f.Length);
            var i = 0;
            while (total > _maxTotalBytes && i < files.Count - 1)
            {
                total -= files[i].Length;
                files[i].Delete();
                i++;
            }
        }
        catch (Exception)
        {
            // Eviction failure is not worth a dropped-event count; the cap
            // is best-effort housekeeping, not a correctness guarantee.
        }
    }
}
