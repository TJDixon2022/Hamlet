using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The capture of 2026-09-23 17:37, the first in the tree with a key, read the
/// way the floors read every capture (work instruction 408).
/// </summary>
/// <remarks>
/// <para>**THE KEY IS INFERRED, NOT TRANSCRIBED** (CLAUDE.md 0.0, FACT-004).
/// Nobody wrote down what was sent; `cw-2026-09-23-173723.key.md` infers it from
/// the decode and the fixed form of a CQ call. Every number here is stated
/// against an inferred key, and the first third of the recording, which nobody
/// read, is never scored.</para>
/// </remarks>
public sealed class TheSeventeenThirtySevenCaptureTests
{
    /// <summary>The recording, under the captures folder.</summary>
    public const string Name = "unadjudicated/cw-2026-09-23-173723";

    /// <summary>The inferred key, for the stretch from the first CQ onward.</summary>
    public const string InferredKey = "CQ CQ CQ DE WB6RED WB6RED";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public TheSeventeenThirtySevenCaptureTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Everything that settled, fed a hop at a time as the floors feed.</summary>
    /// <param name="name">The recording.</param>
    /// <returns>The settled characters, word gaps included, in order.</returns>
    internal static IReadOnlyList<CwCharacter> Settle(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return settled;
    }

    /// <summary>
    /// The scored region: from the first `C` of the first `CQ CQ` to the end.
    /// </summary>
    /// <param name="text">What settled.</param>
    /// <returns>The region, or "" where no `CQ CQ` was read.</returns>
    internal static string ScoredRegion(string text)
    {
        var from = text.IndexOf("CQ CQ", StringComparison.Ordinal);

        return from < 0 ? "" : text[from..].Trim();
    }

    /// <summary>Levenshtein distance, characters inserted, deleted or changed.</summary>
    internal static int Distance(string a, string b)
    {
        var row = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            row[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            var diagonal = row[0];
            row[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var above = row[j];
                row[j] = Math.Min(
                    Math.Min(row[j] + 1, row[j - 1] + 1),
                    diagonal + (a[i - 1] == b[j - 1] ? 0 : 1));
                diagonal = above;
            }
        }

        return row[b.Length];
    }

    /// <remarks>
    /// A printer: every settled character with its own evidence, so a bar can be
    /// chosen from the spans rather than from taste. It asserts nothing.
    /// </remarks>
    [Fact]
    public void EveryCharacterIsPrintedWithItsEvidence()
    {
        var settled = Settle(Name);
        var text = string.Concat(settled.Select(c => c.Text));
        var named = settled.Count(c => !c.IsWordGap && !c.IsUnreadable);
        var placeholders = settled.Count(c => c.IsUnreadable);
        var region = ScoredRegion(text);

        _output.WriteLine($"settled: {text}");
        _output.WriteLine($"named {named}, placeholders {placeholders}");
        _output.WriteLine(
            $"scored region, against an inferred key: \"{region}\" is "
            + $"{Distance(region, InferredKey)} edits from \"{InferredKey}\"");

        foreach (var c in settled.Where(c => !c.IsWordGap))
        {
            _output.WriteLine(
                $"  {c.At.TotalSeconds,7:0.000} s  {c.Text,-2} {c.Pattern,-8} "
                + $"hops {c.SpanHops,3}  span llr {c.SpanLogLikelihoodRatio,12:0.0}  "
                + $"per hop {c.SpanMarginForRecord,9:0.000}");
        }
    }
}
