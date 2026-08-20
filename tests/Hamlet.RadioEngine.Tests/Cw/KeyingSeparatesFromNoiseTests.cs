using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The measurement a live meter would run, cut into live-sized windows and
/// swept for its own pitch (HM-DEC-091, §12.5).
/// </summary>
/// <remarks>
/// <para>**A METER IS ONLY WORTH BUILDING IF THE MEASUREMENT SURVIVES BEING CUT
/// SHORT.** Half a minute of audio can be looked at whole; a meter has six
/// seconds and has to choose its own pitch, because the case it exists for is the
/// one where the decoder has not found a tone at all.</para>
/// <para>**AND IT MUST NOT BE HANDED THE ANSWER.** The sweep runs 400 to 1200 Hz
/// in 25 Hz steps and takes the pitch that most looks like keying, so nothing
/// the decoder concluded reaches it. On one recording the decoder chose 800 Hz
/// while the narrow content sat at 608.</para>
/// <para>These print the table rather than only asserting it, because the numbers
/// are what a later session will compare against.</para>
/// </remarks>
public sealed class KeyingSeparatesFromNoiseTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public KeyingSeparatesFromNoiseTests(ITestOutputHelper output) => _output = output;

    /// <summary>How long a window the meter gets, in seconds.</summary>
    public const double WindowSeconds = 6;

    /// <summary>
    /// Every recording in the repository, adjudicated or not, swept window by
    /// window.
    /// </summary>
    /// <remarks>
    /// **THE UNADJUDICATED ONES ARE INCLUDED AND ARE NOT JUDGED HERE.** Whether
    /// there was a readable station in any of them is Tim's ear and not a
    /// session's (§12.5). What is measured is what the envelope did, which is a
    /// fact about the audio and needs nobody's verdict.
    /// </remarks>
    public static TheoryData<string> Recordings
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var wav in Directory
                         .GetFiles(CapturedSignalTests.Folder, "*.wav", SearchOption.AllDirectories)
                         .OrderBy(p => p, StringComparer.Ordinal))
            {
                data.Add(Path.GetRelativePath(CapturedSignalTests.Folder, wav));
            }

            return data;
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: every recording the repository holds, measured the
    /// way a meter would have to measure it, with the pitch swept rather than
    /// taken from the decoder.</para>
    /// <para>**IT ASSERTS ONLY THAT THE MEASUREMENT RUNS AND IS BOUNDED.** What
    /// separates keyed audio from noise is asserted below, on the two cases where
    /// the truth is known; this one exists to print the table for every file, so a
    /// later session can see what moved.</para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(Recordings))]
    public void EveryRecordingIsSweptWindowByWindow(string name)
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name));

        var windows = Sweep(audio).ToList();

        Assert.NotEmpty(windows);

        _output.WriteLine($"{name}: {windows.Count} windows of {WindowSeconds:0} s");

        foreach (var (tone, profile) in windows)
        {
            _output.WriteLine(
                $"  tone {tone,6:0} Hz  median {profile.MedianMs,5:0} ms  "
                + $"swing {profile.SwingDb,5:0.0} dB  runs {profile.RunsMs.Count,5}  "
                + $"score {profile.Score,5:0.00}");
        }

        var medians = windows.Select(w => w.Profile.MedianMs).OrderBy(v => v).ToList();

        _output.WriteLine(
            $"  MEDIAN OF WINDOWS {medians[medians.Count / 2]:0} ms, "
            + $"tones {windows.Min(w => w.ToneHz):0} to {windows.Max(w => w.ToneHz):0} Hz");

        Assert.All(windows, w => Assert.InRange(
            w.ToneHz, KeyingEnvelope.LowestToneHz, KeyingEnvelope.HighestToneHz));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-091: **the recording that decoded reads as keying in
    /// every one of its windows**, with the pitch found by sweeping rather than
    /// given. This is the upper half of the separation the meter rests on.</para>
    /// <para>The window medians are asserted individually rather than only as an
    /// overall median, because a meter sees one window at a time and an average
    /// that hides a window at seven milliseconds would be an average hiding the
    /// case that matters.</para>
    /// </remarks>
    [Fact]
    public void TheRecordingThatDecodedReadsAsKeyingInEveryWindow()
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, "cw-2026-08-18-004507.wav"));

        var windows = Sweep(audio).ToList();

        foreach (var (tone, profile) in windows)
        {
            _output.WriteLine(
                $"tone {tone:0} Hz  median {profile.MedianMs:0} ms  "
                + $"swing {profile.SwingDb:0.0} dB  score {profile.Score:0.00}");
        }

        Assert.All(windows, w => Assert.InRange(w.Profile.MedianMs, 30, 120));
        Assert.All(windows, w => Assert.True(
            w.Profile.SwingDb > 18,
            $"a window only swung {w.Profile.SwingDb:0.0} dB"));
    }

    /// <remarks>
    /// <para>Proves §12.5: **the control, and it is the half that makes the other
    /// half mean anything.** Noise with nobody in it, through the same sweep,
    /// must not read as keying in any window. Without this a pass above says only
    /// that the method returns numbers.</para>
    /// <para>The sweep is given every chance to find something: eight hundred
    /// hertz of candidates and the widest swing among them wins.</para>
    /// </remarks>
    [Fact]
    public void NoiseReadsAsNoiseInEveryWindow()
    {
        var random = new Random(7300);
        var samples = new float[48_000 * 30];

        for (var i = 0; i < samples.Length; i++)
        {
            samples[i] = (float)((random.NextDouble() - 0.5) * 0.2);
        }

        var windows = Sweep(new MonoAudio(48_000, samples)).ToList();

        foreach (var (tone, profile) in windows)
        {
            _output.WriteLine(
                $"tone {tone:0} Hz  median {profile.MedianMs:0} ms  "
                + $"swing {profile.SwingDb:0.0} dB  score {profile.Score:0.00}");
        }

        Assert.All(windows, w => Assert.True(
            w.Profile.MedianMs < 20,
            $"noise produced a {w.Profile.MedianMs:0} ms median at {w.ToneHz:0} Hz"));
    }

    private static IEnumerable<KeyingSighting> Sweep(MonoAudio audio)
    {
        var length = (int)(audio.SampleRate * WindowSeconds);

        for (var start = 0; start + length <= audio.Samples.Length; start += length)
        {
            var slice = new float[length];

            Array.Copy(audio.Samples, start, slice, 0, length);

            if (KeyingEnvelope.Best(new MonoAudio(audio.SampleRate, slice)) is { } best)
            {
                yield return best;
            }
        }
    }
}
