using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Tests.Psk31;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 379 task 1, **criterion 8.1**: what a PSK31 contact and an Olivia contact
/// earn today, beside what the same contact on FT8 earns.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT AND NOT AN ASSERTION**, the shape of `Unit378Trace` before it.
/// It changes no source file, repairs nothing, and asserts nothing about the product: it prints.
/// It is on neither carry-forward line and it goes green when the product moves under it, which
/// is correct for a trace - **the before is preserved in the report, in `PHASE_OUTCOME.md` and in
/// this task's own commit message**, which is the only place the next unit can read it (work
/// instruction 379 section 3, unit 378's item 4).</para>
/// <para>**THE BAR IS PARITY WITH FT8, MEASURED SIDE BY SIDE** (section 6 ruling 1). One contact
/// is built three times over - the same callsign, the same grid, the same band, the same times -
/// **differing only in the mode pair**: `MODE=FT8`; `MODE=PSK` with `SUBMODE=PSK31`;
/// `MODE=OLIVIA`. Everything after that is read in three columns, and **a cell where all three
/// agree is parity, including all three being nothing.**</para>
/// <para>**COMPUTED, NOT SEEN** (FACT-004). Every figure here is composed on the development
/// machine from records this file wrote and conversations replayed out of the corpus. No port is
/// opened, no device is enumerated, nothing is keyed, and nothing in it is evidence about the
/// radio.</para>
/// </remarks>
public sealed class Unit379Trace : IDisposable
{
    /// <summary>20 m, inside a General class data segment, where the cited table gives an Olivia spot.</summary>
    private const long DialOn20m = 14_071_500;

    /// <summary>The operator's own locator, the one the corpus tests use.</summary>
    private const string MyGrid = "FN00DJ";

    /// <summary>
    /// **The one station, worked three times over.**
    /// </summary>
    /// <remarks>
    /// **THE PREFIX HAS TO RESOLVE OR THE TABLE MEASURES NOTHING.** `YB` is one entity in
    /// `data/callsigns/dxcc-prefixes.json` - Indonesia - so `DxccPrefixes.EntityOf` answers it and
    /// the countries, continents and quill columns have something to say. **`VK` was tried first
    /// and declined**: it is claimed by Australia, Heard I. and Lord Howe I., and the table returns
    /// nothing rather than guessing which (§0.0). That is the table working, and it is reported.
    /// `OI33` is far enough from `FN00DJ` to cross both mileage firsts.
    /// </remarks>
    private const string His = "YB1ABC";

    /// <summary>His locator, four characters, as a record carries it.</summary>
    private const string HisGrid = "OI33";

    /// <summary>The other station of the corpus's textbook exchange, worked on Olivia.</summary>
    private const string OnOlivia = "W1AW";

    /// <summary>The station of the chatty transcript, worked on PSK31.</summary>
    private const string OnPsk31 = "G4XYZ";

    /// <summary>Where a PSK31 carrier sits in the passband.</summary>
    private const double Psk31Hz = 1234;

    private const string Textbook = "01-textbook";

