using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.Controls;

/// <summary>What one thing the scope draws is.</summary>
public enum CwScopeLineKind
{
    /// <summary>The marks: a filled block along the bottom under every marked hop.</summary>
    Bars,

    /// <summary>The detector's pitch, as text in the corner.</summary>
    Tone,

    /// <summary>The tracker's pitch, as text beside it.</summary>
    Mixing,
}

/// <summary>One thing the scope draws, with its words.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="Label">The words it carries.</param>
public sealed record CwScopeLine(CwScopeLineKind Kind, string Label);

/// <summary>What one thing on the training graph's canvas is.</summary>
public enum CwScopeItemKind
{
    /// <summary>A bar the detector found, its true length.</summary>
    Bar,

    /// <summary>A character the decoder settled, over its own span.</summary>
    Letter,

    /// <summary>The small words that fill the empty graph: "listening".</summary>
    Listening,

    /// <summary>The detector's pitch line, as words in the corner.</summary>
    Tone,

    /// <summary>The tracker's pitch line, as words beside it.</summary>
    Mixing,
}

/// <summary>One thing on the canvas, where it is, and its words.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="X">Its left edge; for a letter, the left of the span it was made from.</param>
/// <param name="X2">Its right edge; the same as <paramref name="X"/> for words. A letter is centered between the two.</param>
/// <param name="Text">What it says, or its hover for a bar.</param>
public sealed record CwScopeItem(CwScopeItemKind Kind, double X, double X2, string Text);

/// <summary>One mark, as a bar along the bottom.</summary>
/// <param name="X">Where it starts, in pixels from the left.</param>
/// <param name="X2">Where it ends.</param>
/// <param name="Label">"mark 180 ms".</param>
public sealed record CwScopeBar(double X, double X2, string Label);

/// <summary>
/// **THE OSCILLOSCOPE: THE LEVEL TRACE, AND A BAR UNDER EVERY MARK** (work instruction 478
/// task 1, step 12 criterion 12.2 rewritten, R92).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"We should have replaced the temp controls with something that
/// looks like #2"* - the middle panel of the bars-not-waves picture: a keyed station's trace
/// with flat tops and flat bottoms, and the dits and dahs marked underneath. **That is all it
/// draws.** Units 476 and 477 drew a floor line, a threshold line and a shaded passband; since
/// 477 none of them decides anything (R91), so they are gone (§0.0). The detector still
/// computes them; the verdict row still carries them.</para>
/// <para>**THE TRACE IS ONE BIN**, the one the detector reads. Before any station is found
/// that is the middle of the passband, and after one stops it stays on the last station's
/// pitch, so the trace is never blank; the detector chooses it and this only draws it.</para>
/// <para>**EVERY HOP IS DRAWN; THE SCREEN IS REDRAWN TWENTY TIMES A SECOND.** A dit is its
/// true width.</para>
/// <para>**WORDS FOR EVERYTHING, NEVER COLOR ALONE** (§0.6): the trace and the bars are
/// named at the right-hand end, every bar carries its length where it fits, the pitch is
/// text, and the hover says what the trace is or what a bar is under the pointer.</para>
/// <para>**IT SHOWS AND CHANGES NOTHING.**</para>
/// <para>**SINCE WORK INSTRUCTION 480 IT IS THE TRAINING GRAPH**: it draws <see cref="Items"/>
/// and nothing else, over eight seconds, and the four-second helpers below stay for the frame's
/// own hop arithmetic.</para>
/// <para>**THE TRACE IS BACK** (work instruction 480, the letter over the bars, R94). R95 took it
/// off and drew bars alone, with each settled letter 26 point in a row of its own; on the owner's
/// tab at 14.0685 the detector marked nothing, so the panel was blank and the only thing on it
/// was <c>T T</c>. Now the level of the watched bin is drawn across the eight seconds whether or
/// not anything is marked, the bars sit under it, and the letter row is gone.</para>
/// </remarks>
public sealed class CwScopeControl : Control
{
    /// <summary>The margin at each side, in pixels.</summary>
    public const double Pad = 6;

    /// <summary>The height of the plot: the words at its top, then the trace.</summary>
    public const double PlotHeight = 110;

    /// <summary>Where the letters' band starts, from the top: under the words in the corner.</summary>
    public const double LetterTop = 14;

    /// <summary>How tall the letters' band is, inside the plot.</summary>
    public const double LetterHeight = 24;

    /// <summary>How big a letter is drawn: read at a glance, and small enough to sit over its own bars.</summary>
    public const double LetterSize = 18;

    /// <summary>Where the trace's band starts, from the top: under the letters.</summary>
    public const double TraceTop = LetterTop + LetterHeight;

