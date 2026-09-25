using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The requirement metrics over every keyed recording in the tree, per condition,
/// with the key's kind beside every number (work instruction 439, tasks 3 and 4;
/// PHASE_PLAN.md 1.1 to 1.3; HM-REQ-011, 010, 012, 081, 082).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE NUMBERS.** HM-REQ-011 requires
/// MET-INVENTED at zero and the tree is not there; a test that asserted it would
/// be a known red, and a floor on it would be a ratchet the requirement forbids
/// (V-08, V-10). These are instruments. It asserts only that every keyed recording
/// was measured or said why it could not be.</para>
/// <para>**THE SCORED STRETCHES ARE THE BASELINE'S**, from
/// <see cref="WhatTheStrayLettersRestOnTests.KeyedRecordings"/>: the same
/// recordings, the same keys, the same region of each decode, so a number here and
/// an edit total there are counted over the same characters. Inside each stretch
/// the metrics align symbol by symbol with spaces aside (<see cref="CwMetrics"/>).</para>
/// <para>**A CONDITION IS WHAT `CW_SPEC.md` NAMES ONE** - a channel profile, a
/// sender profile and an SNR in the 2500 Hz reference (section 4) - **AND NO REAL
/// RECORDING HERE HAS ALL THREE.** None was received on a named `CH-*` channel and
/// none carries an SNR in the reference bandwidth, so each real row says so and is
/// grouped by the one part the specification does state for it: its sender, where
/// `CW_SPEC.md` section 10 names the capture, and "not stated" otherwise. The
/// synthetic CQ set has a known sender and a known in-passband level and no fading;
/// its noise is a shaped band, and whether that stands for `CH-AWGN` is not
/// established, so it is labeled as what it is. **EVERY REAL KEY IS INFERRED** and
/// disagreement with it is not by itself proof the decoder is wrong (V-13); **EVERY
/// SYNTHETIC KEY IS EXACT** and no synthetic case is ever sole evidence (12.5).</para>
/// </remarks>
public sealed class TheRequirementsAreMeasuredTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public TheRequirementsAreMeasuredTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A recording measured: its stretches aligned, or why none could be.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Set">Real or synthetic, and which total it belongs to.</param>
    /// <param name="Condition">The condition as `CW_SPEC.md` can state it for this recording.</param>
    /// <param name="Kind">The key's kind.</param>
    /// <param name="Stretches">One alignment per scored stretch.</param>
    /// <param name="Settled">What the decoder settled.</param>
    /// <param name="Scores">The baseline's text scores, for the old count beside the new.</param>
    /// <param name="NotComputable">Why no number can be given, or null.</param>
    internal sealed record Measured(
        string Name, string Set, string Condition, CwKeyKind Kind,
        IReadOnlyList<CwMetricAlignment> Stretches, IReadOnlyList<CwCharacter> Settled,
        IReadOnlyList<CwScore> Scores, string? NotComputable);

    private const string RealChannel = "real HF, no CH-* profile, SNR_2500 not measured";

    /// <summary>The sender each real capture is named as in `CW_SPEC.md` section 10, and only those.</summary>
    private static readonly IReadOnlyDictionary<string, string> NamedSenders = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["cw-2026-08-17-013347"] = "TX-TIGHT (CW_SPEC.md 10, HM-DEC-101)",
        ["unadjudicated/cw-2026-08-24-012403"] = "TX-ITU (CW_SPEC.md 10, the KD0UN capture)",
        ["cw-2026-08-18-004507"] = "TX-FARNS (CW_SPEC.md 6.4 and 10, HM-DEC-115's traffic net)",
    };

    private static readonly Lazy<IReadOnlyList<Measured>> RealMeasured = new(MeasureReal);

    private static readonly Lazy<IReadOnlyList<Measured>> SyntheticMeasured = new(MeasureSynthetic);

    /// <summary>Every real keyed recording, measured once per process.</summary>
    internal static IReadOnlyList<Measured> Real => RealMeasured.Value;

    /// <summary>The synthetic CQ set, measured once per process.</summary>
    internal static IReadOnlyList<Measured> Synthetic => SyntheticMeasured.Value;

    /// <summary>The condition a real recording can be stated under.</summary>
    /// <param name="name">The recording.</param>
    /// <returns>Its label.</returns>
    internal static string RealCondition(string name)
        => $"{RealChannel}, sender "
           + (NamedSenders.TryGetValue(name, out var sender) ? sender : "not stated in CW_SPEC.md");

    /// <summary>The settled characters a stretch of a text score covers.</summary>
    /// <param name="settled">What settled.</param>
    /// <param name="score">A score with its start and region in the text of <see cref="CwReading.Of"/>.</param>
    /// <returns>The characters, or an empty list where the region is empty.</returns>
    internal static IReadOnlyList<CwCharacter> Covered(IReadOnlyList<CwCharacter> settled, CwScore score)
    {
        if (score.Region.Length == 0)
        {
            return Array.Empty<CwCharacter>();
        }

        var owner = new List<int>();

        for (var i = 0; i < settled.Count; i++)
        {
            owner.AddRange(Enumerable.Repeat(i, settled[i].Text.Length));
        }

        var first = owner[score.Start];
        var last = owner[score.Start + score.Region.Length - 1];

        return settled.Skip(first).Take(last - first + 1).ToList();
    }

    /// <summary>Measures one decode against its scored stretches.</summary>
    /// <param name="name">The recording.</param>
    /// <param name="set">Which set it belongs to.</param>
    /// <param name="condition">Its condition.</param>
    /// <param name="kind">The key's kind.</param>
    /// <param name="settled">What settled.</param>
    /// <param name="scores">The baseline's scores, each with its key.</param>
    /// <returns>The recording measured, or a statement of why it cannot be.</returns>
    internal static Measured Measure(
        string name, string set, string condition, CwKeyKind kind,
        IReadOnlyList<CwCharacter> settled, IReadOnlyList<CwScore> scores)
    {
        if (scores.Count == 0)
        {
            return new Measured(name, set, condition, kind, Array.Empty<CwMetricAlignment>(), settled, scores,
                "no scored stretch: the key's opening was not read, so there is no region to score");
        }

        var stretches = scores
            .Select(s => CwMetrics.Align(CwMetrics.Symbols(Covered(settled, s)), s.Key, kind))
            .ToList();

        return new Measured(name, set, condition, kind, stretches, settled, scores, null);
    }

    private static IReadOnlyList<Measured> MeasureReal()
        => WhatTheStrayLettersRestOnTests.KeyedRecordings
            .Select(k =>
            {
                var settled = TheSeventeenThirtySevenCaptureTests.Settle(k.Name);

                return Measure(k.Name, k.Set, RealCondition(k.Name), CwKeyKind.Inferred,
                    settled, k.Score(CwReading.Of(settled)));
            })
            .ToList();

    private static IReadOnlyList<Measured> MeasureSynthetic()
        => SyntheticCq.All
            .Select(recipe =>
            {
                var settled = Settle(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"));
                var score = SyntheticCq.Whole(CwReading.Of(settled));
                var from = CwReading.Of(settled).Text.TakeWhile(char.IsWhiteSpace).Count();

                return Measure(recipe.Name, "synthetic", SyntheticCondition(recipe), CwKeyKind.Exact,
                    settled, new[] { score with { Start = from } });
            })
            .ToList();

    /// <summary>The condition a synthetic case is generated under.</summary>
    /// <param name="recipe">The case.</param>
    /// <returns>Its label, without its speed, which is reported beside it.</returns>
    internal static string SyntheticCondition(CwFixtureRecipe recipe)
    {
        var units = recipe.CharacterGapMilliseconds / recipe.DitMilliseconds;
        var sender = Math.Abs(units - 3) < 0.01
            ? "TX-ITU (1:3:1:3:7)"
            : string.Create(CultureInfo.InvariantCulture, $"character gap {units:0} units, inside TX-FARNS's 3 to 7");

        return string.Create(CultureInfo.InvariantCulture,
            $"synthetic, no fading, shaped noise band (not shown to be CH-AWGN), {sender}, "
            + $"{recipe.SignalToNoiseDb:0} dB in the passband (not restated in the 2500 Hz reference)");
    }

    private static IReadOnlyList<CwCharacter> Settle(string path)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, SyntheticCq.StartingPitchHz);
        var settled = new List<CwCharacter>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return settled;
    }

    private static string Share(int count, int of) => of == 0
        ? "no number: nothing to divide by"
        : (count / (double)of).ToString("0.0000", CultureInfo.InvariantCulture);

    /// <summary>What the stray trace calls added: named characters wholly added by the text alignment.</summary>
    private static int OldAdded(Measured m, bool singleOnly)
    {
        var labels = WhatTheStrayLettersRestOnTests.Labels(m.Settled, m.Scores);

        return m.Settled.Where((c, i) =>
                WhatTheStrayLettersRestOnTests.IsNamed(c)
                && labels[i] == WhatTheStrayLettersRestOnTests.Label.Added
                && (!singleOnly || c.Pattern.Length == 1))
            .Count();
    }

    /// <remarks>
    /// Proves 1.1: MET-INVENTED as `CW_SPEC.md` section 11 defines it, (sure
    /// insertions + sure substitutions) / characters sent, over every keyed
    /// recording, printed per recording and per condition with the key's kind,
    /// and beside it the stray trace's old count of added named characters, so
    /// where the two differ can be read recording by recording. HM-REQ-011
    /// requires zero. Asserts only that every recording was measured or said why
    /// not.
    /// </remarks>
    [Fact]
    public void MetInventedOverEveryKeyedRecording()
    {
        _output.WriteLine("MET-INVENTED = (sure insertions + sure substitutions) / characters sent (CW_SPEC.md 11); HM-REQ-011 requires 0");
        _output.WriteLine("row | recording | set | condition | key | invented | sure added | sure wrong | sent | share | stretches | old added named | old added single-element");

        foreach (var m in Real.Concat(Synthetic))
        {
            if (m.NotComputable is { } why)
            {
                _output.WriteLine($"row | {m.Name} | {m.Set} | {m.Condition} | {CwMetrics.KindWord(m.Kind)} | no number: {why}");
                continue;
            }

            var parts = m.Stretches.Select(CwMetrics.Invented).ToList();
            int added = parts.Sum(p => p.SureAdded), wrong = parts.Sum(p => p.SureWrong), sent = parts.Sum(p => p.Sent);

            _output.WriteLine(
                $"row | {m.Name} | {m.Set} | {m.Condition} | {CwMetrics.KindWord(m.Kind)} | {added + wrong} | {added} | {wrong} | "
                + $"{sent} | {Share(added + wrong, sent)} | {m.Stretches.Count} | {OldAdded(m, false)} | {OldAdded(m, true)}");
        }

        Table("real", Real, m => m.Condition);
        Table("synthetic", Synthetic, m => m.Condition);

        var computed = Real.Where(m => m.NotComputable is null).ToList();
        var all = computed.SelectMany(m => m.Stretches).Select(CwMetrics.Invented).ToList();

        _output.WriteLine(
            $"total | real keyed recordings | {all.Sum(p => p.Count)} invented ({all.Sum(p => p.SureAdded)} sure added, "
            + $"{all.Sum(p => p.SureWrong)} sure wrong) over {all.Sum(p => p.Sent)} sent | inferred keys | "
            + $"{computed.Count} of {Real.Count} recordings measured | share {Share(all.Sum(p => p.Count), all.Sum(p => p.Sent))}");
        _output.WriteLine(
            $"total | old count | {Real.Sum(m => OldAdded(m, false))} added named characters, "
            + $"{Real.Sum(m => OldAdded(m, true))} single-element, over the same recordings and stretches");

        var synth = Synthetic.Where(m => m.NotComputable is null).SelectMany(m => m.Stretches).Select(CwMetrics.Invented).ToList();

        _output.WriteLine(
            $"total | synthetic CQ set | {synth.Sum(p => p.Count)} invented ({synth.Sum(p => p.SureAdded)} sure added, "
            + $"{synth.Sum(p => p.SureWrong)} sure wrong) over {synth.Sum(p => p.Sent)} sent | exact keys | never sole evidence");

        Assert.Equal(WhatTheStrayLettersRestOnTests.KeyedRecordings.Count, Real.Count);
        Assert.Equal(SyntheticCq.All.Count, Synthetic.Count);
        Assert.All(Real.Concat(Synthetic), m => Assert.True(m.NotComputable is not null || m.Stretches.Count > 0));
    }

    private void Table(string set, IReadOnlyList<Measured> measured, Func<Measured, string> condition)
    {
        _output.WriteLine($"condition | {set} | condition | key | invented | sure added | sure wrong | sent | share | recordings measured | recordings with no number");

        foreach (var group in measured.GroupBy(condition).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var parts = group.Where(m => m.NotComputable is null).SelectMany(m => m.Stretches).Select(CwMetrics.Invented).ToList();
            int added = parts.Sum(p => p.SureAdded), wrong = parts.Sum(p => p.SureWrong), sent = parts.Sum(p => p.Sent);
            var kinds = string.Join(" and ", group.Select(m => CwMetrics.KindWord(m.Kind)).Distinct());

            _output.WriteLine(
                $"condition | {set} | {group.Key} | {kinds} | {added + wrong} | {added} | {wrong} | {sent} | {Share(added + wrong, sent)} | "
                + $"{group.Count(m => m.NotComputable is null)} | {group.Count(m => m.NotComputable is not null)}");
        }
    }
}
