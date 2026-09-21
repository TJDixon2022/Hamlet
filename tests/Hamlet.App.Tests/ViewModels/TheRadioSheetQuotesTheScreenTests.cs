using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **Criteria 4.2 and 4.3: every sentence `docs/RADIO_SHEET.md` quotes is one Hamlet says, every
/// refusal Hamlet can say on the PSK31 and Olivia send path is on the sheet, and no line of the
/// sheet - quotes included - sends the operator to the radio except the sentences Hamlet itself
/// says, which are declared one by one and proved to be its own.**
/// </summary>
/// <remarks>
/// <para>**IT IS ASSERTED IN BOTH DIRECTIONS, AND THE BACKWARD ONE IS THE POINT** (work
/// instruction 382 section 6 ruling 1 item 3). Forward alone would pass a sheet that quoted one
/// sentence and left eight out. The backward direction is what keeps the sheet true after
/// tonight: a later unit that adds a refusal to the send path fails this name until the sheet
/// carries it.</para>
/// <para>**TWO KINDS OF CHECK, AND THE OUTPUT SAYS WHICH ONE EACH QUOTE GOT** (ruling 1 item 4).
/// **Kind (a)** drives the shipped view model into the state and compares the sheet's quote with
/// what Hamlet actually put in <c>DigitalSendLine</c>, <c>TransmitRefusalSentence</c> or the other
/// operator-facing lines, character for character. **Kind (b)** finds the literal in a file under
/// <c>src/Hamlet.App</c>, with a sentence assembled from several literals - as
/// <c>NoTransmitDeviceSentence</c> is - found by its parts, in order, in one file. Kind (b) is the
/// weaker one and the report names every sentence that got it.</para>
/// <para>**A SENTENCE WITH A VALUE SPLICED INTO IT IS CHECKED EITHER SIDE OF THE VALUE** (ruling 1
/// item 5). The sheet writes the varying part in angle brackets, and what is compared is each
/// fixed part, in order - so a sheet that quoted one variant's name would be telling the operator
/// he will see a word he may not see, and this would not accept it.</para>
/// <para>**THE SHEET IS REACHED BY THE WALK TO THE REPOSITORY ROOT** that
/// <c>DecisionLogOrderTests.cs:49</c> already uses. There is no new test project, no new fixture
/// kind and no copy of the sheet in the build output.</para>
/// <para>**NOTHING IS KEYED** (§0.2, FACT-004): the wire is `FakePort`, the sound card is
/// `FakeSink`, and every refusal below returns before either is touched.</para>
/// </remarks>
public sealed class TheRadioSheetQuotesTheScreenTests : IDisposable
{
    private const string Mine = "K1ABC";

    /// <summary>20 m, inside a General class data segment, where both modes have a block.</summary>
    private const long On20m = 14_071_500;

    /// <summary>Ruling 2 item 2's target, which is reported and is never a red.</summary>
    private const int TargetLines = 120;

    /// <summary>The same target in bytes.</summary>
    private const int TargetBytes = 10 * 1024;

    /// <summary>
    /// **Tier 1: the instructions** (work instruction 383 section 6 ruling 1 item 2).
    /// </summary>
    /// <remarks>
    /// Each one names the box or a control on it **and tells the reader to do something to it**,
    /// which is the shape R11 forbids: *go and operate it*. The first nine are the union of the
    /// phrase-shaped entries of the three lists in this tree - the eleven this type used before
    /// tonight, <c>TheOliviaSendTests.cs:390</c> and <c>TheAlcSentenceTests.cs:44-49</c> - and the
    /// last two are the ALC list's own, which name a meter on the front of the radio and are on
    /// this list for the reason that list gives: a sentence that needs one of them has handed the
    /// judgement back to the person this path exists to spare. **Whole words, case insensitive.**
    /// </remarks>
    private static readonly string[] Tier1 =
    {
        "move the dial", "turn the dial", "set the dial", "tune the radio", "turn the radio",
        "turn the knob", "at the radio", "on the radio", "at the rig's front", "ALC bar",
        "marked zone",
    };

    /// <summary>
    /// **Tier 2: the hardware nouns, counted and never a red** (ruling 1 item 3).
    /// </summary>
    /// <remarks>
    /// These name the box and its controls and are **not by themselves an instruction**. *A chip
    /// is lit when the dial is inside that mode's block* describes where the receiver is tuned and
    /// asks for nothing; *pick the radio's sound card* names a Windows device; *Connect a radio or
    /// pick the training radio* is Hamlet saying what it has no way to hear through. Turning any
    /// of those red would push the page into paraphrasing English, or into dropping a quote, and
    /// both are forbidden. **What they buy instead is a number in the report.**
    /// </remarks>
    private static readonly string[] Tier2 =
    {
        "knob", "dial", "VFO", "PTT", "mic gain", "RF gain", "meter", "ALC", "rig",
        "transceiver", "radio",
    };

