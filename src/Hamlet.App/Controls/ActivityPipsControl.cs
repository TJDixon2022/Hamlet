using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// The band button's activity indicator: a short row of pips, or a dashed
/// placeholder when there is no data.
/// </summary>
/// <remarks>
/// <para>Kept deliberately quiet (HM-DEC-031). The band buttons must still
/// read as buttons, and "best bet now" stays the single editorial call on
/// top; this is a second, softer signal underneath it. Small, low-contrast,
/// and never competing with the band name.</para>
/// <para>NO DATA DOES NOT LOOK LIKE ZERO. A band nobody is watching draws
/// hollow, dashed pips; a band being watched in silence draws empty solid
/// outlines. They are different claims and rendering them identically would
/// be the visual form of the lie the text is careful to avoid.</para>
/// </remarks>
public sealed class ActivityPipsControl : Control
{
    private const double PipWidth = 5;
    private const double PipHeight = 5;
    private const double Gap = 2.5;

    private static readonly IBrush FilledBrush = PanelPalette.Amber.TitleBrush;
    private static readonly IBrush EmptyBrush = new SolidColorBrush(Color.Parse("#E4E0D8"));
    private static readonly Pen UnknownPen =
        new(new SolidColorBrush(Color.Parse("#C9C4BA")), 1)
        {
            DashStyle = new DashStyle(new double[] { 1, 1 }, 0),
        };

    /// <summary>How many pips are filled, 0 upwards.</summary>
    public static readonly StyledProperty<int> FilledProperty =
        AvaloniaProperty.Register<ActivityPipsControl, int>(nameof(Filled));

    /// <summary>How many pips the indicator has in total.</summary>
    public static readonly StyledProperty<int> TotalProperty =
        AvaloniaProperty.Register<ActivityPipsControl, int>(nameof(Total), 4);

    /// <summary>True when nothing is known, so the pips draw as unknown.</summary>
    public static readonly StyledProperty<bool> IsUnknownProperty =
        AvaloniaProperty.Register<ActivityPipsControl, bool>(nameof(IsUnknown));

    static ActivityPipsControl()
    {
        AffectsRender<ActivityPipsControl>(
            FilledProperty, TotalProperty, IsUnknownProperty);
        AffectsMeasure<ActivityPipsControl>(TotalProperty);
    }

    /// <summary>How many pips are filled.</summary>
    public int Filled
    {
        get => GetValue(FilledProperty);
        set => SetValue(FilledProperty, value);
    }

    /// <summary>How many pips in total.</summary>
    public int Total
    {
        get => GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    /// <summary>True when nothing is known about this band.</summary>
    public bool IsUnknown
    {
        get => GetValue(IsUnknownProperty);
        set => SetValue(IsUnknownProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var total = Math.Max(1, Total);
        return new Size((total * PipWidth) + ((total - 1) * Gap), PipHeight);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var total = Math.Max(1, Total);
        var filled = Math.Clamp(Filled, 0, total);
        var y = (Bounds.Height - PipHeight) / 2;

        for (var i = 0; i < total; i++)
        {
            var rect = new Rect(i * (PipWidth + Gap), y, PipWidth, PipHeight);

            if (IsUnknown)
            {
                // Hollow and dashed: "I cannot see this band", which is a
                // different statement from "this band is empty".
                context.DrawRectangle(null, UnknownPen, rect, 1, 1);
                continue;
            }

            context.FillRectangle(i < filled ? FilledBrush : EmptyBrush, rect, 1);
        }
    }
}
