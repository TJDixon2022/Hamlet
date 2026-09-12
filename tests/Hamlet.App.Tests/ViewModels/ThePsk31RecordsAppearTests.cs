using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 326 task 5, rewritten by 333 task 2: **the PSK31 records are not on the
/// achievements window until he works one, and they are there the moment he does.**
/// </summary>
/// <remarks>
/// <para>**§3.1: ABSENT, NOT DIMMED.** A mode's records do not exist until he has worked
/// that mode - no ghost cards, no grayed placeholder, nothing to read at all.</para>
/// <para>**§3.2: THE UNLOCK REVEALS MORE THAN IT FILLS.** One contact in, the mode's
/// cards out.</para>
/// <para>**§4: COUNTS SAY WORKED AND NEVER CONFIRMED.**</para>
/// <para>**REWRITTEN UNDER §R12 ONTO THE WINDOW AS DRAWN** (work instruction 333 task 2).
/// Until 333 these tests read <see cref="AchievementScreen"/>, which units 330 to 332
/// replaced on the glass with eight badges that click in. The old screen still said
/// *no PSK31* while the window drew **A PSK31 contact** as Hall of Fame's next card, on
/// the page and inside the category, on any log that had earned a DX contact. So the
/// window is stood up headless here, the page and every kind are opened in turn, and
/// every visible run and hover is read.</para>
/// </remarks>
public sealed class ThePsk31RecordsAppearTests : IDisposable
{
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the window is printed.</param>
    /// <remarks>
    /// **THE OPERATOR'S FOLDER IS NOT OURS.** Announcing an opening writes down what
    /// it announced, through `SettingsStore.Save`, so a test that drives the reveal
    /// without redirecting the folder writes into his own settings file.
    /// </remarks>
    public ThePsk31RecordsAppearTests(ITestOutputHelper output)
    {
        _output = output;

        _wasFolder = SettingsStore.DataFolder;
        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit326-reveal-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
        SettingsStore.DataFolder = _folder;
    }

    /// <summary>Takes the scratch folder away again.</summary>
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

    /// <summary>**1: with no PSK31 contact, nothing PSK31 is on the window at all.**</summary>
    /// <remarks>
    /// **THE EVENING HAS A DX CONTACT IN IT ON PURPOSE.** With `first_contact` and
    /// `first_dx` earned, the nearest unearned Hall of Fame first in the list's order is
    /// `first_psk31`, which is the slot that drew *A PSK31 contact* before this unit.
    /// </remarks>
    [AvaloniaFact]
    public void WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed()
    {
        var drawn = Walk(EveningWithDx(), null);

        foreach (var (where, words) in drawn)
        {
            _output.WriteLine(where + ": " + words.Count + " runs, naming PSK "
                + words.Count(w => w.Contains("PSK", StringComparison.OrdinalIgnoreCase)));
        }

        // **EVERY WORD, NOT AN `Earned` FLAG.** A dimmed card is a visible card.
        foreach (var (where, words) in drawn)
        {
            var named = words.FirstOrDefault(
                w => w.Contains("PSK", StringComparison.OrdinalIgnoreCase));

            Assert.True(named is null, where + " draws [" + named + "] before any PSK31 contact");
        }
    }

    /// <summary>**2: the first PSK31 contact reveals the mode's cards, drawn at full ink.**</summary>
    [AvaloniaFact]
    public void TheFirstPsk31ContactRevealsTheModesRecords()
    {
        var cards = new Dictionary<string, IReadOnlyList<AchievementCategoryCard>>(StringComparer.Ordinal);

        var drawn = Walk(
            EveningWithDx().Append(Psk31Contact()).ToList(),
            (kind, model) => cards[kind] = model.Category!.Cards);

        var revealed = cards
            .SelectMany(k => k.Value.Select(c => (Kind: k.Key, Card: c)))
            .Where(x => x.Card.Title.Contains("PSK31", StringComparison.Ordinal))
            .ToList();

        foreach (var (kind, card) in revealed)
        {
            _output.WriteLine(kind + "  " + card.Title + " | " + card.Figure + " | "
                + card.PointsLine + " | earned " + card.Earned + " | opacity " + card.CardOpacity);
        }

        var first = Assert.Single(revealed, x => x.Kind == AchievementKinds.HallOfFame);
        var mode = Assert.Single(revealed, x => x.Kind == AchievementKinds.Modes);

        Assert.Equal("A PSK31 contact", first.Card.Title);
        Assert.Equal("PSK31", mode.Card.Title);

        // **ABSENT OR THERE, NEVER DIMMED**: both are held and drawn at full ink.
        Assert.All(revealed, x => Assert.True(x.Card.Earned, x.Card.Title + " is not earned"));
        Assert.All(revealed, x => Assert.Equal(1.0, x.Card.CardOpacity));

        // And they are on the glass, not only in the model.
        Assert.Contains("A PSK31 contact", drawn[AchievementKinds.HallOfFame]);
        Assert.Contains("PSK31", drawn[AchievementKinds.Modes]);

        // **AND FT8 IS STILL THERE**, unchanged by any of this.
        Assert.Contains("FT8", drawn[AchievementKinds.Modes]);
    }

