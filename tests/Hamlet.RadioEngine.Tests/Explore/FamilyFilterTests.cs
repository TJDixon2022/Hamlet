using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The family filter chips (HM-DEC-061).
/// </summary>
/// <remarks>
/// THE COUNT SHOWS EVEN WHEN THE FAMILY IS SWITCHED OFF, and that is the
/// teaching rather than a detail. Somebody who filters to Morse and still sees
/// forty-one voice stations learns the band is full of people they could talk
/// to, which is the fact this whole app exists to reveal.
/// </remarks>
public sealed class FamilyFilterTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 21, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Spot(string call, string mode)
        => new($"{call} is on the air", 7_032_000, mode, "test", Now, null)
        {
            DxCall = call,
        };

    private static readonly ActivitySpot[] Band =
    {
        Spot("A", "CW"), Spot("B", "CW"), Spot("C", "CW"),
        Spot("D", "FT8"), Spot("E", "RTTY"),
        Spot("F", "SSB"), Spot("G", "SSB"), Spot("H", "SSB"), Spot("I", "SSB"),
    };

    private static IReadOnlySet<ModeFamily> On(params ModeFamily[] families)
        => new HashSet<ModeFamily>(families);

    /// <remarks>
    /// THE COUNT IS OVER EVERYTHING, NOT OVER WHAT SURVIVES THE FILTER. A chip
    /// that read zero because it was switched off would be telling the operator
    /// there is nothing there, which is the opposite of what this control is
    /// for.
    /// </remarks>
    [Fact]
    public void ASwitchedOffFamilyStillShowsHowManyThereAre()
    {
        var chips = FamilyFilter.Chips(Band, On(ModeFamily.Cw));

        var voice = chips.Single(c => c.Family == ModeFamily.Phone);
        var morse = chips.Single(c => c.Family == ModeFamily.Cw);
        var digital = chips.Single(c => c.Family == ModeFamily.Digital);

        Assert.False(voice.IsOn);
        Assert.Equal(4, voice.Count);

        Assert.True(morse.IsOn);
        Assert.Equal(3, morse.Count);

        Assert.False(digital.IsOn);
        Assert.Equal(2, digital.Count);
    }

    /// <remarks>
    /// THE CHIPS FILTER AND THEY NEVER DELETE. This is one more view over the
    /// same store the lenses read, so the set handed in comes back untouched and
    /// switching a family on shows it again (HM-DEC-045, HM-DEC-057).
    /// </remarks>
    [Fact]
    public void FilteringChangesWhatIsDrawnAndNothingElse()
    {
        var morseOnly = FamilyFilter.Apply(Band, On(ModeFamily.Cw));

        Assert.Equal(3, morseOnly.Count);
        Assert.All(morseOnly, s => Assert.Equal("CW", s.Mode));

        // The set is exactly as it was handed in, and turning voice back on
        // brings all four of them back.
        Assert.Equal(9, Band.Length);
        Assert.Equal(
            7, FamilyFilter.Apply(Band, On(ModeFamily.Cw, ModeFamily.Phone)).Count);
    }

    /// <remarks>
    /// Proves all three on is the same as no filter, which is where a fresh
    /// profile starts and the state the operator returns to.
    /// </remarks>
    [Fact]
    public void EverythingOnIsTheSameAsNoFilterAtAll()
    {
        Assert.Equal(Band.Length, FamilyFilter.Apply(Band, FamilyFilter.All).Count);
        Assert.Equal("", FamilyFilter.Summary(FamilyFilter.All));
    }

    /// <remarks>
    /// EVERY CHIP OFF SHOWS EVERYTHING rather than an empty panel. Somebody who
    /// switched all three off has not asked to see nothing; they have wandered
    /// into a state with no meaning, and a blank panel would look broken.
    /// </remarks>
    [Fact]
    public void SwitchingEverythingOffShowsEverythingRatherThanNothing()
    {
        Assert.Equal(Band.Length, FamilyFilter.Apply(Band, On()).Count);
        Assert.Equal("", FamilyFilter.Summary(On()));
    }

    /// <remarks>
    /// A MODE NOTHING RECOGNIZES IS NOT HIDDEN BY A CONTROL THAT DOES NOT NAME
    /// IT. The live feeds report more names than the guide covers, and a spot
    /// that vanished because of a chip nobody could see would be the app losing
    /// something quietly (§0.0).
    /// </remarks>
    [Fact]
    public void AModeNoChipNamesIsShownWheneverAnythingIs()
    {
        var odd = Spot("Z", "SOMETHING NOBODY HAS HEARD OF");
        var set = Band.Append(odd).ToList();

        Assert.Contains(odd, FamilyFilter.Apply(set, On(ModeFamily.Cw)));
        Assert.Contains(odd, FamilyFilter.Apply(set, FamilyFilter.All));
    }

    /// <remarks>
    /// A COLLAPSED PANEL NEVER HIDES THAT IT IS FILTERING (§0.5). Somebody who
    /// shut the panel with two families off and later read a count would take it
    /// for a count of everything, which is the prime directive broken by
    /// omission.
    /// </remarks>
    [Fact]
    public void TheCollapsedSummaryNamesWhatIsBeingFilteredTo()
    {
        Assert.Equal("Morse only", FamilyFilter.Summary(On(ModeFamily.Cw)));
        Assert.Equal(
            "Morse and Voice only",
            FamilyFilter.Summary(On(ModeFamily.Cw, ModeFamily.Phone)));

        // And says nothing when nothing is filtered, rather than adding noise.
        Assert.Equal("", FamilyFilter.Summary(FamilyFilter.All));
    }

    /// <remarks>
    /// Proves the chips are the three families anybody tunes for. Open is the
    /// space between them, so a chip for it would be a filter for "whatever is
    /// left" and nobody wants that.
    /// </remarks>
    [Fact]
    public void ThreeChipsAndTheirWordsMatchTheMapsLegend()
    {
        Assert.Equal(3, FamilyFilter.Offered.Count);
        Assert.DoesNotContain(ModeFamily.Open, FamilyFilter.Offered);

        Assert.Equal("Morse", FamilyFilter.Label(ModeFamily.Cw));
        Assert.Equal("Digital", FamilyFilter.Label(ModeFamily.Digital));
        Assert.Equal("Voice", FamilyFilter.Label(ModeFamily.Phone));
    }

    /// <remarks>
    /// Proves the stored set survives a restart, and that a stored set nobody
    /// can read falls back to everything on rather than to an empty panel.
    /// </remarks>
    [Fact]
    public void AStoredSetIsRestoredAndAnUnreadableOneFallsBackToEverything()
    {
        Assert.Equal(
            On(ModeFamily.Cw, ModeFamily.Phone),
            FamilyFilter.Parse(new[] { "Cw", "Phone" }));

        Assert.Equal(FamilyFilter.All, FamilyFilter.Parse(null));
        Assert.Equal(FamilyFilter.All, FamilyFilter.Parse(Array.Empty<string>()));
        Assert.Equal(FamilyFilter.All, FamilyFilter.Parse(new[] { "nonsense" }));

        // Open is not offered, so a stored Open is ignored rather than creating
        // a chip nothing draws.
        Assert.Equal(FamilyFilter.All, FamilyFilter.Parse(new[] { "Open" }));
    }
}
