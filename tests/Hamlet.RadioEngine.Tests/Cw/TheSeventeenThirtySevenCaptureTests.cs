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

    /// <summary>What the scored region read at unit 408's entry, in edits from the key.</summary>
    /// <remarks>
    /// Measured at `c19ecf61` by <see cref="EveryCharacterIsPrintedWithItsEvidence"/>:
    /// `CQ CQ CQ DE W T E E T E  E ERE D E T T TB 7E E I`, 29 edits from
    /// <see cref="InferredKey"/>, **against an inferred key**.
    /// </remarks>
    public const int EditsAtEntry = 29;

    /// <summary>The 17:37 capture and the three adjudicated anchors' recordings.</summary>
    public static TheoryData<string> BarCases { get; } = new()
    {
        Name,
        "cw-2026-08-17-013347",
        "cw-2026-08-17-134712",
        "unadjudicated/cw-2026-08-18-003758",
    };

    /// <remarks>
    /// <para>Proves 3.7's first clause (R58): **no character is printed whose own
    /// span is below the gate's bar**, <see cref="CwProbabilisticDecoder.CharacterMargin"/>
    /// in log-likelihood per hop, neither on the leading edge nor settled. On the
    /// 17:37 capture it also holds the scored region to no more edits from the
    /// inferred key than it had at entry.</para>
    /// <para>The three anchors' recordings are here because the bench's replay of
    /// the 17:37 WAV settles nothing below the bar at all, so on that case alone
    /// the fact could never have been watched red.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(BarCases))]
    public void NothingBelowTheBarIsPrinted(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var settled = new List<CwCharacter>();
        var below = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;
        decoder.CharacterDecoded += c =>
        {
            if (!c.IsWordGap && c.SpanMarginForRecord < CwProbabilisticDecoder.CharacterMargin)
            {
                below.Add(c);
            }
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        below.AddRange(settled.Where(c =>
            !c.IsWordGap && c.SpanMarginForRecord < CwProbabilisticDecoder.CharacterMargin));

        var text = string.Concat(settled.Select(c => c.Text));

        _output.WriteLine($"{name}: {text}");
        _output.WriteLine(
            $"  {below.Count} printed below the bar of "
            + $"{CwProbabilisticDecoder.CharacterMargin} per hop");

        Assert.Empty(below);

        if (name == Name)
        {
            var region = ScoredRegion(text);
            var edits = Distance(region, InferredKey);

            _output.WriteLine(
                $"  scored region \"{region}\", {edits} edits against an inferred key, "
                + $"{EditsAtEntry} at entry");

            Assert.True(
                edits <= EditsAtEntry,
                $"the scored region reads {edits} edits from an inferred key, "
                + $"worse than {EditsAtEntry} at entry");
        }
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
