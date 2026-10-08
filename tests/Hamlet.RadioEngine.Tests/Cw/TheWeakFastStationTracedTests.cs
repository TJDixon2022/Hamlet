using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE WEAK FAST STATION** (work instruction 556, task 5, HM-DEC-260): `cw-2026-10-03-143906`, about 26 WPM at 11 dB, where
/// a plain read gives `QSY QSY DE W` and the chain prints junk. Traced through the chain: whether it stands, whether its window
/// opens, and what the level reading finds. R88 is lifted for the owner's twelve recordings.
/// </summary>
public sealed class TheWeakFastStationTracedTests(ITestOutputHelper output)
{
    private const string Name = "cw-2026-10-03-143906";

    /// <remarks>
    /// The plain read at the station's pitch; every sender the gate held, second by second; the window's contrast while it
    /// was open; and every letter printed, with its pitch and its marks. Asserts nothing about the text.
    /// </remarks>
    [Fact]
    public void WhereTheChainLosesIt()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Name));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Name);
        var inv = CultureInfo.InvariantCulture;

        foreach (var tone in new[] { 500.0, 514.0, 525.0 })
        {
            var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, tone);

            output.WriteLine(string.Create(inv, $"plain at {tone:0} Hz: dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms, {plain.Marks.Count} marks; `{plain.Text}`"));
        }

        using var chain = new CwChain(audio.SampleRate);
        var trace = new List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)>();
        var keyUp = new List<(double Seconds, double? KeyUpDb)>();
        var letters = new List<(string Text, CwMark[] Marks)>();
        var gate = chain.Decoder.Runs;

        chain.Detector.SetPassband(pitch, width);
        chain.Detector.LaneTrace = trace;
        chain.Detector.LaneKeyUpTrace = keyUp;
        gate.RunRead += (c, run) => letters.Add((c.Text, run.ToArray()));

        var chunk = audio.SampleRate / 100;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

            if ((at + chunk) % audio.SampleRate == 0)
            {
                var shapes = gate.SenderShapes;

                output.WriteLine(string.Create(inv, $"{(at + chunk) / audio.SampleRate,2} s | {string.Join(" | ", shapes.Select(s => $"{s.PitchHz:0} Hz shape {s.Shape.Score:0.00} marks {s.Marks}{(s.Qualified ? " qualified" : string.Empty)}{(s.Printed ? " PRINTED" : string.Empty)}"))}"));
            }
        }

        chain.Decoder.Flush();

        if (trace.Count > 0)
        {
            var contrasts = trace.Zip(keyUp, (t, k) => k.KeyUpDb is { } up ? t.SenderDb - up : double.NaN).Where(double.IsFinite).Order().ToList();

            output.WriteLine(string.Create(inv, $"window open from {trace[0].Seconds:0.00} s, {trace.Count} hops; sender dit {trace[^1].DitSeconds * 1000:0} ms; contrast {(contrasts.Count > 0 ? $"median {contrasts[contrasts.Count / 2]:0.0} dB, {contrasts[0]:0.0} to {contrasts[^1]:0.0}" : "never measured")} (the level path needs {CwEnvelopeDetector.MinLevelContrastDb:0} dB)"));
        }
        else
        {
            output.WriteLine("the window never opened");
        }

        foreach (var (text, marks) in letters)
        {
            output.WriteLine(string.Create(inv, $"letter `{text}` at {marks[0].FromSeconds:0.00} s, {marks.Average(m => m.PitchHz):0} Hz, marks {string.Join(' ', marks.Select(m => $"{m.LengthMs:0}"))} ms"));
        }

        Assert.True(audio.Samples.Length > 0);
    }
}
