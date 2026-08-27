using System.Globalization;
using Avalonia.Data.Converters;

namespace Hamlet.App.Controls;

/// <summary>
/// Ties a radio button to the mode it names, both ways.
/// </summary>
/// <remarks>
/// **A TAB STRIP IS A RADIO GROUP AND THE SELECTION IS A STRING.** Binding each
/// button's checked state straight to the mode would need one property per mode,
/// which is three places for the selection to disagree with itself. This holds
/// one, and each button asks whether it is the one.
/// </remarks>
public sealed class ModeTabConverter : IValueConverter
{
    /// <summary>The one instance, since it holds nothing.</summary>
    public static ModeTabConverter Instance { get; } = new();

    /// <summary>True where the selected mode is this button's own.</summary>
    /// <param name="value">The selected mode.</param>
    /// <param name="targetType">Ignored.</param>
    /// <param name="parameter">This button's mode.</param>
    /// <param name="culture">Ignored.</param>
    /// <returns>Whether this button is the selected one.</returns>
    public object Convert(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.Equals(
            value as string, parameter as string, StringComparison.Ordinal);

    /// <summary>The mode this button names, when it is the one checked.</summary>
    /// <param name="value">Whether it was checked.</param>
    /// <param name="targetType">Ignored.</param>
    /// <param name="parameter">This button's mode.</param>
    /// <param name="culture">Ignored.</param>
    /// <returns>The mode, or nothing where the button was unchecked.</returns>
    /// <remarks>
    /// **UNCHECKING SAYS NOTHING**, deliberately. A radio group unchecks the old
    /// button before checking the new one, and writing back on the uncheck would
    /// blank the selection for an instant and take the panel with it.
    /// </remarks>
    public object? ConvertBack(
        object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true
            ? parameter as string
            : Avalonia.Data.BindingOperations.DoNothing;
}
