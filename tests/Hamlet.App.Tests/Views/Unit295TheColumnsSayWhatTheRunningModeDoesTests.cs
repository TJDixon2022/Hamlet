using System.Globalization;
using System.Text.RegularExpressions;
using Ft8Sharp.Encode;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Work instruction 295 task 2 - the decode columns say what the running mode
/// does, and every figure on them was measured on the mode it is attributed to.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS CATCHES, AND IT IS NOT HYPOTHETICAL: a tooltip
/// asserting a measurement taken on a different mode.** That is what was in the
/// tree from unit 292, when the Digital tab gained FT4, until this unit. The
/// <c>snr</c> header told an operator hovering an FT4 row that the figure agreed
/// with the delivered ratio to 0.26 dB over 510 messages - **which is FT8's
/// number, measured on FT8's eight tones** - and the <c>utc</c> header told him
/// the grid was fifteen seconds while the tab was cutting 7.5 s slots. Three
/// units passed over it because nothing here read the strings.</para>
/// <para>**IT COMPARES THE STRINGS AGAINST THE TREE AND NOT AGAINST ITSELF.** The
/// slot lengths come from <see cref="SlotGrid"/>, the tone counts from the port's
/// own symbol encoders, and the two precision pairs from the files that record
/// them - <c>docs/unit251-snr-trace.md</c> for FT8 and <c>Ft8Reception.cs</c>'s
/// <c>ReadFt4</c> remarks for FT4. A test carrying its own copy of six figures
/// would pass forever while the screen and the measurement drifted apart, which
/// is the fault it exists to prevent.</para>
/// <para>**AND IT CHECKS THE ATTRIBUTION, NOT ONLY THE PRESENCE.** A string
/// carrying all four numbers with FT8's beside the word FT4 would satisfy a
/// contains-check and would be exactly the old fault wearing new figures, so each
/// pair is required in the clause that names its own mode.</para>
/// <para>**NO TRANSMISSION LENGTH IS ALLOWED ON ANY OF THEM.** The
/// 4.48-against-5.04 occupancy is the owner's open question and a tooltip stating
/// either figure would be answering it. A slot length is settled and is fine.</para>
/// </remarks>
public sealed class Unit295TheColumnsSayWhatTheRunningModeDoesTests(ITestOutputHelper output)
{
    /// <summary>The three strings this unit rewrote.</summary>
    private static readonly string[] Rewritten =
        ["HmDecodeUtcHelp", "HmDecodeSnrHelp", "HmDecodeContactHelp"];

