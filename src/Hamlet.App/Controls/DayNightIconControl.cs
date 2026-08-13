using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>Which face a band's day/night icon shows.</summary>
public enum DayNightIcon
{
    /// <summary>The sun's position is unknown, so neither is claimed.</summary>
    Neutral,

    /// <summary>A daytime band.</summary>
    Sun,

    /// <summary>A night band.</summary>
    Moon,

    /// <summary>An all-rounder, useful either side of sunset.</summary>
    Both,
}

/// <summary>
/// The little sun, moon or both in the corner of a band card.
/// </summary>
/// <remarks>
/// <para>Carries the same fact as the card's hue and its dimming, in a third
/// form (HM-DEC-032, HM-DEC-033). A colored bar alone would tell somebody with
/// a color vision deficiency nothing; a shape tells everybody.</para>
/// <para>Drawn rather than set in a glyph font, so it renders identically
/// wherever Hamlet runs and needs no font shipped with it.</para>
/// </remarks>
public sealed class DayNightIconControl : Control
{
    private const double Size = 13;

    /// <summary>Which face to draw.</summary>
    public static readonly StyledProperty<DayNightIcon> IconProperty =
        AvaloniaProperty.Register<DayNightIconControl, DayNightIcon>(nameof(Icon));

    /// <summary>The color to draw it in.</summary>
    public static readonly StyledProperty<IBrush?> TintProperty =
        AvaloniaProperty.Register<DayNightIconControl, IBrush?>(nameof(Tint));

    static DayNightIconControl()
    {
        AffectsRender<DayNightIconControl>(IconProperty, TintProperty);
    }

    /// <summary>Which face to draw.</summary>
    public DayNightIcon Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The color to draw it in.</summary>
    public IBrush? Tint
    {
        get => GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(Icon == DayNightIcon.Both ? Size + 7 : Size, Size);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var brush = Tint ?? Brushes.Gray;
        var center = new Point(Size / 2, Bounds.Height / 2);

        switch (Icon)
        {
            case DayNightIcon.Sun:
                DrawSun(context, brush, center);
                break;

            case DayNightIcon.Moon:
                DrawMoon(context, brush, center);
                break;

            case DayNightIcon.Both:
                DrawSun(context, brush, center);
                DrawMoon(context, brush, new Point(center.X + 7, center.Y));
                break;

            default:
                // Nothing is known about the sun, so the icon claims nothing:
                // a hollow ring rather than a sun or a moon.
                context.DrawEllipse(null, new Pen(brush, 1.1), center, 3.6, 3.6);
                break;
        }
    }

    private static void DrawSun(DrawingContext context, IBrush brush, Point center)
    {
        context.DrawEllipse(brush, null, center, 3.1, 3.1);

        var pen = new Pen(brush, 1.1, lineCap: PenLineCap.Round);
        for (var i = 0; i < 8; i++)
        {
            var angle = i * Math.PI / 4;
            var dx = Math.Cos(angle);
            var dy = Math.Sin(angle);

            context.DrawLine(
                pen,
                new Point(center.X + (dx * 4.7), center.Y + (dy * 4.7)),
                new Point(center.X + (dx * 6.3), center.Y + (dy * 6.3)));
        }
    }

    /// <summary>A crescent: a filled disc with a second disc taken out of it.</summary>
    private static void DrawMoon(DrawingContext context, IBrush brush, Point center)
    {
        var moon = new GeometryGroup { FillRule = FillRule.EvenOdd };
        moon.Children.Add(new EllipseGeometry(
            new Rect(center.X - 5.2, center.Y - 5.2, 10.4, 10.4)));
        moon.Children.Add(new EllipseGeometry(
            new Rect(center.X - 7.6, center.Y - 6.4, 10.4, 10.4)));

        context.DrawGeometry(brush, null, moon);
    }
}
