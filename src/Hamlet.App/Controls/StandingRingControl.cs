using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// **The standing ring**: a thick arc from twelve o'clock, clockwise, the fraction of the way from where
/// his rank began to where the next begins (work instruction 506, `assets/achievements-look/opening-page.html`).
/// </summary>
/// <remarks>
/// <para>**DRAWN IN CODE, NO PACKAGE** (the instruction). A track circle and an arc over it, both stroked
/// with round caps; the arc never rounds up to look encouraging, and at nought it draws the track alone
/// rather than a dot (§0.0, as <see cref="BadgeProgressControl"/> draws its bar).</para>
/// <para>**THE FRACTION IS NEVER THE ONLY CARRIER** (§0.6): the rank, the points and the gap in words are
/// drawn inside and under it.</para>
/// </remarks>
public sealed class StandingRingControl : Control
{
    /// <summary>How far round, from 0 to 1.</summary>
    public static readonly StyledProperty<double> FractionProperty =
        AvaloniaProperty.Register<StandingRingControl, double>(nameof(Fraction));

    /// <summary>The arc's ink.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<StandingRingControl, IBrush?>(nameof(Foreground));

    /// <summary>The track's ink.</summary>
    public static readonly StyledProperty<IBrush?> TrackProperty =
        AvaloniaProperty.Register<StandingRingControl, IBrush?>(nameof(Track));

    /// <summary>The stroke's width.</summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<StandingRingControl, double>(nameof(Thickness), 16);

    static StandingRingControl()
    {
        AffectsRender<StandingRingControl>(FractionProperty, ForegroundProperty, TrackProperty, ThicknessProperty);
    }

    /// <summary>How far round, from 0 to 1.</summary>
    public double Fraction
    {
        get => GetValue(FractionProperty);
        set => SetValue(FractionProperty, value);
    }

    /// <summary>The arc's ink.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>The track's ink.</summary>
    public IBrush? Track
    {
        get => GetValue(TrackProperty);
        set => SetValue(TrackProperty, value);
    }

    /// <summary>The stroke's width.</summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var side = Math.Min(Bounds.Width, Bounds.Height);
        var radius = (side - Thickness) / 2;

        if (radius <= 0)
        {
            return;
        }

        var centre = new Point(Bounds.Width / 2, Bounds.Height / 2);

        if (Track is { } track)
        {
            context.DrawEllipse(null, new Pen(track, Thickness), centre, radius, radius);
        }

        var fraction = Math.Clamp(Fraction, 0, 1);

        if (fraction <= 0 || Foreground is not { } ink)
        {
            return;
        }

        var pen = new Pen(ink, Thickness, lineCap: PenLineCap.Round);

        if (fraction >= 1)
        {
            context.DrawEllipse(null, pen, centre, radius, radius);
            return;
        }

        var end = (-Math.PI / 2) + (fraction * 2 * Math.PI);
        var geometry = new StreamGeometry();

        using (var draw = geometry.Open())
        {
            draw.BeginFigure(new Point(centre.X, centre.Y - radius), false);
            draw.ArcTo(
                new Point(centre.X + (radius * Math.Cos(end)), centre.Y + (radius * Math.Sin(end))),
                new Size(radius, radius),
                0,
                fraction > 0.5,
                SweepDirection.Clockwise);
            draw.EndFigure(false);
        }

        context.DrawGeometry(null, pen, geometry);
    }
}
