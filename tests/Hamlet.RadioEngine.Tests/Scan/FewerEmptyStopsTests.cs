using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **FEWER EMPTY STOPS, NO LOST STATIONS** (work instruction 544, task 4, HM-DEC-248), on the fake centre-mode scope at the
/// radio's four and a half sweeps a second, at a span of ±1.25 kHz where a bin is about 5.26 Hz, as the owner's scan ran.
/// </summary>
/// <remarks>
/// <para>**WHAT THE SCAN FOUND**: in scan 152712, 150 of 163 stops were left after their two-second check as nothing heard,
/// and 146 of those were noise; and `catch-144637-7029532`, not in the tree, held eight keyed marks in its check and was
/// called empty. At bins this fine the scope's noise shows a little energy in many bins every sweep, and a place within a
/// bin of where noise was before comes round often.</para>
/// <para>**THE NOISE HERE** is forty blips a sweep at 6 or 7, about one bin in twelve, standing in for that. **THE STATION**
/// keys SOS, nine marks in about a second and a half, and pauses four seconds between overs.</para>
/// </remarks>
public sealed class FewerEmptyStopsTests : IDisposable
{
    internal const long Home = 7_031_000;

    /// <summary>The station: SOS at 20 WPM, then four seconds of silence, again and again, at 7.0011 MHz.</summary>
    internal static readonly ScopeStation Pausing = new("pausing", 7_001_100, [30, 28, 24, 18, 12, 6, 3], 0.55, "SOS", 4, Amplitude);

    /// <summary>How loud the station sends: set from the environment for the measurement, 0.02 by default.</summary>
    internal static double Amplitude => double.TryParse(Environment.GetEnvironmentVariable("HAMLET_PAUSING_AMP"), System.Globalization.CultureInfo.InvariantCulture, out var a) ? a : 0.02;

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the shares are printed.</param>
    public FewerEmptyStopsTests(ITestOutputHelper output) => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private static async Task<(SpanWorld World, CwScanSummary Summary)> Scan(double seconds)
    {
        var world = await SpanWorld.Ready(Home, [Pausing], seed: 5444);

        world.HalfSpan = 1_250;
        world.BlipsPerSweep = 40;

        var (_, summary) = await world.Run(
            Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N")),
            new CwScanSettings(TimeSpan.FromSeconds(seconds), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30)));

        return (world, summary);
    }

    /// <remarks>
    /// Task 4: the share of stops called empty over three minutes, and what became of the pausing station.
    /// </remarks>
    [Fact]
    public async Task EmptiesAsAShareOfStops()
    {
        var (world, summary) = await Scan(180);

        using (world)
        {
            var stops = summary.Catches.Count;
            var empties = summary.Catches.Count(c => c.Kind == CatchKind.Empty);
            var station = summary.Catches.Where(c => Math.Abs(c.SignalHz - Pausing.Hz) <= 150).ToList();
            var surveys = summary.Surveys ?? [];

            _output.WriteLine($"{surveys.Count} surveys, {surveys.Sum(s => s.Considered.Count)} peaks considered, {surveys.Sum(s => s.Listed)} listed; {stops} stops, {empties} empty ({(stops == 0 ? 0 : 100.0 * empties / stops):0} %)");
            _output.WriteLine($"the pausing station: {string.Join("; ", station.Select(c => $"{c.SignalHz / 1e6:0.0000} MHz {c.Kind} left {c.Left} `{c.Text}`"))}");

            foreach (var s in surveys)
            {
                foreach (var c in s.Considered.Where(c => c.Verdict != ScopeVerdict.NotRepeating || Math.Abs(c.FrequencyHz - Pausing.Hz) < 300))
                {
                    _output.WriteLine($"  survey {s.Index} {s.LowHz / 1e6:0.0000}-{s.HighHz / 1e6:0.0000}, {s.Sweeps} sweeps: {c.FrequencyHz / 1e6:0.00000} level {c.Level} {c.Verdict}: {c.Why}");
                }
            }

            Assert.NotEmpty(station);
        }
    }

    /// <remarks>
    /// **Must not**: the station that keys nine marks and pauses four seconds, the stand-in for `catch-144637-7029532`, is not
    /// called empty when it is visited.
    /// </remarks>
    [Fact]
    public async Task AStationThatPausesIsNotEmpty()
    {
        var (world, summary) = await Scan(60);

        using (world)
        {
            var station = summary.Catches.Where(c => Math.Abs(c.SignalHz - Pausing.Hz) <= 150).ToList();

            _output.WriteLine(string.Join(Environment.NewLine, station.Select(c => $"{c.SignalHz / 1e6:0.0000} MHz {c.Kind} left {c.Left} `{c.Text}`")));

            Assert.NotEmpty(station);
            Assert.DoesNotContain(station, c => c.Kind == CatchKind.Empty);
        }
    }
}