    /// <summary>
    /// **The closed list of tier-1 phrases the sheet is allowed to carry inside a quote** (ruling
    /// 1 item 4).
    /// </summary>
    /// <remarks>
    /// <para>**ONE ENTRY PER SENTENCE, WITH ITS SITE AND ONE LINE SAYING WHY HAMLET SAYS IT.**
    /// The list is closed in both directions: a quoted tier-1 hit that matches no entry is a red,
    /// and an entry that matches nothing on the sheet is a red too, so it cannot rot into a
    /// standing permission.</para>
    /// <para>**NEITHER SENTENCE MAY BE REWRITTEN TO PASS THIS SCAN** (`PHASE_PLAN.md` §6, work
    /// instruction 383 section 9). One of them is what Hamlet says when its own unkey did not get
    /// out, and the only correct advice at that moment is the advice it gives.</para>
    /// </remarks>
    private static readonly DeclaredSentence[] Declared =
    {
        new(
            "the band is too crowded here to call",
            "MainWindowViewModel.cs:16800-16804, the refusal whose token at :16798 is "
            + "no_clear_spot",
            "Hamlet looked for a spot at least 150 Hz from everything it can hear and there was "
            + "not one, and the band is the only thing that changes that."),
        new(
            "neither frame got out",
            "MainWindowViewModel.cs:19587, StopLine, where an abort was fired and neither the CW "
            + "stop nor the PTT-off frame reached the radio",
            "Hamlet told the radio to stop and nothing it sent got there, so the radio may still "
            + "be transmitting and Hamlet has no way left to stop it."),
    };

    /// <summary>Where a value is spliced into a sentence the sheet quotes.</summary>
    private static readonly Regex Spliced = new("<[^<>]*>", RegexOptions.Compiled);

