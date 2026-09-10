using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 302 tasks 4 and 5: **what a card says once callook has
/// answered, and what it says when callook cannot.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE HALF THAT REACHES THE SCREEN.** The directory's own tests
/// prove the cache and the parsing; this proves the sentence on the card, which is
/// what he actually reads.</para>
/// <para>**THE EXPOSURE IS §0.0 AND IT IS A NAME.** A wrong operator's name beside a
/// callsign is a claim he will repeat to the man himself, so a card is either
/// certain or silent.</para>
/// </remarks>
public sealed class Unit302CardNameTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are quoted.</param>
    public Unit302CardNameTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A US card says who he is; everyone else's is untouched.**</summary>
    [Fact]
    public async Task AUsCardSaysWhoHeIsAndTheRestAreUntouched()
    {
        var directory = new StationDirectory(new FakeLookup());

        await directory.AskAboutAsync("W7PP");
        await directory.AskAboutAsync("VP2MAA");

        var american = Card("W7PP", "DM33", directory);
        var everybody = Card("VP2MAA", "FK86", directory);

        _output.WriteLine("US station     : " + american.Callsign
            + " · " + american.Place);
        _output.WriteLine("everybody else : " + everybody.Callsign
            + " · " + everybody.Place);

        Assert.Contains("Richard", american.Place, StringComparison.Ordinal);
        Assert.Contains("Sun City AZ", american.Place, StringComparison.Ordinal);

        // **THE CARD SAYS ONE THING ONCE.** `Richard, Sun City AZ` already says the
        // United States to anybody reading it, so the country does not appear beside
        // it - which is unit 299's caption fault kept out of a smaller frame.
        Assert.DoesNotContain(
            "United States", american.Place, StringComparison.Ordinal);

        // **AND SILENCE IS THE CORRECT ANSWER RATHER THAN A GAP.** The card reads
        // exactly as it did before this unit: the country and the distance, with no
        // name, no town and nothing standing in for them.
        Assert.Contains("Montserrat", everybody.Place, StringComparison.Ordinal);

        Assert.Equal(
            "Montserrat · " + Distance(everybody.Place), everybody.Place);
    }

    /// <summary>**A card drawn before the answer arrives reads as it always did.**</summary>
    /// <remarks>
    /// **NO SPINNER, NO ERROR, NO GAP WHERE A NAME WOULD BE.** The lookup is started
    /// and left; a card is composed from what is known at the moment it is built, so
    /// a slow network costs nothing on screen.
    /// </remarks>
    [Fact]
    public void ACardDrawnBeforeTheAnswerArrivesIsUnchanged()
    {
        var directory = new StationDirectory(new FakeLookup());

        var before = Card("W7PP", "DM33", directory);

        _output.WriteLine("nothing asked yet: " + before.Callsign
            + " · " + before.Place);

        Assert.Contains(
            "United States", before.Place, StringComparison.Ordinal);

        Assert.DoesNotContain("Richard", before.Place, StringComparison.Ordinal);
    }

    /// <summary>**A client that throws leaves the card exactly as it was.**</summary>
    [Fact]
    public async Task AClientThatThrowsLeavesTheCardExactlyAsItWas()
    {
        var directory = new StationDirectory(new FakeLookup { Throws = true });

        await directory.AskAboutAsync("W7PP");

        var card = Card("W7PP", "DM33", directory);

        _output.WriteLine("with no network: " + card.Callsign + " · " + card.Place);

        Assert.Contains("United States", card.Place, StringComparison.Ordinal);
    }

    /// <summary>**The resolve rate over the callsigns this tree's fixtures use.**</summary>
    /// <remarks>
    /// **TASK 5 ASKS WHETHER THIS CLOSES THE STATE ASK OR ONLY NARROWS IT**, and the
    /// answer is a measurement rather than an opinion: how many of the callsigns
    /// actually used in this repository's fixtures get a state, and how many get a
    /// country only.
    /// </remarks>
    [Fact]
    public async Task TheResolveRateOverTheTreesOwnCallsigns()
    {
        var directory = new StationDirectory(new FakeLookup());

        // The callsigns this repository's fixtures and tests actually name.
        var calls = new[]
        {
            "W7PP", "W3YNI", "W1ABC", "KC3QIS", "K4XYZ",
            "VP2MAA", "IK4LZH", "EI4GNB", "VA3VRR", "N4L",
        };

        var withState = new List<string>();
        var countryOnly = new List<string>();

        foreach (var call in calls)
        {
            await directory.AskAboutAsync(call);

            var known = directory.Known(call);

            (known.State.Length > 0 ? withState : countryOnly).Add(call);
        }

        _output.WriteLine("callsigns asked about : " + calls.Length);
        _output.WriteLine("a state               : " + withState.Count
            + "   " + string.Join(", ", withState));
        _output.WriteLine("country only          : " + countryOnly.Count
            + "   " + string.Join(", ", countryOnly));
        _output.WriteLine("");
        _output.WriteLine(
            "THE FAKE RESOLVES ONE US CALLSIGN, so this measures the shape of the "
            + "answer and not the service's coverage: everything the service "
            + "declines gets a country and nothing below it.");

        Assert.All(countryOnly, c => Assert.Equal("", directory.Known(c).State));
        Assert.Single(withState);
    }

    /// <summary>The distance clause off a composed place line.</summary>
    /// <remarks>
    /// **THE MILES ARE READ BACK RATHER THAN RETYPED**, so this asserts the shape of
    /// the line without pinning a figure that belongs to `GridPath`.
    /// </remarks>
    private static string Distance(string place)
        => place.Split(" · ").Last();

    /// <summary>One card for a station, with whatever the directory knows.</summary>
    private static Ft8ContactCard Card(
        string callsign, string grid, StationDirectory directory)
    {
        var facts = new Ft8CardFacts(
            Callsign: callsign,
            State: Ft8ContactState.YourMove,
            Slots: 1,
            LastAtUtc: new DateTime(2026, 9, 10, 11, 58, 0, DateTimeKind.Utc),
            FirstAtUtc: new DateTime(2026, 9, 10, 11, 58, 0, DateTimeKind.Utc),
            YouCalledHim: false,
            HeCameBack: false,
            HisMessages: 1,
            YourMessages: 0,
            HeardInAll: 1,
            ReportFromHim: null,
            ReportToHim: null,
            HeRogered: false,
            YouRogered: false,
            HeSignedOff: false,
            YouSignedOff: false,
            Grid: grid,
            HisLastPayload: "CQ " + callsign + " " + grid,
            YourLastMessage: null);

        return new Ft8ContactCard(
            facts,
            "FN00",
            Ft8CardActionKind.None,
            "",
            "",
            new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc),
            who: directory.Known(callsign));
    }

    /// <summary>The real service's own shapes, with no network.</summary>
    private sealed class FakeLookup : ICallsignLookup
    {
        /// <summary>True to fail as an unreachable network does.</summary>
        public bool Throws { get; set; }

        /// <inheritdoc/>
        public string SourceName => "callook.info (fake)";

        /// <inheritdoc/>
        public Task<CallsignLookupResult?> LookupAsync(
            string callsign, CancellationToken cancellationToken = default)
        {
            if (Throws)
            {
                throw new InvalidOperationException("no network");
            }

            if (!string.Equals(callsign, "W7PP", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<CallsignLookupResult?>(null);
            }

            return Task.FromResult<CallsignLookupResult?>(
                new CallsignLookupResult(
                    "W7PP", LicenseClass.Extra, SourceName, DateTime.UtcNow)
                {
                    LicenseeName = "RICHARD R HALE",
                    TownLine = "SUN CITY, AZ 85373",
                });
        }
    }
}
