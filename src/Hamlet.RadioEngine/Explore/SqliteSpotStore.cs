using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// The spot history, on disk, in SQLite.
/// </summary>
/// <remarks>
/// <para>ONE FILE BESIDE THE OTHERS (HM-DEC-045), under the same folder as
/// settings and telemetry, so everything Hamlet keeps about a person is in one
/// place they can open, inspect and delete.</para>
/// <para>NEVER THROWS FOR STORAGE REASONS, the same discipline the telemetry
/// writer follows (§8). Every operation catches and reports a harmless answer,
/// because losing history is a nuisance and refusing to run over a cache is a
/// bug. A store that cannot even be opened is refused at construction so the
/// caller can fall back to <see cref="MemorySpotStore"/> and say so.</para>
/// <para>Writes are small and batched inside one transaction. They still must
/// not run on the UI thread, which is the caller's job rather than this
/// class's, and the caller does it.</para>
/// <para>The schema keeps the report time and the first and last sighting as
/// separate columns, because they answer different questions: when the thing
/// happened, when Hamlet first knew, and whether anybody has seen it since.
/// </para>
/// </remarks>
public sealed class SqliteSpotStore : ISpotStore
{
    /// <summary>The file name, under Hamlet's own folder.</summary>
    public const string FileName = "spots.db";

    private readonly SqliteConnection _connection;
    private readonly object _gate = new();
    private bool _disposed;

    private SqliteSpotStore(SqliteConnection connection) => _connection = connection;

    /// <inheritdoc/>
    public bool IsPersistent => true;