    /// <summary>The glue between two adjacent C# string literals, for kind (b).</summary>
    private static readonly Regex Concatenation = new("\"\\s*\\+\\s*\"", RegexOptions.Compiled);

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each quote and the kind it got is printed.</param>
    public TheRadioSheetQuotesTheScreenTests(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-radio-sheet-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    // ------------------------------------------------------------------
    // 4.2 forward.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every sentence the sheet quotes exists in an operator-facing string, and the output says
    /// which kind of check each one got.**
    /// </summary>
    [Fact]
    public void EveryQuotedSentenceExistsInAnOperatorFacingString()
    {
        var quotes = Quotes();
        var said = WhatHamletSays();
        var sources = OperatorFacingSources();

        var weak = new List<string>();

        Assert.NotEmpty(quotes);

        foreach (var quote in quotes)
        {
            var parts = FixedParts(quote);

            // **THE CLOSEST PRODUCED SENTENCE, NOT THE FIRST ONE THAT HOLDS THE PARTS.** Two
            // sentences can share a frame - *Hamlet did not send it: ...* is the opening of both
            // the composer's refusal and the variant gate's - so the label beside a quote names
            // the sentence nearest its own length, which is the one it was written from.
            var produced = said
                .Where(one => Holds(one.Sentence, parts))
                .OrderBy(one => Math.Abs(one.Sentence.Length - quote.Length))
                .FirstOrDefault();

            if (produced.Sentence is not null)
            {
                _output.WriteLine("(a) " + Short(quote) + "   <- " + produced.Label);
                continue;
            }

            var found = sources.FirstOrDefault(one => Holds(one.Text, parts));

            Assert.True(
                found.Name is not null,
                "the sheet quotes a sentence no operator-facing string in the tree holds: " + quote);

            weak.Add(quote);
            _output.WriteLine("(b) " + Short(quote) + "   <- found in " + found.Name);
        }

        _output.WriteLine("");
        _output.WriteLine(
            quotes.Count + " quoted sentences: " + (quotes.Count - weak.Count)
            + " checked against what the application itself produced, " + weak.Count
            + " found in the source.");

        foreach (var one in weak)
        {
            _output.WriteLine("  kind (b): " + Short(one));
        }
    }

    // ------------------------------------------------------------------
    // 4.2 backward.
    // ------------------------------------------------------------------

    /// <summary>
    /// **Every refusal this unit can produce on the PSK31 and Olivia send path is quoted somewhere
    /// on the sheet.**
    /// </summary>
    /// <remarks>
    /// **A REFUSAL THAT EXISTS AND IS NOT ON THE SHEET FAILS HERE.** That is the whole of the
    /// backward direction: the sheet cannot quietly go stale, because a send path that grows a
    /// tenth refusal grows a red here with it.
    /// </remarks>
    [Fact]
    public void EveryReachableRefusalOnTheSendPathIsQuoted()
    {
        var quotes = Quotes().Select(FixedParts).ToList();
        var refusals = WhatHamletSays().Where(one => one.IsRefusal).ToList();

        Assert.NotEmpty(refusals);

        var missing = new List<string>();

        foreach (var refusal in refusals)
        {
            var quoted = quotes.Any(parts => Holds(refusal.Sentence, parts));

            _output.WriteLine((quoted ? "quoted     : " : "NOT QUOTED : ") + refusal.Label);
            _output.WriteLine("             " + Short(refusal.Sentence));

            if (!quoted)
            {
                missing.Add(refusal.Label);
            }
        }

        _output.WriteLine("");
        _output.WriteLine(
            (refusals.Count - missing.Count) + " of " + refusals.Count
            + " reachable refusals are quoted on the sheet.");

        Assert.Empty(missing);
    }

    // ------------------------------------------------------------------
    // 4.3 - R11.
    // ------------------------------------------------------------------

    /// <summary>
    /// **No line of the sheet sends him to the radio, except the sentences Hamlet itself says -
    /// and those are declared, one by one, and proved to be Hamlet's.**
    /// </summary>
    /// <remarks>
    /// <para>**EVERY LINE IS SCANNED, QUOTES INCLUDED** (work instruction 383 section 6 ruling 1
    /// item 1). Unit 382's scan excluded the `&gt; ` lines, and the separate session that judged
    /// unit 382 named that exclusion as the reason 4.3 was not met: the one sentence on the page
    /// that actually tells the operator to operate the transceiver is inside a quote, so a scan
    /// that skips the quotes cannot see the thing the criterion is about. **No line is excluded
    /// here for any reason. One scan, one number, every line.**</para>
    /// <para>**AND THE WORD LIST IS WIDER, BECAUSE THE EXCLUSION WAS ONLY HALF OF IT** (ruling 1
    /// item 2). Unit 383 task 1 measured the eleven words unit 382 used over all 176 lines and
    /// got ZERO hits - *dial* was not on that list at all - so widening the scan to the quotes
    /// and changing nothing else would still have reported nothing. The list here is the union of
    /// those eleven with the two R11 lists already in this tree,
    /// <c>TheOliviaSendTests.cs:390</c> and <c>TheAlcSentenceTests.cs:44-49</c>, so the sheet is
    /// held to the same list Hamlet's own sentences are held to rather than to one written
    /// tonight.</para>
    /// <para>**TWO TIERS, AND ONLY ONE OF THEM IS AN ASSERTION** (ruling 1 item 3).
    /// <see cref="Tier1"/> is the instructions - *move the dial*, *at the radio*, *turn the knob*
    /// - and **a tier-1 phrase anywhere in the sheet's own prose is a red, always, with no
    /// exception of any kind**. <see cref="Tier2"/> is the hardware nouns - *dial*, *radio*,
    /// *rig* - which name the box and its controls and are **not by themselves an instruction**:
    /// *a chip is lit when the dial is inside that mode's block* describes where the receiver is
    /// tuned, and *pick the radio's sound card* names a Windows device. **A tier-2 word is
    /// counted, listed with its line and printed as a number, and is never a red** - the number
    /// is the point, so that nobody has to take this scan on trust.</para>
    /// <para>**A TIER-1 PHRASE INSIDE A QUOTE PASSES ONLY AS A DECLARED EXCEPTION** (ruling 1
    /// item 4), and an exception earns all three of: it is in <see cref="Declared"/>, a closed
    /// list with one entry per sentence carrying the site and one line saying why Hamlet says it;
    /// the sentence is proved here to be one Hamlet itself produces, by kind (a) or kind (b), the
    /// same two kinds this type already uses; and **the sheet's own prose beside it carries no
    /// tier-1 phrase of its own**, which is what stops the exception becoming a place to put an
    /// instruction. **An undeclared quoted hit is a red, and so is a declared entry that is not
    /// on the sheet.**</para>
    /// <para>**WHAT THIS READING DOES NOT CLAIM** (ruling 1 item 6). Criterion 4.3 reads *no
    /// sentence tells the operator to touch the radio*. Under the strictest reading of that, a
    /// declared exception is still a sentence on the page that does - the page quotes it, and a
    /// man reading the page reads it. The remedy would be a change to a sentence Hamlet says,
    /// and one of the two is the sentence it says when its own unkey did not get out, where the
    /// only correct advice is the advice it gives; changing either is the owner's and not a
    /// unit's. **The exception list is therefore the honest limit of what a unit can reach**, and
    /// it is reported as a finding with a number rather than argued away.</para>
    /// </remarks>
    [Fact]
    public void NoLineOfTheSheetSendsHimToTheRadioExceptWhereHamletsOwnWordsDo()
    {
        var sheet = Sheet();
        var numbered = sheet.Select((line, at) => (At: at + 1, Line: line)).ToList();

        var tier1 = Hits(numbered, Tier1);
        var tier2 = Hits(numbered, Tier2);

        _output.WriteLine(
            sheet.Count + " lines scanned, ALL of them - " + sheet.Count(IsQuote)
            + " quote lines and " + sheet.Count(l => !IsQuote(l)) + " prose - against "
            + Tier1.Length + " tier-1 phrases and " + Tier2.Length + " tier-2 words.");
        _output.WriteLine("");

        // ------------------------------------------------------------------
        // Tier 1 in the sheet's own prose: a red, always.
        // ------------------------------------------------------------------
        var inProse = tier1.Where(hit => !IsQuote(hit.Line)).ToList();

        _output.WriteLine("TIER 1 IN THE SHEET'S OWN PROSE: " + inProse.Count + "  (the assertion)");

        foreach (var hit in inProse)
        {
            _output.WriteLine("  line " + hit.At + "  \"" + hit.Word + "\"  " + hit.Line.Trim());
        }

        // ------------------------------------------------------------------
        // Tier 1 inside a quote: declared, proved, and nothing beside it.
        // ------------------------------------------------------------------
        var inQuotes = tier1.Where(hit => IsQuote(hit.Line)).ToList();
        var said = WhatHamletSays();
        var sources = OperatorFacingSources();

        var undeclared = new List<string>();
        var unproved = new List<string>();
        var beside = new List<string>();
        var found = new List<string>();

        _output.WriteLine("");
        _output.WriteLine("TIER 1 INSIDE A QUOTE: " + inQuotes.Count);

        foreach (var hit in inQuotes)
        {
            var quote = Quoted(hit.Line);
            var declared = Declared
                .Where(one => hit.Line.Contains(one.Marker, StringComparison.Ordinal))
                .ToList();

            if (declared.Count != 1)
            {
                _output.WriteLine(
                    "  line " + hit.At + "  \"" + hit.Word + "\"  UNDECLARED - "
                    + declared.Count + " entries in the list match it.");

                undeclared.Add("line " + hit.At + ": " + Short(quote));

                continue;
            }

            var entry = declared[0];

            found.Add(entry.Marker);

            _output.WriteLine("  line " + hit.At + "  \"" + hit.Word + "\"  declared.");
            _output.WriteLine("        " + Short(quote));
            _output.WriteLine("        site : " + entry.Site);
            _output.WriteLine("        why  : " + entry.Why);

            // (a) It is proved to be a sentence Hamlet itself produces.
            var parts = FixedParts(quote);
            var produced = said
                .Where(one => Holds(one.Sentence, parts))
                .OrderBy(one => Math.Abs(one.Sentence.Length - quote.Length))
                .FirstOrDefault();

            if (produced.Sentence is not null)
            {
                _output.WriteLine(
                    "        proof: kind (a), produced by the shipped view model - "
                    + produced.Label);
            }
            else
            {
                var literal = sources.FirstOrDefault(one => Holds(one.Text, parts));

                if (literal.Name is null)
                {
                    _output.WriteLine("        proof: NONE - Hamlet does not say this sentence.");

                    unproved.Add("line " + hit.At + ": " + Short(quote));
                }
                else
                {
                    _output.WriteLine("        proof: kind (b), the literal in " + literal.Name);
                }
            }

            // (c) The sheet's own prose beside it says nothing of its own.
            foreach (var line in Beside(sheet, hit.At))
            {
                foreach (var phrase in Tier1)
                {
                    if (Whole(line, phrase))
                    {
                        beside.Add("beside line " + hit.At + ", \"" + phrase + "\": " + line.Trim());
                    }
                }
            }

            _output.WriteLine(
                "        beside: " + Beside(sheet, hit.At).Count
                + " prose lines, and they carry no tier-1 phrase of their own.");
        }

        // A declared exception that is not on the sheet is dead wood, and the list is closed.
        var missing = Declared
            .Where(one => !found.Contains(one.Marker, StringComparer.Ordinal))
            .Select(one => one.Marker)
            .ToList();

        // ------------------------------------------------------------------
        // Tier 2: counted, listed, printed - never a red.
        // ------------------------------------------------------------------
        _output.WriteLine("");
        _output.WriteLine(
            "TIER 2 MENTIONS OF THE RIG'S HARDWARE: " + tier2.Count + " in all - "
            + tier2.Count(hit => !IsQuote(hit.Line)) + " in the sheet's own prose and "
            + tier2.Count(hit => IsQuote(hit.Line)) + " inside quotes. NONE OF THEM IS A RED.");

        foreach (var hit in tier2)
        {
            _output.WriteLine(
                "  line " + hit.At.ToString(CultureInfo.InvariantCulture).PadLeft(3) + "  "
                + (IsQuote(hit.Line) ? "quote" : "prose") + "  \"" + hit.Word + "\"   "
                + Short(hit.Line.Trim()));
        }

        _output.WriteLine("");
        _output.WriteLine(
            "This page mentions the radio's own hardware " + tier2.Count
            + " times and instructs the operator to touch it " + inProse.Count
            + " times in its own voice.");

        Assert.Empty(inProse);
        Assert.Empty(undeclared);
        Assert.Empty(unproved);
        Assert.Empty(beside);
        Assert.Empty(missing);
    }

    // ------------------------------------------------------------------
    // One page - reported, never a red.
    // ------------------------------------------------------------------

    /// <summary>
    /// **What the sheet measures, against ruling 2 item 2's target.**
    /// </summary>
    /// <remarks>
    /// **THE NUMBER IS REPORTED AND IS NOT AN ASSERTION ABOUT THE TARGET** (ruling 2 item 2:
    /// 4.1's content is the criterion and the length is a target). A length assertion that failed
    /// would be answered by dropping a refusal or paraphrasing a quote, and both are forbidden -
    /// so the only thing a red here could buy is the damage it exists to prevent. What IS asserted
    /// is that the file is there and has quotes in it.
    /// </remarks>
    [Fact]
    public void TheSheetIsOnePageAndTheMeasurementIsReported()
    {
        var lines = Sheet();
        var bytes = new FileInfo(Path.Combine(Root(), "docs", "RADIO_SHEET.md")).Length;
        var quotes = Quotes();
        var quoteBytes = lines
            .Where(l => l.StartsWith("> ", StringComparison.Ordinal))
            .Sum(l => l.Length + 1);

        _output.WriteLine("lines : " + lines.Count + "  against a target of " + TargetLines);
        _output.WriteLine("bytes : " + bytes + "  against a target of " + TargetBytes);
        _output.WriteLine("quoted sentences : " + quotes.Count);
        _output.WriteLine(
            "of the bytes, " + quoteBytes + " are the quote lines, which the sheet's own "
            + "convention forbids wrapping.");
        _output.WriteLine(
            lines.Count <= TargetLines && bytes <= TargetBytes
                ? "inside the target on both."
                : "over the target, and the content is the criterion (ruling 2 item 2).");

        Assert.NotEmpty(quotes);
    }

    // ------------------------------------------------------------------
    // The sheet.
    // ------------------------------------------------------------------

    /// <summary>One declared exception: a tier-1 phrase the sheet may carry inside a quote.</summary>
    /// <param name="Marker">Enough of the sentence to name it, and nothing that varies.</param>
    /// <param name="Site">Where under `src/Hamlet.App` Hamlet says it.</param>
    /// <param name="Why">One line: why Hamlet says it, in the register the sheet uses.</param>
    private readonly record struct DeclaredSentence(string Marker, string Site, string Why);

    /// <summary>Every line of the sheet.</summary>
    private static List<string> Sheet()
        => File.ReadAllLines(Path.Combine(Root(), "docs", "RADIO_SHEET.md")).ToList();

    /// <summary>Whether a line is one of the sheet's quotes.</summary>
    private static bool IsQuote(string line)
        => line.StartsWith("> ", StringComparison.Ordinal);

    /// <summary>What a quote line says, between its first and its last double quote.</summary>
    private static string Quoted(string line)
    {
        var opens = line.IndexOf('"');
        var shuts = line.LastIndexOf('"');

        return opens >= 0 && shuts > opens ? line[(opens + 1)..shuts] : line[2..];
    }

    /// <summary>Whether a line carries a phrase as whole words, in any case.</summary>
    /// <remarks>
    /// **WHOLE WORDS, BECAUSE A SUBSTRING SCAN ANSWERS A DIFFERENT QUESTION.** Unit 382 measured
    /// that a substring scan matches `rig` inside `Right-click` and inside `right now`, and a
    /// scan that counted those would push the sheet into paraphrasing English to pass a test.
    /// </remarks>
    private static bool Whole(string line, string phrase)
        => Regex.IsMatch(line, "\\b" + Regex.Escape(phrase) + "\\b", RegexOptions.IgnoreCase);

    /// <summary>Every whole-word hit of any of the phrases, over the lines handed in.</summary>
    private static List<(int At, string Line, string Word)> Hits(
        List<(int At, string Line)> lines, string[] phrases)
    {
        var hits = new List<(int At, string Line, string Word)>();

        foreach (var (at, line) in lines)
        {
            foreach (var phrase in phrases)
            {
                if (Whole(line, phrase))
                {
                    hits.Add((at, line, phrase));
                }
            }
        }

        return hits;
    }

    /// <summary>
    /// **The sheet's own prose beside a quote: what it means and what to do, for that row.**
    /// </summary>
    /// <param name="sheet">Every line.</param>
    /// <param name="at">The quote's line number, counting from one.</param>
    /// <returns>The prose lines that touch the quote, above it and below it.</returns>
    /// <remarks>
    /// **THE PARAGRAPH THAT TOUCHES IT AND NOT THE WHOLE SECTION.** The sheet writes a refusal as
    /// a paragraph of its own - what it means, then what to do - and then the quote, so the prose
    /// beside a quote is the run of non-blank, non-quote lines immediately above it and
    /// immediately below it. That is the text a man reads in the same breath as the sentence, and
    /// it is the text ruling 1 item 4(c) says may add no instruction of its own.
    /// </remarks>
    private static List<string> Beside(List<string> sheet, int at)
    {
        var beside = new List<string>();

        for (var up = at - 2; up >= 0; up--)
        {
            if (sheet[up].Trim().Length == 0 || IsQuote(sheet[up]))
            {
                break;
            }

            beside.Add(sheet[up]);
        }

        for (var down = at; down < sheet.Count; down++)
        {
            if (sheet[down].Trim().Length == 0 || IsQuote(sheet[down]))
            {
                break;
            }

            beside.Add(sheet[down]);
        }

        return beside;
    }

    /// <summary>
    /// **Every sentence the sheet quotes: the text between the first and the last double quote on
    /// a `&gt;` line.**
    /// </summary>
    /// <remarks>
    /// **FIRST AND LAST, BECAUSE A QUOTED SENTENCE CAN HOLD QUOTES ITSELF.** *Sending "&lt;your
    /// line&gt;" now* is what the screen says, and the sheet quotes it as it is said. The
    /// convention that nothing follows the closing quote is what makes the last one unambiguous.
    /// </remarks>
    private static List<string> Quotes()
    {
        var quotes = new List<string>();

        foreach (var line in Sheet().Where(l => l.StartsWith("> ", StringComparison.Ordinal)))
        {
            var opens = line.IndexOf('"');
            var shuts = line.LastIndexOf('"');

            Assert.True(
                opens >= 0 && shuts > opens,
                "a quote line is not \"sentence in double quotes\" and nothing else: " + line);

            quotes.Add(line[(opens + 1)..shuts]);
        }

        return quotes;
    }

    /// <summary>The fixed parts of a quote, in order, with every spliced value taken out.</summary>
    private static List<string> FixedParts(string quote)
        => Spliced.Split(quote)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToList();

    /// <summary>Whether a string holds every fixed part, in the sheet's own order.</summary>
    private static bool Holds(string sentence, IReadOnlyList<string> parts)
    {
        var at = 0;

        foreach (var part in parts)
        {
            var found = sentence.IndexOf(part, at, StringComparison.Ordinal);

            if (found < 0)
            {
                return false;
            }

            at = found + part.Length;
        }

        return parts.Count > 0;
    }

    private static string Short(string sentence)
        => sentence.Length <= 88 ? sentence : sentence[..85] + "...";

    // ------------------------------------------------------------------
    // Kind (a) - what the application itself produced.
    // ------------------------------------------------------------------

    /// <summary>One operator-facing string, and where it came from.</summary>
    /// <param name="Label">What was pressed, and in which state.</param>
    /// <param name="Sentence">What Hamlet put on the screen.</param>
    /// <param name="IsRefusal">True where it is a refusal on the PSK31 or Olivia send path.</param>
    private readonly record struct Said(string Label, string Sentence, bool IsRefusal);

    /// <summary>
    /// **Everything this test can make Hamlet say, driven through the shipped view model.**
    /// </summary>
    private List<Said> WhatHamletSays()
    {
        var said = new List<Said>();

        // The idle screen, which needs no press at all.
        var idle = Panel(null, "PSK31");

        said.Add(new("PSK31 chosen, the strip line", idle.DigitalModeStripLine, false));
        said.Add(new("PSK31 chosen, the decoded panel", idle.DigitalDecodedIdle, false));
        said.Add(new("nothing sent yet", idle.DigitalSendLine, false));
        said.Add(new("the send line's hover", MainWindowViewModel.SendIdleTip, false));
        said.Add(new("the stop, nothing armed", idle.StopLabel, false));

        var olivia = Panel(null, "Olivia");

        said.Add(new("Olivia chosen, the strip line", olivia.DigitalModeStripLine, false));
        said.Add(new("Olivia chosen, the decoded panel", olivia.DigitalDecodedIdle, false));

        // The capture press, which keys nothing and writes nothing where nothing arrived.
        var capture = Panel(null, "PSK31");

        capture.CapturePsk31Command.Execute(null);

        said.Add(new("the capture press, running", capture.Psk31CaptureLine, false));

        capture.CapturePsk31Command.Execute(null);

        said.Add(new("the capture press, nothing heard", capture.Psk31CaptureWhere, false));
        said.Add(new("the capture press, at rest", capture.Psk31CaptureLine, false));

        // Every refusal the send path can reach from a fixture.
        said.Add(Refusal(
            "nothing to send",
            model => model.SendMessageCommand.Execute("")));

        said.Add(Refusal(
            "no transmit path",
            model => model.SendCallToAnyoneCommand.Execute(null),
            arm: false));

        said.Add(Refusal(
            "no clear spot",
            model =>
            {
                model.UsePsk31CandidatesForTests(CrowdedBand());
                model.SendCallToAnyoneCommand.Execute(null);
            }));

        said.Add(Refusal(
            "longer than the cap",
            model => model.SendMessageCommand.Execute(TooLongForTheCap())));

        said.Add(Refusal(
            "Olivia cannot announce itself",
            model =>
            {
                model.UseOliviaDataForTests(OliviaData.Read(null, "{ }", null));
                model.SendCallToAnyoneCommand.Execute(null);
            },
            olivia: true));

        said.Add(Refusal(
            "the Olivia variant is not proved",
            model =>
            {
                var format = File.ReadAllText(Path.Combine(Root(), "data", "olivia", "format.json"));

                model.UseOliviaDataForTests(
                    OliviaData.Read(
                        File.ReadAllText(Path.Combine(Root(), "data", "bands", "olivia-calling.json")),
                        File.ReadAllText(Path.Combine(Root(), "assets", "data", "rsid-codes.json")),
                        format.Replace(", \"proved_by_loopback\": true", "", StringComparison.Ordinal)));

                model.SendCallToAnyoneCommand.Execute(null);
            },
            olivia: true));

        said.Add(Refusal(
            "the text holds what the mode cannot carry",
            model => model.SendMessageCommand.Execute("CQ — DE " + Mine),
            olivia: true));

        said.Add(Refusal(
            "the chosen mode cannot send at all",
            model => model.SendCallToAnyoneCommand.Execute(null),
            mode: "WSPR",
            arm: false));

        // The two the transmit path itself refuses with, which the keyboard modes reach too.
        var (line, sentence) = DeviceRefusal(named: false, throws: null);

        said.Add(new("no transmit device chosen, the send line", line, true));
        said.Add(new("no transmit device chosen, the refusal", sentence, true));

        var (openLine, openSentence) = DeviceRefusal(
            named: true,
            throws: new InvalidOperationException("the device is in use by another application"));

        said.Add(new("the device would not open, the send line", openLine, true));
        said.Add(new("the device would not open, the refusal", openSentence, true));

        // **THE STOP WHOSE TWO FRAMES GOT NOWHERE**, which is the sentence safety turns on and
        // the sheet's second declared exception (work instruction 383 section 6 ruling 1 item 5).
        // **It is not a refusal**: nothing refused a send, and the backward direction above is
        // about the send path's own refusals.
        said.Add(new("the stop, neither frame out", TheStopThatReachedNothing(), false));

        // The send that goes, which is the pair of sentences the walk in sections 2 and 3 ends on.
        said.AddRange(ASendThatGoes("PSK31"));
        said.AddRange(ASendThatGoes("Olivia"));

        return said;
    }

    /// <summary>Drive one refusal and keep the sentence Hamlet put on the panel.</summary>
    private Said Refusal(
        string label,
        Action<MainWindowViewModel> press,
        bool olivia = false,
        bool arm = true,
        string? mode = null)
    {
        using var telemetry = new JsonlTelemetry(_folder, "382", _ => true);

        var model = Panel(telemetry, mode ?? (olivia ? "Olivia" : "PSK31"));

        if (arm)
        {
            GiveItARadio(model, telemetry);
        }

        press(model);
        Settle(model);

        return new(label, model.DigitalSendLine, true);
    }

    /// <summary>
    /// **The stop press over a wire that takes nothing: what the operator is left looking at.**
    /// </summary>
    /// <remarks>
    /// **NOTHING IS ARMED, NOTHING IS COMPOSED AND NOTHING IS KEYED** (§0.2, FACT-004). The abort
    /// is fired at <see cref="MuteWire"/>, which refuses both frames, and that is precisely the
    /// state `MainWindowViewModel.cs:19587` exists to describe - the one moment where Hamlet has
    /// nothing left to try and says so.
    /// </remarks>
    private string TheStopThatReachedNothing()
    {
        using var telemetry = new JsonlTelemetry(_folder, "383", _ => true);

        var model = Panel(telemetry, "PSK31");
        var wire = new MuteWire();
        var sink = new FakeSink();

        model.UseRigPortForTests(wire);
        model.UseArmedSendForTests(
            new Ft8ArmedSend(new Ft8TransmitSequence(wire, sink, guard: null, telemetry)));

        model.StopSendingCommand.Execute(null);
        Settle(model);

        Assert.Equal(0, sink.TimesCalled);
        Assert.Empty(wire.Written);

        return model.DigitalSendLine;
    }

    /// <summary>Drive one of the two transmit-path refusals, and keep both of its sentences.</summary>
    private (string Line, string Sentence) DeviceRefusal(bool named, Exception? throws)
    {
        using var telemetry = new JsonlTelemetry(_folder, "382", _ => true);

        var model = Panel(telemetry, "PSK31", named ? "USB Audio CODEC" : null);

        model.TransmitSinkFactory = _ => throw (throws
            ?? new InvalidOperationException("no sink was asked for"));

        var port = new FakePort();

        model.UseRigPortForTests(port);
        model.BuildTheArmedSend(port);

        model.SendCallToAnyoneCommand.Execute(null);
        Settle(model);

        Assert.Empty(port.Written);

        return (model.DigitalSendLine, model.TransmitRefusalSentence);
    }

    /// <summary>A call to anyone that goes, over fakes: what the panel says at the press and after.</summary>
    private List<Said> ASendThatGoes(string mode)
    {
        using var telemetry = new JsonlTelemetry(_folder, "382", _ => true);

        var model = Panel(telemetry, mode);

        GiveItARadio(model, telemetry);

        model.SendCallToAnyoneCommand.Execute(null);

        var going = model.DigitalSendLine;

        Settle(model);

        return
        [
            new(mode + ": at the press", going, false),
            new(mode + ": when it has gone", model.DigitalSendLine, false),
        ];
    }

    // ------------------------------------------------------------------
    // Kind (b) - the literal in the source.
    // ------------------------------------------------------------------

    /// <summary>One file that can hold an operator-facing literal.</summary>
    /// <param name="Name">Its path under the repository root.</param>
    /// <param name="Text">Its text with the C# concatenation glue taken out.</param>
    private readonly record struct Source(string Name, string Text);

    /// <summary>
    /// **Every file under `src/Hamlet.App` that could hold a sentence, with adjacent literals
    /// joined so a sentence written in three pieces is found whole.**
    /// </summary>
    /// <remarks>
    /// **THAT JOIN IS WHY KIND (b) WORKS AT ALL FOR `NoTransmitDeviceSentence`**, which is one
    /// sentence in three constants (ruling 1 item 4's own example). Removing the `" + "` between
    /// two literals is what a reader does with their eye; doing it here lets the parts be found in
    /// order, in one file, exactly as the ruling asks.
    /// </remarks>
    private static List<Source> OperatorFacingSources()
    {
        var root = Path.Combine(Root(), "src", "Hamlet.App");
        var sources = new List<Source>();

        foreach (var file in Directory
                     .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                     .Where(f => f.EndsWith(".cs", StringComparison.Ordinal)
                                 || f.EndsWith(".axaml", StringComparison.Ordinal))
                     .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                                 && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)))
        {
            var text = File.ReadAllText(file);

            sources.Add(new(
                Path.GetRelativePath(Root(), file),
                Concatenation.Replace(text, "").Replace("\\\"", "\"", StringComparison.Ordinal)));
        }

