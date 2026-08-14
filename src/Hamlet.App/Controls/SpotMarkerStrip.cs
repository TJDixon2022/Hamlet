using Avalonia;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>One spot's place on a strip: the data, and where it landed.</summary>
/// <param name="Dot">The spot itself, carrying everything the tooltip says.</param>
/// <param name="X">Pixels from the left edge of the control.</param>
/// <param name="Prominence">0 to 1, from the ranking. Drives height and ink.</param>
public sealed record SpotMarker(ActivityDot Dot, double X, double Prominence);

/// <summary>
/// A thin rail of spot markers along one edge of a frequency display: lays
/// them out on a <see cref="FrequencyAxis"/>, draws them, and hit-tests them.
/// </summary>
/// <remarks>
/// <para>THE SAME SPOTS, ON EVERY SURFACE THAT HAS AN AXIS. The neighborhood
/// map scatters its dots through its full height because it has height to
/// spare and nothing underneath them. A tape or a waterfall does not: the
/// middle belongs to the frequency scale on one and to the spectrum on the
/// other, and a dot dropped into either would sit on top of the thing the
/// operator came to read. So the markers get an edge of their own, and the
/// display underneath stays untouched.</para>
/// <para>Built for the gesture the waterfall inherits in phase 2. There the
/// operator drags a signal under the hairline, and the marker is what tells
/// them which smear on the screen somebody has already identified. That is why
/// the rail is a separate object taking an axis and a rectangle rather than
/// code inside the tape: the waterfall gets it by asking for it.</para>
/// <para>Layout is rebuilt rather than cached, unlike the map's. The map's
/// window only changes when the band does, so caching it saves real work; a
/// tape's window moves on every single frame of a drag, so a cache would be
/// rebuilt every time it was read and would only cost the check.</para>
/// </remarks>
public sealed class SpotMarkerStrip
{
    /// <summary>How wide each marker is drawn, in pixels.</summary>
    public const double BarWidth = 3.0;

    /// <summary>How far either side of a marker still counts as pointing at it.</summary>
    public const double HitTolerance = 5.0;

    /// <summary>
    /// The shortest a marker is drawn, as a share of the rail's height. The
    /// floor exists because a low-ranked spot is still a real station on a real
    /// frequency, and a marker scaled to nothing would be Hamlet quietly
    /// dropping it (§0.0).
    /// </summary>
    private const double MinimumBarShare = 0.55;

    /// <summary>The hovered marker's outline.</summary>
    private static readonly Pen HoverPen = new(Brushes.White, 1.5);

    private static readonly IBrush HoverFillBrush =
        PanelPalette.Amber.HeaderInkBrush;

    /// <summary>
    /// The ink ramp, built once. Best-ranked markers are fully opaque and the
    /// rest fade back, so a glance at the rail and a glance at the list say the
    /// same thing about what matters (HM-DEC-023).
    /// </summary>
    /// <remarks>
    /// Cached in steps rather than mixed per marker. A tape redraws its rail on
    /// every frame of a flick, and allocating a brush per marker per frame is
    /// exactly the allocation churn HM-DEC-006 keeps off the render path. The
    /// steps are finer than the eye resolves in a three-pixel bar.
    /// </remarks>
    private static readonly IBrush[] InkRamp = BuildRamp();

    private SpotMarker[] _markers = Array.Empty<SpotMarker>();

    /// <summary>
    /// Whether bars grow up from the rail's lower edge, which is what a rail
    /// along the top wants. A rail along the bottom sets this false so its bars
    /// hang from the upper edge and still share a baseline with the display.
    /// </summary>
    public bool GrowUpward { get; init; } = true;

    /// <summary>The markers currently laid out, best-ranked first.</summary>
    public IReadOnlyList<SpotMarker> Markers => _markers;

    /// <summary>The marker under the pointer, or null.</summary>
    public SpotMarker? Hovered { get; private set; }

    /// <summary>Whether anything is on the rail at all.</summary>
    public bool HasMarkers => _markers.Length > 0;

    /// <summary>
    /// Place every spot the axis can see.
    /// </summary>
    /// <param name="dots">The spots, or null.</param>
    /// <param name="axis">The window being drawn.</param>
    /// <remarks>
    /// Spots outside the window are dropped rather than pinned to an edge. A
    /// marker pinned to the edge would claim a frequency it is not on.
    /// </remarks>
    public void Rebuild(IReadOnlyList<ActivityDot>? dots, FrequencyAxis axis)
    {
        if (dots is not { Count: > 0 } || !axis.IsUsable)
        {
            _markers = Array.Empty<SpotMarker>();
            Hovered = null;
            return;
        }

        var built = new List<SpotMarker>(dots.Count);

        foreach (var dot in dots)
        {
            if (!axis.Covers(dot.FrequencyHz))
            {
                continue;
            }

            built.Add(new SpotMarker(
                dot, axis.XOf(dot.FrequencyHz), Math.Clamp(dot.Prominence, 0, 1)));
        }

        _markers = built.ToArray();

        // Whatever was under the pointer has moved, so the highlight is stale
        // until the next pointer event says otherwise.
        Hovered = null;
    }

