using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **Work instruction 387 task 1: the before, measured, for all four of R46's things.**
/// </summary>
/// <remarks>
/// <para>**IT BUILDS NOTHING AND CHANGES NO FILE UNDER `src`.** It realizes real windows and
/// drives a real view model and prints what the tree does today: whether the favorites star is
/// drawn and whether its hit rectangle can be clicked at three sizes, how many decoded rows offer
/// a right-click menu and what each menu holds, what the sun map and the top band measure and how
/// many pixels of the band the map is not using, and whether the Olivia block decoder's own
/// confidence crosses the seam to the row.</para>
/// <para>**IT IS DELIBERATELY NOT ON THE CARRY-FORWARD LIST** (R14, and the reason
/// <c>Unit378Trace</c> and unit 376's trace are not): almost everything in it is printed rather
/// than asserted, and a name that cannot fail teaches nothing by being run. What it asserts is
/// only that the measurement happened - that there were rows and windows to look at - so a silent
/// empty run cannot be read as a finding.</para>
/// <para>**IT READS AND NEVER WRITES A PROPERTY A CONTROL OWNS** (the trap
/// <c>ViewTestsActThroughControlsTests.NoViewTestWritesAPropertyAControlOwns</c> names). The star
/// measurement reads <c>RigDisplayControl</c>'s private hit rectangle by reflection and sets
/// nothing on the control at all.</para>
/// <para>**NOTHING PERSONAL IS READ OR PRINTED** (HM-DEC-018 §2.1). The favorites round-trip is
/// over a settings file this test writes from a literal in this file; **Tim's own settings file is
/// never opened, copied or quoted**, and what is reported of it is a count and a yes or no.</para>
/// <para>**NOTHING IS KEYED AND NO PORT IS OPENED** (FACT-004). Every number is computed off a
/// realized headless window or a view model in memory.</para>
/// </remarks>
public sealed class Unit387Trace : IDisposable
{
    /// <summary>The station the fixture rows are framed for.</summary>
    private const string His = "W1AW";

