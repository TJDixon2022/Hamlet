using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **WHAT THE SENDERS WERE** (work instruction 548, task 4): until a capture runs from the moment Hamlet starts listening, W1AW's
/// recording replayed after five minutes of band noise and a second, weaker station at 650 Hz that stops a minute before it,
/// to see whether a sender held from before can take the terminal from W1AW.
/// </summary>
/// <remarks>
/// <para>**THE NOISE** is white, at the recording's own floor: the tenth percentile of its 20 ms root mean squares, so the band
/// before W1AW sounds like the band under it. **THE SECOND STATION** keys `CQ CQ DE K1ABC K` at 18 WPM, 650 Hz, 10 dB under
/// W1AW's peak, for the first four minutes. R88 is lifted for W1AW's capture; the rest is synthetic.</para>
/// </remarks>
public sealed class ASenderHeldFromBeforeTests(ITestOutputHelper output)
{
    /// <remarks>Prints the senders, the handovers and what printed in the last 30 s, W1AW's; asserts only that W1AW printed.</remarks>
    [Fact]
    public void W1awAfterFiveMinutesOfBand()
    {
        var w1aw = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(AStationsShadowIsNotAStationTests.W1aw));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(AStationsShadowIsNotAStationTests.W1aw);
        var rate = w1aw.SampleRate;

        var rms = Enumerable.Range(0, w1aw.Samples.Length / (rate / 50))
            .Select(i => Math.Sqrt(w1aw.Samples.Skip(i * (rate / 50)).Take(rate / 50).Average(s => (double)s * s)))
            .Order().ToList();
        var floor = rms[rms.Count / 10];
        var peak = w1aw.Samples.Max(s => Math.Abs(s));

        var before = 300 * rate;
        var x = new float[before + w1aw.Samples.Length];
        var random = new Random(5480);

        for (var k = 0; k < before; k++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            x[k] = (float)(floor * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }

        var second = CwSignal.Generate(new CwSignalRequest(
            string.Join(' ', Enumerable.Repeat("CQ CQ DE K1ABC K", 40)), WordsPerMinute: 18, ToneHz: 650, SampleRate: rate,
            Amplitude: peak / Math.Sqrt(10), LeadInSeconds: 5, TailSeconds: 0)).Samples;

        for (var k = 0; k < second.Length && k < 240 * rate; k++)
        {
            x[k] += second[k];
        }

        w1aw.Samples.CopyTo(x, before);

        var t = AStationsShadowIsNotAStationTests.Trace(x, rate, pitch, width);
        var start = before / (double)rate;

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"noise at {20 * Math.Log10(floor):0.0} dBFS rms; W1AW peak {20 * Math.Log10(peak):0.0} dBFS; the second station at {20 * Math.Log10(peak / Math.Sqrt(10)):0.0} dBFS peak, 0 to 240 s"));
        output.WriteLine($"senders at the end: {t.Senders}");

        foreach (var (at, from, to) in t.Handovers)
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"handover at {at:0.00} s: {from:0} -> {to:0} Hz"));
        }

        var during = t.Letters.Where(l => l.To >= start).ToList();

        output.WriteLine($"printed in W1AW's 30 s: {string.Concat(during.Select(l => Math.Abs(l.PitchHz - 600) > CwSenderGate.PitchToleranceHz ? $"[{l.Text}@{l.PitchHz:0}]" : l.Text))}");
        output.WriteLine($"printed before it: {string.Concat(t.Letters.Where(l => l.To < start).Select(l => l.Text)).Length} letters, {t.Letters.Count(l => l.To < start && Math.Abs(l.PitchHz - 650) <= 30)} at 650 Hz");

        Assert.Contains(during, l => Math.Abs(l.PitchHz - 600) <= CwSenderGate.PitchToleranceHz);
    }
}