    /// <summary>
    /// Draw the rail.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="rail">Where the rail lives on the control.</param>
    /// <remarks>
    /// AN EMPTY RAIL IS NOT DRAWN. A permanent groove with nothing in it reads
    /// as "nobody is here", and Hamlet cannot know that: the same emptiness
    /// covers a quiet band, a window between two busy patches, and every spot
    /// feed being down at once. The panel summary and the conditions line are
    /// where that gets said, and they say which one it is (HM-DEC-025).
    /// </remarks>
    public void Render(DrawingContext context, Rect rail)
    {
        if (_markers.Length == 0 || rail.Height <= 0 || rail.Width <= 0)
        {
            return;
        }

        foreach (var marker in _markers)
        {
            if (ReferenceEquals(marker, Hovered))
            {
                continue;
            }

            context.DrawRectangle(
                InkRamp[RampIndex(marker.Prominence)], null,
                BarOf(marker, rail), BarWidth / 2, BarWidth / 2);
        }

        // The hovered one last and outlined, so it is never painted over by a
        // neighbor a few hertz away.
        if (Hovered is not null)
        {
            var bar = BarOf(Hovered, rail).Inflate(1);
            context.DrawRectangle(HoverFillBrush, HoverPen, bar, 2, 2);
        }
    }

    /// <summary>
    /// The marker under a point, or null.
    /// </summary>
    /// <param name="point">Pointer position on the control.</param>
    /// <param name="reach">Where a pointer counts as being on the rail.</param>
    /// <returns>The nearest marker within tolerance, or null.</returns>
    /// <remarks>
    /// The reach is deliberately taller than the rail is drawn. A three-pixel
    /// bar is an honest way to show a frequency and a cruel thing to ask
    /// somebody to hit, and this hobby's median age makes that a mainstream
    /// concern rather than a nicety (§0.6).
    /// </remarks>
    public SpotMarker? At(Point point, Rect reach)
    {
        if (!reach.Contains(point))
        {
            return null;
        }

        SpotMarker? best = null;
        var bestDistance = double.MaxValue;

        foreach (var marker in _markers)
        {
            var distance = Math.Abs(marker.X - point.X);

            if (distance <= (BarWidth / 2) + HitTolerance && distance < bestDistance)
            {
                best = marker;
                bestDistance = distance;
            }
        }

        return best;
    }

    /// <summary>
    /// Move the highlight.
    /// </summary>
    /// <param name="marker">The marker under the pointer, or null.</param>
    /// <returns>True when the highlight actually moved.</returns>
    public bool SetHover(SpotMarker? marker)
    {
        if (ReferenceEquals(marker, Hovered))
        {
            return false;
        }

        Hovered = marker;
        return true;
    }

    private Rect BarOf(SpotMarker marker, Rect rail)
    {
        // Height carries the ranking alongside the ink, so the rail still says
        // what matters when the color is gone (§0.6).
        var height = rail.Height
            * (MinimumBarShare + ((1 - MinimumBarShare) * marker.Prominence));

        var top = GrowUpward ? rail.Bottom - height : rail.Top;

        return new Rect(marker.X - (BarWidth / 2), top, BarWidth, height);
    }

    private static int RampIndex(double prominence)
        => (int)Math.Round(Math.Clamp(prominence, 0, 1) * (InkRamp.Length - 1));

    private static IBrush[] BuildRamp()
    {
        // The tuning family's own amber, read from the palette rather than
        // written out again here (§0.6).
        var amber = PanelPalette.Amber.Title;
        var ramp = new IBrush[33];

        for (var i = 0; i < ramp.Length; i++)
        {
            var alpha = (byte)Math.Clamp(
                110 + (145.0 * i / (ramp.Length - 1)), 90, 255);

            ramp[i] = new SolidColorBrush(
                Color.FromArgb(alpha, amber.R, amber.G, amber.B));
        }

        return ramp;
    }

    /// <summary>
    /// The ink a marker of this rank is drawn in, so other surfaces showing the
    /// same spots can agree with the rail without copying the ramp.
    /// </summary>
    /// <param name="prominence">0 to 1, from the ranking.</param>
    /// <returns>The brush.</returns>
    public static IBrush BrushFor(double prominence) => InkRamp[RampIndex(prominence)];
}
