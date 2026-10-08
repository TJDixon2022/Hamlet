using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **KM3STU READS FROM HIS CQ** (work instruction 559, HM-DEC-263): a POTA activator on 7.0442 MHz, 2026-10-08 12:13 UTC, his
/// tone at 800 Hz, 200 Hz above the radio's 600 Hz pitch, about 13 dB over the noise, 20 to 22 WPM. Live, Hamlet printed
/// nothing for the whole first recording. R88 is lifted for these three recordings. Whether candidates get windows follows
/// <c>HAMLET_CANDIDATE_WINDOWS</c>, so the same trace reads with them off and on.
/// </summary>
public sealed class Km3stuReadsFromHisCqTests(ITestOutputHelper output)
{
    /// <summary>The three recordings, in order.</summary>
    internal static readonly string[] Recordings = ["cw-2026-10-08-121324", "cw-2026-10-08-121357", "cw-2026-10-08-121414"];

    /// <remarks>
    /// The first recording through the app's chain, cold, second by second: the plain read's marks at 800 Hz; every mark the
    /// grid offered within 30 Hz of it, with length, level and own contrast; the sequences there; the marks that stood; the
    /// gate's senders; and the windows open. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheFirstRecordingSecondBySecond()
    {
        var name = Recordings[0];
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);
        var inv = CultureInfo.InvariantCulture;
        var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, 800);

        output.WriteLine(string.Create(inv, $"radio pitch {pitch:0} Hz, filter {width:0} Hz; plain at 800 Hz: dit {plain.Dit * 1000:0} ms, dah {plain.Dah * 1000:0} ms, {plain.Marks.Count} marks: `{plain.Text}`"));

        using var chain = new CwChain(audio.SampleRate);
        var gate = chain.Decoder.Runs;
        var printed = new List<string>();

        chain.Detector.SetPassband(pitch, width);
        gate.CharacterRead += c => printed.Add(c.Text);

        var chunk = audio.SampleRate / 100;
        long seen = 0;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

            if ((at + chunk) % audio.SampleRate != 0)
            {
                continue;
            }

            var t = (at + chunk) / audio.SampleRate;
            var p = plain.Marks.Where(m => m.From >= t - 1 && m.From < t).ToList();
            var offered = chain.Detector.OfferedNow.Where(m => Math.Abs(m.PitchHz - 800) <= 30 && m.FromSeconds >= t - 1.5 && m.FromSeconds < t - 0.5).ToList();
            var batch = chain.Detector.MarksSince(seen);

            seen = batch.Marks.Count > 0 ? batch.Marks[^1].Sequence : seen;

            var stood = batch.Marks.Where(m => Math.Abs(m.PitchHz - 800) <= 30).ToList();
            var sequences = chain.Detector.SequencesNow(t).Split(" | ", StringSplitOptions.RemoveEmptyEntries)
                .Where(s => double.TryParse(s.Split(' ')[0], NumberStyles.Float, inv, out var hz) && Math.Abs(hz - 800) <= 60);

            output.WriteLine(string.Create(inv, $"{t,2} s | plain {string.Join(' ', p.Select(m => $"{(m.To - m.From) * 1000:0}{m.Kind}"))}"));
            output.WriteLine(string.Create(inv, $"     | grid offered (t-1.5..t-0.5) {string.Join(' ', offered.Select(m => $"{m.FromSeconds:0.00}:{m.LengthMs:0}ms {m.PitchHz:0}Hz {m.LevelDb:0.0}dB c{m.OwnContrastDb:0.0}"))}"));
            output.WriteLine(string.Create(inv, $"     | stood near 800: {stood.Count} | sequences: {string.Join(" | ", sequences)}"));
            output.WriteLine(string.Create(inv, $"     | senders {string.Join(" | ", gate.SenderShapes.Select(s => $"{s.PitchHz:0} Hz shape {s.Shape.Score:0.00} marks {s.Marks}{(s.Qualified ? " qualified" : string.Empty)}{(s.Printed ? " PRINTED" : string.Empty)}"))} || windows {chain.Detector.CandidateWindowsNow} lane {chain.Detector.OwnWindowPitchHz:0}"));
        }

        chain.Decoder.Flush();

        output.WriteLine($"printed `{string.Concat(printed)}`");
    }

    /// <remarks>Each recording's plain read at 800 Hz, letter by letter with the time each begins, for the stretches' spans. Asserts nothing.</remarks>
    [Fact]
    public void EachPlainReadWithTimes()
    {
        foreach (var name in Recordings)
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
            var plain = TheStrongStationsOverTests.ReadPlain(audio.Samples, audio.SampleRate, 800);
            var letters = plain.Marks.GroupBy(m => m.Letter).Select(g => (From: g.First().From, To: g.Last().To, Word: g.First().WordBefore, Code: string.Concat(g.Select(m => m.Kind))));

            output.WriteLine($"{name}: `{plain.Text}`");
            output.WriteLine("  " + string.Join(' ', letters.Select(l => string.Create(CultureInfo.InvariantCulture, $"{(l.Word ? "| " : string.Empty)}{MorseAlphabet.Lookup(l.Code) ?? "■"}@{l.From:0.0}-{l.To:0.0}"))));
        }
    }

    /// <remarks>Each recording read cold through the live path, as the scoreboard reads it. Asserts nothing.</remarks>
    [Fact]
    public void EachRecordingReads()
    {
        foreach (var name in Recordings)
        {
            output.WriteLine($"{name} reads `{TheRecordingsScoreboardTests.ReadLive(name).Text}`");
        }
    }
}