    /// <summary>**3: the window says worked, and never confirmed.**</summary>
    /// <remarks>
    /// **§4 OF `ACHIEVEMENTS_PHILOSOPHY.md`.** The old screen carried one disclaimer
    /// sentence with the word in it; the click-in window carries none, so the word does
    /// not appear anywhere it draws.
    /// </remarks>
    [AvaloniaFact]
    public void TheWordingSaysWorkedAndNeverConfirmed()
    {
        var drawn = Walk(EveningWithDx().Append(Psk31Contact()).ToList(), null);

        var everything = string.Join(" | ", drawn.SelectMany(d => d.Value));

        Assert.DoesNotContain("confirm", everything, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("worked", everything, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**4: the reveal writes its event once, with the count and nothing else.**</summary>
    [Fact]
    public void TheRevealWritesItsEventOnceWithTheCountAndNothingPersonal()
    {
        var lines = WithTelemetry(model =>
        {
            // **THE QUIET FIRST LOOK**, which writes down what was already open and
            // announces nothing - a man who imports a log gets no notices.
            model.AnnounceOpeningsForTests(EveningWithDx());

            // Then he works one.
            model.AnnounceOpeningsForTests(EveningWithDx().Append(Psk31Contact()).ToList());

            // **AND READING THE SAME LOG AGAIN SAYS NOTHING MORE.**
            model.AnnounceOpeningsForTests(EveningWithDx().Append(Psk31Contact()).ToList());
        });

        var revealed = Assert.Single(Events(lines, "psk31_records_revealed"));

        _output.WriteLine(revealed.ToString());

        // **THE COUNT IS WHAT THE WINDOW DRAWS** (work instruction 333 task 3): Hall of
        // Fame's *A PSK31 contact* and the Modes card *PSK31*, as test 2 reads them off the
        // glass. Until 333 it was the four records of a screen the window no longer shows.
        Assert.Equal(2, revealed.GetProperty("data").GetProperty("records").GetInt32());

        var text = revealed.ToString();

        foreach (var personal in new[] { "G4XYZ", "LA8ENA", "KC3QIS", "FN00", "IO91", "JO59" })
        {
            Assert.DoesNotContain(personal, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// **The window over a log: the page, then every kind opened and closed in turn.**
    /// </summary>
    /// <param name="records">The log.</param>
    /// <param name="inside">Called with each kind while it is open, or null.</param>
    /// <returns>Every visible run and hover, by `page` or by kind.</returns>
    private static Dictionary<string, List<string>> Walk(
        IReadOnlyList<AdifLogRecord> records, Action<string, AchievementsViewModel>? inside)
    {
        var model = new AchievementsViewModel(
            records, HisGrid, AchievementPoints.Parse(AchievementPoints.Shipped()));

        var window = new AchievementsWindow { DataContext = model };
        var drawn = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        window.Show();
        Settle(window);

        try
        {
            Assert.True(model.ShowsPage, "the window did not open on the page");

            drawn["page"] = Words(window);

            foreach (var kind in AchievementKinds.All)
            {
                model.OpenCategoryCommand.Execute(kind);
                Settle(window);

                Assert.True(model.ShowsCategory, kind + " did not open");

                drawn[kind] = Words(window);
                inside?.Invoke(kind, model);

                model.BackCommand.Execute(null);
                Settle(window);
            }
        }
        finally
        {
            window.Close();
        }

        return drawn;
    }

    /// <summary>Every visible run of text and every hover the window holds.</summary>
    private static List<string> Words(Window window)
    {
        var runs = window.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0)
            .Select(t => t.Text!);

        var tips = window.GetVisualDescendants().OfType<Control>()
            .Where(c => c.IsEffectivelyVisible)
            .Select(c => ToolTip.GetTip(c) as string)
            .Where(s => !string.IsNullOrEmpty(s))
            .Select(s => s!);

        return runs.Concat(tips).ToList();
    }

    private static void Settle(Window window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    private List<string> WithTelemetry(Action<MainWindowViewModel> what)
    {
        using (var telemetry = new JsonlTelemetry(_folder, "326", _ => true))
        {
            var settings = new AppSettings { ReconnectOnStartup = false };

            settings.Operator.Callsign = "KC3QIS";
            settings.Operator.GridSquare = HisGrid;

            what(new MainWindowViewModel(settings, telemetry));
        }

        return Directory.GetFiles(_folder, "*.jsonl").SelectMany(File.ReadAllLines).ToList();
    }

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    /// <summary>
    /// An evening of FT8 with one DX contact in it, and no PSK31 anywhere.
    /// </summary>
    private static List<AdifLogRecord> EveningWithDx()
        => Enumerable.Range(0, 6)
            .Select(i => Record(
                "W" + (i + 1) + "ABC", "20m", "FT8", null, "FN42",
                "+00", "-12",
                "2026-09-10 02:" + i.ToString("00", CultureInfo.InvariantCulture) + ":00"))
            .Append(Record(
                "LA8ENA", "20m", "FT8", null, "JO59", "-05", "-10", "2026-09-10 02:10:00"))
            .ToList();

    /// <summary>One PSK31 contact, spelled the way the export spells it.</summary>
    /// <remarks>
    /// **THE REPORT IS AN RST AND SITS IN THE RST FIELDS** (§3.2). The decibel fields are
    /// empty - a `599` is not a ratio and must never be sorted as one.
    /// </remarks>
    private static AdifLogRecord Psk31Contact()
        => Record(
            "G4XYZ", "20m", "PSK", "PSK31", "IO91",
            null, null, "2026-09-11 21:14:00", "599", "589");

    private static AdifLogRecord Record(
        string call, string band, string mode, string? submode, string? grid,
        string? sent, string? received, string startedUtc,
        string? rstSent = null, string? rstReceived = null)
        => new(
            new AdifContact
            {
                Call = call,
                StationCallsign = "KC3QIS",
                Band = band,
                Mode = mode,
                Submode = submode,
                GridSquare = grid,
                MyGridSquare = HisGrid,
                ReportSent = sent,
                ReportReceived = received,
                RstSent = rstSent,
                RstReceived = rstReceived,
                StartedUtc = DateTime.Parse(
                    startedUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal),
            },
            Array.Empty<string>(),
            Terminated: true);
}
