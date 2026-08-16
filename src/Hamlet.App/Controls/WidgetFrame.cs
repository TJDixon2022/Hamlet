using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// The chrome around one widget: a header to move it by and a grip to resize it
/// (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>Built once rather than per widget, for the same reason the collapsible
/// panel was (§0.5): thirteen copies of a header bar is thirteen places for it to
/// drift.</para>
/// <para>The panel inside keeps its own header, its own family color and its own
/// collapse, because none of that stopped being true when it gained a position.
/// This adds the two things a fixed column did not need, and nothing else.</para>
/// </remarks>
public sealed class WidgetFrame : ContentControl
{
    /// <summary>What this handle does when it is dragged.</summary>
    public static readonly StyledProperty<bool> IsGripProperty =
        AvaloniaProperty.Register<WidgetFrame, bool>(nameof(IsGrip));

    /// <summary>True when dragging this resizes rather than moves.</summary>
    public bool IsGrip
    {
        get => GetValue(IsGripProperty);
        set => SetValue(IsGripProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (DataContext is not WidgetViewModel widget)
        {
            return;
        }

        // THE VISUAL TREE, NOT THE LOGICAL ONE, AND THAT IS WHY DRAGGING NEVER
        // WORKED (HM-DEC-087). An items control's containers are its own logical
        // children and the panel is only a visual one, so walking the logical
        // tree from here reaches the items control and never passes through the
        // canvas at all. It returned null on every press, the drag never began,
        // and nothing anywhere said so.
        if (this.FindAncestorOfType<WidgetCanvas>() is not { } canvas)
        {
            return;
        }

        var at = e.GetPosition(canvas);

        if (IsGrip)
        {
            canvas.BeginResize(widget, at);
        }
        else
        {
            canvas.BeginMove(widget, at);
        }

        // THE CANVAS TAKES THE POINTER, NOT THIS CONTROL, and that is what makes
        // it safe to reorder immediately afterward: bringing the widget to the
        // front rebuilds its container, and this frame goes with it (HM-DEC-087).
        e.Pointer.Capture(canvas);

        widget.Raised?.Invoke(widget);

        e.Handled = true;
    }
}
