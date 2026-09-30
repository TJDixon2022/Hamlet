using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// **A seal**: a round, double-ruled ring in a kind's color, turned a few degrees, holding the short code
/// of what was earned (work instruction 506, `assets/achievements-look/countries-page.html`).
/// </summary>
/// <remarks>
/// <para>**DRAWN IN CODE, NO IMAGE AND NO FONT FILE** (the instruction): two circles a stroke apart and the
/// code in the application's own monospace face. The eight degrees are the picture's, so the seals read as
/// stamped rather than printed.</para>
/// <para>**THE CODE CARRIES WHAT THE HUE ONLY DECORATES** (§0.6): the row beside it names the place in words.</para>
/// </remarks>
public sealed class AchievementSealControl : Control
{
    /// <summary>What the seal holds: `LA`, `PA`, `FN20`, `20`, `FT8`.</summary>
    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<AchievementSealControl, string?>(nameof(Code));

    /// <summary>The ring's and the code's ink.</summary>
    public static readonly StyledProperty<IBrush?> InkProperty =
        AvaloniaProperty.Register<AchievementSealControl, IBrush?>(nameof(Ink));

    /// <summary>The code's size.</summary>
    public static readonly StyledProperty<double> CodeSizeProperty =
        AvaloniaProperty.Register<AchievementSealControl, double>(nameof(CodeSize), 14);

    /// <summary>How far the seal is turned, in degrees; the picture's is minus eight.</summary>
    public const double Turn = -8;

    static AchievementSealControl()
    {
        AffectsRender<AchievementSealControl>(CodeProperty, InkProperty, CodeSizeProperty);
    }

    /// <summary>What the seal holds.</summary>
    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    /// <summary>The ring's and the code's ink.</summary>
    public IBrush? Ink
    {
        get => GetValue(InkProperty);
        set => SetValue(InkProperty, value);
    }

    /// <summary>The code's size.</summary>
    public double CodeSize
    {
        get => GetValue(CodeSizeProperty);
        set => SetValue(CodeSizeProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        var side = Math.Min(Bounds.Width, Bounds.Height);

        if (side <= 6 || Ink is not { } ink)
        {
            return;
        }

        var centre = new Point(Bounds.Width / 2, Bounds.Height / 2);
        var pen = new Pen(ink, 1.5);
        var turn = Matrix.CreateTranslation(-centre.X, -centre.Y)
            * Matrix.CreateRotation(Turn * Math.PI / 180)
            * Matrix.CreateTranslation(centre.X, centre.Y);

        using (context.PushTransform(turn))
        {
            context.DrawEllipse(null, pen, centre, (side / 2) - 1, (side / 2) - 1);
            context.DrawEllipse(null, pen, centre, (side / 2) - 4.5, (side / 2) - 4.5);

            if (string.IsNullOrEmpty(Code))
            {
                return;
            }

            var text = new FormattedText(
                Code,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas, Menlo, monospace"),
                CodeSize,
                ink);

            // **THE CODE SHRINKS TO THE RING RATHER THAN SPILLING OVER IT** (§6).
            var room = side - 14;
            var scale = text.Width > room ? room / text.Width : 1;

            using (context.PushTransform(
                Matrix.CreateTranslation(-text.Width / 2, -text.Height / 2)
                * Matrix.CreateScale(scale, scale)
                * Matrix.CreateTranslation(centre.X, centre.Y)))
            {
                context.DrawText(text, new Point(0, 0));
            }
        }
    }
}
