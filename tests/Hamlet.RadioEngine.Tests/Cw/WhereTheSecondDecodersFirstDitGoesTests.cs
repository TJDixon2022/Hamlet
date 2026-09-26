using System.Globalization;
using Hamlet.RadioEngine.Cw.Second;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// A trace for HM-REQ-122 (work instruction 457, task 2): the port's state at
/// every decision sample through the case of
/// <see cref="TheSecondDecoderIsAFaithfulPortTests"/>, from the last second of
/// its noise lead-in to the end of its second letter. Proves no requirement.
/// </summary>
/// <remarks>
/// <para>**A PRINTER THAT ASSERTS NOTHING.** It answers one question: whether
/// fldigi's detector is already keyed down on noise when the first dit
/// arrives, or whether its AGC has not risen far enough for the dit to cross
/// <c>CWupper</c>.</para>
/// <para>**THE TIMES.** A decision sample is every sixteenth filtered sample,
/// 500 Hz. The filter's output lags its input by 512 samples (the sinc is
/// centred in its 1024 taps), so <c>t_in</c> is the filtered sample less 513,
/// over 8000: the input time the row stands for. The 16-sample bit filter adds
/// a further 15 ms, which is not subtracted.</para>
/// </remarks>
public sealed class WhereTheSecondDecodersFirstDitGoesTests
{
    private const int Rate = FldigiCwDecoder.CW_SAMPLERATE;
    private const int FilterDelay = 512;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the printer.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public WhereTheSecondDecodersFirstDitGoesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Every decision sample from a second before the first mark to the end of the second letter.</summary>
    [Fact]
    public void ThroughTheCaseSampleBySample()
    {
        var (audio, runs, _) = TheSecondDecoderIsAFaithfulPortTests.Render();
        var marks = runs.Where(r => r.On).ToList();

        var decoder = new FldigiCwDecoder(600) { TraceDecisions = true };
        decoder.rx_process(audio);

        _output.WriteLine($"HM-REQ-122 | trace | emitted `{decoder.Text}` | {decoder.Decisions.Count} decision samples");

        foreach (var (mark, i) in marks.Take(6).Select((m, i) => (m, i)))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"mark | {i + 1} | {mark.Start / (double)Rate:0.0000} s to {mark.End / (double)Rate:0.0000} s | {mark.End - mark.Start} samples"));
        }

        var first = marks[0].Start / (double)Rate;
        var endOfSecondLetter = marks[5].End / (double)Rate;

        double InputTime(FldigiCwDecisionRow r) => (r.FilteredSample - 1 - FilterDelay) / (double)Rate;

        var lead = decoder.Decisions.Where(r => InputTime(r) >= first - 1.0 && InputTime(r) < first).ToList();
        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"lead-in last second | {lead.Count} samples | in tone {lead.Count(r => r.State == "tone")} | "
            + $"downs {lead.Count(r => r.Events.Contains("down", StringComparison.Ordinal))} | "
            + $"spikes {lead.Count(r => r.Events.Contains("spike", StringComparison.Ordinal))} | "
            + $"ups {lead.Count(r => r.Events.Contains("up ", StringComparison.Ordinal))} | "
            + $"mean CWupper {lead.Average(r => r.Upper):0.0000} | mean CWlower {lead.Average(r => r.Lower):0.0000} | "
            + $"mean value {lead.Average(r => r.Value):0.0000} | mean magnitude {lead.Average(r => r.Magnitude):0.00000} | "
            + $"agc_peak {lead[^1].AgcPeak:0.00000} | noise_floor {lead[^1].NoiseFloor:0.00000} | sig_avg {lead[^1].SigAvg:0.00000}"));

        _output.WriteLine("row | t_in s | t_filt s | magnitude | value | agc_peak | noise_floor | sig_avg | CWupper | CWlower | state | smpl_ctr | two_dots | events | printed | held");

        foreach (var r in decoder.Decisions.Where(r => InputTime(r) >= first - 1.0 && InputTime(r) <= endOfSecondLetter + 0.25))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"row | {InputTime(r):0.0000} | {r.FilteredSample / (double)Rate:0.0000} | {r.Magnitude:0.00000} | {r.Value:0.0000} | "
                + $"{r.AgcPeak:0.00000} | {r.NoiseFloor:0.00000} | {r.SigAvg:0.00000} | {r.Upper:0.0000} | {r.Lower:0.0000} | "
                + $"{r.State} | {r.SmplCtr} | {r.TwoDots} | {r.Events} | {r.Printed} | {r.Held}"));
        }
    }
}
