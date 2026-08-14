using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The band-conditions line (HM-DEC-025): a plain claim, its evidence beside
/// it, softer words on a thin sample, and a confession when the sources are
/// not answering.
/// </summary>
public sealed class BandConditionsTests
{
    private static readonly DateTime Now = new(2026, 8, 13, 15, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Spot(long hz, int ageMinutes = 1, string mode = "CW")
        => new("someone is calling", hz, mode, "RBN", Now.AddMinutes(-ageMinutes), 18);

    private static IReadOnlyList<ActivitySpot> OnFortyMeters(int count, int ageMinutes = 1)
        => Enumerable.Range(0, count)
            .Select(i => Spot(7_030_000 + (i * 500), ageMinutes))
            .ToList();

    private static SourceStatus Ok(string name, int count = 10)
        => new(name, SourceState.Ok, count, Now, null);

    private static SourceStatus Down(string name)
        => new(name, SourceState.Degraded, 0, Now.AddMinutes(-20), "no answer — retrying in 2 min");

    private static SourceStatus Off(string name)
        => new(name, SourceState.Disabled, 0, null, null);

    /// <remarks>
    /// Proves a busy band is called busy, with the count and the networks it
    /// came from shown beside the claim.
    /// </remarks>
    [Fact]
    public void Busy_BandIsNamedBusyWithItsEvidence()
    {
        var spots = OnFortyMeters(60);

        var line = BandConditions.Describe(
            "40 m", spots, spots, new[] { Ok("RBN"), Ok("POTA") }, Now);

        Assert.Equal(ConditionsConfidence.Sound, line.Confidence);
        Assert.Contains("unusually busy", line.Claim, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("60 signals", line.Evidence, StringComparison.Ordinal);

        // The window is named from the rule rather than pinned to a literal,
        // so widening it stays a decision rather than a test failure.
        Assert.Contains(
            $"{(int)BandConditions.Window.TotalMinutes} minutes",
            line.Evidence, StringComparison.Ordinal);
        Assert.Contains("RBN and POTA", line.Evidence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a quiet band is called quiet — but only when there is enough
    /// data to say so, which here means every source answering and a real
    /// count behind it.
    /// </remarks>
    [Fact]
    public void Quiet_BandIsNamedQuiet()
    {
        var spots = OnFortyMeters(3);

        var line = BandConditions.Describe(
            "40 m", spots, spots, new[] { Ok("RBN"), Ok("POTA") }, Now);

        Assert.Contains("40 m", line.Claim, StringComparison.Ordinal);
        Assert.Contains("3 signals", line.Evidence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the softening rule: under a handful of reports the line refuses
    /// a confident verb. Four signals is evidence that four people were heard,
    /// not evidence that a band is dead.
    /// </remarks>
    [Fact]
    public void ThinSample_SoftensTheWording()
    {
        var spots = OnFortyMeters(2);

        var line = BandConditions.Describe(
            "40 m", spots, spots, new[] { Ok("RBN") }, Now);

        Assert.Equal(ConditionsConfidence.Thin, line.Confidence);
        Assert.Contains("too little to be sure", line.Claim, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves a source that is down is named in the line rather than quietly
    /// reasoned around. "40 m looks quiet" and "40 m looks quiet, but POTA is
    /// down" are different statements.
    /// </remarks>
    [Fact]
    public void DownSource_IsConfessedInTheLine()
    {
        var spots = OnFortyMeters(30);

        var line = BandConditions.Describe(
            "40 m", spots, spots, new[] { Ok("RBN"), Down("POTA") }, Now);

        Assert.Contains("POTA", line.Evidence, StringComparison.Ordinal);
        Assert.Contains("isn't answering", line.Evidence, StringComparison.Ordinal);
        Assert.Equal(ConditionsConfidence.Thin, line.Confidence);
    }

    /// <remarks>
    /// Proves the rule Hamlet must never break: with nothing answering, the
    /// line says Hamlet cannot see the bands and explicitly disclaims saying
    /// anything about whether the band is busy. Inventing calm here is the
    /// prime directive broken (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void NoSources_NeverInventsCalm()
    {
        var line = BandConditions.Describe(
            "40 m",
            Array.Empty<ActivitySpot>(),
            Array.Empty<ActivitySpot>(),
            new[] { Down("RBN"), Down("POTA") },
            Now);

        Assert.Equal(ConditionsConfidence.Blind, line.Confidence);
        Assert.Contains("cannot see the bands", line.Claim, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "says nothing about whether the band is busy",
            line.Evidence,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("quiet", line.Claim, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("empty", line.Claim, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves every source being switched off is distinguished from every
    /// source being broken. One is the operator's doing and is fixed in
    /// Settings; the other is not.
    /// </remarks>
    [Fact]
    public void AllSourcesOff_SaysSoRatherThanReportingAFault()
    {
        var line = BandConditions.Describe(
            "40 m",
            Array.Empty<ActivitySpot>(),
            Array.Empty<ActivitySpot>(),
            new[] { Off("RBN"), Off("POTA") },
            Now);

        Assert.Equal(ConditionsConfidence.Blind, line.Confidence);
        Assert.Contains("switched off", line.Evidence, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves the empty-band case points somewhere better, with the count that
    /// justifies it. An operator who quits after an hour on a dead band is
    /// exactly who this line is for.
    /// </remarks>
    [Fact]
    public void EmptyBand_PointsAtOneWithTraffic()
    {
        var elsewhere = Enumerable.Range(0, 12)
            .Select(i => Spot(7_030_000 + (i * 500)))
            .ToList();

        var line = BandConditions.Describe(
            "20 m", Array.Empty<ActivitySpot>(), elsewhere, new[] { Ok("POTA") }, Now);

        Assert.Equal("40 m", line.SuggestedBand);
        Assert.Contains("Nobody's on 20 m", line.Claim, StringComparison.Ordinal);
        Assert.Contains("Try 40 m", line.Claim, StringComparison.Ordinal);
        Assert.Contains(
            $"No spots in the last {(int)BandConditions.Window.TotalMinutes} minutes",
            line.Evidence, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a band is not suggested on a trivial margin. Sending the
    /// operator across the dial for the sake of one extra spot would waste the
    /// trust the line is trying to earn.
    /// </remarks>
    [Fact]
    public void DoesNotSuggest_ABandOnANoisyMargin()
    {
        var here = OnFortyMeters(10);
        var everywhere = here.Concat(new[] { Spot(14_030_000), Spot(14_035_000) }).ToList();

        var line = BandConditions.Describe(
            "40 m", here, everywhere, new[] { Ok("POTA") }, Now);

        Assert.Null(line.SuggestedBand);
    }

    /// <remarks>
    /// Proves the window is real: spots older than it are not counted as
    /// current activity, so the line describes now rather than the last hour.
    /// </remarks>
    [Fact]
    public void OnlyCounts_WhatIsInsideTheWindow()
    {
        // Comfortably outside whatever the window currently is, so this proves
        // the rule rather than one particular number (HM-DEC-045).
        var old = OnFortyMeters(
            40, ageMinutes: (int)BandConditions.Window.TotalMinutes + 30);

        var line = BandConditions.Describe(
            "40 m", old, old, new[] { Ok("RBN") }, Now);

        Assert.Contains("Nobody's on 40 m", line.Claim, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the claim and the evidence are never delivered apart — the
    /// single-line form still carries both.
    /// </remarks>
    [Fact]
    public void FullText_AlwaysCarriesTheEvidence()
    {
        var spots = OnFortyMeters(30);

        var line = BandConditions.Describe(
            "40 m", spots, spots, new[] { Ok("RBN") }, Now);

        Assert.Contains(line.Claim, line.FullText, StringComparison.Ordinal);
        Assert.Contains(line.Evidence, line.FullText, StringComparison.Ordinal);
    }
}
