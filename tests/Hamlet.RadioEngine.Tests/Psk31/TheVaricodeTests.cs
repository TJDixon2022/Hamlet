using System;
using System.IO;
using System.Linq;
using System.Text;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 314 task 2: **the varicode, cited, and a codec over it.**
/// </summary>
/// <remarks>
/// <para>**PSK31 CARRIES TEXT IN VARICODE**, Peter Martinez G3PLX's self-delimiting
/// variable-length code: common letters are short, every code starts and ends with `1`,
/// and no code contains `00`. **So `00` is the character separator** and a decoder needs
/// no framing beyond splitting on it.</para>
/// <para>**THE TABLE IS DATA WITH A CITATION, NOT A PORT** (§R5, and unit 252's pattern
/// for the DXCC list). `data/psk31/varicode.csv` with `varicode-SOURCE.md` beside it,
/// extracted mechanically from the pinned `fldigi` commit. The code is the published
/// standard; fldigi is the citation for the exact bit strings.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio.</para>
/// </remarks>
public sealed class TheVaricodeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the codes are printed.</param>
    public TheVaricodeTests(ITestOutputHelper output) => _output = output;

    /// <summary>**All 256 codes round-trip.**</summary>
    [Fact]
    public void All256CodesRoundTrip()
    {
        _output.WriteLine("codes in the table: " + Varicode.Codes.Count);

        Assert.Equal(256, Varicode.Codes.Count);

        for (var code = 0; code < 256; code++)
        {
            var bits = Varicode.BitsFor((byte)code);

            Assert.False(
                string.IsNullOrEmpty(bits),
                "byte " + code + " has no varicode");

            var back = Varicode.ByteFor(bits!);

            Assert.True(
                back == code,
                "byte " + code + " encoded to " + bits + " and decoded to "
                + (back?.ToString() ?? "nothing"));
        }
    }

    /// <summary>**The table's three structural promises hold.**</summary>
    /// <remarks>
    /// **THESE ARE WHAT MAKE THE CODE SELF-DELIMITING**, and a table that broke any of
    /// them would make a demodulator built on `00` silently wrong rather than obviously
    /// broken.
    /// </remarks>
    [Fact]
    public void NoCodeContainsTwoZerosAndEveryOneStartsAndEndsWithOne()
    {
        var seen = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);

        foreach (var bits in Varicode.Codes)
        {
            Assert.DoesNotContain("00", bits, StringComparison.Ordinal);
            Assert.StartsWith("1", bits, StringComparison.Ordinal);
            Assert.EndsWith("1", bits, StringComparison.Ordinal);

            Assert.True(seen.Add(bits), bits + " appears twice in the table");
        }

        _output.WriteLine("all " + seen.Count + " codes are distinct, contain no 00, "
            + "and start and end with 1");

        _output.WriteLine("shortest: " + Varicode.Codes.Min(b => b.Length)
            + " bits, longest: " + Varicode.Codes.Max(b => b.Length) + " bits");

        Assert.Equal(256, seen.Count);
    }

    /// <summary>**The five published spot checks.**</summary>
    [Fact]
    public void TheFivePublishedSpotChecks()
    {
        foreach (var (character, expected) in new[]
        {
            (' ', "1"), ('e', "11"), ('t', "101"), ('o', "111"), ('a', "1011"),
        })
        {
            var bits = Varicode.BitsFor((byte)character);

            _output.WriteLine("'" + character + "' -> " + bits);

            Assert.Equal(expected, bits);
        }
    }

    /// <summary>**The QSO text encodes and decodes back identical.**</summary>
    /// <remarks>
    /// **THE SAME TEXT THE FIXTURES CARRY**, read from the file rather than retyped, so
    /// this and the demodulator's own tests cannot come to disagree about what was sent.
    /// </remarks>
    [Fact]
    public void TheQsoTextEncodesAndDecodesBackIdentical()
    {
        var text = QsoText();

        _output.WriteLine("characters : " + text.Length);

        var bits = Varicode.Encode(text);

        _output.WriteLine("bits       : " + bits.Length);
        _output.WriteLine("bits/char  : "
            + ((double)bits.Length / text.Length).ToString("0.0",
                System.Globalization.CultureInfo.InvariantCulture));

        _output.WriteLine("first 60   : " + bits[..60]);

        var back = Varicode.Decode(bits);

        Assert.Equal(text, back);

        // **AND THERE IS A `00` BETWEEN EVERY CHARACTER**, which is what the
        // demodulator will split on. Counting the separators is the same as counting
        // the characters.
        var separators = 0;

        for (var at = 0; at + 1 < bits.Length; at++)
        {
            if (bits[at] == '0' && bits[at + 1] == '0')
            {
                separators++;
                at++;
            }
        }

        _output.WriteLine("separators : " + separators);

        Assert.Equal(text.Length, separators);
    }

    /// <summary>**Idle before and after decodes to the text alone.**</summary>
    /// <remarks>
    /// **IDLE IS A RUN OF `0`s** - continuous phase reversals - which is what a station
    /// sends between characters and before it starts typing. It carries no character and
    /// must produce none.
    /// </remarks>
    [Fact]
    public void IdleBeforeAndAfterDecodesToTheTextAlone()
    {
        var text = QsoText();

        var stream = new string('0', 40) + Varicode.Encode(text) + new string('0', 40);

        _output.WriteLine("stream is " + stream.Length + " bits, "
            + "40 idle either side");

        var back = Varicode.Decode(stream);

        Assert.Equal(text, back);
    }

    /// <summary>The exact text the fixtures carry, read from its own file.</summary>
    private static string QsoText()
    {
        var path = Path.Combine(Root(), "assets", "fixtures", "qso-text.txt");

        Assert.True(File.Exists(path), "no assets/fixtures/qso-text.txt");

        // **THE BYTES AS SENT.** The modulator encodes latin-1 bytes, and the file has
        // CR LF line ends because that is what went on the air.
        return Encoding.Latin1.GetString(File.ReadAllBytes(path));
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}
