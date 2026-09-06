using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 252, task 3: a callsign's entity where that is certain, and
/// nothing at all where it is not.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE TASK THE RULING BITES ON** (Tim, 2026-09-05: *say nothing
/// if we don't know*). A confident wrong country sends the operator after an
/// entity he already has, or past one he needs, and nothing on the screen tells
/// him it is wrong — which is §0.0's practical test failing in the one place this
/// unit could fail it.</para>
/// <para>**THE COMPOUND CASES ARE THE DANGEROUS ONES AND THEY ARE HIS OWN.**
/// `W4/YV7AXM` and `IS0/IK2YCW` are out of his log. Read the home call instead of
/// the prefix and they name Venezuela and Italy: right about the licence, wrong
/// about the contact, and wrong in the direction that costs him a new one.</para>
/// </remarks>
public sealed class TheCallsignCountryIsCertainOrSilentTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the resolutions are printed.</param>
    public TheCallsignCountryIsCertainOrSilentTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The two compound callsigns out of the operator's own log.</summary>
    /// <remarks>
    /// **THE PREFIX BEFORE THE SLASH WINS.** It says where the operator is; the
    /// call after it says who licensed them. For a contact the first is the
    /// answer.
    /// </remarks>
    [Theory]
    [InlineData("W4/YV7AXM", "United States of America")]
    [InlineData("IS0/IK2YCW", "Sardinia")]
    public void ACompoundCallIsWhereTheyAreAndNotWhereTheyAreFrom(
        string call, string entity)
    {
        var got = DxccPrefixes.EntityOf(call);

        _output.WriteLine(call + " -> " + (got ?? "(nothing)"));

        Assert.Equal(entity, got);
    }

    /// <summary>The home calls under those two, read on their own.</summary>
    /// <remarks>
    /// **THE CONTRAST IS THE WHOLE POINT.** These are the answers the compound
    /// forms must NOT give, so if the slash handling ever regresses, this pair and
    /// the pair above disagree and one of them fails.
    /// </remarks>
    [Theory]
    [InlineData("YV7AXM", "Venezuela")]
    [InlineData("IK2YCW", "Italy")]
    public void TheHomeCallOnItsOwnStillReadsAsItsOwnEntity(
        string call, string entity)
        => Assert.Equal(entity, DxccPrefixes.EntityOf(call));

    /// <summary>Sardinia is not Italy, which is the whole reason for DXCC.</summary>
    /// <remarks>
    /// **ITU APPENDIX 42 CANNOT DO THIS AND THAT IS WHY IT IS NOT THE SOURCE.**
    /// It allocates `I` to Italy as a country and has no row for Sardinia at all.
    /// The ARRL DXCC list splits them because an operator counting entities counts
    /// them separately.
    /// </remarks>
    [Fact]
    public void SardiniaIsNotItaly()
    {
        Assert.Equal("Sardinia", DxccPrefixes.EntityOf("IS0AAA"));
        Assert.Equal("Italy", DxccPrefixes.EntityOf("IK2YCW"));

        Assert.NotEqual(
            DxccPrefixes.EntityOf("IS0AAA"), DxccPrefixes.EntityOf("IK2YCW"));
    }

    /// <summary>The longest match wins, so specific entities are reachable.</summary>
    /// <remarks>
    /// **WITHOUT THIS EVERY SPECIFIC ENTITY IS UNREACHABLE.** `EA6` would read as
    /// Spain, `KL` as the United States, `IS0` as Italy — each of them a real
    /// entity swallowed by the shorter prefix that contains it.
    /// </remarks>
    [Theory]
    [InlineData("EA6ABC", "Balearic Is.")]
    [InlineData("EA8XYZ", "Canary Is.")]
    [InlineData("EA9AAA", "Ceuta & Melilla")]
    [InlineData("EA4AAA", "Spain")]
    [InlineData("KL7ABC", "Alaska")]
    [InlineData("AL7XYZ", "Alaska")]
    [InlineData("KH6ABC", "Hawaii")]
    [InlineData("W1ABC", "United States of America")]
    [InlineData("KP4AAA", "Puerto Rico")]
    public void TheLongestMatchWins(string call, string entity)
    {
        var got = DxccPrefixes.EntityOf(call);

        _output.WriteLine(call + " -> " + (got ?? "(nothing)"));

        Assert.Equal(entity, got);
    }

    /// <summary>A prefix two entities share resolves to nothing.</summary>
    /// <remarks>
    /// <para>**AND IT DOES NOT FALL BACK TO A SHORTER MATCH.** `VK9` is five
    /// different islands; answering *Australia* for a station the table has just
    /// said it cannot place is a guess arrived at by persistence, which is exactly
    /// what the ruling forbids.</para>
    /// <para>`3D2` is Fiji, Conway Reef and Rotuma. `CE0` is Easter Island, Juan
    /// Fernandez, and San Felix. `HK0` is Malpelo and San Andres. Fourteen
    /// prefixes are like this and every one of them is silent.</para>
    /// </remarks>
    [Theory]
    [InlineData("3D2AB")]
    [InlineData("VK9XYZ")]
    [InlineData("CE0ABC")]
    [InlineData("HK0AAA")]
    [InlineData("E5ABC")]
    [InlineData("JD1BCD")]
    public void APrefixTwoEntitiesShareResolvesToNothing(string call)
    {
        var got = DxccPrefixes.EntityOf(call);

        _output.WriteLine(call + " -> " + (got ?? "(nothing)"));

        Assert.Null(got);
    }

    /// <summary>A suffix says how somebody is operating, not where.</summary>
    [Theory]
    [InlineData("W1ABC/P", "United States of America")]
    [InlineData("W1ABC/QRP", "United States of America")]
    [InlineData("IK2YCW/M", "Italy")]
    public void ASuffixDoesNotMoveThem(string call, string entity)
        => Assert.Equal(entity, DxccPrefixes.EntityOf(call));

    /// <summary>Maritime and aeronautical mobile resolve to nothing.</summary>
    /// <remarks>
    /// **AT SEA IS NOT AN ENTITY.** Naming the one on the licence would be the
    /// confident wrong country in its purest form: the station is demonstrably not
    /// there, and it is the operator's own callsign that says so.
    /// </remarks>
    [Theory]
    [InlineData("W1ABC/MM")]
    [InlineData("IK2YCW/AM")]
    [InlineData("MM/W1ABC")]
    public void AtSeaOrInTheAirResolvesToNothing(string call)
        => Assert.Null(DxccPrefixes.EntityOf(call));

    /// <summary>Nothing this returns is ever a hedge.</summary>
    /// <remarks>
    /// **THE RULING IS A COUNTRY OR NO TOOLTIP AT ALL** — not *probably*, not
    /// *unknown*, not a confidence mark. This sweeps a wide spread of real and
    /// invented callsigns and fails on any output carrying a word of doubt, which
    /// is the guard against a later session softening the silence into a hedge.
    /// </remarks>
    [Fact]
    public void NoInputEverProducesAHedgedString()
    {
        string[] calls =
        {
            "W1ABC", "YV7AXM", "W4/YV7AXM", "IS0/IK2YCW", "3D2AB", "VK9XYZ",
            "ZZ9ZZZ", "QQQ", "1", "//", "K", "N0BAD", "G0ABC", "JA1XYZ",
            "VU2ABC", "9M2AAA", "T30AB", "CU3AA", "OX3XR", "E51AAA", "",
        };

        string[] hedges =
        {
            "probably", "possibly", "maybe", "unknown", "likely", "perhaps",
            "approximately", "about", "roughly", "uncertain", "guess", "?",
        };

        foreach (var call in calls)
        {
            var got = DxccPrefixes.EntityOf(call);

            _output.WriteLine("[" + call + "] -> " + (got ?? "(nothing)"));

            if (got is null)
            {
                continue;
            }

            Assert.NotEqual("", got.Trim());

            foreach (var hedge in hedges)
            {
                Assert.DoesNotContain(hedge, got, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <summary>The table says where it came from.</summary>
    /// <remarks>
    /// **A TABLE WITH NO SOURCE IS A TABLE TYPED FROM MEMORY** as far as any later
    /// reader can tell, and that is the thing this task forbids twice.
    /// </remarks>
    [Fact]
    public void TheTableNamesItsSource()
    {
        _output.WriteLine(DxccPrefixes.SourceLine);
        _output.WriteLine(
            DxccPrefixes.CertainPrefixCount + " certain prefixes, "
            + DxccPrefixes.SharedPrefixCount + " shared and therefore silent");

        Assert.Contains("ARRL", DxccPrefixes.SourceLine, StringComparison.Ordinal);
        Assert.Contains("2026", DxccPrefixes.SourceLine, StringComparison.Ordinal);

        Assert.True(DxccPrefixes.CertainPrefixCount > 500);
        Assert.True(DxccPrefixes.SharedPrefixCount > 0);
    }
}
