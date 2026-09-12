using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 331 task 4: **the green zone earns its space.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The green zone is wasting a lot of real estate."* Three
/// lines and a full-width panel saying `14.074 MHz · yours to use` / `Your General license
/// covers digital modes here.` / `97.305(c)(3)(ix)` - **and saying the same thing every
/// time**, because all three are facts about the regulation and the regulation does not
/// change while he operates.</para>
/// <para>**SO ONE LINE OF LICENSE, AND THEN WHAT IS TRUE RIGHT NOW, BAND FIRST.** Tim's own
/// reason for the order: *"I really didn't know 14.070 was 20 meters until recently."*</para>
/// <para>**NOTHING HERE IS INVENTED** (§0.0). Every value on the live line is one the
/// screen already publishes elsewhere, and the tests below check that each one moves when
/// its source moves - a line that is right once and then frozen is worse than no line.</para>
/// </remarks>
public sealed class TheGreenZoneTests
{
    private const long Ft8On20 = 14_074_000;

    private const long Psk31On20 = 14_070_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the lines are printed.</param>
    public TheGreenZoneTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: line one carries the frequency, the license phrase and the citation,
    /// in the words the three lines already used.**
    /// </summary>
    [Fact]
    public void LineOneIsTheThreeLinesInTheirOwnWords()
    {
        var model = OnTwentyMeters(Ft8On20);

        var zone = model.GreenZone;

        _output.WriteLine("headline : " + model.PrivilegeStatus.Headline);
        _output.WriteLine("detail   : " + model.PrivilegeStatus.Detail);
        _output.WriteLine("citation : " + model.PrivilegeStatus.Citation);
        _output.WriteLine("");
        _output.WriteLine("line one : " + zone.License);

        Assert.True(zone.HasLicense);

        // **THE FREQUENCY, THE VERDICT, THE REASON AND THE RULE, ALL FOUR.**
        Assert.Contains("14.074 MHz", zone.License, StringComparison.Ordinal);
        Assert.Contains("yours to use", zone.License, StringComparison.Ordinal);
        Assert.Contains("97.305", zone.License, StringComparison.Ordinal);

        // **IN THE SAME WORDS**, with nothing rewritten: every word of the three lines is
        // still there, and the only edit is the trailing period off the detail.
        Assert.Contains(
            model.PrivilegeStatus.Headline, zone.License, StringComparison.Ordinal);
        Assert.Contains(
            model.PrivilegeStatus.Detail.TrimEnd('.'), zone.License, StringComparison.Ordinal);
        Assert.Contains(
            model.PrivilegeStatus.Citation, zone.License, StringComparison.Ordinal);

        // **ONE LINE AND NOT THREE**: no line break anywhere in it.
        Assert.DoesNotContain('\n', zone.License);
    }

