using Avalonia.Media;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.Controls;

/// <summary>One mode family's fill and ink.</summary>
/// <param name="Family">The family these colors stand for.</param>
/// <param name="Label">What the legend calls it.</param>
/// <param name="Fill">Background wash for a segment or a card.</param>
/// <param name="Ink">Text and marks drawn on that fill.</param>
public sealed record ModeColors(ModeFamily Family, string Label, Color Fill, Color Ink)
{
    /// <summary>The fill as a brush, built once.</summary>
    public IBrush FillBrush { get; } = new SolidColorBrush(Fill);

    /// <summary>The ink as a brush, built once.</summary>
    public IBrush InkBrush { get; } = new SolidColorBrush(Ink);
}

/// <summary>
/// The mode-color language: four families, fixed colors, one definition.
/// </summary>
/// <remarks>
/// <para>THE STANDING RULE (CLAUDE.md §0.6, HM-DEC-032). Mode families have
/// the same colors on every surface — the neighborhood map, the field guide,
/// the waterfall, and anything built later. Two copies of a language are two
/// languages, so both controls read from here and nothing carries a color
/// literal of its own.</para>
/// <para>The old map fills separated by lightness alone: a pale amber beside a
/// pale pink read as one wash at a glance, and pink was doing double duty as
/// both "phone segment" and, under hatching, "listen only". These separate by
/// hue and temperature as well, so the four families are distinguishable
/// without reading the labels — and none of them is pink, so the listen-only
/// hatch means one thing again.</para>
/// <para>COLOR IS NEVER THE ONLY CARRIER. Roughly one man in twelve has a
/// color vision deficiency, and this hobby's demographics make that a real
/// slice of the people who will use this. So the map labels every segment, the
/// legend names every family in words, the listen-only veil hatches as well as
/// tints, and the band cards carry an icon and a width alongside their hue.
/// Anything added later inherits that obligation.</para>
/// </remarks>
public static class ModePalette
{
    /// <summary>Morse.</summary>
    public static ModeColors Cw { get; } = new(
        ModeFamily.Cw, "Morse", Color.Parse("#EDC375"), Color.Parse("#5E3800"));

    /// <summary>RTTY, PSK31, FT8 and the rest.</summary>
    public static ModeColors Digital { get; } = new(
        ModeFamily.Digital, "Digital", Color.Parse("#BFB6E4"), Color.Parse("#2B2360"));

    /// <summary>Voice.</summary>
    public static ModeColors Phone { get; } = new(
        ModeFamily.Phone, "Voice", Color.Parse("#A3CBE8"), Color.Parse("#0B3B5C"));

    /// <summary>Open space, or a mixture.</summary>
    public static ModeColors Open { get; } = new(
        ModeFamily.Open, "Open / mixed", Color.Parse("#E4E0D5"), Color.Parse("#6E6A61"));

    /// <summary>All four, in the order the legend shows them.</summary>
    public static IReadOnlyList<ModeColors> All { get; } = new[] { Cw, Digital, Phone, Open };

    /// <summary>The colors for a family.</summary>
    /// <param name="family">The family.</param>
    /// <returns>Its fill and ink.</returns>
    public static ModeColors For(ModeFamily family) => family switch
    {
        ModeFamily.Cw => Cw,
        ModeFamily.Digital => Digital,
        ModeFamily.Phone => Phone,
        _ => Open,
    };
}
