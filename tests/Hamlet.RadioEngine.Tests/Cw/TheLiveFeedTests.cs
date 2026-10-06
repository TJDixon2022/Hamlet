using System.Diagnostics;
using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **WHAT GOES WRONG LIVE THAT A REPLAY DOES NOT SHOW** (work instruction 548, task 1): W1AW's bulletin of 2026-10-06 fed
/// through the app's chain the way the live path feeds it - in the capture's chunk size, with chunks lost - and the cost of
/// the chain per second of audio with more senders held.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-06**: *"We were perfect on W1AW before."* Live it printed `TYP IEI EEIS A I E NEEA NI E EI AND
/// TYW IV` where W1AW sent `TYPE II AND TYPE IV`; replayed through the same chain it reads clean.</para>
/// <para>**THE LIVE FEED.** `WasapiAudioSource` opens the device with a 100 ms buffer and NAudio's shared-mode capture reads
/// it every half buffer, so the chain is handed chunks of about 50 ms, 2,400 samples at 48 kHz, on WASAPI's own thread, and
/// the decoder and the detector both run inside that callback. A chunk that arrives late is the same chunk: nothing in the
/// chain reads a wall clock, only a count of samples. A chunk the device overwrote while the callback ran long is gone, and
/// the chain is never told.</para>
/// <para>R88 is lifted for the W1AW capture; the stations added for the cost are synthetic.</para>
/// </remarks>
public sealed class TheLiveFeedTests(ITestOutputHelper output)
{
    private const string W1aw = "cw-2026-10-06-212015";

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>How to feed: the chunk length in ms, and which chunks are lost.</summary>
    internal sealed record Feed(string Name, int ChunkMs, Func<int, int, bool> Lost);

    /// <summary>The ways of feeding the work instruction names.</summary>
    internal static readonly Feed[] Feeds =
    [
        new("10 ms chunks, nothing lost (the scoreboard's)", 10, (_, _) => false),
        new("50 ms chunks, nothing lost (the live capture's)", 50, (_, _) => false),
        new("100 ms chunks, nothing lost", 100, (_, _) => false),
        new("10 ms chunks, one in a hundred lost", 10, (i, _) => i % 100 == 57),
        new("50 ms chunks, one a second lost", 50, (i, _) => i % 20 == 13),
        new("50 ms chunks, a 200 ms stall every 5 s", 50, (i, _) => i % 100 is >= 60 and < 64),
        new("50 ms chunks, a 200 ms stall every 2 s", 50, (i, _) => i % 40 is >= 20 and < 24),
    ];

    /// <summary>Feed audio through the chain at a passband; the text of each pass and how many samples were lost.</summary>
    internal static (List<string> Passes, long LostSamples) Read(float[] x, int rate, double pitchHz, double widthHz, Feed feed, int passes)
    {
        using var chain = new CwChain(rate);
        var texts = new List<string>();
        var text = new StringBuilder();
        var chunk = rate * feed.ChunkMs / 1000;
        var lost = 0L;
        var index = 0;

        chain.Detector.SetPassband(pitchHz, widthHz);
        chain.Decoder.CharacterSettled += c => text.Append(c.Text);

        for (var p = 0; p < passes; p++)
        {
            for (var at = 0; at + chunk <= x.Length; at += chunk)
            {
                if (feed.Lost(index++, p))
                {
                    lost += chunk;
                    continue;
                }

                chain.Process(new AudioChunk(at + ((long)p * x.Length), rate, x.AsSpan(at, chunk)));
            }

            if (p == passes - 1)
            {
                chain.Decoder.Flush();
            }

            texts.Add(string.Join(' ', text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)));
            text.Clear();
        }