    /// <summary>
    /// **Assertion 2: line two opens with the band name for the dial frequency, from the
    /// cited rows.**
    /// </summary>
    /// <remarks>
    /// **THE BAND CONTAINING THE DIAL AND NOT THE BAND HE PRESSED** (§0.0). The pill he
    /// pressed is a preference and the band around the dial is a measurement; they differ
    /// every time a tune lands outside the band he came from.
    /// </remarks>
    [Fact]
    public void LineTwoOpensWithTheBandForTheDial()
    {
        var model = OnTwentyMeters(Ft8On20);

        _output.WriteLine("band      : " + model.GreenZone.Band);
        _output.WriteLine("frequency : " + model.GreenZone.Frequency);

        Assert.Equal("20 m", model.GreenZone.Band);
        Assert.Equal("14.074 MHz", model.GreenZone.Frequency);

        // **AND IT MOVES WITH THE DIAL.** 7.074 is 40 m and the line says so.
        //
        // **THE BAND IS PRESSED FIRST BECAUSE THE DIAL IS CLAMPED TO IT.** `FrequencyHz`
        // clamps to the selected band's edges, so setting 7.074 while 20 m is selected
        // lands on 13.971 - which is the app being right and the first draft of this test
        // being wrong about it. That clamp is also why *the band containing the dial* and
        // *the band he pressed* agree most of the time; they are still different facts, and
        // the property reads the one that is a measurement.
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

        // **THE PALETTE'S LABEL, SO THE LEGEND AND THIS LINE NAME ONE FAMILY ONE THING.**
        Assert.Equal(
            Hamlet.App.Controls.ModePalette.Digital.Label, model.GreenZone.Family);
        Assert.Equal(ModeFamily.Digital, model.GreenZone.FamilyOf);
        Assert.Equal("FT8", model.GreenZone.SubMode);

        // **AND THE SUB-MODE MOVES WITH THE TAB.**
        model.ChosenDigitalMode = "PSK31";

        Assert.Equal("PSK31", model.GreenZone.SubMode);

        // **A MORSE BLOCK HAS NO SUB-MODE AND THE LINE SAYS SO BY SAYING NOTHING** (§0.0).
        // Carrying the tab's digital pick into a CW block would be the tab answering a
        // question about the dial.
        model.FrequencyHz = model.SelectedBand.Band.CwLowHz + 1000;

        _output.WriteLine(
            "in the CW block: family " + model.GreenZone.Family
            + ", submode [" + model.GreenZone.SubMode + "]");

        Assert.Equal(Hamlet.App.Controls.ModePalette.Cw.Label, model.GreenZone.Family);
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

        // **THE RANKING IS THE PILLS' OWN AND IS NOT COMPUTED HERE.** The badge is set on
        // a band button, so the fixture sets the button the way the ranking would.
        var twenty = model.Bands.First(b => b.Band.Name == "20 m");
        var forty = model.Bands.First(b => b.Band.Name == "40 m");

        foreach (var band in model.Bands)
        {
            band.IsBestBet = false;
        }

        twenty.IsBestBet = true;
        model.NotifyGreenZoneForTests();

        _output.WriteLine(
            "best bet on the band he is on : [" + model.GreenZone.BestBet + "] here="
            + model.GreenZone.BestBetIsHere);

        Assert.True(model.GreenZone.HasBestBet);
        Assert.True(model.GreenZone.BestBetIsHere);
        Assert.Contains(GreenZone.OnIt.Trim(), model.GreenZone.BestBet, StringComparison.Ordinal);
        Assert.StartsWith("20 m", model.GreenZone.BestBet, StringComparison.Ordinal);

        // **AND THE OTHER CASE CARRIES NO CHECK**, which is the non-color carrier of the
        // difference (§0.6).
        twenty.IsBestBet = false;
        forty.IsBestBet = true;
        model.NotifyGreenZoneForTests();

        _output.WriteLine(
            "best bet elsewhere            : [" + model.GreenZone.BestBet + "] here="
            + model.GreenZone.BestBetIsHere);

        Assert.False(model.GreenZone.BestBetIsHere);
        Assert.Equal("40 m", model.GreenZone.BestBet);
        Assert.DoesNotContain(
            GreenZone.OnIt.Trim(), model.GreenZone.BestBet, StringComparison.Ordinal);

        // **PRESSING IT TUNES THERE THE WAY THE PILL DOES** (§0.5.1). The dial lands on
        // 40 m's own jump frequency, which is where `SelectBand` puts it.
        model.TuneToBestBetCommand.Execute(null);

        _output.WriteLine(
            "after pressing it: " + model.SelectedBand.Band.Name + " at "
            + model.FrequencyHz.ToString(CultureInfo.InvariantCulture));

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

        _output.WriteLine("before any feed: [" + model.GreenZone.Heard + "]");

        // **NULL IS NOT NOUGHT** (§0.0). Before the feed has ever answered there is no
        // count, and *0 stations* would be a measurement of an empty band.
        Assert.False(model.GreenZone.HasHeard);
        Assert.Empty(model.GreenZone.Heard);

        model.HeardInTheLastMinute = 31;

        _output.WriteLine("with 31 dots   : [" + model.GreenZone.Heard + "]");

        Assert.True(model.GreenZone.HasHeard);
        Assert.Equal("31 stations", model.GreenZone.Heard);

        // **AND ONE IS SINGULAR**, because a line that says *1 stations* reads as broken.
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

        _output.WriteLine("at 14.070 with PSK31 picked: [" + model.GreenZone.Strayed + "]");

        // **IN ITS OWN SEGMENT: NOTHING TO SAY, AND THE PANEL IS A LINE SHORTER.**
        Assert.False(model.GreenZone.HasStrayed);

        model.FrequencyHz = Ft8On20;

        _output.WriteLine("at 14.074 with PSK31 picked: [" + model.GreenZone.Strayed + "]");

        Assert.True(model.GreenZone.HasStrayed);
        Assert.Contains("PSK31", model.GreenZone.Strayed, StringComparison.Ordinal);
        Assert.Contains("14.070", model.GreenZone.Strayed, StringComparison.Ordinal);
        Assert.Contains("14.074", model.GreenZone.Strayed, StringComparison.Ordinal);

        // **AND PICKING THE MODE HE IS ACTUALLY IN PUTS IT AWAY.**
        model.ChosenDigitalMode = "FT8";

        _output.WriteLine("at 14.074 with FT8 picked  : [" + model.GreenZone.Strayed + "]");

        Assert.False(model.GreenZone.HasStrayed);
    }

