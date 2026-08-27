using CommunityToolkit.Mvvm.ComponentModel;

namespace Hamlet.App.ViewModels;

/// <summary>
/// One of the three tabs, and whether it is the one selected.
/// </summary>
/// <remarks>
/// <para>**SELECTION IS STATE ON THE TAB, NOT ARITHMETIC IN A CONVERTER.** The
/// first build of the tab strip bound each button's `IsChecked` to the selected
/// mode through a converter, passing the button's own name as
/// `ConverterParameter={Binding}`. **A converter parameter cannot itself be a
/// binding**, so it never resolved: `Convert` compared the mode against nothing
/// and returned false for all three, and `ConvertBack` read the name as null and
/// wrote an empty mode.</para>
/// <para>**MEASURED ON 2026-08-27**: a fresh window showed all three tabs
/// unchecked, and the first press of any tab set the mode to the empty string,
/// after which no workspace matched and the screen went blank permanently. The
/// operator photographed it.</para>
/// <para>So each tab owns a boolean the strip binds straight to, and the view
/// model keeps the three in step. There is nothing left to resolve at render
/// time.</para>
/// </remarks>
public sealed partial class ModeTabViewModel : ObservableObject
{
    /// <summary>Creates a tab.</summary>
    /// <param name="name">What it is called, and the mode it selects.</param>
    /// <param name="chosen">What to call when this tab is picked.</param>
    public ModeTabViewModel(string name, Action<string> chosen)
    {
        Name = name;
        _chosen = chosen;
    }

    private readonly Action<string> _chosen;

    /// <summary>What the tab is called.</summary>
    public string Name { get; }

    /// <summary>True while this is the tab showing.</summary>
    /// <remarks>
    /// **THE SETTER ONLY ACTS ON BEING PICKED.** A radio group unchecks the old
    /// button before it checks the new one, so acting on the uncheck would blank
    /// the selection for an instant and take the workspace with it — which is
    /// the failure this whole class exists to prevent.
    /// </remarks>
    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            _chosen(Name);
        }
    }

    /// <summary>Set the flag when the mode led rather than the tab.</summary>
    /// <param name="selected">Whether this is now the tab showing.</param>
    /// <remarks>
    /// Setting the property is safe in both directions: the change handler only
    /// acts on being picked, and picking the tab that is already the mode sets
    /// the same mode again, which the view model's own equality check absorbs.
    /// </remarks>
    internal void Follow(bool selected) => IsSelected = selected;
}
