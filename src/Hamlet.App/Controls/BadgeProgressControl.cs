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

    /// <summary>How long the bar is. The belt's is 46.</summary>
    /// <remarks>
    /// **THE CATEGORY BAND DRAWS THE SAME BAR LONGER AND THICKER** (work instruction 335 task
    /// 1). One control, so the rule that it never rounds up to look encouraging holds on both.
    /// </remarks>
    public static readonly StyledProperty<double> LengthProperty =
        AvaloniaProperty.Register<BadgeProgressControl, double>(nameof(Length), 46);

    /// <summary>How thick the bar is. The belt's is 4.</summary>
    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<BadgeProgressControl, double>(nameof(Thickness), 4);

    /// <summary>The ink for the unfilled track, or null for the belt's pale track.</summary>
    public static readonly StyledProperty<IBrush?> TrackProperty =
        AvaloniaProperty.Register<BadgeProgressControl, IBrush?>(nameof(Track));

    private static readonly IBrush Filled = new SolidColorBrush(Color.Parse("#6E6E66"));
    private static readonly IBrush PaleTrack = new SolidColorBrush(Color.Parse("#D3CDBE"));

    static BadgeProgressControl()
    {
        AffectsMeasure<BadgeProgressControl>(FractionProperty, LengthProperty, ThicknessProperty);
        AffectsRender<BadgeProgressControl>(
            FractionProperty, ForegroundProperty, LengthProperty, ThicknessProperty, TrackProperty);
    }

    /// <summary>How long the bar is.</summary>
    public double Length
    {
        get => GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>How thick the bar is.</summary>
    public double Thickness
    {
        get => GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    /// <summary>The ink for the unfilled track.</summary>
    public IBrush? Track
    {
        get => GetValue(TrackProperty);
        set => SetValue(TrackProperty, value);
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
        => new(Length, Thickness);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var full = new Rect(0, 0, Length, Thickness);
        var round = Thickness / 2;

        context.DrawRectangle(Track ?? PaleTrack, null, full, round, round);

        var along = Math.Clamp(Fraction, 0.0, 1.0) * Length;

        if (along <= 0.0)
        {
            return;
        }

        context.DrawRectangle(
            Foreground ?? Filled, null,
            new Rect(0, 0, along, Thickness), round, round);
    }
}
