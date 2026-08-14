using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// The key to the band map: the four mode families, the listen-only hatch,
/// and the activity dot.
/// </summary>
/// <remarks>
/// <para>This is the piece that turns the color from decoration into a
/// teaching device (HM-DEC-032). Somebody who reads it once knows what every
/// wash on the map means for good, and the map stops being scenery.</para>
/// <para>It also discharges half the obligation that color is never the only
/// carrier of meaning: each swatch is named in words beside it, and the
/// listen-only entry draws the actual hatch rather than describing it.</para>
/// <para>Wraps to a second line when the panel is narrow, so it degrades on a
/// small window rather than clipping.</para>
/// </remarks>
public sealed class MapLegendControl : Control
{
    private const double SwatchWidth = 16;
    private const double SwatchHeight = 10;
    private const double Gap = 6;
    private const double ItemGap = 16;
    private const double LineHeight = 17;

    private static readonly Typeface Sans = new("Segoe UI,Inter,sans-serif");
    private static readonly IBrush TextBrush = new SolidColorBrush(Color.Parse("#55534E"));
    private static readonly Pen SwatchEdge =
        new(new SolidColorBrush(Color.Parse("#33000000")), 0.7);

    private static readonly IBrush VeilBrush =
        new SolidColorBrush(Color.FromArgb(0x33, 0x3A, 0x3A, 0x44));
    private static readonly Pen VeilPen = new(VeilBrush, 2.5);
    private static readonly IBrush DotBrush = PanelPalette.Amber.TitleBrush;

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width is double.PositiveInfinity or <= 0
            ? 600
            : availableSize.Width;

        return new Size(width, Layout(width, null));
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
        => Layout(Bounds.Width, context);

    /// <summary>
    /// Place every entry, and draw them when given a context.
    /// </summary>
    /// <param name="width">Available width.</param>
    /// <param name="context">Drawing context, or null to measure only.</param>
    /// <returns>The height the legend needs.</returns>
    /// <remarks>
    /// One pass that both measures and draws, so the wrap can never differ
    /// between the two.
    /// </remarks>
    private double Layout(double width, DrawingContext? context)
    {
        double x = 0;
        double y = 0;

        // Legend rather than All: the map draws a fifth region that is not a
        // mode family, and a wash nobody can decode is decoration that looks
        // like information (§0.6, HM-DEC-055).
        foreach (var colors in ModePalette.Legend)
        {
            Place(context, colors.Label, ref x, ref y, width, (rect, c) =>
            {
                c.FillRectangle(colors.FillBrush, rect, 2);
                c.DrawRectangle(null, SwatchEdge, rect, 2, 2);
            });
        }

        // The hatch is drawn rather than described: the legend should show the
        // same mark the map shows.
        Place(context, "listen only", ref x, ref y, width, (rect, c) =>
        {
            c.FillRectangle(ModePalette.Open.FillBrush, rect, 2);
            using (c.PushClip(rect))
            {
                for (var hx = rect.X - rect.Height; hx < rect.Right + rect.Height; hx += 5)
                {
                    c.DrawLine(
                        VeilPen,
                        new Point(hx, rect.Bottom),
                        new Point(hx + rect.Height, rect.Y));
                }
            }

            c.DrawRectangle(null, SwatchEdge, rect, 2, 2);
        });

        Place(context, "heard just now", ref x, ref y, width, (rect, c)
            => c.DrawEllipse(DotBrush, null, rect.Center, 3.5, 3.5));

        return y + LineHeight;
    }

    private static void Place(
        DrawingContext? context,
        string label,
        ref double x,
        ref double y,
        double width,
        Action<Rect, DrawingContext> drawSwatch)
    {
        var text = new FormattedText(
            label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Sans, 11, TextBrush);

        var itemWidth = SwatchWidth + Gap + text.Width;

        if (x > 0 && x + itemWidth > width)
        {
            x = 0;
            y += LineHeight;
        }

        if (context is not null)
        {
            var swatch = new Rect(
                x, y + ((LineHeight - SwatchHeight) / 2) - 1, SwatchWidth, SwatchHeight);

            drawSwatch(swatch, context);

            context.DrawText(
                text, new Point(x + SwatchWidth + Gap, y + ((LineHeight - text.Height) / 2)));
        }

        x += itemWidth + ItemGap;
    }
}
