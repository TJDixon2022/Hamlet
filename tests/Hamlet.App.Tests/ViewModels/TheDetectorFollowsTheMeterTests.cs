using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **The meter steers nothing; the detector is not pointed** (work instruction 515, R114, HM-DEC-219).
/// </summary>
/// <remarks>
/// <para>**WHAT THIS FILE HELD**: unit 514's two cases, `AStationTheMeterHearsIsFollowed` and
/// `NoiseTheMeterReadsIsNotFollowed`, which drove the scope tick's follow-the-meter wire. Unit 515
/// retired the wire with the watched bin, and those two are retired with it.</para>
/// <para>**WHAT IT HOLDS NOW**: the view model's own scope tick, a real detector fed silence written
/// here, and a meter reading a station at 500 Hz. Nothing stands in silence, so the detector says no
/// keying and names no pitch, whatever the meter says. Nothing is read from disk (R96).</para>
/// </remarks>
public sealed class TheDetectorFollowsTheMeterTests
{
    private const int Rate = 8000;

    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the reading is printed.</param>
    public TheDetectorFollowsTheMeterTests(ITestOutputHelper output) => _output = output;

    /// <remarks>
    /// Unit 515: a meter reading keying at 500 Hz, 49 ms, score 0.28, through the scope tick, leaves a
    /// detector that has heard only silence saying no keying and no pitch.
    /// </remarks>
    [Fact]
    public void TheMetersStationDoesNotSteerTheDetector()
    {
        var panel = new MainWindowViewModel(new AppSettings(), null);
        var detector = new CwEnvelopeDetector(Rate);
        var silence = new float[Rate / 10];

        typeof(MainWindowViewModel).GetField("_envelope", Private)!.SetValue(panel, detector);
        typeof(MainWindowViewModel).GetField("_keyingReading", Private)!.SetValue(
            panel, new KeyingReading(KeyingVerdict.Keying, 500, 49, 30, 20, 0.28, false, 49));

        detector.Process(silence);
        typeof(MainWindowViewModel).GetMethod("OnScopeTick", Private)!.Invoke(panel, new object?[] { null, EventArgs.Empty });
        detector.Process(silence);

        var reading = detector.Reading;

        _output.WriteLine($"meter keying at 500 Hz; detector keying {reading.Keying}, pitch {reading.PitchHz}");

        Assert.False(reading.Keying);
        Assert.True(double.IsNaN(reading.PitchHz));
    }
}
