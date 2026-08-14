using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.Controls;

/// <summary>
/// A block of copy that quietly marks the jargon in it and explains it on
/// hover.
/// </summary>
/// <remarks>
/// <para>THE MARKING IS QUIET (HM-DEC-041). A dotted underline in a muted
/// brown, visible if you are looking for it and invisible if you are not.
/// Somebody who has known what CQ means for forty years should never notice
/// this control exists. Nothing anywhere says "tutorial mode", nothing pulses,
/// and no color shouts.</para>
/// <para>That restraint is the whole design. The person this is for has spent
/// six years feeling like the hobby has a password he was never given, and an
/// app that decorated every third word with a help icon would be telling him
/// the same thing in a friendlier font.</para>
/// <para>It is a drawn control rather than a stack of inline runs because
/// Avalonia's inline hit-testing does not reach individual runs, and the
/// tooltip has to know which word the pointer is over. Drawing also keeps
/// wrapping under this control's own control, which matters when a term must
/// not be split across a line break.</para>
/// </remarks>
public sealed class GlossaryTextControl : Control
{
    /// <summary>The copy to show.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<GlossaryTextControl, string?>(nameof(Text));

    /// <summary>Font size in device-independent pixels.</summary>
    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<GlossaryTextControl, double>(nameof(FontSize), 12.0);

    /// <summary>Ink for ordinary text.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<GlossaryTextControl, IBrush?>(nameof(Foreground));

    /// <summary>Space between wrapped lines, as a multiple of the font size.</summary>
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<GlossaryTextControl, double>(nameof(LineSpacing), 1.35);

    /// <summary>
    /// The underline under a marked term. Muted brown, thin, and dotted.
    /// </summary>
    private static readonly IBrush MarkBrush = new SolidColorBrush(Color.Parse("#8A6A45"));

    private static readonly Typeface Sans = new("Segoe UI,Inter,sans-serif");

    private readonly Cursor _helpCursor = new(StandardCursorType.Help);

    private Placed[] _layout = Array.Empty<Placed>();
    private double _laidOutFor = -1;
    private GlossaryTerm? _hovered;

    static GlossaryTextControl()
    {
        AffectsMeasure<GlossaryTextControl>(TextProperty, FontSizeProperty, LineSpacingProperty);
        AffectsRender<GlossaryTextControl>(ForegroundProperty);

        // The layout is cached against the width it was built for, which is
        // most of what makes this control cheap. Anything that changes the
        // GLYPHS rather than the box has to drop that cache by hand, or the
        // control keeps drawing the previous text at the same size forever.
        // The lead card is bound to a property that changes on every refresh
        // inside a panel of fixed width, so it sat on its first value.
        TextProperty.Changed.AddClassHandler<GlossaryTextControl>((c, _) => c.Invalidate());
        FontSizeProperty.Changed.AddClassHandler<GlossaryTextControl>((c, _) => c.Invalidate());
        ForegroundProperty.Changed.AddClassHandler<GlossaryTextControl>((c, _) => c.Invalidate());
        LineSpacingProperty.Changed.AddClassHandler<GlossaryTextControl>((c, _) => c.Invalidate());
    }

    /// <summary>Drop the cached layout so the next pass rebuilds it.</summary>
    private void Invalidate()
    {
        _laidOutFor = -1;
        _layout = Array.Empty<Placed>();
        _hovered = null;
        InvalidateVisual();
    }

    /// <summary>Creates the control.</summary>
    public GlossaryTextControl()
    {
        ClipToBounds = false;
        ToolTip.SetShowDelay(this, 150);
    }

    /// <summary>The copy to show.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Font size in device-independent pixels.</summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>Ink for ordinary text.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>Space between wrapped lines, as a multiple of the font size.</summary>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width is double.PositiveInfinity or <= 0
            ? 400
            : availableSize.Width;

        Layout(width);
        return new Size(width, LaidOutHeight());
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Layout(finalSize.Width);
        return new Size(finalSize.Width, LaidOutHeight());
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        Layout(Bounds.Width);

