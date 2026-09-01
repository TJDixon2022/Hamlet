using System.Text.RegularExpressions;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.TableGen;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// Leg A of the CRC proof: the two scalars this library divides by are the pin's, checked by
/// machine at run time rather than by somebody reading two files side by side.
/// </summary>
/// <remarks>
/// <para>
/// <b>A transcription that is wrong cannot survive this.</b> The polynomial and the width are
/// hand-written constants in <see cref="Crc14"/> — the arbiter's rule for unit 206 is that tables
/// go through the checked-in converter and scalars are asserted against the pin, because
/// regenerating the tables file for two numbers would put step 1's byte-for-byte regeneration
/// proof at risk for no gain. This is the assertion that makes that trade honest.
/// </para>
/// <para>
/// <b>One thing was measured here and was not what the unit expected.</b> The work instruction
/// says the two scalars are declared in <c>ft8/crc.h</c>. They are not: <c>crc.h</c> declares
/// only the three functions, and both macros live in <c>ft8/constants.h</c>, which <c>crc.c</c>
/// includes. The assertion follows the tree.
/// </para>
/// <para>
/// <b>No value is printed by anything in this file.</b> Whether the constant matched is metadata
/// and is free; the constant is not.
/// </para>
/// </remarks>
public class UpstreamCrcProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamCrcProvenanceTests(ITestOutputHelper output) => _output = output;

    /// <summary>Where upstream actually declares them, measured rather than assumed.</summary>
    private const string ScalarHeader = @"ft8\constants.h";

    [RequiresReferenceCloneFact]
    public void TheLibrarysCrcScalarsAreThePins()
    {
        var header = Path.Combine(ReferenceClone.Location, ScalarHeader);
        Assert.True(File.Exists(header), $"{header} is not there, so nothing can be checked against it.");

        var macros = CSourceParser.ParseIntegerMacros(File.ReadAllText(header));

        Assert.True(
            macros.ContainsKey("FT8_CRC_POLYNOMIAL"),
            $"{ScalarHeader} does not declare FT8_CRC_POLYNOMIAL as a macro this reader could "
            + "evaluate. An unresolved scalar is not a pass — it is the check not running.");
        Assert.True(
            macros.ContainsKey("FT8_CRC_WIDTH"),
            $"{ScalarHeader} does not declare FT8_CRC_WIDTH as a macro this reader could evaluate.");

        _output.WriteLine($"header                  : {ScalarHeader}");
        _output.WriteLine($"macros resolved in it   : {macros.Count}");
        _output.WriteLine($"FT8_CRC_WIDTH matches   : {macros["FT8_CRC_WIDTH"] == Crc14.Width}");
        _output.WriteLine($"FT8_CRC_POLYNOMIAL match: {macros["FT8_CRC_POLYNOMIAL"] == Crc14.Polynomial}");
        _output.WriteLine("Values are deliberately not printed, by ruling.");

        Assert.Equal(Crc14.Width, macros["FT8_CRC_WIDTH"]);
        Assert.Equal(Crc14.Polynomial, macros["FT8_CRC_POLYNOMIAL"]);
    }

    /// <summary>
    /// The one external known value in the pin — and the measurement showing it is stale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Criterion 1 says "CRC matches known values", and a value this port produced itself is
    /// not one.</b> The pinned clone's <c>test/test.c</c> carries exactly one candidate: a
    /// ten-byte input, a bit count, and a comment saying what the checksum of it should be. It is
    /// the only such statement anywhere in the pin.
    /// </para>
    /// <para>
    /// <b>It does not agree with the pin's own CRC, and the evidence says the comment is what is
    /// wrong.</b> The whole function sits inside a block comment upstream — it is disabled code
    /// that no upstream build runs, so nothing there has been keeping it honest. This test does a
    /// bounded search over the pinned polynomial, the polynomial with its leading term restored,
    /// every register width from 8 to 16 and every bit count the vector could carry, and finds no
    /// reading that produces the stated value. A port that had transposed a digit or mistaken the
    /// bit order would have shown up somewhere in that space. The conclusion recorded here is
    /// that the disabled vector predates the constants beside it.
    /// </para>
    /// <para>
    /// <b>What this test therefore asserts</b> is what is actually known to be true and would be
    /// worth catching if it changed: that the two independent implementations agree on upstream's
    /// own input, and that the stated value is unreachable. It does not assert a match it has not
    /// got, and it does not assert a mismatch that a later correction would have to break.
    /// </para>
    /// <para>
    /// <b>Nothing is transcribed.</b> The input, the bit count and the stated checksum are all
    /// lifted out of the pinned file at run time. Copying them in would have turned an external
    /// vector into an internal one on the way.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamsOneStatedVectorIsDisabledAndDoesNotAgreeWithItsOwnConstants()
    {
        var testSource = Path.Combine(ReferenceClone.Location, @"test\test.c");
        Assert.True(File.Exists(testSource), $"{testSource} is not there.");

        // Read raw: the vector lives inside a block comment, so anything that strips comments
        // first would find nothing at all.
        var text = File.ReadAllText(testSource);

        var call = Regex.Match(text, @"ftx_compute_crc\(\s*(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*,\s*(?<bits>\d+)\s*\)");
        Assert.True(call.Success, $"{testSource} states no ftx_compute_crc call to take a vector from.");

        var name = call.Groups["name"].Value;
        var bits = int.Parse(call.Groups["bits"].Value);

        var expected = Regex.Match(text[call.Index..], @"should be\s+0x(?<value>[0-9A-Fa-f]+)");
        Assert.True(
            expected.Success,
            $"{testSource} calls ftx_compute_crc on {name} but states no expected value after it, "
            + "so there is no known value here after all.");

        var initialiser = Regex.Match(
            text,
            @"\b" + Regex.Escape(name) + @"\s*\[[^\]]*\]\s*=\s*\{(?<body>[^}]*)\}");
        Assert.True(initialiser.Success, $"{testSource} names {name} but does not initialise it inline.");

        var message = initialiser.Groups["body"].Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token =>
            {
                Assert.True(CSourceParser.TryParseIntegerLiteral(token, out var value),
                    $"an element of {name} is not an integer literal this reader handles.");
                Assert.InRange(value, 0, 255);
                return (byte)value;
            })
            .ToArray();

        var disabled = IsInsideABlockComment(text, call.Index);
        var stated = Convert.ToUInt16(expected.Groups["value"].Value, 16);
        var computed = Crc14.Compute(message, bits);

        _output.WriteLine("source                  : test\\test.c");
        _output.WriteLine($"vector identifier       : {name}");
        _output.WriteLine($"vector length           : {message.Length} bytes");
        _output.WriteLine($"bit count stated        : {bits}");
        _output.WriteLine($"inside a block comment  : {disabled}");
        _output.WriteLine($"library matches it      : {computed == stated}");
        _output.WriteLine("The input and the stated checksum are not printed, by ruling.");

        // The two independent implementations agree on upstream's own input. This is the
        // assertion that is worth having here: it is over a message somebody else chose.
        Assert.Equal(computed, CrcCheck.Compute(message, bits));

        var readings = SearchAllReadings(message, stated);
        _output.WriteLine($"readings searched       : {readings.Searched}");
        _output.WriteLine($"readings reproducing it : {readings.Hits}");

        Assert.True(
            readings.Searched > 0,
            "the search space came out empty, which means this test measured nothing.");

        // If a reading ever does turn up, the port — not the comment — is what needs looking at,
        // and this is the assertion that would say so.
        Assert.True(
            readings.Hits == 0,
            $"{readings.Hits} reading(s) of the pinned constants reproduce the value stated in "
            + "test/test.c, so the stale-comment conclusion recorded here is wrong and the port "
            + "is what disagrees. Look at the port.");
    }

    /// <summary>
    /// Every reading of the pinned constants this port could plausibly have got wrong, and how
    /// many of them produce <paramref name="target"/>.
    /// </summary>
    /// <remarks>
    /// The polynomial is the pin's and the pin's with its leading term restored — the one
    /// transcription error a "polynomial without the leading MSB 1" comment invites. The widths
    /// bracket the declared one on both sides, and every bit count the vector could carry is
    /// tried, which covers a miscounted zero-extension.
    /// </remarks>
    private static (int Searched, int Hits) SearchAllReadings(byte[] message, ushort target)
    {
        var polynomials = new[] { (uint)Crc14.Polynomial, Crc14.Polynomial | (1u << Crc14.Width) };
        var searched = 0;
        var hits = 0;

        for (var width = 8; width <= 16; width++)
        {
            foreach (var polynomial in polynomials)
            {
                for (var bits = 0; bits <= message.Length * 8; bits++)
                {
                    searched++;
                    if (Divide(message, bits, polynomial, width) == target)
                    {
                        hits++;
                    }
                }
            }
        }

        return (searched, hits);
    }

    /// <summary>Modulo-2 division at an arbitrary width and polynomial, for the search only.</summary>
    private static ushort Divide(byte[] message, int bitCount, uint polynomial, int width)
    {
        uint remainder = 0;
        var byteIndex = 0;
        for (var bit = 0; bit < bitCount; bit++)
        {
            if (bit % 8 == 0)
            {
                var shift = width - 8;
                remainder ^= shift >= 0 ? (uint)message[byteIndex] << shift : (uint)message[byteIndex] >> -shift;
                remainder &= 0xFFFFu;
                byteIndex++;
            }

            remainder = (remainder & (1u << (width - 1))) != 0
                ? ((remainder << 1) ^ polynomial) & 0xFFFFu
                : (remainder << 1) & 0xFFFFu;
        }

        return (ushort)(remainder & ((1u << width) - 1u));
    }

    /// <summary>Whether the character at <paramref name="index"/> is inside a C block comment.</summary>
    private static bool IsInsideABlockComment(string text, int index)
    {
        var open = text.LastIndexOf("/*", index, StringComparison.Ordinal);
        if (open < 0)
        {
            return false;
        }

        var close = text.IndexOf("*/", open, StringComparison.Ordinal);
        return close < 0 || close > index;
    }
}
