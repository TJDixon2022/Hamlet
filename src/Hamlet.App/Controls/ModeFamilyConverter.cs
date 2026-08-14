using System.Globalization;
using Avalonia.Data.Converters;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.Controls;

/// <summary>
/// A mode family's fill or its ink, for anything that binds to a family
/// (§0.6, HM-DEC-032).
/// </summary>
/// <remarks>
/// <para>ONE DEFINITION, READ FROM <see cref="ModePalette"/>. This carries no
/// color of its own: it is the bridge between a family on the data and the two
/// brushes the palette already defines, so a surface that colors by family
/// cannot acquire a literal on the way.</para>
/// <para>COLOR IS NEVER THE ONLY CARRIER. Every surface that uses this also says
/// what the family is in words. The spot cards already read "on voice" and "on
/// CW" in their own text, so the grayscale test still passes with the fill
/// removed.</para>
/// </remarks>
public sealed class ModeFamilyConverter : IValueConverter
{
    private ModeFamilyConverter(bool ink) => _ink = ink;

    private readonly bool _ink;

    /// <summary>The family's background wash.</summary>
    public static ModeFamilyConverter Fill { get; } = new(ink: false);

    /// <summary>The family's ink, which clears WCAG AA on its own fill.</summary>
    public static ModeFamilyConverter Ink { get; } = new(ink: true);

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var colors = ModePalette.For(value is ModeFamily family ? family : ModeFamily.Open);
        return _ink ? colors.InkBrush : colors.FillBrush;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// A family chip's opacity: full when it is on, dimmed when it is off
/// (HM-DEC-061).
/// </summary>
/// <remarks>
/// DIMMED AND NEVER HIDDEN, and never to nothing. The chip is what carries the
/// count of a family that is switched off, and that count is the teaching: a
/// filtered-out family that disappeared would tell the operator it had stopped
/// existing. It also gives the chip a second carrier beyond its fill, so the
/// on-or-off state survives the grayscale test (§0.6).
/// </remarks>
public sealed class ChipOpacityConverter : IValueConverter
{
    /// <summary>How dim a switched-off chip is drawn.</summary>
    public const double OffOpacity = 0.45;

    /// <summary>Singleton for XAML use.</summary>
    public static ChipOpacityConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 1.0 : OffOpacity;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
