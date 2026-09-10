using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// **How far along a target he is: a filled arc where there is something to
/// measure, and a dashed outline where there is not.**
/// </summary>
/// <remarks>
/// <para>**THE DASHED STATE IS THE POINT OF THIS CONTROL** (work instruction 300
/// task 4). A band he has never worked and a continent he has never reached have
/// **no linear measure at all**: there is no half-worked band. Drawing 0% there, or
/// picking some denominator that makes a number appear, is a figure with nothing
/// behind it, which is §0.0 on the screen rather than in a decode. **So the ring
/// opens up, loses its number, and carries the target instead.**</para>
/// <para>**AND A FILLED ARC IS ONLY EVER A QUANTITY.** Miles covered, decibels below
/// the noise, squares counted: each is arithmetic on something he could count
/// himself. **It is never a likelihood**, because the next thousand miles is far
/// harder than the last, and the card says what the number is of, on its face,
/// beside the ring.</para>
/// <para>**THE TWO STATES DIFFER BY SHAPE** (§0.6): solid against dashed, a number
/// against a word. A greyscale printer keeps both.</para>
/// </remarks>
public sealed class AchievementRingControl : Control
{
    /// <summary>How far along, 0 to 1. Ignored where the ring is open.</summary>
    public static readonly StyledProperty<double> FractionProperty =
        AvaloniaProperty.Register<AchievementRingControl, double>(nameof(Fraction));

    /// <summary>True where there is no linear measure, so the ring is dashed.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<AchievementRingControl, bool>(nameof(IsOpen));

    /// <summary>
    /// What is written inside the ring: a percentage where one exists, the target
    /// where none does, and `done` where it is finished.
    /// </summary>
    public static readonly StyledProperty<string> WordProperty =
        AvaloniaProperty.Register<AchievementRingControl, string>(nameof(Word), "");

    /// <summary>True where he already holds it, which fills the ring.</summary>
    public static readonly StyledProperty<bool> IsEarnedProperty =
        AvaloniaProperty.Register<AchievementRingControl, bool>(nameof(IsEarned));

    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly IBrush Track = new SolidColorBrush(Color.Parse("#DCD8CC"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#2F2E2A"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#6E6E66"));

    static AchievementRingControl()
        => AffectsRender<AchievementRingControl>(
            FractionProperty, IsOpenProperty, WordProperty, IsEarnedProperty);

    /// <summary>How far along, 0 to 1.</summary>
    public double Fraction
    {
        get => GetValue(FractionProperty);
        set => SetValue(FractionProperty, value);
    }

    /// <summary>True where there is nothing linear to measure.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>What is written inside the ring.</summary>
    public string Word
    {
        get => GetValue(WordProperty);
        set => SetValue(WordProperty, value);
    }

    /// <summary>True where he already holds it.</summary>
    public bool IsEarned
    {
        get => GetValue(IsEarnedProperty);
        set => SetValue(IsEarnedProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => new(46, 46);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var side = Math.Min(Bounds.Width, Bounds.Height);
        var middle = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var radius = side / 2 - 3;

        if (IsOpen)
        {
            // **DASHED, AND NO NUMBER ANYWHERE ON IT.** There is nothing to be a
            // percentage of, so the ring says *this is a target* and the words
            // inside say which one.
            context.DrawEllipse(
                null,
                new Pen(Muted, 2, new DashStyle(new double[] { 2, 2.4 }, 0)),
                middle,
                radius,
                radius);

            Inside(context, Word, middle, side, Muted, 10);

            return;
        }

        context.DrawEllipse(null, new Pen(Track, 4), middle, radius, radius);

        var fraction = Math.Clamp(IsEarned ? 1 : Fraction, 0, 1);

        if (fraction > 0)
        {
            // **THE ARC STARTS AT THE TOP AND GOES ROUND**, which is the direction a
            // dial reads, and it is built as a figure rather than as a rectangle
            // masked to a shape so that nothing about it depends on a clip.
            var figure = new PathGeometry();
            var start = middle + Turned(-Math.PI / 2, radius);

            using (var build = figure.Open())
            {
                build.BeginFigure(start, false);

                build.ArcTo(
                    middle + Turned(fraction * Math.PI * 2 - Math.PI / 2, radius),
                    new Size(radius, radius),
                    0,
                    fraction > 0.5,
                    SweepDirection.Clockwise);

                build.EndFigure(false);
            }

            context.DrawGeometry(
                null, new Pen(Green, 4, lineCap: PenLineCap.Round), figure);
        }

        Inside(context, Word, middle, side, Ink, IsEarned ? 10 : 12);
    }

    private static Vector Turned(double radians, double radius)
        => new(Math.Cos(radians) * radius, Math.Sin(radians) * radius);

    /// <summary>Put a short word in the middle of the ring.</summary>
    private static void Inside(
        DrawingContext context, string? text, Point middle, double side,
        IBrush brush, double size)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var laid = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            size,
            brush)
        {
            TextAlignment = TextAlignment.Center,
            MaxTextWidth = side - 10,
        };

        context.DrawText(
            laid, new Point(middle.X - laid.Width / 2, middle.Y - laid.Height / 2));
    }
}

/// <summary>
/// **The small mark that stops a column of targets reading as one thing.**
/// </summary>
/// <remarks>
/// <para>**DRAWN FROM PRIMITIVES AND NEVER FROM A FONT** (work instruction 300 task
/// 3). A glyph taken from a character set renders as a picture on one machine and as
/// an empty box on another, and a carrier that is sometimes absent is not a carrier.
/// </para>
/// <para>**IT IS A SECOND CARRIER AND NEVER THE ONLY ONE** (§0.6). Every card still
/// says in words what it is; the mark makes the list scannable rather than
/// legible.</para>
/// </remarks>
public sealed class AchievementGlyphControl : Control
{
    /// <summary>Which mark to draw.</summary>
    public static readonly StyledProperty<AchievementGlyph> GlyphProperty =
        AvaloniaProperty.Register<AchievementGlyphControl, AchievementGlyph>(
            nameof(Glyph));

    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#5F5C53"));

