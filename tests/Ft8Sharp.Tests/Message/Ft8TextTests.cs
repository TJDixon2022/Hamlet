using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The character primitives, walked exhaustively rather than sampled.
/// </summary>
/// <remarks>
/// <para>
/// <b>What these prove: that each alphabet is a bijection between its indices and a set of ASCII
/// characters, and that every one of the 128 ASCII inputs has a defined answer in every alphabet.
/// What they do not prove: that the alphabet is upstream's.</b> A mapping in the wrong order
/// round-trips perfectly and is wholly wrong on the air. What corroborates the shape is
/// <c>UpstreamMessageProvenanceTests</c>, which reads the alphabet count and each length out of the
/// pin at run time; the pin holds no message-level known value to check the ordering against, so
/// the ordering is settled by step 3's bit-identical symbol comparison against the reference
/// implementation and by nothing before it.
/// </para>
/// <para>
/// Both alphabets are small enough to finish. There is no seed here and no sampling argument.
/// </para>
/// </remarks>
public class Ft8TextTests
{
    private readonly ITestOutputHelper _output;

    public Ft8TextTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void EveryCodePointInEveryAlphabetRoundTrips()
    {
        var total = 0;

        foreach (var table in Enum.GetValues<Ft8CharTable>())
        {
            var length = Ft8Text.Length(table);
            Assert.True(length > 0, $"{table} has no length, so nothing was walked for it.");

            for (var index = 0; index < length; index++)
            {
                var c = Ft8Text.Character(index, table);
                Assert.True(
                    c != Ft8Text.Unknown,
                    $"{table} index {index} is inside the alphabet and produced the unknown-character "
                    + "sentinel, so either the length or the branching is wrong.");

                Assert.Equal(index, Ft8Text.Index(c, table));
                total++;
            }

            // A bijection needs the other direction too: no two indices may give the same character.
            var distinct = new HashSet<char>();
            for (var index = 0; index < length; index++)
            {
                Assert.True(
                    distinct.Add(Ft8Text.Character(index, table)),
                    $"{table} produces the same character for two different indices, so it is not a "
                    + "bijection and a decode from it could not be trusted.");
            }

            _output.WriteLine($"{table,-24} {length,3} code points, all round-tripped, all distinct");
        }

        _output.WriteLine($"total code points walked : {total}");
        Assert.Equal(190, total);
    }

    [Fact]
    public void EveryAsciiInputHasADefinedAnswerInEveryAlphabet()
    {
        foreach (var table in Enum.GetValues<Ft8CharTable>())
        {
            var accepted = 0;
            var rejected = 0;

            for (var code = 0; code < 128; code++)
            {
                var c = (char)code;
                var index = Ft8Text.Index(c, table);

                if (index == Ft8Text.NotFound)
                {
                    rejected++;
                    continue;
                }

                accepted++;
                Assert.InRange(index, 0, Ft8Text.Length(table) - 1);

                // Accepted means it is really in the alphabet, not merely that a number came back.
                Assert.Equal(c, Ft8Text.Character(index, table));
            }

            _output.WriteLine($"{table,-24} accepted {accepted,3}, cleanly rejected {rejected,3}");
            Assert.Equal(128, accepted + rejected);
            Assert.Equal(Ft8Text.Length(table), accepted);
        }
    }

    /// <summary>
    /// Indices outside an alphabet answer with the sentinel rather than throwing, over a range far
    /// wider than any caller could reach.
    /// </summary>
    [Fact]
    public void AnIndexOutsideAnAlphabetIsRefusedRatherThanThrowing()
    {
        foreach (var table in Enum.GetValues<Ft8CharTable>())
        {
            for (var index = -600; index < 600; index++)
            {
                var c = Ft8Text.Character(index, table);
                if (index >= 0 && index < Ft8Text.Length(table))
                {
                    Assert.NotEqual(Ft8Text.Unknown, c);
                }
                else
                {
                    Assert.Equal(Ft8Text.Unknown, c);
                }
            }
        }

        // An alphabet this library does not declare is refused in both directions.
        var undeclared = (Ft8CharTable)99;
        Assert.False(Ft8Text.IsDefined(undeclared));
        Assert.Equal(Ft8Text.Unknown, Ft8Text.Character(0, undeclared));
        Assert.Equal(Ft8Text.NotFound, Ft8Text.Index('A', undeclared));
    }

    /// <summary>
    /// The fixed-width integer formatting the report field is written with, at the widths and
    /// signs the message layer actually asks for.
    /// </summary>
    [Fact]
    public void FixedWidthIntegersFormatAndParseBackAcrossTheReportRange()
    {
        for (var value = -99; value <= 99; value++)
        {
            var text = Ft8Text.IntToDd(value, 2, true);
            Assert.Equal(3, text.Length);
            Assert.Equal(value, Ft8Text.DdToInt(text, 3));
        }

        for (var value = 0; value <= 999; value++)
        {
            var text = Ft8Text.IntToDd(value, 3, false);
            Assert.Equal(3, text.Length);
            Assert.Equal(value, Ft8Text.DdToInt(text, 3));
        }
    }

    /// <summary>
    /// The character predicates, over the whole of ASCII, so that a callsign's shape test is not
    /// resting on a culture-aware framework method that would answer differently.
    /// </summary>
    [Fact]
    public void TheCharacterPredicatesAreAsciiOnly()
    {
        for (var code = 0; code < 128; code++)
        {
            var c = (char)code;
            Assert.Equal(c is >= '0' and <= '9', Ft8Text.IsDigit(c));
            Assert.Equal(c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'), Ft8Text.IsLetter(c));
            Assert.Equal(c == ' ', Ft8Text.IsSpace(c));
        }

        // Not a tab and not a newline: upstream's is_space is a space and nothing else, and a
        // framework method that said otherwise would change how a message is tokenised.
        Assert.False(Ft8Text.IsSpace('\t'));
        Assert.False(Ft8Text.IsSpace('\n'));

        // Upcasing is ASCII-only, so characters outside it pass through untouched.
        Assert.Equal('A', Ft8Text.ToUpper('a'));
        Assert.Equal('A', Ft8Text.ToUpper('A'));
        Assert.Equal('é', Ft8Text.ToUpper('é'));
    }
}
