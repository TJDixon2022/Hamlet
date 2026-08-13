using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The Reverse Beacon Network client: its line format, and the two filters
/// that make a six-spot-a-second firehose usable (HM-DEC-024).
/// </summary>
public sealed class RbnTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 15, 13, 0, DateTimeKind.Utc);

    /// <summary>Real lines captured from the live feed on 2026-08-13.</summary>
    private const string CqLine =
        "DX de WE9V-#:   14047.90  NZ1J           CW    17 dB  15 WPM  CQ      1513Z";

    private const string BeaconLine =
        "DX de K5TR-#:   28254.50  K4JEE/B        CW    20 dB  15 WPM  BEACON  1513Z";

    private const string LongSpotterLine =
        "DX de DL8LAS-3-#: 14046.00  OE3WHU/QRP     CW     7 dB  15 WPM  CQ      1513Z";

    private static ActivityContext Pittsburgh40m => new()
    {
        BandLowHz = 7_000_000,
        BandHighHz = 7_300_000,
        HomeDistrict = 3,
        HomeInNorthAmerica = true,
    };

    /// <remarks>
    /// Proves the line format is read by landmark rather than by column: the
    /// spotter field runs long on some skimmers and eats its own padding, so
    /// a fixed-width reader would silently misread every one of them.
    /// </remarks>
    [Theory]
    [InlineData(CqLine, "WE9V", 14_047_900L, "NZ1J", 17, 15)]
    [InlineData(LongSpotterLine, "DL8LAS", 14_046_000L, "OE3WHU/QRP", 7, 15)]
    public void Parse_ReadsRealLines(
        string line, string spotter, long hz, string dx, int db, int wpm)
    {
        var spot = RbnSpotLine.Parse(line, Now);

        Assert.NotNull(spot);
        Assert.Equal(spotter, spot!.Spotter);
        Assert.Equal(hz, spot.FrequencyHz);
        Assert.Equal(dx, spot.DxCall);
        Assert.Equal(db, spot.SignalDb);
        Assert.Equal(wpm, spot.Wpm);
        Assert.Equal(SpotCallType.Cq, spot.CallType);
    }

    /// <remarks>
    /// Proves a beacon is recognized as one. A beacon transmits to nobody, so
    /// mislabelling it as a CQ would send a newcomer to call a machine that
    /// will never answer.
    /// </remarks>
    [Fact]
    public void Parse_RecognizesBeacons()
    {
        var spot = RbnSpotLine.Parse(BeaconLine, Now);

        Assert.NotNull(spot);
        Assert.Equal(SpotCallType.Beacon, spot!.CallType);
    }

    /// <remarks>
    /// Proves an unparseable line is dropped rather than half-read. Banner
    /// text, the login prompt and anything the format changes into must never
    /// become a spot (HM-DEC-009).
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("Please enter your call: ")]
    [InlineData("Local users: 697")]
    [InlineData("DX de WE9V-#:   not-a-frequency  NZ1J  CW  17 dB  15 WPM  CQ  1513Z")]
    [InlineData("DX de WE9V-#: 14047.90")]
    public void Parse_DropsWhatItCannotRead(string? line)
        => Assert.Null(RbnSpotLine.Parse(line, Now));

    /// <remarks>
    /// Proves the midnight wrap: a "2358Z" line read just after midnight UTC
    /// belongs to yesterday, and must not be shown as a spot from the future.
    /// </remarks>
    [Fact]
    public void Parse_RollsTheStampBackAcrossMidnight()
    {
        var justAfterMidnight = new DateTime(2026, 8, 14, 0, 2, 0, DateTimeKind.Utc);
        var line = "DX de WE9V-#:   7047.90  NZ1J           CW    17 dB  15 WPM  CQ      2358Z";

        var spot = RbnSpotLine.Parse(line, justAfterMidnight);

        Assert.NotNull(spot);
        Assert.Equal(new DateTime(2026, 8, 13, 23, 58, 0, DateTimeKind.Utc), spot!.HeardAtUtc);
        Assert.True(spot.HeardAtUtc < justAfterMidnight);
    }

    /// <remarks>
    /// Proves the firehose filter of HM-DEC-024 keeps what is plausibly
    /// audible and drops the rest: an in-band spot from a North American
    /// skimmer survives, the same band heard only in Europe does not, and a
    /// nearby skimmer on the wrong band does not either.
    /// </remarks>
    [Fact]
    public async Task Filter_KeepsInBandRegionalSpotsAndDropsTheRest()
    {
        var source = new RbnActivitySource(
            () => new ScriptedConnection(), "KC3QIS", () => Now);
        source.SetContext(Pittsburgh40m);

        // In band, US skimmer in a neighboring district: keep.
        Assert.True(source.Accept(
            "DX de K3GMQ-#:   7032.00  W1ABC          CW    22 dB  15 WPM  CQ      1513Z"));

        // In band, Canadian skimmer: same continent, keep.
        Assert.True(source.Accept(
            "DX de VE3EEE-#:  7035.00  W2DEF          CW    12 dB  18 WPM  CQ      1513Z"));

        // In band, but heard only in Europe: drop.
        Assert.True(source.Accept(
            "DX de DL8LAS-#:  7040.00  OK1GHI         CW     5 dB  25 WPM  CQ      1513Z"));

        // Nearby skimmer, wrong band: drop.
        Assert.True(source.Accept(
            "DX de K3GMQ-#:  14047.90  NZ1J           CW    17 dB  15 WPM  CQ      1513Z"));

        var spots = await source.GetSpotsAsync();
        var calls = spots.Select(s => s.DxCall).ToList();

        Assert.Contains("W1ABC", calls);
        Assert.Contains("W2DEF", calls);
        Assert.DoesNotContain("OK1GHI", calls);
        Assert.DoesNotContain("NZ1J", calls);

        source.Dispose();
    }

    /// <remarks>
    /// Proves the map is not filtered by continent. The list answers "who can
    /// I work"; the map shows the shape of the band, and a band with signals
    /// on it that the map hid would be a lie of omission (HM-DEC-023).
    /// </remarks>
    [Fact]
    public void MapSpots_KeepEveryInBandReport()
    {
        var source = new RbnActivitySource(
            () => new ScriptedConnection(), "KC3QIS", () => Now);
        source.SetContext(Pittsburgh40m);

        source.Accept(
            "DX de K3GMQ-#:   7032.00  W1ABC          CW    22 dB  15 WPM  CQ      1513Z");
        source.Accept(
            "DX de DL8LAS-#:  7040.00  OK1GHI         CW     5 dB  25 WPM  CQ      1513Z");

        var mapCalls = source.GetMapSpots().Select(s => s.DxCall).ToList();

        Assert.Contains("W1ABC", mapCalls);
        Assert.Contains("OK1GHI", mapCalls);

        source.Dispose();
    }

    /// <remarks>
    /// Proves many skimmers hearing one station collapse into one card that
    /// counts them. Twelve separate rows for the same CQ would bury the band;
    /// one row saying twelve receivers hear it is the honest signal a newcomer
    /// actually wants.
    /// </remarks>
    [Fact]
    public async Task Reports_CollapsePerStationAndAreCounted()
    {
        var source = new RbnActivitySource(
            () => new ScriptedConnection(), "KC3QIS", () => Now);
        source.SetContext(Pittsburgh40m);

        source.Accept(
            "DX de K3GMQ-#:   7032.00  W1ABC          CW     8 dB  15 WPM  CQ      1513Z");
        source.Accept(
            "DX de W8XYZ-#:   7032.00  W1ABC          CW    24 dB  15 WPM  CQ      1513Z");
        source.Accept(
            "DX de K2AAA-#:   7032.10  W1ABC          CW    11 dB  15 WPM  CQ      1513Z");

        var spots = await source.GetSpotsAsync();
        var w1abc = Assert.Single(spots);

        Assert.Equal(3, w1abc.ReportCount);
        Assert.Equal(24, w1abc.SignalDb);
        Assert.Contains("3 receivers hear it", w1abc.Story, StringComparison.Ordinal);

        source.Dispose();
    }

    /// <remarks>
    /// Proves the retained set stays bounded whatever the band does. RBN can
    /// deliver twenty thousand spots an hour; an unbounded window would grow
    /// until the render cost showed.
    /// </remarks>
    [Fact]
    public void Window_StaysBounded()
    {
        var source = new RbnActivitySource(
            () => new ScriptedConnection(), "KC3QIS", () => Now);
        source.SetContext(Pittsburgh40m);

        for (var i = 0; i < RbnActivitySource.MaxRetainedSpots * 2; i++)
        {
            var hz = 7_000_000 + (i * 10);
            source.Accept(
                $"DX de K3GMQ-#:   {hz / 1000.0:0.00}  W1AB{i % 10}          "
                + "CW    12 dB  15 WPM  CQ      1513Z");
        }

        Assert.True(source.RetainedCount <= RbnActivitySource.MaxRetainedSpots);

        source.Dispose();
    }

    /// <remarks>
    /// Proves reports age out of the window. RBN is a live stream, so a spot
    /// past the retention window is history and stops being offered as
    /// "happening now".
    /// </remarks>
    [Fact]
    public async Task Window_ForgetsWhatHasAgedOut()
    {
        var clock = Now;
        var source = new RbnActivitySource(
            () => new ScriptedConnection(), "KC3QIS", () => clock);
        source.SetContext(Pittsburgh40m);

        source.Accept(
            "DX de K3GMQ-#:   7032.00  W1ABC          CW    22 dB  15 WPM  CQ      1513Z");
        Assert.NotEmpty(await source.GetSpotsAsync());

        clock = Now + RbnActivitySource.RetentionWindow + TimeSpan.FromMinutes(1);
        Assert.Empty(await source.GetSpotsAsync());

        source.Dispose();
    }

    /// <remarks>
    /// Proves the login handshake sends the operator's callsign, which is the
    /// whole of RBN's authentication, and that Hamlet waits to be asked.
    /// </remarks>
    [Fact]
    public async Task Login_SendsTheCallsignWhenPrompted()
    {
        var connection = new ScriptedConnection(
            "Please enter your call: ",
            "Hello, KC3QIS! Connected.",
            "DX de K3GMQ-#:   7032.00  W1ABC          CW    22 dB  15 WPM  CQ      1513Z");

        var source = new RbnActivitySource(() => connection, "KC3QIS", () => Now);
        source.SetContext(Pittsburgh40m);
        source.Start();

        await connection.Exhausted.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(new[] { "KC3QIS" }, connection.Written);

        source.Dispose();
    }

    /// <remarks>
    /// Proves Hamlet will not connect without a callsign. RBN has no anonymous
    /// login, and inventing one would be lying to the service on the
    /// operator's behalf (HM-DEC-024).
    /// </remarks>
    [Fact]
    public void Start_RefusesWithoutACallsign()
    {
        var connected = false;
        var source = new RbnActivitySource(
            () =>
            {
                connected = true;
                return new ScriptedConnection();
            },
            "   ",
            () => Now);

        source.Start();

        Assert.False(connected);
        Assert.False(source.IsLoggedIn);

        source.Dispose();
    }
}
