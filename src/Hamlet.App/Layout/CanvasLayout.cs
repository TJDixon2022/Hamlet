namespace Hamlet.App.Layout;

/// <summary>Where one widget sits, and how big it is (HM-DEC-086).</summary>
/// <param name="Id">Which widget.</param>
/// <param name="X">Distance from the left of the canvas.</param>
/// <param name="Y">Distance from the top.</param>
/// <param name="Width">How wide.</param>
/// <param name="Height">How tall.</param>
/// <remarks>
/// **FREE PLACEMENT, NOT A GRID** (HM-DEC-086). These are real distances rather
/// than row and column numbers, because a grid decides for the operator how big
/// things are allowed to be, and the whole point of the canvas is that it does
/// not. Widgets snap to their neighbors when they come close, which is help
/// rather than a rule.
/// </remarks>
public sealed record Placement(
    string Id,
    double X,
    double Y,
    double Width,
    double Height);

/// <summary>
/// An arrangement of widgets, saved or offered (HM-DEC-086).
/// </summary>
/// <param name="Name">What it is called.</param>
/// <param name="Blurb">What it is for, in one line.</param>
/// <param name="Placements">What is on it and where.</param>
/// <param name="Preset">
/// True when this is one Hamlet ships rather than one the operator saved.
/// </param>
public sealed record CanvasLayout(
    string Name,
    string Blurb,
    IReadOnlyList<Placement> Placements,
    bool Preset = false)
{
    /// <summary>An empty arrangement.</summary>
    public static CanvasLayout Empty { get; } = new("", "", Array.Empty<Placement>());

    /// <summary>
    /// A copy nothing else holds a reference into.
    /// </summary>
    /// <returns>An independent arrangement with the same contents.</returns>
    /// <remarks>
    /// **A PRESET IS A STARTING POINT AND NEVER A DOCUMENT** (HM-DEC-086).
    /// Pressing one loads a fresh copy every time, and dragging things about
    /// afterward does not change the preset. So every load goes through here, and
    /// the records are immutable besides, which means the copy cannot be
    /// accidentally shallow in a way that only shows up after somebody has
    /// rearranged their canvas twice.
    /// </remarks>
    public CanvasLayout Fresh()
        => this with { Placements = Placements.Select(p => p with { }).ToList() };

    /// <summary>The same arrangement without one widget.</summary>
    /// <param name="id">Which one to take off.</param>
    /// <returns>A new arrangement.</returns>
    public CanvasLayout Without(string id)
        => this with { Placements = Placements.Where(p => p.Id != id).ToList() };

    /// <summary>The same arrangement with one widget added or moved.</summary>
    /// <param name="placement">Where it goes.</param>
    /// <returns>A new arrangement.</returns>
    public CanvasLayout With(Placement placement)
        => this with
        {
            Placements = Placements
                .Where(p => p.Id != placement.Id)
                .Append(placement)
                .ToList(),
        };

    /// <summary>True when this widget is on the canvas.</summary>
    /// <param name="id">Which one.</param>
    /// <returns>True when it is placed.</returns>
    public bool Holds(string id) => Placements.Any(p => p.Id == id);

    /// <summary>
    /// Somewhere clear to put a widget that has just been asked for.
    /// </summary>
    /// <param name="widget">What is arriving.</param>
    /// <returns>Where to put it.</returns>
    /// <remarks>
    /// Down the left and then across, past whatever is already there. Not clever,
    /// and deliberately so: a widget that appears somewhere predictable can be
    /// moved in one drag, and a widget that appears somewhere clever cannot be
    /// found at all.
    /// </remarks>
    public Placement Room(Widget widget)
    {
        const double gap = 12;
        var x = gap;
        var y = gap;

        while (Placements.Any(p => Overlaps(p, x, y, widget.Width, widget.Height)))
        {
            y += 40;

            if (y > 1400)
            {
                y = gap;
                x += 60;
            }

            if (x > 1600)
            {
                break;
            }
        }

        return new Placement(widget.Id, x, y, widget.Width, widget.Height);
    }

    private static bool Overlaps(
        Placement p, double x, double y, double width, double height)
        => x < p.X + p.Width && x + width > p.X
            && y < p.Y + p.Height && y + height > p.Y;
}
