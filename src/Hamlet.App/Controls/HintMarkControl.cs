using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>What kind of thing a <see cref="HintMarkControl"/> is holding.</summary>
/// <remarks>
/// **THE MARK SAYS WHAT KIND OF THING IT HOLDS** (work instruction 281, task 2), so
/// he can tell before he hovers whether it is worth hovering. Three kinds, because
/// three is what the sentences behind them actually are.
/// </remarks>
public enum HintKind
{
    /// <summary>Teaching: how it works, what to do, what a word means.</summary>
    Tip,

    /// <summary>A number and where it came from.</summary>
    Measurement,

    /// <summary>What Hamlet can and cannot see past (§0.0).</summary>
    Boundary,

    /// <summary>
    /// The technical detail a surface deliberately kept off its face.
    /// </summary>
    /// <remarks>
    /// **TIM'S RULING, 2026-09-08, AND IT IS THE `i` HE ASKED FOR.** His reason:
    /// *"My goal is that somebody first overcomes their failure and their inability
    /// to accomplish anything. And as they get more interested and have more
    /// success, they start looking at the technical details. But the details are
    /// nerdy and geeky, and I want to hide them behind an intentional decision to
    /// look at them."*
    /// **IT IS A KIND AND NOT A SECOND CONTROL.** The mark already exists, already
    /// leads its tooltip with the kind in words, and already refuses to draw when
    /// it has nothing to say; a second one beside it would be a copy that drifts
    /// (§0).
    /// </remarks>
    Detail,
}

/// <summary>
/// A small mark that holds a sentence, and shows it only on hover.
/// </summary>
/// <remarks>
/// <para>**TIM'S RULING, 2026-09-08**: *"I want clean visual screens with text only
/// where I, the user, intentionally hover."* Not less text — none, unless he asked
/// for it. This is what he asks with.</para>
/// <para>**NOTHING IS DELETED.** Every sentence still exists in the application, one
/// hover away, which is the whole difference between this and tidying. A sentence
/// removed is a fact lost (§0.0, HM-DEC-092) and unit 281's task 7 catches it.</para>
/// <para>**A FAULT IS NOT ADVICE AND DOES NOT COME HERE.** A clock that is wrong, a
/// slot that refused, a device that is not there: those stay visible and stay in
/// words, unhovered. That is the single exception to the ruling and putting one
/// behind this mark would break it.</para>
/// <para>**THE TOOLTIP LEADS WITH THE KIND IN WORDS**, so the glyph is never the
/// only carrier of what sort of thing this is (§0.6). Roughly one man in twelve
/// cannot rely on a color, and everybody can fail to recognise a symbol.</para>
/// <para>**IT DRAWS NOTHING WHEN IT HOLDS NOTHING.** An empty mark is a hover target
/// for a sentence that does not exist, which teaches somebody that hovering is not
/// worth it.</para>
/// <para>**THE MARK IS STILL DRAWN AND IT NOW HAS A BACKGROUND UNDER IT** (work
/// instruction 301 task 1, Tim's ruling of 2026-09-09 after comparing this against
/// unit 299's globe and finding the globe wins). The original note here argued
/// against composing this from a `Border` wrapping a `TextBlock`, and **that
/// argument still holds and is not what changed**: two hit targets for one idea, and
/// the ring and the glyph must share one tooltip. Nothing is composed, nothing is
/// nested, and there is still exactly one control and one tooltip. **One line was
/// added to `Render`.**</para>
/// <para>**WHAT WAS MEASURED, BECAUSE THE FIRST TWO ANSWERS WERE WRONG.** As it
/// stood, the mark was **not hit-testable anywhere** - nine points across it, the
/// centre included, found nothing at all behind the pointer, so it was worse than a
/// thin outline. **Giving the drawn ring a transparent fill did not fix it**: the
/// ellipse's fill reaches the picture and not the pointer. **Deriving from `Border`
/// or `Panel` is not available** - both seal `Render`, so neither can also draw a
/// ring. What does register is **a transparent rectangle over the control's own
/// bounds**, which is the same shape `Border` paints for its `Background`, and that
/// is the line below.</para>
/// <para>**SO THE TARGET IS THE MARK'S WHOLE BOX RATHER THAN THE DISC INSIDE THE
/// RING**, by about a fifth of its area at the corners. That is the forgiving
/// direction to be wrong in for somebody who has been aiming at this and missing.
/// </para>
/// <para>**A MARK HOLDING NOTHING IS STILL NOT A TARGET.** `MeasureOverride` gives it
/// no size when it holds no sentence, so the rectangle has nothing to cover and an
/// empty mark stays unhoverable, exactly as before.</para>
/// </remarks>
public sealed class HintMarkControl : Control
{
    /// <summary>The sentence it holds. Nothing is drawn when it is blank.</summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<HintMarkControl, string?>(nameof(Text));