    /// <summary>
    /// **The `utc` header says what the grid is on each mode, and never fifteen
    /// seconds flat.**
    /// </summary>
    [Fact]
    public void TheUtcColumnNamesBothGridsAndAssertsNeitherAsTheGrid()
    {
        var said = Tooltip("HmDecodeUtcHelp");

        output.WriteLine("HmDecodeUtcHelp");
        output.WriteLine("  " + said);
        output.WriteLine("  SlotGrid.Ft8.SlotSeconds = " + SlotGrid.Ft8.SlotSeconds);
        output.WriteLine("  SlotGrid.Ft4.SlotSeconds = " + SlotGrid.Ft4.SlotSeconds);

        // BOTH MODES ARE NAMED, so the sentence is true whichever one is running.
        Assert.Contains("FT8", said, StringComparison.Ordinal);
        Assert.Contains("FT4", said, StringComparison.Ordinal);

        // AND EACH CARRIES ITS OWN GRID, taken from the code that cuts the slots.
        Assert.Contains(Spelled(SlotGrid.Ft8.SlotSeconds), said, StringComparison.Ordinal);
        Assert.Contains(Spelled(SlotGrid.Ft4.SlotSeconds), said, StringComparison.Ordinal);

        // **THE CLAUSE THAT WAS FALSE.** "FT8 runs on a strict fifteen-second grid"
        // was the whole sentence's subject, so an FT4 row was told the wrong mode
        // and the wrong number in one breath.
        Assert.DoesNotContain("strict fifteen-second grid", said, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The `snr` header carries both measured precisions, each attributed to the
    /// mode it was measured on, and each equal to what the tree records.**
    /// </summary>
    [Fact]
    public void TheSnrColumnCarriesEachModesOwnMeasuredPrecision()
    {
        var said = Tooltip("HmDecodeSnrHelp");
        var ft8 = RecordedForFt8();
        var ft4 = RecordedForFt4();

        output.WriteLine("HmDecodeSnrHelp");
        output.WriteLine("  " + said);
        output.WriteLine(string.Empty);
        output.WriteLine("recorded in the tree:");
        output.WriteLine($"  FT8  {ft8.Count} messages, {ft8.Mae:F2} dB mean, "
            + $"{ft8.P95:F2} dB at the 95th   (docs/unit251-snr-trace.md section 6)");
        output.WriteLine($"  FT4  {ft4.Count} messages, {ft4.Mae:F2} dB mean, "
            + $"{ft4.P95:F2} dB at the 95th   (Ft8Reception.cs, ReadFt4)");
        output.WriteLine(string.Empty);
        output.WriteLine("tone counts from the port:");
        output.WriteLine("  FT8 " + Ft8SymbolEncoder.ToneCount + " tones, "
            + (Ft8SymbolEncoder.ToneCount - 1) + " that were not sent");
        output.WriteLine("  FT4 " + Ft4SymbolEncoder.ToneCount + " tones, "
            + (Ft4SymbolEncoder.ToneCount - 1) + " that were not sent");

        // ---- THE TONE COUNTS, FROM THE ENCODERS ------------------------------
        // The old string said "the seven tones that were not" unconditionally. On
        // an FT4 row it is three, because FT4 carries two bits a symbol and FT8
        // carries three.
        Assert.Contains(
            Spelled(Ft8SymbolEncoder.ToneCount - 1) + " of them on FT8",
            said,
            StringComparison.Ordinal);
        Assert.Contains(
            Spelled(Ft4SymbolEncoder.ToneCount - 1) + " on FT4",
            said,
            StringComparison.Ordinal);

        // ---- THE PRECISIONS, EACH IN THE CLAUSE THAT NAMES ITS MODE ----------
        var ft8Clause = ClauseNaming(said, "FT8 messages");
        var ft4Clause = ClauseNaming(said, "FT4 messages");

        output.WriteLine(string.Empty);
        output.WriteLine("the FT8 clause: " + ft8Clause);
        output.WriteLine("the FT4 clause: " + ft4Clause);

        AssertCarries(ft8Clause, ft8, "FT8");
        AssertCarries(ft4Clause, ft4, "FT4");

        // **AND NEITHER CLAUSE CARRIES THE OTHER'S NUMBERS.** This is the assertion
        // the whole file is for: a string with all four figures in it passes a
        // contains-check while telling an FT4 operator FT8's precision.
        Assert.DoesNotContain(Figure(ft8.Mae), ft4Clause, StringComparison.Ordinal);
        Assert.DoesNotContain(Figure(ft8.P95), ft4Clause, StringComparison.Ordinal);
        Assert.DoesNotContain(Figure(ft4.Mae), ft8Clause, StringComparison.Ordinal);
        Assert.DoesNotContain(Figure(ft4.P95), ft8Clause, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The contact column, and the comment above it, stop calling a slot fifteen
    /// seconds.**
    /// </summary>
    /// <remarks>
    /// **THIS STRING IS BOUND TO NOTHING AND IS CORRECTED ANYWAY.**
    /// <c>Unit295ReportLegTraceTests.TheThreeTooltipsAsTheTreeHoldsThem</c> measured
    /// that no view references it. A false sentence in the tree is false whether or
    /// not a screen is showing it, and no binding was added: putting a new tooltip on
    /// screen is a different decision.
    /// </remarks>
    [Fact]
    public void TheContactColumnAndItsCommentSayWhatASlotIsOnEachMode()
    {
        var said = Tooltip("HmDecodeContactHelp");
        var comment = CommentAbove("HmDecodeContactHelp");

        output.WriteLine("HmDecodeContactHelp");
        output.WriteLine("  " + said);
        output.WriteLine(string.Empty);
        output.WriteLine("the comment above it:");
        output.WriteLine(comment);

        Assert.Contains("FT8", said, StringComparison.Ordinal);
        Assert.Contains("FT4", said, StringComparison.Ordinal);
        Assert.Contains(Spelled(SlotGrid.Ft8.SlotSeconds), said, StringComparison.Ordinal);
        Assert.Contains(Spelled(SlotGrid.Ft4.SlotSeconds), said, StringComparison.Ordinal);

        // **THE COMMENT IS PART OF THE ASSERTION.** It read "a slot being fifteen
        // seconds", flat, and a comment that contradicts the string beneath it is
        // how the string gets edited back to the wrong thing.
        Assert.DoesNotContain("a slot being fifteen seconds", comment, StringComparison.Ordinal);
        Assert.Contains("FT4", comment, StringComparison.Ordinal);
    }

    /// <summary>
    /// **No rewritten string puts a transmission length on a screen.**
    /// </summary>
    /// <remarks>
    /// The 4.48-against-5.04 occupancy is with the owner and is isolated at
    /// <c>src/Ft8Sharp/Ft4Timing.cs</c> so that a ruling costs one edit. A tooltip
    /// quoting either figure would answer his question on his behalf, on a screen,
    /// which is the one thing this unit was told not to do.
    /// </remarks>
    [Fact]
    public void NoRewrittenStringPutsATransmissionLengthOnAScreen()
    {
        foreach (var key in Rewritten)
        {
            var said = Tooltip(key);

            output.WriteLine(key + ": " + said.Length + " characters");

            Assert.DoesNotContain("4.48", said, StringComparison.Ordinal);
            Assert.DoesNotContain("5.04", said, StringComparison.Ordinal);
            Assert.DoesNotContain(
                Figure(SlotGrid.Ft4.TransmissionSeconds), said, StringComparison.Ordinal);
            Assert.DoesNotContain(
                Figure(SlotGrid.Ft8.TransmissionSeconds), said, StringComparison.Ordinal);
        }
    }

    // -------------------------------------------------------------------------
    // What the tree records.
    // -------------------------------------------------------------------------

    /// <summary>One mode's agreement figures as some file in the tree holds them.</summary>
    private readonly record struct Agreement(int Count, double Mae, double P95);

    /// <summary>
    /// FT8's, from <c>docs/unit251-snr-trace.md</c> section 6's summary row.
    /// </summary>
    /// <remarks>
    /// **THE TABLE'S OWN `BOTH all` LINE**, which is the row the tooltip has quoted
    /// since unit 251: placement, rung, trials, decoded, measured, MAE, p95, bias.
    /// </remarks>
    private static Agreement RecordedForFt8()
    {
        var text = File.ReadAllText(Path.Combine(Root(), "docs", "unit251-snr-trace.md"));
        var row = Regex.Match(
            text,
            @"BOTH\s+all\s+(?<trials>\d+)\s+\d+\s+\d+\s+(?<mae>[\d.]+)\s+(?<p95>[\d.]+)",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        Assert.True(
            row.Success,
            "docs/unit251-snr-trace.md no longer carries a `BOTH all` summary row, so "
            + "the FT8 figures on the snr tooltip cannot be checked against anything.");

        return new Agreement(
            int.Parse(row.Groups["trials"].Value, CultureInfo.InvariantCulture),
            double.Parse(row.Groups["mae"].Value, CultureInfo.InvariantCulture),
            double.Parse(row.Groups["p95"].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// FT4's, from <c>Ft8Reception.ReadFt4</c>'s own remarks, which is where unit 294
    /// wrote them down beside the code that produces the number.
    /// </summary>
    private static Agreement RecordedForFt4()
    {
        var text = File.ReadAllText(Path.Combine(
            Root(), "src", "Hamlet.RadioEngine", "Audio", "Ft8Reception.cs"));

        var count = Regex.Match(
            text,
            @"(?<count>\d+) synthesized messages at five rungs FT4 decodes at",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        var figures = Regex.Match(
            text,
            @"is \*\*(?<mae>[\d.]+) dB and the 95th percentile (?<p95>[\d.]+) dB\*\*",
            RegexOptions.None,
            TimeSpan.FromSeconds(5));

        Assert.True(
            count.Success && figures.Success,
            "Ft8Reception.ReadFt4 no longer records the FT4 estimator's measured error, "
            + "so the FT4 figures on the snr tooltip cannot be checked against anything.");

        return new Agreement(
            int.Parse(count.Groups["count"].Value, CultureInfo.InvariantCulture),
            double.Parse(figures.Groups["mae"].Value, CultureInfo.InvariantCulture),
            double.Parse(figures.Groups["p95"].Value, CultureInfo.InvariantCulture));
    }

    /// <summary>That one clause carries one mode's count and both its figures.</summary>
    private static void AssertCarries(string clause, Agreement recorded, string mode)
    {
        Assert.Contains(
            recorded.Count.ToString(CultureInfo.InvariantCulture),
            clause,
            StringComparison.Ordinal);

        Assert.True(
            clause.Contains(Figure(recorded.Mae), StringComparison.Ordinal),
            $"the {mode} clause of the snr tooltip does not carry {Figure(recorded.Mae)}, "
            + $"which is the mean absolute error the tree records for {mode}. It reads: {clause}");

        Assert.True(
            clause.Contains(Figure(recorded.P95), StringComparison.Ordinal),
            $"the {mode} clause of the snr tooltip does not carry {Figure(recorded.P95)}, "
            + $"which is the 95th percentile the tree records for {mode}. It reads: {clause}");
    }

    // -------------------------------------------------------------------------
    // Reading the strings out of App.axaml.
    // -------------------------------------------------------------------------

    /// <summary>One tooltip as the application's own resources hold it.</summary>
    private static string Tooltip(string key)
    {
        var line = Array.Find(
            AppAxaml(), l => l.Contains("x:Key=\"" + key + "\"", StringComparison.Ordinal));

        Assert.True(line is not null, key + " is no longer in src/Hamlet.App/App.axaml");

        var from = line!.IndexOf('>', line.IndexOf("x:Key", StringComparison.Ordinal)) + 1;
        var to = line.LastIndexOf("</x:String>", StringComparison.Ordinal);

        Assert.True(to > from, key + " is not a one-line x:String and cannot be read here");

        return line[from..to];
    }

    /// <summary>The XML comment immediately above one resource, if there is one.</summary>
    private static string CommentAbove(string key)
    {
        var lines = AppAxaml();
        var at = Array.FindIndex(
            lines, l => l.Contains("x:Key=\"" + key + "\"", StringComparison.Ordinal));

        Assert.True(at > 0, key + " is not in src/Hamlet.App/App.axaml");

        var end = at - 1;

        if (!lines[end].Contains("-->", StringComparison.Ordinal))
        {
            return "";
        }

        var start = end;

        while (start > 0 && !lines[start].Contains("<!--", StringComparison.Ordinal))
        {
            start--;
        }

        return string.Join(Environment.NewLine, lines[start..(end + 1)]);
    }

    private static string[]? _appAxaml;

    private static string[] AppAxaml() => _appAxaml ??= File.ReadAllLines(
        Path.Combine(Root(), "src", "Hamlet.App", "App.axaml"));

    /// <summary>The clause of a sentence that names one mode, up to the next break.</summary>
    private static string ClauseNaming(string said, string phrase)
    {
        var at = said.IndexOf(phrase, StringComparison.Ordinal);

        Assert.True(at >= 0, "the snr tooltip has no clause naming \"" + phrase + "\"");

        var from = 0;

        for (var i = at; i >= 0; i--)
        {
            if (IsBreak(said, i))
            {
                from = i + 1;
                break;
            }
        }

        var to = said.Length;

        for (var i = at + phrase.Length; i < said.Length; i++)
        {
            if (IsBreak(said, i))
            {
                to = i;
                break;
            }
        }

        return said[from..to].Trim();
    }

    /// <summary>
    /// Whether one character ends a clause.
    /// </summary>
    /// <remarks>
    /// **A FULL STOP INSIDE A DECIMAL IS NOT A BREAK.** Every figure this file
    /// checks carries one - 0.26, 1.41 - and splitting on the bare character cut
    /// each clause off at "delivered to 0", which passed the mode check and then
    /// failed the figure check for a reason that had nothing to do with the screen.
    /// </remarks>
    private static bool IsBreak(string said, int at)
        => said[at] == ';'
            || (said[at] == '.'
                && (at + 1 >= said.Length || !char.IsDigit(said[at + 1])));

    /// <summary>A decibel figure as the screen writes it.</summary>
    private static string Figure(double value)
        => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>A whole number as English spells it, and a fraction as digits.</summary>
    /// <remarks>
    /// The tooltips spell small whole numbers - *fifteen seconds*, *seven of them* -
    /// which is what the surrounding prose does, and write 7.5 as digits because
    /// there is no readable spelling of it.
    /// </remarks>
    private static string Spelled(double value) => value switch
    {
        3 => "three",
        7 => "seven",
        15 => "fifteen",
        _ => Figure(value),
    };

    /// <summary>The repository root, found by walking up to the solution.</summary>
    private static string Root()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Hamlet.sln")))
        {
            dir = dir.Parent;
        }

        Assert.True(dir is not null, "no Hamlet.sln above " + AppContext.BaseDirectory);

        return dir!.FullName;
    }
}
