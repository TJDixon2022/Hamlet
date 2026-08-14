using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Hamlet.App.ViewModels;

/// <summary>
/// One line in a menu that tunes somewhere (HM-DEC-072).
/// </summary>
/// <remarks>
/// <para>IT CARRIES ITS OWN COMMAND, AND THAT IS THE WHOLE REASON IT EXISTS. A
/// menu opens in its own popup and a popup is a separate visual tree, so a
/// binding that walks up to the window looking for a command resolves to
/// nothing. The item then does nothing when it is clicked, and nothing about
/// that fails to compile or looks wrong on screen, which is the worst
/// combination there is.</para>
/// <para>Holding the command here also makes the menu testable without a
/// window: the thing a click would run is a property somebody can execute.</para>
/// </remarks>
public sealed class TuneMenuItem
{
    /// <summary>Builds one line.</summary>
    /// <param name="label">What the line says.</param>
    /// <param name="go">What clicking it does.</param>
    public TuneMenuItem(string label, Action go)
    {
        ArgumentNullException.ThrowIfNull(go);

        Label = label;
        Tune = new RelayCommand(go);
    }

    /// <summary>What the line says.</summary>
    public string Label { get; }

    /// <summary>What clicking it does.</summary>
    public ICommand Tune { get; }
}
