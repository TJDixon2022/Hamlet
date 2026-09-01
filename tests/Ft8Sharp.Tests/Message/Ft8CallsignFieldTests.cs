using System.Text;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The 28-bit callsign field: a seeded corpus across every shape the field admits, every special
/// token by name, every sub-range boundary and the value on each side of it, and the assertion
/// that the hashed region refuses rather than guesses.
/// </summary>
/// <remarks>
/// <para>
/// <b>Sampled, not exhausted, and the reason is the clock.</b> A full sweep is 268 435 456
/// round-trips. The default corpus is a million, generated systematically across the shapes rather
/// than drawn uniformly, and the full sweep is available behind <c>FT8_CALLSIGN_FULL_SWEEP=1</c>.
/// A fast inner loop is worth more to this phase than a bigger sample.
/// </para>
/// <para>
/// <b>What a round-trip proves.</b> That the packer and the unpacker are inverses over the corpus.
/// It does not prove that the integers agree with the reference implementation's — a field packed
/// in the wrong order round-trips perfectly and is wholly wrong on the air. The boundaries are
/// corroborated against the pin by machine in <see cref="UpstreamMessageProvenanceTests"/>; the
/// arithmetic between them is settled by step 3.
/// </para>
/// </remarks>
public class Ft8CallsignFieldTests
{
    private const int Seed = 20260901;
    private const int CorpusSize = 1_000_000;

    private readonly ITestOutputHelper _output;

    public Ft8CallsignFieldTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void AMillionGeneratedCallsignsRoundTripThroughTheField()
    {
        var random = new Random(Seed);
        var roundTripped = 0;
        var refused = 0;
        var collisions = new List<string>();
        var unexplained = new List<string>();
        var byShape = new int[ShapeCount];

        for (var i = 0; i < CorpusSize; i++)
        {
            // Systematic across the shapes, seeded within each: every shape gets an equal share
            // rather than whatever a uniform draw happens to produce.
            var shape = i % ShapeCount;
            var callsign = GenerateCallsign(random, shape, out var suffixed, out var messageType);

            var packResult = Ft8CallsignField.TryPack(callsign, out var value, out var suffix);
            if (packResult != Ft8FieldResult.Ok)
            {
                refused++;
                continue;
            }

            Assert.Equal(suffixed, suffix);
            Assert.True(value < Ft8CallsignField.Range, $"[{callsign}] packed outside the field.");

            var unpackResult = Ft8CallsignField.TryUnpack(
                value, suffix, messageType, out var text, out var fieldType);

            Assert.Equal(Ft8FieldResult.Ok, unpackResult);
            Assert.Equal(Ft8FieldType.Callsign, fieldType);

            if (text == callsign)
            {
                roundTripped++;
                byShape[shape]++;
                continue;
            }

            if (IsPrefixWorkaroundCollision(callsign))
            {
                collisions.Add($"[{callsign}] -> [{text}]");
            }
            else
            {
                unexplained.Add($"[{callsign}] -> [{text}]");
            }
        }

        _output.WriteLine($"corpus size            : {CorpusSize}   seed: {Seed}");
        _output.WriteLine($"round-tripped          : {roundTripped}");
        for (var shape = 0; shape < ShapeCount; shape++)
        {
            _output.WriteLine($"    shape {shape}            : {byShape[shape]}");
        }

        _output.WriteLine($"refused at pack        : {refused}");
        _output.WriteLine($"prefix collisions      : {collisions.Count} (upstream's own, see below)");
        _output.WriteLine($"unexplained mismatches : {unexplained.Count}");
        foreach (var mismatch in unexplained.Take(20))
        {
            _output.WriteLine($"    {mismatch}");
        }

        _output.WriteLine(
            "A prefix collision is a callsign that is spelled the way one of upstream's two prefix "
            + "work-arounds spells its compressed form, so it packs to the same integer as the "
            + "callsign that work-around is for and unpacks to that one. It is upstream's, it is on "
            + "the air, and it is reported rather than repaired.");

        Assert.Empty(unexplained);
        Assert.Equal(0, refused);
        Assert.True(
            roundTripped > CorpusSize * 0.9,
            $"only {roundTripped} of {CorpusSize} round-tripped, which is too few for the rest of "
            + "this to mean anything.");
    }

