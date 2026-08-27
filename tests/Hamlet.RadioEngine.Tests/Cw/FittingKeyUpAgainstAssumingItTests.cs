using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The fitted key-up model against the shipped one, on the whole corpus.
/// </summary>
/// <remarks>
/// <para>**MEASURED BEFORE IT IS WIRED TO ANYTHING** (work instruction 035, task
/// 4: *"be willing to lose"*). The shipped model scores key-up as a Rayleigh at
/// the noise scale; the fitted one gives it its own location and width, taken
/// from the same local span.</para>
/// <para>**THE FIRST ACCEPTANCE LINE IS THE SILENCE PROPERTY**, and it is checked
/// here rather than at the end: a recording holding nothing must score no higher
/// under the new model than under the old.</para>
/// </remarks>
public sealed class FittingKeyUpAgainstAssumingItTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparison is printed.</param>
    public FittingKeyUpAgainstAssumingItTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static MonoAudio Read(string relative)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", relative + ".wav"));

    private static MonoAudio Tail(MonoAudio audio, double seconds)
    {
        var want = (int)(audio.SampleRate * seconds);

        if (audio.Samples.Length <= want)
        {
            return audio;
        }

        var slice = new float[want];

        for (var i = 0; i < want; i++)
        {
            slice[i] = audio.Samples[audio.Samples.Length - want + i];
        }

        return new MonoAudio(audio.SampleRate, slice);
    }

    /// <summary>
    /// The window ratio a stream of log-likelihoods implies, which is the
    /// quantity <see cref="CwProbabilisticDecoder.Gate"/> is expressed in.
    /// </summary>
    /// <remarks>
    /// **THE SAME ARITHMETIC THE DECODER DOES**, and it has to be, or the two
    /// columns are measured on different scales and the comparison says nothing.
    /// The decoder scores the best keying path against the all-key-up hypothesis
    /// and divides by the hop count; the best possible path is bounded above by
    /// taking whichever hypothesis wins at every hop, so this is that bound.
    /// </remarks>
    private static double Ceiling(double[] keyDown, double[] keyUp)
    {
        if (keyUp.Length == 0)
        {
            return 0;
        }

        var nothing = 0.0;
        var best = 0.0;

        for (var i = 0; i < keyUp.Length; i++)
        {
            nothing += keyUp[i];
            best += Math.Max(keyDown[i], keyUp[i]);
        }

        return (best - nothing) / keyUp.Length;
    }

    /// <summary>Every recording this comparison is judged on.</summary>
    private static (string Path, double ToneHz, string What)[] Cases { get; } =
    {
        ("unadjudicated/cw-2026-08-22-014113", 606.0, "unread, he hears it"),
        ("unadjudicated/cw-2026-08-22-014308", 606.0, "unread, he hears it"),
        ("unadjudicated/cw-2026-08-26-125941", 403.5, "unread, he hears it"),
        ("unadjudicated/cw-2026-08-24-012403", 439.8, "READS - control"),
        ("cw-2026-08-17-013347", 600.0, "READS - VA3VRR"),
        ("cw-2026-08-17-134712", 500.0, "READS - N4L"),
        ("cw-2026-08-18-004507", 501.0, "READS - the bulletin"),
        ("unadjudicated/cw-2026-08-20-014854", 600.0, "HOLDS NOTHING"),
        ("unadjudicated/cw-2026-08-20-014935", 825.0, "HOLDS NOTHING"),
    };

    /// <remarks>
    /// <para>**THE WHOLE UNIT IN ONE TABLE.** For each recording, the window
    /// ratio the shipped model reaches and the one the fitted model reaches,
    /// against the gate of 1.40.</para>
    /// <para>**WHAT WOULD MAKE THIS CHANGE WORTH SHIPPING**: the three unread
    /// captures rise over the gate, the recordings holding nothing do not, and
    /// the captures that already read do not fall under it. **Anything else and
    /// it is reverted**, which is what the order asks for and what this test is
    /// arranged to show at a glance.</para>
    /// </remarks>
    [Fact]
    public void WhatFittingKeyUpDoesToEveryRecording()
    {
        var window = CwProbabilisticStream.WindowSeconds;
        var span = CwProbabilisticDecoder.NoiseSpanSeconds;

        _output.WriteLine(
            $"  12 s window, gate {CwProbabilisticDecoder.Gate:0.00} per hop");
        _output.WriteLine("");
        _output.WriteLine(
            "  recording                          | assumed | fitted | move   | what");
        _output.WriteLine(
            "  -----------------------------------|---------|--------|--------|-----");

        var brokeSilence = new List<string>();
        var liftedUnread = new List<string>();
        var lostAReader = new List<string>();

        foreach (var (path, toneHz, what) in Cases)
        {
            var slice = Tail(Read(path), window);

            var envelope = CwProbabilisticDecoder.Envelope(
                slice.Samples, slice.SampleRate, toneHz);

            var (downA, upA) = CwProbabilisticDecoder.LogLikelihoods(envelope, span);
            var (downF, upF) =
                CwProbabilisticDecoder.FittedLogLikelihoods(envelope, span);

            var assumed = Ceiling(downA, upA);
            var fitted = Ceiling(downF, upF);

            var name = path.Replace("unadjudicated/", "");

            if (what.Contains("HOLDS NOTHING", StringComparison.Ordinal)
                && fitted > assumed)
            {
                brokeSilence.Add($"{name} {assumed:0.00} -> {fitted:0.00}");
            }

            if (what.Contains("unread", StringComparison.Ordinal)
                && fitted >= CwProbabilisticDecoder.Gate)
            {
                liftedUnread.Add(name);
            }

            if (what.Contains("READS", StringComparison.Ordinal)
                && assumed >= CwProbabilisticDecoder.Gate
                && fitted < CwProbabilisticDecoder.Gate)
            {
                lostAReader.Add(name);
            }

            _output.WriteLine(
                $"  {name,-34} | {assumed,7:0.00} | {fitted,6:0.00} | "
                + $"{fitted - assumed,+6:+0.00;-0.00} | {what}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  unread captures lifted over the gate: {liftedUnread.Count} of 3"
            + (liftedUnread.Count > 0
                ? " - " + string.Join(", ", liftedUnread) : ""));
        _output.WriteLine(
            $"  recordings holding nothing that scored higher: {brokeSilence.Count} of 2"
            + (brokeSilence.Count > 0
                ? " - " + string.Join(", ", brokeSilence) : ""));
        _output.WriteLine(
            $"  readers pushed under the gate: {lostAReader.Count}"
            + (lostAReader.Count > 0
                ? " - " + string.Join(", ", lostAReader) : ""));

        // **NOTHING IS ASSERTED ABOUT THE OUTCOME.** This is the measurement that
        // decides whether the change ships, and a test that demanded either
        // answer would be deciding it in advance.
        Assert.Equal(9, Cases.Length);
    }
}
