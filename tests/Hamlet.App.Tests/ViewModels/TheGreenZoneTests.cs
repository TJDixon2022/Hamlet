using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Solar;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instructions 331 task 4 and 332 task 2: **the green zone is the world's clock.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**, on unit 331's two lines: *"I said this was wasted real estate
/// on the right so you just put more on the left."* Then, on the card's map: *"We ought to use
/// something like this with a dark/light indicator to show where the band can reach right
/// now. Looking for contacts in eastern Europe at 2:00 pm EST is not likely."* And *"without
/// the orphan dots, maybe just a dot at my grid"*.</para>
/// <para>**REWRITTEN IN WORK INSTRUCTION 332.** Assertions 1 to 6 are 331's and still hold -
/// the record still carries every one of those facts. Assertions 7 and 8 measured a two-line
/// panel that is gone; what replaces them measures the three regions.</para>
/// <para>**REWRITTEN AGAIN IN WORK INSTRUCTION 334 UNDER R12.** Tim, 2026-09-12: *"Too
/// redundant. We don't need the repeat of the band list on the green. Maybe make the map
/// bigger."* Assertion 10 measured the pills and now asserts they are gone and the map took
/// their width; assertion 12 no longer looks for the line under the pill he is on.</para>
/// <para>**THE SUN IS FACT AND PROPAGATION IS NOT** (§0.0). The night side is asserted
/// against arithmetic done by hand in this file; nothing here asserts or permits a claim that
/// a band is open.</para>
/// </remarks>
public sealed class TheGreenZoneTests
{
    private const long Ft8On20 = 14_074_000;

    private const long Psk31On20 = 14_070_000;

    private const string HisGrid = "FN00";

    /// <summary>
    /// **The map's width on this window at 1400 before the pills came off**, measured by work
    /// instruction 334 on the headless host: 202 x 110.
    /// </summary>
    private const double MapWidthBefore = 202;

    /// <summary>
    /// **What the map grew by on the same window when the pills came off: 202 to 232.**
    /// </summary>
    /// <remarks>
    /// **MEASURED, NOT CHOSEN** (work instruction 334's ARBITER block). At 1400 the right block's
    /// width is set by the count and its sparkline, not by the pills, which had wrapped to three
    /// rows inside it; so the width they gave back is small, and the map's growth in height -
    /// 110 to 127 - is where most of their room went.
    /// </remarks>
    private const double MapWidthGrew = 30;

