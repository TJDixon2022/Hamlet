using Hamlet.App.Licensing;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Xunit;

namespace Hamlet.App.Tests.Licensing;

/// <summary>
/// The grid square is derived, never demanded, and never overwrites a
/// hand-entered value (HM-DEC-037).
/// </summary>
public sealed class GridResolverTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Trafford, PA — the coordinates callook holds for KC3QIS.</summary>
    private static readonly LatLon Home = new(40.3782746, -79.7081649);

    private static CallsignLookupResult Answer(LatLon? at)
        => new("KC3QIS", LicenseClass.General, "callook.info", Now) { Location = at };

    private static OperatorProfile Profile(string grid = "", bool byHand = false)
    {
        var profile = new OperatorProfile { Callsign = "KC3QIS", Location = "Trafford, PA" };

        if (grid.Length > 0)
        {
            if (byHand)
            {
                profile.SetGridByHand(grid, Now);
            }
            else
            {
                profile.SetPositionFromLookup(
                    OperatorLocation.FromGrid(grid)!.Value, "callook.info", Now);
            }
        }

        return profile;
    }

    /// <remarks>
    /// THE ONE THAT MATTERS. Proves Tim's profile as it stands — callsign set,
    /// grid empty — resolves on the next launch, which is what makes the band
    /// cards visible for the first time (HM-DEC-033).
    /// </remarks>
    [Fact]
    public void AnEmptyGrid_IsFilledFromTheLookup()
    {
        var profile = Profile();

        Assert.True(GridResolver.NeedsResolution(profile));

        var outcome = GridResolver.Apply(profile, Answer(Home), Now);

        Assert.Equal(GridResolutionOutcome.Resolved, outcome.Outcome);
        Assert.Equal("FN00DJ", profile.GridSquare);
        Assert.Equal(ProfileFactSource.LookedUp, profile.GridSquareSource);
        Assert.Equal("callook.info", profile.GridSquareSourceName);
        Assert.Equal("2026-08-14", profile.GridSquareSetOn);
        Assert.False(GridResolver.NeedsResolution(profile));
    }

    /// <remarks>
    /// Proves the coordinates are stored, not just the locator. Distance and
    /// the solar clock both want degrees, and a locator only ever gives them
    /// back to within a few miles.
    /// </remarks>
    [Fact]
    public void TheCoordinatesAreStoredBehindTheLocator()
    {
        var profile = Profile();
        GridResolver.Apply(profile, Answer(Home), Now);

        Assert.Equal(Home.Latitude, profile.Latitude!.Value, 6);
        Assert.Equal(Home.Longitude, profile.Longitude!.Value, 6);
        Assert.Equal(Home, profile.Position);
    }

    /// <remarks>
    /// THE RULE FROM HM-DEC-028, APPLIED AGAIN. Proves a hand-entered grid
    /// survives a lookup that disagrees: nothing is written, both values are
    /// carried, and the operator decides. It binds harder here than for the
    /// class — the FCC holds a mailing address, and somebody operating
    /// portable knows better than it does.
    /// </remarks>
    [Fact]
    public void AHandEnteredGrid_SurvivesALookupThatDisagrees()
    {
        var profile = Profile("EM79", byHand: true);

        var outcome = GridResolver.Apply(profile, Answer(Home), Now);

        Assert.Equal(GridResolutionOutcome.Mismatch, outcome.Outcome);
        Assert.True(outcome.NeedsOperatorDecision);

        // Untouched.
        Assert.Equal("EM79", profile.GridSquare);
        Assert.Equal(ProfileFactSource.EnteredByOperator, profile.GridSquareSource);

        // Both values are on offer.
        Assert.Contains("FN00", outcome.Found, StringComparison.Ordinal);
        Assert.Equal("EM79", outcome.Existing);
        Assert.Contains("FN00", outcome.Narration, StringComparison.Ordinal);
        Assert.Contains("EM79", outcome.Narration, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves an agreement is silent. A hand-entered grid in the same square as
    /// the lookup is not a disagreement, and interrupting somebody because
    /// their antenna sits in FN00DJ rather than FN00DK would be pedantry with a
    /// dialog attached.
    /// </remarks>
    [Theory]
    [InlineData("FN00")]
    [InlineData("FN00dj")]
    [InlineData("FN00ab")]
    public void AHandEnteredGridInTheSameSquare_IsNotADisagreement(string grid)
    {
        var profile = Profile(grid, byHand: true);

        var outcome = GridResolver.Apply(profile, Answer(Home), Now);

        Assert.Equal(GridResolutionOutcome.NotNeeded, outcome.Outcome);
        Assert.Equal("", outcome.Narration);
        Assert.Equal(OperatorLocation.Normalize(grid), profile.GridSquare);
    }

    /// <remarks>
    /// Proves a lookup with no coordinates leaves the field empty and says so,
    /// rather than deriving one from the location string. "Trafford, PA" places
    /// a call district, which is a published table, and it does not place a
    /// station within seventy miles.
    /// </remarks>
    [Fact]
    public void NoCoordinates_LeavesTheFieldEmptyAndNeverGuessesFromTheLocation()
    {
        var profile = Profile();

        var outcome = GridResolver.Apply(profile, Answer(null), Now);

        Assert.Equal(GridResolutionOutcome.NoCoordinates, outcome.Outcome);
        Assert.Equal("", profile.GridSquare);
        Assert.Null(profile.Latitude);
        Assert.Null(profile.Position);
        Assert.Contains("Settings", outcome.Narration, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a service that could not be reached at all changes nothing and
    /// says nothing. Being offline is a condition, not an event worth a status
    /// line about a field the operator never asked for.
    /// </remarks>
    [Fact]
    public void NoAnswerAtAll_ChangesNothing()
    {
        var profile = Profile();

        var outcome = GridResolver.Apply(profile, null, Now);

        Assert.Equal(GridResolutionOutcome.NotNeeded, outcome.Outcome);
        Assert.Equal("", profile.GridSquare);
        Assert.Equal("", outcome.Narration);
    }

    /// <remarks>
    /// Proves a grid already resolved by a lookup is refreshed without
    /// narrating. Nobody wants to be told about their grid square on every
    /// startup for the rest of the app's life.
    /// </remarks>
    [Fact]
    public void AnAlreadyResolvedGrid_IsRefreshedSilently()
    {
        var profile = Profile("FN00DJ");

        var moved = new LatLon(41.0, -80.0);
        var outcome = GridResolver.Apply(profile, Answer(moved), Now);

        Assert.Equal("", outcome.Narration);
        Assert.Equal(OperatorLocation.ToGrid(moved), profile.GridSquare);
    }

    /// <remarks>
    /// Proves clearing the box hands the field back to the lookup rather than
    /// pinning it empty forever.
    /// </remarks>
    [Fact]
    public void ClearingTheGrid_HandsItBackToTheLookup()
    {
        var profile = Profile("EM79", byHand: true);

        profile.SetGridByHand("", Now);

        Assert.Equal("", profile.GridSquare);
        Assert.Equal(ProfileFactSource.Unset, profile.GridSquareSource);
        Assert.False(profile.GridSquareWasSetByHand);
        Assert.True(GridResolver.NeedsResolution(profile));
    }

    /// <remarks>
    /// Proves a locator that is not a locator is kept as typed but backs no
    /// coordinates, so nothing downstream draws a distance from a square that
    /// does not exist.
    /// </remarks>
    [Theory]
    [InlineData("nonsense")]
    [InlineData("ZZ99")]
    [InlineData("F")]
    public void AMalformedGrid_BacksNoCoordinates(string typed)
    {
        var profile = Profile();
        profile.SetGridByHand(typed, Now);

        Assert.Equal(typed.Trim().ToUpperInvariant(), profile.GridSquare);
        Assert.Null(profile.Latitude);
        Assert.Null(profile.Position);
    }

    /// <remarks>
    /// Proves the explanation is prose a person would say, not a definition
    /// (§0.7). The test names the two things it must contain: what it is like,
    /// and what Hamlet does with it.
    /// </remarks>
    [Fact]
    public void TheExplanationIsWrittenForSomebodyWhoHasNeverHeardOfIt()
    {
        var text = GridResolver.Explanation;

        Assert.Contains("postal code", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("how far away", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sun", text, StringComparison.OrdinalIgnoreCase);

        // The jargon is the thing being dissolved, so it may not be the
        // explanation.
        Assert.DoesNotContain("Maidenhead", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("locator", text, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the provenance line says where the value came from, so "FN00DJ,
    /// from your license address" and "FN00DJ, because you typed it" never look
    /// the same (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void ProvenanceSaysWhereTheValueCameFrom()
    {
        var empty = GridResolver.DescribeProvenance(Profile());
        Assert.Contains("Not set", empty, StringComparison.OrdinalIgnoreCase);

        var looked = Profile();
        GridResolver.Apply(looked, Answer(Home), Now);
        Assert.Contains("callook.info", GridResolver.DescribeProvenance(looked), StringComparison.Ordinal);

        var typed = Profile("EM79", byHand: true);
        Assert.Contains("you set this", GridResolver.DescribeProvenance(typed), StringComparison.OrdinalIgnoreCase);
    }
}
