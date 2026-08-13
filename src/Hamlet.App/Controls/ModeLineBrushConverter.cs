using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// Mode-line color: amber-brown while inside the CW segment, danger red
/// outside it — honest state per the prime directive, not decoration.
/// </summary>
public sealed class ModeLineBrushConverter : IValueConverter
{
    private static readonly IBrush Inside = new SolidColorBrush(Color.Parse("#9A4A00"));
    private static readonly IBrush Outside = new SolidColorBrush(Color.Parse("#A32D2D"));

    /// <summary>Singleton for XAML use.</summary>
    public static ModeLineBrushConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Inside : Outside;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
