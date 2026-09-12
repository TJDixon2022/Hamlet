using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 330 task 3: **the quill popup is a preview of the card he would earn.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The achievement indicator popup is not pretty or
/// interesting."* What he pressed gave him `LA8ENA / Norway · a new country · you have
/// worked 3 in Europe / A QSO first. The card comes with it.` - three lines of gray text in
/// a box. Every word true, none of it worth opening.</para>
/// <para>**WHAT IS ASSERTED HERE IS THE CONTENT AND THE RULES IT MUST NOT BREAK**: a card
/// face is drawn; a counter's face names the entity and its teaching line carries the count
/// and the remainder from the cited table; a door's face names nothing and its teaching line
/// names nothing; the fact line is the card's own column; *confirmed* is nowhere; and
/// `nudge_opened` still fires with the kind and nothing else.</para>
/// <para>**COMPUTED, NOT SEEN.** The words come off the view model and the controls off a
/// realized headless window. Nobody looked at the application.</para>
/// </remarks>
public sealed class TheQuillPopupTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    /// <summary>Vienna, JN88 - a long path and a clear compass word from FN00.</summary>
    private const string FarGrid = "JN88";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the rendered popup is printed.</param>
    public TheQuillPopupTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Assertion 1: a counter's face names the entity and teaches the set.**
    /// </summary>
    /// <remarks>
    /// **THE COUNT AND THE REMAINDER BOTH COME OFF `DxccContinents`**, which reads
    /// `data/callsigns/dxcc-continents.json` and carries its own source line - so the
    /// figures in the sentence are the cited table's and not this file's (§0).
    /// </remarks>
    [Fact]
    public void ACountersFaceNamesTheEntityAndTeachesTheSet()
    {
        var preview = NudgePreview.For(
            new NudgeReason(NudgeKind.Visible, "Norway", "Europe", 3),
            "4,000 miles · northeast · his time 09:21 by the sun");

        Print(preview);

        Assert.True(preview.HasPreview);
        Assert.True(preview.IsCounter);
        Assert.False(preview.IsDoor);

        Assert.Equal("Norway", preview.FaceName);
        Assert.Equal("Norway", preview.FaceHeadline);
        Assert.Equal("a new country", preview.FaceUnder);
        Assert.Equal(AchievementMarkForm.Counter, preview.FaceForm);

        // **THE ORDINAL IS HIS WORKED COUNT PLUS THIS ONE.** He has worked 3 in Europe,
        // so this card would be his 4th.
        Assert.Contains("Your 4th country in Europe", preview.TeachingLine, StringComparison.Ordinal);

        // **AND THE REMAINDER IS THE CITED TABLE'S, WORKED OUT HERE THE SAME WAY.**
        var onIt = DxccContinents.EntitiesOn(DxccContinents.Of("Norway"));

        _output.WriteLine("");
        _output.WriteLine(
            "the table puts " + onIt + " entities on Europe, so the remainder is "
            + (onIt - 4));

        Assert.True(onIt > 4, "the cited table knows no useful count for Europe");

        Assert.Contains(
            (onIt - 4).ToString(CultureInfo.InvariantCulture) + " to go",
            preview.TeachingLine,
            StringComparison.Ordinal);

        Assert.Equal(
            "4,000 miles · northeast · his time 09:21 by the sun", preview.FactLine);

        Assert.Equal(NudgeWords.Earns, preview.ClosingLine);
    }

    /// <summary>
    /// **Assertion 2: a door's face names nothing, and neither does its teaching.**
    /// </summary>
    /// <remarks>
    /// **§3.1 IS ABSOLUTE AND THIS IS WHERE IT IS EASIEST TO BREAK.** A preview whose whole
    /// job is to be worth opening is under pressure to say what is behind the door. Nothing
    /// in the object knows.
    /// </remarks>
    [Fact]
    public void ADoorsFaceNamesNothingAndNeitherDoesItsTeaching()
    {
        var preview = NudgePreview.For(
            new NudgeReason(NudgeKind.Door, "", "", 0), "600 miles · south");

        Print(preview);

        Assert.True(preview.HasPreview);
        Assert.True(preview.IsDoor);
        Assert.False(preview.IsCounter);

        Assert.Empty(preview.FaceName);
        Assert.False(preview.HasFaceName);
        Assert.Equal("a new area", preview.FaceHeadline);
        Assert.False(preview.HasFaceUnder);
        Assert.Equal(AchievementMarkForm.Door, preview.FaceForm);

        Assert.Equal(NudgePreview.DoorTeaching, preview.TeachingLine);

        var everything = preview.FaceHeadline + " " + preview.FaceUnder + " "
            + preview.TeachingLine + " " + preview.ClosingLine;

        foreach (var continent in new[]
        {
            "Africa", "Antarctica", "Asia", "Europe", "North America", "Oceania",
            "South America",
        })
        {
            Assert.DoesNotContain(everything, continent, StringComparison.OrdinalIgnoreCase);
        }

        // **AND NO COUNT EITHER**, because a number of things behind a door describes the
        // door.
        Assert.DoesNotContain("to go", everything, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**Assertion 3: *confirmed* appears nowhere, and every count says worked.**</summary>
    /// <remarks>**§4.** Hamlet has contacts and no confirmations.</remarks>
    [Fact]
    public void ConfirmedAppearsNowhere()
    {
        foreach (var preview in new[]
        {
            NudgePreview.For(new NudgeReason(NudgeKind.Visible, "Norway", "Europe", 3)),
            NudgePreview.For(new NudgeReason(NudgeKind.Door, "", "", 0)),
            NudgePreview.For(new NudgeReason(NudgeKind.None, "", "", 0)),
        })
        {
            var everything = preview.FaceHeadline + " " + preview.FaceUnder + " "
                + preview.TeachingLine + " " + preview.FactLine + " "
                + preview.ClosingLine;

            _output.WriteLine("[" + everything.Trim() + "]");

            Assert.DoesNotContain("confirm", everything, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("verified", everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**Assertion 4: a station who earns nothing has no preview at all.**</summary>
    [Fact]
    public void AStationWhoEarnsNothingHasNoPreview()
    {
        var preview = NudgePreview.For(new NudgeReason(NudgeKind.None, "", "", 0));

        Assert.False(preview.HasPreview);
        Assert.Same(NudgePreview.None, preview);

        // **AND A COUNTER WITH NO ENTITY IS NOT A CARD EITHER**, because the face would
        // have nothing on it and a blank card is a claim that there is one.
        Assert.False(
            NudgePreview.For(new NudgeReason(NudgeKind.Visible, "", "Europe", 3))
                .HasPreview);
    }

    /// <summary>
    /// **Assertion 5: the teaching drops the remainder rather than guessing it.**
    /// </summary>
    /// <remarks>
    /// **§0.0.** The cited table knows nothing about a made-up entity, so there is no
    /// continent count to subtract from and the line stops after the ordinal. **It does not
    /// say *0 to go* and it does not say *some to go*.**
    /// </remarks>
    [Fact]
    public void TheTeachingDropsTheRemainderRatherThanGuessingIt()
    {
        var preview = NudgePreview.For(
            new NudgeReason(NudgeKind.Visible, "Nowhere At All", "Atlantis", 3));

        _output.WriteLine(preview.TeachingLine);

        Assert.Equal("Your 4th country in Atlantis", preview.TeachingLine);
        Assert.DoesNotContain("to go", preview.TeachingLine, StringComparison.Ordinal);

        // **AND A COUNTER WITH NO CONTINENT AT ALL TEACHES NOTHING RATHER THAN GUESSING.**
        var noContinent = NudgePreview.For(
            new NudgeReason(NudgeKind.Visible, "Norway", "", 0));

        Assert.False(noContinent.HasTeachingLine);
        Assert.Empty(noContinent.TeachingLine);
    }

    /// <summary>
    /// **Assertion 6: the fact line is the card's own column, and absent without a grid.**
    /// </summary>
    [Fact]
    public void TheFactLineIsTheCardsOwnColumn()
    {
        var card = Card(FarGrid);

        card.UseNudge(new NudgeReason(NudgeKind.Visible, "Austria", "Europe", 2));

        _output.WriteLine("distance : " + card.DistanceValue);
        _output.WriteLine("his time : " + card.SolarTimeValue);
        _output.WriteLine("fact     : " + card.NudgePreview.FactLine);

        Assert.Contains(card.DistanceValue, card.NudgePreview.FactLine, StringComparison.Ordinal);
        Assert.Contains(card.SolarTimeValue, card.NudgePreview.FactLine, StringComparison.Ordinal);
        Assert.Equal(card.PreviewFactLine, card.NudgePreview.FactLine);

        // **NO GRID, NO LINE** (§0.0) - not a dash, and not the operator's own clock.
        var blind = Card(grid: null);

        blind.UseNudge(new NudgeReason(NudgeKind.Visible, "Austria", "Europe", 2));

        _output.WriteLine("");
        _output.WriteLine("no grid  : [" + blind.NudgePreview.FactLine + "]");

        Assert.False(blind.NudgePreview.HasFactLine);
        Assert.Empty(blind.NudgePreview.FactLine);
        Assert.True(blind.NudgePreview.HasPreview);
    }

    /// <summary>**Assertion 7: `nudge_opened` fires with the kind and nothing else.**</summary>
    /// <remarks>**HM-DEC-018, §2.1, §R13.** Not the callsign, not the entity.</remarks>
    [Fact]
    public void NudgeOpenedFiresWithTheKindOnly()
    {
        var seen = new List<NudgeKind>();

        var card = Card(FarGrid);

        card.UseNudge(new NudgeReason(NudgeKind.Door, "", "", 0));
        card.NudgeOpened = kind => seen.Add(kind);

        card.OpenTheNudgeCommand.Execute(null);

        Assert.True(card.NudgeIsOpen);
        Assert.Equal(new[] { NudgeKind.Door }, seen);

        card.CloseTheNudgeCommand.Execute(null);

        Assert.False(card.NudgeIsOpen);
        Assert.Single(seen);
    }

    /// <summary>
    /// **Assertion 8: the popup on the screen contains a card face, drawn undimmed.**
    /// </summary>
    /// <remarks>
    /// **THE THREE GRAY LINES ARE GONE AND SOMETHING IS DRAWN IN THEIR PLACE**, read off a
    /// realized headless window rather than off the markup. What is checked is that the
    /// popup holds a quill of the right form, the entity's name, the teaching and the
    /// closing line, and that nothing in it is faded.
    /// </remarks>
    [AvaloniaFact]
    public void ThePopupOnTheScreenContainsACardFace()
    {
        var model = APanelWithOneCard();

        var window = new MainWindow { DataContext = model, Width = 1400, Height = 1200 };

        window.Show();

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        try
        {
            var card = model.DigitalCards[0];

            card.UseNudge(new NudgeReason(NudgeKind.Visible, "Norway", "Europe", 3));
            card.OpenTheNudgeCommand.Execute(null);

            for (var i = 0; i < 6; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            var popup = window.FindControl<ItemsControl>("DigitalContactCards")!
                .GetVisualDescendants()
                .OfType<Popup>()
                .FirstOrDefault(p => p.IsOpen && Holds(p));

            Assert.True(popup is not null, "the card's quill popup is not open on the window");

            var host = popup!.Host as Visual;

            Assert.True(host is not null, "the open popup has no host to read");

            var drawn = host!.GetVisualDescendants().OfType<TextBlock>()
                .Where(t => t.IsVisible && (t.Text ?? "").Trim().Length > 0)
                .Select(t => t.Text!.Trim())
                .ToList();

            foreach (var line in drawn)
            {
                _output.WriteLine("  | " + line);
            }

            // **THE FACE, THE TEACHING AND THE CLOSING LINE ARE ALL ON IT.**
            Assert.Contains("Norway", drawn);
            Assert.Contains("a new country", drawn);
            Assert.Contains(drawn, t => t.StartsWith("Your 4th country in Europe", StringComparison.Ordinal));
            Assert.Contains(NudgeWords.Earns, drawn);

            // **AND THE QUILL IS ON IT, IN THE COUNTER'S FORM.**
            var quill = host.GetVisualDescendants().OfType<AchievementMarkControl>()
                .FirstOrDefault();

            Assert.True(quill is not null, "the popup draws no quill at all");
            Assert.Equal(AchievementMarkForm.Counter, quill!.Form);

            _output.WriteLine("");
            _output.WriteLine(
                "the quill on the face: " + quill.Form + ", "
                + quill.Bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + quill.Bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)
                + " px, ink " + Describe(quill.LitBrush));

            // **RENDERED AS IT WILL LOOK WHEN EARNED, NOT DIMMED** (the instruction).
            foreach (var faded in host.GetVisualDescendants().OfType<Control>()
                .Where(c => c.Opacity < 0.999))
            {
                Assert.Fail(
                    faded.GetType().Name + " in the popup is drawn at "
                    + faded.Opacity.ToString("0.00", CultureInfo.InvariantCulture)
                    + ", so the card he would earn is previewed dimmed");
            }

            // **AND THE WORD *confirmed* IS NOT ON THE GLASS EITHER** (§4).
            Assert.DoesNotContain(
                drawn, t => t.Contains("confirm", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>True where this popup is the one the quill opens.</summary>
    private static bool Holds(Popup popup)
        => popup.Host is Visual host
            && host.GetVisualDescendants().OfType<ContentControl>()
                .Any(c => c.Name is "CardNudgePreview" or "RowNudgePreview");

    private static string Describe(Avalonia.Media.IBrush? brush)
        => brush is Avalonia.Media.ISolidColorBrush solid
            ? solid.Color.ToString()
            : brush?.GetType().Name ?? "(none)";

    private void Print(NudgePreview preview)
    {
        _output.WriteLine("face      : " + preview.FaceHeadline
            + (preview.HasFaceUnder ? "  /  " + preview.FaceUnder : ""));
        _output.WriteLine("quill     : " + preview.FaceForm);
        _output.WriteLine("teaching  : " + preview.TeachingLine);
        _output.WriteLine("fact      : " + preview.FactLine);
        _output.WriteLine("closing   : " + preview.ClosingLine);
    }

    /// <summary>One conversation card with a grid, and nothing said.</summary>
    private static Ft8ContactCard Card(string? grid)
    {
        var at = new DateTime(2026, 9, 12, 11, 2, 0, DateTimeKind.Utc);

        var facts = new Ft8CardFacts(
            Callsign: "OE1ABC",
            State: Ft8ContactState.YourMove,
            Slots: 1,
            LastAtUtc: at,
            FirstAtUtc: at,
            YouCalledHim: false,
            HeCameBack: false,
            HisMessages: 1,
            YourMessages: 0,
            HeardInAll: 1,
            ReportFromHim: null,
            ReportToHim: null,
            HeRogered: false,
            YouRogered: false,
            HeSignedOff: false,
            YouSignedOff: false,
            Grid: grid,
            HisLastPayload: null,
            YourLastMessage: null);

        return new Ft8ContactCard(
            facts, "FN00", Ft8CardActionKind.None, "", "", at.AddSeconds(10));
    }

    /// <summary>A panel holding exactly one card, the recipe unit 297 wrote.</summary>
    private static MainWindowViewModel APanelWithOneCard()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.AddSentRowForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));
        model.RecordSentForTests(Station + " " + HisCall + " FN00", Slot("02:11:00"));

        model.AddDecodeRowForTests(
            "021115", "-09", "0.2", "1240", HisCall + " " + Station + " " + FarGrid,
            Slot("02:11:15"), 14_074_000);

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        Assert.Single(model.DigitalCards);

        return model;
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
