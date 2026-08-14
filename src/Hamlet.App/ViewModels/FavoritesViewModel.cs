using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>One favorite, editable (HM-DEC-060).</summary>
public sealed partial class FavoriteRowViewModel : ObservableObject
{
    /// <summary>Wraps a favorite for the manage window.</summary>
    /// <param name="favorite">The favorite.</param>
    public FavoriteRowViewModel(Favorite favorite)
    {
        Favorite = favorite;
        _name = favorite.Name;
    }

    /// <summary>The favorite as saved.</summary>
    public Favorite Favorite { get; private set; }

    /// <summary>What the operator calls it. Editable.</summary>
    [ObservableProperty]
    private string _name;

    /// <summary>The frequency as the app writes it.</summary>
    public string FrequencyLabel => Favorite.FrequencyLabel;

    /// <summary>Its mode, its band, and when it was saved.</summary>
    /// <remarks>
    /// The three facts that answer "what was this for", which is the whole
    /// reason Hamlet's favorites exist rather than the radio's numbered slots.
    /// </remarks>
    public string Provenance
    {
        get
        {
            var parts = new List<string>();

            if (Favorite.Mode.Length > 0)
            {
                parts.Add(Favorite.Mode);
            }

            if (Favorite.BandName.Length > 0)
            {
                parts.Add(Favorite.BandName);
            }

            parts.Add($"saved {Favorite.SavedUtc.ToLocalTime():d MMMM}");

            return string.Join(" · ", parts);
        }
    }

    /// <summary>What the map said lives there, or "".</summary>
    public string Neighborhood => Favorite.Neighborhood;

    /// <summary>Take the edited name back into the record.</summary>
    /// <returns>The favorite as it now stands.</returns>
    public Favorite Commit()
    {
        var trimmed = Name.Trim();

        // An empty name is a favorite nobody could pick out of a list, so it
        // falls back to what it was called rather than becoming blank.
        Favorite = Favorite with
        {
            Name = trimmed.Length == 0 ? Favorite.Name : trimmed,
        };

        Name = Favorite.Name;
        return Favorite;
    }
}

/// <summary>
/// Renaming, reordering and deleting favorites (HM-DEC-060).
/// </summary>
/// <remarks>
/// Every row shows its mode, its band and when it was saved, because those are
/// the facts that answer "what was this for" and answering that is the whole
/// point of Hamlet keeping favorites at all rather than the radio's own memory
/// channels.
/// </remarks>
public sealed partial class FavoritesViewModel : ObservableObject
{
    private readonly ObservableCollection<Favorite> _target;
    private readonly Action _save;

    /// <summary>Opens the window over the live list.</summary>
    /// <param name="favorites">The list the app is showing.</param>
    /// <param name="save">How to write it back.</param>
    public FavoritesViewModel(ObservableCollection<Favorite> favorites, Action save)
    {
        _target = favorites ?? throw new ArgumentNullException(nameof(favorites));
        _save = save ?? throw new ArgumentNullException(nameof(save));

        Rows = new ObservableCollection<FavoriteRowViewModel>(
            favorites.Select(f => new FavoriteRowViewModel(f)));
    }

    /// <summary>Designer constructor.</summary>
    public FavoritesViewModel()
        : this(new ObservableCollection<Favorite>(), () => { })
    {
    }

    /// <summary>The rows, in the order they appear.</summary>
    public ObservableCollection<FavoriteRowViewModel> Rows { get; }

    /// <summary>The row the operator has selected, or null.</summary>
    [ObservableProperty]
    private FavoriteRowViewModel? _selected;

    /// <summary>True when there is nothing saved yet.</summary>
    public bool IsEmpty => Rows.Count == 0;

    /// <summary>What an empty window says.</summary>
    /// <remarks>
    /// Telling somebody how to fill it rather than showing them a blank list,
    /// because a blank list reads as something being broken (§0.7).
    /// </remarks>
    public const string EmptyNote =
        "Nothing saved yet. Tune somewhere worth coming back to and press the star "
        + "under the display, and Hamlet will remember the frequency and what "
        + "lives there.";

    /// <summary>Move a favorite up the list.</summary>
    [RelayCommand]
    private void MoveUp()
    {
        var at = Selected is null ? -1 : Rows.IndexOf(Selected);

        if (at > 0)
        {
            Rows.Move(at, at - 1);
            Apply();
        }
    }

    /// <summary>Move a favorite down the list.</summary>
    [RelayCommand]
    private void MoveDown()
    {
        var at = Selected is null ? -1 : Rows.IndexOf(Selected);

        if (at >= 0 && at < Rows.Count - 1)
        {
            Rows.Move(at, at + 1);
            Apply();
        }
    }

    /// <summary>Delete the selected favorite.</summary>
    [RelayCommand]
    private void Remove()
    {
        if (Selected is { } row)
        {
            Rows.Remove(row);
            Selected = null;
            Apply();
        }
    }

    /// <summary>Take the edited names and the order back into the app.</summary>
    [RelayCommand]
    private void Apply()
    {
        _target.Clear();

        foreach (var row in Rows)
        {
            _target.Add(row.Commit());
        }

        _save();
        OnPropertyChanged(nameof(IsEmpty));
    }
}
