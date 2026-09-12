using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// **The eight badge emblems, drawn as vector paths.**
/// </summary>
/// <remarks>
/// <para>**NO IMAGE ASSETS** (work instruction 331 section 10, and §0's rule about
/// generated rather than copied). `assets/achievements-opening-mockup.png` is the shape the
/// owner approved and is **for reading, not for shipping**: nothing in `src/` references
/// it, and every emblem below is geometry in this file at whatever size the badge is
/// given.</para>
/// <para>**THE COUNTRIES EMBLEM IS HAMLET'S OWN AND NOT A REAL FLAG** (the instruction
/// says so twice). Three plain pennants on poles in three of the page's own colors: the
/// idea *flags* without asserting a nation, which is the one thing a real flag on an
/// achievements page would do wrong.</para>
/// <para>**EVERY EMBLEM SITS BESIDE ITS OWN NAME IN WORDS** (§0.6). The badge's band
/// carries `Countries` two millimetres to the right of the pennants, so nobody has to
/// recognize a drawing to read the page - which is also why none of these has to be a
/// masterpiece.</para>
/// <para>**ONE CONTROL AND NOT EIGHT.** Eight controls is eight places for a stroke width
/// to drift, and the page draws them from one `Kind` string that the view model supplies.
/// An unknown kind draws nothing rather than a placeholder: an emblem nobody chose is a
/// picture asserting something (§0.0).</para>
/// </remarks>
public sealed class BadgeEmblemControl : Control
{
    /// <summary>Which emblem to draw. See the eight names in `AchievementBadges`.</summary>
    public static readonly StyledProperty<string> EmblemProperty =
        AvaloniaProperty.Register<BadgeEmblemControl, string>(nameof(Emblem), "");

    /// <summary>What to draw it in. White on the color band.</summary>
    public static readonly StyledProperty<IBrush?> InkProperty =
        AvaloniaProperty.Register<BadgeEmblemControl, IBrush?>(nameof(Ink));

    /// <summary>The three pennant colors, which are three of the page's own bands.</summary>
    private static readonly IBrush[] Pennants =
    {
        new SolidColorBrush(Color.Parse("#D9534F")),
        new SolidColorBrush(Color.Parse("#4F7FBF")),
        new SolidColorBrush(Color.Parse("#E0A82E")),
    };

    static BadgeEmblemControl()
        => AffectsRender<BadgeEmblemControl>(EmblemProperty, InkProperty);

    /// <summary>Which emblem to draw.</summary>
    public string Emblem
    {
        get => GetValue(EmblemProperty);
        set => SetValue(EmblemProperty, value);
    }

