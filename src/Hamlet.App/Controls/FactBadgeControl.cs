using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Hamlet.App.Settings;

namespace Hamlet.App.Controls;

/// <summary>
/// The small pill beside a profile field saying whether a lookup confirmed it.
/// </summary>
/// <remarks>
/// <para>A check mark and a word, never a check mark alone (§0.6, HM-DEC-044).
/// The tick is the thing the eye finds and the word is the thing that means
/// something, and roughly one man in twelve cannot rely on the color the two
/// are drawn in.</para>
/// <para>Drawn rather than composed so the tick, the word and the rounded
/// ground stay one object with one tooltip. A Border wrapping a Path and a
/// TextBlock would hand the pointer three hit targets for one idea.</para>
/// </remarks>
public sealed class FactBadgeControl : Control
{
    /// <summary>The badge to draw. Nothing is drawn when it is not visible.</summary>
    public static readonly StyledProperty<ProfileFactBadge?> BadgeProperty =
        AvaloniaProperty.Register<FactBadgeControl, ProfileFactBadge?>(nameof(Badge));

    /// <summary>Which family's colors a verified badge uses.</summary>
    public static readonly StyledProperty<PanelFamily> FamilyProperty =
        AvaloniaProperty.Register<FactBadgeControl, PanelFamily>(nameof(Family));

    private const double PillHeight = 17;
    private const double PadX = 7;
    private const double TickWidth = 9;
    private const double Gap = 5;
    private const double FontSize = 10.5;

    private static readonly Typeface Sans = new("Segoe UI,Inter,sans-serif");

    static FactBadgeControl()
    {
        AffectsMeasure<FactBadgeControl>(BadgeProperty, FamilyProperty);
        AffectsRender<FactBadgeControl>(BadgeProperty, FamilyProperty);
    }

    /// <summary>Creates the badge.</summary>
    public FactBadgeControl() => ToolTip.SetShowDelay(this, 150);

    /// <summary>The badge to draw.</summary>
    public ProfileFactBadge? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <summary>Which family's colors a verified badge uses.</summary>
    public PanelFamily Family
    {
        get => GetValue(FamilyProperty);
        set => SetValue(FamilyProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        var badge = Badge;

        if (badge is not { IsVisible: true })
        {
            ToolTip.SetTip(this, null);
            return default;
        }

        ToolTip.SetTip(this, badge.Tooltip);

        var text = Format(badge);
        var tick = badge.IsVerified ? TickWidth + Gap : 0;

        return new Size((PadX * 2) + tick + text.Width, PillHeight);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var badge = Badge;

        if (badge is not { IsVisible: true })
        {
            return;
        }

        // A disagreement is amber whatever family the section is, because it
        // is the same caution the band map and the mismatch panel already use.
        var colors = badge.Differs
            ? PanelPalette.Amber
            : PanelPalette.For(Family);

        var text = Format(badge);
        var tick = badge.IsVerified ? TickWidth + Gap : 0;
        var width = (PadX * 2) + tick + text.Width;

        context.DrawRectangle(
            colors.PillFillBrush, null,
            new Rect(0, 0, width, PillHeight), PillHeight / 2, PillHeight / 2);

        var x = PadX;

        if (badge.IsVerified)
        {
            DrawTick(context, colors.PillInkBrush, x);
            x += TickWidth + Gap;
        }

        context.DrawText(text, new Point(x, (PillHeight - text.Height) / 2));
    }

    /// <summary>A check mark, drawn as two strokes.</summary>
    private static void DrawTick(DrawingContext context, IBrush ink, double x)
    {
        var pen = new Pen(ink, 1.6, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var mid = PillHeight / 2;

        context.DrawLine(pen, new Point(x, mid), new Point(x + 3.2, mid + 3.2));
        context.DrawLine(pen, new Point(x + 3.2, mid + 3.2), new Point(x + 9, mid - 4));
    }

    private FormattedText Format(ProfileFactBadge badge)
    {
        var colors = badge.Differs ? PanelPalette.Amber : PanelPalette.For(Family);

        return new FormattedText(
            badge.Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Sans, FontSize, colors.PillInkBrush);
    }
}
