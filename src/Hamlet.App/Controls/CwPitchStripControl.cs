using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>What one mark on the strip is.</summary>
public enum CwPitchMarkKind
{
    /// <summary>The band the tracker searches, shaded.</summary>
    Searched,

    /// <summary>A bin the survey admitted as keying, a short tick.</summary>
    Admitted,

    /// <summary>The pitch the decoder is mixing at, a solid line.</summary>
    Tracker,

    /// <summary>The keying meter's best pitch, a dashed line.</summary>
    Meter,
}

/// <summary>One mark as drawn.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="X">Where it starts, in pixels from the left.</param>
/// <param name="X2">Where it ends; the same as <paramref name="X"/> for a line.</param>
/// <param name="Label">The words it carries.</param>
public sealed record CwPitchMark(CwPitchMarkKind Kind, double X, double X2, string Label);

/// <summary>
/// **THE PITCH STRIP: THE WHOLE RANGE, AND WHERE THE DETECTOR IS LOOKING IN IT** (work
/// instruction 474 task 2, HM-DEC-184).
/// </summary>
/// <remarks>
/// <para>**IT SHOWS AND CHANGES NOTHING.** Every figure is the view model's, read from the
/// tracker, the survey and the keying meter; nothing here is handed back.</para>
/// <para>**EVERY MARK CARRIES WORDS AND A SHAPE, NEVER COLOR ALONE** (§0.6): the searched
/// band is shaded and labelled with its numbers, the tracker is a solid line labelled above
/// the bar, the meter a dashed line labelled below it, and the admitted bins are short
/// ticks named in the line under the strip. The hover says what each is.</para>
/// </remarks>
public sealed class CwPitchStripControl : Control
{
    /// <summary>What to draw.</summary>
    public static readonly StyledProperty<CwPitchStrip?> StripProperty =
        AvaloniaProperty.Register<CwPitchStripControl, CwPitchStrip?>(nameof(Strip));

    private const double Pad = 6;
    private const double LabelHeight = 14;
    private const double BarHeight = 16;
    private const double AxisHeight = 12;

