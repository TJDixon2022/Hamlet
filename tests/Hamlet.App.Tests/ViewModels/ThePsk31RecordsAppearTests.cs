using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 326 task 5: **the PSK31 records are not on the achievements screen
/// until he works one, and they are there the moment he does.**
/// </summary>
/// <remarks>
/// <para>**§3.1: ABSENT, NOT DIMMED.** A mode's records do not exist until he has worked
/// that mode - no ghost cards, no grayed placeholder, nothing to read at all. A dimmed
/// card is a list of things he has not done, and *he has had enough of those* (§4).</para>
/// <para>**§3.2: THE UNLOCK REVEALS MORE THAN IT FILLS.** One contact in, a scope of
/// records out.</para>
/// <para>**§4: COUNTS SAY WORKED AND NEVER CONFIRMED.** A confirmation is somebody
/// else's word and Hamlet has not got one.</para>
/// <para>**COMPUTED, NOT SEEN.** The screen is built as a view model and read; no window
/// is opened here.</para>
/// </remarks>
public sealed class ThePsk31RecordsAppearTests : IDisposable
{
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;
    private readonly string _wasFolder;

    /// <summary>Creates the tests and redirects the data folder.</summary>
    /// <param name="output">Where the screen is printed.</param>
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

    /// <summary>**1: with no PSK31 contact, nothing PSK31 is on the screen at all.**</summary>
    [Fact]
    public void WithNoPsk31ContactNoPsk31CardIsOnTheScreenAndNoneIsDimmed()
    {
        var screen = Screen(Ft8Evening());

        foreach (var scope in screen.Scopes)
        {
            _output.WriteLine(scope.Key + "  " + scope.Title + "  " + scope.Summary);
        }

        Assert.DoesNotContain(
            screen.Scopes, s => s.Key.Contains("PSK", StringComparison.OrdinalIgnoreCase));

        // **AND NOT ANYWHERE ELSE ON IT EITHER, IN ANY STATE.** A dimmed card is a
        // visible card, so the assertion is over every word the screen holds rather
        // than over an `Earned` flag somewhere.
        Assert.DoesNotContain("PSK", Everything(screen), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**2: the first PSK31 contact reveals the mode's records.**</summary>
    [Fact]
    public void TheFirstPsk31ContactRevealsTheModesRecords()
    {
        var screen = Screen(Ft8Evening().Append(Psk31Contact()).ToList());

        var psk31 = screen.Scopes.SingleOrDefault(
            s => string.Equals(s.Key, "mode-PSK31", StringComparison.Ordinal));

        Assert.True(psk31 is not null, "the first PSK31 contact revealed no records");

        _output.WriteLine(psk31!.Key + "  " + psk31.Title + "  " + psk31.Summary);

        foreach (var group in psk31.Groups)
        {
            _output.WriteLine("  " + group.Title);

            foreach (var card in group.Cards)
            {
                _output.WriteLine("    " + card.Title + " - " + card.Detail);
            }
        }

        Assert.Equal("PSK31", psk31.Title);
        Assert.True(psk31.Count > 0, "the scope opened with no records in it");

        // **AND THE FT8 SCOPE IS STILL THERE**, unchanged by any of this.
        Assert.Contains(
            screen.Scopes, s => string.Equals(s.Key, "mode-FT8", StringComparison.Ordinal));
    }

    /// <summary>**3: the revealed records say worked, and never confirmed.**</summary>
    /// <remarks>
    /// <para>**§4 OF `ACHIEVEMENTS_PHILOSOPHY.md`.** DXCC and Worked All States are
    /// counted by confirmations, and Hamlet knows only what passed on the air.</para>
    /// <para>**THE WHOLE-SCREEN SWEEP IS NOT `DoesNotContain("confirm")`**, and that
    /// is the finding rather than a softening: the entity cards already carry *the
    /// count says worked rather than confirmed*, which is §4 being obeyed out loud.
    /// So the assertion is that the word appears **only** inside that disclaimer, and
    /// nowhere at all among the mode's own records.</para>
    /// </remarks>
    [Fact]
    public void TheWordingSaysWorkedAndNeverConfirmed()
    {
        var screen = Screen(Ft8Evening().Append(Psk31Contact()).ToList());

        var psk31 = string.Join(
            " | ",
            screen.Scopes
                .Where(s => string.Equals(s.Key, "mode-PSK31", StringComparison.Ordinal))
                .SelectMany(s => s.Groups.SelectMany(g => g.Cards.SelectMany(Words))));

        _output.WriteLine(psk31);

        Assert.DoesNotContain("confirm", psk31, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("worked", psk31, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PSK31", psk31, StringComparison.Ordinal);

        // **AND NOWHERE ON THE SCREEN DOES THE WORD STAND ALONE.** Take the one
        // sentence that refuses the claim out, and the word is gone from the screen.
        const string Disclaimer =
            "The count says worked rather than confirmed: the DXCC award is counted "
            + "from confirmations, on paper or electronic, and Hamlet only knows what "
            + "passed on the air from your own log.";

        var everything = Everything(screen);

        Assert.Contains(Disclaimer, everything, StringComparison.Ordinal);

        Assert.DoesNotContain(
            "confirm",
            everything.Replace(Disclaimer, " ", StringComparison.Ordinal),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**4: the reveal writes its event once, with the count and nothing else.**</summary>
    [Fact]
    public void TheRevealWritesItsEventOnceWithTheCountAndNothingPersonal()
    {
        var lines = WithTelemetry(model =>
        {
            // **THE QUIET FIRST LOOK**, which writes down what was already open and
            // announces nothing - a man who imports a log gets no notices.
            model.AnnounceOpeningsForTests(Ft8Evening());

            // Then he works one.
            model.AnnounceOpeningsForTests(Ft8Evening().Append(Psk31Contact()).ToList());

            // **AND READING THE SAME LOG AGAIN SAYS NOTHING MORE.**
            model.AnnounceOpeningsForTests(Ft8Evening().Append(Psk31Contact()).ToList());
        });

        var revealed = Assert.Single(Events(lines, "psk31_records_revealed"));

        _output.WriteLine(revealed.ToString());

        Assert.True(revealed.GetProperty("data").GetProperty("records").GetInt32() > 0);

        var text = revealed.ToString();

        foreach (var personal in new[] { "G4XYZ", "KC3QIS", "FN00", "IO91" })
        {
            Assert.DoesNotContain(personal, text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Every word the screen would show, for an absence assertion.</summary>
    private static string Everything(AchievementScreen screen)
        => string.Join(
            " | ",
            screen.Scopes.SelectMany(
                s => new[] { s.Key, s.Title, s.Kind, s.Summary }
                    .Concat(s.Groups.Select(g => g.Title))
                    .Concat(s.Groups.SelectMany(g => g.Cards.SelectMany(Words))))
            .Concat(screen.Places.SelectMany(
                p => new[] { p.Title }.Concat(p.Cards.SelectMany(Words))))
            .Concat(screen.Challenges.SelectMany(Words)));

    /// <summary>Every word one card would put on the screen.</summary>
    private static IEnumerable<string> Words(AchievementCard card)
        => new[]
        {
            card.Key, card.Title, card.Figure, card.Station, card.Detail,
            card.Progress, card.Target, card.Under, card.RingMeans,
        };

    private static AchievementScreen Screen(IReadOnlyList<AdifLogRecord> records)
        => new(new AchievementLog(records, HisGrid));

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

    /// <summary>An evening of FT8, with no PSK31 in it anywhere.</summary>
    private static List<AdifLogRecord> Ft8Evening()
        => Enumerable.Range(0, 6)
            .Select(i => Record(
                "W" + (i + 1) + "ABC", "20m", "FT8", null, "FN42",
                "+00", "-12",
                "2026-09-10 02:" + i.ToString("00", CultureInfo.InvariantCulture) + ":00"))
            .ToList();

    /// <summary>One PSK31 contact, spelled the way the export spells it.</summary>
    /// <remarks>
    /// **THE REPORT IS AN RST AND SITS IN THE RST FIELDS** (task 3, §3.2). The decibel
    /// fields are empty, which is why no *how faint* record appears for this mode - a
    /// `599` is not a ratio and must never be sorted as one.
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
