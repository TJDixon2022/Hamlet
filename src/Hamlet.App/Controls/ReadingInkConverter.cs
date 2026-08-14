using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// Green for a real, current reading; dimmer for one that is stale, absent or
/// impossible.
/// </summary>
/// <remarks>
/// Color repeats what the row already says in words, and never carries the
/// meaning alone (§0.6): the value column says "unknown" where there is no
/// reading, and the age column says how old a stale one is. A reader who cannot
/// separate the two greens still gets the whole fact.
/// </remarks>
public sealed class ReadingInkConverter : IValueConverter
{
    /// <summary>The shared instance.</summary>
    public static ReadingInkConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object Convert(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? InstrumentPalette.ConfidentBrush : InstrumentPalette.UncertainBrush;

    /// <inheritdoc/>
    public object ConvertBack(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("Reading ink is display only.");
}
