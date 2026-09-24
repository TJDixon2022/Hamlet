using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every space the decoder inserts inside a word, in the ten keyed captures and
/// 17:37, traced to the gap that caused it and the thresholds in force when it was
/// read (work instruction 413, task 1; PHASE_PLAN.md 3.1).
/// </summary>
/// <remarks>
/// <para>**WHAT DECIDES A SPACE.** In <c>CwProbabilisticDecoder.DecodeAt</c> the
/// gap between characters and the gap between words carry the same evidence, the
/// key-up likelihood over the same span, so which one the path takes is settled by
/// the length penalty alone: <c>want</c> for kinds 3 and 4, either three and seven
/// units at the stream's speed or the three gaps <c>CwProbabilisticStream</c> holds
/// from <c>CwUnitEstimator.MeasureGaps</c>. The two penalties cost the same at the
/// geometric mean of the two wants, and that is the boundary printed here.</para>
/// <para>**HOW A GAP IS MEASURED FROM WHAT SETTLED.** A character's <c>At</c> is the
/// moment its key went up, and <c>SpanHops</c> reaches back to where its first mark
/// began, so the silence between two named characters is the next one's start
/// taken from the previous one's end. A word gap's own <c>At</c> is where it ended;
/// where it ends before the next named character starts, something the emission
/// bar left out sat in between, and the line says so.</para>
/// <para>**THE STREAM'S HELD GAPS ARE READ BY REFLECTION** at the moment each
/// character settles, because nothing public says whether the sender's own gaps or
/// the textbook ones were in force. It reads; it changes nothing.</para>
/// <para>A printer. It asserts nothing (3.1).</para>
/// </remarks>
public sealed class WhereTheWordsBreakTests
{
    private static readonly FieldInfo StructureHeld = typeof(CwProbabilisticStream)
        .GetField("_structureHeld", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly FieldInfo HeldGaps = typeof(CwProbabilisticStream)
        .GetField("_heldGaps", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereTheWordsBreakTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One settled character and what the stream held when it settled.</summary>
    /// <param name="Character">What settled.</param>
    /// <param name="Wpm">The speed the window was read at.</param>
    /// <param name="Held">True when the sender's own gaps were in force.</param>
    /// <param name="Gaps">The held gaps, meaningful only when held.</param>
    private sealed record Settled(
        CwCharacter Character, double Wpm, bool Held, CwUnitEstimator.CwGapLengths Gaps)
    {
        public double UnitMs => 1200.0 / Wpm;

        public double CharacterWantMs => Held ? Gaps.CharacterMilliseconds : 3 * UnitMs;

        public double WordWantMs => Held ? Gaps.WordMilliseconds : 7 * UnitMs;

        /// <summary>Where the word reading starts to cost less than the character reading.</summary>
        public double WordFromMs => Math.Sqrt(CharacterWantMs * WordWantMs);

        public string Thresholds
            => $"unit {UnitMs:0} ms ({Wpm:0.0} wpm), "
               + (Held
                   ? $"held gaps element {Gaps.ElementMilliseconds:0} character {Gaps.CharacterMilliseconds:0} word {Gaps.WordMilliseconds:0}"
                     + (Gaps.CharacterBoundaryClipped ? " character clipped" : "")
                     + (Gaps.WordBoundaryClipped ? " word clipped" : "")
                   : $"textbook character {CharacterWantMs:0} word {WordWantMs:0}")
               + $", word from {WordFromMs:0} ms";
    }

    /// <summary>A boundary between two named characters in a scored region.</summary>
    private sealed record Boundary(
        string Capture, Settled Before, Settled After, Settled? Space, int Spaces,
        bool KeySpace, double GapMs, double SpaceEndsEarlyMs, string Around, string KeyAround)
    {
        public bool DecodeSpace => Spaces > 0;

        public Settled Context => Space ?? After;

        public double Units => GapMs / Context.UnitMs;

        public double OverWordFrom => GapMs / Context.WordFromMs;

        public string Kind => (KeySpace, DecodeSpace) switch
        {
            (false, true) => "inserted",
            (true, false) => "missing",
            (true, true) => "word kept",
            _ => "joined",
        };
    }

    private static IReadOnlyList<Settled> Settle(string name)
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var stream = decoder.Stream;
        var settled = new List<Settled>();

        decoder.CharacterSettled += c => settled.Add(new Settled(
            c,
            stream.Last.WordsPerMinute,
            (bool)StructureHeld.GetValue(stream)!,
            (CwUnitEstimator.CwGapLengths)HeldGaps.GetValue(stream)!));

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return settled;
    }

    private static double EndMs(CwCharacter c) => c.At.TotalMilliseconds;

    private static double StartMs(CwCharacter c)
        => c.At.TotalMilliseconds - (c.SpanHops * CwProbabilisticDecoder.HopMilliseconds);

    /// <summary>Every boundary between two named characters the alignment reaches.</summary>
    private static IEnumerable<Boundary> Boundaries(
        string capture, IReadOnlyList<Settled> settled, CwScore score)
    {
        // Which settled character each position of the text came from.
        var owner = new List<int>();
        var text = new System.Text.StringBuilder();

        for (var i = 0; i < settled.Count; i++)
        {
            text.Append(settled[i].Character.Text);
            owner.AddRange(Enumerable.Repeat(i, settled[i].Character.Text.Length));
        }

        var decode = text.ToString();
        var keyText = new System.Text.StringBuilder();
        var d = score.Start;
        int? previous = null;
        var keySpace = false;
        var decodeSpaces = new List<int>();

        foreach (var step in score.Steps)
        {
            if (step.Key is { } k)
            {
                keyText.Append(k);

                if (k == ' ')
                {
                    keySpace = true;
                }
            }

            if (step.Decoded is null)
            {
                continue;
            }

            var index = owner[d];
            var c = settled[index].Character;

            d++;

            if (c.IsWordGap)
            {
                decodeSpaces.Add(index);
                continue;
            }

            if (c.IsUnreadable || index == previous)
            {
                continue;
            }

            if (previous is { } p)
            {
                var before = settled[p];
                var after = settled[index];
                var space = decodeSpaces.Count > 0 ? settled[decodeSpaces[^1]] : null;
                var at = d - 1;
                var from = Math.Max(0, at - 12);
                var to = Math.Min(decode.Length, at + 10);
                var keyNow = keyText.ToString();

                yield return new Boundary(
                    capture,
                    before,
                    after,
                    space,
                    decodeSpaces.Count,
                    keySpace,
                    StartMs(after.Character) - EndMs(before.Character),
                    space is null ? 0 : StartMs(after.Character) - EndMs(space.Character),
                    decode[from..to].Replace(' ', '_'),
                    keyNow[Math.Max(0, keyNow.Length - 12)..].Replace(' ', '_'));
            }

            previous = index;
            keySpace = false;
            decodeSpaces.Clear();
        }
    }

    private void Line(string label, Boundary b)
        => _output.WriteLine(
            $"{label} | {b.Capture} | {b.Context.Character.At.TotalSeconds,8:0.000} s | "
            + $"gap {b.GapMs,5:0} ms, {b.Units,4:0.0} units, {b.OverWordFrom:0.00} of word-from | "
            + $"{b.Context.Thresholds} | before `{b.Before.Character.Text}` {b.Before.Character.Pattern} ends "
            + $"{(b.Before.Character.Pattern.EndsWith('-') ? "dah" : "dit")} | "
            + (b.Spaces > 1 ? $"{b.Spaces} spaces, something left out between | " : "")
            + (b.SpaceEndsEarlyMs > 1 ? $"space ends {b.SpaceEndsEarlyMs:0} ms before the next mark | " : "")
            + $"decode `{b.Around}` key so far `{b.KeyAround}`");

    private static string Spread(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return "none";
        }

        double At(double share) => sorted[(int)Math.Round(share * (sorted.Length - 1))];

        return $"n {sorted.Length}, min {sorted[0]:0.00}, quartile {At(0.25):0.00}, median {At(0.5):0.00}, "
               + $"quartile {At(0.75):0.00}, max {sorted[^1]:0.00}";
    }

