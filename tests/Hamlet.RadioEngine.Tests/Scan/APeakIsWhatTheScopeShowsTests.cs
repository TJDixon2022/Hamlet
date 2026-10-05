using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **A PEAK IS JUDGED BY WHAT THE SCOPE REALLY SHOWS** (work instruction 542, task 2, HM-DEC-246), on fake sweeps shaped
/// like the first real scan's: a floor the radio clips to nought, noise blips at 6 and 7 that never come back, a keyed
/// station at level 6 that does, and a static crash many bins wide.
/// </summary>
public sealed class APeakIsWhatTheScopeShowsTests
{
    private const long Low = 6_995_000;
    private const long High = 7_130_000;
    private const int Bins = 475;
    private const long Station = 7_031_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the peaks are printed.</param>
    public APeakIsWhatTheScopeShowsTests(ITestOutputHelper output) => _output = output;

    private static int BinOf(long hz) => (int)((hz - Low) / ((High - Low) / (double)Bins));

    /// <summary>Twenty sweeps, two seconds of the scope at ten a second.</summary>
    private static ScopeWatch Watched(bool station, int seed)
    {
        var random = new Random(seed);
        var watch = new ScopeWatch();
        var keyed = BinOf(Station);

        for (var s = 0; s < 20; s++)
        {
            var bins = new byte[Bins];

            // Two noise blips a sweep, at 6 or 7, anywhere, never twice at one place.
            for (var b = 0; b < 2; b++)
            {
                bins[random.Next(Bins)] = (byte)(6 + random.Next(2));
            }

            // A keyed station at level 6, up in about half the sweeps, as a keyed signal is, in its own bin and half the next.
            if (station && random.NextDouble() < 0.5)
            {
                bins[keyed] = 6;
                bins[keyed + 1] = 3;
            }

            // A static crash in two sweeps: thirty bins at 30.
            if (s is 7 or 13)
            {
                for (var b = 300; b < 330; b++)
                {
                    bins[b] = 30;
                }
            }

            watch.Add(Low, High, bins);
        }

        return watch;
    }

    /// <remarks>
    /// The keyed station at level 6 that repeats is caught, at its place within a bin; no blip at 6 or 7 is, and no crash.
    /// At five seeds.
    /// </remarks>
    [Theory]
    [InlineData(5421)]
    [InlineData(5422)]
    [InlineData(5423)]
    [InlineData(5424)]
    [InlineData(5425)]
    public void AKeyedStationAtSixIsCaughtAndBlipsAreNot(int seed)
    {
        var peaks = Watched(station: true, seed).Peaks(7_000_000, 7_125_000);

        _output.WriteLine(string.Join(Environment.NewLine, peaks.Select(p => $"{p.FrequencyHz} Hz, level {p.Level}, floor {p.Floor}, stood in {p.Repeats} of {p.Sweeps}")));

        var only = Assert.Single(peaks);

        Assert.InRange(only.FrequencyHz, Station - 300, Station + 300);
        Assert.Equal(0, only.Floor);
    }

    /// <remarks>With no station, blips at 6 and 7 and the crash show nothing, at the same five seeds.</remarks>
    [Theory]
    [InlineData(5421)]
    [InlineData(5422)]
    [InlineData(5423)]
    [InlineData(5424)]
    [InlineData(5425)]
    public void BlipsAndACrashAloneAreNothing(int seed)
        => Assert.Empty(Watched(station: false, seed).Peaks(7_000_000, 7_125_000));

    /// <remarks>Where the radio does not clip its noise, the margin over the median floor stands as before.</remarks>
    [Fact]
    public void AnUnclippedFloorKeepsItsMargin()
    {
        var random = new Random(5426);
        var watch = new ScopeWatch();

        for (var s = 0; s < 20; s++)
        {
            var bins = new byte[Bins];

            for (var i = 0; i < Bins; i++)
            {
                bins[i] = (byte)(20 + random.Next(-3, 4));
            }

            bins[BinOf(Station)] = 90;
            watch.Add(Low, High, bins);
        }

        var only = Assert.Single(watch.Peaks(7_000_000, 7_125_000));

        Assert.InRange(only.FrequencyHz, Station - 300, Station + 300);
    }
}
