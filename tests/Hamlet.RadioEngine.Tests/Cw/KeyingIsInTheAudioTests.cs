using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Whether there is keying in a recording at all, measured before anything is
/// concluded about the decoder (HM-DEC-091, §12.5).
/// </summary>
/// <remarks>
/// <para>**A DIAGNOSIS WAS PRODUCED FROM COUNTERS THAT WERE NOT COUNTING THE
/// RECORDING.** Two captures read as nothing, a session concluded that the speed
/// tracker cannot lock on a sloppy human fist, and the measurement that overturned
/// it was taken outside this repository. An analysis nobody can re-run is an
/// argument, which is what §0.0.1 says about a decode with no audio behind it.
/// This is that measurement, inside the tree.</para>
/// <para>**IT ASSERTS NOTHING ABOUT WHAT ANY STATION SENT** (§0.0). Nobody knows,
/// and inventing a transcript would be worse than having none. What it asserts is
/// what the envelope does: how many times the key went down, for how long, and how
/// far the signal moved between quiet and loud.</para>
/// <para>**THE TWO PICTURES THE PROJECT NEEDED TO BE ABLE TO TELL APART** are a
/// couple of hundred key-downs gathered into two clusters, which is somebody
/// sending, and fifteen hundred at a median of five or six milliseconds, which is
/// a threshold being crossed by noise. The second test here is the control: pure
/// noise through the same method, so a pass on the first cannot be the method
/// finding structure in anything it is handed.</para>
/// </remarks>
public sealed class KeyingIsInTheAudioTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the profile is printed.</param>
    public KeyingIsInTheAudioTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The pitch this recording's own sidecar records the decoder as having
    /// found. Read rather than swept, because it is already a measurement and
    /// this test is not the place to take a second one (HM-DEC-091).
    /// </summary>
    private const double ToneHz = 500;

    /// <remarks>
    /// <para>Proves HM-DEC-091: **this recording contains somebody keying**, and
    /// the evidence is the shape of the distribution rather than any count the
    /// decoder kept. The runs gather into a short cluster and a long one about
    /// three times its length, which is what a dit and a dah are.</para>
    /// <para>The unit comes out at 57 ms, and that is worth writing down: the
    /// element gaps on this same recording were independently measured at 40 ms
    /// with a 57 ms dit when HM-DEC-115 was ruled, from a different direction and
    /// with different code. Two measurements agreeing is the nearest thing this
    /// project has to ground truth about a real signal.</para>
    /// </remarks>
    [Fact]
    public void TheKeptRecordingContainsSomebodyKeying()
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, "cw-2026-08-18-004507.wav"));

        var profile = KeyingEnvelope.Measure(audio, ToneHz);

        Report(profile);

        // **NOT FIFTEEN HUNDRED.** That figure, at a median of a few
        // milliseconds, is the signature of a gate chattering on noise.
        Assert.InRange(profile.RunsMs.Count, 80, 400);
        Assert.InRange(profile.MedianMs, 45, 70);
        Assert.True(
            profile.SwingDb > 15,
            $"the envelope only moved {profile.SwingDb:0.0} dB between quiet and loud");

        // Anything under twenty milliseconds is shorter than any element at any
        // speed this recording could be at, so it is edge and noise rather than
        // keying, and it is counted and set aside rather than quietly dropped.
        var elements = profile.RunsMs.Where(r => r >= 20).ToList();
        var dits = elements.Where(r => r < 100).ToList();
        var dahs = elements.Where(r => r >= 100).ToList();

        Assert.True(dits.Count >= 20, $"only {dits.Count} short elements");
        Assert.True(dahs.Count >= 20, $"only {dahs.Count} long elements");

        var dit = dits.Average();
        var dah = dahs.Average();

        _output.WriteLine($"dit {dit:0.0} ms x{dits.Count}, dah {dah:0.0} ms x{dahs.Count}");

        Assert.InRange(dit, 45, 70);
        Assert.InRange(dah, 130, 185);

        // **BIMODAL MEANS THE TWO HEAPS DO NOT TOUCH.** A smear with a mean in
        // each half would pass every test above and would not be keying.
        Assert.True(
            dits.Max() < dahs.Min(),
            $"the clusters overlap: longest short {dits.Max():0} ms, "
            + $"shortest long {dahs.Min():0} ms");

        Assert.InRange(dah / dit, 2.3, 3.6);
    }

    /// <remarks>
    /// <para>Proves §12.5: **the control.** Noise with no station in it, through
    /// the same method, must not produce two clusters. Without this, a pass above
    /// says only that the method returns numbers.</para>
    /// <para>What it does produce is the picture the two unreadable captures
    /// showed: a great many crossings, almost all of them shorter than any
    /// element anybody sends.</para>
    /// </remarks>
    [Fact]
    public void NoiseWithNobodyInItLooksLikeNoise()
    {
        var random = new Random(7300);
        var samples = new float[48_000 * 30];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((random.NextDouble() - 0.5) * 0.2);
        }

        var profile = KeyingEnvelope.Measure(new MonoAudio(48_000, samples), ToneHz);

        Report(profile);

        Assert.True(
            profile.MedianMs < 20,
            $"noise produced a median run of {profile.MedianMs:0} ms");

        var elements = profile.RunsMs.Where(r => r >= 20).ToList();

        Assert.True(
            elements.Count < profile.RunsMs.Count / 4,
            $"{elements.Count} of {profile.RunsMs.Count} noise runs were element length");
    }

    private void Report(KeyingProfile profile)
    {
        _output.WriteLine(
            $"runs {profile.RunsMs.Count}, median {profile.MedianMs:0} ms, "
            + $"swing {profile.SwingDb:0.0} dB");

        var bins = new SortedDictionary<int, int>();

        foreach (var run in profile.RunsMs)
        {
            var bin = (int)(run / 10) * 10;
            bins[bin] = bins.TryGetValue(bin, out var had) ? had + 1 : 1;
        }

        foreach (var (bin, count) in bins)
        {
            _output.WriteLine($"{bin,5}-{bin + 9,-5} ms  {count,5}");
        }
    }
}