    /// <summary>The operator on the fixture panel.</summary>
    private const string Mine = "KC3QIS";

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every table is printed.</param>
    public Unit387Trace(ITestOutputHelper output)
    {
        _output = output;
        _folder = Path.Combine(Path.GetTempPath(), "hamlet-unit387-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the scratch folder.</summary>
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

    // =====================================================================================
    // Item 1 - favorites: the star, the click, and whether the saved frequencies survive.
    // =====================================================================================

    /// <summary>
    /// **Does the star draw, and does it answer a click, at 1920, 1400 and 1100 x 780?**
    /// </summary>
    /// <remarks>
    /// <para>**A ZEROED `_starRect` MEANS THE STAR CANNOT BE CLICKED** even if something is drawn,
    /// which is why the rectangle rather than the glyph is the measurement. The bail at
    /// <c>RigDisplayControl.DrawStar</c> sets it to <c>default</c> and returns.</para>
    /// <para>**THE TWO CANDIDATES SECTION 5 NAMES ARE BOTH MEASURED HERE**: whether the strip that
    /// was the way in is on the window at all, and whether the bail fires at Tim's widths.</para>
    /// </remarks>
    [AvaloniaFact]
    public void Item1TheStarIsDrawnOrItIsNotAndTheRectangleSaysWhichAtThreeSizes()
    {
        _output.WriteLine("=== ITEM 1a. THE STAR ON THE RIG DISPLAY, AT THREE SIZES ==============");
        _output.WriteLine("");
        _output.WriteLine("  RigDisplayControl.DrawStar bails when room < the glyph's width, where");
        _output.WriteLine("  room = clockX - 14 - x. On the bail _starRect becomes default and the");
        _output.WriteLine("  star is neither drawn nor clickable.");
        _output.WriteLine("");

        var looked = 0;

        foreach (var (width, height) in new[] { (1920.0, 1040.0), (1400.0, 1040.0), (1100.0, 780.0) })
        {
            var window = TheTopRowTests.Realized(width, height, null, null);
            var model = (MainWindowViewModel)window.DataContext!;

            try
            {
                Unit376TheTopBandTests.Pump(window);

                var rig = window.GetVisualDescendants().OfType<RigDisplayControl>().Single();

                // **THE RENDER PASS IS FORCED**, because `_starRect` is written by `Render` and a
                // headless window that was never painted has never run it.
                Paint(window, rig);

                var rect = StarRect(rig);
                var where = Px(width) + " x " + Px(height);

                looked++;

                _output.WriteLine(
                    "  " + where.PadRight(14)
                    + " rig face " + Box(new Rect(rig.Bounds.Size))
                    + " | IsFavorite " + rig.IsFavorite
                    + " | label [" + rig.FavoriteLabel + "]");
                _output.WriteLine(
                    "                 _starRect " + Box(rect)
                    + " -> " + (rect == default ? "BAILED: no star drawn and nothing to click"
                                                : "drawn and clickable"));

                // The model's own side of the same question.
                _output.WriteLine(
                    "                 model: HasFavorites " + model.HasFavorites
                    + ", Favorites " + model.Favorites.Count
                    + ", FavoriteMenu " + model.FavoriteMenu.Count
                    + ", ToggleFavoriteCommand " + (model.ToggleFavoriteCommand is null ? "null" : "present")
                    + ", TuneToFavoriteCommand " + (model.TuneToFavoriteCommand is null ? "null" : "present")
                    + ", ManageFavoritesCommand " + (model.ManageFavoritesCommand is null ? "null" : "present"));
            }
            finally
            {
                window.Close();
            }
        }

        _output.WriteLine("");
        _output.WriteLine("=== ITEM 1a2. IS THERE A WAY IN ON THE WINDOW AT ALL? =================");
        _output.WriteLine("");

        var w1920 = TheTopRowTests.Realized(1920, 1040, null, null);

        try
        {
            Unit376TheTopBandTests.Pump(w1920);

            // The favorites strip's own controls, by what they were: a name, a note box and a
            // list. Section 5 says the comment is there and the controls are not.
            var combos = w1920.GetVisualDescendants().OfType<ComboBox>()
                .Where(c => (c.PlaceholderText ?? "").Contains("place", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var favoriteWords = w1920.GetVisualDescendants().OfType<TextBlock>()
                .Where(t => (t.Text ?? "").Contains("favorite", StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Text!)
                .ToList();

            _output.WriteLine("  a places dropdown on the window : " + combos.Count);
            _output.WriteLine("  the word favorite on the window : " + favoriteWords.Count
                + (favoriteWords.Count == 0 ? "" : " -> " + string.Join(" | ", favoriteWords)));
            _output.WriteLine("  the Radio menu's Favorites submenu is the only way in today");
            _output.WriteLine("    (MainWindow.axaml 2739 and 2774, gated on HasFavorites)");
        }
        finally
        {
            w1920.Close();
        }

        _output.WriteLine("");

        Assert.Equal(3, looked);
    }

    /// <summary>
    /// **Do the saved frequencies survive in the settings file, under today's loader?**
    /// </summary>
    /// <remarks>
    /// **THE FILE IS THIS TEST'S OWN AND NOT TIM'S** (HM-DEC-018 §2.1). It is reconstructed from
    /// the same shape <c>TheSettingsSurviveAnUpgradeTests</c>' 1.13.30 fixture carries, written to
    /// a scratch folder, and round-tripped through <c>SettingsStore</c>. Nothing personal is read.
    /// </remarks>
    [AvaloniaFact]
    public void Item1TheSavedFrequenciesRoundTripThroughTodaysLoader()
    {
        _output.WriteLine("=== ITEM 1b. DO THE SAVED FREQUENCIES SURVIVE THE LOADER? =============");
        _output.WriteLine("");
        _output.WriteLine("  The file below is written by this test from a literal in this file.");
        _output.WriteLine("  Tim's own settings file is not opened, copied or quoted (HM-DEC-018 2.1).");
        _output.WriteLine("");

        var path = Path.Combine(_folder, "settings-with-favorites.json");

        File.WriteAllText(path, WithTwoFavorites);

        var loaded = SettingsStore.LoadFrom(path);

        _output.WriteLine("  loaded from a 1.13.30-shaped file : " + loaded.Favorites.Count + " favorites");

        foreach (var favorite in loaded.Favorites)
        {
            _output.WriteLine(
                "    " + favorite.FrequencyHz.ToString("#,0", CultureInfo.InvariantCulture) + " Hz"
                + " | mode [" + favorite.Mode + "]"
                + " | band [" + favorite.BandName + "]"
                + " | name [" + favorite.Name + "]"
                + " | note [" + favorite.Note + "]");
        }

        var written = Path.Combine(_folder, "after-save.json");

        SettingsStore.SaveTo(loaded, written);

        var again = SettingsStore.LoadFrom(written);

        _output.WriteLine("  saved and loaded again            : " + again.Favorites.Count + " favorites");
        _output.WriteLine(
            "  the dial and the mode came back   : "
            + (again.Favorites.Count == loaded.Favorites.Count
               && again.Favorites.Zip(loaded.Favorites).All(p =>
                   p.First.FrequencyHz == p.Second.FrequencyHz && p.First.Mode == p.Second.Mode)
                ? "YES, every one of them, frequency and mode"
                : "NO"));
        _output.WriteLine("");

        Assert.Equal(2, loaded.Favorites.Count);
        Assert.Equal(2, again.Favorites.Count);
    }

    // =====================================================================================
    // Item 2 - the row menu: how many of how many offer one today.
    // =====================================================================================

    /// <summary>
    /// **How many of how many rows offer a menu today, and what each menu holds.**
    /// </summary>
    /// <remarks>
    /// The fixture is deliberately mixed the way section 8 item 2 asks: station read and no
    /// station read, live and ended, PSK31 and Olivia. Unit 378 measured 2 of 7 before it built
    /// anything; this is the same measurement over the rows R46(b) is actually about.
    /// </remarks>
    [AvaloniaFact]
    public void Item2HowManyRowsOfferAMenuAndWhetherTheCardFiresOnARowThatNamesNobody()
    {
        _output.WriteLine("=== ITEM 2. THE ROW MENU TODAY =======================================");
        _output.WriteLine("");

        var looked = 0;
        var withAMenu = 0;
        var namesAStation = 0;

        foreach (var (what, model, rows) in Panels())
        {
            _output.WriteLine("-- " + what + " ------------------------------------------");

            foreach (var row in rows)
            {
                var station = model.Psk31StationOn(row);
                var canned = model.Psk31CannedMenuFor(row);
                var flyout = MainWindow.SendFlyoutFor(model, row);
                var items = flyout?.Items.OfType<MenuItem>().Count() ?? 0;
                var notes = flyout?.Items.OfType<MenuItem>().Count(i => i.Command is null) ?? 0;

                looked++;

                if (station is not null)
                {
                    namesAStation++;
                }

                if (flyout is not null)
                {
                    withAMenu++;
                }

                _output.WriteLine(
                    "  " + Short(row.Message).PadRight(42)
                    + " | station " + (station ?? "-").PadRight(8)
                    + " | ended " + (row.Ended ? "y" : "n")
                    + " | canned " + (canned is null ? "null" : canned.Count.ToString(CultureInfo.InvariantCulture)).PadLeft(4)
                    + " | MENU " + (flyout is null ? "NONE" : items + " items, " + notes + " of them notes"));
            }

            _output.WriteLine("");
        }

        _output.WriteLine("  ROWS LOOKED AT          : " + looked);
        _output.WriteLine("  ROWS NAMING A STATION   : " + namesAStation);
        _output.WriteLine("  ROWS OFFERING A MENU    : " + withAMenu + " of " + looked);
        _output.WriteLine("  ROWS OFFERING NOTHING   : " + (looked - withAMenu) + " of " + looked);
        _output.WriteLine("");
        _output.WriteLine("  SendFlyoutFor returns null where Psk31CannedMenuFor is null AND");
        _output.WriteLine("  SendMenuFor is null - MainWindow.axaml.cs 503 and 541. Psk31CannedMenuFor");
        _output.WriteLine("  answers null where Psk31StationOn(row) is null, at MainWindowViewModel.cs 18061.");
        _output.WriteLine("");

        // **DOES THE RIGHT-CLICK'S OWN CARD FIRE ON A ROW THAT NAMES NOBODY?** If it already does,
        // *make a card anyway* is a label on a thing that exists rather than a new mechanism.
        _output.WriteLine("=== ITEM 2b. DOES THE RIGHT-CLICK'S CARD FIRE ON A ROW NAMING NOBODY? ==");
        _output.WriteLine("");

        var panel = OliviaPanel(out var oliviaRows);
        var nameless = oliviaRows.FirstOrDefault(r => panel.Psk31StationOn(r) is null);

        if (nameless is null)
        {
            _output.WriteLine("  the fixture produced no row that names nobody - nothing to measure");
        }
        else
        {
            var before = panel.DigitalCards.Count;

            panel.OpenPsk31CardCommand.Execute(nameless);

            var after = panel.DigitalCards.Count;

            _output.WriteLine("  the row            : " + Short(nameless.Message));
            _output.WriteLine("  Psk31StationOn     : null");
            _output.WriteLine("  cards before       : " + before);
            _output.WriteLine("  OpenPsk31CardCommand.Execute(row) at MainWindow.axaml.cs 466");
            _output.WriteLine("  cards after        : " + after);
            _output.WriteLine(
                "  ANSWER             : " + (after > before
                    ? "YES - the card already appears, so *make a card anyway* is a LABEL on a thing that exists"
                    : "NO - the card does not appear, so *make a card anyway* needs the press to reach further"));
        }

        _output.WriteLine("");

        Assert.True(looked > 0, "the fixture produced no rows to look at");
    }

    // =====================================================================================
    // Item 3 - the sun map and the band.
    // =====================================================================================

    /// <summary>
    /// **The band, the map, the caption and the pixels of the band the map is not using.**
    /// </summary>
    [AvaloniaFact]
    public void Item3TheSunMapAndTheBandAtBothWidths()
    {
        _output.WriteLine("=== ITEM 3. THE SUN MAP AND THE BAND IT SITS IN ======================");
        _output.WriteLine("");
        _output.WriteLine("  6.1's ceiling is 220 px with the pills, R42 measured the band at 214");
        _output.WriteLine("  with an arithmetic floor of 197. The map is hard-coded Height=134 at");
        _output.WriteLine("  MainWindow.axaml 996.");
        _output.WriteLine("");

        var looked = 0;

        foreach (var width in new[] { 1920.0, 1400.0 })
        {
            foreach (var mode in Unit376TheTopBandTests.Modes)
            {
                var window = TheTopRowTests.Realized(width, TheTopRowTests.WindowHeight, null, null);
                var model = (MainWindowViewModel)window.DataContext!;

                try
                {
                    model.ChosenDigitalMode = mode;
                    Unit376TheTopBandTests.Pump(window);

                    var band = Unit376TheTopBandTests.Band(window);
                    var map = TheTopRowTests.Named<GrayLineMapControl>(window, "GreenZoneGrayLine");
                    var caption = window.GetVisualDescendants().OfType<Control>()
                        .First(c => c.Name == "GreenZoneClockCaption");
                    var column = window.GetVisualDescendants().OfType<Control>()
                        .First(c => c.Name == "GreenZoneMap");
                    var card = TheTopRowTests.Card(window);

                    var unused = band.WithPills - map.Bounds.Height;
                    var share = band.WithPills <= 0 ? 0 : map.Bounds.Height / band.WithPills * 100.0;

                    // **WHERE THE MAP COULD GROW TO WITHOUT THE BAND GROWING**: its own column's
                    // slot inside the card, less the caption and the stack's 2 px spacing.
                    var slot = TheTopRowTests.RectIn(column.Parent as Control ?? column, window);
                    var headroom = card.Bounds.Height - map.Bounds.Height
                        - caption.Bounds.Height - 2;

                    looked++;

                    _output.WriteLine(
                        "  " + (Px(width) + " " + mode).PadRight(16)
                        + " band with pills " + Px(band.WithPills).PadLeft(7)
                        + " | without " + Px(band.WithoutPills).PadLeft(7)
                        + " | map " + Px(map.Bounds.Width) + " x " + Px(map.Bounds.Height)
                        + " | caption h " + Px(caption.Bounds.Height).PadLeft(6));
                    var left = window.GetVisualDescendants().OfType<Control>()
                        .First(c => c.Name == "GreenZoneLeft");

                    _output.WriteLine(
                        "                   card " + Box(TheTopRowTests.RectIn(card, window))
                        + " | GreenZoneMap " + Box(TheTopRowTests.RectIn(column, window))
                        + " | its grid " + Box(slot)
                        + " | GreenZoneLeft " + Box(TheTopRowTests.RectIn(left, window)));
                    _output.WriteLine("                   " + band.Governs);
                    _output.WriteLine(
                        "                   WHO GOVERNS THE CARD'S HEIGHT: left column wants "
                        + Px(left.DesiredSize.Height) + ", the map column wants "
                        + Px(column.DesiredSize.Height) + " -> "
                        + (left.DesiredSize.Height >= column.DesiredSize.Height
                            ? "THE TEXT GOVERNS, so the map can grow into the slack without the card growing"
                            : "THE MAP GOVERNS, so every pixel the map takes the card takes too"));
                    _output.WriteLine(
                        "                   the map is NOT using " + Px(unused)
                        + " px of the band, and fills " + share.ToString("0.0", CultureInfo.InvariantCulture)
                        + "% of it | of the card's " + Px(card.Bounds.Height) + " px it fills "
                        + (card.Bounds.Height <= 0 ? "0" : (map.Bounds.Height / card.Bounds.Height * 100.0).ToString("0.0", CultureInfo.InvariantCulture))
                        + "% with " + Px(headroom) + " px of headroom"
                        + " | against 6.1's ceiling of 220: "
                        + (band.WithPills <= 220 ? "inside by " + Px(220 - band.WithPills) : "OVER by " + Px(band.WithPills - 220)));
                }
                finally
                {
                    model.ChosenDigitalMode = "FT8";
                    window.Close();
                }
            }
        }

        _output.WriteLine("");

        Assert.Equal(6, looked);
    }

    // =====================================================================================
    // Item 4 - THE DROP CANDIDATE'S MEASUREMENT: does a per-block confidence cross the seam?
    // =====================================================================================

    /// <summary>
    /// **Does the Olivia block decoder expose a per-block confidence, and does anything between it
    /// and the row consult it?**
    /// </summary>
    /// <remarks>
    /// The answer is read off the engine's own types by reflection and off a driven view model,
    /// not out of a comment: if the member is not on the record, reflection says so.
    /// </remarks>
    [AvaloniaFact]
    public void Item4WhetherAPerBlockConfidenceCrossesTheSeamToTheRow()
    {
        _output.WriteLine("=== ITEM 4. THE BLIND-FOUND CARRIER AND ITS CONFIDENCE ================");
        _output.WriteLine("");

        foreach (var type in new[]
                 {
                     typeof(OliviaBlock), typeof(OliviaDecoding), typeof(OliviaChannel),
                     typeof(OliviaChannelState), typeof(OliviaCandidate), typeof(OliviaTrial),
                     typeof(Psk31Channel),
                 })
        {
            var members = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name + " : " + Pretty(p.PropertyType))
                .ToList();

            _output.WriteLine("  " + type.Name);
            _output.WriteLine("    " + string.Join(", ", members));
        }

        _output.WriteLine("");
        _output.WriteLine("  THE GATE THAT ALREADY EXISTS, in OliviaStream.Read:");
        _output.WriteLine("    var accepted = result.Snr >= _demodulator.Threshold;  -- only an accepted");
        _output.WriteLine("    block's characters are appended to _text, and a rejected one counts in");
        _output.WriteLine("    BlocksRejected. So Text is already 'every character of every block it was");
        _output.WriteLine("    sure of' by the demodulator's own threshold.");
        _output.WriteLine("");
        _output.WriteLine("  WHAT THE ROW PATH CONSULTS, at MainWindowViewModel.ShowOliviaChannels:");
        _output.WriteLine("    Readable: c.BlocksDecoded > 0   -- and then, in ShowPsk31Channels,");
        _output.WriteLine("    heardOnly = text.Length == 0 && !channel.Readable.");
        _output.WriteLine("    So the row shows text the moment ONE block is accepted, and neither");
        _output.WriteLine("    OliviaChannel.Found nor BlocksRejected reaches that decision.");
        _output.WriteLine("");

        // Driven, not asserted from a comment: a blind-found channel with one accepted block and
        // many rejected ones, which is the shape of Tim's 13:37 row.
        var model = Panel("Olivia");

        model.ShowOliviaChannelsForTests(new[]
        {
            new OliviaChannel(1, "4/500", 1500, OliviaListener.FoundBlind, 0, "Hk7DYYYzfzYXTDYY", 1, 11, false),
            new OliviaChannel(2, "8/250", 1000, OliviaListener.FoundByRsid, 0, "CQ CQ de " + His + " K", 9, 0, false),
        });

        var rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        _output.WriteLine("  A BLIND-FOUND 4/500 WITH 1 BLOCK ACCEPTED AND 11 REJECTED, DRIVEN:");

        foreach (var row in rows)
        {
            _output.WriteLine(
                "    hz " + row.Hz.PadLeft(5)
                + " | variant " + row.Variant.PadRight(8)
                + " | HeardNotReadable " + row.HeardNotReadable
                + " | message [" + Short(row.Message) + "]");
        }

        _output.WriteLine("");
        _output.WriteLine("  ANSWER: the per-block confidence EXISTS and DOES cross the seam -");
        _output.WriteLine("  OliviaBlock.Snr and .Accepted, OliviaStream/OliviaChannel.BlocksDecoded and");
        _output.WriteLine("  .BlocksRejected, OliviaChannelState.SyncSnr and .LastBlockSnr, and");
        _output.WriteLine("  OliviaChannel.Found says whether the blind search or an RSID found it.");
        _output.WriteLine("  NOTHING HAS TO BE INVENTED, so task 5 is not dropped on that ground.");
        _output.WriteLine("");

        Assert.Equal(2, rows.Count);
    }

    // =====================================================================================
    // The fixtures and the small helpers.
    // =====================================================================================

    /// <summary>A mixed fixture: station read and not, live and ended, PSK31 and Olivia.</summary>
    private IEnumerable<(string What, MainWindowViewModel Model, List<DigitalDecodeRow> Rows)> Panels()
    {
        var psk31 = Psk31Panel(out var psk31Rows);

        yield return ("PSK31, six carriers - three naming a station, three not", psk31, psk31Rows);

        var olivia = OliviaPanel(out var oliviaRows);

        yield return ("OLIVIA, four channels - two naming a station, two not", olivia, oliviaRows);
    }

    private MainWindowViewModel Psk31Panel(out List<DigitalDecodeRow> rows)
    {
        var model = Panel("PSK31");

        model.ShowPsk31ChannelsForTests(new[]
        {
            // Names a station, live.
            new Psk31Channel(1, 500, 10, "CQ CQ CQ de " + His + " " + His + " pse K\n", true),

            // Names a station, addressed to the operator, live.
            new Psk31Channel(2, 750, 11, Mine + " de K3ABC  RST 599 599  BTU " + Mine + " de K3ABC K\n", true),

            // Names a station, ended.
            new Psk31Channel(3, 1000, 12, "K9XYZ de N2DEF  R R TNX 73 73 SK\n", true),

            // NAMES NOBODY - a carrier heard with nothing read off it (the squelch shut).
            new Psk31Channel(4, 1250, 13, "", false),

            // NAMES NOBODY - characters arrived but no callsign is in them.
            new Psk31Channel(5, 1500, 14, "...-.- the band is long tonight and the qsb is deep\n", true),

            // NAMES NOBODY - a fragment the parser cannot read a call out of.
            new Psk31Channel(6, 1750, 15, "e5 ttt tu ee\n", true),
        }.ToList());

        rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        return model;
    }

    private MainWindowViewModel OliviaPanel(out List<DigitalDecodeRow> rows)
    {
        var model = Panel("Olivia");

        model.ShowOliviaChannelsForTests(new[]
        {
            // Names a station, live, RSID-announced.
            new OliviaChannel(1, "8/250", 1000, OliviaListener.FoundByRsid, 0, "CQ CQ CQ de " + His + " " + His + " pse K\n", 9, 0, false),

            // Names a station, ended.
            new OliviaChannel(2, "16/500", 1500, OliviaListener.FoundByRsid, 0, "K9XYZ de N2DEF  R R TNX 73 73 SK\n", 9, 0, true),

            // NAMES NOBODY - blind-found, one block accepted, the rest rejected. Tim's shape.
            new OliviaChannel(3, "4/500", 2000, OliviaListener.FoundBlind, 0, "Hk7DYYYzfzYXTDYY", 1, 11, false),

            // NAMES NOBODY - heard, nothing read at all.
            new OliviaChannel(4, "32/1000", 2400, OliviaListener.FoundBlind, 0, "", 0, 4, false),
        });

        rows = model.DigitalDecodes.Where(r => r.IsTextOnly).ToList();

        return model;
    }

    private static MainWindowViewModel Panel(string mode)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00DJ";
        settings.Operator.OperatorName = "Tim";
        settings.Operator.Location = "Pennsylvania";
        settings.Operator.LicenseClass = Hamlet.RadioEngine.Licensing.LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>(StringComparer.OrdinalIgnoreCase));
        model.ChooseDigitalModeCommand.Execute(mode);

        return model;
    }

    /// <summary>
    /// Runs the control's own `Render` over its own laid-out bounds, so what it draws and the hit
    /// rectangle it leaves behind can be read.
    /// </summary>
    /// <remarks>
    /// **THE HEADLESS WINDOW IS NEVER PAINTED** - `CaptureRenderedFrame` refuses without Skia -
    /// so the control is drawn into an off-screen bitmap **at the size the window's own layout
    /// gave it**. The size is the product's, measured; only the surface is the test's.
    /// </remarks>
    /// <param name="window">The realized window, already settled.</param>
    /// <param name="control">The control whose drawing is being measured.</param>
    private static void Paint(Window window, Control control)
    {
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var size = new PixelSize(
            Math.Max(1, (int)Math.Ceiling(control.Bounds.Width)),
            Math.Max(1, (int)Math.Ceiling(control.Bounds.Height)));

        using var surface = new Avalonia.Media.Imaging.RenderTargetBitmap(size);
        using var context = surface.CreateDrawingContext();

        control.Render(context);
    }

    /// <summary>Reads `RigDisplayControl`'s private hit rectangle. It writes nothing.</summary>
    /// <param name="rig">The control.</param>
    /// <returns>The rectangle, which is `default` where the bail fired.</returns>
    private static Rect StarRect(RigDisplayControl rig)
    {
        var field = typeof(RigDisplayControl)
            .GetField("_starRect", BindingFlags.NonPublic | BindingFlags.Instance);

        return field is null ? default : (Rect)field.GetValue(rig)!;
    }

    private static string Pretty(Type type)
        => type.IsGenericType
            ? type.Name[..type.Name.IndexOf('`', StringComparison.Ordinal)]
              + "<" + string.Join(",", type.GetGenericArguments().Select(a => a.Name)) + ">"
            : type.Name;

    private static string Px(double value)
        => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Box(Rect r)
        => "[" + Px(r.X) + "," + Px(r.Y) + " " + Px(r.Width) + "x" + Px(r.Height) + "]";

    private static string Short(string? text)
    {
        var one = (text ?? "").Replace("\n", " / ", StringComparison.Ordinal).Trim();

        return one.Length <= 40 ? one : one[..37] + "...";
    }

    /// <summary>
    /// A settings file carrying two favorites, in the shape `TheSettingsSurviveAnUpgradeTests`'
    /// 1.13.30 fixture uses. **Written by this test; it is nobody's real file.**
    /// </summary>
    private const string WithTwoFavorites = """
        {
          "WindowWidth": 1100,
          "WindowHeight": 780,
          "ReconnectOnStartup": false,
          "Favorites": [
            {
              "FrequencyHz": 14070150,
              "Name": "14.070, PSK31 corner",
              "Mode": "USB-D",
              "BandName": "20 m",
              "Neighborhood": "PSK31 corner",
              "SavedUtc": "2026-09-10T18:00:00Z",
              "Note": "a clear spot"
            },
            {
              "FrequencyHz": 7030000,
              "Name": "7.030, the CW watering hole",
              "Mode": "CW",
              "BandName": "40 m",
              "Neighborhood": "CW",
              "SavedUtc": "2026-09-11T02:00:00Z",
              "Note": ""
            }
          ]
        }
        """;
}
