using System.Globalization;
using System.Text.RegularExpressions;

namespace Hamlet.RadioEngine.Capture;

/// <summary>
/// **THE DISK IS KEPT** (work instruction 549, task 4): automatic captures use ten gigabytes at most, the oldest goes first,
/// nothing is written with under twenty gigabytes free, and nothing outside the automatic folder is ever touched.
/// </summary>
/// <remarks>
/// <para>**THE OWNER'S FIGURES**, 2026-10-07: *10 GB at most; the oldest automatic capture goes first; never below 20 GB free;
/// his own Record presses are never deleted.* A gigabyte here is 2^30 bytes, the unit Windows Explorer shows, so the folder
/// never reads larger in Explorer than he was promised.</para>
/// <para>**ONLY WHAT IS DIRECTLY UNDER THE AUTOMATIC FOLDER IS EVER DELETED**, one capture's folder at a time, and a folder
/// is deleted only after its full path is checked to lie inside it. The owner's Record presses, the scan's catches and the
/// telemetry live elsewhere under Hamlet's data folder and are never enumerated, let alone deleted.</para>
/// <para>**OLDEST BY THE TIME IN ITS NAME**, which is when the capture began in UTC (<c>w1aw-2026-10-07-205900</c>,
/// <c>trouble-2026-10-07-181500-senders</c>); a folder without one is ordered by when it was made.</para>
/// </remarks>
public sealed partial class AutoCaptureDisk
{
    /// <summary>The most automatic captures may hold: ten gigabytes.</summary>
    public const long CapBytes = 10L << 30;

    /// <summary>The least the disk may have free before nothing more is written: twenty gigabytes.</summary>
    public const long FloorBytes = 20L << 30;

    /// <summary>What the line under the terminal header says while the floor holds writing off.</summary>
    public const string PausedLine = "auto capture paused · disk under 20 GB free";

    private readonly string _root;
    private readonly Func<long> _freeBytes;
    private readonly long _cap;
    private readonly long _floor;
    private readonly List<string> _deleted = new();
    private readonly object _gate = new();

    /// <summary>A keeper for one automatic folder.</summary>
    /// <param name="autoFolder">The automatic folder: <c>captures\auto</c>.</param>
    /// <param name="freeBytes">How many bytes the disk has free; the drive's own unless a test gives one.</param>
    /// <param name="cap">The most the folder may hold.</param>
    /// <param name="floor">The least the disk may have free.</param>
    public AutoCaptureDisk(string autoFolder, Func<long>? freeBytes = null, long cap = CapBytes, long floor = FloorBytes)
    {
        _root = Path.GetFullPath(autoFolder ?? throw new ArgumentNullException(nameof(autoFolder)));
        _freeBytes = freeBytes ?? (() => FreeBytesOf(_root));
        _cap = cap;
        _floor = floor;
    }

    /// <summary>Whether the disk is under its floor, so nothing automatic is written.</summary>
    public bool Paused
    {
        get
        {
            try
            {
                return _freeBytes() < _floor;
            }
            catch (Exception)
            {
                // A drive that cannot be read is not known to have room (§0.0).
                return true;
            }
        }
    }

    /// <summary>The folders deleted to make room, by name, oldest first: for the tests and the sheet.</summary>
    public IReadOnlyList<string> Deleted
    {
        get { lock (_gate) { return _deleted.ToList(); } }
    }

    /// <summary>The bytes free on the drive a folder is on.</summary>
    /// <param name="folder">The folder.</param>
    /// <returns>Bytes.</returns>
    public static long FreeBytesOf(string folder) => new DriveInfo(Path.GetPathRoot(Path.GetFullPath(folder))!).AvailableFreeSpace;

    /// <summary>
    /// Make room for <paramref name="bytes"/> more: null where it may be written, or the reason it may not.
    /// </summary>
    /// <param name="bytes">What is about to be written.</param>
    /// <param name="keep">A capture folder being written, never deleted to make room; null for none.</param>
    /// <returns>Null, or why not.</returns>
    public string? MakeRoom(long bytes, string? keep)
    {
        lock (_gate)
        {
            return MakeRoomLocked(bytes, keep);
        }
    }

    private string? MakeRoomLocked(long bytes, string? keep)
    {
        if (Paused)
        {
            return "the disk has under 20 GB free, and no automatic capture is written below that";
        }

        try
        {
            var kept = keep is null ? null : Path.GetFullPath(keep);
            var folders = Directory.Exists(_root)
                ? Directory.GetDirectories(_root)
                    .Select(Path.GetFullPath)
                    .Where(f => Inside(f) && !string.Equals(f, kept, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(Began)
                    .ToList()
                : new List<string>();
            var used = Directory.Exists(_root) ? SizeOf(_root) : 0;

            while (used + bytes > _cap && folders.Count > 0)
            {
                var oldest = folders[0];

                folders.RemoveAt(0);
                used -= SizeOf(oldest);
                Directory.Delete(oldest, recursive: true);
                _deleted.Add(Path.GetFileName(oldest));
            }

            return used + bytes > _cap
                ? "the automatic captures would pass 10 GB even with every older one deleted"
                : null;
        }
        catch (Exception)
        {
            // Never-throw (§8): a disk that cannot be measured or cleared is not written to.
            return "the automatic folder could not be measured or cleared";
        }
    }

    // **THE GUARD ON EVERY DELETE**: a direct child of the automatic folder, by its full path.
    private bool Inside(string folder)
        => string.Equals(Path.GetDirectoryName(folder), _root.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);

    private static DateTime Began(string folder)
        => Stamp().Match(Path.GetFileName(folder)) is { Success: true } m
            && DateTime.TryParseExact(m.Value, "yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var at)
            ? at
            : Directory.GetCreationTimeUtc(folder);

    private static long SizeOf(string folder)
        => Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Sum(f => new FileInfo(f).Length);

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}-\d{6}")]
    private static partial Regex Stamp();
}
