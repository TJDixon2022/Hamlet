using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Hamlet.App.Controls;

/// <summary>
/// **Favorites, one drop-down under the green zone** - PHASE_PLAN.md 10.6, rev7 (Tim,
/// 2026-09-22, marked on his screen); work instruction 388 task 2.
/// </summary>
/// <remarks>
/// <para>**WHERE HE HAD THEM.** Before `a51bc2a6` of 2026-08-27 the list was a `ComboBox` beside
/// the green block reading *favorites*, and picking a line tuned there and let the box go back to
/// reading its name. That commit took it off with no ruling of its own; unit 387 brought the list
/// back as a caret on the rig face, and Tim marked the empty row under the green zone as where it
/// lived and where it goes. **One control, reading *Favorites*, in that row.**</para>
/// <para>**THE VIEW MODEL DECIDES AND THIS DRAWS.** Every line's words and every line's command
/// are <c>MainWindowViewModel.FavoriteMenu</c>'s - the same list the Radio menu shows, built by
/// the same <c>RebuildMenus</c>, each line carrying <c>TuneToFavorite</c> with its own
/// <c>FavoriteTuned</c> telemetry. Nothing here decides what a favorite is called, what tuning to
/// one does or what it records (R14).</para>
/// <para>**IT NEVER HOLDS A SELECTION** (§0.0). A box that kept showing the place picked would go
/// on claiming it the moment the dial moved, so this is a button that opens a list and always
/// reads *Favorites*.</para>
/// <para>**EMPTY IS A NOTE, NEVER A GREYED CONTROL** (§0.5.1 and the 2026-09-06 rule). With
/// nothing saved the control is still here and still opens: the list says so in a note that
/// carries no command and cannot be hit, with <c>Manage favorites…</c> under it.</para>
/// <para>**IT OPENS A LIST AND SENDS NOTHING** (§0.2). A line tunes; nothing composes, arms or
/// keys.</para>
/// </remarks>
public sealed class FavoritesDropDownControl : DropDownButton
{
    /// <summary>The saved places, each with its own way there.</summary>
    public static readonly StyledProperty<IEnumerable<ViewModels.TuneMenuItem>?> FavoritesProperty =
        AvaloniaProperty.Register<FavoritesDropDownControl, IEnumerable<ViewModels.TuneMenuItem>?>(nameof(Favorites));

    /// <summary>Rename, reorder and delete - the window the Radio menu already opens.</summary>
    public static readonly StyledProperty<ICommand?> ManageFavoritesCommandProperty =
        AvaloniaProperty.Register<FavoritesDropDownControl, ICommand?>(nameof(ManageFavoritesCommand));

    /// <summary>
    /// The sentence the list offers when nothing is saved - the same note the rig face's list
    /// offered, reused and not reworded.
    /// </summary>
    public const string NothingSaved = "Nothing saved here yet - press the star to save where you are.";

    /// <summary>Creates the control, reading its one word.</summary>
    public FavoritesDropDownControl()
    {
        Content = "Favorites";
    }

    /// <inheritdoc/>
    protected override Type StyleKeyOverride => typeof(DropDownButton);

    /// <summary>The saved places, each with its own way there.</summary>
    public IEnumerable<ViewModels.TuneMenuItem>? Favorites
    {
        get => GetValue(FavoritesProperty);
        set => SetValue(FavoritesProperty, value);
    }

    /// <summary>What the list's last line runs.</summary>
    public ICommand? ManageFavoritesCommand
    {
        get => GetValue(ManageFavoritesCommandProperty);
        set => SetValue(ManageFavoritesCommandProperty, value);
    }

    /// <summary>The list the last press put up, for a test to read.</summary>
    /// <remarks>
    /// **RECORDED FOR A TEST AND READ BY NOTHING IN THE APPLICATION**, the shape
    /// <c>MainWindow.SendFlyoutUnderTheMouse</c> already uses: a headless test presses the real
    /// control and reads back exactly what appeared.
    /// </remarks>
    internal MenuFlyout? ListShown { get; private set; }

    /// <inheritdoc/>
    /// <remarks>
    /// **THE LIST IS BUILT AT THE PRESS**, from the list as it stands then, so a place saved a
    /// moment ago is on it and a place removed is not.
    /// </remarks>
    protected override void OnClick()
    {
        var flyout = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedLeft };
        var saved = Favorites?.ToList() ?? new List<ViewModels.TuneMenuItem>();

        if (saved.Count == 0)
        {
            flyout.Items.Add(new MenuItem { Header = NothingSaved, IsHitTestVisible = false });
        }
        else
        {
            foreach (var item in saved)
            {
                flyout.Items.Add(new MenuItem { Header = item.Label, Command = item.Tune });
            }
        }

        if (ManageFavoritesCommand is { } manage)
        {
            flyout.Items.Add(new Separator());
            flyout.Items.Add(new MenuItem { Header = "Manage favorites…", Command = manage });
        }

        ListShown = flyout;
        Flyout = flyout;

        base.OnClick();
    }
}
