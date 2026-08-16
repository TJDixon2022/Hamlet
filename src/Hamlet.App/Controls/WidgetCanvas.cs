using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// Free placement: widgets go where the operator puts them (HM-DEC-086).
/// </summary>
/// <remarks>
/// <para>**NOT A GRID.** A grid decides in advance how big things are allowed to
/// be and where their edges may fall, and then the operator spends their time
/// negotiating with it. This is a plain surface with real coordinates, and the
/// only cleverness in it is that edges within a few pixels of a neighbor's edge
/// snap to it, so things line up without having to be lined up.</para>
/// <para>The position lives on the widget's view model rather than here, so what
/// gets saved is read off the arrangement rather than measured off the screen
/// (§0.1's spirit: the view draws what the model holds).</para>
/// </remarks>
public sealed class WidgetCanvas : Canvas
{
    /// <summary>
    /// How close an edge has to be before it lines up with a neighbor.
    /// </summary>
    /// <remarks>
    /// Ten pixels: near enough that nudging into line is easy, far enough that a
    /// deliberate small gap survives. Snapping that fights the operator is worse
    /// than no snapping.
    /// </remarks>
    public const double SnapWithin = 10;

    /// <summary>The smallest a widget may be dragged down to.</summary>
    /// <remarks>
    /// A widget resized to nothing is a widget that has been lost, and finding it
    /// again means hunting for a few pixels. This is the floor.
    /// </remarks>
    public const double Smallest = 160;

    private WidgetViewModel? _dragging;
    private Point _grabbed;
    private double _fromX;
    private double _fromY;
    private double _fromWidth;
    private double _fromHeight;
    private bool _resizing;

    /// <summary>Start moving a widget, from its header.</summary>
    /// <param name="widget">Which one.</param>
    /// <param name="at">Where the pointer is, in canvas coordinates.</param>
    public void BeginMove(WidgetViewModel widget, Point at) => Begin(widget, at, false);

    /// <summary>Start resizing a widget, from its grip.</summary>
    /// <param name="widget">Which one.</param>
    /// <param name="at">Where the pointer is, in canvas coordinates.</param>
    public void BeginResize(WidgetViewModel widget, Point at) => Begin(widget, at, true);

    private void Begin(WidgetViewModel widget, Point at, bool resizing)
    {
        _dragging = widget;
        _resizing = resizing;
        _grabbed = at;
        _fromX = widget.X;
        _fromY = widget.Y;
        _fromWidth = widget.Width;
        _fromHeight = widget.Height;
    }

    /// <summary>
    /// How much room everything on the canvas actually takes.
    /// </summary>
    /// <param name="availableSize">What the parent is offering.</param>
    /// <returns>The bounding box of everything placed, plus a margin.</returns>
    /// <remarks>
    /// A plain canvas reports nothing, because it does not care where its children
    /// went. That is exactly wrong inside a scroller: an arrangement taller than
    /// the window would have its bottom half unreachable, which is the complaint
    /// that pinned the header and the status bar in the first place (HM-DEC-051).
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);

        var right = 0.0;
        var bottom = 0.0;

        foreach (var child in Children)
        {
            if (child.DataContext is WidgetViewModel widget)
            {
                right = Math.Max(right, widget.X + widget.Width);
                bottom = Math.Max(bottom, widget.Y + widget.Height);
            }
        }

        return new Size(right + Edge, bottom + Edge);
    }

    /// <summary>Room left past the last widget, so nothing sits on the rim.</summary>
    private const double Edge = 24;

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_dragging is not { } widget)
        {
            return;
        }

        var at = e.GetPosition(this);
        var dx = at.X - _grabbed.X;
        var dy = at.Y - _grabbed.Y;

        if (_resizing)
        {
            widget.Width = Math.Max(Smallest, _fromWidth + dx);
            widget.Height = Math.Max(Smallest, _fromHeight + dy);
            Snap(widget, edges: true);
        }
        else
        {
            widget.X = Math.Max(0, _fromX + dx);
            widget.Y = Math.Max(0, _fromY + dy);
            Snap(widget, edges: false);
        }

        // So the scroller learns about a widget dragged past the bottom edge
        // while it is being dragged rather than afterward.
        InvalidateMeasure();

        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        Finish();
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        Finish();
    }

    /// <summary>
    /// The drag is over, so the arrangement is worth keeping.
    /// </summary>
    /// <remarks>
    /// Once at the end rather than on every pixel of the drag, and the widget
    /// carries its own way of saying so, which is what keeps this from depending
    /// on when the items panel was built (HM-DEC-086).
    /// </remarks>
    private void Finish()
    {
        if (_dragging is { } widget)
        {
            widget.Settled?.Invoke(widget);
        }

        _dragging = null;
    }

    /// <summary>
    /// Line one widget up with its neighbors, where it is nearly there already.
    /// </summary>
    /// <param name="widget">The one being moved.</param>
    /// <param name="edges">
    /// True while resizing, when the bottom-right corner is what moves.
    /// </param>
    /// <remarks>
    /// Pulled out and made static so it can be tested without a window
    /// (HM-DEC-086). Interaction code that can only be checked by hand is
    /// interaction code nobody checks.
    /// </remarks>
    private void Snap(WidgetViewModel widget, bool edges)
    {
        var others = Children
            .Select(c => c.DataContext)
            .OfType<WidgetViewModel>()
            .Where(w => !ReferenceEquals(w, widget))
            .ToList();

        if (edges)
        {
            widget.Width = SnapEdge(
                widget.X + widget.Width,
                others.SelectMany(o => new[] { o.X, o.X + o.Width })) - widget.X;

            widget.Height = SnapEdge(
                widget.Y + widget.Height,
                others.SelectMany(o => new[] { o.Y, o.Y + o.Height })) - widget.Y;

            widget.Width = Math.Max(Smallest, widget.Width);
            widget.Height = Math.Max(Smallest, widget.Height);

            return;
        }

        widget.X = SnapEdge(
            widget.X, others.SelectMany(o => new[] { o.X, o.X + o.Width }));

        widget.Y = SnapEdge(
            widget.Y, others.SelectMany(o => new[] { o.Y, o.Y + o.Height }));
    }

    /// <summary>
    /// The nearest neighboring edge within reach, or the value unchanged.
    /// </summary>
    /// <param name="value">Where the edge is.</param>
    /// <param name="candidates">Where the neighbors' edges are.</param>
    /// <returns>Where it should go.</returns>
    public static double SnapEdge(double value, IEnumerable<double> candidates)
    {
        var best = value;
        var closest = SnapWithin;

        foreach (var candidate in candidates)
        {
            var distance = Math.Abs(candidate - value);

            if (distance < closest)
            {
                closest = distance;
                best = candidate;
            }
        }

        return best;
    }
}
