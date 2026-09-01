using System.Text.RegularExpressions;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.TableGen;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// Leg A of the hash's provenance: every scalar of the callsign hash and its cache that can be
/// resolved out of the pinned clone <em>by machine</em>, asserted against this library's own
/// constant at run time.
/// </summary>
/// <remarks>
/// <para>
/// <b>This exists because the hash is the one thing here a round trip cannot check.</b> Every other
/// encoding in this port is a private agreement between its own packer and its own unpacker, and a
/// corpus that closes proves they are inverses. A hash travels on the air: it is only useful if it
/// agrees with what the transmitting station computed, and a hash that is wrong but self-consistent
/// passes every corpus and is silently deaf for good. So the hash gets a different standard of
/// proof, and this is the first of its three legs.
/// </para>
/// <para>
/// <b>Two grades of corroboration, counted separately, because they are not the same evidence.</b>
/// A scalar upstream states as a macro is resolved by <see cref="CSourceParser.ParseIntegerMacros"/>
/// and is corroborated in the strong sense. A scalar upstream writes as a literal inside a function
/// body is not a macro and never will be, so it is located by anchoring on the <em>expression that
/// uses it</em> inside the definition it belongs to, and the captured token is put through the same
/// literal reader the table converter uses. That is still a mechanical read of the pin at run time
/// rather than a transcription — nobody typed it — but it is anchored on a shape, and a shape can be
/// rewritten upstream in a way a macro name cannot. It is counted and reported as the weaker thing
/// it is, exactly as unit 207 reported the alphabet lengths it could only read out of a comment.
/// </para>
/// <para>
/// <b>Names, never values.</b> Every assertion prints which scalar matched and none of them prints
/// what it was. A scalar that cannot be resolved at all is named as uncorroborated rather than
/// quietly dropped: an honest count is worth more than a claim of full provenance.
/// </para>
/// <para>
/// <b>Nothing under <c>TableGen/</c> was changed for this test.</b> It calls
/// <see cref="CSourceParser.ParseIntegerMacros"/>, <see cref="CSourceParser.TryParseIntegerLiteral"/>
/// and <c>ExpressionEvaluator</c> as unit 206 left them, so <c>Ft8TableGenerationTests</c> and its
/// byte-for-byte regeneration proof are untouched.
/// </para>
/// </remarks>
public class UpstreamCallsignHashProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamCallsignHashProvenanceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The scalars the hash itself is made of, read out of the definition that computes it.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheHashScalarsMatchThePin()
    {
        var source = ReadFromPin(@"ft8\message.c");
        var definition = ReferenceCloneHashInventoryTests.ExtractDefinition(source, "save_callsign");

        Assert.NotNull(definition);
        Assert.Contains('{', definition!);

        _output.WriteLine("NAMES ONLY — no multiplier, base, mask, shift width or alphabet is printed here.");
        _output.WriteLine($"definition located in message.c : save_callsign, {definition!.Split('\n').Length} lines");

        var byMacro = 0;
        var byExpression = 0;
        var uncorroborated = new List<string>();

        // Every one of these anchors on the surrounding expression rather than on the literal, so a
        // number that happens to appear elsewhere in the function cannot be mistaken for this one.
        void CheckExpression(string role, string pattern, long ours)
        {
            var match = Regex.Match(definition, pattern);
            if (!match.Success || !CSourceParser.TryParseIntegerLiteral(match.Groups["v"].Value, out var theirs))
            {
                uncorroborated.Add(role);
                _output.WriteLine($"    {role,-26} NOT RESOLVABLE from the expression — uncorroborated");
                return;
            }

            Assert.True(
                theirs == ours,
                $"the {role} in the pin does not equal this library's constant. The value is deliberately "
                + "not in this message; the port is wrong and the constant has to be re-read from the pin "
                + "through the gated emitter.");

            byExpression++;
            _output.WriteLine($"    {role,-26} matches (literal in a function body — weaker than a macro)");
        }

        // n58 = (BASE * n58) + j, the accumulation that packs the call before it is hashed.
        CheckExpression(
            "packing base",
            @"=\s*\(\s*(?<v>\d+)\s*\*\s*n58\s*\)\s*\+",
            (long)Ft8CallsignHash.PackingBase);

        // The bound on how many characters of the call are read, and the identical bound on the
        // right-padding loop that follows it. Both are asserted, because they must be the same
        // number for the padding to mean what it means.
        CheckExpression(
            "callsign length read",
            @"callsign\[i\]\s*!=\s*'\\0'\s*&&\s*i\s*<\s*(?<v>\d+)",
            Ft8CallsignHash.MaxCallsignLength);
        CheckExpression(
            "callsign length padded",
            @"while\s*\(\s*i\s*<\s*(?<v>\d+)\s*\)",
            Ft8CallsignHash.MaxCallsignLength);

        // The multiplier, the product shift and the mask, out of the one expression that carries
        // all three. This is the constant the whole risk sits on.
        CheckExpression(
            "hash multiplier",
            @"\(\s*(?<v>[0-9]+u?l*)\s*\*\s*n58\s*\)\s*>>",
            (long)Ft8CallsignHash.Multiplier);
        CheckExpression(
            "product width",
            @">>\s*\(\s*(?<v>\d+)\s*-\s*22\s*\)",
            64);
        CheckExpression(
            "hash width",
            @">>\s*\(\s*64\s*-\s*(?<v>\d+)\s*\)",
            Ft8CallsignHash.Bits22);
        CheckExpression(
            "hash mask",
            @">>\s*\(\s*64\s*-\s*22\s*\)\s*\)\s*&\s*\(\s*(?<v>0[xX][0-9a-fA-F]+u?l*)\s*\)",
            Ft8CallsignHash.Mask22);

        // The two truncations. That the narrow hashes are shifts of the wide one rather than
        // separate functions is itself the structural fact, and it is read here rather than assumed.
        CheckExpression(
            "12-bit truncation shift",
            @"n12\s*=\s*n22\s*>>\s*(?<v>\d+)",
            Ft8CallsignHash.Shift12);
        CheckExpression(
            "10-bit truncation shift",
            @"n10\s*=\s*n22\s*>>\s*(?<v>\d+)",
            Ft8CallsignHash.Shift10);

        // The alphabet, corroborated as an identifier rather than as a value: the name upstream
        // packs against is read out of this definition, and its position in the pin's own
        // enumeration is what pairs it with this library's alphabet. The length of that alphabet is
        // then asserted to be the packing base, which is what makes the two facts one fact.
        var alphabet = Regex.Match(definition, @"nchar\s*\([^,]+,\s*(?<name>FT8_CHAR_TABLE_[A-Z_]+)\s*\)");
        if (!alphabet.Success)
        {
            uncorroborated.Add("packing alphabet");
            _output.WriteLine("    packing alphabet           NOT RESOLVABLE — uncorroborated");
        }
        else
        {
            var name = alphabet.Groups["name"].Value;
            var declaredOrder = DeclaredAlphabetOrder(ReadFromPin(@"ft8\text.h"));
            var position = declaredOrder.IndexOf(name);
            Assert.True(position >= 0, $"{name} is used by save_callsign but is not declared in text.h.");

            var ours = Enum.GetValues<Ft8CharTable>()[position];
            Assert.Equal(Ft8CharTable.AlphanumericSpaceSlash, ours);
            Assert.Equal((long)Ft8CallsignHash.PackingBase, Ft8Text.Length(ours));

            byExpression++;
            _output.WriteLine(
                $"    packing alphabet           matches ({name} is at position {position} in the pin's own "
                + "declaration order, which is this library's AlphanumericSpaceSlash, whose length is the base)");
        }

        _output.WriteLine($"corroborated by macro      : {byMacro}");
        _output.WriteLine($"corroborated by expression : {byExpression}");
        _output.WriteLine($"uncorroborated             : {uncorroborated.Count} ({string.Join(", ", uncorroborated)})");

        Assert.Empty(uncorroborated);
        Assert.Equal(10, byExpression);
        Assert.Equal(0, byMacro);
    }

    /// <summary>
    /// The pin's own declaration order of the six alphabets, which is what pairs an upstream
    /// alphabet name with one of this library's.
    /// </summary>
    private static List<string> DeclaredAlphabetOrder(string textHeader) =>
        Regex.Matches(textHeader, @"(?<name>FT8_CHAR_TABLE_[A-Z_]+)\s*,\s*//\s*table\[")
            .Select(m => m.Groups["name"].Value)
            .ToList();

    private static string ReadFromPin(string relative)
    {
        var path = Path.Combine(ReferenceClone.Location, relative);
        Assert.True(File.Exists(path), $"{path} is not there, so nothing can be corroborated against it.");
        return File.ReadAllText(path);
    }
}
