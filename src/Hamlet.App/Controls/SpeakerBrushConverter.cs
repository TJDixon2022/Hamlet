using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Hamlet.App.Controls;

/// <summary>
/// Tints the operator's own lines in a worked contact differently from the
/// other station's.
/// </summary>
/// <remarks>
/// <para>Two speakers alternating down a page are hard to follow without some
/// visual difference, and the difference here is deliberately slight: a warm
/// tint for your own lines against plain white for theirs. It is the same
/// restraint the glossary marking uses. The panel is trying to look like
/// somebody wrote it down for you rather than like a chat window
/// (HM-DEC-043).</para>
/// <para>Color is not the only carrier: every row is labeled "You" or "Them"
/// in words above the text, so the tint is a convenience rather than the
/// information (§0.6).</para>
/// </remarks>
public sealed class SpeakerBrushConverter : IValueConverter
{
    private static readonly IBrush YouFill = new SolidColorBrush(Color.Parse("#FBF3E4"));
    private static readonly IBrush ThemFill = Brushes.White;
    private static readonly IBrush YouEdge = new SolidColorBrush(Color.Parse("#E4CFA6"));
    private static readonly IBrush ThemEdge = new SolidColorBrush(Color.Parse("#DCD8CE"));

    private readonly bool _edge;

    private SpeakerBrushConverter(bool edge) => _edge = edge;

    /// <summary>The panel background for a speaker.</summary>
    public static SpeakerBrushConverter Fill { get; } = new(false);

    /// <summary>The border for a speaker.</summary>
    public static SpeakerBrushConverter Edge { get; } = new(true);

    /// <inheritdoc/>
    public object Convert(
        object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isYou = value is true;

        return _edge
            ? (isYou ? YouEdge : ThemEdge)
            : (isYou ? YouFill : ThemFill);
    }

    /// <inheritdoc/>
    public object ConvertBack(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("One-way only.");
}
