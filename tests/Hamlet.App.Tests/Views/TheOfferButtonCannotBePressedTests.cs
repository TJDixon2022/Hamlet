using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 282, task 4: the button that expanded a panel that is not there.
/// </summary>
/// <remarks>
/// <para>**A CONTROL'S RESTING APPEARANCE SAYS IT CAN BE PRESSED** (§0.5.1,
/// HM-DEC-087). The CW tab's *Have a look* button ran `OpenReceiveHelpCommand`,
/// which sets `ReceiveHelpExpanded` on `widget.receiveHelp` — one of thirteen data
/// templates in `MainWindow.axaml` that nothing references. **Pressing it did
/// nothing and looked exactly like pressing a button that works**, which is the
/// fault `BindingHealthTests` cannot catch, because the binding resolves perfectly
/// onto a property nothing renders.</para>
/// <para>**IT IS DISABLED RATHER THAN REMOVED.** Whether to rehome the widget,
/// delete the templates or fold the advice elsewhere is `HM-OPEN-087` and Tim's;
/// taking the button away would tidy the evidence out of sight before he has ruled.
/// §0.5.1's exception is for a control that genuinely cannot be used, and it still
/// has to say why.</para>
/// </remarks>
public sealed class TheOfferButtonCannotBePressedTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the button's state is printed.</param>
    public TheOfferButtonCannotBePressedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**The command refuses, so the button is drawn as unusable.**</summary>
    /// <remarks>
    /// Watched failing first: the command had no `CanExecute` and the button was
    /// enabled.
    /// </remarks>
    [AvaloniaFact]
    public void TheButtonIsDrawnAsSomethingThatCannotBeUsed()
    {
        var panel = new MainWindowViewModel(
            HowMuchTheApplicationSaysTests.Settled(), null)
        {
            OperatingMode = "CW",
        };

        var window = new MainWindow { DataContext = panel };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        var button = window.GetVisualDescendants().OfType<Button>()
            .FirstOrDefault(b => b.Name == "ReceiveHelpOfferButton");

        Assert.True(button is not null, "no button named ReceiveHelpOfferButton");

        _output.WriteLine("enabled : " + button!.IsEffectivelyEnabled);
        _output.WriteLine("why     : " + (ToolTip.GetTip(button) as string ?? "(none)"));

        Assert.False(panel.CanOpenReceiveHelp);
        Assert.False(button.IsEffectivelyEnabled);
    }

    /// <summary>**And it says why, rather than being grey and silent.**</summary>
    /// <remarks>
    /// §0.5.1: grey is reserved for what genuinely cannot be used, **and it still
    /// says what would change it.** A control that is dead and mute teaches somebody
    /// that the application is broken.
    /// </remarks>
    [AvaloniaFact]
    public void ItSaysWhyItCannotBePressed()
    {
        var why = MainWindowViewModel.ReceiveHelpUnreachable;

        _output.WriteLine(why);

        Assert.Contains("not on any screen", why, StringComparison.Ordinal);
        Assert.Contains("cannot do anything", why, StringComparison.Ordinal);

        // **AND IT SENDS HIM NOWHERE THAT DOES NOT EXIST.** A first draft pointed
        // at the Radio menu, which carries Connect, Favorites and Recent and has
        // never carried the receive help. A tooltip naming a screen that is not
        // there is the same fault as the button, one level along (§0.0).
        Assert.DoesNotContain("under Radio", why, StringComparison.Ordinal);
    }
}
