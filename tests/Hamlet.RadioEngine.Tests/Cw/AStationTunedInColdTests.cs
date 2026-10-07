using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A STATION TUNED IN COLD** (work instruction 552, task 2): W1AW's recording read from its first second, the way the operator
/// meets every station he tunes onto. R88 is lifted for W1AW's `cw-2026-10-06-212015`.
/// </summary>
/// <remarks>
/// **WHAT IT SHOWS, AND WHY THE TASK DID NOT SHIP**: the opening prints `E NE II AEED` for `PE II AND`, and every letter of it
/// is labelled at its pick exactly as the sender's lines at the end would label it (split 117 ms then, 119 at the end; letter
/// line 113 then, 111). The detector never called P's first dah, at 0.42 to 0.62 s, while it had no gap level yet, and called
/// N's dah as 60 ms of its 205. Every mark it called was printed. Re-reading the backlog at the pick cannot bring back a mark
/// that was never called.
/// </remarks>
public sealed class AStationTunedInColdTests(ITestOutputHelper output)
{
    /// <remarks>
    /// Every letter W1AW's sender prints in its first ten seconds, with the sender's split between dit and dah and its letter
    /// line at the moment it printed and at the end of the recording, and how each mark would be labelled by those at the end.
    /// Asserts nothing; the trace is the result.
    /// </remarks>
    [Fact]
    public void TheOpeningAsPrintedAndAsKnownLater()
    {
        var name = AStationsShadowIsNotAStationTests.W1aw;
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);
        var rate = audio.SampleRate;

        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var printed = new List<(string Text, CwMark[] Run, double Split, double Letter, double Heard)>();
        var heard = 0.0;

        chain.Detector.SetPassband(pitch, width);
        gate.RunRead += (c, run) => printed.Add((c.Text, run.ToArray(), gate.StationSplitSeconds, gate.StationLines?.CharacterSeconds ?? double.NaN, heard));

        for (var at = 0; at + (rate / 100) <= audio.Samples.Length; at += rate / 100)
        {
            chain.Process(new AudioChunk(at, rate, audio.Samples.AsSpan(at, rate / 100)));
            heard = (at + (rate / 100)) / (double)rate;
        }

        chain.Decoder.Flush();

        var finalSplit = gate.StationSplitSeconds;
        var finalLetter = gate.StationLines?.CharacterSeconds ?? double.NaN;

        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"at the end: split {finalSplit * 1000:0} ms, letter line {finalLetter * 1000:0} ms"));

        foreach (var (text, run, split, letter, at) in printed.Where(p => p.Run[0].FromSeconds < 10))
        {
            var marks = string.Join(" ", run.Select(m => string.Create(CultureInfo.InvariantCulture, $"{(m.ToSeconds - m.FromSeconds) * 1000:0}")));
            var gaps = string.Join(" ", run.Skip(1).Select((m, i) => string.Create(CultureInfo.InvariantCulture, $"{(m.FromSeconds - run[i].ToSeconds) * 1000:0}")));
            var later = string.Concat(run.Select(m => m.ToSeconds - m.FromSeconds >= finalSplit ? "-" : "."));

            output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"{run[0].FromSeconds:0.00}-{run[^1].ToSeconds:0.00} s printed at {at:0.00} s `{text}`: marks {marks} ms, gaps {gaps} ms; split then {split * 1000:0}, letter line then {letter * 1000:0}; labels at the end {later}"));
        }

        // Every mark the detector called in the first 4.5 s, the printed sender's or not: whether the opening's missing dahs
        // were never called, or were called and went elsewhere.
        foreach (var m in chain.Detector.MarksSince(0).Marks.Where(m => m.FromSeconds < 4.5).OrderBy(m => m.FromSeconds))
        {
            var mine = printed.Any(p => p.Run.Any(r => r.Sequence == m.Sequence));

            output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"mark {m.FromSeconds:0.000}-{m.ToSeconds:0.000} s, {m.LengthMs:0} ms at {m.PitchHz:0} Hz, {m.LevelDb:0.0} dB, contrast {m.ContrastDb:0.0}{(m.Keyed ? ", keyed" : string.Empty)}{(mine ? ", printed" : ", not printed")}"));
        }

        Assert.NotEmpty(printed);
    }
}
