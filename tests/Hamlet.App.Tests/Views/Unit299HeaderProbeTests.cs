using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 299, task 2: **is the card's `i` actually empty?**
/// </summary>
/// <remarks>
/// <para>**THE INSTRUCTION SAYS IT IS AND THIS MEASURES IT RATHER THAN BELIEVING
/// IT.** *Nothing here describes the tree. Check every claim and report mismatches.*
/// Unit 297 built `Ft8ContactCard.Detail` and its own test asserted the string is
/// full, so if the operator saw nothing the fault is between the property and the
/// screen and not in the property.</para>
/// <para>**IT READS THE TOOLTIP OFF THE REALIZED CONTROL**, because that is what
/// hovering actually produces. `HintMarkControl` sets the tip inside
/// `MeasureOverride` and draws nothing at all when its text is blank, so a mark that
/// never measured and a mark with no text look identical from outside.</para>
/// </remarks>
public sealed class Unit299HeaderProbeTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the probe's findings are printed.</param>
    public Unit299HeaderProbeTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What the card's `i` mark holds on a realized card.**</summary>
    [AvaloniaFact]
    public void TheCardsInfoMarkIsProbed()
    {
        var settings = Settings();
        var window = new MainWindow { DataContext = APanelWithOneCard(settings) };

        _output.WriteLine("grid in settings before the window: \""
            + settings.Operator.GridSquare + "\"");

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        var cards = window.FindControl<ItemsControl>("DigitalContactCards");

        Assert.True(cards is not null, "the cards list is not in the window");

        var marks = cards!.GetVisualDescendants().OfType<HintMarkControl>().ToList();

        _output.WriteLine("hint marks realized inside the card: " + marks.Count);

        foreach (var mark in marks)
        {
            var tip = ToolTip.GetTip(mark) as string;

            _output.WriteLine("  kind=" + mark.Kind
                + "  bounds=" + mark.Bounds
                + "  text length=" + (mark.Text?.Length ?? -1)
                + "  tip length=" + (tip?.Length ?? -1));

            if (tip is { Length: > 0 })
            {
                _output.WriteLine("  tip: " + tip);
            }
        }

        var model = (MainWindowViewModel)window.DataContext!;

        _output.WriteLine("");
        _output.WriteLine("operator grid on a row after the window came up: "
            + (model.DigitalMineDecodes.Count > 0
                ? "\"" + model.DigitalMineDecodes[0].ObserverGrid + "\""
                : "(no rows)"));
        _output.WriteLine("rows on the mine side: " + model.DigitalMineDecodes.Count);

        foreach (var row in model.DigitalMineDecodes)
        {
            _output.WriteLine("  row sent=" + row.IsSent + " sender=" + row.Sender
                + " hz=" + row.Hz + " heardOnHz=" + row.HeardOnHz);
        }

        _output.WriteLine("cards: " + model.DigitalCards.Count);
        _output.WriteLine("grid in settings after the window: \""
            + settings.Operator.GridSquare + "\"");
        _output.WriteLine("card place: " + model.DigitalCards[0].Place);
        _output.WriteLine("card facts grid: " + (model.DigitalCards[0].Facts.Grid ?? "(none)"));
        _output.WriteLine("card detail length: "
            + (model.DigitalCards.Count > 0 ? model.DigitalCards[0].Detail.Length : -1));

        var detail = marks.FirstOrDefault(m => m.Kind == HintKind.Detail);

        Assert.True(detail is not null, "the card has no Detail mark at all");

        _output.WriteLine("");
        _output.WriteLine("Detail mark text: "
            + (detail!.Text is { Length: > 0 } ? detail.Text : "(EMPTY)"));

        window.Close();
    }


    /// <summary>**Where the band paragraph goes: card, or view model?**</summary>
    /// <remarks>
    /// The window probe showed the hover carrying the grid and the slots and **no
    /// band paragraph**, on a fixture whose row carries an audio offset, a time
    /// offset and a dial. This builds the card directly with a technical record in
    /// hand, so the answer is either that the card cannot word it or that the view
    /// model never handed it over.
    /// </remarks>
    [Fact]
    public void TheBandParagraphIsProbed()
    {
        var facts = new Ft8CardFacts(
            Callsign: Station,
            State: Ft8ContactState.YourMove,
            Slots: 3,
            LastAtUtc: Slot("02:11:15"),
            FirstAtUtc: Slot("02:11:00"),
            YouCalledHim: true,
            HeCameBack: true,
            HisMessages: 1,
            YourMessages: 1,
            HeardInAll: 1,
            ReportFromHim: -9,
            ReportToHim: -12,
            HeRogered: false,
            YouRogered: false,
            HeSignedOff: false,
            YouSignedOff: false,
            Grid: "EN52",
            HisLastPayload: "EN52",
            YourLastMessage: Station + " " + HisCall + " FN00");

        var withTech = new Ft8ContactCard(
            facts, "FN00", Ft8CardActionKind.Send, "Send", "x", Slot("02:12:00"),
            new Ft8CardTechnical("1240", "0.2", 14_074_000), -21);

        var without = new Ft8ContactCard(
            facts, "FN00", Ft8CardActionKind.Send, "Send", "x", Slot("02:12:00"));

        _output.WriteLine("WITH a technical record:");
        _output.WriteLine("  " + withTech.Detail);
        _output.WriteLine("");
        _output.WriteLine("WITHOUT one:");
        _output.WriteLine("  " + without.Detail);

        Assert.Contains("1240 Hz", withTech.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("1240 Hz", without.Detail, StringComparison.Ordinal);
    }


    /// <summary>**What the view model actually hands the card.**</summary>
    [Fact]
    public void TheViewModelSideIsProbed()
    {
        var model = APanelWithOneCard();

        foreach (var row in model.DigitalMineDecodes)
        {
            _output.WriteLine("row: sent=" + row.IsSent
                + "  sender=" + row.Sender
                + "  hz=" + row.Hz
                + "  dt=" + row.Dt
                + "  heardOnHz=" + row.HeardOnHz
                + "  msg=" + row.Message);
        }

        var card = model.DigitalCards[0];

        _output.WriteLine("");
        _output.WriteLine("card place: " + card.Place);
        _output.WriteLine("card facts grid: " + (card.Facts.Grid ?? "(none)"));
        _output.WriteLine("card detail: " + card.Detail);
    }

    /// <summary>A panel holding exactly one card, in the your-turn state.</summary>
    private static AppSettings Settings()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        return settings;
    }

    /// <summary>A panel holding exactly one card, in the your-turn state.</summary>
    private static MainWindowViewModel APanelWithOneCard()
        => APanelWithOneCard(Settings());

    private static MainWindowViewModel APanelWithOneCard(AppSettings settings)
    {
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
            "021115", "-09", "0.2", "1240", HisCall + " " + Station + " EN52",
            Slot("02:11:15"), 14_074_000);

        model.CardsNowForTests = Slot("02:12:00");
        model.RebuildCardsForTests();

        Assert.Single(model.DigitalCards);

        return model;
    }

    /// <summary>A slot boundary on the evening in question, in true UTC.</summary>
    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
}
