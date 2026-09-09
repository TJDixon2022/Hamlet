using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Views;

/// <summary>
/// The notice that says a belt has been earned.
/// </summary>
/// <remarks>
/// <para>**IT MUST NOT COST HIM A CONTACT** (Tim, 2026-09-08). He is often mid-
/// exchange with fourteen seconds to reply, so this takes no keyboard focus, blocks
/// no click, and closes nothing he had open. `ShowActivated="False"` is the whole of
/// it: **a window that activates closes an open context menu**, which is the exact
/// way a dialog would cost him the reply it is congratulating him for.</para>
/// <para>**AND IT LEAVES ON ITS OWN.** A notice he has to dismiss is a click he did
/// not ask for at the moment he can least afford one — so it goes after eight
/// seconds, or on a click if he wants it gone sooner.</para>
/// </remarks>
public partial class BadgeWindow : Window
{
    /// <summary>How long the notice stays.</summary>
    /// <remarks>
    /// Eight seconds: long enough to read three short lines, and gone before the
    /// slot after next. **Half a slot would be too fast to read and two slots would
    /// still be on screen when he is answering.**
    /// </remarks>
    public static TimeSpan Stays => TimeSpan.FromSeconds(8);

    /// <summary>How far up each further notice sits, in pixels.</summary>
    /// <remarks>
    /// **TWO ACHIEVEMENTS CAN LAND ON ONE CONTACT.** A belt crossing and a mode
    /// first are separate ladders read off the same log, so both can fire at once,
    /// and without this they would be placed at the same point with one hiding the
    /// other completely. **A notice that is covered is a notice he never got**, and
    /// the acknowledgement he would lose is the rarer of the two.
    /// </remarks>
    private const int Stacks = 108;

    /// <summary>How many notices are on screen right now.</summary>
    private static int _showing;

    private readonly DispatcherTimer _leaves;

    /// <summary>Creates the notice.</summary>
    public BadgeWindow()
    {
        InitializeComponent();

        _leaves = new DispatcherTimer { Interval = Stays };
        _leaves.Tick += (_, _) => Leave();

        Opened += (_, _) => _leaves.Start();
        PointerPressed += (_, _) => Leave();

        Closed += (_, _) => _showing = Math.Max(0, _showing - 1);
    }

    /// <summary>
    /// **Show one award beside its owner, without taking anything from him.**
    /// </summary>
    /// <param name="owner">The window it belongs beside.</param>
    /// <param name="award">What was earned.</param>
    /// <remarks>
    /// <para>Never-throw (§8): a badge is an acknowledgement, and nothing about it is
    /// worth taking the application down for — least of all in the middle of a
    /// contact.</para>
    /// <para>**`Show` AND NOT `ShowDialog`.** Nothing is blocked and nothing is
    /// awaited, so the caller carries on and so does he.</para>
    /// </remarks>
    public static void Announce(Window? owner, BadgeAward award)
    {
        ArgumentNullException.ThrowIfNull(award);

        if (award.Says.Length == 0)
        {
            return;
        }

        try
        {
            var notice = new BadgeWindow { DataContext = award };

            if (owner is not null)
            {
                // Bottom-right of the owner, out of the way of the decoded lists
                // and the Send controls, which are where he is looking and
                // clicking.
                //
                // **AND ABOVE ANYTHING ALREADY THERE.** A belt crossing and a mode
                // first are two ladders read off one log, so both can arrive on the
                // same contact, and stacked is the difference between two
                // acknowledgements and one.
                notice.Position = new Avalonia.PixelPoint(
                    owner.Position.X + Math.Max(0, (int)owner.Width - 360),
                    owner.Position.Y
                        + Math.Max(0, (int)owner.Height - 200 - (_showing * Stacks)));
            }

            _showing++;

            notice.Show();
        }
        catch (Exception)
        {
            // A notice that could not be shown is a notice nobody sees. The count
            // and the rank are still on the status bar either way.
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>Stop the clock and go.</summary>
    private void Leave()
    {
        _leaves.Stop();
        Close();
    }
}