    /// <summary>What kind of thing it holds.</summary>
    public static readonly StyledProperty<HintKind> KindProperty =
        AvaloniaProperty.Register<HintMarkControl, HintKind>(nameof(Kind));

    /// <summary>The ink. Defaults to the muted text color when unset.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<HintMarkControl, IBrush?>(nameof(Foreground));

    private const double Diameter = 14;
    private const double GlyphSize = 9.5;

    private static readonly Typeface Sans = new("Segoe UI,Inter,sans-serif");
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#6E6E66"));

    static HintMarkControl()
    {
        AffectsMeasure<HintMarkControl>(TextProperty, KindProperty);
        AffectsRender<HintMarkControl>(TextProperty, KindProperty, ForegroundProperty);
    }

    /// <summary>Creates the mark.</summary>
    /// <remarks>
    /// A short delay, matching <see cref="FactBadgeControl"/>. He is hovering on
    /// purpose, so making him wait for what he asked for is its own small rudeness.
    /// </remarks>
    public HintMarkControl() => ToolTip.SetShowDelay(this, 150);

    /// <summary>The sentence it holds.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>What kind of thing it holds.</summary>
    public HintKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The ink.</summary>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>The glyph for one kind.</summary>
    /// <param name="kind">Which kind.</param>
    /// <returns>One character.</returns>
    public static string Glyph(HintKind kind) => kind switch
    {
        HintKind.Measurement => "#",
        HintKind.Boundary => "⊣",
        HintKind.Detail => "i",
        _ => "?",
    };

    /// <summary>What the kind is called, in a word.</summary>
    /// <param name="kind">Which kind.</param>
    /// <returns>The word the tooltip leads with.</returns>
    public static string Word(HintKind kind) => kind switch
    {
        HintKind.Measurement => "measurement",
        HintKind.Boundary => "what Hamlet can see",
        HintKind.Detail => "the detail behind this",
        _ => "tip",
    };

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var text = Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            ToolTip.SetTip(this, null);
            return default;
        }

        ToolTip.SetTip(this, Word(Kind) + " — " + text.Trim());

        return new Size(Diameter, Diameter);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        var ink = Foreground ?? Muted;
        var middle = Diameter / 2;

        // **THE TRANSPARENT BACKGROUND, WHICH IS THE HIT TARGET AND IS THE WHOLE
        // OF THIS UNIT'S TASK 1.** It paints nothing. Without it the middle of this
        // mark belongs to whatever is behind it, which is what Tim was hitting.
        context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));

        // **A RING, NEVER A FILL** (HM-DEC-012). A column of filled marks reads as
        // dots of ink somebody spilled; an outline reads as somewhere to point.
        context.DrawEllipse(
            null, new Pen(ink, 1), new Point(middle, middle), middle - 0.5, middle - 0.5);

        var glyph = new FormattedText(
            Glyph(Kind), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Sans, GlyphSize, ink);

        context.DrawText(
            glyph,
            new Point(middle - (glyph.Width / 2), middle - (glyph.Height / 2)));
    }
}