    /// <summary>Every special token the field reserves, by name.</summary>
    [Fact]
    public void EverySpecialTokenRoundTripsByName()
    {
        foreach (var token in new[] { "DE", "QRZ", "CQ" })
        {
            Assert.Equal(Ft8FieldResult.Ok, Ft8CallsignField.TryPack(token, out var value, out var suffix));
            Assert.False(suffix);
            Assert.Equal(
                Ft8FieldResult.Ok,
                Ft8CallsignField.TryUnpack(value, false, 1, out var text, out var type));
            Assert.Equal(token, text);
            Assert.Equal(Ft8FieldType.Token, type);
            _output.WriteLine($"{token,-10} round-trips as a token");
        }

        // The numeric CQ family, all thousand of it.
        for (var n = 0; n < 1000; n++)
        {
            var token = "CQ " + n.ToString("000");
            Assert.Equal(Ft8FieldResult.Ok, Ft8CallsignField.TryPack(token, out var value, out _));
            Assert.Equal(Ft8CallsignField.FirstNumericCq + (uint)n, value);
            Assert.Equal(
                Ft8FieldResult.Ok,
                Ft8CallsignField.TryUnpack(value, false, 1, out var text, out var type));
            Assert.Equal(token, text);
            Assert.Equal(Ft8FieldType.TokenWithArgument, type);
        }

        _output.WriteLine("CQ nnn     round-trips for all 1000 values");

        // The lettered CQ family, over its whole range, which is small enough to finish.
        //
        // It does not round-trip in full, and that is upstream's asymmetry rather than this port's.
        // The unpacker trims only the leading spaces off the four-symbol modifier, so a modifier
        // with a space anywhere after them keeps it; upstream's parser then stops at that space and
        // reads a shorter modifier. Every one of those is counted and every one is required to be
        // of exactly that shape — nothing else may fail to round-trip.
        var lettered = 0;
        var roundTripped = 0;
        var spaceInModifier = 0;

        for (var n = Ft8CallsignField.FirstLetteredCq; n <= Ft8CallsignField.LastLetteredCq; n++)
        {
            Assert.Equal(
                Ft8FieldResult.Ok,
                Ft8CallsignField.TryUnpack(n, false, 1, out var text, out var type));
            Assert.Equal(Ft8FieldType.TokenWithArgument, type);
            Assert.Equal(Ft8FieldResult.Ok, Ft8CallsignField.TryPack(text, out var back, out _));
            lettered++;

            if (back == n)
            {
                roundTripped++;
                continue;
            }

            Assert.True(
                text["CQ ".Length..].Contains(' '),
                $"the modifier at {n} decoded to [{text}] and packed back to {back}, and it is not "
                + "the space-in-the-modifier shape. That is a defect in this port, not upstream's.");
            spaceInModifier++;
        }

        _output.WriteLine($"CQ abcd    {lettered} values, all decoded");
        _output.WriteLine($"           {roundTripped} round-trip");
        _output.WriteLine($"           {spaceInModifier} carry a space in the modifier and do not, which is upstream's");
        Assert.Equal(531441, lettered);
        Assert.Equal(lettered, roundTripped + spaceInModifier);
    }

