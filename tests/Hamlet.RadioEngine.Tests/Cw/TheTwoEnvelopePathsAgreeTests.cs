using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The offline envelope and the streaming one are the same filter, and the
/// integrator is the shape it says it is.
/// </summary>
/// <remarks>
/// <para>**THERE ARE TWO ENVELOPE PATHS AND NOTHING HAS EVER CHECKED THEY
/// MATCH.** The offline one lays a centred window over each hop and the
/// streaming one sums the audio behind the newest sample, so they differ by half
/// a window in time and always did. That is a known and accepted difference; what
/// is not acceptable is the two disagreeing about the *shape* of the filter,
/// because then a measurement taken through one is not a fact about the other
/// (HM-DEC-119).</para>
/// <para>**AND A TAPER IN A RING BUFFER IS THE EASIEST THING IN THIS FILE TO GET
/// SILENTLY WRONG.** A boxcar can be summed in any order; a weighted window
/// cannot. Weighting by array index rather than by age rotates the taper against
/// the signal once per fill, which produces an envelope that is wrong in a way no
/// single test of the decoder would name.</para>
/// </remarks>
public sealed class TheTwoEnvelopePathsAgreeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the comparison is printed.</param>
    public TheTwoEnvelopePathsAgreeTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Proves the integrator is the width it claims. The equivalent noise
    /// bandwidth of a window is the sample rate times the sum of its squared
    /// weights over the square of their sum, which is arithmetic on the taper
    /// itself and owes nothing to the constant beside it.
    /// </remarks>
    [Fact]
    public void TheIntegratorIsAsWideAsItSaysItIs()
    {
        const int rate = 48_000;

        foreach (var wanted in new[] { 60.0, 45.0, 30.0, 20.0 })
        {
            var window = CwProbabilisticDecoder.IntegratorWindow(rate, wanted);
            var taper = CwProbabilisticDecoder.IntegratorTaper(window);

            var sum = taper.Sum();
            var enbw = rate * taper.Sum(w => w * w) / (sum * sum);

            _output.WriteLine(
                $"asked {wanted,5:0.0} Hz -> {window,5} samples "
                + $"({window * 1000.0 / rate,6:0.00} ms), measured {enbw,6:0.00} Hz");

            Assert.True(
                Math.Abs(enbw - wanted) < 0.5,
                $"asked for {wanted:0.0} Hz and the taper measures {enbw:0.00} Hz.");
        }
    }

    /// <remarks>
    /// Proves the two paths run the same filter. The streaming path is half a
    /// window late by construction, so the comparison lines them up by that
    /// offset and then asks whether they agree; if the shapes differed no
    /// alignment would make them.
    /// </remarks>
    [Fact]
    public void TheStreamingEnvelopeIsTheOfflineOneDelayedByHalfAWindow()
    {
        var audio = CwSignal.Generate(new CwSignalRequest(
            "CQ DE N0CALL K",
            WordsPerMinute: 18,
            ToneHz: 600,
            Amplitude: 0.5,
            NoiseAmplitude: 0.02,
            Seed: 11));

        var offline = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, 600);

        var streamed = Streamed(audio.Samples, audio.SampleRate, 600);

        var window = CwProbabilisticDecoder.IntegratorWindow(
            audio.SampleRate, CwProbabilisticDecoder.IntegratorBandwidthHz);

        var hop = (int)(audio.SampleRate
            * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

        // **THE LAG IS NOT SIMPLY HALF A WINDOW, AND ASSUMING IT WAS COST AN
        // HOUR.** The offline window is centred on its hop. The streaming window
        // *ends* on the last sample of its hop, and it is pushed after that hop
        // completes, so its centre sits half a window plus one sample behind the
        // end of hop n, which is a whole hop later than hop n's own centre. The
        // two corrections pull in opposite directions and do not cancel: at eight
        // kilohertz it is 2.35 hops rather than 3.33.
        //
        // It is also not a whole number of hops, so the offline envelope is read
        // between its own samples rather than at the nearest one. Rounding
        // compares the two a millisecond and a half apart, which on the rising
        // edge of a mark is a fifth of the peak and looks exactly like a broken
        // filter.
        var lag = (((window - 1) / 2.0) + 1) / hop - 1;

        var peak = offline.Max();
        var worst = 0.0;
        var compared = 0;

        // The first window's worth of each is skipped: one is filling its ring
        // and the other is running off the end of the recording, and neither is
        // the filter's steady state.
        // **THE STREAMING ENVELOPE IS LATE, SO IT MATCHES THE OFFLINE ONE
        // EARLIER.** The offline window is centred on its hop and the streaming
        // window ends on it, so the streaming reading at hop n describes the
        // audio the offline reading at hop n minus the lag describes.
        for (var n = (int)Math.Ceiling(lag) + (window / hop);
            n < streamed.Count && n < offline.Length;
            n++)
        {
            var at = n - lag;
            var below = (int)at;
            var share = at - below;

            var interpolated = (offline[below] * (1 - share))
                + (offline[below + 1] * share);

            worst = Math.Max(worst, Math.Abs(interpolated - streamed[n]));
            compared++;
        }

        _output.WriteLine(
            $"window {window} samples, lag {lag:0.00} hops, {compared} hops compared");
        _output.WriteLine(
            $"worst disagreement {worst:0.000000} against a peak of {peak:0.000000} "
            + $"({worst / peak:P2} of peak)");

        Assert.True(compared > 100, "not enough of the envelope was compared.");

        // Five per cent at the worst hop, against the sixty a rotated taper or a
        // different window shape produces. The residue is the interpolation's own
        // error on the steepest edges, and it is a bound on the disagreement
        // rather than a measurement of it.
        Assert.True(
            worst < peak * 0.05,
            $"the two envelope paths disagree by {worst / peak:P1} of the peak, "
            + "which is more than the alignment can account for.");
    }

    private static IReadOnlyList<double> Streamed(
        float[] samples, int rate, double toneHz)
    {
        var stream = new CwProbabilisticStream(rate) { ToneHz = toneHz };
        var taken = new List<double>();

        // The stream keeps only its own rolling window, so the envelope is read
        // out hop by hop as it is produced rather than asked for at the end.
        var hop = (int)(rate * CwProbabilisticDecoder.HopMilliseconds / 1000.0);
        var seen = 0;

        for (var at = 0; at + hop <= samples.Length; at += hop)
        {
            stream.Process(samples.AsSpan(at, hop));

            if (stream.EnvelopeHops > seen)
            {
                seen = stream.EnvelopeHops;
                taken.Add(stream.NewestEnvelope);
            }
            else
            {
                taken.Add(stream.NewestEnvelope);
            }
        }

        return taken;
    }
}
