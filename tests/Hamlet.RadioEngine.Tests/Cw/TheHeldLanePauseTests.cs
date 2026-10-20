using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE PAUSE ON W1AW'S PIECE 2, TRACED** (work instruction 576): with a held lane reading, from the last real mark before
/// `W1AW &lt;AS&gt;` to the first after, every mark the held lane called, its start, length and level against the lane's lines and
/// its contrast, beside the printed sender's dit, letter gap and word gap and the longest gap he had left in the piece before.
/// Asserts nothing.
/// </summary>
public sealed class TheHeldLanePauseTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <remarks>The pause after `W1AW &lt;AS&gt;`, as the rule left off and with the end of the hold.</remarks>
    [Fact]
    public void ThePauseAfterW1awAs()
    {
        foreach (var end in new[] { false, true })
        {
            output.WriteLine(end ? "## with the end of the hold" : "## a held lane reading, no end");
            Trace(end);
        }
    }

    private void Trace(bool ending)
    {
        const string recording = "w1aw-2026-10-07/piece-02";
        const double start = 224;
        const double end = 246;

        using var ends = ending ? null : CwRules.Off(CwRules.HeldLanePause);

        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        using var chain = new CwChain(audio.SampleRate);
        var gate = chain.Decoder.Runs;
        var handover = new List<string>();
        var heldTrace = new List<(double At, double PitchHz, bool Mark, double From, double To, double LevelDb, double UpDb, double DownDb, double ContrastDb)>();
        var printed = new List<(double At, double PitchHz, string Text)>();
        var handed = new List<CwMark>();
        var station = new List<(double At, double Dit, double? Letter, double? Word, double Line, double Pitch)>();
        long sequence = 0;

        chain.Detector.SetPassband(pitch, width);
        chain.Detector.HandoverTrace = handover;

        if (chain.Detector.Bank is { } bank)
        {
            bank.HeldTrace = heldTrace;
        }

        gate.RunRead += (c, run) => printed.Add((run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text));

        var chunk = audio.SampleRate / 100;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

            var batch = chain.Detector.MarksSince(sequence);

            if (batch.Marks.Count > 0)
            {
                handed.AddRange(batch.Marks);
                sequence = batch.Marks.Max(m => m.Sequence);
            }

            var now = (at + chunk) / (double)audio.SampleRate;
            var (letter, word) = gate.StationGaps;

            station.Add((now, gate.StationDitSeconds, letter, word, gate.StationWordLineSeconds, gate.StationPitchHz));
        }

        chain.Decoder.Flush();

        var near = handed.Where(m => Math.Abs(m.PitchHz - pitch) <= 50).OrderBy(m => m.FromSeconds).ToList();
        var before = near.Where(m => m.ToSeconds < start).ToList();
        var longest = before.Zip(before.Skip(1), (a, b) => b.FromSeconds - a.ToSeconds).DefaultIfEmpty(double.NaN).Max();

        output.WriteLine($"handover: {string.Join("; ", handover.Where(h => double.Parse(h.Split(' ')[0], Inv) is >= start - 30 and <= end))}");
        output.WriteLine($"printed {start} to {end} s: {string.Join(" ", printed.Where(p => p.At is >= start and <= end).Select(p => string.Create(Inv, $"{p.Text}@{p.At:0.00}/{p.PitchHz:0}")))}");
        output.WriteLine(string.Create(Inv, $"the longest gap between his marks within 50 Hz of {pitch:0} Hz before {start} s: {longest * 1000:0} ms"));
        output.WriteLine("marks handed on within 50 Hz, " + start + " to " + end + " s:");
        output.WriteLine(string.Join(" ", near.Where(m => m.FromSeconds is >= start and <= end).Select(m => string.Create(Inv, $"{m.FromSeconds:0.000}+{m.LengthMs:0}/{m.PitchHz:0}"))));
        output.WriteLine("| lane (Hz) | called at | start (s) | length (ms) | level (dB) | key-down (dB) | key-up (dB) | level under key-down / contrast | contrast (dB) | his dit (ms) | his letter gap (ms) | his word gap (ms) | his word line (ms) | printed pitch |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var m in heldTrace.Where(t => t.Mark && t.From is >= start and <= end))
        {
            var s = station.LastOrDefault(x => x.At <= m.At);

            output.WriteLine(string.Create(Inv, $"| {m.PitchHz:0} | {m.At:0.000} | {m.From:0.000} | {(m.To - m.From) * 1000:0} | {m.LevelDb:0.0} | {m.DownDb:0.0} | {m.UpDb:0.0} | {(m.DownDb - m.LevelDb) / m.ContrastDb:0.00} | {m.ContrastDb:0.0} | {s.Dit * 1000:0} | {s.Letter * 1000:0} | {s.Word * 1000:0} | {s.Line * 1000:0} | {s.Pitch:0} |"));
        }

        var hops = heldTrace.Where(t => t.At is >= start and <= end).ToList();

        if (hops.Count > 0)
        {
            output.WriteLine(string.Create(Inv, $"held hops {start} to {end} s: {hops.Count}, first at {hops[0].At:0.000}, last at {hops[^1].At:0.000}; contrast every half second: {string.Join(" ", hops.Where((_, i) => i % 100 == 0).Select(h => $"{h.At:0.0}:{h.ContrastDb:0.0}"))}"));
        }
    }
}
