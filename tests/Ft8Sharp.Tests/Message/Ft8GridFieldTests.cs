using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The grid-and-report field, swept exhaustively: all 32 768 values, not a sample of them.
/// </summary>
/// <remarks>
/// <para>
/// <b>There is no seed here and no sampling argument to make.</b> The field is fifteen bits wide
/// and the whole of it fits in a test that returns in milliseconds, so every value is asserted
/// rather than a share of them. That makes this a real proof over a whole field — which is a
/// different and stronger thing than the corpus tests elsewhere in this project, and it is still
/// a proof of <em>self-consistency</em> rather than of agreement with upstream.
/// </para>
/// <para>
/// <b>The contract: every value either unpacks to text that re-packs to the same bits, or is
/// refused.</b> Nothing falls through, nothing throws, and the two counts sum to the size of the
/// field.
/// </para>
/// </remarks>
public class Ft8GridFieldTests
{
    private readonly ITestOutputHelper _output;

    public Ft8GridFieldTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void EveryValueOfTheFieldEitherRoundTripsOrIsRefused()
    {
        var roundTripped = 0;
        var refused = 0;
        var grids = 0;
        var reports = 0;
        var tokens = 0;
        var empty = 0;
        var mismatches = new List<string>();

        for (var value = 0; value < Ft8GridField.Range; value++)
        {
            var result = Ft8GridField.TryUnpack(value, false, out var text, out var fieldType);

            if (result != Ft8FieldResult.Ok)
            {
                refused++;
                Assert.Equal(Ft8FieldResult.Malformed, result);
                continue;
            }

            var repacked = Ft8GridField.Pack(text);
            if (repacked != value)
            {
                mismatches.Add($"{value} -> [{text}] -> {repacked}");
                continue;
            }

            roundTripped++;
            switch (fieldType)
            {
                case Ft8FieldType.Grid: grids++; break;
                case Ft8FieldType.Report: reports++; break;
                case Ft8FieldType.Token: tokens++; break;
                case Ft8FieldType.None: empty++; break;
                default: Assert.Fail($"value {value} decoded with no field type."); break;
            }
        }

        _output.WriteLine($"field size          : {Ft8GridField.Range}");
        _output.WriteLine($"round-tripped       : {roundTripped}");
        _output.WriteLine($"    grid squares    : {grids}");
        _output.WriteLine($"    signal reports  : {reports}");
        _output.WriteLine($"    fixed tokens    : {tokens}");
        _output.WriteLine($"    no third field  : {empty}");
        _output.WriteLine($"refused             : {refused}");
        _output.WriteLine($"sum                 : {roundTripped + refused}");
        _output.WriteLine($"neither             : {mismatches.Count}");
        foreach (var mismatch in mismatches)
        {
            _output.WriteLine($"    {mismatch}");
        }

        Assert.Empty(mismatches);
        Assert.Equal(Ft8GridField.Range, roundTripped + refused);
    }

    /// <summary>
    /// The same sweep with the <c>R</c> flag set, where the property is weaker and the report says
    /// why.
    /// </summary>
    /// <remarks>
    /// <b>Upstream's packer has no route to an <c>R</c>-prefixed grid square and its own comment
    /// says so, while its unpacker will produce one.</b> That asymmetry is upstream's and is not
    /// repaired here — repairing it would change what goes on the air. So under the flag the
    /// assertion is the absolute one this project can still make: every value has a defined answer
    /// and none of them throws. The count that re-packs and the count that does not are both
    /// reported.
    /// </remarks>
    [Fact]
    public void EveryValueUnderTheReportFlagHasADefinedAnswer()
    {
        var repacked = 0;
        var decodedButNotRepacked = 0;
        var refused = 0;

        for (var value = 0; value < Ft8GridField.Range; value++)
        {
            var result = Ft8GridField.TryUnpack(value, true, out var text, out _);

            if (result != Ft8FieldResult.Ok)
            {
                refused++;
                continue;
            }

            if (Ft8GridField.Pack(text) == (value | Ft8GridField.ReportFlag))
            {
                repacked++;
            }
            else
            {
                decodedButNotRepacked++;
            }
        }

        _output.WriteLine($"with the R flag set — decoded and re-packing : {repacked}");
        _output.WriteLine($"                      decoded, not re-packing: {decodedButNotRepacked}");
        _output.WriteLine($"                      refused                : {refused}");
        _output.WriteLine(
            "The ones that do not re-pack are the grid squares and the three fixed tokens: upstream's "
            + "packer has no route that sets this flag alongside them, and its own comment says so. "
            + "Not repaired here, because repairing it would change the wire format.");

        Assert.Equal(Ft8GridField.Range, repacked + decodedButNotRepacked + refused);
        Assert.True(repacked > 0, "nothing round-tripped under the flag, so the report path is broken.");
    }

