using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Cw.Second;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Where the two decoders' readings meet on one clock, over every keyed
/// recording and every synthetic case (work instruction 465, task 1; HM-REQ-120,
/// 125, 126, 127; PHASE_PLAN.md 9.6).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT EITHER DECODER.** Nothing under
/// `src` changes for it.</para>
/// <para>**BOTH DECODERS COME FROM ONE HARNESS** (V-11):
/// <see cref="BothDecodersAreScoredAlikeTests.Rows"/>, and the port's p from
/// <see cref="FldigiConfidence.For(Hamlet.RadioEngine.Cw.Second.FldigiCwDecoder)"/>
/// on <see cref="WhatEachDecoderKnowsAboutEachCharacterFact.RunPort"/>'s second
/// run, checked there to print exactly what the harness's run printed.</para>
/// <para>**FIXED HERE, BEFORE ANY ARBITRATED TEXT WAS SEEN, AND NEVER CHANGED
/// AFTER** (V-14):</para>
/// <list type="bullet">
/// <item>**The span rule** (465 DECIDED (2)): two characters, one of each decoder,
/// are on the same span where their spans overlap by at least half the shorter
/// span on the recording's clock; pairs one to one, the largest overlap first.
/// Word boundaries are not characters and are not paired.</item>
/// <item>**The vote table** (465 DECIDED (3)): `calibration.md` section 2's
/// held-out verdicts (unit 464, `efdd5d11`), a decoder voting on a condition only
/// where it is calibrated (HM-REQ-124): ours on real HF, all; real HF, sender not
/// stated; synthetic, all; synthetic TX-ITU 15 dB. The port on none. In the
/// harness a recording runs under its own condition row; live, under real HF,
/// all.</item>
/// <item>**The margin** (465 DECIDED (5)): 0.05 on |p ours - p port|,
/// HM-REQ-127's recommended value, held until the owner rules. A tie is a
/// disagreement's case: an agreement has no "winning character".</item>
/// </list>
/// </remarks>
public sealed class WhereTheTwoReadingsMeetFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public WhereTheTwoReadingsMeetFact(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Work instruction 465 task 0, item 6: every character each decoder put out
    /// on all 35, word boundaries included, with its class and p, in order. Every
    /// byte-identical check of the unit is made against this print.
    /// </remarks>
    [Fact]
    public void EachDecodersCharactersWithClassAndP()
    {
        foreach (var r in BothDecodersAreScoredAlikeTests.Rows)
        {
            for (var i = 0; i < r.Ours.Settled.Count; i++)
            {
                var c = r.Ours.Settled[i];

                _output.WriteLine(string.Create(Invariant,
                    $"save | ours | {r.Name} | {i} | {Visible(c.Text)} | {ClassOf(c)} | {P(c.Probability)}"));
            }

            if (r.Port is null)
            {
                _output.WriteLine($"save | port | {r.Name} | not run: {r.PortNotRun}");
                continue;
            }

            var p = FldigiConfidence.For(WhatEachDecoderKnowsAboutEachCharacterFact.RunPort(r));

            for (var i = 0; i < r.Port.Settled.Count; i++)
            {
                var c = r.Port.Settled[i];

                _output.WriteLine(string.Create(Invariant,
                    $"save | port | {r.Name} | {i} | {Visible(c.Text)} | {ClassOf(c)} | {P(p[i])}"));
            }
        }
    }

    /// <summary>The fraction of the shorter span two characters must share to be on the same span (465 DECIDED (2)).</summary>
    internal const double SameSpanShare = 0.5;

    /// <summary>HM-REQ-127's margin, its recommended value, held until the owner rules (465 DECIDED (5)).</summary>
    internal const double TieMargin = 0.05;

    /// <summary>How far the port's filter output lags its input, in 8000 Hz samples (<see cref="FldigiCwDecisionRow.FilteredSample"/>).</summary>
    internal const int PortFilterLag = 512;

    /// <summary>One character of one decoder on the recording's clock.</summary>
    /// <param name="Decoder">ours or port.</param>
    /// <param name="Index">Its index in what the decoder put out.</param>
    /// <param name="Character">The character as the scorer takes it.</param>
    /// <param name="Start">Its first mark's start, seconds from the recording's first sample; NaN where it could not be read.</param>
    /// <param name="End">Its last mark's end, likewise.</param>
    /// <param name="P">Its p.</param>
    internal sealed record Reading(string Decoder, int Index, CwCharacter Character, double Start, double End, double P)
    {
        /// <summary>True where both ends are known.</summary>
        public bool HasSpan => double.IsFinite(Start) && double.IsFinite(End) && End >= Start;
    }

    /// <summary>One span: ours' reading, the port's, or both, and the case it falls in.</summary>
    internal sealed record Meeting(Reading? Ours, Reading? Port, string Case)
    {
        /// <summary>Where it starts, for ordering.</summary>
        public double At => Ours?.Start ?? Port!.Start;
    }

    /// <summary>
    /// Our characters on the clock: <see cref="CwCharacter.At"/> is the end of the
    /// character's last hop (<c>CwProbabilisticStream</c>, the window's start hop
    /// plus the character's <c>EndHop</c>, times <see cref="CwProbabilisticDecoder.HopMilliseconds"/>),
    /// and <see cref="CwCharacter.SpanHops"/> hops before it is where it starts.
    /// </summary>
    internal static IReadOnlyList<Reading> OursOnTheClock(IReadOnlyList<CwCharacter> settled)
    {
        var list = new List<Reading>();

        for (var i = 0; i < settled.Count; i++)
        {
            var c = settled[i];

            if (c.IsWordGap)
            {
                continue;
            }

            var end = c.At.TotalSeconds;
            var start = c.SpanHops > 0 ? end - (c.SpanHops * CwProbabilisticDecoder.HopMilliseconds / 1000.0) : double.NaN;

            list.Add(new Reading("ours", i, c, start, end, c.Probability));
        }

        return list;
    }

    /// <summary>
    /// The port's characters on the clock, from its key events placed on the decision
    /// rows each was raised in: the character's last up event ends it, and its first
    /// up event less that element's length starts it, both less
    /// <see cref="PortFilterLag"/>, at 8000 Hz. The up events are those
    /// <see cref="FldigiConfidence"/> reads its margins from.
    /// </summary>
    internal static IReadOnlyList<Reading> PortOnTheClock(FldigiCwDecoder port, IReadOnlyList<CwCharacter> settled, IReadOnlyList<double> p)
    {
        var list = new List<Reading>();
        var ups = new List<(FldigiCwKeyEvent Event, long Row)>();
        var k = 0;
        var e = 0;

        foreach (var row in port.Decisions)
        {
            foreach (var token in row.Events.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (token is not ("down" or "up" or "spike" or "overflow"))
                {
                    continue;
                }

                var ev = port.KeyEvents[k++];

                if (ev.Kind == "up")
                {
                    ups.Add((ev, row.FilteredSample));
                }
            }

            if (row.Printed.Length == 0)
            {
                continue;
            }

            var emission = port.Emissions[e];
            var c = settled[e];

            if (!c.IsWordGap)
            {
                var want = emission.Representation.Length;
                var mine = ups.Skip(Math.Max(0, ups.Count - want)).ToList();
                var found = want > 0 && mine.Count == want && mine[^1].Event.Representation == emission.Representation;
                var start = found ? (mine[0].Row - mine[0].Event.Element - PortFilterLag) / (double)FldigiCwDecoder.CW_SAMPLERATE : double.NaN;
                var end = found ? (mine[^1].Row - PortFilterLag) / (double)FldigiCwDecoder.CW_SAMPLERATE : double.NaN;

                list.Add(new Reading("port", e, c, start, end, p[e]));
            }

            ups = new List<(FldigiCwKeyEvent, long)>();
            e++;
        }

        return list;
    }

    /// <summary>
    /// The span rule (465 DECIDED (2)): two characters are on the same span where
    /// their spans overlap by at least <see cref="SameSpanShare"/> of the shorter;
    /// pairs are taken one to one, the largest overlap first. Then the case: the
    /// same text is agree; different text is a tie where the two p's are within
    /// <see cref="TieMargin"/> of each other, else disagree; a character with no
    /// partner is one-sided.
    /// </summary>
    internal static IReadOnlyList<Meeting> Meet(IReadOnlyList<Reading> ours, IReadOnlyList<Reading> port)
    {
        var candidates = new List<(int O, int P, double Overlap)>();

        for (var o = 0; o < ours.Count; o++)
        {
            for (var q = 0; q < port.Count; q++)
            {
                if (!ours[o].HasSpan || !port[q].HasSpan)
                {
                    continue;
                }

                var overlap = Math.Min(ours[o].End, port[q].End) - Math.Max(ours[o].Start, port[q].Start);
                var shorter = Math.Min(ours[o].End - ours[o].Start, port[q].End - port[q].Start);

                if (overlap > 0 && overlap >= SameSpanShare * shorter)
                {
                    candidates.Add((o, q, overlap));
                }
            }
        }

        var oursTaken = new int?[ours.Count];
        var portTaken = new bool[port.Count];

        foreach (var (o, q, _) in candidates.OrderByDescending(c => c.Overlap).ThenBy(c => c.O).ThenBy(c => c.P))
        {
            if (oursTaken[o] is null && !portTaken[q])
            {
                oursTaken[o] = q;
                portTaken[q] = true;
            }
        }

        var meetings = new List<Meeting>();

        for (var o = 0; o < ours.Count; o++)
        {
            if (oursTaken[o] is not { } q)
            {
                meetings.Add(new Meeting(ours[o], null, "one-sided ours"));
                continue;
            }

            var a = ours[o];
            var b = port[q];
            var kase = a.Character.Text == b.Character.Text ? "agree"
                : Math.Abs(a.P - b.P) <= TieMargin ? "tie"
                : "disagree";

            meetings.Add(new Meeting(a, b, kase));
        }

        for (var q = 0; q < port.Count; q++)
        {
            if (!portTaken[q])
            {
                meetings.Add(new Meeting(null, port[q], "one-sided port"));
            }
        }

        return meetings.OrderBy(m => double.IsNaN(m.At) ? double.MaxValue : m.At).ToList();
    }

    /// <summary>Both decoders' readings of one harness row, met.</summary>
    internal static (IReadOnlyList<Reading> Ours, IReadOnlyList<Reading> Port, IReadOnlyList<Meeting> Meetings) MeetRow(BothDecodersAreScoredAlikeTests.Row r)
    {
        var ours = OursOnTheClock(r.Ours.Settled);

        if (r.Port is null)
        {
            return (ours, Array.Empty<Reading>(), Meet(ours, Array.Empty<Reading>()));
        }

        var run = WhatEachDecoderKnowsAboutEachCharacterFact.RunPort(r);
        var port = PortOnTheClock(run, r.Port.Settled, FldigiConfidence.For(run));

        return (ours, port, Meet(ours, port));
    }

    /// <summary>The cases, in print order.</summary>
    internal static readonly string[] Cases = { "agree", "disagree", "tie", "one-sided ours", "one-sided port" };

    /// <remarks>
    /// Work instruction 465 task 1: the common clock, the pairing per recording,
    /// the counts per condition, and the rules fixed before any arbitrated text
    /// (V-14). Prints only; asserts that every recording was met.
    /// </remarks>
    [Fact]
    public void BothReadingsOnOneClock()
    {
        Print("fixed | span rule | two characters, one of each decoder, are on the same span where their spans overlap by at least half the shorter span, on the recording's clock in seconds from its first sample; pairs are taken one to one, the largest overlap first (ties: the earlier of ours, then the earlier of the port's); word boundaries are not characters and are not paired (465 DECIDED (2))");
        Print("fixed | cases | agree: the same text; disagree: different text with the two p's more than 0.05 apart; tie: different text with the two p's within 0.05 (|p ours - p port| <= 0.05); one-sided: no partner. A tie is a disagreement's case only: HM-REQ-127's 'the winning character' presumes two characters, and an agreement has one (a reading forced by the tree, recorded in section 4)");
        Print("fixed | margin | 0.05, HM-REQ-127's recommended value, on the absolute difference of the two p's; a named constant held until the owner rules (465 DECIDED (5))");
        Print("fixed | vote table | calibration.md section 2's held-out verdicts as unit 464 measured them (b30bef48, efdd5d11), per decoder per condition row; a decoder votes on a condition only where its verdict is calibrated (HM-REQ-124): ours on real HF, all; real HF, sender not stated; synthetic, all; synthetic TX-ITU 15 dB. The port on none (465 DECIDED (3))");
        Print("fixed | condition in force | in the harness, a recording's own condition row as TheRequirementsAreMeasuredTests states it, the most specific row the tree has for it; live, real HF, all, because no sender or channel profile is known live (465 DECIDED (3))");
        Print("fixed | who is emitted | both vote: agree with the more confident's class; disagree to the higher p at its own class; tie to the higher p, dim, never sure. One votes: its reading at its own class, the other's on the record; the advisory reading never displaces a character or raises a class. Neither votes: ours as it prints today. One-sided ours: ours as it prints today whether or not it votes. One-sided port: emitted only where the port votes (465 DECIDED (4))");

        Print("clock | ours | CwCharacter.At is the end of the character's last hop (CwProbabilisticStream.cs:575-577: the window's start hop plus the character's EndHop, times CwProbabilisticDecoder.HopMilliseconds, 5 ms); it starts CwCharacter.SpanHops hops before that");
        Print("clock | port | from its key events: each up event placed on the decision row it was raised in (FldigiCwDecisionRow.FilteredSample, 8000 Hz; TraceDecisions on, as FldigiConfidence reads them); the character ends at its last up event and starts at its first up event less that element's length (FldigiCwKeyEvent.Element, filtered samples), both less the filter's 512-sample lag; the up events are the last Representation.Length before the emission, the last carrying its representation, as FldigiConfidence.MarginsOf takes them. FldigiCwEmission.InputSample is when it was printed, and the port's filter hands out 1024 samples at a time, so it is 128 ms coarse and is not the span");
        Print("clock | harness feed | BothDecodersAreScoredAlikeTests.Both: the whole file through FldigiRateAdapter.ToFldigiRate (481-tap Blackman-windowed sinc, cut-off 3600 Hz, keep every 6th of 48000 Hz), and FldigiCwDecoder constructed once at the pitch instrument's median over the file's keyed windows (CwPitchInstrument.Measure), never at our tracker's pitch; ours is fed hop by hop from 600 Hz (real) or SyntheticCq.StartingPitchHz (synthetic) through CwDecoder.Process");

        Print("seam | CW tab | MainWindowViewModel.StartDecoding (MainWindowViewModel.cs:11244-11254): _decoder.LeadingEdge += Transcript.OfferEdge (the leading edge, replaced whole each time); _decoder.CharacterSettled += Transcript.Settle (what the transcript keeps); _decoder.CharacterDecoded, which takes no text and stamps _lastDecodeUtc and _lastCharacterUtc; unsubscribed in StopDecoding (11353-11354). CwTranscript (ViewModels/CwTranscript.cs) is the one transcript the tab renders");
        Print("seam | other readers of settled characters | AutoCallViewModel.cs:426 and ScanViewModel.cs:515 each subscribe CharacterSettled += Take on the decoder they are handed; neither is the CW tab's transcript, and both take whatever CharacterSettled raises");
        Print("seam | capture sheet | MainWindowViewModel.CaptureAudioAsync writes the sidecar: `text` (MainWindowViewModel.cs:12012, Transcript.PlainText) and `spanLlr` (12033, SpanRatioLine over Transcript.Recent(): per character its text, SpanLogLikelihoodRatio, MarginLlr and MarginShareForRecord). No p, and no second reading, is recorded per character today");
        Print("seam | the chunks | CwDecoder.Listen subscribes OnSamples to the IAudioSource; CwDecoder.Process walks each AudioChunk a tracker hop at a time (CwDecoder.Step), at the source's own rate (_audioInput.SampleRate: WasapiAudioSource at the device's rate, TrainingAudioSource at its own). Step is where a second reader can be handed the same hop at the pitch in force (_probabilistic.ToneHz), outside Cw/Second");
        Print("seam | the port's pitch | FldigiCwDecoder.frequency is readonly, set only by the constructor (FldigiCwDecoder.cs:281), and Frequency has a getter only (342); upstream fldigi's modem::set_freq is not in the port. The port cannot follow a pitch change without a line under Cw/Second changing, so it is re-constructed at the new pitch, losing its filter and speed state (465 DECIDED (8))");

        var perRecording = new List<(BothDecodersAreScoredAlikeTests.Row Row, IReadOnlyList<Meeting> Meetings)>();
        var offsets = new List<(double Start, double End)>();

        Print("pair | recording | case | ours span s | ours text | ours class | ours p | port span s | port text | port class | port p");

        foreach (var r in BothDecodersAreScoredAlikeTests.Rows)
        {
            var (ours, port, meetings) = MeetRow(r);

            perRecording.Add((r, meetings));

            foreach (var m in meetings)
            {
                Print($"pair | {r.Name} | {m.Case} | {Side(m.Ours)} | {Side(m.Port)}");

                if (m.Ours is { } a && m.Port is { } b && m.Case == "agree")
                {
                    offsets.Add((b.Start - a.Start, b.End - a.End));
                }
            }

            Print(string.Create(Invariant,
                $"unspanned | {r.Name} | ours {ours.Count(x => !x.HasSpan)} of {ours.Count} | port {port.Count(x => !x.HasSpan)} of {port.Count}{(r.Port is null ? " (port not run: " + r.PortNotRun + ")" : "")}"));
        }

        if (offsets.Count > 0)
        {
            var starts = offsets.Select(o => o.Start).OrderBy(x => x).ToList();
            var ends = offsets.Select(o => o.End).OrderBy(x => x).ToList();

            Print(string.Create(Invariant,
                $"clock check | on {offsets.Count} agreements, port less ours: start median {starts[starts.Count / 2]:0.000} s (10th {starts[starts.Count / 10]:0.000}, 90th {starts[starts.Count * 9 / 10]:0.000}); end median {ends[ends.Count / 2]:0.000} s (10th {ends[ends.Count / 10]:0.000}, 90th {ends[ends.Count * 9 / 10]:0.000})"));
        }

        Print("count | condition | key | recordings | agree | disagree | tie within 0.05 | one-sided ours | one-sided port | agree with p within 0.05 | ours votes | port votes");

        foreach (var real in new[] { true, false })
        {
            var set = perRecording.Where(x => x.Row.Real == real).ToList();

            Count(real ? "real HF, all" : "synthetic, all", real, set);

            foreach (var g in set.GroupBy(x => x.Row.Condition).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                Count(g.Key, real, g.ToList());
            }
        }

        Print("p on disagreements | condition | disagreements and ties | mean p ours | mean p port | port higher | ours higher | equal");

        foreach (var real in new[] { true, false })
        {
            var d = perRecording.Where(x => x.Row.Real == real).SelectMany(x => x.Meetings)
                .Where(m => m.Case is "disagree" or "tie").ToList();

            Print(d.Count == 0
                ? $"p on disagreements | {(real ? "real HF, all" : "synthetic, all")} | 0"
                : string.Create(Invariant,
                    $"p on disagreements | {(real ? "real HF, all" : "synthetic, all")} | {d.Count} | {d.Average(m => m.Ours!.P):0.000} | {d.Average(m => m.Port!.P):0.000} | {d.Count(m => m.Port!.P > m.Ours!.P)} | {d.Count(m => m.Ours!.P > m.Port!.P)} | {d.Count(m => m.Ours!.P == m.Port!.P)}"));
        }

        Assert.Equal(BothDecodersAreScoredAlikeTests.Rows.Count, perRecording.Count);
    }

    /// <remarks>
    /// Work instruction 465 task 2, item 5: all 35 through <see cref="CwArbiter"/>
    /// under <see cref="CwVoteTable"/>, each recording under its own condition row.
    /// Prints the arbitrated transcript in task 0's save format, the arbiter's own
    /// records counted per case per condition in task 1's format, and a check that
    /// <see cref="CwSecondHarvester"/> reads the port exactly as task 1's clock did.
    /// Asserts only that the harvester and task 1 agree; the comparison with task 0's
    /// save is made on the printout.
    /// </remarks>
    [Fact]
    public void TheCorpusThroughTheArbiter()
    {
        var byRow = new List<(BothDecodersAreScoredAlikeTests.Row Row, CwArbitrated Result)>();
        var harvestDiffers = 0;

        foreach (var r in BothDecodersAreScoredAlikeTests.Rows)
        {
            var second = Array.Empty<CwSecondReading>() as IReadOnlyList<CwSecondReading>;

            if (r.Port is not null)
            {
                var run = WhatEachDecoderKnowsAboutEachCharacterFact.RunPort(r);
                var task1 = PortOnTheClock(run, r.Port.Settled, FldigiConfidence.For(run));

                second = CwSecondHarvester.Of(run);

                var same = second.Count == task1.Count && second.Zip(task1).All(x =>
                    x.First.Text == x.Second.Character.Text && x.First.Confidence == x.Second.Character.Confidence
                    && x.First.P.Equals(x.Second.P) && x.First.Start.Equals(x.Second.Start) && x.First.End.Equals(x.Second.End));

                Print($"harvest check | {r.Name} | {(same ? "same" : "DIFFERS")} | {second.Count} readings, task 1 {task1.Count}");
                harvestDiffers += same ? 0 : 1;
            }

            var result = CwArbiter.Arbitrate(r.Ours.Settled, second, CwVoteTable.For(r.Condition));

            byRow.Add((r, result));

            for (var i = 0; i < result.Characters.Count; i++)
            {
                var c = result.Characters[i];

                Print(string.Create(Invariant,
                    $"save | ours | {r.Name} | {i} | {Visible(c.Text)} | {ClassOf(c)} | {P(c.Probability)}"));
            }
        }

        Print("arbiter count | condition | key | recordings | agree | disagree | tie within 0.05 | one-sided ours | one-sided port | emitted from ours | emitted from the port | not emitted | ours votes | port votes");

        foreach (var real in new[] { true, false })
        {
            var set = byRow.Where(x => x.Row.Real == real).ToList();

            ArbiterCount(real ? "real HF, all" : "synthetic, all", real, set);

            foreach (var g in set.GroupBy(x => x.Row.Condition).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                ArbiterCount(g.Key, real, g.ToList());
            }
        }

        Assert.Equal(0, harvestDiffers);
    }

    /// <remarks>
    /// Work instruction 465 task 3, items 1, 5 and 6: all 35 through the live
    /// <see cref="CwDecoder"/> path, fed hop by hop from sample 0 as the metrics
    /// feed it, once with ours alone and once with the second reader and the
    /// arbiter. Prints the live transcript in task 0's save format, how often the
    /// port was built or rebuilt, the live spans per case, and the decode time of
    /// each. Asserts only that every recording was read both ways.
    /// </remarks>
    [Fact]
    public void TheLivePathOverTheCorpus()
    {
        var recordings = WhatTheStrayLettersRestOnTests.KeyedRecordings
            .Select(k => (k.Name, Real: true, Condition: TheRequirementsAreMeasuredTests.RealCondition(k.Name),
                File: Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), Hz: 600.0))
            .Concat(Fixtures.SyntheticCq.All.Select(c => (c.Name, Real: false, Condition: TheRequirementsAreMeasuredTests.SyntheticCondition(c),
                File: Path.Combine(Fixtures.SyntheticCq.Folder, c.Name + ".wav"), Hz: Fixtures.SyntheticCq.StartingPitchHz)))
            .ToList();
        var alone = new Dictionary<bool, double> { [true] = 0, [false] = 0 };
        var both = new Dictionary<bool, double> { [true] = 0, [false] = 0 };
        var audioSeconds = new Dictionary<bool, double> { [true] = 0, [false] = 0 };
        var read = 0;

        Print("live | recording | constructions | retunes | after skips | memory rebuilds | agree | disagree | tie | one-sided ours | one-sided port (not emitted) | ours alone s | both s");

        foreach (var (name, real, _, file, hz) in recordings)
        {
            var audio = WavAudio.Read(file);
            var (_, tAlone, _, _) = Live(audio.Samples, audio.SampleRate, hz, false);
            var (live, tBoth, decoder, unemitted) = Live(audio.Samples, audio.SampleRate, hz, true);
            var records = live.Where(c => !c.IsWordGap).Select(c => c.Arbitration!).ToList();
            var r = decoder.SecondReader!;

            alone[real] += tAlone;
            both[real] += tBoth;
            audioSeconds[real] += audio.Samples.Length / (double)audio.SampleRate;
            read++;

            for (var i = 0; i < live.Count; i++)
            {
                var c = live[i];

                Print(string.Create(Invariant,
                    $"save | ours | {name} | {i} | {Visible(c.Text)} | {ClassOf(c)} | {P(c.Probability)}"));
            }

            Print(string.Create(Invariant,
                $"live | {name} | {r.Constructions} | {r.Retunes} | {r.AfterSkips} | {r.MemoryRebuilds} | {records.Count(a => a.Case == CwArbitrationCase.Agree)} | {records.Count(a => a.Case == CwArbitrationCase.Disagree)} | {records.Count(a => a.Case == CwArbitrationCase.Tie)} | {records.Count(a => a.Case == CwArbitrationCase.OneSidedOurs)} | {unemitted} | {tAlone:0.00} | {tBoth:0.00}"));
        }

        foreach (var real in new[] { true, false })
        {
            Print(string.Create(Invariant,
                $"decode time | {(real ? "real HF, inferred keys" : "synthetic, exact keys")} | audio {audioSeconds[real]:0.0} s | ours alone {alone[real]:0.00} s | ours, the port and the arbiter {both[real]:0.00} s | ratio {both[real] / alone[real]:0.00}"));
        }

        Assert.Equal(recordings.Count, read);
    }

    // One recording through CwDecoder hop by hop, as BothDecodersAreScoredAlikeTests.DriveOurs feeds it.
    private static (List<CwCharacter> Settled, double Seconds, CwDecoder Decoder, int Unemitted) Live(float[] samples, int rate, double hz, bool second)
    {
        var decoder = new CwDecoder(rate, hz, second);
        var settled = new List<CwCharacter>();
        var hop = decoder.Tracker.HopSamples;

        decoder.CharacterSettled += settled.Add;

        var watch = System.Diagnostics.Stopwatch.StartNew();

        for (var at = 0L; at + hop <= samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, rate, samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();
        watch.Stop();

        return (settled, watch.Elapsed.TotalSeconds, decoder, decoder.SecondOnlySpans);
    }

    private void ArbiterCount(string label, bool real, IReadOnlyList<(BothDecodersAreScoredAlikeTests.Row Row, CwArbitrated Result)> rows)
    {
        var all = rows.SelectMany(x => x.Result.Records).ToList();
        var cases = new[]
        {
            CwArbitrationCase.Agree, CwArbitrationCase.Disagree, CwArbitrationCase.Tie,
            CwArbitrationCase.OneSidedOurs, CwArbitrationCase.OneSidedSecond,
        };
        var vote = CwVoteTable.For(label);

        Print(string.Create(Invariant,
            $"arbiter count | {label} | {(real ? "inferred" : "exact")} | {rows.Count} | {string.Join(" | ", cases.Select(c => all.Count(r => r.Case == c)))} | {all.Count(r => r.Emitted == CwReader.Ours)} | {all.Count(r => r.Emitted == CwReader.Second)} | {all.Count(r => r.Emitted is null)} | {(vote.Ours ? "votes" : "advisory")} | {(vote.Second ? "votes" : "advisory")}"));
    }

    private void Count(string label, bool real, IReadOnlyList<(BothDecodersAreScoredAlikeTests.Row Row, IReadOnlyList<Meeting> Meetings)> rows)
    {
        var all = rows.SelectMany(x => x.Meetings).ToList();
        var closeAgree = all.Count(m => m.Case == "agree" && Math.Abs(m.Ours!.P - m.Port!.P) <= TieMargin);

        Print(string.Create(Invariant,
            $"count | {label} | {(real ? "inferred" : "exact")} | {rows.Count} | {string.Join(" | ", Cases.Select(c => all.Count(m => m.Case == c)))} | {closeAgree} | {Votes(label, "ours")} | {Votes(label, "port")}"));
    }

    // calibration.md section 2's held-out verdicts (465 DECIDED (3)); the four rows ours is calibrated on, the port on none.
    private static string Votes(string condition, string decoder)
        => decoder == "ours" && (condition is "real HF, all" or "synthetic, all"
                                 || condition.EndsWith("sender not stated in CW_SPEC.md", StringComparison.Ordinal)
                                 || (condition.Contains("TX-ITU (1:3:1:3:7), 15 dB", StringComparison.Ordinal)))
            ? "votes"
            : "advisory";

    private static string Side(Reading? r) => r is null
        ? "none | | | "
        : string.Create(Invariant, $"{S(r.Start)}-{S(r.End)} | {Visible(r.Character.Text)} | {ClassOf(r.Character)} | {r.P:0.000}");

    private static string S(double s) => double.IsNaN(s) ? "NaN" : s.ToString("0.000", Invariant);

    private void Print(string line) => _output.WriteLine(line);

    /// <summary>sure, dim, placeholder or gap, as <see cref="CwSymbol"/> classes it.</summary>
    internal static string ClassOf(CwCharacter c) => c.IsWordGap ? "gap" : CwSymbol.Of(c).Class switch
    {
        CwSymbolClass.Sure => "sure",
        CwSymbolClass.NotSure => "dim",
        _ => "placeholder",
    };

    /// <summary>A word boundary printed as a word, so a line never ends in a space.</summary>
    internal static string Visible(string text) => text == MorseAlphabet.WordGap ? "(space)" : text;

    /// <summary>A p printed round-trip, or NaN.</summary>
    internal static string P(double p) => double.IsNaN(p) ? "NaN" : p.ToString("R", Invariant);
}