    /// <summary>
    /// Every boundary the pin declares, and the value on each side of it.
    /// </summary>
    [Fact]
    public void EverySubRangeBoundaryAndItsNeighboursBehaveAsDeclared()
    {
        var boundaries = new (string Name, uint Value)[]
        {
            ("first token", 0),
            ("last bare token", Ft8CallsignField.TokenCq),
            ("first CQ nnn", Ft8CallsignField.FirstNumericCq),
            ("last CQ nnn", Ft8CallsignField.LastNumericCq),
            ("first CQ abcd", Ft8CallsignField.FirstLetteredCq),
            ("last CQ abcd", Ft8CallsignField.LastLetteredCq),
            ("last defined token", Ft8CallsignField.LastDefinedToken),
            ("end of token range", Ft8CallsignField.TokenRangeSize),
            ("end of hash range", Ft8CallsignField.BasecallBase),
            ("top of the field", Ft8CallsignField.Range - 1),
        };

        foreach (var (name, value) in boundaries)
        {
            foreach (var offset in new[] { -1, 0, 1 })
            {
                var at = (long)value + offset;
                if (at < 0 || at >= Ft8CallsignField.Range)
                {
                    continue;
                }

                var result = Ft8CallsignField.TryUnpack((uint)at, false, 1, out var text, out _);

                // The one thing asserted for every one of them: a defined answer and no exception.
                Assert.True(
                    result is Ft8FieldResult.Ok or Ft8FieldResult.Malformed
                        or Ft8FieldResult.UnresolvedCallsign,
                    $"{name}{offset:+0;-0;+0} produced no defined answer.");

                _output.WriteLine(
                    $"{name,-22}{offset,+3} : {result,-20} {(result == Ft8FieldResult.Ok ? $"[{text}]" : string.Empty)}");
            }
        }

        // And the specific behaviour each boundary is a boundary of.
        Assert.Equal(
            Ft8FieldResult.Malformed,
            Ft8CallsignField.TryUnpack(Ft8CallsignField.LastDefinedToken + 1, false, 1, out _, out _));
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8CallsignField.TryUnpack(Ft8CallsignField.LastDefinedToken, false, 1, out _, out _));
        Assert.Equal(
            Ft8FieldResult.UnresolvedCallsign,
            Ft8CallsignField.TryUnpack(Ft8CallsignField.TokenRangeSize, false, 1, out _, out _));
        Assert.Equal(
            Ft8FieldResult.UnresolvedCallsign,
            Ft8CallsignField.TryUnpack(Ft8CallsignField.BasecallBase - 1, false, 1, out _, out _));
        Assert.Equal(
            Ft8FieldResult.Ok,
            Ft8CallsignField.TryUnpack(Ft8CallsignField.Range - 1, false, 1, out _, out _));
    }

    /// <summary>
    /// The seam: the hashed-callsign region refuses rather than guessing, in both directions.
    /// </summary>
    /// <remarks>
    /// <b>This is HM-DEC-009 in practice and it is the assertion this whole unit turns on.</b> The
    /// hash region holds a number that stands for a callsign held in a rolling cache this library
    /// does not have. Refusing means no placeholder text, no partial message, and no numeric field
    /// dressed as a call. Upstream, with no cache attached, writes a literal placeholder and
    /// reports success; that is the divergence, and it is deliberate.
    /// </remarks>
    [Fact]
    public void TheHashedCallsignRegionIsRefusedAndNeverGuessedAt()
    {
        var random = new Random(Seed);
        var checkedValues = 0;

        // Both ends, the middle, and a seeded sample across the whole 22-bit region.
        var samples = new List<uint>
        {
            Ft8CallsignField.TokenRangeSize,
            Ft8CallsignField.TokenRangeSize + 1,
            Ft8CallsignField.TokenRangeSize + (Ft8CallsignField.HashRangeSize / 2),
            Ft8CallsignField.BasecallBase - 2,
            Ft8CallsignField.BasecallBase - 1,
        };

        for (var i = 0; i < 200_000; i++)
        {
            samples.Add(Ft8CallsignField.TokenRangeSize
                + (uint)random.Next(0, (int)Ft8CallsignField.HashRangeSize));
        }

        foreach (var value in samples)
        {
            foreach (var suffix in new[] { false, true })
            {
                var result = Ft8CallsignField.TryUnpack(value, suffix, 1, out var text, out var type);
                Assert.Equal(Ft8FieldResult.UnresolvedCallsign, result);
                Assert.Equal(string.Empty, text);
                Assert.Equal(Ft8FieldType.Unknown, type);
                checkedValues++;
            }
        }

        _output.WriteLine($"hashed-region values checked : {checkedValues}");
        _output.WriteLine("every one refused as unresolved, with no text written");

        // And the packing side of the same seam: a non-standard callsign is refused rather than
        // written as a value that could not be read back.
        foreach (var call in new[] { "EA8/G5LSI", "YL/LB2JK", "PJ4/KA1ABC", "K1ABC/QRP" })
        {
            var result = Ft8CallsignField.TryPack(call, out var value, out _);
            Assert.Equal(Ft8FieldResult.RequiresHashCache, result);
            Assert.Equal(0u, value);
            _output.WriteLine($"{call,-14} refused at pack as requiring the hash cache");
        }
    }

    /// <summary>
    /// The suffix bit is only read under a message type that says what it means, and is refused
    /// under any other.
    /// </summary>
    [Fact]
    public void TheSuffixBitIsRefusedUnderATypeThatDoesNotDefineIt()
    {
        Assert.Equal(Ft8FieldResult.Ok, Ft8CallsignField.TryPack("K1ABC", out var value, out _));

        Assert.Equal(Ft8FieldResult.Ok, Ft8CallsignField.TryUnpack(value, true, 1, out var slashR, out _));
        Assert.Equal("K1ABC/R", slashR);

        Assert.Equal(Ft8FieldResult.Ok, Ft8CallsignField.TryUnpack(value, true, 2, out var slashP, out _));
        Assert.Equal("K1ABC/P", slashP);

        foreach (var type in new[] { 0, 3, 4, 5, 6, 7 })
        {
            Assert.Equal(
                Ft8FieldResult.Malformed,
                Ft8CallsignField.TryUnpack(value, true, type, out _, out _));
        }
    }

    /// <summary>
    /// Unpacking never throws for any value the field can hold, and packing never throws for any
    /// string.
    /// </summary>
    [Fact]
    public void NeitherDirectionThrowsForAnythingItIsHanded()
    {
        var random = new Random(Seed);

        for (var i = 0; i < 500_000; i++)
        {
            var value = (uint)random.Next(0, (int)Ft8CallsignField.Range);
            var type = random.Next(0, 8);
            Ft8CallsignField.TryUnpack(value, random.Next(2) == 0, type, out _, out _);
        }

        for (var i = 0; i < 100_000; i++)
        {
            var length = random.Next(0, 16);
            var text = new StringBuilder(length);
            for (var j = 0; j < length; j++)
            {
                text.Append((char)random.Next(0, 128));
            }

            var result = Ft8CallsignField.TryPack(text.ToString(), out _, out _);
            Assert.True(Enum.IsDefined(result));
        }

        // Including values above the field, which a caller should not produce and must not crash on.
        for (var i = 0; i < 1000; i++)
        {
            Assert.Equal(
                Ft8FieldResult.Malformed,
                Ft8CallsignField.TryUnpack(Ft8CallsignField.Range + (uint)i, false, 1, out _, out _));
        }
    }

    /// <summary>
    /// The whole 28-bit field, swept. Off by default; it is 268 million round-trips and the fast
    /// inner loop is worth more than the coverage on an ordinary run.
    /// </summary>
    [Fact]
    public void TheFullTwentyEightBitSweep()
    {
        if (Environment.GetEnvironmentVariable("FT8_CALLSIGN_FULL_SWEEP") != "1")
        {
            _output.WriteLine(
                "Not asked. Run: dotnet test tests/Ft8Sharp.Tests -e FT8_CALLSIGN_FULL_SWEEP=1 "
                + "--filter FullyQualifiedName~TheFullTwentyEightBitSweep");
            return;
        }

        var started = DateTime.UtcNow;
        var decoded = 0L;
        var unresolved = 0L;
        var malformed = 0L;
        var notRepacking = 0L;

        for (uint value = 0; value < Ft8CallsignField.Range; value++)
        {
            var result = Ft8CallsignField.TryUnpack(value, false, 1, out var text, out _);
            switch (result)
            {
                case Ft8FieldResult.Ok:
                    decoded++;
                    if (Ft8CallsignField.TryPack(text, out var back, out _) != Ft8FieldResult.Ok
                        || back != value)
                    {
                        notRepacking++;
                    }

                    break;

                case Ft8FieldResult.UnresolvedCallsign:
                    unresolved++;
                    break;

                default:
                    malformed++;
                    break;
            }
        }

        var elapsed = DateTime.UtcNow - started;
        _output.WriteLine($"full sweep of {Ft8CallsignField.Range} values in {elapsed.TotalSeconds:F1} s");
        _output.WriteLine($"    decoded                  : {decoded}");
        _output.WriteLine($"        of which not re-packing : {notRepacking}");
        _output.WriteLine($"    unresolved (hash region) : {unresolved}");
        _output.WriteLine($"    malformed                : {malformed}");

        Assert.Equal(Ft8CallsignField.Range, (uint)(decoded + unresolved + malformed));
        Assert.Equal(Ft8CallsignField.HashRangeSize, (uint)unresolved);
    }

    /// <summary>
    /// The shapes the basecall admits, as the pin's own branching defines them.
    /// </summary>
    /// <remarks>
    /// Four routes into the six packing positions: a call whose area digit is third, a call whose
    /// area digit is second, and the two prefix work-arounds that let a seven-character call from
    /// Swaziland or Guinea fit. Each is generated with and without a suffix.
    /// </remarks>
    private const int ShapeCount = 8;

    private static string GenerateCallsign(Random random, int shape, out bool suffixed, out int messageType)
    {
        var withSuffix = shape >= 4;
        var text = new StringBuilder();

        switch (shape % 4)
        {
            case 0:
                // Area digit third: two leading alphanumerics, a digit, then up to three letters.
                text.Append(Alphanumeric(random));
                text.Append(Alphanumeric(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(0, 4));
                break;

            case 1:
                // Area digit second: one alphanumeric, a digit, then one to three letters. The
                // shortest of these is three characters, which is the shortest the field admits.
                text.Append(Alphanumeric(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(1, 4));
                break;

            case 2:
                // The Swaziland prefix work-around.
                text.Append("3DA0");
                AppendLetters(text, random, random.Next(1, 4));
                break;

            default:
                // The Guinea prefix work-around.
                text.Append("3X");
                text.Append(Letter(random));
                text.Append(Digit(random));
                AppendLetters(text, random, random.Next(0, 3));
                break;
        }

        suffixed = withSuffix;
        messageType = 1;

        if (!withSuffix)
        {
            return text.ToString();
        }

        // Half the suffixed calls carry /R under type 1, half carry /P under type 2.
        if (random.Next(2) == 0)
        {
            text.Append("/R");
        }
        else
        {
            text.Append("/P");
            messageType = 2;
        }

        return text.ToString();
    }

    private static char Alphanumeric(Random random)
    {
        var n = random.Next(36);
        return n < 10 ? (char)('0' + n) : (char)('A' + n - 10);
    }

    private static char Digit(Random random) => (char)('0' + random.Next(10));

    private static char Letter(Random random) => (char)('A' + random.Next(26));

    private static void AppendLetters(StringBuilder text, Random random, int count)
    {
        for (var i = 0; i < count; i++)
        {
            text.Append(Letter(random));
        }
    }

    /// <summary>
    /// Whether a callsign is spelled the way one of upstream's prefix work-arounds spells its own
    /// compressed form.
    /// </summary>
    /// <remarks>
    /// <b>These are real collisions in the wire format, not defects in this port.</b> The Swaziland
    /// work-around writes <c>3DA0XYZ</c> into the six positions as <c>3D0XYZ</c>, so a callsign
    /// actually spelled <c>3D0XYZ</c> packs to the same integer and unpacks to the Swaziland one.
    /// The Guinea work-around does the same with a leading <c>Q</c>. Reported, not repaired:
    /// repairing them would change what goes on the air.
    /// </remarks>
    private static bool IsPrefixWorkaroundCollision(string callsign) =>
        callsign.StartsWith("3D0", StringComparison.Ordinal)
        || (callsign.Length >= 2 && callsign[0] == 'Q' && Ft8Text.IsLetter(callsign[1]));
}
