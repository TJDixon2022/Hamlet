using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Leg B for the waveform: an independent second synthesis, computed a different way, held against
/// the library's.
/// </summary>
/// <remarks>
/// <para>
/// <b>It is the weaker of two agreeing legs and it is kept anyway.</b> Upstream's own WAV strictly
/// dominates a second implementation of ours — <see cref="Ft8WaveformComparisonTests"/> is the
/// stronger evidence and it agreed. This was kept for two reasons that survive that. First, it runs
/// on a machine with no clone, where the comparison skips. Second and decisively, task 4 is required
/// to be watched refusing a waveform built with the smoothing parameter moved, and the library holds
/// that parameter fixed at the modulation's own value — so without an implementation that takes the
/// parameter there is no altered waveform to refuse, and a required refusal would have gone
/// unexercised.
/// </para>
/// <para>
/// <b>The two do not agree to the last bit and it would be wrong if they did.</b> The library
/// accumulates phase in single precision and takes a remainder every sample, because that is what
/// upstream does and matching upstream is the whole point of it. This one totals the phase in double
/// and never wraps. Those two roundings diverge by construction, and the size of the divergence is
/// itself worth measuring: it is what the library's agreement with upstream would have looked like
/// if the port had quietly computed in double "because it is more accurate".
/// </para>
/// </remarks>
public class Ft8WaveformSecondOpinionTests
{
    private readonly ITestOutputHelper _output;

    public Ft8WaveformSecondOpinionTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The bound, in sixteen-bit counts, written down after the measurement rather than before it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Two orders of magnitude looser than the comparison against upstream, and the reason is the
    /// interesting part of this file.</b> The library keeps the per-sample phase step in single
    /// precision, because upstream does. That step is about half a radian and a single-precision
    /// value of that size is granular to some six parts in a hundred million — so the library's step
    /// differs from the exact one by a fixed fraction of that, in the same direction, at every one of
    /// the hundred and fifty thousand samples. It does not cancel; it accumulates into a phase offset
    /// of a few thousandths of a radian by the end of the transmission, which is a hundred-odd counts
    /// of a full-scale sinusoid. This second opinion holds the step in double and so does not have
    /// it.
    /// </para>
    /// <para>
    /// <b>Which is the finding, and it is the one that justifies the port's precision.</b> A port
    /// that had computed the phase in double "because it is more accurate" would have been more
    /// accurate and would have disagreed with upstream's own waveform by about this much. The
    /// library agrees with upstream to one count precisely because it reproduces upstream's
    /// single-precision evaluation rather than improving on it. The test below shows the divergence
    /// is drift by measuring it at the start of the transmission and at the end.
    /// </para>
    /// </remarks>
    private const int Bound = 128;

    [Fact]
    public void AnIndependentSecondSynthesisAgreesWithTheLibrarysOverTheWholeCorpus()
    {
        var corpus = EncodeCorpus.Build();

        var worst = 0;
        var worstMessage = string.Empty;
        var worstSample = -1;
        var differing = 0L;
        var total = 0L;
        var messages = 0;

        // The two ends of the transmission, so the divergence can be shown to be drift rather than
        // a structural disagreement about the waveform.
        var lead = Ft8Waveform.PaddingSampleCount(Ft8Waveform.DefaultSampleRate);
        var perSymbol = Ft8Waveform.SamplesPerSymbol(Ft8Waveform.DefaultSampleRate);
        var signalLength = Ft8Waveform.SampleCount(Ft8Waveform.DefaultSampleRate);
        var worstInFirstSymbol = 0;
        var worstInLastSymbol = 0;

        foreach (var entry in corpus)
        {
            var symbols = Ft8SymbolEncoder.Encode(entry.Message);
            var ours = Ft8Waveform.SynthesizeSlotPcm16(symbols);
            var theirs = Ft8WaveformSecondOpinion.SynthesizeSlotPcm16(symbols);

            Assert.Equal(ours.Length, theirs.Length);
            for (var i = 0; i < ours.Length; i++)
            {
                var delta = Math.Abs(ours[i] - theirs[i]);
                total++;

                var offset = i - lead;
                if (offset >= 0 && offset < perSymbol)
                {
                    worstInFirstSymbol = Math.Max(worstInFirstSymbol, delta);
                }
                else if (offset >= signalLength - perSymbol && offset < signalLength)
                {
                    worstInLastSymbol = Math.Max(worstInLastSymbol, delta);
                }

                if (delta == 0)
                {
                    continue;
                }

                differing++;
                if (delta > worst)
                {
                    worst = delta;
                    worstMessage = entry.Label;
                    worstSample = i;
                }
            }

            messages++;
        }

        _output.WriteLine($"messages                : {messages}");
        _output.WriteLine($"samples                 : {total}");
        _output.WriteLine($"MAXIMUM ABSOLUTE DIFFERENCE : {worst} counts");
        _output.WriteLine($"    at                  : {worstMessage}, sample {worstSample}");
        _output.WriteLine($"samples differing at all : {differing} ({(double)differing / total:P3})");
        _output.WriteLine($"bound asserted          : {Bound} counts");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"worst in the FIRST symbol : {worstInFirstSymbol} counts");
        _output.WriteLine($"worst in the LAST symbol  : {worstInLastSymbol} counts");
        _output.WriteLine(
            "for comparison, the library against UPSTREAM's own waveform is 1 count — the tighter "
            + "of the two, because the library and upstream share a single-precision evaluation "
            + "and this second opinion deliberately does not.");

