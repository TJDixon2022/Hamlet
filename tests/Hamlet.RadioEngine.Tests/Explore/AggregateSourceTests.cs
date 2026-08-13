using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The aggregate's honest-degradation rules (HM-DEC-022): one dead network
/// never takes the panel down, an off switch means gone, and a failing source
/// backs off instead of hammering.
/// </summary>
public sealed class AggregateSourceTests
{
    private static readonly DateTime Start = new(2026, 8, 13, 15, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Spot(string source, string call, DateTime heardAt)
        => new($"{call} is calling CQ", 7_032_000, "CW", source, heardAt, 15)
        {
            DxCall = call,
            CallType = SpotCallType.Cq,
        };

    /// <remarks>
    /// Proves the rule that matters most when a volunteer service falls over:
    /// a source that fails leaves its previous spots on screen, ageing where
    /// the operator can see them, and is marked Degraded rather than blanking
    /// a panel somebody was reading (HM-DEC-022).
    /// </remarks>
    [Fact]
    public async Task DeadSource_LeavesItsPreviousSpotsVisible()
    {
        var clock = Start;
        var alive = true;

        var flaky = new StubSource("POTA", () => alive
            ? new[] { Spot("POTA", "K3ABC", clock) }
            : throw new HttpRequestExceptionStub());

        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { flaky }, _ => true, () => clock);

        var first = await aggregate.GetSpotsAsync();
        Assert.Single(first);

        alive = false;
        clock = Start.AddMinutes(1);

        var second = await aggregate.GetSpotsAsync();

        Assert.Single(second);
        Assert.Equal("K3ABC", second[0].DxCall);

        var status = aggregate.Statuses.Single();
        Assert.Equal(SourceState.Degraded, status.State);
        Assert.True(status.IsLetDown);
        Assert.Equal(Start, status.LastOkUtc);
    }

    /// <remarks>
    /// Proves the confession half of the same rule: once the cached spots have
    /// aged past being "happening now" there is nothing left to show, and the
    /// source says Failed rather than reporting an empty band.
    /// </remarks>
    [Fact]
    public async Task DeadSource_EventuallyReportsFailedRatherThanEmpty()
    {
        var clock = Start;
        var alive = true;

        var flaky = new StubSource("POTA", () => alive
            ? new[] { Spot("POTA", "K3ABC", clock) }
            : throw new HttpRequestExceptionStub());

        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { flaky }, _ => true, () => clock);

        await aggregate.GetSpotsAsync();

        alive = false;
        clock = Start + AggregateActivitySource.CacheMaxAge + TimeSpan.FromMinutes(1);

        var spots = await aggregate.GetSpotsAsync();

