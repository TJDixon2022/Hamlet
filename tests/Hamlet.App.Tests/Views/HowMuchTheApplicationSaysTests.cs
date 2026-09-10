using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Licensing;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 281, task 1: how much permanently-visible text the whole
/// application carries, screen by screen.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: *"I want clean visual screens with text only
/// where I, the user, intentionally hover."* Unit 280 measured one tab, because the
/// order it was given named one tab, and the paragraph he was complaining about was
/// on a different screen. **The ruling was always app-wide**, so the measurement is
/// too.</para>
/// <para>**MEASURED THROUGH THE REAL WINDOW RATHER THAN BY COUNTING SOURCE**, for
/// unit 280's reason: almost none of this wording is a literal in the markup, so a
/// source count measures the wrong thing and then flatters whatever moved.</para>
/// <para>**IT COUNTS TWO KINDS OF TEXT AND UNIT 280 COUNTED ONE.** A
/// <see cref="TextBlock"/> and a <see cref="GlossaryTextControl"/> both put words on
/// the screen; the second is a bare <c>Control</c> that draws its own runs, so
/// walking for <c>TextBlock</c> alone silently misses it. That is why the figure
/// here for the Digital tab does not match unit 280's, and the difference is
/// reported rather than reconciled away.</para>
/// <para>**PERMANENTLY VISIBLE MEANS WITHOUT HOVERING OR OPENING ANYTHING.** A
/// tooltip is not counted, which is the whole ruling: advice may exist, it may not
/// occupy the screen while he operates.</para>
/// <para>**AND FROM WORK INSTRUCTION 282 IT CARRIES A CEILING PER SURFACE.** It
/// still asserts no total — how terse is terse enough is not a session's call —
/// but a surface that has been swept may not quietly grow back. **That is the fix
/// that outlives any one paragraph**: an 884-character line survived two sweeps
/// because it is composed at run time and a source search for it comes back empty,
/// and a search that cannot find a string is indistinguishable from a screen that
/// does not have one (§12.5).</para>
/// </remarks>
public sealed class HowMuchTheApplicationSaysTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the per-screen counts are printed.</param>
    public HowMuchTheApplicationSaysTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**Every character the application shows without being asked.**</summary>
    [AvaloniaFact]
    public void TheWholeApplicationIsMeasuredForWords()
    {
        var screens = new List<(string Name, int Blocks, int Chars)>();

        foreach (var mode in new[] { "CW", "Digital", "Voice" })
        {
            var window = OperatingWindow(mode, out var panel);

            screens.Add(Measure("MainWindow — " + mode + " tab", window));

            // The workspace on its own, so the shared chrome is not counted three
            // times over when the totals are read.
            // The markup names it CwWorkspace, not CWWorkspace, so the mode
            // string is not the name and a case-sensitive compare misses it.
            var workspace = window.GetVisualDescendants().OfType<Grid>()
                .FirstOrDefault(g =>
                    string.Equals(
                        g.Name, mode + "Workspace",
                        StringComparison.OrdinalIgnoreCase));

            if (workspace is not null)
            {
                screens.Add(Measure("    of which " + mode + "Workspace", workspace));
            }

            GC.KeepAlive(panel);
        }

        screens.Add(Measure(
            "SettingsWindow",
            new SettingsWindow { DataContext = new SettingsViewModel() }));

        screens.Add(Measure(
            "AboutWindow",
            new AboutWindow { DataContext = new AboutViewModel() }));

        screens.Add(Measure(
            "ContactLogWindow",
            new ContactLogWindow
            {
                DataContext = new ContactLogViewModel(
                    Array.Empty<AdifLogRecord>(), "contacts.adi"),
            }));

        screens.Add(Measure(
            "RigDiagnosticsWindow",
            new RigDiagnosticsWindow
            {
                DataContext = new RigDiagnosticsViewModel(null, RigState.Empty),
            }));

        screens.Add(Measure(
            "FavoritesWindow",
            new FavoritesWindow { DataContext = new FavoritesViewModel() }));

        screens.Add(Measure(
            "DecisionLogWindow",
            new DecisionLogWindow { DataContext = new DecisionLogViewModel() }));

        screens.Add(Measure(
            "LogContactWindow",
            new LogContactWindow
            {
                DataContext = new LogContactViewModel(
                    new AdifContact { Call = "W3YNI" }),
            }));

        _output.WriteLine("PERMANENTLY-VISIBLE TEXT, SCREEN BY SCREEN");
        _output.WriteLine("");

        foreach (var (name, blocks, chars) in screens)
        {
            _output.WriteLine(
                chars.ToString().PadLeft(6) + " chars  "
                + blocks.ToString().PadLeft(4) + " blocks   " + name);
        }

        var total = screens
            .Where(s => !s.Name.StartsWith("    ", StringComparison.Ordinal))
            .Sum(s => s.Chars);

        _output.WriteLine("");
        _output.WriteLine("TOTAL (indented rows excluded, they are subsets): " + total);

        Assert.True(total > 0, "nothing was measured anywhere in the application");
    }

    /// <summary>What each surface held after task 1, and what it may not exceed.</summary>
    /// <remarks>
    /// <para>**THE MEASURED FIGURE IS KEPT BESIDE THE CEILING ON PURPOSE.** A ceiling
    /// alone says what is allowed and nothing about what is true, and the gap between
    /// them is the only thing that tells a reader whether a surface is near its limit
    /// or nowhere near it.</para>
    /// <para>**THE MARGIN IS 100 CHARACTERS, ROUNDED UP TO THE NEXT 50, AND IT IS
    /// CHOSEN RATHER THAN INHERITED.** With the profile settled the measurement is
    /// stable to a couple of dozen characters — the Shakespeare byline rotates
    /// through forty-five strings of different lengths (HM-DEC-039) — so 100 is about
    /// four times the noise and a surface can gain a label or a unit without a false
    /// red. **And the fault this exists to catch is a paragraph**, which runs 448 to
    /// 884 characters on the rows the conditions file ships, so 100 cannot hide
    /// one.</para>
    /// <para>**SETTINGS IS CAPPED WHERE IT STANDS AND HAS NOT BEEN REDUCED.** It
    /// holds the largest share of the application and work instruction 282 parks it.
    /// The ceiling stops it growing; it makes no claim that it is small.</para>
    /// </remarks>
    public static IReadOnlyList<(string Surface, int Measured, int Ceiling)> Ceilings { get; }
        = new[]
        {
            ("MainWindow — CW tab", 426, 550),
            // **RE-SET BY WORK INSTRUCTION 302 ONCE THE FIGURE HELD STILL.** These
            // two rows were red for three units and could not be raised, because the
            // number they cap moved by itself: 1281 and 1293 an hour apart in unit
            // 300, 1259 in unit 301. **Three live readouts were the cause** - the
            // slot countdown, the card's relative age, and the clock-offset line -
            // and all three now contribute their widest reading rather than their
            // current one, so two sweeps of the same tree agree.
            //
            // **AND FOUR CAUSES WERE FOUND, NOT ONE.** Three live readouts now
            // contribute their widest reading rather than their current one; the
            // seeded card's clock is pinned, so it no longer ages out of the list
            // mid-sweep and lets the empty-state prose back in; and the fixture no
            // longer races a background reconnect, which was worth 17 characters
            // depending on whether it had landed. **Measured four class runs
            // running: 426, 1194, 1193 and 468 every time, in this class and in
            // another.**
            //
            // **THE FOUR MAINWINDOW ROWS ALL FELL, AND THAT IS THE PINNED CLOCK.**
            // With the offset stated rather than raced, the strip carries the
            // unmeasured line instead of a measured one, so the surfaces genuinely
            // say less than the figures they were set from a unit ago.
            ("MainWindow — Digital tab", 1194, 1300),
            // **THE WORKING TAB SAYS LESS THAN THE IDLE ONE**, which is the whole
            // shape of this phase: the empty-state explanations go away once there
            // is traffic, and everything composed at run time is now on a hover.
            ("MainWindow — Digital tab, working", 1193, 1300),
            ("MainWindow — Voice tab", 468, 600),
            ("SettingsWindow", 1628, 1750),
            ("RigDiagnosticsWindow", 1346, 1450),
            ("AboutWindow", 726, 850),
            ("LogContactWindow", 716, 850),
            ("FavoritesWindow", 280, 400),
            ("ContactLogWindow", 246, 350),
            ("DecisionLogWindow", 196, 300),

            // **THE ACHIEVEMENTS SCREEN IS CAPPED TWICE, AND FOR THE REASON THE
            // DIGITAL TAB IS**: a ceiling caps a state and not only a surface. An
            // empty log draws six rows nobody has earned; a log with contacts in it
            // draws a callsign, a date and a band on every earned row, which is
            // text an idle measurement never sees. **Measured, the worked state is
            // 31 characters larger**, and the two land on the same ceiling because
            // the margin is wider than the difference.
            //
            // **THE SIX EXPLANATIONS ARE NOT IN EITHER FIGURE AND THAT IS THE
            // POINT.** Every row's account of what stands in its way is on a
            // `HintMarkControl`, which draws nothing until he hovers it, so the
            // worked card carries 1,557 characters of prose that no ceiling has to
            // hold down. That is Tim's ruling of 2026-09-08 working rather than
            // being worked around: text only where he hovers.
            // **RE-MEASURED BY WORK INSTRUCTION 300, WHICH RE-PRESENTED THIS
            // SCREEN**, and the figures fell hard: 412 to 158, and 443 to 153. Two
            // separate things account for it, and only the second is this unit's.
            //
            // **THE FIRST WAS ALREADY THERE**: measured at this unit's own starting
            // commit, before a line was changed, the two states read 149 and 144.
            // So the row had been three and a half times its surface for at least a
            // unit, which is a ceiling guarding nothing.
            //
            // **THE SECOND IS THE POINT OF THE UNIT**: *no sentence carries what a
            // figure can.* The prose lines under the targets became rings with the
            // remaining figure in them, and the screen says less because the
            // information moved rather than went.
            ("AchievementsWindow", 158, 300),
            ("AchievementsWindow, worked", 153, 300),
        };

    /// <summary>**No surface says more than its ceiling allows.**</summary>
    /// <remarks>
    /// Watched failing first: with the Digital tab's ceiling left at the figure it
    /// held before the profile was settled, this reported it 100 characters over.
    /// </remarks>
    [AvaloniaFact]
    public void NoSurfaceSaysMoreThanItsCeiling()
    {
        var over = new List<string>();

        foreach (var (surface, was, ceiling) in Ceilings)
        {
            var chars = Chars(Surface(surface));

            _output.WriteLine(
                chars.ToString().PadLeft(5) + " / " + ceiling.ToString().PadLeft(5)
                + "   set from " + was.ToString().PadLeft(5) + "   " + surface);

            if (chars > ceiling)
            {
                over.Add(surface + " holds " + chars + ", ceiling " + ceiling);
            }
        }

        Assert.True(
            over.Count == 0,
            "these surfaces have grown past what they are allowed to say: "
            + string.Join(" | ", over));
    }

    /// <summary>
    /// **The ceiling is tight enough to have caught the paragraph.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS WHAT MAKES THE CEILING WORTH HAVING.** A ceiling that is
    /// merely above today's figure catches nothing; the question is whether it is
    /// below the figure the fault produced. The tune-in paragraph is 884 characters
    /// composed from the shipped rows, so a CW tab drawing it would measure the idle
    /// figure plus 884 — and this asserts that number is over the ceiling while the
    /// real one is under it.</para>
    /// <para>**IT IS ARITHMETIC ON TWO MEASUREMENTS AND NOT A TIMING RACE.** An
    /// earlier draft narrated the paragraph and then measured the window, and the
    /// rig heartbeat overwrote the status line between the two — so it asserted
    /// something true about a screen that no longer held the paragraph. **A test
    /// that passes for the wrong reason is worse than no test**, and that draft is
    /// recorded here rather than quietly replaced.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheCeilingIsTightEnoughToHaveCaughtTheParagraph()
    {
        var paragraph = ReceiverSetupVoice.Say(
            ReceiverConditions.ForMode("CW")
                .Select(c => new ConditionResult(c, ConditionOutcome.Changed, "", ""))
                .ToList());

        var window = OperatingWindow("CW", out _);

        var now = Chars(window);
        var ceiling = Ceilings.First(c => c.Surface == "MainWindow — CW tab").Ceiling;
        var wouldHaveBeen = now + paragraph.Length;

        _output.WriteLine("the paragraph        : " + paragraph.Length + " characters");
        _output.WriteLine("the CW tab now       : " + now);
        _output.WriteLine("with it on the bar   : " + wouldHaveBeen);
        _output.WriteLine("its ceiling          : " + ceiling);

        Assert.True(
            now <= ceiling,
            "the CW tab is already over its ceiling at " + now);

        Assert.True(
            wouldHaveBeen > ceiling,
            "a tab drawing the tune-in paragraph would have measured " + wouldHaveBeen
            + ", which is under the ceiling of " + ceiling + " - so this ceiling "
            + "would not have caught the fault it was set for");
    }

    /// <summary>**A sentence added to a capped surface turns it red.**</summary>
    /// <remarks>
    /// <para>The instruction's own criterion. It adds a real `TextBlock` to the
    /// realized tree rather than editing the application, because the question is
    /// whether the ceiling **bites** — and breaking the app to prove a test works
    /// leaves a mess behind for somebody else.</para>
    /// <para>**IT IS THE SAME WALK THE CEILING USES**, so a ceiling that could not
    /// see the added sentence would fail here rather than passing quietly.</para>
    /// </remarks>
    [AvaloniaFact]
    public void AddingASentenceToACappedSurfaceTurnsItRed()
    {
        var window = OperatingWindow("Digital", out _);

        var before = Chars(window);
        var ceiling = Ceilings.First(c => c.Surface == "MainWindow — Digital tab").Ceiling;

        Assert.True(before <= ceiling, "the fixture starts over its own ceiling");

        // A paragraph of the size this whole unit exists to stop.
        var added = new string('x', 400);

        window.GetVisualDescendants().OfType<Panel>().First()
            .Children.Add(new TextBlock { Text = added });

        Pump(window);

        var after = Chars(window);

        _output.WriteLine("before : " + before + "  ceiling " + ceiling);
        _output.WriteLine("added  : " + added.Length + " characters");
        _output.WriteLine("after  : " + after);

        Assert.Equal(before + added.Length, after);

        Assert.True(
            after > ceiling,
            "a 400-character paragraph did not take this surface over its ceiling, "
            + "so the ceiling is too loose to catch the fault it is for");
    }

    /// <summary>**Every ceiling is the measured figure plus the stated margin.**</summary>
    /// <remarks>
    /// A ceiling raised to make a red go away has stopped measuring anything. The
    /// rule is written down as arithmetic, so raising one means re-measuring and
    /// saying so rather than nudging a number.
    /// </remarks>
    [AvaloniaFact]
    public void EveryCeilingIsTheMeasuredFigurePlusTheStatedMargin()
    {
        foreach (var (surface, measured, ceiling) in Ceilings)
        {
            var wanted = ((measured + 100 + 49) / 50) * 50;

            _output.WriteLine(
                surface.PadRight(28) + measured.ToString().PadLeft(5)
                + " + 100, to the next 50 = " + wanted.ToString().PadLeft(5)
                + "   set: " + ceiling);

            Assert.Equal(wanted, ceiling);
        }
    }

    /// <summary>The realized window for one capped surface, for another class.</summary>
    /// <param name="surface">The row's name in <see cref="Ceilings"/>.</param>
    /// <returns>The shown window.</returns>
    /// <remarks>
    /// **ONE PLACE BUILDS A SURFACE** (§0). Work instruction 283's sweep walks the
    /// same windows this class caps, and a second copy of that list is how the two
    /// come to disagree about what has been looked at.
    /// </remarks>
    public static Window SurfaceForSweep(string surface) => Surface(surface);

    /// <summary>The realized window for one row of <see cref="Ceilings"/>.</summary>
    /// <param name="surface">The row's name.</param>
    /// <returns>The shown window.</returns>
    /// <remarks>
    /// **A CEILING CAPS A STATE AND NOT ONLY A SURFACE**, which is the lesson of
    /// three units in a row. Unit 282's composed send line is 473 characters and
    /// appears only after a transmission; unit 283's link-check branch is 304 and
    /// appears only on a radio that does not announce. **An idle window cannot see
    /// either**, so the Digital tab is capped twice — once at rest, and once in the
    /// state the operator actually sits in.
    /// </remarks>
    private static Window Surface(string surface)
    {
        if (surface.EndsWith("working", StringComparison.Ordinal))
        {
            return Working();
        }

        if (surface.StartsWith("MainWindow", StringComparison.Ordinal))
        {
            return OperatingWindow(surface.Split("— ")[1].Split(' ')[0], out _);
        }

        Window window = surface switch
        {
            "SettingsWindow" => new SettingsWindow { DataContext = new SettingsViewModel() },
            "AboutWindow" => new AboutWindow { DataContext = new AboutViewModel() },
            "FavoritesWindow" => new FavoritesWindow { DataContext = new FavoritesViewModel() },
            "DecisionLogWindow" => new DecisionLogWindow { DataContext = new DecisionLogViewModel() },
            "RigDiagnosticsWindow" => new RigDiagnosticsWindow
            {
                DataContext = new RigDiagnosticsViewModel(null, RigState.Empty),
            },
            "ContactLogWindow" => new ContactLogWindow
            {
                DataContext = new ContactLogViewModel(
                    Array.Empty<AdifLogRecord>(), "contacts.adi"),
            },
            "AchievementsWindow" => new AchievementsWindow
            {
                DataContext = new AchievementsViewModel(
                    Array.Empty<AdifLogRecord>()),
            },
            "AchievementsWindow, worked" => new AchievementsWindow
            {
                DataContext = new AchievementsViewModel(
                    TheAchievementsScreenTests.TwoModeLog()),
            },
            _ => new LogContactWindow
            {
                DataContext = new LogContactViewModel(new AdifContact { Call = "W3YNI" }),
            },
        };

        window.Show();
        Pump(window);

        return window;
    }

    /// <summary>The same walk this class measures with, less the byline.</summary>
    /// <param name="root">The realized window.</param>
    /// <returns>The character count a ceiling is set against.</returns>
    /// <remarks>
    /// <para>**THE ONE THING A CEILING CANNOT MEASURE IS A DIE ROLL.** The
    /// Shakespeare byline is chosen at random from forty-five lines of 20 to 104
    /// characters (HM-DEC-039), so the same screen measures 84 characters apart
    /// between one launch and the next — **more than the whole margin**, which would
    /// make every ceiling either loose enough to hide a paragraph or tight enough to
    /// go red on a Tuesday.</para>
    /// <para>**IT IS STILL MEASURED AND STILL PRINTED** by
    /// <see cref="TheWholeApplicationIsMeasuredForWords"/>, because it is on the
    /// screen and this harness does not pretend otherwise. What it is not is prose
    /// about the radio, which is the thing the ceiling exists to hold down.</para>
    /// </remarks>
    /// <summary>
    /// **The live readouts, and the widest each can ever be.**
    /// </summary>
    /// <remarks>
    /// <para>**A CEILING SET AGAINST A MOVING NUMBER CANNOT BE SET CORRECTLY**, and
    /// two rows were red and unraisable for three units because of it (work
    /// instruction 302, `HM-OPEN-089`). Measured by standing the tab up twice 37
    /// seconds apart: the slot countdown rendered `10` and then `3`, and that was
    /// the only difference between the two runs.</para>
    /// <para>**BOTH OF THESE BELONG ON SCREEN AND NEITHER MOVES BEHIND A HOVER.**
    /// The countdown is Tim's own ask - *nobody has responded, but I want to know
    /// how long I have till the next transmit cycle* - and how stale a card is, is
    /// the whole reason its time line exists. **They are live measurements, not
    /// prose**, and the ceiling exists to stop prose growing.</para>
    /// <para>**SO THE SWEEP COUNTS EACH AT ITS WIDEST READING** rather than at
    /// whatever the clock happened to say. That keeps them inside the ceiling's
    /// remit - turn one into a sentence and the figure still moves - while making
    /// the figure the same on two runs of the same tree.</para>
    /// <para>**THE WIDTHS ARE THE WORST CASE AND ARE DERIVED, NOT GUESSED.** The
    /// countdown is `Ft8Turn.CountText`: whole seconds on FT8, so at most `15`, and
    /// one decimal on FT4's 7.5-second slot, so at most `7.5` - three characters,
    /// and `?` where no clock has been measured. The time line is
    /// `HH:mm:ss UTC · <age>`, which is 12 characters of clock plus the longest age
    /// `Ft8ContactCard.Ago` can compose; `999 hours ago` is 13, so 12 + 3 + 13 is
    /// 28. The clock line is `Ft8Slots.ClockOffset.Describe`: `clock is ` is 9, the
    /// offset formats to two decimals, ` s slow` is 7, `, checked ` is 10 and the
    /// longest age it composes is `89 minutes ago` at 14 - so 47 covers an offset up
    /// to four figures, and the unmeasured branch, `clock not checked yet, so slots
    /// cannot be cut`, is 44 and fits under it.</para>
    /// </remarks>
    private static readonly Dictionary<string, int> LiveReadouts = new()
    {
        ["TurnRingCountText"] = 3,
        ["CardTimeLineText"] = 28,
        ["ClockOffsetLineText"] = 47,
    };

    /// <summary>
    /// How many characters one control contributes to its surface's total.
    /// </summary>
    /// <remarks>
    /// **A LIVE READOUT CONTRIBUTES ITS CEILING AND NOT ITS CURRENT READING**, so
    /// two sweeps of the same tree agree. Everything else contributes what it says.
    /// **A live readout that grew past its stated width is a defect and fails
    /// loudly** rather than being quietly clamped: it means the reading is no longer
    /// what this table says it is.
    /// </remarks>
    private static int Contribution(Control control, string text)
    {
        if (control.Name is not { } name || !LiveReadouts.TryGetValue(name, out var most))
        {
            return text.Length;
        }

        Assert.True(
            text.Length <= most,
            name + " rendered " + text.Length + " characters (\"" + text
            + "\"), past the " + most + " this sweep counts it as. Either the "
            + "readout changed or the stated width was wrong; re-derive it.");

        return most;
    }

    /// <summary>The same count, for another class.</summary>
    /// <param name="root">A realized surface.</param>
    /// <returns>What it says, with its live readouts counted at their widest.</returns>
    /// <remarks>
    /// **ONE PLACE COUNTS A SURFACE** (§0), for the same reason one place builds one:
    /// a second copy of this sweep is how two tests come to disagree about what a
    /// surface holds.
    /// </remarks>
    public static int CharsForSweep(Window root) => Chars(root);

    private static int Chars(Window root)
        => root.GetVisualDescendants()
            .Where(v => (v as Control)?.Name != "Byline")
            .Select(v => v switch
            {
                TextBlock block when block.IsEffectivelyVisible
                    => (Control?)block,
                GlossaryTextControl gloss when gloss.IsEffectivelyVisible
                    => gloss,
                _ => null,
            })
            .Where(c => c is not null)
            .Select(c => (Control: c!, Text: c switch
            {
                TextBlock block => block.Text,
                GlossaryTextControl gloss => gloss.Text,
                _ => null,
            }))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Text))
            .Sum(pair => Contribution(pair.Control, pair.Text!));

    /// <summary>
    /// **The Digital tab in the state he operates it in**, not at rest.
    /// </summary>
    /// <returns>The shown window.</returns>
    /// <remarks>
    /// Traffic on both lists, a message he sent, the long link-check branch his own
    /// radio produces, and the receiver narration a tune-in composes. **Every one of
    /// those is a string an idle measurement never sees**, and two of the three were
    /// found by a unit only after they had shipped.
    /// </remarks>
    private static Window Working()
    {
        var window = OperatingWindow("Digital", out var panel);

        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        panel.AddDecodeRowForTests(
            "214130", "-11", "0.2", "1240", "CQ W3YNI FN20", slot, 14_074_000);
        panel.AddDecodeRowForTests(
            "214130", "-13", "0.3", "1310", "KC3QIS W3YNI +02", slot, 14_074_000);
        panel.AddSentRowForTests("W3YNI KC3QIS R-09", slot.AddSeconds(15));

        // The link check his own radio produces: it does not announce its own
        // changes, which is the 304-character branch.
        panel.UseLinkCheckForTests(LinkSelfCheck.Describe(
            null,
            RigState.Empty
                .With(RigValue.Known(
                    RigField.Frequency, 14_074_000, "14.074000", slot, "poll"))
                .With(RigValue.Known(
                    RigField.CivTransceive, 0, "off", slot, "poll")),
            slot,
            true));

        // And a tune-in's narration on the status line.
        panel.NarrateForTests(
            ReceiverSetupVoice.Say(
                ReceiverConditions.ForMode("FT8")
                    .Select(c => new ConditionResult(c, ConditionOutcome.Changed, "", ""))
                    .ToList()),
            "");

        Pump(window);

        return window;
    }

    /// <summary>The main window in one operating mode, realized.</summary>
    /// <param name="mode">CW, Digital or Voice.</param>
    /// <param name="panel">The view model behind it, kept alive by the caller.</param>
    /// <returns>The shown window.</returns>
    private static Window OperatingWindow(string mode, out MainWindowViewModel panel)
    {
        var settings = Settled();

        panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = mode,
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        Pump(window);

        if (mode == "Digital")
        {
            var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

            // **THE CARD'S CLOCK IS PINNED, AND THAT IS THE THIRD MOVER** (work
            // instruction 302 task 2). The slot was already fixed and *now* was not,
            // so the seeded contact aged against the wall clock: measured five times
            // in six seconds, this surface read 1293 with 76 blocks and then 1309
            // with 66 - **the card dropped out of the list and the empty-state
            // explanation it had been hiding came back**, which is why the total rose
            // while the block count fell.
            //
            // **A CEILING CANNOT CAP A SURFACE THAT PICKS ITS OWN STATE FROM THE
            // HOUR IT RAN.** Two minutes is a card he is still looking at, which is
            // the state the panel is built around, and it is the same two minutes on
            // every run for ever.
            panel.CardsNowForTests = slot.AddMinutes(2);

            panel.AddDecodeRowForTests(
                "214130", "-11", "0.2", "1240", "CQ W3YNI FN00", slot, 14_074_000);
            panel.AddDecodeRowForTests(
                "214130", "-13", "0.3", "1310", "KC3QIS W3YNI +02", slot, 14_074_000);

            Pump(window);
        }

        // **THE CLOCK IS PUT IN A STATED CONDITION LAST, AND THIS IS THE FOURTH
        // MOVER** (work instruction 302 task 2). `ClockIsConcerning` decides whether
        // the offset line is on the strip at all, and the sweep counts that line as
        // 47 - so its presence alone is worth 46 against the glyph that replaces it.
        //
        // **WHAT WAS MOVING IT IS A LIVE NETWORK CALL.** `QueryTheClockAsync` runs an
        // SNTP query and assigns the result whenever it lands, so the first window
        // built in a process measured 1240 and every later one 1194 - the query
        // having answered in between. **A ceiling cannot be set against a figure
        // that depends on whether a time server replied yet.**
        //
        // **IT IS SET AFTER THE PUMP AND NOT BEFORE**, because an assignment at
        // construction is simply overwritten by the reply. Unknown is the honest
        // condition for a window with no radio behind it: the clock has not been
        // checked, so the line says so and says it at a length that does not move.
        panel.ClockOffset = Hamlet.RadioEngine.Audio.ClockOffset.Unknown;

        Pump(window);

        return window;
    }

    /// <summary>Sum what one realized subtree shows, and print its longest.</summary>
    /// <param name="name">What to call it in the printout.</param>
    /// <param name="root">The window or subtree to walk.</param>
    /// <returns>The name, the block count and the character count.</returns>
    private (string Name, int Blocks, int Chars) Measure(string name, Visual root)
    {
        if (root is Window window && !window.IsVisible)
        {
            window.Show();
            Pump(window);
        }

        var shown = Shown(root).ToList();
        var chars = shown.Sum(t => t.Length);

        _output.WriteLine("=== " + name + " — " + chars + " chars in "
            + shown.Count + " blocks");

        foreach (var text in shown.OrderByDescending(t => t.Length))
        {
            _output.WriteLine("  " + text.Length.ToString().PadLeft(4) + "  " + text);
        }

        _output.WriteLine("");

        return (name, shown.Count, chars);
    }

    /// <summary>
    /// Everything on the screen without hovering or opening anything.
    /// </summary>
    /// <param name="root">Where to start.</param>
    /// <returns>The visible strings.</returns>
    /// <remarks>
    /// **BOTH TEXT-BEARING CONTROLS.** <see cref="GlossaryTextControl"/> is a bare
    /// <c>Control</c> that draws its own text, so it is invisible to a walk that
    /// looks only for <see cref="TextBlock"/> — which is what unit 280's harness
    /// did, and why its Digital-tab figure is lower than this one's.
    /// </remarks>
    public static IEnumerable<string> Shown(Visual root)
    {
        foreach (var visual in root.GetVisualDescendants())
        {
            var text = visual switch
            {
                TextBlock block when block.IsEffectivelyVisible => block.Text,
                GlossaryTextControl gloss when gloss.IsEffectivelyVisible => gloss.Text,
                _ => null,
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text!;
            }
        }
    }

    /// <summary>
    /// A profile with nothing left for Hamlet to look up.
    /// </summary>
    /// <returns>The settings.</returns>
    /// <remarks>
    /// <para>**A MEASUREMENT THAT RACES A NETWORK LOOKUP CANNOT CARRY A CEILING**
    /// (work instruction 282 task 2). With the class unset, the panel starts
    /// resolving the licence from the callsign the moment it is built, and whether
    /// that has landed by the time the layout is pumped depends on how long the test
    /// run has been going. Measured on this machine: the Voice tab read **541
    /// characters unresolved and 623 resolved**, the same window in two states, and
    /// a ceiling set against either would be a coin toss against the other.</para>
    /// <para>**AND THE RESOLVED STATE IS THE RIGHT ONE TO MEASURE.** His own profile
    /// has the class verified from callook.info and the grid `FN00DJ` beside it, so
    /// the settled screen is the screen he operates.</para>
    /// </remarks>
    public static AppSettings Settled()
    {
        var settings = new AppSettings();

        // **THE SWEEP DOES NOT RACE A RECONNECT** (work instruction 302 task 2).
        // `ReconnectOnStartup` ships on (HM-DEC-052), so the fixture was starting the
        // training radio in the background; when that landed before a sweep it
        // created the spectrum source and the waterfall's collapsed summary stopped
        // saying `not listening yet`, taking **17 characters** off the total.
        //
        // **THAT IS WHY BOTH DIGITAL ROWS FELL BY EXACTLY 17 TOGETHER** on some runs
        // of this class and not others - measured, 1293 and 1245 against 1276 and
        // 1228 - while the surface was rock steady across thirty-four sweeps on its
        // own. **A ceiling cannot be set against a figure that depends on whether a
        // background connect has finished yet.**
        settings.ReconnectOnStartup = false;

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.GridSquareSource = ProfileFactSource.LookedUp;
        settings.Operator.GridSquareVerifiedAs = "FN00DJ";
        settings.Operator.Latitude = 40.0417;
        settings.Operator.Longitude = -74.9583;
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.Operator.LicenseClassSource = LicenseClassSource.LookedUp;
        settings.Operator.LicenseClassVerifiedAs = LicenseClass.General;

        return settings;
    }

    /// <summary>Let the layout settle so the tree is realized.</summary>
    /// <param name="window">The window to pump.</param>
    public static void Pump(Window window)
    {
        window.UpdateLayout();
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }
}