        foreach (var placed in _layout)
        {
            context.DrawText(placed.Text, placed.Origin);

            if (placed.Term is null)
            {
                continue;
            }

            // The mark: a dotted rule a shade under the baseline. Drawn as
            // short segments rather than with a dash pattern, so it looks the
            // same at every scaling factor.
            var y = placed.Origin.Y + placed.Text.Baseline + 2.0;
            for (var x = placed.Origin.X; x < placed.Origin.X + placed.Text.Width; x += 3.0)
            {
                context.FillRectangle(MarkBrush, new Rect(x, y, 1.4, 1.0));
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        SetHover(TermAt(e.GetPosition(this)));
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetHover(null);
    }

    private void SetHover(GlossaryTerm? term)
    {
        if (ReferenceEquals(term, _hovered))
        {
            return;
        }

        _hovered = term;

        if (term is null)
        {
            ToolTip.SetIsOpen(this, false);
            ToolTip.SetTip(this, null);
            Cursor = Cursor.Default;
            return;
        }

        ToolTip.SetIsOpen(this, false);
        ToolTip.SetTip(this, $"{term.Heading}\n\n{term.Explanation}");
        ToolTip.SetIsOpen(this, true);
        Cursor = _helpCursor;
    }

    private GlossaryTerm? TermAt(Point point)
    {
        foreach (var placed in _layout)
        {
            if (placed.Term is null)
            {
                continue;
            }

            var box = new Rect(
                placed.Origin.X,
                placed.Origin.Y,
                placed.Text.Width,
                placed.Text.Height);

            if (box.Contains(point))
            {
                return placed.Term;
            }
        }

        return null;
    }

    private double LaidOutHeight()
        => _layout.Length == 0
            ? 0
            : _layout.Max(p => p.Origin.Y + p.Text.Height);

    /// <summary>
    /// Place every word, wrapping at the available width.
    /// </summary>
    /// <remarks>
    /// Words are placed one at a time so a wrap never lands inside a marked
    /// term, which would leave half a dotted underline at the end of a line
    /// and look like a rendering fault rather than a mark.
    /// </remarks>
    private void Layout(double width)
    {
        if (Math.Abs(width - _laidOutFor) < 0.5 && _layout.Length > 0)
        {
            return;
        }

        _laidOutFor = width;

        var text = Text ?? "";
        if (text.Length == 0 || width <= 0)
        {
            _layout = Array.Empty<Placed>();
            return;
        }

        var ink = Foreground ?? Brushes.Black;
        var lineHeight = FontSize * LineSpacing;
        var spaceWidth = MeasureSpace(ink);

        var placed = new List<Placed>();
        double x = 0;
        double y = 0;

        foreach (var span in Glossary.Mark(text))
        {
            foreach (var token in SplitKeepingSpaces(span.Text))
            {
                if (token == "\n")
                {
                    x = 0;
                    y += lineHeight;
                    continue;
                }

                // The space is carried as an advance rather than as part of
                // the drawn text. FormattedText.Width ignores trailing
                // whitespace, so a word measured with its space attached
                // advances as though the space were not there and the whole
                // paragraph runs together.
                var trailing = token.EndsWith(' ');
                var word = trailing ? token.TrimEnd(' ') : token;

                if (word.Length == 0)
                {
                    x += spaceWidth;
                    continue;
                }

                var formatted = new FormattedText(
                    word, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    Sans, FontSize, ink);

                if (x > 0 && x + formatted.Width > width)
                {
                    x = 0;
                    y += lineHeight;
                }

                placed.Add(new Placed(formatted, new Point(x, y), span.Term));
                x += formatted.Width + (trailing ? spaceWidth : 0);
            }
        }

        _layout = placed.ToArray();
    }

    /// <summary>
    /// The width of one space at the current size.
    /// </summary>
    /// <remarks>
    /// Measured as the difference between two strings rather than by
    /// formatting a lone space, because a lone space measures as zero for the
    /// same reason a trailing one does.
    /// </remarks>
    private double MeasureSpace(IBrush ink)
    {
        double Width(string s) => new FormattedText(
            s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Sans, FontSize, ink).Width;

        return Math.Max(1.0, Width("n n") - Width("nn"));
    }

    /// <summary>
    /// Break a run into words, keeping the spaces so the text is reassembled
    /// exactly as written.
    /// </summary>
    private static IEnumerable<string> SplitKeepingSpaces(string text)
    {
        var start = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                if (i > start)
                {
                    yield return text[start..i];
                }

                yield return "\n";
                start = i + 1;
                continue;
            }

            if (text[i] != ' ')
            {
                continue;
            }

            // Keep the space attached to the word before it, so a marked term
            // never carries a trailing underlined space.
            yield return text[start..(i + 1)];
            start = i + 1;
        }

        if (start < text.Length)
        {
            yield return text[start..];
        }
    }

    private sealed record Placed(FormattedText Text, Point Origin, GlossaryTerm? Term);
}
