using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Logging;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 297, task 2: **the card template draws, and every binding in it
/// resolves.**
/// </summary>
/// <remarks>
/// <para>**§0.5.1 IS ABSOLUTE AND `BindingHealthTests` CANNOT REACH THIS.** That test
/// builds the window over an empty view model, so `DigitalCards` is empty, the card
/// `DataTemplate` is never realized and not one binding inside it is ever evaluated.
/// **A whole template can be wrong and that sweep stays green.**</para>
/// <para>**AND THE FAILURE IS SILENT.** Avalonia yields null on a failed cast rather
/// than throwing, so a card button whose command did not resolve renders and behaves
/// exactly like a disabled one - which is HM-DEC-087, and it is the reason every
/// command on this template reaches the view model through
/// `$parent[ItemsControl]` rather than through the item.</para>
/// <para>**WHAT THE WATCHED-FAILING ATTEMPT ACTUALLY SHOWED, AND IT CORRECTS THIS
/// TEST'S OWN CLAIM.** Pointing the action button's command at the item instead of
/// the parent does not produce a silent runtime failure here: **compiled bindings are
/// on** (§6) and the `x:DataType` on the template makes it a build error,
/// `CardActionCommand` not being a member of `Ft8ContactCard`. So the HM-DEC-087
/// class of fault is caught by the compiler on this template rather than by this
/// test.</para>
/// <para>**WHICH LEAVES THIS TEST A NARROWER AND STILL REAL JOB**: proving the
/// template is realized at all, that its controls are live rather than drawn, and
/// that nothing Avalonia logs as a binding complaint comes out of it. A card whose
/// list never realizes, or whose button carries a null command for any other reason,
/// is a panel of labels that looks exactly like a panel of buttons.</para>
/// </remarks>
public sealed class Unit297CardBindingTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the drawn card is printed.</param>
    public Unit297CardBindingTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A realized card complains about nothing and draws its parts.**</summary>
    [AvaloniaFact]
    public void TheCardTemplateBindsWithoutOneComplaint()
    {
        var complaints = new List<string>();
        var was = Logger.Sink;

        Logger.Sink = new Collector(complaints);

        try
        {
            var window = new MainWindow { DataContext = APanelWithOneCard() };

            window.Show();

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            // **BY NAME AND NOT BY WALKING THE VISUAL TREE.** The Digital
            // workspace is one of three and only the visible one is realized, so a
            // visual walk finds nothing on a window that opened on another tab. The
            // name scope is the window's own and holds it either way.
            var cards = window.FindControl<ItemsControl>("DigitalContactCards");

            Assert.True(cards is not null, "the cards list is not in the window");

            var drawn = cards!.GetVisualDescendants().OfType<TextBlock>()
                .Select(t => t.Text ?? "")
                .Where(t => t.Trim().Length > 0)
                .ToList();

            foreach (var line in drawn)
            {
                _output.WriteLine("  | " + line);
            }

            // **THE CARD IS ACTUALLY ON THE SCREEN**, which is what makes the
            // binding sweep below mean anything: a template that never realized
            // would complain about nothing either.
            Assert.Contains(Station, drawn);
            Assert.Contains("Your turn", drawn);
            Assert.Contains(drawn, t => t.StartsWith("K9XP came back", StringComparison.Ordinal));
            Assert.Contains(drawn, t => t.EndsWith("UTC", StringComparison.Ordinal)
                                        || t.Contains(" UTC · ", StringComparison.Ordinal));

            // **THE ACTION IS A LIVE CONTROL AND NOT A LABEL** (§0.5.1). A button
            // with a null command is indistinguishable from a disabled one, so the
            // command is read off the control rather than trusted.
            var action = cards.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Classes.Contains("hm-cardaction"));

            Assert.True(action is not null, "the card has no action button");
            Assert.True(action!.Command is not null, "the action button's command is null");
            Assert.True(action.IsEffectivelyEnabled, "the action button is not usable");

            var clear = cards.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(b => b.Classes.Contains("hm-cardx"));

            Assert.True(clear is not null, "the card has no X");
            Assert.True(clear!.Command is not null, "the X's command is null");

            // **THE RING LEFT THIS CARD IN UNIT 305 AND THIS LINE DID NOT** (rewritten
            // under §R12, work instruction 330 task 2). It asserted that a visible `Arc`
            // was somewhere under the cards list, which was unit 297's *the ring is
            // beside the button that needs it*; unit 305 moved the slot clock above the
            // panels, because a countdown inside a container that may not exist is a
            // countdown that vanishes on the evening nobody answers. **So this has been
            // red on a stale premise ever since**, and a red here stops the run before
            // the binding sweep at the bottom - which is the assertion this class exists
            // for and the one a rebuilt card template needs.
            //
            // **WHAT REPLACES IT IS THE SAME QUESTION ASKED WHERE THE ANSWER LIVES**: the
            // ring is on the window, once, and nothing duplicated it back onto the card.
            Assert.Empty(cards.GetVisualDescendants().OfType<Arc>());
            Assert.True(
                window.FindControl<Control>("SlotClock") is not null,
                "the slot clock is not on the window, so the ring unit 305 moved there "
                + "is not anywhere at all");

            window.Close();
        }
        finally
        {
            Logger.Sink = was;
        }

        var bindings = complaints
            .Where(l => l.Contains("[Binding]", StringComparison.Ordinal))
            .Distinct()
            .ToList();

        Assert.True(
            bindings.Count == 0,
            "the card template has bindings that do not resolve, and a control "
            + "bound to nothing looks and behaves exactly like a disabled one:"
            + Environment.NewLine + string.Join(Environment.NewLine, bindings));
    }

    /// <summary>A panel holding exactly one card, in the your-turn state.</summary>
    private static MainWindowViewModel APanelWithOneCard()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        // **THE DIGITAL TAB, BECAUSE THAT IS THE ONE THE PANEL IS ON**, and the
        // panel expanded, because a collapsed one never realizes its contents and
        // a template that was never realized complains about nothing.
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
            "021115", "-09", "0.2", "1240", HisCall + " " + Station + " -09",
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

    /// <summary>Keeps every line Avalonia logs while the window is up.</summary>
    private sealed class Collector : ILogSink
    {
        private readonly List<string> _lines;

        public Collector(List<string> lines) => _lines = lines;

        public bool IsEnabled(LogEventLevel level, string area) => true;

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
            => _lines.Add("[" + area + "] " + messageTemplate);

        public void Log(
            LogEventLevel level, string area, object? source, string messageTemplate,
            params object?[] propertyValues)
            => _lines.Add("[" + area + "] " + messageTemplate + " "
                          + string.Join(", ", propertyValues));
    }
}
