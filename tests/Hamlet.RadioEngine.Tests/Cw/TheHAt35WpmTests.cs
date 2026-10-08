using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **`H` AT 35 WPM** (work instruction 555, task 3, HM-DEC-259). The qualifying run live lost a dit of `H` at 30 and 35 WPM:
/// `T IE` for THE, `S ELD` for HELD. That recording is not in the tree; W1AW's 35 WPM text in piece 2 of the session of
/// 2026-10-07 is, where a dit is 34 ms with a 34 ms gap. Every H there is traced through the station's window, and a synthetic
/// line at 35 WPM is read through the filter with the bench's AGC and with a 6 dB fade. R88 is lifted for the four pieces.
/// </summary>
public sealed class TheHAt35WpmTests(ITestOutputHelper output)
{
    /// <remarks>
    /// Piece 2 read cold through the app's chain, as the scoreboard reads it. For each H the offline read places in the 35 WPM
    /// text (from 246 s), and for the THE the offline read itself took as `TSE`: the offline marks, the window's level each
    /// hop as decibels under the station's key-down level (`#` within 6 dB, the half amplitude a mark is trimmed to; `+`
    /// within 10; `.` under), and every letter the gate made over it with its marks' lengths.
    /// </remarks>
    [Fact]
    public void EveryHInPieceTwoTraced()
    {
        var piece = TheW1awSessionReplayTests.Pieces[1];
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(piece);
        var offline = TheW1awTableTests.OfflineMarks(audio.Samples, audio.SampleRate, pitch);
        var offlineLetters = TheW1awTableTests.OfflineLetters(offline);

        using var chain = new CwChain(audio.SampleRate);
        var runs = new List<(string Text, CwMark[] Marks)>();

        chain.Detector.SetPassband(pitch, width);
        chain.Detector.LaneTrace = [];
        chain.Decoder.Runs.RunRead += (c, run) => runs.Add((c.Text, run.ToArray()));

        var chunk = audio.SampleRate / 100;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
        }

        chain.Decoder.Flush();

        // The H of each THE, THEN and TSE from 246 s: the second letter of the word, up to the third.
        var words = new List<List<(double From, string Text)>>();

        foreach (var l in offlineLetters)
        {
            if (l.SpaceBefore || words.Count == 0)
            {
                words.Add([]);
            }

            words[^1].Add((l.From, l.Text));
        }

        var targets = words
            .Where(w => w.Count >= 3 && w[0].From >= 246 && string.Concat(w.Select(l => l.Text)) is "THE" or "THEN" or "TSE")
            .Select(w => (Word: string.Concat(w.Select(l => l.Text)), From: w[1].From - 0.01, To: w[2].From - 0.01))
            .ToList();
        var inv = CultureInfo.InvariantCulture;
        var whole = 0;

        foreach (var (word, from, to) in targets)
        {
            var marks = offline.Where(m => m.From >= from && m.From < to).ToList();
            var trace = chain.Detector.LaneTrace.Where(t => t.Seconds >= from - t.DelaySeconds && t.Seconds < to + 0.01 + t.DelaySeconds).ToList();
            var envelope = new StringBuilder();

            foreach (var t in trace)
            {
                var under = t.SenderDb - t.LaneDb;

                envelope.Append(under <= 6 ? '#' : under <= 10 ? '+' : '.');
            }

            var made = runs.Where(r => r.Marks[^1].ToSeconds > from && r.Marks[0].FromSeconds < to).ToList();
            var read = string.Concat(made.Select(r => r.Text));

            whole += read == "H" ? 1 : 0;

            output.WriteLine(string.Create(inv, $"H of {word} at {from + 0.01:0.000} s | offline marks {string.Join(' ', marks.Select(m => $"{m.Length * 1000:0}"))} ms, gaps {string.Join(' ', marks.Zip(marks.Skip(1), (a, b) => $"{(b.From - a.To) * 1000:0}"))} ms"));
            output.WriteLine(string.Create(inv, $"  window, 5 ms hops: {envelope} (key-down {trace.FirstOrDefault().SenderDb:0.0} dB)"));
            output.WriteLine(string.Create(inv, $"  the gate made: {string.Join(" | ", made.Select(r => $"`{r.Text}` from {string.Join(' ', r.Marks.Select(m => $"{(m.ToSeconds - m.FromSeconds) * 1000:0}"))} ms"))}"));
        }

        output.WriteLine($"H read whole: {whole} of {targets.Count}");

        Assert.NotEmpty(targets);
    }

    /// <remarks>
    /// The synthetic line at 35 WPM: through the filter with the bench's AGC, and the same with a 6 dB fade across it, the
    /// whole audio's level falling evenly from start to end after the filter.
    /// </remarks>
    /// <param name="faded">Whether the fade is applied.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheSyntheticLine(bool faded)
    {
        const string Sent = "THE HELD HABITS OF HIS SMITH";
        var samples = AHandIsReadAgainstItselfTests.Under("agc-filter", Sent, _ => new AHandIsReadAgainstItselfTests.Sending(35, 0), 5550);

        if (faded)
        {
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= (float)Math.Pow(10, -6.0 / 20 * i / samples.Length);
            }
        }

        var read = TheRecordingsScoreboardTests.ReadLive(samples, 8000, 625, 500).Text;

        output.WriteLine($"`{Sent}`, 35 WPM, {(faded ? "a 6 dB fade" : "steady")}: read `{read}`");

        Assert.Equal(Sent, read);
    }
}
