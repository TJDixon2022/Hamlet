using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The join nobody has ever taken.</b> Upstream's expected lists carry a signal-to-noise ratio on
/// every one of their 1298 lines, in <c>fields[1]</c>, parsed by a checked-in test since unit 216 —
/// and no unit has ever put that column beside matched-or-missed.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it decides something.</b> Criterion 3 has stood at 760 of 1298 through three units with
/// every miss named and every closable cause measured at zero, and the argument that could not be
/// settled is whether the residue is this receiver being deaf or those recordings being beyond it.
/// Split the expected lines by the strength their own list gives them and the argument resolves one
/// way or the other: either the misses are the faint ones, which is a receiver behaving like a
/// receiver, or this port is missing loud ones, which is a defect with an address.
/// </para>
/// <para>
/// <b>THE TWO dB SCALES ARE NOT PROVEN TO BE THE SAME SCALE.</b> The column in these lists is a third
/// party's estimate under a convention this project did not choose — and unit 216 proved from the
/// lists themselves that they were not written by the pinned decoder, so it is not even
/// <c>ft8_lib</c>'s convention. It is treated here as <b>ordinal</b>: shapes and rankings are
/// compared, absolute agreement is not, and no conclusion that depends on the two scales being
/// identical is available from this file.
/// </para>
/// <para>
/// <b>One reading survives any convention</b>, and it is the sharpest thing here: a missed expected
/// line whose own column says 0 dB or better is a strong signal by anybody's convention, and no
/// sensitivity argument covers it.
/// </para>
/// <para>
/// <b>NOTHING IS TOLD TO THE DECODE PATH.</b> The recordings are decoded exactly as
/// <c>TheReferenceRecordingsDecodeAgainstUpstreamsOwnExpectedLists</c> decodes them — samples in,
/// text out, no frequency, no count, no list. The expected lines are read <em>afterwards</em> and
/// joined to what came back.
/// </para>
/// </remarks>
public class Ft8OnAirSnrJoinTests
{
    /// <summary>The bin width, in decibels of the list's own column.</summary>
    private const double BinWidth = 3.0;

    private readonly ITestOutputHelper _output;

    public Ft8OnAirSnrJoinTests(ITestOutputHelper output) => _output = output;

    /// <summary>One expected line: what the list said, how strong it said it was, and whether it came back.</summary>
    private sealed record Line(string File, double Snr, string Text, bool Hashed)
    {
        internal bool Matched { get; set; }
    }

    /// <summary>
    /// <b>The 1298 expected lines split by the SNR their own list gives them, against the decode
    /// rate.</b>
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheExpectedListsOwnSnrColumnIsJoinedToMatchedOrMissed()
    {
        var recordings = ReferenceRecordings.WithExpectedLists();
        Assert.NotEmpty(recordings);

        var lines = new List<Line>();
        var unparsed = 0;
        var extras = 0;

        foreach (var recording in recordings)
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);

            // Read the list into rows carrying their own SNR. The text is normalised exactly the
            // way ReferenceRecording.ExpectedMessages normalises it, so this is the same list.
            var rows = new List<Line>();
            foreach (var raw in File.ReadAllLines(recording.ExpectedPath))
            {
                var tilde = raw.IndexOf('~');
                if (tilde < 0)
                {
                    continue;
                }

                var text = ReferenceRecording.Normalise(raw[(tilde + 1)..]);
                if (text.Length == 0)
                {
                    continue;
                }

                var fields = raw[..tilde].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length < 4 || !double.TryParse(fields[1], out var snr))
                {
                    unparsed++;
                    continue;
                }

                var hashed = ExpectedMessagePacker.TryPack(text, out _)
                    == ExpectedMessagePacker.PackFailure.HashedCallsignLostInTheList;

