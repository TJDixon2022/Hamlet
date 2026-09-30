using System.Reflection;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **When the reader has nobody, the detector follows the meter** (work instruction 514, task 1,
/// HM-DEC-218).
/// </summary>
/// <remarks>
/// <para>**22:59, 7.0249**: the meter read a station at 500 Hz, a 49 ms dit, score 0.28, while the
/// reader printed nobody and the detector sat on 600.</para>
/// <para>**THE VIEW MODEL'S OWN SCOPE TICK**, driven on a real detector fed silence written here, so
/// only the pitch it is told to follow can move it; the meter's reading is set as the meter would
/// publish it. Nothing is read from disk (R96).</para>
/// </remarks>
public sealed class TheDetectorFollowsTheMeterTests
{
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the pitches are printed.</param>
    public TheDetectorFollowsTheMeterTests(ITestOutputHelper output) => _output = output;

    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    /// <summary>The detector watching 600, the meter reading as given, one scope tick, then a tenth of a second.</summary>
    private static (double Before, double After) Tick(KeyingReading meter)
    {
        var panel = new MainWindowViewModel(new AppSettings(), null);
        var detector = new CwEnvelopeDetector(Rate);
        var silence = new float[Rate / 10];

        typeof(MainWindowViewModel).GetField("_envelope", Private)!.SetValue(panel, detector);
        typeof(MainWindowViewModel).GetField("_keyingReading", Private)!.SetValue(panel, meter);

        detector.Follow(600);
        detector.Process(silence);

        var before = detector.WatchedHz;

        typeof(MainWindowViewModel).GetMethod("OnScopeTick", Private)!.Invoke(panel, new object?[] { null, EventArgs.Empty });
        detector.Process(silence);

        return (before, detector.WatchedHz);
    }

    /// <remarks>Task 1: a meter reading a station at 500 Hz, 49 ms, score 0.28, with the reader printing nobody, moves the detector to 500.</remarks>
    [Fact]
    public void AStationTheMeterHearsIsFollowed()
    {
        var (before, after) = Tick(new KeyingReading(KeyingVerdict.Keying, 500, 49, 30, 20, 0.28, false, 49));

        _output.WriteLine($"meter keying at 500 Hz, 49 ms, score 0.28: detector {before:0} Hz, after one tick {after:0} Hz");

        Assert.Equal(600, before);
        Assert.Equal(500, after);
    }

    /// <remarks>Task 1: a meter reading of score 0.06 with a 4 ms median is noise, and the detector does not move to it.</remarks>
    [Fact]
    public void NoiseTheMeterReadsIsNotFollowed()
    {
        var (before, after) = Tick(new KeyingReading(KeyingVerdict.Keying, 500, 4, 30, 20, 0.06, false, 4));

        _output.WriteLine($"meter at 500 Hz, 4 ms, score 0.06: detector {before:0} Hz, after one tick {after:0} Hz");

        Assert.Equal(600, before);
        Assert.NotEqual(500, after);
    }
}