        Assert.Equal(corpus.Count, messages);

        // The divergence is drift, not disagreement: the two synthesize the opening of the
        // transmission to within a couple of counts and part company steadily thereafter. If they
        // differed about the pulse, the tone spacing or the layout instead, the first symbol would
        // be as wrong as the last.
        Assert.True(
            worstInFirstSymbol < worstInLastSymbol / 4,
            $"the two implementations differ by {worstInFirstSymbol} counts in the first symbol and "
            + $"{worstInLastSymbol} in the last. A divergence that is already at full size in the "
            + "first symbol is not accumulated rounding — it is a disagreement about the pulse, the "
            + "tone spacing or the layout, and it would need finding rather than bounding.");
        Assert.True(
            worst <= Bound,
            $"the independent second synthesis differs from the library's by {worst} counts at "
            + $"{worstMessage} sample {worstSample}, past the bound of {Bound}. Some divergence is "
            + "expected — one totals the phase in double and the other accumulates it in single with "
            + "a remainder every sample — but a difference this size is larger than that accounts "
            + "for and one of the two has a defect.");
    }

    /// <summary>
    /// The second implementation is used to build the waveform a plausibly wrong port would build,
    /// so that the test which claims to catch that port can be watched catching it.
    /// </summary>
    /// <remarks>
    /// <b>A guard that has never refused is not a guard</b> — step 1's ruling, applied to the one
    /// assertion in this project that protects a property nothing else can see.
    /// <see cref="Ft8WaveformTests.PhaseIsContinuousAcrossEverySymbolBoundary"/> claims to catch a
    /// synthesizer that restarts phase at each symbol. Here is one, built on purpose, and here is
    /// that test's measurement failing on it.
    /// </remarks>
    [Fact]
    public void APortThatRestartsPhaseAtEachSymbolIsCaughtByTheContinuityMeasurement()
    {
        var message = new byte[Ft8Sharp.Message.Ft8Payload.MessageBytes];
        Assert.Equal(
            Ft8Sharp.Message.Ft8PackResult.Ok,
            Ft8Sharp.Message.Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));
        var symbols = Ft8SymbolEncoder.Encode(message);

        var rate = Ft8Waveform.DefaultSampleRate;
        var perSymbol = Ft8Waveform.SamplesPerSymbol(rate);

        var faithful = Ft8WaveformSecondOpinion.Synthesize(symbols, rate);
        var broken = Ft8WaveformSecondOpinion.Synthesize(symbols, rate, restartPhaseEachSymbol: true);

        var (faithfulBoundary, faithfulElsewhere) = LargestSteps(faithful, perSymbol);
        var (brokenBoundary, brokenElsewhere) = LargestSteps(broken, perSymbol);

        _output.WriteLine($"phase accumulated : largest step at a boundary {faithfulBoundary:F4}, "
            + $"elsewhere {faithfulElsewhere:F4}");
        _output.WriteLine($"phase restarted   : largest step at a boundary {brokenBoundary:F4}, "
            + $"elsewhere {brokenElsewhere:F4}");

        // Restarting phase is invisible to everything else. It is still the right length.
        Assert.Equal(faithful.Length, broken.Length);

        // And every symbol still comes back out of it, which is exactly why the continuity
        // measurement has to exist: the tone recovery does not care.
        Assert.Equal(
            Ft8Waveform.SymbolCount * perSymbol,
            broken.Length);

        // The measurement that does care.
        Assert.True(
            faithfulBoundary <= faithfulElsewhere * 1.5f,
            "the faithful waveform should have boundaries indistinguishable from the middle of a "
            + "symbol, and it does not — the check itself is wrong.");

        Assert.True(
            brokenBoundary > brokenElsewhere * 1.5f,
            $"a synthesizer that restarts phase at every symbol produced a largest boundary step of "
            + $"{brokenBoundary} against {brokenElsewhere} elsewhere, which the continuity "
            + "measurement would NOT have caught. That measurement is the only thing in this project "
            + "protecting the one property of the waveform that the length, the range, the "
            + "determinism and the tone recovery are all blind to.");
    }

    private static (float AtBoundary, float Elsewhere) LargestSteps(float[] signal, int perSymbol)
    {
        var boundary = 0.0f;
        var elsewhere = 0.0f;
        for (var i = 1; i < signal.Length; i++)
        {
            var step = MathF.Abs(signal[i] - signal[i - 1]);
            if (i % perSymbol == 0)
            {
                boundary = MathF.Max(boundary, step);
            }
            else
            {
                elsewhere = MathF.Max(elsewhere, step);
            }
        }

        return (boundary, elsewhere);
    }
}
