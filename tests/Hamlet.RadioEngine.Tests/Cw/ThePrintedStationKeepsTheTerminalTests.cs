using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE PRINTED STATION KEEPS THE TERMINAL** (work instruction 550, task 3, HM-DEC-254): W1AW's recording after a minute of a
/// weaker station at 650 Hz calling, which falls quiet as W1AW starts and calls again beside it.
/// </summary>
/// <remarks>
/// <para>**THE SECOND STATION** keys `CQ CQ DE K1ABC K1ABC K` at 18 WPM, 650 Hz, 10 dB under W1AW's peak, synthetic and clean,
/// so it scores as high as W1AW or higher: from 5 s, every 15 s, quiet from 50 s to 70 s, then calling again beside W1AW. The
/// band before W1AW is white noise at the recording's own floor. R88 is lifted for W1AW's capture; the rest is synthetic.</para>
/// <para>**BEFORE THIS UNIT** the terminal went to any qualified sender scoring higher at a pause, and a clean synthetic call
/// scores above W1AW, so W1AW's pauses could hand it over. Now a challenger must have qualified for 15 s and score 0.16 more.</para>
/// </remarks>
public sealed class ThePrintedStationKeepsTheTerminalTests(ITestOutputHelper output)
{
    [Fact]
    public void W1awKeepsTheTerminalBesideAWeakerStationAt650()
    {
        var w1aw = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(AStationsShadowIsNotAStationTests.W1aw));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(AStationsShadowIsNotAStationTests.W1aw);
        var rate = w1aw.SampleRate;

        var rms = Enumerable.Range(0, w1aw.Samples.Length / (rate / 50))
            .Select(i => Math.Sqrt(w1aw.Samples.Skip(i * (rate / 50)).Take(rate / 50).Average(s => (double)s * s)))
            .Order().ToList();
        var floor = rms[rms.Count / 10];
        var peak = w1aw.Samples.Max(s => Math.Abs(s));

        var before = 60 * rate;
        var x = new float[before + w1aw.Samples.Length];
        var random = new Random(5500);

        for (var k = 0; k < before; k++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = random.NextDouble();

            x[k] = (float)(floor * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2));
        }

        var call = CwSignal.Generate(new CwSignalRequest(
            "CQ CQ DE K1ABC K1ABC K", WordsPerMinute: 18, ToneHz: 650, SampleRate: rate,
            Amplitude: peak / Math.Sqrt(10), LeadInSeconds: 0, TailSeconds: 0)).Samples;
        var starts = new[] { 5.0, 20, 35, 70, 85 };

        foreach (var start in starts)
        {
            var at = (int)(start * rate);

            for (var k = 0; k < call.Length && at + k < x.Length; k++)
            {
                x[at + k] += call[k];
            }
        }

        for (var k = 0; k < w1aw.Samples.Length; k++)
        {
            x[before + k] += w1aw.Samples[k];
        }

        var t = AStationsShadowIsNotAStationTests.Trace(x, rate, pitch, width);

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"one call {call.Length / (double)rate:0.0} s, at {string.Join(", ", starts)} s; W1AW from 60 s"));
        output.WriteLine($"senders at the end: {t.Senders}");

        foreach (var (at, from, to) in t.Handovers)
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"handover at {at:0.00} s: {from:0} -> {to:0} Hz"));
        }

        foreach (var s in TheShapeScoreTakenApartTests.Run(x, rate, pitch, width, "w1aw beside 650").Where(s => !s.Detector && s.Seconds >= 60))
        {
            output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"  {s.Seconds:0} s: {s.PitchHz:0} Hz shape {s.Shape.Score:0.00}{(s.Qualified ? ", qualified" : string.Empty)}{(s.Printed ? ", printed" : string.Empty)}"));
        }

        var w1awFrom =t.Letters.FirstOrDefault(l => l.From >= 60 && Math.Abs(l.PitchHz - 600) <= CwSenderGate.PitchToleranceHz)?.From ?? double.NaN;
        var after = t.Letters.Where(l => l.From >= w1awFrom).ToList();

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"W1AW's first letter at {w1awFrom:0.00} s"));
        output.WriteLine($"printed from then: {string.Concat(after.Select(l => Math.Abs(l.PitchHz - 600) > CwSenderGate.PitchToleranceHz ? $"[{l.Text}@{l.PitchHz:0}]" : l.Text))}");
        output.WriteLine($"printed before: {string.Concat(t.Letters.Where(l => l.From < 60).Select(l => l.Text))}");

        // W1AW takes the terminal once the 650 Hz station falls quiet, and keeps it while that station calls again beside it.
        Assert.True(double.IsFinite(w1awFrom));
        Assert.All(after, l => Assert.True(Math.Abs(l.PitchHz - 600) <= CwSenderGate.PitchToleranceHz, $"{l.Text} at {l.PitchHz:0} Hz, {l.From:0.00} s"));
        Assert.DoesNotContain(t.Handovers, h => h.At > w1awFrom && Math.Abs(h.ToHz - 650) <= 30);
    }
}