    private static readonly IBrush Rail = new SolidColorBrush(Color.Parse("#E8ECEF"));
    private static readonly IBrush Searched = new SolidColorBrush(Color.Parse("#C9DDF0"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#44505A"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#7A8590"));
    private static readonly IBrush TrackerInk = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly IBrush MeterInk = new SolidColorBrush(Color.Parse("#B06A10"));
    private static readonly Pen EdgePen = new(new SolidColorBrush(Color.Parse("#9AA5AF")), 1);
    private static readonly Pen AdmittedPen = new(Ink, 2);
    private static readonly Pen TrackerPen = new(TrackerInk, 2);
    private static readonly Pen MeterPen = new(MeterInk, 2, new DashStyle(new double[] { 2, 2 }, 0));

    static CwPitchStripControl()
    {
        AffectsRender<CwPitchStripControl>(StripProperty);
        AffectsMeasure<CwPitchStripControl>(StripProperty);
    }

    /// <summary>What to draw.</summary>
    public CwPitchStrip? Strip
    {
        get => GetValue(StripProperty);
        set => SetValue(StripProperty, value);
    }

    /// <summary>Where a pitch lands across a strip this wide.</summary>
    /// <param name="hz">The pitch.</param>
    /// <param name="strip">The strip.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>Pixels from the left, clamped to the drawn range.</returns>
    public static double XOf(double hz, CwPitchStrip strip, double width)
    {
        ArgumentNullException.ThrowIfNull(strip);

        var span = strip.HighHz - strip.LowHz;

        if (span <= 0 || width <= 2 * Pad)
        {
            return Pad;
        }

        var share = Math.Clamp((hz - strip.LowHz) / span, 0, 1);

        return Pad + (share * (width - (2 * Pad)));
    }

    /// <summary>Every mark, as drawn, with its words.</summary>
    /// <param name="strip">The strip.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>The searched band, each admitted bin, the tracker and the meter where present.</returns>
    public static IReadOnlyList<CwPitchMark> Marks(CwPitchStrip strip, double width)
    {
        ArgumentNullException.ThrowIfNull(strip);

        var marks = new List<CwPitchMark>
        {
            new(CwPitchMarkKind.Searched,
                XOf(strip.SearchLowHz, strip, width),
                XOf(strip.SearchHighHz, strip, width),
                strip.SearchLabel),
        };

        foreach (var hz in strip.AdmittedHz)
        {
            var x = XOf(hz, strip, width);
            marks.Add(new CwPitchMark(
                CwPitchMarkKind.Admitted, x, x,
                string.Create(CultureInfo.InvariantCulture, $"survey admitted {hz:0} Hz")));
        }

        if (!double.IsNaN(strip.TrackerHz))
        {
            var x = XOf(strip.TrackerHz, strip, width);
            marks.Add(new CwPitchMark(CwPitchMarkKind.Tracker, x, x, strip.TrackerLabel));
        }

        if (!double.IsNaN(strip.MeterHz))
        {
            var x = XOf(strip.MeterHz, strip, width);
            marks.Add(new CwPitchMark(CwPitchMarkKind.Meter, x, x, strip.MeterLabel));
        }

        return marks;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,
            LabelHeight + BarHeight + AxisHeight + LabelHeight);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Strip is not { } strip || Bounds.Width <= 2 * Pad || strip.HighHz <= strip.LowHz)
        {
            return;
        }

        var width = Bounds.Width;
        var barTop = LabelHeight;
        var barBottom = barTop + BarHeight;

        context.FillRectangle(Rail, new Rect(Pad, barTop, width - (2 * Pad), BarHeight));

        foreach (var mark in Marks(strip, width))
        {
            switch (mark.Kind)
            {
                case CwPitchMarkKind.Searched:
                    context.FillRectangle(Searched, new Rect(mark.X, barTop, mark.X2 - mark.X, BarHeight));
                    context.DrawLine(EdgePen, new Point(mark.X, barTop), new Point(mark.X, barBottom));
                    context.DrawLine(EdgePen, new Point(mark.X2, barTop), new Point(mark.X2, barBottom));
                    Text(context, mark.Label, (mark.X + mark.X2) / 2, barTop + 2, Muted, 10);
                    break;

                case CwPitchMarkKind.Admitted:
                    context.DrawLine(AdmittedPen, new Point(mark.X, barBottom - 6), new Point(mark.X, barBottom));
                    break;

                case CwPitchMarkKind.Tracker:
                    context.DrawLine(TrackerPen, new Point(mark.X, barTop - 2), new Point(mark.X, barBottom));
                    Text(context, "▼ " + mark.Label, mark.X, 0, TrackerInk, 10);
                    break;

                case CwPitchMarkKind.Meter:
                    context.DrawLine(MeterPen, new Point(mark.X, barTop), new Point(mark.X, barBottom + 2));
                    Text(context, "▲ " + mark.Label, mark.X, barBottom + AxisHeight, MeterInk, 10);
                    break;
            }
        }

        // The axis: every hundred hertz, numbered every two hundred.
        for (var hz = Math.Ceiling(strip.LowHz / 100) * 100; hz <= strip.HighHz; hz += 100)
        {
            var x = XOf(hz, strip, width);
            context.DrawLine(EdgePen, new Point(x, barBottom), new Point(x, barBottom + 3));

            if ((int)hz % 200 == 0)
            {
                Text(context, hz.ToString("0", CultureInfo.InvariantCulture), x, barBottom + 1, Muted, 9);
            }
        }
    }

    private void Text(DrawingContext context, string words, double centreX, double top, IBrush brush, double size)
    {
        var text = new FormattedText(
            words, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, size, brush);

        var left = Math.Clamp(centreX - (text.Width / 2), 0, Math.Max(0, Bounds.Width - text.Width));

        context.DrawText(text, new Point(left, top));
    }
}