    /// <summary>2 pm EDT on the day Tim chose the darkness: 18:00 UTC.</summary>
    private static readonly DateTime TwoPmEdt = new(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheGreenZoneTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: the license line carries the frequency, the license phrase and the
    /// citation, in the words the three lines already used.**
    /// </summary>
    [Fact]
    public void LineOneIsTheThreeLinesInTheirOwnWords()
    {
        var model = OnTwentyMeters(Ft8On20);

        model.ChosenDigitalMode = "FT8";

        var zone = model.GreenZone;

        _output.WriteLine("headline : " + model.PrivilegeStatus.Headline);
        _output.WriteLine("detail   : " + model.PrivilegeStatus.Detail);
        _output.WriteLine("citation : " + model.PrivilegeStatus.Citation);
        _output.WriteLine("");
        _output.WriteLine("line one : " + zone.License);
        _output.WriteLine("phrase   : " + zone.LicensePhrase);
        _output.WriteLine("mode line: " + zone.ModeLine);

        Assert.True(zone.HasLicense);

        Assert.Contains("14.074 MHz", zone.License, StringComparison.Ordinal);
        Assert.Contains("yours to use", zone.License, StringComparison.Ordinal);
        Assert.Contains("97.305", zone.License, StringComparison.Ordinal);

        Assert.Contains(
            model.PrivilegeStatus.Headline, zone.License, StringComparison.Ordinal);
        Assert.Contains(
            model.PrivilegeStatus.Detail.TrimEnd('.'), zone.License, StringComparison.Ordinal);
        Assert.Contains(
            model.PrivilegeStatus.Citation, zone.License, StringComparison.Ordinal);

        Assert.DoesNotContain('\n', zone.License);

        // **THE LEFT REGION'S TWO SMALLER LINES ARE THE SAME WORDS, SPLIT WHERE THE REGION
        // SHOWS THE FREQUENCY BESIDE THE BAND.**
        Assert.Equal("yours to use", zone.Verdict);
        Assert.Equal("Digital · FT8 · yours to use", zone.ModeLine);
        Assert.Contains("97.305", zone.LicensePhrase, StringComparison.Ordinal);
        Assert.DoesNotContain("MHz", zone.LicensePhrase, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Assertion 2: the band is the band for the dial frequency, from the cited rows.**
    /// </summary>
    [Fact]
    public void LineTwoOpensWithTheBandForTheDial()
    {
        var model = OnTwentyMeters(Ft8On20);

        _output.WriteLine("band      : " + model.GreenZone.Band);
        _output.WriteLine("frequency : " + model.GreenZone.Frequency);

        Assert.Equal("20 m", model.GreenZone.Band);
        Assert.Equal("14.074 MHz", model.GreenZone.Frequency);

        // **THE BAND IS PRESSED FIRST BECAUSE THE DIAL IS CLAMPED TO IT.** `FrequencyHz`
        // clamps to the selected band's edges.
        model.SelectBandCommand.Execute(model.Bands.First(b => b.Band.Name == "40 m"));
        model.FrequencyHz = 7_074_000;

        _output.WriteLine("after tuning to 7.074: " + model.GreenZone.Band
            + " · " + model.GreenZone.Frequency);

        Assert.Equal("40 m", model.GreenZone.Band);
        Assert.Equal("7.074 MHz", model.GreenZone.Frequency);
    }

    /// <summary>
    /// **Assertion 3: the family and the sub-mode are the tab's, and the family word is the
    /// palette's own.**
    /// </summary>
    [Fact]
    public void TheFamilyAndSubModeComeFromTheMapAndTheTab()
    {
        var model = OnTwentyMeters(Ft8On20);

        model.ChosenDigitalMode = "FT8";

        _output.WriteLine(
            "family : " + model.GreenZone.Family + " (" + model.GreenZone.FamilyOf + ")");
        _output.WriteLine("submode: " + model.GreenZone.SubMode);

        Assert.Equal(ModePalette.Digital.Label, model.GreenZone.Family);
        Assert.Equal(ModeFamily.Digital, model.GreenZone.FamilyOf);
        Assert.Equal("FT8", model.GreenZone.SubMode);

        model.ChosenDigitalMode = "PSK31";

        Assert.Equal("PSK31", model.GreenZone.SubMode);

        // **A MORSE BLOCK HAS NO SUB-MODE AND THE LINE SAYS SO BY SAYING NOTHING** (§0.0).
        model.FrequencyHz = model.SelectedBand.Band.CwLowHz + 1000;

        _output.WriteLine(
            "in the CW block: family " + model.GreenZone.Family
            + ", submode [" + model.GreenZone.SubMode + "]");

        Assert.Equal(ModePalette.Cw.Label, model.GreenZone.Family);
        Assert.Empty(model.GreenZone.SubMode);
    }

    /// <summary>
    /// **Assertion 4: the best bet carries the check when it is the band he is on and the
    /// nudge when it is not, and pressing it tunes there.**
    /// </summary>
    [Fact]
    public void TheBestBetChecksWhenItIsHereAndNudgesWhenItIsNot()
    {
        var model = OnTwentyMeters(Ft8On20);

        var twenty = model.Bands.First(b => b.Band.Name == "20 m");
        var forty = model.Bands.First(b => b.Band.Name == "40 m");

        foreach (var band in model.Bands)
        {
            band.IsBestBet = false;
        }

        twenty.IsBestBet = true;
        model.NotifyGreenZoneForTests();

        Assert.True(model.GreenZone.HasBestBet);
        Assert.True(model.GreenZone.BestBetIsHere);
        Assert.Contains(GreenZone.OnIt.Trim(), model.GreenZone.BestBet, StringComparison.Ordinal);
        Assert.StartsWith("20 m", model.GreenZone.BestBet, StringComparison.Ordinal);

        twenty.IsBestBet = false;
        forty.IsBestBet = true;
        model.NotifyGreenZoneForTests();

        Assert.False(model.GreenZone.BestBetIsHere);
        Assert.Equal("40 m", model.GreenZone.BestBet);
        Assert.DoesNotContain(
            GreenZone.OnIt.Trim(), model.GreenZone.BestBet, StringComparison.Ordinal);

        model.TuneToBestBetCommand.Execute(null);

        Assert.Equal("40 m", model.SelectedBand.Band.Name);
        Assert.Equal(forty.Band.JumpHz, model.FrequencyHz);
    }

    /// <summary>
    /// **Assertion 5: the heard count is the map's own dots, and absent is not nought.**
    /// </summary>
    [Fact]
    public void TheHeardCountIsTheDotsOwnAndAbsentIsNotNought()
    {
        var model = OnTwentyMeters(Ft8On20);

        Assert.False(model.GreenZone.HasHeard);
        Assert.Empty(model.GreenZone.Heard);

        model.HeardInTheLastMinute = 31;

        Assert.True(model.GreenZone.HasHeard);
        Assert.Equal("31 stations", model.GreenZone.Heard);

        model.HeardInTheLastMinute = 1;

        Assert.Equal("1 station", model.GreenZone.Heard);
    }

    /// <summary>
    /// **Assertion 6: the strayed line appears only when the sub-mode's segment does not
    /// contain the dial.**
    /// </summary>
    [Fact]
    public void TheStrayedLineAppearsOnlyWhenItIsTrue()
    {
        var model = OnTwentyMeters(Psk31On20);

        model.ChosenDigitalMode = "PSK31";

        Assert.False(model.GreenZone.HasStrayed);

        model.FrequencyHz = Ft8On20;

        Assert.True(model.GreenZone.HasStrayed);
        Assert.Contains("PSK31", model.GreenZone.Strayed, StringComparison.Ordinal);
        Assert.Contains("14.070", model.GreenZone.Strayed, StringComparison.Ordinal);
        Assert.Contains("14.074", model.GreenZone.Strayed, StringComparison.Ordinal);

        model.ChosenDigitalMode = "FT8";

        Assert.False(model.GreenZone.HasStrayed);
    }

    /// <summary>
    /// **Assertion 7: the subsolar longitude matches a hand-computed value within 1°.**
    /// </summary>
    /// <remarks>
    /// <para>**THE HAND COMPUTATION IS A DIFFERENT FORMULA FROM THE CODE'S**, written out
    /// below, so the two agreeing is evidence rather than one formula checking itself. The
    /// code uses NOAA's general solar position series; this uses the textbook equation of
    /// time, EoT = 9.87 sin 2B - 7.53 cos B - 1.5 sin B minutes, with B = 360/365 (N - 81)
    /// degrees.</para>
    /// <para>**2026-09-12 18:00 UTC is 2 pm EDT**, day 255: B = 171.62°, EoT = +4.38 min,
    /// so the sun is overhead at -15 × (18 - 12 + 4.38/60) = **-91.10°**, over the Gulf of
    /// Mexico south of Louisiana - which is where 2 pm EDT puts it.</para>
    /// </remarks>
    [Fact]
    public void TheSubsolarLongitudeMatchesAHandComputedValueWithinADegree()
    {
        var n = TwoPmEdt.DayOfYear;
        var b = 360.0 / 365.0 * (n - 81);
        double Sin(double d) => Math.Sin(d * Math.PI / 180);
        double Cos(double d) => Math.Cos(d * Math.PI / 180);

        var eot = (9.87 * Sin(2 * b)) - (7.53 * Cos(b)) - (1.5 * Sin(b));
        var handLongitude = -15.0 * (TwoPmEdt.Hour - 12 + (eot / 60.0));
        var handDeclination = 23.44 * Sin(b);

        var sun = SolarTerminator.At(TwoPmEdt);

        _output.WriteLine("day " + n + ", B " + F(b) + "°, EoT " + F(eot) + " min");
        _output.WriteLine("hand : longitude " + F(handLongitude) + "°, declination " + F(handDeclination) + "°");
        _output.WriteLine("code : longitude " + F(sun.Longitude) + "°, declination " + F(sun.Latitude) + "°");

        Assert.InRange(sun.Longitude, handLongitude - 1.0, handLongitude + 1.0);

        // **THE DECLINATION'S HAND FORMULA IS THE COARSER ONE** - a pure sine of the day -
        // so it is held to a degree and a half; the sun is a few degrees north ten days
        // before the equinox, and both say so.
        Assert.InRange(sun.Latitude, handDeclination - 1.5, handDeclination + 1.5);
    }

    /// <summary>
    /// **Assertion 7a: the night side is the sun's, and the gray edge is a fade.**
    /// </summary>
    /// <remarks>
    /// **TIM'S OWN EXAMPLE, AS ARITHMETIC ON THE SUN AND NOTHING ELSE.** At 2 pm EDT his grid
    /// is in daylight and Moscow is past dusk; at 10 pm EDT his grid is dark. On the equator
    /// 93° east of the sun the sun is about 3° below the horizon, which is inside the
    /// six-degree fade and so is neither day nor night.
    /// </remarks>
    [Fact]
    public void TheNightSideIsTheSunsAndTheGrayEdgeIsAFade()
    {
        var sun = SolarTerminator.At(TwoPmEdt);
        var his = OperatorLocation.FromGrid(HisGrid)!.Value;

        var hisAtTwo = SolarTerminator.Darkness(sun, his.Latitude, his.Longitude);
        var moscowAtTwo = SolarTerminator.Darkness(sun, 55.75, 37.62);

        var tenPm = SolarTerminator.At(TwoPmEdt.AddHours(8));
        var hisAtTen = SolarTerminator.Darkness(tenPm, his.Latitude, his.Longitude);

        var edge = SolarTerminator.Darkness(sun, 0, sun.Longitude + 93);

        _output.WriteLine("his grid at 2 pm EDT  : darkness " + F(hisAtTwo));
        _output.WriteLine("Moscow at 2 pm EDT    : darkness " + F(moscowAtTwo));
        _output.WriteLine("his grid at 10 pm EDT : darkness " + F(hisAtTen));
        _output.WriteLine("equator, 93° east     : darkness " + F(edge));

        Assert.Equal(0, hisAtTwo);
        Assert.Equal(1, moscowAtTwo);
        Assert.Equal(1, hisAtTen);
        Assert.InRange(edge, 0.2, 0.8);
    }

    /// <summary>
    /// **Assertion 8: the map carries exactly one marker, at the operator's grid, and none
    /// without one.**
    /// </summary>
    [Fact]
    public void TheMapCarriesExactlyOneMarkerAtTheOperatorsGrid()
    {
        const double Width = 220;
        var height = Ft8GlobeControl.HeightFor(Width);

        var drawn = GrayLineMapControl.WhatWouldBeDrawn(TwoPmEdt, HisGrid, Width, height);

        var his = OperatorLocation.FromGrid(HisGrid)!.Value;
        var pixel = FlatWorldMap.Relief.Place(his.Latitude, his.Longitude)!.Value;
        var scale = Width / FlatWorldMap.Relief.WidthPixels;
        var expected = new Point(pixel.X * scale, pixel.Y * scale);

        _output.WriteLine(
            "markers " + drawn.Markers + " at " + drawn.MarkerAt + ", expected " + expected);
        _output.WriteLine(
            "night cells " + drawn.NightCells + ", twilight cells " + drawn.TwilightCells);

        Assert.Equal(1, drawn.Markers);
        Assert.InRange(drawn.MarkerAt.X, expected.X - 0.5, expected.X + 0.5);
        Assert.InRange(drawn.MarkerAt.Y, expected.Y - 0.5, expected.Y + 0.5);

        // **AND THE NIGHT SIDE IS THERE, WITH A GRAY EDGE BETWEEN.**
        Assert.True(drawn.NightCells > 0, "no night drawn at 2 pm EDT");
        Assert.True(drawn.TwilightCells > 0, "no gray edge drawn");

        // **NO GRID, NO MARKER** - never a guessed one.
        Assert.Equal(0, GrayLineMapControl.WhatWouldBeDrawn(TwoPmEdt, null, Width, height).Markers);
    }

    /// <summary>**Assertion 9: the sparkline reflects the heard count.**</summary>
    [Fact]
    public void TheSparklineReflectsTheHeardCount()
    {
        var heard = new[] { 2, 7, 8, 21, 40, 59, 61, 300 }
            .Select(s => TwoPmEdt.AddSeconds(-s))
            .ToList();

        var bins = GreenZone.Sparkline(heard, TwoPmEdt);
        var count = heard.Count(at => TwoPmEdt - at <= TimeSpan.FromMinutes(1));

        _output.WriteLine("bins, oldest first: " + string.Join(" ", bins) + " = " + bins.Sum());
        _output.WriteLine("heard in the last minute: " + count);

        Assert.Equal(GreenZone.SparklineBins, bins.Count);
        Assert.Equal(count, bins.Sum());
        Assert.Equal(1, bins[^1]);
        Assert.Equal(2, bins[^2]);
    }

    /// <summary>
    /// **Assertion 10: no band pill is on the green zone, and the map is wider by the space they
    /// held.**
    /// </summary>
    /// <remarks>
    /// <para>**TIM, 2026-09-12** (R21): *"Too redundant. We don't need the repeat of the band
    /// list on the green. Maybe make the map bigger."* The pills are the row above the panel.
    /// **REWRITTEN IN WORK INSTRUCTION 334 UNDER R12**; this assertion measured the pills unit
    /// 332 put there.</para>
    /// <para>**THE LEFT BLOCK, THE COUNT AND THE RULE OF THUMB STAY**, so they are asserted here
    /// beside what went.</para>
    /// </remarks>
    [AvaloniaFact]
    public void NoBandPillIsOnTheGreenZoneAndTheMapTookTheirWidth()
    {
        var window = Realized(Ft8On20, "FT8");

        try
        {
            var panel = Panel(window);
            var controls = panel.GetVisualDescendants().OfType<Control>().ToList();
            var map = Named<GrayLineMapControl>(window, "GreenZoneGrayLine");

            _output.WriteLine(
                "chips " + controls.Count(c => c.Classes.Contains("hm-chip"))
                + ", pips " + controls.OfType<ActivityPipsControl>().Count()
                + ", map " + F(map.Bounds.Width) + " x " + F(map.Bounds.Height)
                + " (was " + F(MapWidthBefore) + " x 110.00)");

            Assert.DoesNotContain(controls, c => c.Name == "GreenZonePills");
            Assert.DoesNotContain(controls, c => c.Classes.Contains("hm-chip"));
            Assert.Empty(controls.OfType<ActivityPipsControl>());
            Assert.DoesNotContain(
                VisibleText(panel),
                t => (t.Text ?? "").Contains("you are on it", StringComparison.OrdinalIgnoreCase));

            Assert.InRange(
                map.Bounds.Width, MapWidthBefore + MapWidthGrew - 0.5, MapWidthBefore + MapWidthGrew + 0.5);

            foreach (var name in new[]
            {
                "GreenZoneBand", "GreenZoneFrequency", "GreenZoneModeLine", "GreenZoneLicenseLine",
                "GreenZoneHeard", "GreenZoneRuleOfThumb",
            })
            {
                Assert.True(Named<TextBlock>(window, name).IsEffectivelyVisible, name + " is not drawn");
            }

            // **THE COUNT IS ON THE RIGHT OF THE MAP AND THE RULE OF THUMB UNDER IT.**
            var mapAt = map.TranslatePoint(new Point(0, 0), panel)!.Value;
            var heard = Named<TextBlock>(window, "GreenZoneHeard");
            var rule = Named<TextBlock>(window, "GreenZoneRuleOfThumb");

            Assert.True(
                heard.TranslatePoint(new Point(0, 0), panel)!.Value.X >= mapAt.X + map.Bounds.Width,
                "the heard count is not right of the map");
            Assert.True(
                rule.TranslatePoint(new Point(0, 0), panel)!.Value.Y >= mapAt.Y + map.Bounds.Height,
                "the rule of thumb is not under the map");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 11: on the window, the band is the largest text and in family color.**
    /// </summary>
    [AvaloniaFact]
    public void TheBandIsTheLargestTextOnThePanelAndInFamilyInk()
    {
        var window = Realized(Ft8On20, "FT8");

        try
        {
            var band = Named<TextBlock>(window, "GreenZoneBand");
            var panel = Panel(window);

            var sizes = VisibleText(panel)
                .Select(t => (t.Text, t.FontSize))
                .ToList();

            foreach (var (text, size) in sizes)
            {
                _output.WriteLine(F(size).PadLeft(5) + "  " + text);
            }

            Assert.True(band.IsEffectivelyVisible, "the band is not drawn");
            Assert.Equal("20 m", band.Text);
            Assert.True(
                sizes.Where(s => s.Text != band.Text).All(s => s.FontSize < band.FontSize),
                "something on the panel is as large as the band");

            var ink = Assert.IsAssignableFrom<ISolidColorBrush>(band.Foreground);
            var digital = Assert.IsAssignableFrom<ISolidColorBrush>(ModePalette.Digital.InkBrush);

            Assert.Equal(digital.Color, ink.Color);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 12: the map region renders, with its rule of thumb and no claim of
    /// openness; the sparkline and count agree.**
    /// </summary>
    [AvaloniaFact]
    public void TheMapRegionRendersWithItsRuleOfThumbAndNoClaimOfOpenness()
    {
        var window = Realized(Ft8On20, "FT8");

        try
        {
            var map = Named<GrayLineMapControl>(window, "GreenZoneGrayLine");

            _output.WriteLine(
                "map " + F(map.Bounds.Width) + " x " + F(map.Bounds.Height) + " for " + map.Utc
                + " at " + map.OperatorGrid);

            Assert.True(map.IsEffectivelyVisible, "the map is not drawn");
            Assert.True(map.Bounds.Height > 40, "the map has no height");
            Assert.Equal(HisGrid, map.OperatorGrid);

            var rule = Named<TextBlock>(window, "GreenZoneRuleOfThumb");

            _output.WriteLine("rule: " + rule.Text);

            Assert.True(rule.IsEffectivelyVisible);
            Assert.Contains("rule of thumb", rule.Text ?? "", StringComparison.OrdinalIgnoreCase);

            // **NOTHING ON THE PANEL SAYS A BAND IS OPEN** (§0.0).
            foreach (var text in VisibleText(Panel(window)).Select(t => t.Text ?? ""))
            {
                foreach (var claim in new[] { "open", "likely", "chance", "reach" })
                {
                    Assert.DoesNotContain(claim, text, StringComparison.OrdinalIgnoreCase);
                }
            }

            var sparkline = Named<SparklineControl>(window, "GreenZoneSparkline");
            var model = (MainWindowViewModel)window.DataContext!;

            _output.WriteLine(
                "sparkline " + string.Join(" ", sparkline.Values ?? Array.Empty<int>())
                + " | count " + Named<TextBlock>(window, "GreenZoneHeard").Text);

            // **THE MODEL'S OWN COUNT, NOT THE FIXTURE'S SIX.** The realized window runs its
            // own spot refresh, and measured, it replaced the six with twenty-five before the
            // assertion ran - with the sparkline's bins adding up to twenty-five too, which is
            // the property this checks.
            Assert.Equal(model.HeardInTheLastMinute, sparkline.Values?.Sum());
            Assert.Equal(model.GreenZone.Heard, Named<TextBlock>(window, "GreenZoneHeard").Text);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**Assertion 13: the panel uses at least 90% of its width.**</summary>
    /// <remarks>
    /// **INK, NOT BOXES.** A text block stretched across a panel occupies the panel whatever
    /// it says, so each run counts for the width its own text lays out to, and a control for
    /// its bounds. The figure is from the leftmost ink to the rightmost over the panel's
    /// inside width.
    /// </remarks>
    [AvaloniaFact]
    public void ThePanelUsesAtLeastNinetyPercentOfItsWidth()
    {
        var window = Realized(Ft8On20, "FT8");

        try
        {
            var panel = Panel(window);
            var inside = panel.Bounds.Width - panel.Padding.Left - panel.Padding.Right
                - panel.BorderThickness.Left - panel.BorderThickness.Right;

            var left = double.MaxValue;
            var right = double.MinValue;
            var furthest = "";

            foreach (var control in panel.GetVisualDescendants().OfType<Control>()
                .Where(c => c.IsEffectivelyVisible && c.Bounds.Width > 0))
            {
                if (control is not (TextBlock or GrayLineMapControl or SparklineControl))
                {
                    continue;
                }

                if (control is TextBlock { Text: var said } text && string.IsNullOrWhiteSpace(said))
                {
                    continue;
                }

                var at = control.TranslatePoint(new Point(0, 0), panel)?.X ?? 0;
                var width = control.Bounds.Width;

                if (control is TextBlock run)
                {
                    var ink = Math.Min(width, Natural(run));

                    at += run.TextAlignment switch
                    {
                        TextAlignment.Right => width - ink,
                        TextAlignment.Center => (width - ink) / 2,
                        _ => 0,
                    };

                    width = ink;
                }

                left = Math.Min(left, at);

                if (at + width > right)
                {
                    right = at + width;
                    furthest = control.Name
                        ?? (control as TextBlock)?.Text
                        ?? control.GetType().Name;
                }
            }

            var used = (right - left) / inside;

            _output.WriteLine(
                "panel inside " + F(inside) + " px; ink from " + F(left) + " to " + F(right)
                + " = " + (used * 100).ToString("0.0", CultureInfo.InvariantCulture) + "%"
                + "; furthest right: [" + furthest + "]");

            Assert.True(used >= 0.9, "the panel uses " + F(used * 100) + "% of its width");

            // **AND NOTHING RUNS PAST ITS EDGE.** The first green run of this test read 142%
            // - the three regions asked for more than the panel had and spilled out of the
            // right side, which a floor alone passes.
            var start = panel.Padding.Left + panel.BorderThickness.Left;

            Assert.True(
                left >= start - 0.5 && right <= start + inside + 0.5,
                "the panel's ink runs from " + F(left) + " to " + F(right) + " and its inside is "
                + F(start) + " to " + F(start + inside));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>**Assertion 14: the two events carry what they name and nothing else.**</summary>
    [Fact]
    public void TheTwoEventsCarryWhatTheyNameAndNothingElse()
    {
        var sink = new Recording();

        AppEvents.GreenZoneRendered(sink, 640.44, 150.2, "left,map,right");
        AppEvents.TerminatorComputed(sink, -91.1234, 4.4012);

        foreach (var (name, data) in sink.Events)
        {
            _output.WriteLine(name + "  " + string.Join("  ", data.Select(kv => kv.Key + "=" + kv.Value)));
        }

        Assert.Equal("green_zone_rendered", sink.Events[0].Name);
        Assert.Equal(new[] { "width", "height", "regions" }, sink.Events[0].Data.Keys);

        Assert.Equal("terminator_computed", sink.Events[1].Name);
        Assert.Equal(new[] { "subsolarLongitude", "declination" }, sink.Events[1].Data.Keys);
    }

    // ------------------------------------------------------------------------------------

    private static string F(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static T Named<T>(Window window, string name)
        where T : Control
    {
        var found = window.GetVisualDescendants().OfType<T>().FirstOrDefault(c => c.Name == name);

        Assert.True(found is not null, "there is no " + typeof(T).Name + " named " + name);

        return found!;
    }

    /// <summary>The green zone panel: the bordered box around the license line.</summary>
    private static Border Panel(Window window)
        => Named<TextBlock>(window, "GreenZoneLicenseLine").GetVisualAncestors()
            .OfType<Border>()
            .First(b => b.BorderThickness.Left > 0);

    private static IEnumerable<TextBlock> VisibleText(Visual root)
        => root.GetVisualDescendants().OfType<TextBlock>()
            .Where(t => t.IsEffectivelyVisible && (t.Text ?? "").Trim().Length > 0);

    private static double Natural(TextBlock text)
        => new TextLayout(
            text.Text ?? "",
            new Typeface(text.FontFamily, text.FontStyle, text.FontWeight),
            text.FontSize,
            null).Width;

    /// <summary>A view model on 20 m with a license class, a grid and a dial.</summary>
    private static MainWindowViewModel OnTwentyMeters(long dialHz)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.LicenseClass = LicenseClass.General;
        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = HisGrid;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        model.SelectBandCommand.Execute(model.Bands.First(b => b.Band.Name == "20 m"));
        model.FrequencyHz = dialHz;

        return model;
    }

    /// <summary>The window, realized, with the map panel on it at 2 pm EDT and six heard.</summary>
    private static Window Realized(long dialHz, string subMode)
    {
        var model = OnTwentyMeters(dialHz);

        model.ChosenDigitalMode = subMode;
        model.MapExpanded = true;
        model.GrayLineUtc = TwoPmEdt;
        model.HeardInTheLastMinute = 6;
        model.HeardSparkline = GreenZone.Sparkline(
            new[] { 3, 9, 14, 30, 44, 58 }.Select(s => TwoPmEdt.AddSeconds(-s)), TwoPmEdt);

        var window = new MainWindow { DataContext = model, Width = 1400, Height = 1200 };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }

    /// <summary>A sink that remembers, so a test can read what was written.</summary>
    private sealed class Recording : ITelemetry
    {
        public List<(string Name, IReadOnlyDictionary<string, object?> Data)> Events { get; } = new();

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => Events.Add((eventName, data ?? new Dictionary<string, object?>()));
    }
}