    /// <summary>Where the row of bars starts, from the top: under the plot.</summary>
    public const double BarRowTop = PlotHeight + Gap;

    /// <summary>Where the training graph's bars start, from the top.</summary>
    public const double BarTop = BarRowTop;

    /// <summary>How tall a bar is on the training graph; its length is its time.</summary>
    public const double TrainingBarHeight = 14;

    /// <summary>What to draw.</summary>
    public static readonly StyledProperty<CwScopeFrame?> FrameProperty =
        AvaloniaProperty.Register<CwScopeControl, CwScopeFrame?>(nameof(Frame));

    private const double BarLabelHeight = 12;
    private const double Gap = 3;

    private static readonly IBrush Plot = new SolidColorBrush(Color.Parse("#F6F8F9"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#44505A"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#7A8590"));
    private static readonly IBrush MarkInk = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly Pen SpanPen = new(Muted, 1);
    private static readonly IBrush UnreadableInk = InstrumentPalette.UnreadableBrush;

    static CwScopeControl()
    {
        AffectsRender<CwScopeControl>(FrameProperty);
    }

    /// <summary>What to draw.</summary>
    public CwScopeFrame? Frame
    {
        get => GetValue(FrameProperty);
        set => SetValue(FrameProperty, value);
    }

    /// <summary>How many hops four seconds hold at this frame's hop.</summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The hops the width stands for.</returns>
    public static int HopsAcross(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return frame.HopMs > 0
            ? (int)Math.Ceiling(CwEnvelopeDetector.HistorySeconds * 1000 / frame.HopMs)
            : Math.Max(1, frame.Hops.Count);
    }

    /// <summary>Where the start of a hop lands; the newest hop ends at the right edge.</summary>
    /// <param name="index">The hop, oldest 0; <c>Hops.Count</c> is the right edge.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The width the four seconds are laid across, pads included.</param>
    /// <returns>Pixels from the left.</returns>
    public static double XOfHop(int index, CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var across = Math.Max(HopsAcross(frame), frame.Hops.Count);
        var slot = index + (across - frame.Hops.Count);

        return Pad + ((double)slot / across * Math.Max(0, width - (2 * Pad)));
    }

    /// <summary>Every mark in the frame, as a bar with its length.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The width the four seconds are laid across, pads included.</param>
    /// <returns>One bar per run of marked hops, oldest first; nothing under a gap.</returns>
    public static IReadOnlyList<CwScopeBar> Bars(CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var bars = new List<CwScopeBar>();
        var start = -1;

        for (var i = 0; i <= frame.Hops.Count; i++)
        {
            var mark = i < frame.Hops.Count && frame.Hops[i].Mark;

            if (mark && start < 0)
            {
                start = i;
            }
            else if (!mark && start >= 0)
            {
                bars.Add(new CwScopeBar(
                    XOfHop(start, frame, width),
                    XOfHop(i, frame, width),
                    string.Create(CultureInfo.InvariantCulture, $"mark {(i - start) * frame.HopMs:0} ms")));
                start = -1;
            }
        }

        return bars;
    }

    /// <summary>The kinds of thing the graph draws, with their words; no level trace since R97.</summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The bars, the detector's pitch and the tracker's.</returns>
    public static IReadOnlyList<CwScopeLine> Lines(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return new[]
        {
            new CwScopeLine(CwScopeLineKind.Bars, "marks"),
            new CwScopeLine(CwScopeLineKind.Tone, frame.ToneLine),
            new CwScopeLine(CwScopeLineKind.Mixing, frame.MixingLine),
        };
    }

    /// <summary>Everything on the canvas, as items: nothing is drawn that is not here.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>The items.</returns>
    /// <remarks>
    /// <para>**BLOCKS AND LETTERS, AND WHAT IS DRAWN STAYS UNTIL IT SCROLLS OFF** (work
    /// instructions 485 and 487, R97, R100). A filled block for every mark the detector calls, as
    /// long as the mark lasted, newest at the right, with empty space between; no level trace at
    /// all, because the owner read a noise line as a signal. Nothing new is drawn while nobody is
    /// keying, and nothing drawn blinks out when the detector lets go: it scrolls off the left with
    /// time. "listening" only before anything has been heard at all.</para>
    /// <para>**THE LETTER OVER THE BLOCKS THAT MADE IT, AND IT STAYS** (R94, R100, §0.0, work
    /// instruction numbered 508 run as unit 509, HM-DEC-213). Each character the terminal printed
    /// spans the time it was made from - its end on the decoder's own audio clock and its span in
    /// the decoder's hops - and scrolls left with its blocks until it slides off. It is never moved
    /// to sit better, and never taken away by a redraw. A word gap draws nothing; a prosign is its
    /// bracketed name; an unreadable character is the placeholder glyph.</para>
    /// </remarks>
    public static IReadOnlyList<CwScopeItem> Items(CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var training = frame.Training;
        var items = new List<CwScopeItem>
        {
            new(CwScopeItemKind.Tone, Pad, Pad, frame.ToneLine),
            new(CwScopeItemKind.Mixing, Pad, Pad, frame.MixingLine),
        };

        if (training.Listening)
        {
            items.Add(new CwScopeItem(CwScopeItemKind.Listening, width / 2, width / 2, ListeningWords));
            return items;
        }

        foreach (var bar in training.Bars)
        {
            items.Add(new CwScopeItem(
                CwScopeItemKind.Bar,
                XOfTime(bar.StartUtc, training.NowUtc, width),
                XOfTime(bar.EndUtc, training.NowUtc, width),
                BarTip(bar)));
        }

        foreach (var letter in DrawnLetters(frame))
        {
            items.Add(new CwScopeItem(
                CwScopeItemKind.Letter,
                XOfTime(letter.StartUtc, training.NowUtc, width),
                XOfTime(letter.EndUtc, training.NowUtc, width),
                Shown(letter)));
        }

        return items;
    }

    /// <summary>The settled characters the scope draws, oldest first: every letter the terminal printed, still on the scroll.</summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The letters, in the order their items appear.</returns>
    /// <remarks>
    /// **A LETTER ON THE SCROLL STAYS ON THE SCROLL** (work instruction numbered 508, run as unit
    /// 509, R100, HM-DEC-213). The letters are the graph's own list, appended from the
    /// `CharacterSettled` event the terminal prints from and trimmed only as time carries them off
    /// the left. Before, each frame kept only the letters with a block beneath them on that frame,
    /// and the detector rebuilds its last four seconds of blocks every tick, so a printed letter
    /// blinked out, drew late, or never drew at all when its blocks moved.
    /// </remarks>
    public static IReadOnlyList<CwGraphLetter> DrawnLetters(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var training = frame.Training;

        return training.Listening ? Array.Empty<CwGraphLetter>() : training.Letters;
    }

    /// <summary>What a letter's hover says, first of all.</summary>
    public const string LetterTipWords = "the decoder made this letter from the bars beneath it";

    /// <summary>What a settled character is drawn as: the placeholder glyph where it was unreadable.</summary>
    private static string Shown(CwGraphLetter letter)
        => letter.Confidence == CwConfidence.Unreadable ? MorseAlphabet.Unreadable : letter.Text;

    /// <summary>What a letter's hover says: where it came from, its class and its confidence.</summary>
    private static string LetterTip(CwGraphLetter letter)
    {
        var word = letter.Confidence switch
        {
            CwConfidence.High => "sure",
            CwConfidence.Low => "unsure",
            _ => "heard but unreadable",
        };

        var confidence = double.IsFinite(letter.Probability)
            ? string.Create(CultureInfo.InvariantCulture, $", {letter.Probability * 100:0}% likely right")
            : "";

        return letter.HasSpan
            ? Shown(letter) + ": " + LetterTipWords + "; " + word + confidence
            : Shown(letter) + ": drawn where the decoder finished it, because it gave no span, so the bars it "
              + "came from are not known; " + word + confidence;
    }

    /// <summary>The small word on an empty graph.</summary>
    public const string ListeningWords = "listening";

    /// <summary>Where a moment lands on the eight-second axis; now is the right edge.</summary>
    /// <param name="atUtc">The moment.</param>
    /// <param name="nowUtc">Now.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>Pixels from the left, held inside the pads.</returns>
    public static double XOfTime(DateTime atUtc, DateTime nowUtc, double width)
    {
        var fraction = 1 - ((nowUtc - atUtc).TotalSeconds / CwTrainingGraph.WindowSeconds);

        return Pad + (Math.Clamp(fraction, 0, 1) * Math.Max(0, width - (2 * Pad)));
    }

    /// <summary>
    /// What a block's hover says: dit or dah, its length, and how far inside the shape of a keyed tone
    /// the mark under it scored, or that none was handed out there (work instruction 502, R110).
    /// </summary>
    /// <param name="bar">The block.</param>
    /// <returns><c>dah, 150 ms, shape 0.87 of 1</c>, or <c>dit, 20 ms, not handed out as a mark</c>.</returns>
    public static string BarTip(CwGraphBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{(bar.Dah ? "dah" : "dit")}, {bar.LengthMs:0} ms, {(double.IsNaN(bar.ShapeScore) ? "not handed out as a mark" : $"shape {bar.ShapeScore:0.00} of 1")}");
    }

    /// <summary>What the hover says at a point: a block's words over a block, a letter's over a letter.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <param name="point">The pointer, in the control's pixels.</param>
    /// <returns>The words.</returns>
    public static string TipAt(CwScopeFrame frame, double width, Point point)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var items = Items(frame, width);

        if (point.Y >= LetterTop && point.Y < LetterTop + LetterHeight)
        {
            var letters = items.Where(i => i.Kind == CwScopeItemKind.Letter).ToList();

            for (var i = 0; i < letters.Count; i++)
            {
                var item = letters[i];
                var center = (item.X + item.X2) / 2;

                if ((point.X >= item.X && point.X <= item.X2) || Math.Abs(point.X - center) <= LetterSize / 2)
                {
                    return LetterTip(DrawnLetters(frame)[i]);
                }
            }
        }

        if (point.Y >= BarTop && point.Y < BarTop + TrainingBarHeight)
        {
            foreach (var item in items.Where(i => i.Kind == CwScopeItemKind.Bar))
            {
                if (point.X >= item.X && point.X <= Math.Max(item.X2, item.X + 1))
                {
                    return item.Text;
                }
            }
        }

        return CwHearingViewModel.ScopeTip;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,
            BarTop + TrainingBarHeight + BarLabelHeight);

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Frame is { } frame)
        {
            ToolTip.SetTip(this, TipAt(frame, Bounds.Width, e.GetPosition(this)));
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        ToolTip.SetTip(this, CwHearingViewModel.ScopeTip);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Frame is not { } frame || Bounds.Width <= 2 * Pad)
        {
            return;
        }