    static AchievementGlyphControl()
        => AffectsRender<AchievementGlyphControl>(GlyphProperty);

    /// <summary>Which mark to draw.</summary>
    public AchievementGlyph Glyph
    {
        get => GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => new(16, 16);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var side = Math.Min(Bounds.Width, Bounds.Height);

        if (side <= 0)
        {
            return;
        }

        var pen = new Pen(Ink, 1.4, lineCap: PenLineCap.Round);
        var o = new Point((Bounds.Width - side) / 2, (Bounds.Height - side) / 2);

        Point At(double x, double y) => new(o.X + x * side, o.Y + y * side);

        switch (Glyph)
        {
            case AchievementGlyph.Distance:
                // A hop: a dot, an arc over it, a dot.
                context.DrawEllipse(Ink, null, At(0.12, 0.78), 1.6, 1.6);
                context.DrawEllipse(Ink, null, At(0.88, 0.78), 1.6, 1.6);
                Arc(context, pen, At(0.12, 0.78), At(0.88, 0.78), side * 0.46);
                break;

            case AchievementGlyph.Faint:
                // Three waves, each shorter than the last.
                context.DrawLine(pen, At(0.2, 0.14), At(0.2, 0.86));
                context.DrawLine(pen, At(0.5, 0.32), At(0.5, 0.68));
                context.DrawLine(pen, At(0.8, 0.45), At(0.8, 0.55));
                break;

            case AchievementGlyph.Band:
                // Three bars, tallest on the left, the way a band chart is drawn.
                context.DrawLine(pen, At(0.18, 0.86), At(0.18, 0.2));
                context.DrawLine(pen, At(0.5, 0.86), At(0.5, 0.46));
                context.DrawLine(pen, At(0.82, 0.86), At(0.82, 0.62));
                break;

            case AchievementGlyph.Night:
                // A crescent: a circle with a bite out of it.
                context.DrawGeometry(
                    null,
                    pen,
                    new CombinedGeometry(
                        GeometryCombineMode.Exclude,
                        new EllipseGeometry(
                            new Rect(At(0.1, 0.1), At(0.9, 0.9))),
                        new EllipseGeometry(
                            new Rect(At(0.36, 0.02), At(1.12, 0.78)))));
                break;

            case AchievementGlyph.GreyLine:
                // A sun on the horizon: half a circle above a line.
                context.DrawLine(pen, At(0.06, 0.68), At(0.94, 0.68));
                Arc(context, pen, At(0.24, 0.68), At(0.76, 0.68), side * 0.3);
                break;

            case AchievementGlyph.Continent:
                // A globe: a circle with a meridian across it.
                context.DrawEllipse(null, pen, At(0.5, 0.5), side * 0.4, side * 0.4);
                context.DrawEllipse(null, pen, At(0.5, 0.5), side * 0.16, side * 0.4);
                context.DrawLine(pen, At(0.1, 0.5), At(0.9, 0.5));
                break;

            case AchievementGlyph.Grid:
                // Four boxes.
                context.DrawRectangle(null, pen, new Rect(At(0.12, 0.12), At(0.46, 0.46)));
                context.DrawRectangle(null, pen, new Rect(At(0.54, 0.12), At(0.88, 0.46)));
                context.DrawRectangle(null, pen, new Rect(At(0.12, 0.54), At(0.46, 0.88)));
                context.DrawRectangle(null, pen, new Rect(At(0.54, 0.54), At(0.88, 0.88)));
                break;

            case AchievementGlyph.Day:
                // A stack of contacts: three rules, one shorter.
                context.DrawLine(pen, At(0.12, 0.24), At(0.88, 0.24));
                context.DrawLine(pen, At(0.12, 0.5), At(0.88, 0.5));
                context.DrawLine(pen, At(0.12, 0.76), At(0.6, 0.76));
                break;

            default:
                break;
        }
    }

    /// <summary>An arc from one point to another, bulging upward.</summary>
    private static void Arc(
        DrawingContext context, IPen pen, Point from, Point to, double radius)
    {
        var figure = new PathGeometry();

        using (var build = figure.Open())
        {
            build.BeginFigure(from, false);
            build.ArcTo(
                to, new Size(radius, radius), 0, false, SweepDirection.Clockwise);
            build.EndFigure(false);
        }

        context.DrawGeometry(null, pen, figure);
    }
}
