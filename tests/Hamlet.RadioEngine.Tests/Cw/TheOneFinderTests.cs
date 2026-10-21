using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **ONE FINDER: THE BANK FINDS EVERY STATION** (work instruction 584, HM-DEC-288, <see cref="CwRules.BankFinds"/>): what it costs,
/// and the traces each round of the rebuild named. Asserts nothing.
/// </summary>
public sealed class TheOneFinderTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <remarks>
    /// Work instruction 584, section 2, step 6: W1AW's piece-04 read through the chain with the bank as the finder and without it, the
    /// process's CPU time and the wall clock for each, the first read thrown away to warm the process.
    /// </remarks>
    [Fact]
    public void TheOneFinderCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        (double Cpu, double Wall, string Text) Time(bool bankFinds)
        {
            using var on = bankFinds ? CwRules.Choose(CwRules.BankFinds) : null;
            using var off = bankFinds ? null : CwRules.Off(CwRules.BankFinds);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var wall = System.Diagnostics.Stopwatch.StartNew();
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);

            process.Refresh();

            return ((process.TotalProcessorTime - cpu).TotalSeconds, wall.Elapsed.TotalSeconds, text);
        }

        Time(false);

        var two = Time(false);
        var one = Time(true);

        output.WriteLine(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s at {audio.SampleRate} Hz, filter {width:0} Hz"));
        output.WriteLine(string.Create(Inv, $"two finders: {two.Cpu:0.00} s CPU, {two.Wall:0.00} s wall"));
        output.WriteLine(string.Create(Inv, $"one finder:  {one.Cpu:0.00} s CPU, {one.Wall:0.00} s wall; {(one.Cpu - two.Cpu) / two.Cpu:+0%;-0%} of the chain with two"));
        output.WriteLine("two: " + two.Text);
        output.WriteLine("one: " + one.Text);
    }

    /// <remarks>
    /// Work instruction 584, section 3: the recordings the round's largest loss named, read with one finder and with two. Every letter
    /// with the second it printed, its pitch and contrast; every hold and its end; and with one finder, each second, the bank's lanes
    /// standing within 50 Hz of each held pitch with their shapes.
    /// </remarks>
    [Theory]
    [InlineData("w1aw-2026-10-07/piece-02")]
    [InlineData("cw-2026-10-02-200157")]
    [InlineData("cw-2026-10-03-221530")]
    [InlineData("cw-2026-10-03-221502")]
    public void TheLargestLossTraced(string recording)
    {
        foreach (var one in new[] { false, true })
        {
            using var on = one ? CwRules.Choose(CwRules.BankFinds) : null;
            using var off = one ? null : CwRules.Off(CwRules.BankFinds);
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
            using var chain = new CwChain(audio.SampleRate);
            var letters = new List<string>();
            var trace = new List<string>();
            var lanes = new List<string>();
            var chunk = audio.SampleRate / 100;
            var now = 0.0;
            var lastSecond = -1;

            chain.Detector.SetPassband(pitch, width);
            chain.Detector.HandoverTrace = trace;
            chain.Decoder.Runs.RunRead += (c, run) => letters.Add(string.Create(Inv, $"{run[^1].ToSeconds:0.00} {c.Text}@{run.Average(m => m.PitchHz):0}/{run.Average(m => m.ContrastDb):0}"));

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
                now = (at + chunk) / (double)audio.SampleRate;

                if (one && (int)(now * 4) != lastSecond && chain.Detector.Bank is { } bank)
                {
                    lastSecond = (int)(now * 4);

                    var standing = bank.Standing(now);
                    var held = chain.Detector.HeldStations;

                    lanes.Add(string.Create(Inv, $"{lastSecond / 4.0,5:0.00} s held [{string.Join(", ", held.Select(h => $"{h.PitchHz:0}/{CwLaneBank.DitSeconds[h.Dit] * 1000:0}"))}] standing [{string.Join(", ", standing.Where(s => s.Sequence.Shape.Score >= 0.1).OrderBy(s => s.Sequence.PitchHz).Select(s => $"{s.Sequence.PitchHz:0}/{CwLaneBank.DitSeconds[s.Dit] * 1000:0} {s.Sequence.Shape.Score:0.00}"))}]"));
                }
            }

            chain.Decoder.Flush();

            output.WriteLine(string.Create(Inv, $"{recording}, {pitch:0} Hz, filter {width:0} Hz, {(one ? "one finder" : "two finders")}:"));
            output.WriteLine("  text: " + string.Concat(letters.Select(l => l.Split(' ')[1].Split('@')[0])));
            output.WriteLine("  letters: " + string.Join("  ", letters));
            output.WriteLine("  holds: " + string.Join("; ", trace));
            output.WriteLine("  lane shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Where(h => h.Finder == "lane" && h.AtSeconds < 40).Select(h => string.Create(Inv, $"{h.AtSeconds:0.00} s {h.PitchHz:0} of {h.OfPitchHz:0} share {h.Share:0.00}"))));

            foreach (var line in lanes)
            {
                output.WriteLine("  " + line);
            }
        }
    }
}
