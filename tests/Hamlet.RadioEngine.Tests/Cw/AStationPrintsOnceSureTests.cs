using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A STATION PRINTS ONCE HAMLET IS SURE OF IT** (work instruction 561, HM-DEC-265): on the air on 2026-10-08 a weak portable
/// station printed every mark it sent while too weak to read, `I RM T REIE E EIT A E AEI EE ETIEI E IE I HEE EIEEAENOH/P N K`,
/// because a station began printing at the shape it is let go at, 0.1. These measure the shape each printed station first
/// printed at, its peak in its first ten seconds, and the weak cases.
/// </summary>
public sealed class AStationPrintsOnceSureTests(ITestOutputHelper output)
{
    private const int Rate = 8000;

    /// <summary>One time a station was given the terminal.</summary>
    internal sealed record Episode(double At, double PitchHz, double FirstShape)
    {
        public double Peak10 { get; set; }

        public double SureAt { get; set; } = double.NaN;

        public double EndAt { get; set; } = double.NaN;

        public List<(double At, string Text)> Letters { get; } = [];
    }

    /// <summary>Reads audio through the app's chain and returns each episode of a printed station, chunk by chunk.</summary>
    internal static (List<Episode> Episodes, string Text) Trace(float[] samples, int rate, double pitchHz, double widthHz)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var episodes = new List<Episode>();
        var characters = new List<string>();
        var chunk = rate / 100;
        var now = 0.0;
        Episode? current = null;

        chain.Detector.SetPassband(pitchHz, widthHz);
        gate.CharacterRead += c => characters.Add(c.Text);
        gate.RunRead += (c, run) => (current ?? episodes.LastOrDefault())?.Letters.Add((now, c.Text));

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            now = (at + chunk) / (double)rate;
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            var printed = gate.SenderShapes.Where(s => s.Printed).Select(s => ((double PitchHz, double Score)?)(s.PitchHz, s.Shape.Score)).FirstOrDefault();

            if (printed is not { } p)
            {
                if (current is not null)
                {
                    current.EndAt = now;
                    current = null;
                }

                continue;
            }

            if (current is null || Math.Abs(current.PitchHz - p.PitchHz) > 30)
            {
                if (current is not null)
                {
                    current.EndAt = now;
                }

                current = new Episode(now, p.PitchHz, p.Score);
                episodes.Add(current);
            }

            if (now - current.At <= 10)
            {
                current.Peak10 = Math.Max(current.Peak10, p.Score);
            }