        Assert.Empty(spots);
        Assert.Equal(SourceState.Failed, aggregate.Statuses.Single().State);
    }

    /// <remarks>
    /// Proves a failing source is retried on a backoff rather than on every
    /// refresh. An outage should cost a struggling volunteer service nothing.
    /// </remarks>
    [Fact]
    public async Task FailingSource_BacksOffBeforeRetrying()
    {
        var clock = Start;
        var source = new StubSource("POTA", () => throw new HttpRequestExceptionStub());

        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { source }, _ => true, () => clock);

        await aggregate.GetSpotsAsync();
        Assert.Equal(1, source.Calls);

        // Inside the backoff window: not asked again.
        clock = Start.AddSeconds(10);
        await aggregate.GetSpotsAsync();
        Assert.Equal(1, source.Calls);

        // Past it: asked again.
        clock = Start + SourceBackoff.BaseDelay + TimeSpan.FromSeconds(1);
        await aggregate.GetSpotsAsync();
        Assert.Equal(2, source.Calls);
    }

    /// <remarks>
    /// Proves the backoff schedule doubles from its base and stops at the cap,
    /// with no clock read and no randomness — so it is testable exactly (§5).
    /// </remarks>
    [Fact]
    public void Backoff_DoublesThenCaps()
    {
        Assert.Equal(TimeSpan.Zero, SourceBackoff.Delay(0));
        Assert.Equal(SourceBackoff.BaseDelay, SourceBackoff.Delay(1));
        Assert.Equal(SourceBackoff.BaseDelay * 2, SourceBackoff.Delay(2));
        Assert.Equal(SourceBackoff.BaseDelay * 4, SourceBackoff.Delay(3));
        Assert.Equal(SourceBackoff.MaxDelay, SourceBackoff.Delay(50));
        Assert.True(SourceBackoff.Delay(1000) <= SourceBackoff.MaxDelay);
    }

    /// <remarks>
    /// Proves "off" means gone. A source the operator switched off contributes
    /// nothing and its cached spots are dropped — a disabled feed that kept
    /// quietly supplying the list would make the switch a lie.
    /// </remarks>
    [Fact]
    public async Task DisabledSource_ContributesNothing()
    {
        var clock = Start;
        var enabled = true;

        var source = new StubSource("POTA", () => new[] { Spot("POTA", "K3ABC", clock) });
        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { source }, _ => enabled, () => clock);

        Assert.Single(await aggregate.GetSpotsAsync());

        enabled = false;
        var spots = await aggregate.GetSpotsAsync();

        Assert.Empty(spots);
        Assert.Equal(SourceState.Disabled, aggregate.Statuses.Single().State);
        Assert.Equal(1, source.Calls);
    }

    /// <remarks>
    /// Proves one source failing does not stop the others being read. This is
    /// the whole point of fanning out rather than chaining.
    /// </remarks>
    [Fact]
    public async Task OneFailure_DoesNotSuppressTheOthers()
    {
        var clock = Start;
        var broken = new StubSource("POTA", () => throw new HttpRequestExceptionStub());
        var working = new StubSource("RBN", () => new[] { Spot("RBN", "W1ABC", clock) });

        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { broken, working }, _ => true, () => clock);

        var spots = await aggregate.GetSpotsAsync();

        Assert.Single(spots);
        Assert.Equal("W1ABC", spots[0].DxCall);
        Assert.Contains(aggregate.Statuses, s => s is { Name: "RBN", State: SourceState.Ok });
        Assert.Contains(aggregate.Statuses, s => s is { Name: "POTA", State: SourceState.Failed });
    }

    /// <remarks>
    /// Proves duplicates collapse in preference order. The same station is
    /// routinely reported by RBN and POTA within seconds, and the operator
    /// should keep the version that knows it is a park activation.
    /// </remarks>
    [Fact]
    public async Task Duplicates_KeepTheRicherSource()
    {
        var clock = Start;

        var pota = new StubSource("POTA", () => new[]
        {
            Spot("POTA", "K3ABC", clock) with { IsActivation = true, PlaceLabel = "US-PA" },
        });

        var rbn = new StubSource("RBN", () => new[] { Spot("RBN", "K3ABC", clock) });

        // POTA leads, as the shell constructs it.
        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { pota, rbn }, _ => true, () => clock);

        var spot = Assert.Single(await aggregate.GetSpotsAsync());

        Assert.Equal("POTA", spot.Source);
        Assert.True(spot.IsActivation);
    }

    /// <remarks>
    /// Proves spots older than the cache window are dropped rather than shown
    /// as current. A forty-five-minute-old spot is not "happening now"
    /// (HM-DEC-009, HM-DEC-020).
    /// </remarks>
    [Fact]
    public async Task StaleSpots_AreDropped()
    {
        var clock = Start;
        var old = Start - AggregateActivitySource.CacheMaxAge - TimeSpan.FromMinutes(5);

        var source = new StubSource("POTA", () => new[] { Spot("POTA", "K3ABC", old) });
        var aggregate = new AggregateActivitySource(
            new IActivitySource[] { source }, _ => true, () => clock);

        Assert.Empty(await aggregate.GetSpotsAsync());
    }

    /// <summary>Stands in for a transport failure without a live socket.</summary>
    private sealed class HttpRequestExceptionStub : Exception
    {
    }
}
