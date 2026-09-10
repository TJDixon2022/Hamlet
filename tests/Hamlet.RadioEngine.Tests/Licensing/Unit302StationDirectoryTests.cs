using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hamlet.RadioEngine.Licensing;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Licensing;

/// <summary>
/// Work instruction 302 tasks 4, 5 and 6: **a name and a town on the card, asked
/// once, and silent where it cannot be known.**
/// </summary>
/// <remarks>
/// <para>**THE EXPOSURE IS A NAME ON A CARD** (§0.0). A wrong operator's name beside
/// a callsign is not a display fault - it is a claim the operator will repeat to the
/// person he is working. So the rule is certain or silent: **no hedge, no
/// *unknown*, and nothing inferred from a callsign or a grid.**</para>
/// <para>**THE FIXTURES ARE THE REAL SERVICE'S OWN SHAPES**, read from callook on
/// 2026-09-10 rather than imagined: `W7PP` answers with `RICHARD R HALE` and
/// `SUN CITY, AZ 85373`, and `VP2MAA` answers `{"status": "INVALID"}` and nothing
/// else. **No test here touches the network.**</para>
/// <para>**IT WAS WATCHED FAILING.** With `StationName.From` handing back the
/// licensee name whole, `AUsCallsignShowsAFirstNameAndATown` reports
/// `RICHARD R HALE` where a card wants somebody to greet.</para>
/// </remarks>
public sealed class Unit302StationDirectoryTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public Unit302StationDirectoryTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>**A US callsign gets a first name and a town.**</summary>
    [Fact]
    public async Task AUsCallsignShowsAFirstNameAndATown()
    {
        var directory = new StationDirectory(new FakeLookup());

        await directory.AskAboutAsync("W7PP");

        var known = directory.Known("W7PP");

        _output.WriteLine("callsign : " + known.Callsign);
        _output.WriteLine("name     : " + known.Name);
        _output.WriteLine("town     : " + known.Town);
        _output.WriteLine("state    : " + known.State);

        Assert.Equal("Richard", known.Name);
        Assert.Equal("Sun City AZ", known.Town);
        Assert.Equal("AZ", known.State);
    }

    /// <summary>**A callsign the service declines gets nothing at all.**</summary>
    /// <remarks>
    /// **SILENCE IS THE CORRECT ANSWER AND NOT A GAP** (Tim, 2026-09-08). Most of
    /// what makes FT8 interesting is outside the United States, and those cards read
    /// exactly as they always did.
    /// </remarks>
    [Fact]
    public async Task ACallsignTheServiceDeclinesGetsNothing()
    {
        var directory = new StationDirectory(new FakeLookup());

        foreach (var call in new[] { "VP2MAA", "IK4LZH", "EI4GNB" })
        {
            await directory.AskAboutAsync(call);

            var known = directory.Known(call);

            _output.WriteLine(
                call.PadRight(8) + "name=\"" + known.Name + "\"  town=\""
                + known.Town + "\"  anything=" + known.HasAnything);

            Assert.False(
                known.HasAnything,
                call + " was given a name or a town it cannot have");
        }
    }

    /// <summary>**A client that throws leaves the card exactly as it was.**</summary>
    /// <remarks>
    /// **OFFLINE IS A CONDITION AND NOT AN ERROR.** No spinner, no message, no gap:
    /// the card shows what it always showed. **And the failure is not remembered** -
    /// the network being down says nothing about this callsign, so the next ask is
    /// allowed to succeed.
    /// </remarks>
    [Fact]
    public async Task AClientThatThrowsLeavesTheCardAsItWas()
    {
        var broken = new FakeLookup { Throws = true };
        var directory = new StationDirectory(broken);

        await directory.AskAboutAsync("W7PP");

        _output.WriteLine(
            "after a throw: anything=" + directory.Known("W7PP").HasAnything);

        Assert.False(directory.Known("W7PP").HasAnything);

        // **AND IT IS ASKED AGAIN ONCE THE NETWORK IS BACK**, which a cached failure
        // would have prevented for ever.
        broken.Throws = false;

        await directory.AskAboutAsync("W7PP");

        _output.WriteLine(
            "after it recovers: name=\"" + directory.Known("W7PP").Name + "\"");

        Assert.Equal("Richard", directory.Known("W7PP").Name);
    }

    /// <summary>**One lookup per callsign, however many decodes name it.**</summary>
    /// <remarks>
    /// **FOURTEEN MESSAGES A SLOT AND FOUR SLOTS A MINUTE** is a great many requests
    /// for a station he sees repeatedly, and unit 278's worked-mark reads the log
    /// once for the same reason. **The answer that there is nothing to know is cached
    /// too**, or a non-US callsign would be asked about every slot for ever.
    /// </remarks>
    [Fact]
    public async Task OneLookupPerCallsignHoweverManyDecodesNameIt()
    {
        var lookup = new FakeLookup();
        var directory = new StationDirectory(lookup);

        // A busy slot: fourteen messages, three stations, seen over four slots.
        for (var slot = 0; slot < 4; slot++)
        {
            foreach (var call in Busy())
            {
                await directory.AskAboutAsync(call);
            }
        }

        _output.WriteLine("messages seen : " + (Busy().Count * 4));
        _output.WriteLine("distinct calls: " + Busy().Distinct().Count());
        _output.WriteLine("requests made : " + lookup.Calls.Count);
        _output.WriteLine("  " + string.Join(", ", lookup.Calls));

        Assert.Equal(Busy().Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            lookup.Calls.Count);
    }

    /// <summary>**The state is read only where the line is really that shape.**</summary>
    /// <remarks>
    /// **A STATION PLACED IN THE WRONG STATE IS THE CONFIDENT WRONG ANSWER §0.0
    /// EXISTS TO PREVENT**, so anything that is not `TOWN, XX ZIP` yields nothing
    /// rather than being forced into a state.
    /// </remarks>
    [Theory]
    [InlineData("SUN CITY, AZ 85373", "Sun City AZ", "AZ")]
    [InlineData("TRAFFORD, PA 15085", "Trafford PA", "PA")]
    [InlineData("NEW YORK, NY 10001", "New York NY", "NY")]
    [InlineData("SOMEWHERE ABROAD", "", "")]
    [InlineData("LONDON, GREATER LONDON SW1", "", "")]
    [InlineData("", "", "")]
    public void TheStateIsReadOnlyWhereTheLineIsThatShape(
        string line, string town, string state)
    {
        _output.WriteLine(
            ("\"" + line + "\"").PadRight(30) + " -> \""
            + StationName.TownAndState(line) + "\"  state \""
            + StationName.StateOf(line) + "\"");

        Assert.Equal(town, StationName.TownAndState(line));
        Assert.Equal(state, StationName.StateOf(line));
    }

    /// <summary>**A name that is not a person's yields nothing.**</summary>
    /// <remarks>
    /// **A CLUB LICENCE HOLDS AN ORGANISATION**, and greeting somebody by a club's
    /// name is the same fault as greeting them by the wrong one.
    /// </remarks>
    [Theory]
    [InlineData("RICHARD R HALE", "Richard")]
    [InlineData("TIMOTHY J DIXON", "Timothy")]
    [InlineData("MARY-ANNE O CONNOR", "Mary-Anne")]
    [InlineData("O'BRIEN PATRICK J", "O'Brien")]
    [InlineData("R HALE", "")]
    [InlineData("HALE", "")]
    [InlineData("", "")]
    public void ANameThatIsNotAPersonsYieldsNothing(string full, string first)
    {
        _output.WriteLine(
            ("\"" + full + "\"").PadRight(24) + " -> \""
            + StationName.FirstName(full) + "\"");

        Assert.Equal(first, StationName.FirstName(full));
    }

    /// <summary>A busy slot's worth of callsigns, three of them distinct.</summary>
    private static List<string> Busy()
        => new()
        {
            "W7PP", "VP2MAA", "W7PP", "IK4LZH", "W7PP", "VP2MAA", "W7PP",
            "IK4LZH", "W7PP", "VP2MAA", "W7PP", "IK4LZH", "W7PP", "VP2MAA",
        };

    /// <summary>
    /// **The real service's own shapes, read on 2026-09-10, with no network.**
    /// </summary>
    private sealed class FakeLookup : ICallsignLookup
    {
        /// <summary>Every callsign it was asked about, in order.</summary>
        public List<string> Calls { get; } = new();

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

            Calls.Add(callsign);

            // Everything that is not this one US callsign answers INVALID, which is
            // what the live service does for a callsign it does not hold.
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
