using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>One chip in the favorites row under the green zone.</summary>
/// <param name="Favorite">The saved spot, handed to the tune and forget commands.</param>
/// <remarks>
/// **A ROW OF CHIPS SINCE WORK INSTRUCTION 390** (Tim, 2026-09-22: *"Favorites looks boring! Sex
/// it up"*), refining 10.6's drop-down. Everything here is read off the saved spot; nothing is
/// decided twice, so the chip, the Radio menu and the manage window cannot disagree.
/// </remarks>
public sealed record FavoriteChip(Favorite Favorite)
{
    /// <summary>The spot's frequency and mode, *14.074 USB-D*, drawn in the family's ink.</summary>
    public string Spot => Favorite.Mode.Length > 0
        ? Favorite.FrequencyLabel + " " + Favorite.Mode
        : Favorite.FrequencyLabel;

    /// <summary>The name he gave it, beside the spot.</summary>
    public string Name => Favorite.Name;

    /// <summary>The family the saved mode belongs to, for the ink.</summary>
    public ModeFamily Family => FamilyOf(Favorite.Mode);

    /// <summary>The hover: the name and his note, where he wrote one.</summary>
    public string Hover => Favorite.HasNote
        ? Favorite.Name + " - " + Favorite.Note + ". Click to tune."
        : Favorite.Name + ". Click to tune.";

    /// <summary>Which family a saved rig mode belongs to.</summary>
    /// <param name="mode">The mode as the rig display wrote it when the spot was saved.</param>
    /// <returns>The family, or open where the mode does not say.</returns>
    /// <remarks>
    /// **THE RIG WRITES `USB-D` FOR A DATA MODE AND `USB-?` WHERE THE FLAG WAS UNREAD**
    /// (`RigState.ModeWithVariant`). `-D` is digital; `-?` could be voice or data and is open,
    /// because coloring it either would be a guess (§0.0). Everything else is the mode guide's own
    /// answer (<see cref="ModeGuide.FamilyFor"/>), so no second list of modes is kept here.
    /// </remarks>
    public static ModeFamily FamilyOf(string? mode)
    {
        var name = (mode ?? "").Trim();

        if (name.EndsWith("-D", StringComparison.OrdinalIgnoreCase))
        {
            return ModeFamily.Digital;
        }

        return name.EndsWith("-?", StringComparison.Ordinal)
            ? ModeFamily.Open
            : ModeGuide.FamilyFor(name);
    }
}