        return sources;
    }

    // ------------------------------------------------------------------
    // The fixtures.
    // ------------------------------------------------------------------

    /// <summary>A panel on 20 m with one of the five sub-modes pressed.</summary>
    private static MainWindowViewModel Panel(JsonlTelemetry? telemetry, string mode, string? device = null)
    {
        var settings = new AppSettings { ReconnectOnStartup = false, AudioOutputDeviceId = device };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN42";
        settings.Operator.OperatorName = "Pat";
        settings.Operator.Location = "Boston MA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= On20m && b.Band.HighHz >= On20m);
        model.FrequencyHz = On20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>Give the panel the one send path, over fakes. Nothing opens and nothing keys.</summary>
    private static void GiveItARadio(MainWindowViewModel model, JsonlTelemetry telemetry)
    {
        var port = new FakePort();
        var sink = new FakeSink();

        model.UseRigPortForTests(port);
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(port, sink, guard: null, telemetry)));
    }

    /// <summary>A band with no gap over 150 Hz anywhere a call could go.</summary>
    private static List<Psk31Candidate> CrowdedBand()
    {
        var crowded = new List<Psk31Candidate>();

        for (var hz = Psk31ClearSpot.LowestCallHz - 100; hz <= Psk31ClearSpot.HighestCallHz + 100; hz += 100)
        {
            crowded.Add(new Psk31Candidate(hz, 12, Psk31ClearSpot.Occupied + 0.2, true));
        }

        return crowded;
    }

    /// <summary>More PSK31 audio than a macro's cap allows, in Latin-1 the varicode carries.</summary>
    private static string TooLongForTheCap()
        => string.Concat(Enumerable.Repeat("CQ CQ CQ DE " + Mine + " " + Mine + " PSE K ", 40));

    /// <summary>Let a send that the click fired finish, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 400 && model.HasSomethingToStop; tries++)
        {
            Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    /// <summary>The repository root, by the walk `DecisionLogOrderTests.cs:49` already uses.</summary>
    private static string Root()
    {
        var here = new DirectoryInfo(AppContext.BaseDirectory);

        while (here is not null && !File.Exists(Path.Combine(here.FullName, "CLAUDE.md")))
        {
            here = here.Parent;
        }

        Assert.NotNull(here);

        return here!.FullName;
    }
}
