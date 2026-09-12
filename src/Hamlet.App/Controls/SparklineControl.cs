using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// **Stations heard here over the last minute, as a line** (work instruction 332 task 2).
/// </summary>
/// <remarks>
/// <para>**THE NUMBER BESIDE IT IS THE CARRIER AND THE LINE IS SECOND** (§0.6). The count
/// reads `6 stations` in words; the line says whether they came all at once or kept coming,
/// which a single number cannot.</para>
/// <para>**EVERY BIN IS THE DOTS' OWN** - <see cref="ViewModels.GreenZone.Sparkline"/> bins
/// the same heard-at times the count is taken from, so the line and the count cannot come to
/// disagree about one minute of the band.</para>
/// <para>**VECTOR, NO IMAGE.** A polyline and a dot at the newest end.</para>
/// </remarks>
public sealed class SparklineControl : Control
{
    /// <summary>The bins, oldest first.</summary>
    public static readonly StyledProperty<IReadOnlyList<int>?> ValuesProperty =
        AvaloniaProperty.Register<SparklineControl, IReadOnlyList<int>?>(nameof(Values));

    private static readonly IBrush Line = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly IBrush Newest = new SolidColorBrush(Color.Parse("#C8842A"));

    static SparklineControl()
    {
        AffectsRender<SparklineControl>(ValuesProperty);
    }

    /// <summary>The bins, oldest first.</summary>
    public IReadOnlyList<int>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            double.IsInfinity(availableSize.Width) ? 120 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 28 : availableSize.Height);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var values = Values;

        if (values is null || values.Count < 2 || Bounds.Width <= 4 || Bounds.Height <= 4)
        {
            return;
        }

        var highest = Math.Max(1, values.Max());
        var step = (Bounds.Width - 4) / (values.Count - 1);

        Point At(int i)
            => new(
                2 + (i * step),
                Bounds.Height - 2 - ((double)values[i] / highest * (Bounds.Height - 4)));

        var line = new StreamGeometry();

        using (var draw = line.Open())
        {
            draw.BeginFigure(At(0), false);

            for (var i = 1; i < values.Count; i++)
            {
                draw.LineTo(At(i));
            }

            draw.EndFigure(false);
        }

        context.DrawGeometry(null, new Pen(Line, 1.5, lineCap: PenLineCap.Round), line);
        context.DrawEllipse(Newest, null, At(values.Count - 1), 2.5, 2.5);
    }
}
