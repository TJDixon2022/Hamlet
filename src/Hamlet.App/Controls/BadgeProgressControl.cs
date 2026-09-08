using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// A small bar showing how far the count is between one belt rank and the next.
/// </summary>
/// <remarks>
/// <para>**IT REPLACED A LABEL** (work instruction 281 task 4). Beside the count sat
/// the words *6 to 10*, which is a sentence doing a bar's work. The words are on the
/// ring's hover; this is the part that reads without being asked.</para>
/// <para>**DRAWN IN A NEUTRAL INK AND NEVER IN THE BELT'S COLOUR** (§0.6). The
/// grayscale rule for this whole widget is that the count and the progress survive
/// and only the rank is lost, so the one thing carrying the rank is the ring. A bar
/// tinted to match would have made the hue load-bearing twice over.</para>
/// <para>**IT IS A MEASUREMENT AND SO IT IS DRAWN AS ONE** (§0.0). The filled part
/// is the real fraction of the way between two thresholds; it does not round up to
/// look encouraging, and at nought it draws an empty track rather than a sliver.</para>
/// </remarks>
public sealed class BadgeProgressControl : Control
{
    /// <summary>How far along, from 0 to 1.</summary>
    public static readonly StyledProperty<double> FractionProperty =
        AvaloniaProperty.Register<BadgeProgressControl, double>(nameof(Fraction));

    /// <summary>The ink for the filled part. The track is drawn paler.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<BadgeProgressControl, IBrush?>(nameof(Foreground));

    private const double BarWidth = 46;
    private const double BarHeight = 4;

    private static readonly IBrush Filled = new SolidColorBrush(Color.Parse("#6E6E66"));
    private static readonly IBrush Track = new SolidColorBrush(Color.Parse("#D3CDBE"));

    static BadgeProgressControl()
    {
        AffectsMeasure<BadgeProgressControl>(FractionProperty);
        AffectsRender<BadgeProgressControl>(FractionProperty, ForegroundProperty);
    }

    /// <summary>How far along, from 0 to 1.</summary>
    public double Fraction
    {
        get => GetValue(FractionProperty);
        set => SetValue(FractionProperty, value);
    }

    /// <summary>The ink for the filled part.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(BarWidth, BarHeight);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var full = new Rect(0, 0, BarWidth, BarHeight);

        context.DrawRectangle(Track, null, full, BarHeight / 2, BarHeight / 2);

        var along = Math.Clamp(Fraction, 0.0, 1.0) * BarWidth;

        if (along <= 0.0)
        {
            return;
        }

        context.DrawRectangle(
            Foreground ?? Filled, null,
            new Rect(0, 0, along, BarHeight), BarHeight / 2, BarHeight / 2);
    }
}