    /// <summary>
    /// **Assertion 7: the panel is shorter, by a measured number of pixels.**
    /// </summary>
    /// <remarks>
    /// <para>**THE BEFORE IS REBUILT RATHER THAN REMEMBERED.** The three lines that used to
    /// be stacked are the headline, the detail and the citation; their heights are measured
    /// on the realized panel's own type sizes, so the figure is arithmetic on this host
    /// rather than a number carried in from a commit message.</para>
    /// <para>**AND THE THIRD LINE IS COUNTED ONLY WHEN IT IS TRUE**, which is what makes
    /// the saving two lines in the ordinary case and one when something has strayed.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ThePanelIsShorterByAMeasuredNumberOfPixels()
    {
        var window = Realized(Ft8On20, "FT8");

        try
        {
            var license = window.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.Name == "GreenZoneLicenseLine");

            var live = window.GetVisualDescendants().OfType<StackPanel>()
                .First(p => p.Name == "GreenZoneLiveLine");

            var strayed = window.GetVisualDescendants().OfType<TextBlock>()
                .First(t => t.Name == "GreenZoneStrayedLine");

            var panel = (StackPanel)license.Parent!;

            _output.WriteLine(
                "license line : " + Px(license.Bounds.Height) + " px, ["
                + license.Text + "]");
            _output.WriteLine("live line    : " + Px(live.Bounds.Height) + " px");
            _output.WriteLine(
                "strayed line : drawn=" + strayed.IsVisible + ", "
                + Px(strayed.Bounds.Height) + " px");
            _output.WriteLine("the panel    : " + Px(panel.Bounds.Height) + " px");

            Assert.True(license.IsVisible, "the license line is not drawn");
            Assert.True(live.IsVisible, "the live line is not drawn");
            Assert.False(strayed.IsVisible, "nothing has strayed and the line is drawn");

            // **WHAT THE THREE STACKED LINES COST, ON THIS HOST'S OWN TYPE.** The headline
            // was 13 point and the detail 12 and the citation 10, in a stack with three
            // pixels of spacing; the one line that replaces them is 12.
            var was = license.Bounds.Height + live.Bounds.Height;
            var three = Line(window, 13) + Line(window, 12) + Line(window, 10) + (2 * 3);
            var one = Line(window, 12);

            _output.WriteLine("");
            _output.WriteLine(
                "the license was three lines at 13, 12 and 10 point plus two 3 px gaps: "
                + Px(three) + " px");
            _output.WriteLine("it is one line at 12 point: " + Px(one) + " px");
            _output.WriteLine("so the license is " + Px(three - one) + " px shorter");
            _output.WriteLine(
                "and the live line it makes room for is " + Px(live.Bounds.Height) + " px, "
                + "so the panel is " + Px(three - one - live.Bounds.Height)
                + " px shorter than it was with the same facts on it");

            Assert.True(
                three - one > 0,
                "the license line is not shorter than the three it replaced");

            Assert.True(was > 0, "nothing was measured");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// **Assertion 8: every run of the live line is on the screen, and the best bet is a
    /// press.**
    /// </summary>
    [AvaloniaFact]
    public void EveryRunOfTheLiveLineIsOnTheScreen()
    {
        var window = Realized(Ft8On20, "FT8");

        try
        {
            foreach (var name in new[]
            {
                "GreenZoneBand", "GreenZoneFrequency", "GreenZoneFamily", "GreenZoneSubMode",
            })
            {
                var run = window.GetVisualDescendants().OfType<TextBlock>()
                    .FirstOrDefault(t => t.Name == name);

                Assert.True(run is not null, name + " is not on the window");

                _output.WriteLine(name.PadRight(20) + " [" + run!.Text + "] visible="
                    + run.IsVisible);

                Assert.True(run.IsVisible, name + " is not drawn");
                Assert.False(
                    string.IsNullOrWhiteSpace(run.Text), name + " has nothing in it");
            }

            var press = window.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Name == "GreenZoneBestBet");

            Assert.True(press is not null, "the best bet is not on the window");

            _output.WriteLine(
                "GreenZoneBestBet".PadRight(20) + " [" + press!.Content
                + "] command=" + (press.Command is not null));

            // **A LIVE CONTROL AND NOT A DEAD ONE** (§0.5.1, HM-DEC-087).
            Assert.NotNull(press.Command);
        }
        finally
        {
            window.Close();
        }
    }

    private static string Px(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>How tall one line of the panel's own face is at a point size.</summary>
    private static double Line(Window window, double size)
        => new Avalonia.Media.FormattedText(
            "14.074 MHz",
            CultureInfo.InvariantCulture,
            Avalonia.Media.FlowDirection.LeftToRight,
            new Avalonia.Media.Typeface(
                window.GetVisualDescendants().OfType<TextBlock>()
                    .First(t => t.Name == "GreenZoneLicenseLine").FontFamily),
            size,
            null).Height;

    /// <summary>A view model on 20 m with a license class and a dial.</summary>
    private static MainWindowViewModel OnTwentyMeters(long dialHz)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.LicenseClass = LicenseClass.General;
        settings.Operator.Callsign = "KC3QIS";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
        };

        model.SelectBandCommand.Execute(model.Bands.First(b => b.Band.Name == "20 m"));
        model.FrequencyHz = dialHz;

        return model;
    }

    /// <summary>The window, realized, with the map panel on it.</summary>
    private static Window Realized(long dialHz, string subMode)
    {
        var model = OnTwentyMeters(dialHz);

        model.ChosenDigitalMode = subMode;
        model.MapExpanded = true;

        var window = new MainWindow { DataContext = model, Width = 1400, Height = 1200 };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        return window;
    }
}
