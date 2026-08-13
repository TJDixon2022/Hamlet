using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// Colors the band-map status line by tone, and hides the parts of it that
/// have nothing to say.
/// </summary>
/// <remarks>
/// <para>Green when the frequency is theirs, amber when it is listen-only,
/// and plain slate when the license class is unknown.</para>
/// <para>Amber and not red is the whole point (HM-DEC-029). Being outside
/// your privileges while tuning around is not an error — it is the ordinary
/// state of most of the band for most licenses, and an app that flashed red
/// at somebody for listening would be teaching exactly the fear this feature
/// exists to remove.</para>
/// </remarks>
public sealed class PrivilegeToneConverter : IValueConverter
{
    private enum Role
    {
        Background,
        Border,
        Text,
        NotEmpty,
        NotNull,
    }

    private static readonly IBrush YoursFill = new SolidColorBrush(Color.Parse("#E9F6EC"));
    private static readonly IBrush YoursEdge = new SolidColorBrush(Color.Parse("#B7DEC4"));
    private static readonly IBrush YoursText = new SolidColorBrush(Color.Parse("#1F7A3D"));

    private static readonly IBrush ListenFill = new SolidColorBrush(Color.Parse("#FDF1E0"));
    private static readonly IBrush ListenEdge = new SolidColorBrush(Color.Parse("#EBCFA1"));
    private static readonly IBrush ListenText = new SolidColorBrush(Color.Parse("#9A5B00"));

    private static readonly IBrush PlainFill = new SolidColorBrush(Color.Parse("#F5F4F0"));
    private static readonly IBrush PlainEdge = new SolidColorBrush(Color.Parse("#DCD9D2"));
    private static readonly IBrush PlainText = new SolidColorBrush(Color.Parse("#55534E"));

    private readonly Role _role;

    private PrivilegeToneConverter(Role role) => _role = role;

    /// <summary>The panel fill for a tone.</summary>
    public static PrivilegeToneConverter Background { get; } = new(Role.Background);

    /// <summary>The panel border for a tone.</summary>
    public static PrivilegeToneConverter Border { get; } = new(Role.Border);

    /// <summary>The headline color for a tone.</summary>
    public static PrivilegeToneConverter Text { get; } = new(Role.Text);

    /// <summary>True when a string has something to show.</summary>
    public static PrivilegeToneConverter NotEmpty { get; } = new(Role.NotEmpty);

    /// <summary>True when a value is present.</summary>
    public static PrivilegeToneConverter NotNull { get; } = new(Role.NotNull);

    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (_role == Role.NotEmpty)
        {
            return value is string s && s.Trim().Length > 0;
        }

        if (_role == Role.NotNull)
        {
            return value is not null;
        }

        var tone = value as PrivilegeTone? ?? PrivilegeTone.Unknown;

        return _role switch
        {
            Role.Background => tone switch
            {
                PrivilegeTone.Yours => YoursFill,
                PrivilegeTone.ListenOnly => ListenFill,
                _ => PlainFill,
            },
            Role.Border => tone switch
            {
                PrivilegeTone.Yours => YoursEdge,
                PrivilegeTone.ListenOnly => ListenEdge,
                _ => PlainEdge,
            },
            _ => tone switch
            {
                PrivilegeTone.Yours => YoursText,
                PrivilegeTone.ListenOnly => ListenText,
                _ => PlainText,
            },
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("The status line is display-only.");
}