                rows.Add(new Line(recording.Name, snr, text, hashed));
            }

            // THE UNTOLD PATH. Samples in, text out. Identical to the criterion's own run.
            var result = new Ft8SlotDecoder(geometry).Decode(recording.ReadSamples());
            var returned = result.Texts.Select(ReferenceRecording.Normalise).ToList();

            // The same multiset comparison the criterion uses, so a list carrying one message twice
            // is not satisfied by one decode - the difference being that here the row that was
            // satisfied is remembered rather than only counted.
            foreach (var text in returned)
            {
                var at = rows.FindIndex(r => !r.Matched && string.Equals(r.Text, text, StringComparison.Ordinal));
                if (at >= 0)
                {
                    rows[at].Matched = true;
                }
                else
                {
                    extras++;
                }
            }

            lines.AddRange(rows);
        }

        var matched = lines.Count(l => l.Matched);
        var missed = lines.Count - matched;

        _output.WriteLine("THE JOIN. Every expected line, its own list's SNR, and whether it came back.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  recordings              : {recordings.Count}");
        _output.WriteLine($"  expected lines parsed   : {lines.Count}");
        _output.WriteLine($"  lines with no SNR field : {unparsed}");
        _output.WriteLine($"  MATCHED                 : {matched}");
        _output.WriteLine($"  MISSED                  : {missed}");
        _output.WriteLine($"  returned, not on a list : {extras}");
        _output.WriteLine($"  the column's range      : {lines.Min(l => l.Snr):F1} to {lines.Max(l => l.Snr):F1} dB");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  These totals must reproduce unit 216's and unit 217's, because this is");
        _output.WriteLine("  the same untold run read a second way and not a new or filtered one.");
        _output.WriteLine(string.Empty);

        WriteTable("ALL 1298 EXPECTED LINES", lines);

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE SAME TABLE WITH THE HASHED LINES EXCLUDED. Unit 217 measured that the");
        _output.WriteLine("list's own writer lost the callsign on these, printing <...>, so NO receiver");
        _output.WriteLine("could ever match them. BOTH TABLES ARE GIVEN AND NEITHER STANDS ALONE.");
        _output.WriteLine(string.Empty);

        var representable = lines.Where(l => !l.Hashed).ToList();
        _output.WriteLine($"  hashed lines excluded   : {lines.Count - representable.Count}");
        _output.WriteLine($"  lines remaining         : {representable.Count}");
        _output.WriteLine($"  MATCHED                 : {representable.Count(l => l.Matched)}");
        _output.WriteLine(string.Empty);

        WriteTable("EXPECTED LINES THE LIST DID NOT LOSE TO A HASH", representable);

        // THE NUMBER THAT NEEDS NO CALIBRATION.
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE NUMBER THAT NEEDS NO CALIBRATION AT ALL. A missed message at +3 dB is a");
        _output.WriteLine("defect no sensitivity argument covers, whatever convention the column uses.");
        _output.WriteLine(string.Empty);

        foreach (var floor in new[] { 0.0, -5.0 })
        {
            var strong = lines.Where(l => l.Snr >= floor).ToList();
            var strongMissed = strong.Where(l => !l.Matched).ToList();
            var strongMissedRepresentable = strongMissed.Where(l => !l.Hashed).ToList();

            _output.WriteLine($"  AT {floor,5:F1} dB OR BETTER, BY THE LIST'S OWN COLUMN:");
            _output.WriteLine($"    expected lines        : {strong.Count}");
            _output.WriteLine($"    matched               : {strong.Count - strongMissed.Count}");
            _output.WriteLine($"    MISSED                : {strongMissed.Count}");
            _output.WriteLine($"    of those, hashed      : {strongMissed.Count - strongMissedRepresentable.Count}"
                + "  (no receiver could match these)");
            _output.WriteLine($"    MISSED AND MATCHABLE  : {strongMissedRepresentable.Count}");
            _output.WriteLine(string.Empty);

            if (strongMissed.Count > 0)
            {
                _output.WriteLine($"    every one of them, in full:");
                _output.WriteLine($"    {"file",-22} {"snr",6} {"hashed",7}  text");
                foreach (var line in strongMissed.OrderByDescending(l => l.Snr))
                {
                    _output.WriteLine($"    {line.File,-22} {line.Snr,6:F1} "
                        + $"{(line.Hashed ? "HASHED" : string.Empty),7}  {line.Text}");
                }

                _output.WriteLine(string.Empty);
            }
        }

        _output.WriteLine("  THE TWO dB SCALES ARE NOT PROVEN TO BE THE SAME SCALE. This column is a");
        _output.WriteLine("  third party's estimate under its own convention - and unit 216 proved");
        _output.WriteLine("  these lists were not written by the pinned decoder, so it is not even");
        _output.WriteLine("  ft8_lib's convention. It is read here as ORDINAL: shapes and rankings,");
        _output.WriteLine("  never absolute agreement with the synthetic ladder's axis.");

        // The integrity check, and it is the one that matters: the parse did not lose a line, and
        // matched plus missed is the whole list. NO BOUND IS PUT ON THE MATCH COUNT ITSELF - a test
        // that reddens when the receiver improves is worse than no test.
        var declared = recordings.Sum(r => r.ExpectedCount);
        Assert.Equal(declared, lines.Count);
        Assert.Equal(lines.Count, matched + missed);
    }

    /// <summary>
    /// <b>Where the strong-SNR misses actually die, and whether they are random.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This extends unit 217's accounting rather than rebuilding it.</b> That unit gave all 538
    /// misses a bucket and an agreement figure; what it did not do — because nobody had joined the
    /// SNR column yet — is ask whether the misses the list calls <em>strong</em> die differently from
    /// the ones it calls weak. If they die the same way, the list's column is telling us little. If
    /// they die at the search while the weak ones die at the code, that is two different faults.
    /// </para>
    /// <para>
    /// <b>And one reading that costs nothing and could matter more than either.</b> If the same text
    /// is missed in file after file, the misses are not random draws against a noise floor — they are
    /// a property of a particular station's signal, and that has an address a noise argument does
    /// not.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheStrongSnrMissesAreLocatedAndCountedForRepeats()
    {
        const double StrongFloor = 0.0;

        var recordings = ReferenceRecordings.WithExpectedLists();
        var search = new Ft8SyncSearch();

        var rows = new List<(string File, double Snr, string Text, double Hz, bool WithinFourHz)>();
        var withCandidate = 0;

        foreach (var recording in recordings)
        {
            var geometry = new Ft8WaterfallGeometry(recording.SampleRate);

            var strong = new List<(double Snr, string Text, double Hz)>();
            var all = new List<string>();
            foreach (var raw in File.ReadAllLines(recording.ExpectedPath))
            {
                var tilde = raw.IndexOf('~');
                if (tilde < 0)
                {
                    continue;
                }

                var text = ReferenceRecording.Normalise(raw[(tilde + 1)..]);
                if (text.Length == 0)
                {
                    continue;
                }

                all.Add(text);

                var fields = raw[..tilde].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length < 4 || !double.TryParse(fields[1], out var snr) || snr < StrongFloor)
                {
                    continue;
                }

                if (ExpectedMessagePacker.TryPack(text, out _)
                    == ExpectedMessagePacker.PackFailure.HashedCallsignLostInTheList)
                {
                    continue;
                }

                if (double.TryParse(fields[3], out var hz))
                {
                    strong.Add((snr, text, hz));
                }
            }

            if (strong.Count == 0)
            {
                continue;
            }

            // The untold path again, plus the candidate list read out of the same waterfall.
            var waterfall = new Ft8Monitor(geometry).Analyse(recording.ReadSamples());
            var result = new Ft8SlotDecoder(geometry).Decode(waterfall);
            var returned = result.Texts.Select(ReferenceRecording.Normalise).ToList();
            var candidates = search.Find(waterfall);

            var outstanding = new List<string>(all);
            foreach (var text in returned)
            {
                var at = outstanding.FindIndex(e => string.Equals(e, text, StringComparison.Ordinal));
                if (at >= 0)
                {
                    outstanding.RemoveAt(at);
                }
            }

            foreach (var (snr, text, hz) in strong)
            {
                var at = outstanding.FindIndex(e => string.Equals(e, text, StringComparison.Ordinal));
                if (at < 0)
                {
                    continue;
                }

                outstanding.RemoveAt(at);

                var near = SensitivityLadder.NearestTo(candidates, geometry, hz) is not null;
                if (near)
                {
                    withCandidate++;
                }

                rows.Add((recording.Name, snr, text, hz, near));
            }
        }

        _output.WriteLine("THE MISSED EXPECTED LINES AT 0 dB OR BETTER THAT A RECEIVER COULD HAVE");
        _output.WriteLine("MATCHED - the hashed ones excluded, because no receiver could match those.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  of them                          : {rows.Count}");
        _output.WriteLine($"  WITH A KEPT CANDIDATE WITHIN 4 Hz: {withCandidate}");
        _output.WriteLine($"  with no candidate near them      : {rows.Count - withCandidate}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  Unit 216 measured the same thing over ALL the misses and got 509 of 531,");
        _output.WriteLine("  95.9 per cent. If the strong ones match that, they die where the weak ones");
        _output.WriteLine("  die and the list's column is not separating two faults.");
        _output.WriteLine(string.Empty);

        _output.WriteLine("ARE THEY RANDOM? The same text missed in file after file is not a draw");
        _output.WriteLine("against a noise floor - it is a property of one station's signal.");
        _output.WriteLine(string.Empty);

        var repeats = rows
            .GroupBy(r => r.Text, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ToList();

        _output.WriteLine($"  distinct texts among them        : {repeats.Count}");
        _output.WriteLine($"  texts missed in more than one file: {repeats.Count(g => g.Count() > 1)}");
        _output.WriteLine($"  lines belonging to a repeated text: {repeats.Where(g => g.Count() > 1).Sum(g => g.Count())}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"times",6} {"snr range",14}  text");
        foreach (var group in repeats.Where(g => g.Count() > 1))
        {
            _output.WriteLine($"{group.Count(),6} {group.Min(r => r.Snr),6:F0} to "
                + $"{group.Max(r => r.Snr),3:F0}  {group.Key}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"file",-22} {"snr",6} {"list Hz",9} {"4Hz",4}  text");
        foreach (var row in rows.OrderByDescending(r => r.Snr))
        {
            _output.WriteLine($"{row.File,-22} {row.Snr,6:F1} {row.Hz,9:F1} "
                + $"{(row.WithinFourHz ? "yes" : "NO"),4}  {row.Text}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOTHING WAS TOLD TO THE DECODE PATH. The frequency column is read only to");
        _output.WriteLine("  choose which candidate to look at AFTER the search has answered; the");
        _output.WriteLine("  search's own list was taken whole and was not filtered by it.");

        // The instrument must have found something to say. No bound on the count itself.
        Assert.NotEmpty(rows);
    }

    /// <summary>Prints one SNR-binned table: bin, lines, matched, missed, rate.</summary>
    private void WriteTable(string title, IReadOnlyList<Line> lines)
    {
        _output.WriteLine(title);
        _output.WriteLine($"{"snr bin, dB",14} {"lines",7} {"matched",8} {"missed",7} {"rate %",8}");

        var lowest = Math.Floor(lines.Min(l => l.Snr) / BinWidth) * BinWidth;
        var highest = Math.Floor(lines.Max(l => l.Snr) / BinWidth) * BinWidth;

        for (var edge = highest; edge >= lowest; edge -= BinWidth)
        {
            var bin = lines.Where(l => l.Snr >= edge && l.Snr < edge + BinWidth).ToList();
            var binMatched = bin.Count(l => l.Matched);
            var rate = bin.Count == 0 ? 0.0 : 100.0 * binMatched / bin.Count;

            _output.WriteLine($"{edge,6:F0} to {edge + BinWidth,3:F0} {bin.Count,7} {binMatched,8} "
                + $"{bin.Count - binMatched,7} {rate,8:F1}");
        }

        _output.WriteLine($"{"TOTAL",14} {lines.Count,7} {lines.Count(l => l.Matched),8} "
            + $"{lines.Count(l => !l.Matched),7} "
            + $"{(lines.Count == 0 ? 0 : 100.0 * lines.Count(l => l.Matched) / lines.Count),8:F1}");
        _output.WriteLine("  THE COUNT IN EVERY BIN IS GIVEN so a bin with four lines in it cannot be");
        _output.WriteLine("  read as a trend.");
    }
}