        return (texts, lost);
    }

    /// <remarks>
    /// Task 1: W1AW's recording fed twice end to end in each way, so the second pass has the first as its history, as the
    /// live session had minutes of the station before it. Prints what each pass prints; asserts only that the clean feed
    /// reads.
    /// </remarks>
    [Fact]
    public void W1awFedAsLive()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(W1aw));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(W1aw);
        string? clean = null;

        foreach (var feed in Feeds)
        {
            var (passes, lost) = Read(audio.Samples, audio.SampleRate, pitch, width, feed, 2);

            clean ??= passes[1];
            output.WriteLine($"{feed.Name}: {lost * 1000 / audio.SampleRate} ms lost over two passes");
            output.WriteLine($"  first  `{passes[0]}`");
            output.WriteLine($"  second `{passes[1]}` | {Score(passes[1])}");
        }

        Assert.Contains("TYPE IV RADIO EMISSIONS", clean, StringComparison.Ordinal);
    }

    /// <summary>W1AW's reference on the scoreboard.</summary>
    internal const string Reference = "PE II AND TYPE IV RADIO EMISSIONS HOWEVER, THIS CME IS";

    /// <summary>Letters right and wrong against W1AW's reference.</summary>
    internal static string Score(string printed)
        => $"{TheRecordingsScoreboardTests.Right(printed, Reference)} right, {TheRecordingsScoreboardTests.WrongAt(printed, Reference).Count} wrong of {TheRecordingsScoreboardTests.Letters(Reference).Length}";

    /// <remarks>
    /// Task 1: the cost of the chain per second of audio, on this machine, at 48 kHz in 50 ms chunks: W1AW alone, and with two
    /// and five more synthetic stations keying at other pitches in the passband, so the gate holds one, three and six senders.
    /// Prints the time and the margin against real time and against the 100 ms buffer; asserts only that it was measured.
    /// </remarks>
    [Fact]
    public void WhatTheChainCostsPerSecond()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(W1aw));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(W1aw);
        var rate = audio.SampleRate;
        string[] calls = ["CQ CQ DE K1ABC K1ABC K", "QRL QRL DE N2XYZ", "TEST DE W3QQQ K", "CQ DX DE VE3AAA", "73 TU DE K4ZZZ SK"];
        double[] pitches = [420, 520, 700, 760, 820];

        foreach (var others in new[] { 0, 2, 5 })
        {
            var x = (float[])audio.Samples.Clone();

            for (var s = 0; s < others; s++)
            {
                var station = CwSignal.Generate(new CwSignalRequest(
                    string.Join(' ', Enumerable.Repeat(calls[s], 6)), WordsPerMinute: 14 + (3 * s), ToneHz: pitches[s], SampleRate: rate,
                    Amplitude: 0.2, LeadInSeconds: 0.5 + s, TailSeconds: 0, Seed: 5480 + s)).Samples;

                for (var k = 0; k < x.Length && k < station.Length; k++)
                {
                    x[k] += station[k];
                }
            }

            using var chain = new CwChain(rate);
            var chunk = rate / 20;
            var worst = 0.0;
            var total = Stopwatch.StartNew();

            chain.Detector.SetPassband(pitch, width);

            for (var at = 0; at + chunk <= x.Length; at += chunk)
            {
                var one = Stopwatch.GetTimestamp();

                chain.Process(new AudioChunk(at, rate, x.AsSpan(at, chunk)));
                worst = Math.Max(worst, Stopwatch.GetElapsedTime(one).TotalMilliseconds);
            }

            total.Stop();

            var seconds = x.Length / (double)rate;
            var held = chain.Decoder.Runs.SenderShapes.Count;

            output.WriteLine(string.Create(Invariant,
                $"W1AW and {others} more: senders held at the end {held}; {total.Elapsed.TotalMilliseconds / seconds:0.0} ms per second of audio ({100 * total.Elapsed.TotalSeconds / seconds:0.0}% of real time); worst 50 ms chunk {worst:0.0} ms, against the 100 ms the device buffer holds"));
        }

        Assert.True(audio.Samples.Length > 0);
    }
}
