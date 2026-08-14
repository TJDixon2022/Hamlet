using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Hamlet.App.Controls;

/// <summary>
/// Inks a field in its section's color when a lookup confirmed its value.
/// </summary>
/// <remarks>
/// <para>A verified value reads as a fact rather than as a form entry
/// (HM-DEC-044): the section's darker ink instead of the ordinary text color,
/// alongside the semibold weight the style applies. The badge beside it
/// carries the word, so this is emphasis rather than information and nothing
/// is lost if it goes unnoticed (§0.6).</para>
/// <para>The unverified case returns <see cref="AvaloniaProperty.UnsetValue"/>
/// rather than null, and the difference is not academic. Null is a value: it
/// sets the foreground to nothing and the text disappears, which is exactly
/// what happened the first time this ran. UnsetValue leaves the control on
/// whatever the theme would have given it.</para>
/// </remarks>
public sealed class VerifiedInkConverter : IValueConverter
{
    private readonly PanelFamily _family;

    private VerifiedInkConverter(PanelFamily family) => _family = family;

    /// <summary>The operator section's ink.</summary>
    public static VerifiedInkConverter Green { get; } = new(PanelFamily.Green);

    /// <summary>The license section's ink.</summary>
    public static VerifiedInkConverter Amber { get; } = new(PanelFamily.Amber);

    /// <inheritdoc/>
    public object? Convert(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? PanelPalette.For(_family).PillInkBrush
            : AvaloniaProperty.UnsetValue;

    /// <inheritdoc/>
    public object ConvertBack(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("One-way only.");
}
