using System.Globalization;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The Digital tab's decoded table shows what was decoded, and nothing else.
/// </summary>
/// <remarks>
/// <para>**WORK INSTRUCTION 037 BUILT THAT TABLE OUT OF FOUR LITERAL STRINGS AND
/// SAID SO IN ITS OWN MARKUP**: nothing decoded, nothing moved, nothing was
/// wired, and it existed so the operator could argue with a finished-looking FT8
/// session before there was a decoder to argue with. There is one now.</para>
/// <para>**BOTH HALVES ARE ASSERTED IN ONE FILE ON PURPOSE.** That the real rows
/// arrive, and that the invented ones are gone. Either one alone can pass while
/// the tab shows a mixture, which is worse than showing neither.</para>
/// <para>**NOTHING HERE OPENS A WINDOW OR TOUCHES A SOUND CARD.** A recording is
/// built in an array and handed to the view model, and the markup is read as
/// text.</para>
/// </remarks>
public sealed class TheDecodedTableIsRealTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rows are printed.</param>
    public TheDecodedTableIsRealTests(ITestOutputHelper output) => _output = output;

    /// <summary>The four messages the markup used to assert had been heard.</summary>
    private static readonly string[] Invented =
    {
        "W9XYZ K1ABC -13",
        "CQ K1ABC FN42",
        "CQ DX EA3QQ JN11",
        "VE7AA N0RR RR73",
    };

    /// <summary>A clock that has been checked and found to match UTC.</summary>
    private static ClockOffset Measured =>
        new(0, new DateTime(2026, 9, 2, 14, 20, 0, DateTimeKind.Utc));

    /// <summary>
    /// **NOTHING IS SHOWN BEFORE ANYTHING IS HEARD, AND THE PANEL SAYS WHY**
    /// (HM-DEC-021).
    /// </summary>
    [Fact]
    public void AnUntouchedTabShowsItsIdleLineAndNoRows()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        _output.WriteLine($"  rows {model.DigitalDecodes.Count}");
        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");
        _output.WriteLine($"  idle [{model.DigitalDecodedIdle}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);
        Assert.Equal(DigitalIdleText.Decoded, model.DigitalDecodedIdle);
    }

    /// <summary>
    /// **THE ONE THE UNIT EXISTS FOR.** Audio with a transmission in it goes in,
    /// and the row that comes out carries the message that was sent.
    /// </summary>
    [Fact]
    public void AudioWithATransmissionInItFillsTheTable()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            Recording(48000),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            Measured);

        foreach (var row in model.DigitalDecodes)
        {
            _output.WriteLine(
                $"  {row.Utc}  {row.Snr}  {row.Dt}  {row.Hz}  {row.Message}");
        }

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        var only = Assert.Single(model.DigitalDecodes);

        Assert.Equal("CQ K1ABC FN42", only.Message);
        Assert.Equal("142230", only.Utc);

        // Placed at 1240 Hz. The search answers in bins a fraction of a tone
        // wide, so the column is asserted as a neighbourhood and not a number.
        Assert.InRange(int.Parse(only.Hz, CultureInfo.InvariantCulture), 1236, 1244);

        // **THE SNR COLUMN CARRIES A NUMBER NOW, AND THIS ASSERTION IS STALE
        // RATHER THAN THE COLUMN BEING WRONG.** It was written when `Ft8Sharp`
        // returned a sync score and no decibels, so a dash was the only honest
        // cell. `Ft8Sharp.Deep.Ft8DeepSignalToNoise` measures a real ratio -
        // 0.26 dB mean absolute error over 510 synthesized messages,
        // `docs/unit251-snr-trace.md` §6 - and `PHASE_PLAN.md` step 2 rules that
        // the column shows a number once agreement is inside 2 dB.
        //
        // **THE DASH IS STILL THE ANSWER WHERE NOTHING WAS MEASURED**, which is
        // what the second assertion holds: whatever is in this cell is either a
        // signed whole number of decibels or the dash, and never a bare digit
        // that could be read as a serial or a score.
        Assert.NotEqual(DigitalDecodeRow.NoMeasurement, only.Snr);
        Assert.True(
            only.Snr[0] is '+' or '-',
            "the snr cell reads " + only.Snr + ", which carries no sign - most "
            + "FT8 reports are negative and an unsigned figure in that column "
            + "reads as a missing minus rather than as a strong station");

        Assert.True(model.HasDigitalDecodes);
        // The summary now names the direction too, so a collapsed panel says
        // which end the newest slot is at (§0.5).
        Assert.Equal(
            "142230 UTC · 1 shown · newest first", model.DigitalDecodedSummary);
    }

    /// <summary>
    /// **A CLOCK NOBODY HAS CHECKED IS SAID IN WORDS**, on the panel's own
    /// summary, rather than left as an empty table that looks like a dead band.
    /// </summary>
    [Fact]
    public void AnUnmeasuredClockLeavesTheTableEmptyAndTheSummarySpeaking()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            Recording(48000),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            ClockOffset.Unknown);

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);
        Assert.Equal(Ft8SlotCutter.NoOffset, model.DigitalDecodedSummary);
    }

    /// <summary>
    /// **AUDIO THAT DECODED NOTHING SAYS SO RATHER THAN GOING QUIET.** Silence
    /// through the whole path leaves the table empty and the summary reporting
    /// that a slot was read and nobody was in it.
    /// </summary>
    [Fact]
    public void SilenceLeavesTheTableEmptyAndTheSummaryReportingTheSlot()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            new MonoAudio(48000, new float[48000 * 30]),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            Measured);

        _output.WriteLine($"  summary [{model.DigitalDecodedSummary}]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);
        Assert.Contains("one slot", model.DigitalDecodedSummary, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE FOUR INVENTED ROWS ARE OUT OF THE MARKUP.** Read as text, because
    /// the assertion is about what a reader of the file would find, and a bound
    /// table with a stale literal beside it would pass any test that only looked
    /// at the collection.
    /// </summary>
    [Fact]
    public void NoInventedDecodeIsLeftInTheMarkup()
    {
        var markup = DecodedPanelMarkup();

        foreach (var message in Invented)
        {
            var literal = $"Text=\"{message}\"";

            _output.WriteLine($"  {literal,-40} {(markup.Contains(literal, StringComparison.Ordinal) ? "STILL THERE" : "gone")}");

            Assert.DoesNotContain(literal, markup, StringComparison.Ordinal);
        }

        // **THE COLLECTION THE TABLE BINDS IS `DigitalVisibleDecodes` AND HAS BEEN
        // SINCE THE FILTER WAS BUILT.** This line asked for `{Binding DigitalDecodes}`,
        // which unit 252's `everything` / `CQ` chips replaced with the filtered view, so
        // it has been failing on a name rather than on a fault - a table bound to the
        // filtered collection is still a table bound to real decodes, which is all this
        // assertion was ever for. Corrected in work instruction 331 task 1a, which is
        // the unit that read the failure. **What it guards is unchanged**: the rows come
        // from a collection and not from the four literals above.
        Assert.Contains(
            "ItemsSource=\"{Binding DigitalVisibleDecodes}\"", markup, StringComparison.Ordinal);
    }

    /// <summary>
    /// **NO PANEL ON THE TAB STILL CLAIMS A STATION WAS HEARD.** The decoded
    /// table became real in this unit, so a mode strip counting nine messages or
    /// a plain-English card describing a contact that never happened is not a
    /// placeholder any more — it is the tab disagreeing with itself.
    /// </summary>
    [Fact]
    public void NoPanelOnTheTabStillClaimsAStationWasHeard()
    {
        var markup = DigitalWorkspaceMarkup();

        var claims = new[]
        {
            "reading it &#183; 9 messages this slot",
            "4 stations &#183; 2 contacts running",
            "14:22:45 UTC &#183; 4 shown",
            "W9XYZ is answering K1ABC",
            "K1ABC is calling anyone",
            "EA3QQ is calling for distant stations",
        };

        foreach (var claim in claims)
        {
            var literal = $"\"{claim}\"";

            _output.WriteLine(
                $"  {claim,-42} "
                + $"{(markup.Contains(literal, StringComparison.Ordinal) ? "STILL THERE" : "gone")}");

            Assert.DoesNotContain(literal, markup, StringComparison.Ordinal);
        }

        // **AND THE PANEL THAT LOST ITS CARDS IS ITSELF GONE** (Tim's ruling,
        // 2026-09-04: the decoded text is what people are saying). Unit 224
        // emptied it and left its idle line standing; unit 241 removed it,
        // because "nobody heard yet" over a table of real messages is a surface
        // asserting something untrue about what was heard.
        Assert.DoesNotContain(
            "DigitalSayingPanel", markup, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "What people are saying", markup, StringComparison.Ordinal);

        // The decoded panel's own idle line stays: an empty panel that says
        // nothing is indistinguishable from a broken one (HM-DEC-021).
        Assert.Contains(
            "{Binding DigitalDecodedIdle}", markup, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE MODE STRIP CARRIES ONLY TIM'S OWN IDLE LINE** (§12.1).
    /// </summary>
    /// <remarks>
    /// **THIS USED TO ASSERT THE SAME OF "WHAT PEOPLE ARE SAYING", AND THAT
    /// PANEL IS GONE** (Tim's ruling, 2026-09-04: the decoded text is what
    /// people are saying). It never had a feed and said "nobody heard yet" over
    /// a table holding sixty-three real messages, which is a surface asserting
    /// something untrue about what was heard.
    ///
    /// What the removed half guarded has not been dropped, it has been narrowed
    /// by ruling and moved: Hamlet may now translate the closed FT8 vocabulary
    /// and must say nothing about anything else. `TheMessageReadsAsThreeParts`
    /// asserts both halves of that, including the silence.
    /// </remarks>
    [Fact]
    public void TheModeStripCarriesOnlyTimsOwnIdleLine()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        _output.WriteLine($"  strip  [{model.DigitalModeStripLine}]");

        // **AN UPDATED CALL, NOT A CHANGED EXPECTATION** (unit 290 task 7).
        // `ModeStrip` was a const saying *FT8 runs in fifteen second slots*; it is
        // now a function of the grid the tab is running on, which is still FT8's.
        Assert.Equal(
            DigitalIdleText.ModeStripFor(Hamlet.RadioEngine.Audio.SlotGrid.Ft8),
            model.DigitalModeStripLine);
    }

    /// <summary>
    /// **THE MODE STRIP REPORTS THE PRESS RATHER THAN A PLACEHOLDER.** After a
    /// decode it carries what the press made of the audio.
    /// </summary>
    [Fact]
    public void TheModeStripReportsWhatThePressFound()
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        model.ShowDecodes(
            Recording(48000),
            new DateTime(2026, 9, 2, 14, 22, 47, DateTimeKind.Utc),
            Measured);

        _output.WriteLine($"  strip [{model.DigitalModeStripLine}]");

        Assert.Equal("one message out of one slot", model.DigitalModeStripLine);
    }

    /// <summary>The whole Digital workspace's markup.</summary>
    private static string DigitalWorkspaceMarkup()
    {
        var markup = File.ReadAllText(
            Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var from = markup.IndexOf(
            "x:Name=\"DigitalWorkspace\"", StringComparison.Ordinal);
        var to = markup.IndexOf(
            "x:Name=\"VoiceWorkspace\"", StringComparison.Ordinal);

        Assert.InRange(from, 0, int.MaxValue);

        return markup[from..to];
    }

    /// <summary>The decoded panel's own markup, and nothing else's.</summary>
    /// <remarks>
    /// **IT RAN TO THE PLAIN-ENGLISH PANEL AND NOW RUNS TO THE END OF THE
    /// FILE**, because unit 241 removed that panel (Tim, 2026-09-04). The slice
    /// was scoped so a claim in the panel below could not satisfy an assertion
    /// about this one; with that panel gone the decoded table is the last thing
    /// in the Digital workspace, and every assertion over the slice is a
    /// DoesNotContain, so a wider slice can only catch more.
    /// </remarks>
    private static string DecodedPanelMarkup()
    {
        var markup = File.ReadAllText(
            Path.Combine(Root(), "src", "Hamlet.App", "Views", "MainWindow.axaml"));

        var from = markup.IndexOf(
            "x:Name=\"DigitalDecodedPanel\"", StringComparison.Ordinal);
        // **THE SLICE USED TO END AT THE "WHAT PEOPLE ARE SAYING" PANEL**,
        // which unit 241 removed on Tim's ruling of 2026-09-04. The decoded
        // panel is the last thing in the Digital workspace now, so the slice
        // runs to the end of the file. That is wider than before and no weaker:
        // every assertion over this slice is a DoesNotContain, so a longer slice
        // can only catch more.
        var to = markup.Length;

        Assert.InRange(from, 0, int.MaxValue);

        return markup[from..to];
    }

    /// <summary>Thirty seconds of audio with one transmission in the whole slot.</summary>
    /// <param name="rate">The rate to build it at.</param>
    private static MonoAudio Recording(int rate)
    {
        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("CQ", "K1ABC", "FN42", message));

        var slot = Ft8Waveform.SynthesizeSlot(
            Ft8SymbolEncoder.Encode(message), rate, 1240f);

        var samples = new float[rate * 30];

        // 14:22:30 is thirteen seconds after a recording ending at 14:22:47 began.
        slot.CopyTo(samples.AsSpan(13 * rate));

        return new MonoAudio(rate, samples);
    }

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