    /// <remarks>
    /// The second half of 3.1's trace: for every read of the stream over the ten
    /// keyed captures and 17:37, what <c>CwUnitEstimator.MeasureGaps</c> would find
    /// in that window and, where it finds nothing, which of its tests refused -
    /// too few gaps, heaps too close, or no trough - and how long the run of
    /// consecutive separated reads got against the twelve the stream asks for.
    /// Asserts nothing.
    /// </remarks>
    [Fact]
    public void WhyTheSendersGapsAreNotHeld()
    {
        var envelopeField = typeof(CwProbabilisticStream)
            .GetField("_envelope", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var countField = typeof(CwProbabilisticStream)
            .GetField("_envelopeCount", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var threeMeans = typeof(CwUnitEstimator)
            .GetMethod("ThreeMeansOnLogs", BindingFlags.NonPublic | BindingFlags.Static)!;
        var isTrough = typeof(CwUnitEstimator)
            .GetMethod("IsTrough", BindingFlags.NonPublic | BindingFlags.Static)!;

        var names = TheBenchmarkIsKeyedTests.Run
            .Where(r => TheBenchmarkIsKeyedTests.Stretches(r.Name).Count > 0)
            .Select(r => r.Name)
            .Append(TheSeventeenThirtySevenCaptureTests.Name);

        _output.WriteLine("reads | capture | reads | held | separated | too few gaps | heaps too close | no trough | out of order | longest run | median centroids when refused, ms");

        foreach (var name in names)
        {
            var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var decoder = new CwDecoder(audio.SampleRate, 600);
            var stream = decoder.Stream;
            int reads = 0, held = 0, separated = 0, few = 0, close = 0, trough = 0, wordOnly = 0, disorder = 0, run = 0, longest = 0;
            var refused = new List<double[]>();

            stream.LeadingEdgeChanged += _ =>
            {
                reads++;

                if ((bool)StructureHeld.GetValue(stream)!)
                {
                    held++;
                }

                var count = (int)countField.GetValue(stream)!;
                var window = ((double[])envelopeField.GetValue(stream)!)[..count];
                var unit = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);

                if (!unit.IsReady)
                {
                    return;
                }

                var reading = CwUnitEstimator.MeasureGaps(window, CwProbabilisticDecoder.HopMilliseconds, unit.UnitMilliseconds);
                var gaps = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds).Gaps;

                if (reading.Separated)
                {
                    separated++;
                    run++;
                    longest = Math.Max(longest, run);

                    if (reading.CharacterMilliseconds >= reading.WordMilliseconds)
                    {
                        disorder++;
                    }

                    return;
                }

                run = 0;

                if (gaps.Count < 12)
                {
                    few++;
                    return;
                }

                var c = (double[])threeMeans.Invoke(null, new object[] { gaps })!;

                refused.Add(c.Select(v => v / unit.UnitMilliseconds).ToArray());

                if (c[1] / c[0] < 1.5 || c[2] / c[1] < 1.5)
                {
                    close++;
                }
                else
                {
                    var low = (bool)isTrough.Invoke(null, new object[] { gaps, c[0], c[1] })!;
                    var high = (bool)isTrough.Invoke(null, new object[] { gaps, c[1], c[2] })!;

                    if (!low || !high)
                    {
                        trough++;
                    }

                    if (low && !high)
                    {
                        wordOnly++;
                    }
                }
            };

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            string Median(int i)
                => refused.Count == 0 ? "-" : $"{refused.Select(r => r[i]).OrderBy(v => v).ElementAt(refused.Count / 2):0.0}";

            _output.WriteLine(
                $"reads | {name} | {reads} | {held} | {separated} | {few} | {close} | {trough}, {wordOnly} at the word boundary only | {disorder} | {longest} | "
                + $"in units {Median(0)} / {Median(1)} / {Median(2)}");
        }
    }