    /// <summary>
    /// The two refusals this port makes where upstream returns text, asserted directly so that a
    /// later session cannot quietly turn them back into decodes.
    /// </summary>
    [Fact]
    public void TheTwoDeliberateRefusalsAreWhereTheyAreSaidToBe()
    {
        // The boundary value both sub-ranges claim.
        Assert.Equal(
            Ft8FieldResult.Malformed,
            Ft8GridField.TryUnpack(Ft8GridField.MaxGrid, false, out _, out _));

        // Either side of it decodes.
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(Ft8GridField.MaxGrid - 1, false, out _, out _));
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(Ft8GridField.MaxGrid + Ft8GridField.CodeNone, false, out _, out _));

        // The last report whose number fits two digits decodes; the first that does not is refused.
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(
                Ft8GridField.MaxGrid + Ft8GridField.LastReportCode, false, out var last, out var type));
        Assert.Equal(Ft8FieldType.Report, type);
        Assert.Equal(3, last.Length);

        Assert.Equal(
            Ft8FieldResult.Malformed,
            Ft8GridField.TryUnpack(
                Ft8GridField.MaxGrid + Ft8GridField.LastReportCode + 1, false, out _, out _));

        // The grid square whose name a token has taken, and its immediate neighbours which do not
        // collide and must still decode.
        Assert.Equal(
            Ft8FieldResult.Malformed,
            Ft8GridField.TryUnpack(Ft8GridField.GridClaimedByAToken, false, out _, out _));
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(Ft8GridField.GridClaimedByAToken - 1, false, out _, out _));
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(Ft8GridField.GridClaimedByAToken + 1, false, out _, out _));

        // And the token that took it still decodes, from its own sub-range.
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(Ft8GridField.MaxGrid + Ft8GridField.CodeRr73, false, out var token, out _));
        Assert.Equal("RR73", token);
    }

    /// <summary>The four fixed tokens and the empty field, by name.</summary>
    [Fact]
    public void TheFixedTokensAreWhatTheySay()
    {
        Assert.Equal(Ft8FieldResult.Ok, Ft8GridField.TryUnpack(Ft8GridField.MaxGrid + Ft8GridField.CodeNone, false, out var none, out var noneType));
        Assert.Equal(string.Empty, none);
        Assert.Equal(Ft8FieldType.None, noneType);

        foreach (var (code, expected) in new[]
                 {
                     (Ft8GridField.CodeRrr, "RRR"),
                     (Ft8GridField.CodeRr73, "RR73"),
                     (Ft8GridField.CodeSeventyThree, "73"),
                 })
        {
            Assert.Equal(
                Ft8FieldResult.Ok,
                Ft8GridField.TryUnpack(Ft8GridField.MaxGrid + code, false, out var text, out var type));
            Assert.Equal(expected, text);
            Assert.Equal(Ft8FieldType.Token, type);
            Assert.Equal(Ft8GridField.MaxGrid + code, Ft8GridField.Pack(text));
        }
    }

    /// <summary>
    /// The corners of the grid sub-range, by name, so that an off-by-one in the eighteen-by-eighteen
    /// arithmetic shows up as a named failure rather than as a count that is one out.
    /// </summary>
    [Fact]
    public void TheCornersOfTheGridSubRangeAreWhereTheyBelong()
    {
        Assert.Equal(0, Ft8GridField.Pack("AA00"));
        Assert.Equal(Ft8GridField.MaxGrid - 1, Ft8GridField.Pack("RR99"));

        Assert.Equal(Ft8FieldResult.Ok, Ft8GridField.TryUnpack(0, false, out var first, out _));
        Assert.Equal("AA00", first);

        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8GridField.TryUnpack(Ft8GridField.MaxGrid - 1, false, out var last, out _));
        Assert.Equal("RR99", last);
    }

    /// <summary>Packing arbitrary text never throws, whatever is handed to it.</summary>
    [Fact]
    public void PackingArbitraryTextNeverThrows()
    {
        var random = new Random(20260901);
        for (var i = 0; i < 100_000; i++)
        {
            var length = random.Next(0, 12);
            var chars = new char[length];
            for (var j = 0; j < length; j++)
            {
                chars[j] = (char)random.Next(0, 128);
            }

            var packed = Ft8GridField.Pack(new string(chars));
            Assert.InRange(packed, 0, 0xFFFF);
        }

        Assert.Equal(Ft8GridField.MaxGrid + Ft8GridField.CodeNone, Ft8GridField.Pack(null));
        Assert.Equal(Ft8GridField.MaxGrid + Ft8GridField.CodeNone, Ft8GridField.Pack(string.Empty));
    }
}
