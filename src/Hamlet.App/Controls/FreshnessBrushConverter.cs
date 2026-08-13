using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// Feed-age color: muted while fresh, amber past twice the refresh interval,
/// red once it reads "stale" (HM-DEC-020). The prime directive applied to the
/// happening-now panel — the operator sees the data aging, rather than a
/// confident count of spots that stopped being true twenty minutes ago.
/// </summary>
public sealed class FreshnessBrushConverter : IValueConverter
{
    private static readonly IBrush Fresh = new SolidColorBrush(Color.Parse("#6E6E66"));
    private static readonly IBrush Aging = new SolidColorBrush(Color.Parse("#C25E00"));
    private static readonly IBrush Stale = new SolidColorBrush(Color.Parse("#A32D2D"));

    /// <summary>Singleton for XAML use.</summary>
    public static FreshnessBrushConverter Instance { get; } = new();

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            FreshnessLevel.Stale => Stale,
            FreshnessLevel.Aging => Aging,
            _ => Fresh,
        };

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
