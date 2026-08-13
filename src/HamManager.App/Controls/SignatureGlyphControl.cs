using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using HamManager.RadioEngine.Explore;

namespace HamManager.App.Controls;

/// <summary>
/// A mode's waterfall fingerprint, drawn small and dark — the field guide's
/// "what it looks like" identity (HM-DEC-016). Five archetypes: CW dots,
/// FT8 blocks, SSB smear, RTTY rails, PSK31 ribbon.
/// </summary>
public sealed class SignatureGlyphControl : Control
{
    private static readonly IBrush ScreenBrush = new SolidColorBrush(Color.Parse("#12161A"));
    private static readonly IBrush TraceBrush = new SolidColorBrush(Color.Parse("#7EE3A9"));
    private static readonly IBrush FaintBrush = new SolidColorBrush(Color.Parse("#2E5A45"));

    /// <summary>Which fingerprint to draw.</summary>
    public static readonly StyledProperty<SignatureKind> KindProperty =
        AvaloniaProperty.Register<SignatureGlyphControl, SignatureKind>(nameof(Kind));

    static SignatureGlyphControl()
    {
        AffectsRender<SignatureGlyphControl>(KindProperty);
    }

    /// <summary>Which fingerprint to draw.</summary>
    public SignatureKind Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize) => new(82, 40);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        context.DrawRectangle(ScreenBrush, null, new Rect(0, 0, w, h), 4, 4);

        switch (Kind)
        {
            case SignatureKind.Dots:
                Bar(context, 8, 0.45, 6);
                Bar(context, 18, 0.45, 14);
                Bar(context, 38, 0.45, 6);
                Bar(context, 48, 0.45, 6);
                Bar(context, 60, 0.45, 14);
                break;

            case SignatureKind.Blocks:
                context.FillRectangle(TraceBrush, new Rect(8, 6, 10, h * 0.35));
                context.FillRectangle(TraceBrush, new Rect(26, h * 0.55, 10, h * 0.35));
                context.FillRectangle(TraceBrush, new Rect(44, 6, 10, h * 0.35));
                context.FillRectangle(TraceBrush, new Rect(62, h * 0.55, 10, h * 0.35));
                break;

            case SignatureKind.Smear:
                context.FillRectangle(FaintBrush, new Rect(8, 6, w - 16, h - 12));
                context.FillRectangle(TraceBrush, new Rect(8, h * 0.32, w - 16, 4));
                context.FillRectangle(FaintBrush, new Rect(8, h * 0.62, w - 16, 3));
                break;

            case SignatureKind.Rails:
                context.FillRectangle(TraceBrush, new Rect(w * 0.28, 5, 4, h - 10));
                context.FillRectangle(TraceBrush, new Rect(w * 0.66, 5, 4, h - 10));
                break;

            case SignatureKind.Ribbon:
                context.FillRectangle(TraceBrush, new Rect(w / 2 - 1.5, 5, 3, h - 10));
                break;
        }
    }

    private void Bar(DrawingContext context, double x, double yFrac, double width)
        => context.FillRectangle(TraceBrush,
            new Rect(x, Bounds.Height * yFrac, width, 3.5));
}