    /// <summary>What to draw it in.</summary>
    public IBrush? Ink
    {
        get => GetValue(InkProperty);
        set => SetValue(InkProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => new(26, 26);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var side = Math.Min(Bounds.Width, Bounds.Height);

        if (side <= 1)
        {
            return;
        }

        // **EVERY EMBLEM IS DRAWN IN A UNIT BOX AND SCALED**, so a badge at any size gets
        // the same drawing rather than the same pixels - the fault unit 330 found in the
        // tray mark, where a ruled number went onto the box instead of onto the ink.
        var ink = Ink ?? Brushes.White;
        var pen = new Pen(ink, side * 0.09, lineCap: PenLineCap.Round);
        var box = new Rect(
            (Bounds.Width - side) / 2, (Bounds.Height - side) / 2, side, side);

        switch (Emblem)
        {
            case "trophy":
                Trophy(context, box, ink, pen);
                break;
            case "americas":
                Americas(context, box, ink);
                break;
            case "flags":
                Flags(context, box, pen);
                break;
            case "star":
                Star(context, box, ink);
                break;
            case "grid":
                GridOfNine(context, box, ink, pen);
                break;
            case "globe":
                Globe(context, box, ink, pen);
                break;
            case "waves":
                Waves(context, box, pen);
                break;
            case "modes":
                Modes(context, box, ink, pen);
                break;
            default:
                // **NOTHING, ON PURPOSE.** An emblem nobody chose would be a picture
                // asserting a kind that does not exist.
                break;
        }
    }

    /// <summary>A trophy: a bowl, two handles, a stem and a base.</summary>
    private static void Trophy(DrawingContext context, Rect box, IBrush ink, Pen pen)
    {
        var w = box.Width;
        var h = box.Height;

        var bowl = new StreamGeometry();

        using (var draw = bowl.Open())
        {
            draw.BeginFigure(new Point(box.X + (w * 0.28), box.Y + (h * 0.16)), true);
            draw.LineTo(new Point(box.X + (w * 0.72), box.Y + (h * 0.16)));
            draw.LineTo(new Point(box.X + (w * 0.66), box.Y + (h * 0.52)));
            draw.LineTo(new Point(box.X + (w * 0.34), box.Y + (h * 0.52)));
            draw.EndFigure(true);
        }

        context.DrawGeometry(ink, null, bowl);

        // The two handles, as arcs of a pen rather than as filled shapes.
        context.DrawLine(
            pen,
            new Point(box.X + (w * 0.28), box.Y + (h * 0.22)),
            new Point(box.X + (w * 0.14), box.Y + (h * 0.36)));
        context.DrawLine(
            pen,
            new Point(box.X + (w * 0.72), box.Y + (h * 0.22)),
            new Point(box.X + (w * 0.86), box.Y + (h * 0.36)));

        context.DrawLine(
            pen,
            new Point(box.X + (w * 0.5), box.Y + (h * 0.52)),
            new Point(box.X + (w * 0.5), box.Y + (h * 0.74)));

        context.FillRectangle(
            ink,
            new Rect(
                box.X + (w * 0.3), box.Y + (h * 0.76), w * 0.4, h * 0.12));
    }

    /// <summary>
    /// **The Americas, as a silhouette Hamlet drew** - a broad north and a narrow south
    /// joined at an isthmus.
    /// </summary>
    /// <remarks>
    /// **IT IS A SHAPE AND NOT A MAP** (§0.0 binds pictures). Nothing on this page aligns
    /// to it, no coastline is claimed and no frequency or distance is read off it; it is
    /// the emblem of a kind called Continents and the mockup's own choice.
    /// </remarks>
    private static void Americas(DrawingContext context, Rect box, IBrush ink)
    {
        var w = box.Width;
        var h = box.Height;

        var shape = new StreamGeometry();

        using (var draw = shape.Open())
        {
            draw.BeginFigure(new Point(box.X + (w * 0.14), box.Y + (h * 0.16)), true);
            draw.LineTo(new Point(box.X + (w * 0.86), box.Y + (h * 0.16)));
            draw.LineTo(new Point(box.X + (w * 0.60), box.Y + (h * 0.52)));
            draw.LineTo(new Point(box.X + (w * 0.70), box.Y + (h * 0.58)));
            draw.LineTo(new Point(box.X + (w * 0.50), box.Y + (h * 0.90)));
            draw.LineTo(new Point(box.X + (w * 0.38), box.Y + (h * 0.58)));
            draw.LineTo(new Point(box.X + (w * 0.46), box.Y + (h * 0.52)));
            draw.EndFigure(true);
        }

        context.DrawGeometry(ink, null, shape);
    }

    /// <summary>
    /// **Three pennants on poles - Hamlet's emblem, and not a real flag of anywhere.**
    /// </summary>
    private static void Flags(DrawingContext context, Rect box, Pen pen)
    {
        var w = box.Width;
        var h = box.Height;
        var pole = new Pen(pen.Brush, pen.Thickness * 0.7);

        for (var i = 0; i < 3; i++)
        {
            var x = box.X + (w * (0.24 + (i * 0.24)));
            var top = box.Y + (h * (0.18 + (i == 1 ? 0.0 : 0.08)));

            context.DrawLine(pole, new Point(x, top), new Point(x, box.Y + (h * 0.88)));

            var pennant = new StreamGeometry();

            using (var draw = pennant.Open())
            {
                draw.BeginFigure(new Point(x, top), true);
                draw.LineTo(new Point(x + (w * 0.17), top + (h * 0.07)));
                draw.LineTo(new Point(x, top + (h * 0.16)));
                draw.EndFigure(true);
            }

            context.DrawGeometry(Pennants[i], null, pennant);
        }
    }

    /// <summary>A five-pointed star.</summary>
    private static void Star(DrawingContext context, Rect box, IBrush ink)
    {
        var mid = box.Center;
        var outer = box.Width * 0.44;
        var inner = outer * 0.42;

        var star = new StreamGeometry();

        using (var draw = star.Open())
        {
            for (var i = 0; i < 10; i++)
            {
                var radius = i % 2 == 0 ? outer : inner;
                var angle = (-Math.PI / 2) + (i * Math.PI / 5);
                var point = new Point(
                    mid.X + (radius * Math.Cos(angle)),
                    mid.Y + (radius * Math.Sin(angle)));

                if (i == 0)
                {
                    draw.BeginFigure(point, true);
                }
                else
                {
                    draw.LineTo(point);
                }
            }

            draw.EndFigure(true);
        }

        context.DrawGeometry(ink, null, star);
    }

    /// <summary>A three-by-three grid with one square filled.</summary>
    /// <remarks>
    /// **ONE FILLED SQUARE AND NOT A COUNT.** The emblem says what a grid square is; how
    /// many he has is in the badge's corner, in words. A filled count here would be a
    /// picture making a claim the corner already makes better.
    /// </remarks>
    private static void GridOfNine(DrawingContext context, Rect box, IBrush ink, Pen pen)
    {
        var side = box.Width * 0.62;
        var cell = side / 3;
        var x0 = box.Center.X - (side / 2);
        var y0 = box.Center.Y - (side / 2);
        var thin = new Pen(pen.Brush, pen.Thickness * 0.8);

        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                var cellBox = new Rect(
                    x0 + (column * cell), y0 + (row * cell), cell, cell);

                if (row == 1 && column == 1)
                {
                    context.FillRectangle(ink, cellBox.Deflate(cell * 0.14));
                }
                else
                {
                    context.DrawRectangle(null, thin, cellBox.Deflate(cell * 0.14));
                }
            }
        }
    }

    /// <summary>A globe with an arc over it - grid to grid, added up.</summary>
    private static void Globe(DrawingContext context, Rect box, IBrush ink, Pen pen)
    {
        var mid = new Point(box.Center.X, box.Center.Y + (box.Height * 0.08));
        var radius = box.Width * 0.30;

        context.DrawEllipse(null, pen, mid, radius, radius);

        // The meridian, which says *globe* rather than *circle*.
        context.DrawEllipse(
            null, new Pen(pen.Brush, pen.Thickness * 0.7), mid, radius * 0.42, radius);

        var arc = new StreamGeometry();

        using (var draw = arc.Open())
        {
            draw.BeginFigure(
                new Point(box.X + (box.Width * 0.14), box.Y + (box.Height * 0.34)), false);
            draw.CubicBezierTo(
                new Point(box.X + (box.Width * 0.34), box.Y - (box.Height * 0.02)),
                new Point(box.X + (box.Width * 0.74), box.Y - (box.Height * 0.02)),
                new Point(box.X + (box.Width * 0.92), box.Y + (box.Height * 0.30)));
            draw.EndFigure(false);
        }

        context.DrawGeometry(null, new Pen(ink, pen.Thickness * 0.9), arc);
    }

    /// <summary>Three waves of different lengths - a first on each band.</summary>
    /// <remarks>
    /// **THE LENGTHS CARRY THE IDEA AND NOT A MEASUREMENT.** Three waves at three
    /// wavelengths say *bands*; nothing here is to scale and nothing reads a frequency off
    /// it.
    /// </remarks>
    private static void Waves(DrawingContext context, Rect box, Pen pen)
    {
        var w = box.Width;
        var h = box.Height;
        var thin = new Pen(pen.Brush, pen.Thickness * 0.8, lineCap: PenLineCap.Round);

        var lengths = new[] { 0.34, 0.22, 0.14 };

        for (var i = 0; i < lengths.Length; i++)
        {
            var y = box.Y + (h * (0.28 + (i * 0.22)));
            var step = w * lengths[i];
            var wave = new StreamGeometry();

            using (var draw = wave.Open())
            {
                draw.BeginFigure(new Point(box.X + (w * 0.12), y), false);

                var up = true;

                for (var x = box.X + (w * 0.12); x < box.X + (w * 0.88); x += step)
                {
                    draw.QuadraticBezierTo(
                        new Point(x + (step / 2), y + (up ? -h * 0.10 : h * 0.10)),
                        new Point(Math.Min(x + step, box.X + (w * 0.88)), y));

                    up = !up;
                }

                draw.EndFigure(false);
            }

            context.DrawGeometry(null, thin, wave);
        }
    }

    /// <summary>
    /// `·-`, two tones and `PSK` - the three ways Hamlet's five modes sound.
    /// </summary>
    private static void Modes(DrawingContext context, Rect box, IBrush ink, Pen pen)
    {
        var w = box.Width;
        var h = box.Height;

        // The dit and the dah, top left.
        context.FillRectangle(
            ink, new Rect(box.X + (w * 0.10), box.Y + (h * 0.18), w * 0.10, h * 0.10));
        context.FillRectangle(
            ink, new Rect(box.X + (w * 0.26), box.Y + (h * 0.18), w * 0.30, h * 0.10));

        // Two tones, as bars of different height, top right.
        context.FillRectangle(
            ink, new Rect(box.X + (w * 0.66), box.Y + (h * 0.14), w * 0.10, h * 0.34));
        context.FillRectangle(
            ink, new Rect(box.X + (w * 0.80), box.Y + (h * 0.26), w * 0.10, h * 0.22));

        // `PSK`, as three strokes rather than as text, so nothing here needs a font.
        var y = box.Y + (h * 0.66);
        var tall = h * 0.22;
        var thin = new Pen(pen.Brush, pen.Thickness * 0.8);

        context.DrawLine(
            thin, new Point(box.X + (w * 0.14), y), new Point(box.X + (w * 0.14), y + tall));
        context.DrawLine(
            thin,
            new Point(box.X + (w * 0.14), y),
            new Point(box.X + (w * 0.26), y + (tall * 0.4)));
        context.DrawLine(
            thin,
            new Point(box.X + (w * 0.26), y + (tall * 0.4)),
            new Point(box.X + (w * 0.14), y + (tall * 0.55)));

        context.DrawLine(
            thin,
            new Point(box.X + (w * 0.48), y),
            new Point(box.X + (w * 0.34), y + (tall * 0.45)));
        context.DrawLine(
            thin,
            new Point(box.X + (w * 0.34), y + (tall * 0.45)),
            new Point(box.X + (w * 0.48), y + tall));

        context.DrawLine(
            thin, new Point(box.X + (w * 0.60), y), new Point(box.X + (w * 0.60), y + tall));
        context.DrawLine(
            thin,
            new Point(box.X + (w * 0.74), y),
            new Point(box.X + (w * 0.60), y + (tall * 0.5)));
        context.DrawLine(
            thin,
            new Point(box.X + (w * 0.60), y + (tall * 0.5)),
            new Point(box.X + (w * 0.76), y + tall));
    }
}