    /// <summary>
    /// Open the store, or return null when it cannot be opened.
    /// </summary>
    /// <param name="path">Full path to the database file.</param>
    /// <returns>The store, or null to fall back to memory.</returns>
    /// <remarks>
    /// Null rather than an exception: the caller's job is to carry on without
    /// history, not to handle a failure. Every reason this can fail is
    /// environmental, and none of them is worth a crash.
    /// </remarks>
    public static SqliteSpotStore? TryOpen(string path)
    {
        try
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var connection = new SqliteConnection(
                new SqliteConnectionStringBuilder
                {
                    DataSource = path,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                }.ToString());

            connection.Open();

            using (var pragma = connection.CreateCommand())
            {
                // WAL keeps a reader from blocking the writer, which matters
                // because the UI reads history while a refresh is writing it.
                pragma.CommandText =
                    "PRAGMA journal_mode=WAL;"
                    + "CREATE TABLE IF NOT EXISTS spots ("
                    + "  key TEXT PRIMARY KEY,"
                    + "  story TEXT NOT NULL,"
                    + "  frequency_hz INTEGER NOT NULL,"
                    + "  mode TEXT NOT NULL,"
                    + "  source TEXT NOT NULL,"
                    + "  heard_at TEXT NOT NULL,"
                    + "  first_seen TEXT NOT NULL,"
                    + "  last_seen TEXT NOT NULL,"
                    + "  wpm INTEGER,"
                    + "  call_type INTEGER NOT NULL,"
                    + "  signal_db INTEGER,"
                    + "  dx_call TEXT,"
                    + "  spotter_call TEXT,"
                    + "  proximity INTEGER NOT NULL,"
                    + "  is_activation INTEGER NOT NULL,"
                    + "  reference TEXT,"
                    + "  place_label TEXT,"
                    + "  report_count INTEGER,"
                    + "  latitude REAL,"
                    + "  longitude REAL,"
                    + "  acted_on TEXT);"
                    + "CREATE INDEX IF NOT EXISTS ix_spots_heard ON spots(heard_at);";
                pragma.ExecuteNonQuery();
            }

            AddActedOnColumn(connection);

            return new SqliteSpotStore(connection);
        }
        catch (Exception)
        {
            // A locked file, a full disk, a read-only folder. None of these is
            // a reason the app cannot run.
            return null;
        }
    }

    /// <summary>
    /// Give an older database the acted-on column.
    /// </summary>
    /// <remarks>
    /// CREATE TABLE IF NOT EXISTS leaves a table that already exists alone, so a
    /// database written before HM-DEC-057 would keep its old shape and every
    /// read of the new column would fail. The ALTER is tried and its failure
    /// swallowed, because "the column is already there" and "it could not be
    /// added" both end the same way: carry on, and lose nothing worse than the
    /// memory of which spots were visited (§8).
    /// </remarks>
    private static void AddActedOnColumn(SqliteConnection connection)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "ALTER TABLE spots ADD COLUMN acted_on TEXT;";
            command.ExecuteNonQuery();
        }
        catch (Exception)
        {
            // Already present, which is the ordinary case.
        }
    }

    /// <inheritdoc/>
    public int Record(IReadOnlyList<ActivitySpot> spots, DateTime nowUtc)
    {
        if (spots.Count == 0)
        {
            return 0;
        }

        lock (_gate)
        {
            if (_disposed)
            {
                return 0;
            }

            try
            {
                using var transaction = _connection.BeginTransaction();
                var inserted = 0;

                foreach (var spot in spots)
                {
                    inserted += Upsert(spot, nowUtc, transaction);
                }

                transaction.Commit();
                return inserted;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Insert a spot, or move its last-seen forward if it is already held.
    /// </summary>
    /// <returns>1 when a row was created, 0 when one was updated.</returns>
    /// <remarks>
    /// The report time and first sighting are deliberately left alone on an
    /// update. A station spotted again twenty minutes later did not start
    /// calling twenty minutes later, and treating a re-sighting as a new event
    /// is exactly the "presented as if it just arrived" failure HM-DEC-045
    /// forbids.
    /// </remarks>
    private int Upsert(ActivitySpot spot, DateTime nowUtc, SqliteTransaction transaction)
    {
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText =
            "INSERT INTO spots ("
            + " key, story, frequency_hz, mode, source, heard_at, first_seen, last_seen,"
            + " wpm, call_type, signal_db, dx_call, spotter_call, proximity,"
            + " is_activation, reference, place_label, report_count, latitude, longitude)"
            + " VALUES ("
            + " $key, $story, $hz, $mode, $source, $heard, $now, $now,"
            + " $wpm, $callType, $db, $dx, $spotter, $prox,"
            + " $activation, $reference, $place, $reports, $lat, $lon)"
            + " ON CONFLICT(key) DO UPDATE SET"
            + "   last_seen = $now,"
            + "   report_count = COALESCE($reports, report_count),"
            + "   is_activation = MAX(is_activation, $activation);";

        command.Parameters.AddWithValue("$key", SpotIdentity.KeyFor(spot));
        command.Parameters.AddWithValue("$story", spot.Story);
        command.Parameters.AddWithValue("$hz", spot.FrequencyHz);
        command.Parameters.AddWithValue("$mode", spot.Mode ?? "");
        command.Parameters.AddWithValue("$source", spot.Source ?? "");
        command.Parameters.AddWithValue("$heard", Stamp(spot.HeardAtUtc));
        command.Parameters.AddWithValue("$now", Stamp(nowUtc));
        command.Parameters.AddWithValue("$wpm", (object?)spot.Wpm ?? DBNull.Value);
        command.Parameters.AddWithValue("$callType", (int)spot.CallType);
        command.Parameters.AddWithValue("$db", (object?)spot.SignalDb ?? DBNull.Value);
        command.Parameters.AddWithValue("$dx", (object?)spot.DxCall ?? DBNull.Value);
        command.Parameters.AddWithValue("$spotter", (object?)spot.SpotterCall ?? DBNull.Value);
        command.Parameters.AddWithValue("$prox", (int)spot.Proximity);
        command.Parameters.AddWithValue("$activation", spot.IsActivation ? 1 : 0);
        command.Parameters.AddWithValue("$reference", (object?)spot.Reference ?? DBNull.Value);
        command.Parameters.AddWithValue("$place", (object?)spot.PlaceLabel ?? DBNull.Value);
        command.Parameters.AddWithValue("$reports", (object?)spot.ReportCount ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$lat", (object?)spot.StationLocation?.Latitude ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$lon", (object?)spot.StationLocation?.Longitude ?? DBNull.Value);

        // One row changes either way; the insert is distinguished by whether
        // the key was already there.
        using var exists = _connection.CreateCommand();
        exists.Transaction = transaction;
        exists.CommandText = "SELECT 1 FROM spots WHERE key = $key;";
        exists.Parameters.AddWithValue("$key", SpotIdentity.KeyFor(spot));
        var had = exists.ExecuteScalar() is not null;

        command.ExecuteNonQuery();
        return had ? 0 : 1;
    }

    /// <inheritdoc/>
    public IReadOnlyList<StoredSpot> Since(DateTime sinceUtc)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return Array.Empty<StoredSpot>();
            }

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText =
                    "SELECT story, frequency_hz, mode, source, heard_at, first_seen,"
                    + " last_seen, wpm, call_type, signal_db, dx_call, spotter_call,"
                    + " proximity, is_activation, reference, place_label, report_count,"
                    + " latitude, longitude, acted_on"
                    + " FROM spots WHERE heard_at >= $since ORDER BY heard_at DESC;";
                command.Parameters.AddWithValue("$since", Stamp(sinceUtc));

                var rows = new List<StoredSpot>();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    rows.Add(Read(reader));
                }

                return rows;
            }
            catch (Exception)
            {
                return Array.Empty<StoredSpot>();
            }
        }
    }

    /// <inheritdoc/>
    public int Prune(DateTime beforeUtc)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return 0;
            }

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "DELETE FROM spots WHERE heard_at < $before;";
                command.Parameters.AddWithValue("$before", Stamp(beforeUtc));
                return command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /// <inheritdoc/>
    public void MarkActedOn(string key, DateTime nowUtc)
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText =
                    "UPDATE spots SET acted_on = $now WHERE key = $key;";
                command.Parameters.AddWithValue("$now", Stamp(nowUtc));
                command.Parameters.AddWithValue("$key", key);
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                // Forgetting that somebody visited a spot costs them one
                // repeated card, which is not worth an exception (§8).
            }
        }
    }

    /// <inheritdoc/>
    public int Count()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return 0;
            }

            try
            {
                using var command = _connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM spots;";
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _connection.Close();
                _connection.Dispose();
            }
            catch (Exception)
            {
                // Closing a cache is not worth an exception on shutdown.
            }
        }
    }

    private static StoredSpot Read(SqliteDataReader r)
    {
        double? lat = r.IsDBNull(17) ? null : r.GetDouble(17);
        double? lon = r.IsDBNull(18) ? null : r.GetDouble(18);

        var spot = new ActivitySpot(
            r.GetString(0),
            r.GetInt64(1),
            r.GetString(2),
            r.GetString(3),
            Parse(r.GetString(4)),
            r.IsDBNull(7) ? null : r.GetInt32(7))
        {
            CallType = (SpotCallType)r.GetInt32(8),
            SignalDb = r.IsDBNull(9) ? null : r.GetInt32(9),
            DxCall = r.IsDBNull(10) ? null : r.GetString(10),
            SpotterCall = r.IsDBNull(11) ? null : r.GetString(11),
            Proximity = (SpotProximity)r.GetInt32(12),
            IsActivation = r.GetInt32(13) != 0,
            Reference = r.IsDBNull(14) ? null : r.GetString(14),
            PlaceLabel = r.IsDBNull(15) ? null : r.GetString(15),
            ReportCount = r.IsDBNull(16) ? null : r.GetInt32(16),
            StationLocation = lat is { } la && lon is { } lo ? new LatLon(la, lo) : null,
        };

        return new StoredSpot(
            spot,
            Parse(r.GetString(5)),
            Parse(r.GetString(6)),
            r.IsDBNull(19) ? null : Parse(r.GetString(19)));
    }

    /// <summary>
    /// Round-trip format. ISO 8601 to the second, in UTC, so the column sorts
    /// and compares as text without a conversion in the query.
    /// </summary>
    private static string Stamp(DateTime utc)
        => utc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

    private static DateTime Parse(string stamp)
        => DateTime.TryParse(
            stamp, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var t)
            ? t
            : DateTime.UtcNow;
}