            if (double.IsNaN(current.SureAt) && p.Score >= 0.4)
            {
                current.SureAt = now;
            }
        }

        chain.Decoder.Flush();

        return (episodes, string.Join(' ', string.Concat(characters).Split(' ', StringSplitOptions.RemoveEmptyEntries)));
    }

    internal static IEnumerable<(string Name, float[] Samples, int Rate, double PitchHz, double WidthHz)> Everything()
    {
        foreach (var name in TheRecordingsScoreboardTests.Stretches.Select(s => s.Recording).Distinct())
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);

            yield return (name, audio.Samples, audio.SampleRate, pitch, width);
        }

        foreach (var name in TheW1awSessionReplayTests.Pieces)
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);

            yield return (name, audio.Samples, audio.SampleRate, pitch, width);
        }

        foreach (var name in new[] { "catch-153810-7033367", "catch-154614-7047190", "catch-154819-7050903" })
        {
            var audio = WavAudio.Read(TheStrongStationsOverTests.Wav(name));

            yield return (name, audio.Samples, audio.SampleRate, 600, 500);
        }
    }

    /// <remarks>
    /// The measurement: for every recording on both tables and the three scan catches, each time a station was given the
    /// terminal, the shape it first printed at, its peak in its first ten seconds, when it first reached 0.4, and the letters it
    /// printed before then. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheShapeAtFirstPrint()
    {
        var inv = CultureInfo.InvariantCulture;

        foreach (var (name, samples, rate, pitch, width) in Everything())
        {
            var (episodes, _) = Trace(samples, rate, pitch, width);

            foreach (var e in episodes)
            {
                var before = e.Letters.Where(l => double.IsNaN(e.SureAt) || l.At < e.SureAt).ToList();

                output.WriteLine(string.Create(inv, $"| `{name}` | {e.At:0.00} s | {e.PitchHz:0} Hz | {e.FirstShape:0.00} | {e.Peak10:0.00} | {(double.IsNaN(e.SureAt) ? "never" : $"{e.SureAt:0.00} s")} | {e.Letters.Count} | {before.Count} | `{string.Concat(before.Select(l => l.Text))}` |"));
            }
        }
    }

    /// <remarks>
    /// The three weak cases, the junk printed before a station's call: `143906`, `121324`, and a synthetic weak caller keyed
    /// at 10 dB that strengthens to 16 dB over its call. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheThreeWeakCases()
    {
        foreach (var name in new[] { "cw-2026-10-03-143906", "cw-2026-10-08-121324" })
        {
            output.WriteLine($"{name} reads `{TheRecordingsScoreboardTests.ReadLive(name).Text}`");
        }

        output.WriteLine($"the weak caller, 10 dB strengthening to 16 dB: `{AWeakCallerStrengthening()}`");
    }

    /// <remarks>
    /// Whether the rule ever fires: weak callers from 4 to 10 dB that strengthen by 6 dB, and the 143906 and 121324 recordings,
    /// read with the rule off and on. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheRuleOffAndOn()
    {
        foreach (var db in new[] { 4, 6, 8, 10 })
        {
            string off;

            using (CwRules.Off(CwRules.SureFirst))
            {
                off = AWeakCallerStrengthening(db);
            }

            output.WriteLine($"{db} to {db + 6} dB | off `{off}` | on `{AWeakCallerStrengthening(db)}`");
        }

        foreach (var name in new[] { "cw-2026-10-03-143906", "cw-2026-10-08-121324", "cw-2026-10-03-221502", "cw-2026-10-03-221745" })
        {
            string off;

            using (CwRules.Off(CwRules.SureFirst))
            {
                off = TheRecordingsScoreboardTests.ReadLive(name).Text;
            }

            output.WriteLine($"{name} | off `{off}` | on `{TheRecordingsScoreboardTests.ReadLive(name).Text}`");
        }
    }

    /// <remarks>
    /// Task 2: the three low-confidence stretches read offline again at three envelope cutoffs, 30, 40 and 60 Hz, beside their
    /// references. A stretch is promoted only where every read agrees with its reference. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheLowStretchesReadAgain()
    {
        var low = TheRecordingsScoreboardTests.Stretches.Where(s => s.Confidence == TheRecordingsScoreboardTests.Confidence.Low);

        foreach (var s in low)
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(s.Recording));

            output.WriteLine($"{s.Recording} at {s.PitchHz:0} Hz, {s.From}-{s.To} s, reference `{s.Reference}`");

            foreach (var cutoff in new[] { 30.0, 40, 60 })
            {
                var (_, letters) = TheRecordingsScoreboardTests.ReadOffline(s, audio.Samples, audio.SampleRate, cutoff);
                var same = TheRecordingsScoreboardTests.Letters(letters) == TheRecordingsScoreboardTests.Letters(s.Reference);

                output.WriteLine($"  {cutoff:0} Hz: `{letters.Trim()}`{(same ? " - agrees" : string.Empty)}");
            }
        }
    }

    /// <remarks>
    /// Work instruction 562, task 2: `221851`'s letters read offline with the time each begins, to find where its reference
    /// ends at `K`, and the stretch read again cut there. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheSignOffCutAtItsK()
    {
        var inv = CultureInfo.InvariantCulture;
        var s = TheRecordingsScoreboardTests.Stretches.Single(x => x.Recording == "cw-2026-10-03-221851");
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(s.Recording));
        var marks = TheW1awTableTests.OfflineMarks(audio.Samples, audio.SampleRate, s.PitchHz);
        var letters = TheW1awTableTests.OfflineLetters(marks);

        output.WriteLine(string.Join(' ', letters.Select(l => string.Create(inv, $"{(l.SpaceBefore ? "| " : string.Empty)}{l.Text}@{l.From:0.00}"))));
        output.WriteLine(string.Join(' ', marks.Select(m => string.Create(inv, $"{m.From:0.00}-{m.To:0.00}"))));

        foreach (var to in new[] { 30.0, 16.5 })
        {
            var (_, read) = TheRecordingsScoreboardTests.ReadOffline(s with { To = to }, audio.Samples, audio.SampleRate);
            var same = TheRecordingsScoreboardTests.Letters(read) == TheRecordingsScoreboardTests.Letters(s.Reference);

            output.WriteLine(string.Create(inv, $"to {to:0.0} s: `{read.Trim()}`{(same ? " - agrees" : string.Empty)}"));
        }
    }

    /// <summary>
    /// A synthetic weak caller: `CQ CQ DE N1XYZ N1XYZ K` sent twice at 20 WPM and 700 Hz, the first time at 10 dB and the second
    /// at 16 dB, through the filter, read at a 600 Hz pitch and a 500 Hz filter.
    /// </summary>
    internal static string AWeakCallerStrengthening(int db = 10)
    {
        const string Call = "CQ CQ DE N1XYZ N1XYZ K";
        var weak = AHandIsReadAgainstItselfTests.Keyed(Call, _ => new AHandIsReadAgainstItselfTests.Sending(20, 0), 5611, db, 1, 700);
        var strong = AHandIsReadAgainstItselfTests.Keyed(Call, _ => new AHandIsReadAgainstItselfTests.Sending(20, 0), 5612, db + 6, 1, 700);
        var joined = new float[weak.Length + strong.Length];

        weak.CopyTo(joined, 0);
        strong.CopyTo(joined, weak.Length);

        return TheRecordingsScoreboardTests.ReadLive(NarrownessReadsTheFiltersBandTests.ThroughTheFilter(joined), Rate, 600, 500).Text;
    }
}