    /// <remarks>
    /// Proves 3.1's trace: every space the decoder inserted where the inferred key
    /// has none, over the ten keyed captures and 17:37, printed with the gap that
    /// caused it, the unit and the two gap wants in force, and the element before;
    /// the ten furthest inside the character side of the boundary printed in full;
    /// and beside them the same measures for every boundary the decoder got right, so
    /// what separates the two can be read rather than asserted. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EveryInsertedSpaceIsTraced()
    {
        var boundaries = new List<Boundary>();

        foreach (var (name, _) in TheBenchmarkIsKeyedTests.Run)
        {
            var stretches = TheBenchmarkIsKeyedTests.Stretches(name);

            if (stretches.Count == 0)
            {
                continue;
            }

            var settled = Settle(name);
            var reading = CwReading.Of(settled.Select(s => s.Character));

            foreach (var stretch in stretches)
            {
                var score = CwScorer.Within(reading, stretch.Key, CwKeyKind.Inferred);

                boundaries.AddRange(Boundaries(name, settled, score));
            }
        }

        var tenCount = boundaries.Count;

        {
            var name = TheSeventeenThirtySevenCaptureTests.Name;
            var settled = Settle(name);
            var reading = CwReading.Of(settled.Select(s => s.Character));
            var region = CwScorer.FromFirst(reading, "CQ");
            var score = CwScorer.Whole(region, TheSeventeenThirtySevenCaptureTests.InferredKey, CwKeyKind.Inferred);
            var start = reading.Text.IndexOf("CQ", StringComparison.Ordinal);

            boundaries.AddRange(Boundaries(name, settled, score with { Start = start }));
        }

        _output.WriteLine("trace | capture | settled at | gap | thresholds in force | element before | notes | text around");

        foreach (var b in boundaries.Where(b => b.Kind == "inserted"))
        {
            Line("inserted", b);
        }

        foreach (var b in boundaries.Where(b => b.Kind == "missing"))
        {
            Line("missing", b);
        }

        _output.WriteLine("the ten worst in the ten keyed captures, furthest inside the character side of the boundary in force:");

        foreach (var b in boundaries.Take(tenCount).Where(b => b.Kind == "inserted").OrderBy(b => b.OverWordFrom).Take(10))
        {
            Line("worst", b);
        }

        foreach (var (label, set) in new[]
                 {
                     ("the ten", boundaries.Take(tenCount).ToList()),
                     ("17:37", boundaries.Skip(tenCount).ToList()),
                     ("all", boundaries),
                 })
        {
            _output.WriteLine(
                $"summary {label} | boundaries {set.Count} | inserted {set.Count(b => b.Kind == "inserted")} | "
                + $"missing {set.Count(b => b.Kind == "missing")} | word kept {set.Count(b => b.Kind == "word kept")} | "
                + $"joined {set.Count(b => b.Kind == "joined")}");

            foreach (var kind in new[] { "inserted", "joined", "word kept", "missing" })
            {
                var of = set.Where(b => b.Kind == kind).ToList();

                _output.WriteLine(
                    $"spread {label} | {kind} | held {of.Count(b => b.Context.Held)} of {of.Count} | "
                    + $"double spaces {of.Count(b => b.Spaces > 1)} | space ends early {of.Count(b => b.SpaceEndsEarlyMs > 1)} | "
                    + $"after a dah {of.Count(b => b.Before.Character.Pattern.EndsWith('-'))}");
                _output.WriteLine($"spread {label} | {kind} | gap in units | {Spread(of.Select(b => b.Units))}");
                _output.WriteLine($"spread {label} | {kind} | gap over word-from | {Spread(of.Select(b => b.OverWordFrom))}");
                _output.WriteLine($"spread {label} | {kind} | word-from in units | {Spread(of.Select(b => b.Context.WordFromMs / b.Context.UnitMs))}");
            }
        }
    }
}
