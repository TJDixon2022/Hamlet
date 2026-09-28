using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE METER'S SWING BAR IS THE LOWEST SWING ON A STATION THE OWNER HEARD** (work
/// instruction 479 task 2, step 12 criterion 12.4, R93, HM-DEC-187).
/// </summary>
/// <remarks>
/// <para>On 2026-09-28 at 15:38 to 15:39 UTC the owner pressed *You're an idiot* on four
/// stations he heard; the meter's swing read 15.1 to 19.7 against a bar of 17, with medians
/// of 3 to 9 ms. The lowest swing on a station he heard is 15.1.</para>
/// <para>**NO RECORDING** (R88). The profile is written here, figure by figure; nothing is
/// measured from audio.</para>
/// </remarks>
public sealed class TheSwingBarIsTheLowestTheOwnerHeardTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public TheSwingBarIsTheLowestTheOwnerHeardTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Proves a window scoring 0.20, twice the keying score, with an element median of 45 ms
    /// and a swing of 15.5 dB - under 17, over the owner's lowest heard 15.1 - reads as keying.
    /// </remarks>
    [Fact]
    public void AStationSwingingFifteenAndAHalfIsKeying()
    {
        var profile = Profile(swingDb: 15.5);

        _output.WriteLine($"score {profile.Score:0.00}, element median {profile.ElementMedianMs:0} ms, swing {profile.SwingDb:0.0} dB against {CwKeyingThresholds.ConfidentSwingDb:0.0}");

        Assert.Equal(0.20, profile.Score, 6);
        Assert.True(CwKeyingMeter.LooksKeyed(profile));
    }

    /// <remarks>
    /// Proves the bar still refuses: the same window swinging 14.9 dB, under the bar, is not
    /// keying - the swing gate is lowered, not removed.
    /// </remarks>
    [Fact]
    public void TheSameWindowSwingingUnderTheBarIsNot()
    {
        var profile = Profile(swingDb: 14.9);

        _output.WriteLine($"swing {profile.SwingDb:0.0} dB against {CwKeyingThresholds.ConfidentSwingDb:0.0}");

        Assert.False(CwKeyingMeter.LooksKeyed(profile));
    }

    // Share 0.5 times purity 0.4 is a score of 0.20; 45 ms is a dit at about 27 words a minute.
    private static KeyingProfile Profile(double swingDb)
        => new(
            RunsMs: new double[] { 45, 45, 135, 45 },
            MedianMs: 45,
            SwingDb: swingDb,
            ElementShare: 0.5,
            ElementPurity: 0.4,
            Duty: 0.5,
            ElementMedianMs: 45);
}