    /// <summary>The Olivia variant the record and the card carry here.</summary>
    private const string Variant = "16/500";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the trace and redirects the data folder, so no real log is read.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit379Trace(ITestOutputHelper output)
    {
        _output = output;
        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit379-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Puts the real folder back.</summary>
    public void Dispose()
    {
        SettingsStore.DataFolder = _wasFolder;

        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    /// <summary>The three columns, in the order the report prints them.</summary>
    private static IReadOnlyList<(string Name, ContactMode Mode)> Columns { get; } =
    [
        ("FT8", ContactModes.Named("FT8")!),
        ("PSK31", ContactModes.Named("PSK31")!),
        ("Olivia", ContactModes.Olivia(Variant)),
    ];

    /// <summary>**What each mode earns today, in three columns.**</summary>
    [Fact]
    public void Unit379TraceWhatEachModeEarnsToday()
    {
        var points = AchievementPoints.Parse(AchievementPoints.Shipped());

        Assert.True(points.Loaded, "the shipped points file did not parse");

        Line("UNIT 379 TASK 1 - CRITERION 8.1. Measured before anything moved.");
        Line("operator grid " + MyGrid + "; the station " + His + " at " + HisGrid + ", 20 m.");
        Line("the three columns differ only in the mode pair:");

        foreach (var (name, mode) in Columns)
        {
            Line("  " + Pad(name, 8) + " MODE=" + (mode.AdifMode ?? "<null>")
                + "  SUBMODE=" + (mode.AdifSubmode ?? "<null>"));
        }

        var logs = Columns.ToDictionary(c => c.Name, c => LogOf(c.Mode));

        ItemOne(logs);
        ItemTwo(logs, points);
        ItemThree();
        ItemFour(logs, points);
        ItemFive();
        ItemSix();
        ItemSeven(points);
    }

    /// <summary>**1: the one contact, three times over, as `AchievementLog` reads it back.**</summary>
    private void ItemOne(IReadOnlyDictionary<string, AchievementLog> logs)
    {
        Head("1. THE ONE CONTACT, THREE TIMES OVER - what AchievementLog.Read gets out of it");

        Line(Pad("field", 12) + Cols());

        foreach (var field in new[]
                 {
                     "Entity", "Continent", "Band", "Mode", "Grid", "State", "Miles",
                 })
        {
            var cells = Columns.Select(c =>
            {
                var contact = logs[c.Name].Contacts.Single();

                return field switch
                {
                    "Entity" => Show(contact.Entity),
                    "Continent" => Show(contact.Continent),
                    "Band" => Show(contact.Band),

                    // **BY NAME, AND `null` WHERE NOTHING MATCHED**, which is the whole
                    // question: unit 368 fixed Olivia matching nothing here.
                    "Mode" => Show(contact.Mode?.Name),
                    "Grid" => Show(contact.Grid),
                    "State" => Show(contact.State),
                    _ => contact.Miles is { } m
                        ? m.ToString("0", CultureInfo.InvariantCulture)
                        : "null",
                };
            }).ToList();

            Row(field, cells);
        }
    }

    /// <summary>**2: the eight kinds, the firsts, the scores and the total, three columns.**</summary>
    private void ItemTwo(
        IReadOnlyDictionary<string, AchievementLog> logs, AchievementPoints points)
    {
        Head("2. THE EIGHT KINDS - AchievementScores.WorkedIn on each of the three logs");

        Line(Pad("kind", 14) + Cols());

        foreach (var kind in AchievementKinds.All)
        {
            Row(
                kind,
                Columns.Select(c => AchievementScores
                    .WorkedIn(kind, logs[c.Name])
                    .ToString(CultureInfo.InvariantCulture)).ToList(),
                14);
        }

        Line("");
        Line("THE HALL OF FAME KEYS - AchievementScores.FirstsEarned on each:");

        foreach (var (name, _) in Columns)
        {
            var earned = AchievementScores.FirstsEarned(logs[name]);

            Line("  " + Pad(name, 8) + string.Join(", ", earned)
                + "   (" + earned.Count + ")");
        }

        Line("");
        Line("THE SCORE PER KIND, WITH THE SHIPPED POINTS FILE:");
        Line(Pad("kind", 14) + Cols());

        var scores = Columns.ToDictionary(c => c.Name, c => new AchievementScores(logs[c.Name], points));

        foreach (var kind in AchievementKinds.All)
        {
            Row(
                kind,
                Columns.Select(c => scores[c.Name].For(kind).Points is { } p
                    ? p.ToString(CultureInfo.InvariantCulture)
                    : "null").ToList(),
                14);
        }

        Row(
            "TOTAL",
            Columns.Select(c => scores[c.Name].Total is { } t
                ? t.ToString(CultureInfo.InvariantCulture)
                : "null").ToList(),
            14);

        Line("");
        Line("MARKED CELLS - every cell where PSK31 or Olivia differs from FT8.");
        Line("SHORT is the gap and is this unit's to close (section 6 ruling 1 item 3);");
        Line("ABOVE is not a gap - a keyboard mode reaching MORE than FT8 from the same facts");
        Line("is a fact about the owner's points file and is reported, never levelled down.");

        var marked = 0;
        var short8 = 0;

        foreach (var kind in AchievementKinds.All)
        {
            var ft8 = AchievementScores.WorkedIn(kind, logs["FT8"]);

            foreach (var name in new[] { "PSK31", "Olivia" })
            {
                var theirs = AchievementScores.WorkedIn(kind, logs[name]);

                if (theirs != ft8)
                {
                    marked++;

                    var how = theirs < ft8 ? "SHORT" : "ABOVE";

                    if (theirs < ft8)
                    {
                        short8++;
                    }

                    Line("  MARKED " + how + " worked  " + Pad(kind, 14) + name + " " + theirs + " vs FT8 " + ft8);
                }
            }

            var ft8Points = scores["FT8"].For(kind).Points;

            foreach (var name in new[] { "PSK31", "Olivia" })
            {
                var theirs = scores[name].For(kind).Points;

                if (theirs != ft8Points)
                {
                    marked++;

                    var how = (theirs ?? 0) < (ft8Points ?? 0) ? "SHORT" : "ABOVE";

                    if ((theirs ?? 0) < (ft8Points ?? 0))
                    {
                        short8++;
                    }

                    Line("  MARKED " + how + " points  " + Pad(kind, 14) + name + " "
                        + Show(theirs?.ToString(CultureInfo.InvariantCulture))
                        + " vs FT8 " + Show(ft8Points?.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        foreach (var name in new[] { "PSK31", "Olivia" })
        {
            if (scores[name].Total != scores["FT8"].Total)
            {
                marked++;

                var how = (scores[name].Total ?? 0) < (scores["FT8"].Total ?? 0) ? "SHORT" : "ABOVE";

                if ((scores[name].Total ?? 0) < (scores["FT8"].Total ?? 0))
                {
                    short8++;
                }

                Line("  MARKED " + how + " total   " + name + " " + scores[name].Total
                    + " vs FT8 " + scores["FT8"].Total);
            }
        }

        Line("  MARKED CELLS IN ITEM 2: " + marked + ", OF WHICH SHORT OF FT8: " + short8);
        Line("  THE SHORT SET IS EXACTLY WHAT TASK 2 HAS TO CLOSE.");

        // **AND THE POINTS FILE'S OWN HALL OF FAME KEYS**, because the asymmetry above is the
        // file's and not the scorer's, and the report has to be able to say which.
        Line("");
        Line("THE POINTS FILE'S HALL OF FAME KEYS, for every mode the log can write:");

        foreach (var mode in ContactModes.Logged)
        {
            var key = "first_" + mode.Name.ToLowerInvariant();

            Line("  " + Pad(mode.Name, 8) + Pad(key, 16)
                + (points.Special(AchievementKinds.HallOfFame, key) is { } worth
                    ? worth + " pts"
                    : "NO SUCH KEY IN THE FILE"));
        }
    }

    /// <summary>
    /// **3: what Hamlet's own record carries, which is a different question.**
    /// </summary>
    /// <remarks>
    /// **THE SCORER CAN ONLY READ WHAT THE RECORD CARRIES**, so a gap here and a gap in item 2
    /// are different repairs. Each of the three is driven all the way through
    /// `ContactLogEntryForStation`, the one place a log record is built.
    /// </remarks>
    private void ItemThree()
    {
        Head("3. WHAT HAMLET'S OWN RECORD CARRIES - ContactLogEntryForStation, all three modes");

        var corpus = Psk31Corpus.Load();
        var built = new Dictionary<string, AdifContact?>(StringComparer.Ordinal);

        // **THE SAME STATION AND THE SAME WORDS ON BOTH KEYBOARD MODES** (section 6 ruling 1:
        // the only honest way to check an *exactly as* is a comparison). The textbook transcript
        // is replayed down the PSK31 panel and down the Olivia panel, so a field that is filled
        // for one and null for the other is the mode's doing and not the fixture's.
        var psk31Panel = Psk31Panel(corpus.Operator);

        WorkTranscriptOnPsk31(psk31Panel, corpus);
        built["PSK31"] = psk31Panel.ContactLogEntryForStation(OnOlivia, DialOn20m);

        // **A REAL OLIVIA CHANNEL AT A VARIANT, THE SAME WORDS.**
        var oliviaPanel = OliviaPanel(corpus.Operator);

        WorkOlivia(oliviaPanel, corpus);

        var card = oliviaPanel.DigitalCards.FirstOrDefault(c => c.Callsign == OnOlivia);

        Line("the Olivia card is showing variant \"" + (card?.OliviaVariant ?? "<no card>") + "\"");

        var psk31Card = psk31Panel.DigitalCards.FirstOrDefault(c => c.Callsign == OnOlivia);

        Line("the PSK31 card is showing grid \"" + (psk31Card?.Facts.Grid ?? "<no card>")
            + "\"; the Olivia card \"" + (card?.Facts.Grid ?? "<no card>") + "\"");

        built["Olivia"] = oliviaPanel.ContactLogEntryForStation(OnOlivia, DialOn20m);

        // **AND AN FT8 CONTACT WITH THE SAME STATION, THE SAME GRID AND ITS OWN DIAL.**
        var ft8Panel = SlottedPanel(corpus.Operator, "FT8");
        var slot = new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc);

        ft8Panel.AddDecodeRowForTests(
            "214130", "-12", "0.2", "1240", "CQ " + OnOlivia + " FN31", slot, 14_074_000);
        ft8Panel.AddDecodeRowForTests(
            "214145", "-12", "0.2", "1240", corpus.Operator + " " + OnOlivia + " -12",
            slot.AddSeconds(15), 14_074_000);

        built["FT8"] = ft8Panel.ContactLogEntryForStation(OnOlivia, 14_074_000);

        Line("");
        Line(Pad("field", 18) + Pad("FT8", 26) + Pad("PSK31", 26) + "Olivia");

        var order = new[] { "FT8", "PSK31", "Olivia" };

        foreach (var field in FieldNames)
        {
            var cells = order.Select(m => Show(FieldOf(built[m], field))).ToList();

            Line(Pad(field, 18) + Pad(cells[0], 26) + Pad(cells[1], 26) + cells[2]);
        }

        Line("");
        Line("FILLED FOR ONE MODE AND NULL FOR ANOTHER:");

        var any = false;

        foreach (var field in FieldNames)
        {
            var filled = order.Where(m => FieldOf(built[m], field) is not null).ToList();

            if (filled.Count is > 0 and < 3)
            {
                any = true;
                Line("  " + Pad(field, 18) + "filled for " + string.Join(", ", filled)
                    + "; null for " + string.Join(", ", order.Except(filled)));
            }
        }

        if (!any)
        {
            Line("  none - every field is filled for all three or null for all three");
        }

        Line("");
        Line("AND THE WHOLE RECORD AS THE FILE WOULD CARRY IT:");

        foreach (var m in order)
        {
            Line("  " + Pad(m, 8) + (built[m] is { } c ? AdifLog.Record(c).Replace("\n", " ") : "<null>"));
        }
    }

    /// <summary>**4: the Modes badge and the Hall of Fame, before, card titles verbatim.**</summary>
    private void ItemFour(
        IReadOnlyDictionary<string, AchievementLog> logs, AchievementPoints points)
    {
        Head("4. THE MODES BADGE AND THE HALL OF FAME, BEFORE - titles verbatim");

        Line("AchievementBadgePage.ModesToWork: \"" + AchievementBadgePage.ModesToWork + "\"");
        Line("AchievementScores.WorkableModes: " + AchievementScores.WorkableModes);

        foreach (var kind in new[] { AchievementKinds.Modes, AchievementKinds.HallOfFame })
        {
            foreach (var (name, _) in Columns)
            {
                var page = new AchievementBadgePage(logs[name], points) { OperatorGrid = MyGrid };
                var category = AchievementCategory.For(kind, page);

                Line("");
                Line("  " + kind + " / " + name + ":");

                if (category is null)
                {
                    Line("    <no category>");
                    continue;
                }

                Line("    standing  \"" + category.Standing + "\"");
                Line("    score     \"" + category.ScoreLine + "\"");
                Line("    band      \"" + category.BandLine + "\"");

                foreach (var c in category.Cards)
                {
                    Line("    card      \"" + c.Title + "\"  earned=" + c.Earned
                        + "  figure=\"" + c.Figure + "\"  points=\"" + c.PointsLine + "\"");
                    Line("              callsign=\"" + c.Callsign + "\" callGrid=\"" + c.CallGridLine
                        + "\" bandMode=\"" + c.BandModeLine + "\" date=\"" + c.DateLine
                        + "\" distance=\"" + c.DistanceLine + "\" wants=\"" + c.WantsLine + "\"");
                }
            }
        }

        // **AND THE TWO KEYBOARD FIRSTS SIDE BY SIDE ON ONE LOG THAT HOLDS BOTH**, earned and
        // unearned, because the card for a first is built by a different method in each state
        // and a gap can be in either.
        Line("");
        Line("THE TWO KEYBOARD FIRSTS ON ONE LOG THAT HOLDS BOTH CONTACTS:");

        var bothPage = new AchievementBadgePage(
            new AchievementLog(
                [RecordOf(ContactModes.Named("PSK31")!), RecordOf(ContactModes.Olivia(Variant))],
                MyGrid),
            points)
        {
            OperatorGrid = MyGrid,
        };

        PrintFirsts(bothPage, "both worked");

        // **AND THE UNEARNED STATE**: a log with neither, so both firsts are still to come.
        var neitherPage = new AchievementBadgePage(
            new AchievementLog([RecordOf(ContactModes.Named("FT8")!)], MyGrid),
            points)
        {
            OperatorGrid = MyGrid,
        };

        PrintFirsts(neitherPage, "neither worked");

        // **AND THE STATE WHERE THE OLIVIA FIRST IS THE ONE STILL TO COME.** The Hall of Fame
        // draws the earned cards and exactly one unearned *next* card, so the only way to see
        // what Olivia's *next* card says is a log that has earned everything else.
        Line("");
        Line("THE UNEARNED CARD, on a log holding every workable mode BUT Olivia:");

        // **THE OPERATOR'S OWN ENTITY IS THE FIRST IN THE FILE** (`FirstsEarned`), so the home
        // station leads and the DX one follows - otherwise `first_dx` never earns and it, rather
        // than the keyboard first, is the one unearned card the page draws.
        List<AdifLogRecord> AllBut(string name)
            => ContactModes.Logged
                .Where(m => m.IsContactMode
                            && !string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase))
                .SelectMany(m => new[]
                {
                    RecordOf(m, "W1AW", "FN31"), RecordOf(m, His, HisGrid),
                })
                .ToList();

        var allButOlivia = AllBut(ContactModes.OliviaName);
        var allButPsk31 = AllBut("PSK31");

        foreach (var (what, records) in new[]
                 {
                     ("all but Olivia", allButOlivia), ("all but PSK31", allButPsk31),
                 })
        {
            var page = new AchievementBadgePage(new AchievementLog(records, MyGrid), points)
            {
                OperatorGrid = MyGrid,
            };

            var category = AchievementCategory.For(AchievementKinds.HallOfFame, page);
            var next = category?.Cards.FirstOrDefault(c => !c.Earned);

            Line("  " + Pad(what, 16) + (next is null
                ? "<no unearned card>"
                : "\"" + next.Title + "\"  wants=\"" + next.WantsLine
                  + "\"  noCaller=\"" + next.NoCallerLine + "\""));
        }
    }

    /// <summary>The two keyboard-mode Hall of Fame cards on one page, whatever state they are in.</summary>
    private void PrintFirsts(AchievementBadgePage page, string what)
    {
        Line("");
        Line("  " + what + ":");

        var category = AchievementCategory.For(AchievementKinds.HallOfFame, page);

        if (category is null)
        {
            Line("    <no category>");
            return;
        }

        foreach (var c in category.Cards.Where(
                     c => c.Title.Contains("PSK31", StringComparison.OrdinalIgnoreCase)
                          || c.Title.Contains("Olivia", StringComparison.OrdinalIgnoreCase)))
        {
            Line("    \"" + c.Title + "\"  earned=" + c.Earned + "  figure=\"" + c.Figure + "\"");
            Line("        callsign=\"" + c.Callsign + "\" callGrid=\"" + c.CallGridLine
                + "\" bandMode=\"" + c.BandModeLine + "\" date=\"" + c.DateLine + "\"");
            Line("        wants=\"" + c.WantsLine + "\" noCaller=\"" + c.NoCallerLine + "\"");
        }
    }

    /// <summary>**5: the quill, row by row - a PSK31 row, an Olivia row, and an FT8 row.**</summary>
    private void ItemFive()
    {
        Head("5. THE QUILL, ROW BY ROW - is a keyboard-mode row's quill the FT8 row's?");

        var corpus = Psk31Corpus.Load();

        var psk31Panel = Psk31Panel(corpus.Operator);

        psk31Panel.ShowPsk31ChannelsForTests([new Psk31Channel(1, Psk31Hz, 10.0, CqFrom(His))]);

        var oliviaPanel = OliviaPanel(corpus.Operator);

        oliviaPanel.ShowOliviaChannelsForTests([OliviaChannelOf(68, Variant, CallingOffsetOn(oliviaPanel), CqFrom(His))]);

        var ft8Panel = SlottedPanel(corpus.Operator, "FT8");
        var slot = new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc);

        ft8Panel.AddDecodeRowForTests("214130", "-12", "0.2", "1240", "CQ " + His + " " + HisGrid, slot, 14_074_000);

        var rows = new List<(string Kind, DigitalDecodeRow? Row)>
        {
            ("FT8", ft8Panel.DigitalDecodes.FirstOrDefault(r => r.Sender == His)),
            ("PSK31", psk31Panel.DigitalDecodes.FirstOrDefault(r => r.Sender == His)),
            ("Olivia", oliviaPanel.DigitalDecodes.FirstOrDefault(r => r.Sender == His)),
        };

        foreach (var (kind, row) in rows)
        {
            Line("");
            Line("  " + kind + " row for " + His + ":");

            if (row is null)
            {
                Line("    <no row named that station>");
                continue;
            }

            Line("    IsTextOnly        " + row.IsTextOnly);
            Line("    Variant           \"" + row.Variant + "\"  HasVariant=" + row.HasVariant);
            Line("    Nudge             " + row.Nudge);
            Line("    NudgeTip          \"" + row.NudgeTip + "\"");
            Line("    NudgeReasonLine   \"" + row.NudgeReasonLine + "\"");
            Line("    NudgeEarnsLine    \"" + row.NudgeEarnsLine + "\"");
            Line("    NudgePreview      HasPreview=" + row.NudgePreview.HasPreview
                + " IsDoor=" + row.NudgePreview.IsDoor);
        }

        Line("");
        Line("SAME KIND, SAME WORDS AS THE FT8 ROW?");

        var ft8Row = rows.First(r => r.Kind == "FT8").Row;

        foreach (var (kind, row) in rows.Where(r => r.Kind != "FT8"))
        {
            if (ft8Row is null || row is null)
            {
                Line("  " + Pad(kind, 8) + "cannot be compared - a row is missing");
                continue;
            }

            var differences = new List<string>();

            if (row.Nudge != ft8Row.Nudge)
            {
                differences.Add("Nudge " + row.Nudge + " vs " + ft8Row.Nudge);
            }

            if (row.NudgeTip != ft8Row.NudgeTip)
            {
                differences.Add("NudgeTip");
            }

            if (row.NudgeReasonLine != ft8Row.NudgeReasonLine)
            {
                differences.Add("NudgeReasonLine");
            }

            if (row.NudgeEarnsLine != ft8Row.NudgeEarnsLine)
            {
                differences.Add("NudgeEarnsLine");
            }

            if (row.NudgePreview.HasPreview != ft8Row.NudgePreview.HasPreview)
            {
                differences.Add("NudgePreview.HasPreview");
            }

            Line("  " + Pad(kind, 8) + (differences.Count == 0
                ? "IDENTICAL - same kind, same words"
                : "DIFFERS: " + string.Join("; ", differences)));
        }
    }

    /// <summary>**6: the CQ list the achievements page is handed, every call verbatim.**</summary>
    private void ItemSix()
    {
        Head("6. THE CQ LIST - CqSnapshot.From over an FT8 CQ, a PSK31 CQ and an Olivia CQ");

        var corpus = Psk31Corpus.Load();

        var ft8Panel = SlottedPanel(corpus.Operator, "FT8");
        var slot = new DateTime(2026, 9, 19, 21, 41, 30, DateTimeKind.Utc);

        ft8Panel.AddDecodeRowForTests("214130", "-12", "0.2", "1240", "CQ IK4LZH JN54", slot, 14_074_000);

        var psk31Panel = Psk31Panel(corpus.Operator);

        psk31Panel.ShowPsk31ChannelsForTests([new Psk31Channel(1, Psk31Hz, 10.0, CqFrom(OnPsk31))]);

        var oliviaPanel = OliviaPanel(corpus.Operator);

        oliviaPanel.ShowOliviaChannelsForTests(
            [OliviaChannelOf(68, Variant, CallingOffsetOn(oliviaPanel), CqFrom(OnOlivia))]);

        var rows = ft8Panel.DigitalDecodes
            .Concat(psk31Panel.DigitalDecodes)
            .Concat(oliviaPanel.DigitalDecodes)
            .ToList();

        Line("the decoded rows handed in: " + rows.Count);

        foreach (var row in rows)
        {
            Line("  sender=\"" + row.Sender + "\" addressee=\"" + row.Addressee
                + "\" IsTextOnly=" + row.IsTextOnly + " Variant=\"" + row.Variant + "\"");
        }

        var snapshot = CqSnapshot.From(rows, new DateTime(2026, 9, 21, 19, 0, 0, DateTimeKind.Utc));

        Line("");
        Line("CqSnapshot.From gave " + snapshot.Calls.Count + " calls, read at " + snapshot.ReadAt + ":");

        foreach (var call in snapshot.Calls)
        {
            Line("  Callsign=\"" + call.Callsign + "\" Grid=\"" + call.Grid
                + "\" HeardUtc=\"" + call.HeardUtc + "\" Mode=\"" + call.Mode + "\"");
        }
    }

    /// <summary>**7: the page's eight scores and the total, before, three columns.**</summary>
    private void ItemSeven(AchievementPoints points)
    {
        Head("7. THE PAGE'S TOTALS, BEFORE - an AchievementsViewModel built the way OpenAchievements builds it");

        Line(Pad("badge", 14) + Cols());

        var pages = Columns.ToDictionary(
            c => c.Name,
            c => new AchievementsViewModel([RecordOf(c.Mode)], MyGrid, points).Page!);

        foreach (var kind in AchievementKinds.All)
        {
            Row(
                kind,
                Columns.Select(c => pages[c.Name].Scores.For(kind).Points is { } p
                    ? p.ToString(CultureInfo.InvariantCulture)
                    : "null").ToList(),
                14);
        }

        Line("");

        foreach (var (name, _) in Columns)
        {
            Line("  " + Pad(name, 8) + "TotalLine \"" + pages[name].TotalLine + "\"");
        }
    }

    /// <summary>Every field of the `AdifContact` the log record carries.</summary>
    private static IReadOnlyList<string> FieldNames { get; } =
    [
        "Call", "StationCallsign", "StartedUtc", "EndedUtc", "Band", "FrequencyMhz",
        "Mode", "Submode", "ReportSent", "ReportReceived", "RstSent", "RstReceived",
        "GridSquare", "MyGridSquare", "State", "Comment",
    ];

    private static string? FieldOf(AdifContact? contact, string field)
    {
        if (contact is null)
        {
            return null;
        }

        return field switch
        {
            "Call" => contact.Call,
            "StationCallsign" => contact.StationCallsign,
            "StartedUtc" => contact.StartedUtc?.ToString("s", CultureInfo.InvariantCulture),
            "EndedUtc" => contact.EndedUtc?.ToString("s", CultureInfo.InvariantCulture),
            "Band" => contact.Band,
            "FrequencyMhz" => contact.FrequencyMhz?.ToString("0.000000", CultureInfo.InvariantCulture),
            "Mode" => contact.Mode,
            "Submode" => contact.Submode,
            "ReportSent" => contact.ReportSent,
            "ReportReceived" => contact.ReportReceived,
            "RstSent" => contact.RstSent,
            "RstReceived" => contact.RstReceived,
            "GridSquare" => contact.GridSquare,
            "MyGridSquare" => contact.MyGridSquare,
            "State" => contact.State,
            _ => contact.Comment,
        };
    }

    /// <summary>One log holding exactly the one contact, in that mode and nothing else.</summary>
    private static AchievementLog LogOf(ContactMode mode) => new([RecordOf(mode)], MyGrid);

    /// <summary>
    /// **The one record, whose only variable is the mode pair.** Every other field is the same
    /// literal in all three columns, so a difference downstream can only be the mode's.
    /// </summary>
    private static AdifLogRecord RecordOf(ContactMode mode) => RecordOf(mode, His, HisGrid);

    /// <summary>The same record for another station, where a second entity is wanted.</summary>
    private static AdifLogRecord RecordOf(ContactMode mode, string call, string grid)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = "20m",
                Mode = mode.AdifMode,
                Submode = mode.AdifSubmode,
                RstSent = "599",
                RstReceived = "599",
                GridSquare = grid,
                MyGridSquare = MyGrid,
                StartedUtc = new DateTime(2026, 9, 19, 21, 0, 0, DateTimeKind.Utc),
                EndedUtc = new DateTime(2026, 9, 19, 21, 12, 0, DateTimeKind.Utc),
            },
            [],
            true);

    /// <summary>A call to anybody, in the words a keyboard operator sends one in.</summary>
    private static string CqFrom(string who)
        => "CQ CQ CQ de " + who + " " + who + " " + who + " pse k\n";

    /// <summary>An Olivia channel as the listener would list it.</summary>
    private static OliviaChannel OliviaChannelOf(int id, string variant, double centerHz, string text)
        => new(id, variant, centerHz, OliviaListener.FoundByRsid, 0, text, text.Length > 0 ? 9 : 0, 0, false, centerHz);

    /// <summary>Where the band's Olivia calling center sits, by the cited table's own arithmetic.</summary>
    private static double CallingOffsetOn(MainWindowViewModel model)
    {
        var row = OliviaData.Current.Calling!.CallingRowFor(model.SelectedBand.Band.Name)!;

        return row.CenterHz - (double)model.FrequencyHz;
    }

    /// <summary>Works the textbook exchange to 73 on the Olivia calling spot, a press at a time.</summary>
    private static void WorkOlivia(MainWindowViewModel model, Psk31Corpus corpus)
    {
        var calling = CallingOffsetOn(model);
        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, OnOlivia, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        var heard = "";

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            model.ShowOliviaChannelsForTests([OliviaChannelOf(68, Variant, calling, heard)]);

            if (over == 0)
            {
                model.AnswerPsk31Command.Execute(
                    model.DigitalDecodes.First(r => r.Sender == OnOlivia));
            }
            else
            {
                model.CardActionCommand.Execute(
                    model.DigitalCards.First(c => c.Callsign == OnOlivia));
            }

            Settle(model);
        }
    }

    /// <summary>
    /// **The same textbook exchange `WorkOlivia` replays, down the PSK31 panel instead.**
    /// </summary>
    /// <remarks>
    /// The words, the station and the number of overs are identical to the Olivia run, so item 3's
    /// two keyboard columns differ in the mode and in nothing else.
    /// </remarks>
    private static void WorkTranscriptOnPsk31(MainWindowViewModel model, Psk31Corpus corpus)
    {
        var his = corpus.Transcripts.Single(t => t.Name == Textbook).Lines
            .Where(l => string.Equals(l.Frm, OnOlivia, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Text + "\n")
            .ToList();

        var heard = "";

        for (var over = 0; over < his.Count; over++)
        {
            heard += his[over];

            model.ShowPsk31ChannelsForTests([new Psk31Channel(1, Psk31Hz, 10.0, heard)]);

            if (over == 0)
            {
                model.AnswerPsk31Command.Execute(
                    model.DigitalDecodes.First(r => r.Sender == OnOlivia));
            }
            else
            {
                model.CardActionCommand.Execute(
                    model.DigitalCards.First(c => c.Callsign == OnOlivia));
            }

            Settle(model);
        }
    }

    private static MainWindowViewModel OliviaPanel(string mine)
    {
        var model = Panel(mine);

        model.TapForTests = new AudioTap();
        model.ChooseDigitalModeCommand.Execute("Olivia");

        return model;
    }

    private static MainWindowViewModel Psk31Panel(string mine)
    {
        var model = Panel(mine);

        model.ChooseDigitalModeCommand.Execute("PSK31");

        return model;
    }

    private static MainWindowViewModel SlottedPanel(string mine, string mode)
    {
        var model = Panel(mine);

        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    private static MainWindowViewModel Panel(string mine)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = mine;
        settings.Operator.GridSquare = MyGrid;
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Trafford PA";
        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.SelectedBand = model.Bands.First(b => b.Band.LowHz <= DialOn20m && b.Band.HighHz >= DialOn20m);
        model.FrequencyHz = DialOn20m;
        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.UseRigPortForTests(new FakePort());
        model.UseArmedSendForTests(new Ft8ArmedSend(new Ft8TransmitSequence(new FakePort(), new FakeSink())));

        return model;
    }

    /// <summary>Wait for the send the press started, and run what it posted to the UI thread.</summary>
    private static void Settle(MainWindowViewModel model)
    {
        for (var tries = 0; tries < 2000 && model.HasSomethingToStop; tries++)
        {
            System.Threading.Thread.Sleep(10);
        }

        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    private static string Cols()
        => Pad("FT8", 16) + Pad("PSK31", 16) + "Olivia";

    private static string Show(string? value) => value ?? "null";

    private static string Pad(string value, int width)
        => value.Length >= width ? value + "  " : value.PadRight(width);

    private void Row(string label, IReadOnlyList<string> cells, int width = 12)
        => Line(Pad(label, width) + Pad(cells[0], 16) + Pad(cells[1], 16) + cells[2]);

    private void Head(string title)
    {
        Line("");
        Line(new string('=', 78));
        Line(title);
        Line(new string('=', 78));
    }

    private void Line(string text) => _output.WriteLine(text);
}