        var width = Bounds.Width;
        var toneRight = Pad + 4;
        var letters = DrawnLetters(frame);
        var letter = 0;

        context.FillRectangle(Plot, new Rect(Pad, 0, width - (2 * Pad), BarTop + TrainingBarHeight + BarLabelHeight));

        foreach (var item in Items(frame, width))
        {
            switch (item.Kind)
            {
                case CwScopeItemKind.Tone:
                    toneRight = Text(context, item.Text, Pad + 4, 1, double.IsNaN(frame.ToneHz) ? Muted : MarkInk, 11);
                    break;

                case CwScopeItemKind.Mixing:
                    Text(context, "· " + item.Text, toneRight + 6, 2, Muted, 10);
                    break;

                case CwScopeItemKind.Listening:
                    Text(context, item.Text, item.X - 20, BarTop, Muted, 10);
                    break;

                case CwScopeItemKind.Letter:
                {
                    // The letter centered over the span it was made from, with a hairline under
                    // it as long as that span, in the terminal's three ways of saying how sure:
                    // sure is bold ink, unsure italic and muted, unreadable the placeholder glyph
                    // in its own color. Weight, slant and glyph carry it as well as color (§0.6).
                    var (face, brush) = item.Text == MorseAlphabet.Unreadable
                        ? (Typeface.Default, UnreadableInk)
                        : letters[letter].Confidence == CwConfidence.High
                            ? (new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), Ink)
                            : (new Typeface(FontFamily.Default, FontStyle.Italic, FontWeight.Normal), Muted);

                    var glyph = new FormattedText(
                        item.Text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, face, LetterSize, brush);
                    var center = (item.X + item.X2) / 2;

                    context.DrawText(glyph, new Point(center - (glyph.Width / 2), LetterTop));

                    if (item.X2 - item.X >= 2)
                    {
                        context.DrawLine(SpanPen, new Point(item.X, TraceTop - 1), new Point(item.X2, TraceTop - 1));
                    }

                    letter++;
                    break;
                }

                case CwScopeItemKind.Bar:
                {
                    // The bar at its true length; its milliseconds under it where they fit.
                    var w = Math.Max(1, item.X2 - item.X);
                    context.FillRectangle(MarkInk, new Rect(item.X, BarTop, w, TrainingBarHeight));

                    var label = new FormattedText(
                        item.Text[(item.Text.IndexOf(", ", StringComparison.Ordinal) + 2)..], CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, Typeface.Default, 9, MarkInk);

                    if (label.Width <= w + 4)
                    {
                        context.DrawText(label, new Point(item.X, BarTop + TrainingBarHeight));
                    }

                    break;
                }
            }
        }
    }

    /// <summary>Draw words; returns where they end, so the next words can follow them.</summary>
    private double Text(DrawingContext context, string words, double x, double top, IBrush brush, double size)
    {
        var text = new FormattedText(
            words, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, size, brush);
        var left = Math.Clamp(x, 0, Math.Max(0, Bounds.Width - text.Width));

        context.DrawText(text, new Point(left, top));

        return left + text.Width;
    }
}
