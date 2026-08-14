using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The spot history: nothing is lost on restart, and it never grows without
/// bound (HM-DEC-045).
/// </summary>
public sealed class SpotStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-spots-" + Guid.NewGuid().ToString("N"));

    private static readonly DateTime Now = new(2026, 8, 14, 20, 0, 0, DateTimeKind.Utc);

    private string DbPath => Path.Combine(_folder, SqliteSpotStore.FileName);

    private static ActivitySpot Spot(
        string call,
        long hz = 7_032_000,
        int agoMinutes = 0,
        bool activation = false,
        string source = "POTA")
        => new(
            $"{call} is on the air", hz, "CW", source,
            Now.AddMinutes(-agoMinutes), 15)
        {
            DxCall = call,
            IsActivation = activation,
        };

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp folder is not worth failing a test over.
        }
    }

    /// <remarks>
    /// THE ONE THAT MATTERS. Proves a spot survives the process: written by
    /// one store, read back by another opened over the same file. This is what
    /// closes the RBN startup gap, where a fresh run used to know nothing at
    /// all until somebody transmitted.
    /// </remarks>
    [Fact]
    public void ASpotSurvivesARestart()
    {
        using (var first = SqliteSpotStore.TryOpen(DbPath))
        {
            Assert.NotNull(first);
            Assert.Equal(1, first!.Record(new[] { Spot("W1ABC") }, Now));
        }

        using var second = SqliteSpotStore.TryOpen(DbPath);

        Assert.NotNull(second);
        var held = second!.Since(Now.AddHours(-6));

        Assert.Single(held);
        Assert.Equal("W1ABC", held[0].Spot.DxCall);
        Assert.Equal(Now, held[0].Spot.HeardAtUtc);
    }

    /// <remarks>
    /// Proves every field the record carries comes back intact, so history is
    /// as good as a live spot rather than a thinner version of one.
    /// </remarks>
    [Fact]
    public void EverythingOnTheRecordSurvives()
    {
        var rich = new ActivitySpot(
            "NA9M is activating Capital City State Trail", 7_046_000, "CW", "POTA",
            Now.AddMinutes(-3), 17)
        {
            CallType = SpotCallType.Cq,
            SignalDb = 24,
            DxCall = "NA9M",
            SpotterCall = "KJ7DT",
            Proximity = SpotProximity.Continent,
            IsActivation = true,
            Reference = "US-4410",
            PlaceLabel = "US-WI",
            ReportCount = 7,
            StationLocation = new LatLon(43.0731, -89.4012),
        };

        using var store = SqliteSpotStore.TryOpen(DbPath);
        store!.Record(new[] { rich }, Now);

        var back = store.Since(Now.AddHours(-1))[0].Spot;

        Assert.Equal(rich.Story, back.Story);
        Assert.Equal(rich.FrequencyHz, back.FrequencyHz);
        Assert.Equal(rich.Mode, back.Mode);
        Assert.Equal(rich.Source, back.Source);
        Assert.Equal(rich.HeardAtUtc, back.HeardAtUtc);
        Assert.Equal(rich.Wpm, back.Wpm);
        Assert.Equal(rich.CallType, back.CallType);
        Assert.Equal(rich.SignalDb, back.SignalDb);
        Assert.Equal(rich.DxCall, back.DxCall);
        Assert.Equal(rich.SpotterCall, back.SpotterCall);
        Assert.Equal(rich.Proximity, back.Proximity);
        Assert.True(back.IsActivation);
        Assert.Equal(rich.Reference, back.Reference);
        Assert.Equal(rich.PlaceLabel, back.PlaceLabel);
        Assert.Equal(rich.ReportCount, back.ReportCount);
        Assert.Equal(43.0731, back.StationLocation!.Value.Latitude, 4);
    }

    /// <remarks>
    /// DEDUPLICATION UPDATES, IT DOES NOT INSERT. Proves seeing the same
    /// station again moves the last-seen time and leaves the report time
    /// alone. A station spotted again twenty minutes later did not start
    /// calling twenty minutes later, and treating a re-sighting as a new event
    /// would present an old spot as if it had just arrived.
    /// </remarks>
    [Fact]
    public void SeeingASpotAgainUpdatesLastSeenRatherThanInserting()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        var first = Spot("W1ABC", agoMinutes: 20);
        Assert.Equal(1, store!.Record(new[] { first }, Now.AddMinutes(-20)));
        Assert.Equal(0, store.Record(new[] { first }, Now));
        Assert.Equal(1, store.Count());

        var held = store.Since(Now.AddHours(-6))[0];

        Assert.Equal(Now.AddMinutes(-20), held.Spot.HeardAtUtc);
        Assert.Equal(Now.AddMinutes(-20), held.FirstSeenUtc);
        Assert.Equal(Now, held.LastSeenUtc);
    }

    /// <remarks>
    /// Proves the same station on a frequency a few hertz away is the same
    /// station, since two skimmers measuring one carrier rarely agree to the
    /// hertz. And that a genuinely different frequency is a different spot.
    /// </remarks>
    [Fact]
    public void NearlyTheSameFrequencyIsTheSameSpot()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        store!.Record(new[] { Spot("W1ABC", 7_032_000) }, Now);
        store.Record(new[] { Spot("W1ABC", 7_032_100) }, Now);
        Assert.Equal(1, store.Count());

        store.Record(new[] { Spot("W1ABC", 7_040_000) }, Now);
        Assert.Equal(2, store.Count());
    }

    /// <remarks>
    /// Proves an activation beats a bare skimmer report of the same station.
    /// Knowing somebody is in a park is worth more to a newcomer than knowing
    /// a receiver heard them, and the merge must not lose that by arriving in
    /// the wrong order.
    /// </remarks>
    [Fact]
    public void AnActivationBeatsABareSkimmerReport()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        store!.Record(new[] { Spot("W1ABC", source: "RBN") }, Now);
        store.Record(new[] { Spot("W1ABC", activation: true) }, Now);

        Assert.Equal(1, store.Count());
        Assert.True(store.Since(Now.AddHours(-1))[0].Spot.IsActivation);
    }

    /// <remarks>
    /// PRUNING BOUNDS THE STORE. Proves old rows go and recent ones stay, so
    /// the file cannot grow forever on a machine left running for a month.
    /// </remarks>
    [Fact]
    public void PruningBoundsTheStore()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        for (var i = 0; i < 40; i++)
        {
            store!.Record(
                new[] { Spot($"W{i}ABC", 7_000_000 + (i * 1000), agoMinutes: i * 120) },
                Now);
        }

        Assert.Equal(40, store!.Count());

        var gone = store.Prune(Now.AddDays(-1));

        Assert.True(gone > 0, "pruning removed nothing");
        Assert.Equal(40 - gone, store.Count());
        Assert.All(
            store.Since(DateTime.MinValue),
            r => Assert.True(r.Spot.HeardAtUtc >= Now.AddDays(-1)));
    }

    /// <remarks>
    /// Proves the cutoff is respected, so a view over the last hour does not
    /// quietly include yesterday.
    /// </remarks>
    [Fact]
    public void SinceRespectsItsCutoff()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        store!.Record(
            new[]
            {
                Spot("W1ABC", 7_010_000, agoMinutes: 5),
                Spot("W2ABC", 7_020_000, agoMinutes: 90),
                Spot("W3ABC", 7_030_000, agoMinutes: 600),
            },
            Now);

        Assert.Equal(3, store.Count());
        Assert.Single(store.Since(Now.AddMinutes(-30)));
        Assert.Equal(2, store.Since(Now.AddMinutes(-120)).Count);
        Assert.Equal(3, store.Since(Now.AddDays(-1)).Count);
    }

    /// <remarks>
    /// Proves the newest report comes back first, which is the order the list
    /// wants and the order the ranking assumes.
    /// </remarks>
    [Fact]
    public void HistoryComesBackNewestFirst()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        store!.Record(
            new[]
            {
                Spot("W2ABC", 7_020_000, agoMinutes: 30),
                Spot("W1ABC", 7_010_000, agoMinutes: 2),
                Spot("W3ABC", 7_030_000, agoMinutes: 60),
            },
            Now);

        var order = store.Since(Now.AddHours(-6)).Select(r => r.Spot.DxCall).ToList();

        Assert.Equal(new[] { "W1ABC", "W2ABC", "W3ABC" }, order);
    }

    /// <remarks>
    /// A STORE THAT CANNOT BE OPENED DEGRADES WITHOUT THROWING. Proves a path
    /// that cannot become a file returns null rather than an exception, so the
    /// caller falls back to memory and the app still starts. Refusing to run
    /// over a cache would be a bug (§8).
    /// </remarks>
    [Fact]
    public void AStoreThatCannotBeOpenedReturnsNull()
    {
        // A file standing where the folder would have to be.
        Directory.CreateDirectory(_folder);
        var blocker = Path.Combine(_folder, "blocked");
        File.WriteAllText(blocker, "not a folder");

        var store = SqliteSpotStore.TryOpen(Path.Combine(blocker, "sub", SqliteSpotStore.FileName));

        Assert.Null(store);
    }

    /// <remarks>
    /// Proves a disposed store answers harmlessly rather than throwing, since
    /// a refresh can land while the window is closing.
    /// </remarks>
    [Fact]
    public void ADisposedStoreIsHarmless()
    {
        var store = SqliteSpotStore.TryOpen(DbPath);
        store!.Record(new[] { Spot("W1ABC") }, Now);
        store.Dispose();

        Assert.Equal(0, store.Record(new[] { Spot("W2ABC") }, Now));
        Assert.Empty(store.Since(Now.AddHours(-1)));
        Assert.Equal(0, store.Prune(Now));
        Assert.Equal(0, store.Count());

        // And disposing twice is fine.
        store.Dispose();
    }

    /// <remarks>
    /// Proves the memory fallback applies the same rules, so behaviour does
    /// not quietly change when the database could not be opened.
    /// </remarks>
    [Fact]
    public void TheMemoryFallbackBehavesTheSameWay()
    {
        using var store = new MemorySpotStore();

        Assert.False(store.IsPersistent);

        var spot = Spot("W1ABC", agoMinutes: 20);
        Assert.Equal(1, store.Record(new[] { spot }, Now.AddMinutes(-20)));
        Assert.Equal(0, store.Record(new[] { spot }, Now));
        Assert.Equal(1, store.Count());

        var held = store.Since(Now.AddHours(-6))[0];
        Assert.Equal(Now.AddMinutes(-20), held.Spot.HeardAtUtc);
        Assert.Equal(Now, held.LastSeenUtc);

        store.Record(new[] { Spot("W9ABC", 7_090_000, agoMinutes: 600) }, Now);
        Assert.Equal(1, store.Prune(Now.AddHours(-2)));
    }

    /// <remarks>
    /// Proves the persistent store says it is persistent and the fallback says
    /// it is not, so the app can tell the operator which one they have rather
    /// than letting them assume history is being kept.
    /// </remarks>
    [Fact]
    public void EachStoreSaysWhetherItSurvivesARestart()
    {
        using var disk = SqliteSpotStore.TryOpen(DbPath);
        using var memory = new MemorySpotStore();

        Assert.True(disk!.IsPersistent);
        Assert.False(memory.IsPersistent);
    }

    /// <remarks>
    /// MARKED, NEVER REMOVED (HM-DEC-057). Tuning to a spot takes it out of
    /// "what's new" and leaves it exactly where it was under "best chance",
    /// because it is still a live station and somebody may want to go back. The
    /// mark survives a restart, so a station worked last night is not offered
    /// again this morning as an arrival.
    /// </remarks>
    [Fact]
    public void AVisitIsRecordedAndSurvivesARestartWithoutRemovingAnything()
    {
        var spot = Spot("W3ABC");
        var key = SpotIdentity.KeyFor(spot);

        using (var store = SqliteSpotStore.TryOpen(DbPath))
        {
            store!.Record(new[] { spot, Spot("K2XYZ", 7_040_000) }, Now);
            store.MarkActedOn(key, Now);

            Assert.Equal(2, store.Count());
        }

        using var reopened = SqliteSpotStore.TryOpen(DbPath);
        var held = reopened!.Since(Now.AddHours(-1));

        // Both rows are still there. One of them remembers being visited.
        Assert.Equal(2, held.Count);
        Assert.Single(held, r => r.ActedOnUtc is not null);
        Assert.Equal(
            "W3ABC",
            held.Single(r => r.ActedOnUtc is not null).Spot.DxCall);
    }

    /// <remarks>
    /// Proves marking something the store has never heard of does nothing and
    /// says nothing, rather than inventing a row for a spot that does not exist
    /// (§0.0).
    /// </remarks>
    [Fact]
    public void MarkingASpotTheStoreDoesNotHoldChangesNothing()
    {
        using var store = SqliteSpotStore.TryOpen(DbPath);

        store!.MarkActedOn("nobody|0", Now);

        Assert.Equal(0, store.Count());
    }

    /// <remarks>
    /// Proves the memory fallback behaves the same way, since it is what the
    /// app runs on when the database cannot be opened and what most of these
    /// tests drive.
    /// </remarks>
    [Fact]
    public void TheMemoryFallbackRemembersAVisitToo()
    {
        using var store = new MemorySpotStore();
        var spot = Spot("W3ABC");

        store.Record(new[] { spot }, Now);
        store.MarkActedOn(SpotIdentity.KeyFor(spot), Now);

        Assert.Equal(1, store.Count());
        Assert.NotNull(store.Since(Now.AddHours(-1))[0].ActedOnUtc);
    }
}
