using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Capture;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Capture;

/// <summary>
/// **THE DISK IS KEPT** (work instruction 549, task 4, HM-DEC-253): a fake data folder, a fake free-space figure and a cap
/// shrunk to kilobytes so the rule can be seen at work. No recording is read.
/// </summary>
public sealed class TheDiskIsKeptTests : IDisposable
{
    private readonly string _data = Path.Combine(Path.GetTempPath(), "hamlet-549-" + Guid.NewGuid().ToString("N"));
    private DateTime _now = new(2026, 10, 7, 13, 10, 0, DateTimeKind.Utc);

    public TheDiskIsKeptTests()
    {
        // The owner's own: a Record press, a scan's catch, the telemetry. None is under captures\auto.
        Write(Path.Combine(Captures, "cw-2026-10-01-120000.wav"), 4000);
        Write(Path.Combine(Captures, "cw-2026-10-01-120000.txt"), 100);
        Write(Path.Combine(Captures, "scans", "scan-2026-10-04-200000", "catch-01.wav"), 4000);
        Write(Path.Combine(_data, "telemetry", "2026-10-07.jsonl"), 500);
    }

    private string Captures => Path.Combine(_data, "captures");

    private string Auto => Path.Combine(Captures, "auto");

    public void Dispose()
    {
        try
        {
            Directory.Delete(_data, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void AtTheCapTheOldestAutomaticCaptureGoesAndNothingElse()
    {
        Write(Path.Combine(Auto, "trouble-2026-10-05-090000-senders", "capture.wav"), 1000);
        Write(Path.Combine(Auto, "w1aw-2026-10-03-205900", "piece-01.wav"), 1000);
        Write(Path.Combine(Auto, "w1aw-2026-10-06-205900", "piece-01.wav"), 1000);

        var disk = new AutoCaptureDisk(Auto, () => 100L << 30, cap: 3500, floor: 20L << 30);

        // 3,000 held and 1,000 more wanted: the oldest by the time in its name goes, the 3rd, not the 5th.
        Assert.Null(disk.MakeRoom(1000, keep: null));
        Assert.Equal(["w1aw-2026-10-03-205900"], disk.Deleted);
        Assert.Equal(
            ["trouble-2026-10-05-090000-senders", "w1aw-2026-10-06-205900"],
            Directory.GetDirectories(Auto).Select(Path.GetFileName).Order());

        // Room for 3,000 more: both remaining go, oldest first, and nothing else on the disk is touched.
        Assert.Null(disk.MakeRoom(3000, keep: null));
        Assert.Equal(["w1aw-2026-10-03-205900", "trouble-2026-10-05-090000-senders", "w1aw-2026-10-06-205900"], disk.Deleted);
        OwnersFilesAreAllThere();
    }

    [Fact]
    public void AManualCaptureIsNeverTouched()
    {
        Write(Path.Combine(Auto, "w1aw-2026-10-03-205900", "piece-01.wav"), 1000);

        // More than the cap can ever hold: every automatic capture may go, and still it is refused rather than taking his.
        var disk = new AutoCaptureDisk(Auto, () => 100L << 30, cap: 2000, floor: 20L << 30);

        Assert.NotNull(disk.MakeRoom(5000, keep: null));
        OwnersFilesAreAllThere();

        // And the capture being written is never deleted to make room for its own next piece.
        Write(Path.Combine(Auto, "w1aw-2026-10-07-125900", "piece-01.wav"), 1500);
        Assert.NotNull(disk.MakeRoom(1000, keep: Path.Combine(Auto, "w1aw-2026-10-07-125900")));
        Assert.True(File.Exists(Path.Combine(Auto, "w1aw-2026-10-07-125900", "piece-01.wav")));
        OwnersFilesAreAllThere();
    }

    [Fact]
    public void UnderTheFreeSpaceFloorNothingIsWritten()
    {
        var disk = new AutoCaptureDisk(Auto, () => (20L << 30) - 1);
        var auto = new CwAutoCapture(Auto, W1awMorseFrequencies.Default, () => _now, threaded: false, disk);
        var conditions = new AutoCaptureConditions(true, true, false, 7_047_500, 500);
        var buffer = Enumerable.Repeat(0.1f, 100).ToArray();

        // In Wednesday's slow code practice on W1AW's frequency, with four senders held and audio lost.
        for (var s = 0; s < 30; s++)
        {
            auto.Hear(s * 100, 100, buffer);
            _now = _now.AddSeconds(1);
            auto.Tick(conditions with { SendersHeld = 5, AudioLostMilliseconds = s });
        }

        Assert.Null(auto.W1awCapture);
        Assert.Equal(AutoCaptureDisk.PausedLine, auto.Line);
        Assert.Equal("auto capture paused · disk under 20 GB free", auto.Line);
        Assert.False(Directory.Exists(Auto) && Directory.EnumerateFileSystemEntries(Auto).Any());
        OwnersFilesAreAllThere();
    }

    [Fact]
    public void AW1awCaptureMakesRoomPieceByPiece()
    {
        // One old trouble capture of 50,000 bytes; a piece at 100 samples a second is 60,044.
        Write(Path.Combine(Auto, "trouble-2026-10-05-090000-audio-lost", "capture.wav"), 50_000);

        var disk = new AutoCaptureDisk(Auto, () => 100L << 30, cap: 100_000, floor: 20L << 30);
        var auto = new CwAutoCapture(Auto, W1awMorseFrequencies.Default, () => _now, threaded: false, disk);
        var conditions = new AutoCaptureConditions(true, true, false, 7_047_500, 500);
        var buffer = Enumerable.Repeat(0.1f, 100).ToArray();

        for (var s = 0; s < 302; s++)
        {
            auto.Hear(s * 100, 100, buffer);
            _now = _now.AddSeconds(1);
            auto.Tick(conditions);
        }

        // The first piece needed the old trouble capture's room; the second needs the first piece's, which is its own
        // capture and is kept, so the capture ends saying why rather than eat itself.
        Assert.Equal(["trouble-2026-10-05-090000-audio-lost"], disk.Deleted);

        // And it stays stopped: a fresh capture of the same session could delete the piece just written.
        for (var s = 0; s < 5; s++)
        {
            _now = _now.AddSeconds(1);
            auto.Tick(conditions);
            Assert.Null(auto.W1awCapture);
        }

        Assert.Equal(CwAutoCapture.RefusedLine, auto.Line);
        Assert.Single(Directory.GetDirectories(Auto));

        var sheet = File.ReadAllText(Directory.GetFiles(Directory.GetDirectories(Auto).Single(), "piece-01.txt").Single());

        Assert.Contains("ended      the automatic captures would pass 10 GB even with every older one deleted", sheet);
        OwnersFilesAreAllThere();
    }

    private static void Write(string path, int bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, new byte[bytes]);
    }

    private void OwnersFilesAreAllThere()
    {
        Assert.Equal(4000, new FileInfo(Path.Combine(Captures, "cw-2026-10-01-120000.wav")).Length);
        Assert.True(File.Exists(Path.Combine(Captures, "cw-2026-10-01-120000.txt")));
        Assert.True(File.Exists(Path.Combine(Captures, "scans", "scan-2026-10-04-200000", "catch-01.wav")));
        Assert.True(File.Exists(Path.Combine(_data, "telemetry", "2026-10-07.jsonl")));
    }
}
