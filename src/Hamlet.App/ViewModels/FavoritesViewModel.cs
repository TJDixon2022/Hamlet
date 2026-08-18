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
        _note = favorite.Note;
    }

    /// <summary>The favorite as saved.</summary>
    public Favorite Favorite { get; private set; }

    /// <summary>What the operator calls it. Editable.</summary>
    [ObservableProperty]
    private string _name;

    /// <summary>Why this one, in his own words. Editable, and usually empty.</summary>
    /// <remarks>
    /// The same line the strip beside the name carries, editable here because
    /// this is where somebody comes to tidy up. Empty is ordinary and stays
    /// empty: nothing suggests one.
    /// </remarks>
    [ObservableProperty]
    private string _note;

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

            // **AN EMPTY NOTE IS ALLOWED TO STAY EMPTY**, unlike the name. A
            // favorite with no name is one nobody can pick out of a list; a
            // favorite with no note is the ordinary case, and clearing one is a
            // thing the operator is entitled to do.
            Note = (Note ?? "").Trim(),
        };

        Name = Favorite.Name;
        Note = Favorite.Note;
        return Favorite;
    }
}

/// <summary>One place the operator has been, ready to be starred (HM-DEC-072).</summary>
public sealed partial class RecentRowViewModel : ObservableObject
{
    /// <summary>Wraps an entry for the manage window.</summary>
    /// <param name="entry">The entry.</param>
    /// <param name="isSaved">Whether a favorite already sits there.</param>
    public RecentRowViewModel(RecentStation entry, bool isSaved)
    {
        Entry = entry;
        _isSaved = isSaved;
    }

    /// <summary>The entry.</summary>
    public RecentStation Entry { get; }

    /// <summary>How it reads: a station where one was identified, a place where
    /// none was.</summary>
    public string Label => Entry.Label;

    /// <summary>The frequency as the app writes it.</summary>
    public string FrequencyLabel => Entry.FrequencyLabel;

    /// <summary>Mode, band and when he was there.</summary>
    public string Provenance
    {
        get
        {
            var parts = new List<string>();

            if (Entry.Mode.Length > 0)
            {
                parts.Add(Entry.Mode);
            }

            if (Entry.BandName.Length > 0)
            {
                parts.Add(Entry.BandName);
            }

            parts.Add($"you were here {Entry.VisitedUtc.ToLocalTime():d MMMM}");

            // Where a station is named, how Hamlet knows travels with it
            // (HM-DEC-073).
            if (Entry.IsIdentified)
            {
                parts.Add(Entry.Provenance);
            }

            return string.Join(" · ", parts);
        }
    }

    /// <summary>True when this place is already a favorite.</summary>
    [ObservableProperty]
    private bool _isSaved;

    /// <summary>What the star says.</summary>
    public string StarLabel => IsSaved ? "★ saved" : "☆ save";

    partial void OnIsSavedChanged(bool value) => OnPropertyChanged(nameof(StarLabel));
}

/// <summary>
/// Renaming, reordering and deleting favorites, and starring the places he has
/// been (HM-DEC-060, HM-DEC-072).
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
    private readonly Action<RecentStation>? _star;

    /// <summary>Opens the window over the live lists.</summary>
    /// <param name="favorites">The list the app is showing.</param>
    /// <param name="save">How to write it back.</param>
    /// <param name="recent">Where the operator has been, or null.</param>
    /// <param name="star">How to turn one of those into a favorite, or null.</param>
    public FavoritesViewModel(
        ObservableCollection<Favorite> favorites,
        Action save,
        IEnumerable<RecentStation>? recent = null,
        Action<RecentStation>? star = null)
    {
        _target = favorites ?? throw new ArgumentNullException(nameof(favorites));
        _save = save ?? throw new ArgumentNullException(nameof(save));
        _star = star;

        Rows = new ObservableCollection<FavoriteRowViewModel>(
            favorites.Select(f => new FavoriteRowViewModel(f)));

        Recent = new ObservableCollection<RecentRowViewModel>(
            (recent ?? Enumerable.Empty<RecentStation>())
                .Select(r => new RecentRowViewModel(r, IsSaved(r))));
    }

    /// <summary>Designer constructor.</summary>
    public FavoritesViewModel()
        : this(new ObservableCollection<Favorite>(), () => { })
    {
    }

    /// <summary>Where the operator has been (HM-DEC-072).</summary>
    public ObservableCollection<RecentRowViewModel> Recent { get; }

    /// <summary>True when there is anywhere to show.</summary>
    public bool HasRecent => Recent.Count > 0;

    /// <summary>
    /// Star a place he has been into a favorite.
    /// </summary>
    /// <param name="row">The row.</param>
    /// <remarks>
    /// THE OTHER DOOR TO THE SAME ACT (HM-DEC-072). The app owns the save, so
    /// this hands it back rather than building a favorite of its own, and a
    /// favorite born here is the same object as one born at the star.
    /// </remarks>
    [RelayCommand]
    private void Star(RecentRowViewModel? row)
    {
        if (row is null || row.IsSaved || _star is null)
        {
            return;
        }

        _star(row.Entry);
        row.IsSaved = IsSaved(row.Entry);
    }

    private bool IsSaved(RecentStation entry)
        => Favorites.At(_target, entry.FrequencyHz) is not null;

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
        + "in the display, and Hamlet will remember the frequency and what lives "
        + "there. Anywhere you have already been is below, and the star beside it "
        + "does the same job after the fact.";

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
