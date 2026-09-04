using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Hamlet.App.Controls;

/// <summary>
/// Keeps a scrolling list at the end new rows arrive at, unless the operator has
/// scrolled away to read something.
/// </summary>
/// <remarks>
/// <para>**A LIST THAT YANKS ITSELF IS UNUSABLE AT FOUR SLOTS A MINUTE.** FT8
/// delivers a batch of rows every fifteen seconds, and on the band measured on
/// 2026-09-04 that was fourteen at a time. Somebody reading a callsign they
/// missed must be able to finish reading it.</para>
/// <para>**IT WORKS OUT WHICH END IS THE LIVE END RATHER THAN BEING TOLD.** The
/// panel can be ordered newest-first or oldest-first, so "the end" is the top in
/// one and the bottom in the other. Watching where the rows are actually being
/// inserted answers that without this control knowing the setting exists, which
/// means the sort toggle cannot get out of step with the scrolling.</para>
/// <para>**NEAR THE END, NOT AT IT.** Forty pixels of tolerance, the same figure
/// `CwTerminalControl` has used since the CW terminal was built. Requiring an
/// exact offset would stop following after any rounding, and the symptom would
/// be a list that follows sometimes.</para>
/// </remarks>
public static class FollowingScroll
{
    /// <summary>How close to the end still counts as being at it.</summary>
    private const double Tolerance = 40;

    /// <summary>Attach to the items control whose collection should be followed.</summary>
    public static readonly AttachedProperty<bool> FollowsProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "Follows", typeof(FollowingScroll));

    /// <summary>Reads the attached flag.</summary>
    /// <param name="control">The control.</param>
    /// <returns>Whether it follows.</returns>
    public static bool GetFollows(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);

        return control.GetValue(FollowsProperty);
    }

    /// <summary>Sets the attached flag.</summary>
    /// <param name="control">The control.</param>
    /// <param name="value">Whether it should follow.</param>
    public static void SetFollows(Control control, bool value)
    {
        ArgumentNullException.ThrowIfNull(control);

        control.SetValue(FollowsProperty, value);
    }

    static FollowingScroll()
        => FollowsProperty.Changed.AddClassHandler<Control>(OnFollowsChanged);

    private static void OnFollowsChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (control is not ItemsControl items)
        {
            return;
        }

        if (e.NewValue is true)
        {
            items.PropertyChanged += OnItemsSourceChanged;
            Subscribe(items, items.ItemsSource);
        }
        else
        {
            items.PropertyChanged -= OnItemsSourceChanged;
            Subscribe(items, null);
        }
    }

    private static void OnItemsSourceChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is ItemsControl items && e.Property == ItemsControl.ItemsSourceProperty)
        {
            Subscribe(items, items.ItemsSource);
        }
    }

    /// <summary>Where the rows for each watched control are coming from.</summary>
    /// <remarks>
    /// **A CONDITIONAL WEAK TABLE, SO WATCHING A LIST CANNOT KEEP A WINDOW
    /// ALIVE.** The alternative is a dictionary keyed on the control, which is a
    /// leak with a friendly name.
    /// </remarks>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<
        ItemsControl, NotifyCollectionChangedEventHandler> Watched = new();

    private static void Subscribe(ItemsControl items, IEnumerable? source)
    {
        if (Watched.TryGetValue(items, out var existing))
        {
            if (items.ItemsSource is INotifyCollectionChanged had)
            {
                had.CollectionChanged -= existing;
            }

            Watched.Remove(items);
        }

        if (source is not INotifyCollectionChanged notifying)
        {
            return;
        }

        void Handler(object? _, NotifyCollectionChangedEventArgs e) => Follow(items, e);

        Watched.Add(items, Handler);
        notifying.CollectionChanged += Handler;
    }

    private static void Follow(ItemsControl items, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add)
        {
            return;
        }

        var scroller = items.FindAncestorOfType<ScrollViewer>();

        if (scroller is null)
        {
            return;
        }

        // **WHICH END THE ROWS ARE ARRIVING AT DECIDES WHICH END TO HOLD.**
        // Inserted at nought means newest-first and the live end is the top;
        // anywhere else means they are being appended and it is the bottom.
        var atTop = e.NewStartingIndex == 0;

        var fromEnd = atTop
            ? scroller.Offset.Y
            : scroller.Extent.Height - scroller.Offset.Y - scroller.Viewport.Height;

        if (fromEnd > Tolerance)
        {
            // The operator has scrolled away to read something. Leave it alone.
            return;
        }

        // **AFTER THE LAYOUT PASS, NOT DURING IT.** The rows that were just added
        // have not been measured yet, so the extent this would scroll against is
        // the old one.
        Dispatcher.UIThread.Post(
            () =>
            {
                if (atTop)
                {
                    scroller.Offset = scroller.Offset.WithY(0);
                }
                else
                {
                    scroller.ScrollToEnd();
                }
            },
            DispatcherPriority.Background);
    }
}
