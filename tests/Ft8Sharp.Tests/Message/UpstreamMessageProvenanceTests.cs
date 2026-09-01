using System.Text.RegularExpressions;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.TableGen;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// Every scalar of the message layer that can be mechanically resolved out of the pinned clone,
/// asserted against this library's own constant.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is what stands in for a known value, and it is weaker than one.</b> A round-trip over a
/// corpus proves that this library's packer and unpacker are inverses; it proves nothing about
/// whether the bits they agree on are the bits the reference implementation would have produced.
/// What this test does prove is narrower and real: that no field width, sub-range boundary or
/// alphabet length was transcribed wrong. It cannot corroborate the branching arithmetic that sits
/// between them — that is what step 3's bit-identical symbol comparison is for, and this test says
/// so rather than letting a green tick imply more than it measured.
/// </para>
/// <para>
/// <b>Names, never values.</b> Each assertion prints which scalar matched. None of them prints
/// what it was. A scalar that is not mechanically resolvable is counted and named as
/// uncorroborated rather than quietly dropped, because an honest "four of seven by machine" is
/// worth more than a claim of full provenance.
/// </para>
/// <para>
/// <b>The existing reader is reused rather than replaced.</b>
/// <see cref="CSourceParser.ParseIntegerMacros"/> and <c>ExpressionEvaluator</c> already resolve
/// cast integer macros; nothing in either was changed for this unit, so
/// <c>Ft8TableGenerationTests</c> is unaffected. The one thing they cannot read is an alphabet
/// length, because upstream states those in the comment beside an enumerator rather than as a
/// macro — that is reported as the weaker provenance it is.
/// </para>
/// </remarks>
public class UpstreamMessageProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamMessageProvenanceTests(ITestOutputHelper output) => _output = output;

    [RequiresReferenceCloneFact]
    public void FieldBoundariesMatchTheMacrosInThePin()
    {
        var source = ReadFromPin(@"ft8\message.c");
        var macros = CSourceParser.ParseIntegerMacros(source);

        _output.WriteLine("NAMES ONLY — no scalar's value is printed here, by ruling.");
        _output.WriteLine($"integer macros resolved in message.c : {macros.Count}");

        var corroborated = 0;
        var uncorroborated = new List<string>();

        void Check(string macro, long ours)
        {
            if (!macros.TryGetValue(macro, out var theirs))
            {
                uncorroborated.Add(macro);
                _output.WriteLine($"    {macro,-12} NOT RESOLVABLE as a macro — uncorroborated");
                return;
            }

            Assert.True(
                theirs == ours,
                $"{macro} in the pin does not equal this library's constant. The value is deliberately "
                + "not in this message; the port is wrong and the constant has to be re-read from the "
                + "pin through the gated emitter.");

            corroborated++;
            _output.WriteLine($"    {macro,-12} matches");
        }

        Check("MAX22", Ft8CallsignField.HashRangeSize);
        Check("NTOKENS", Ft8CallsignField.TokenRangeSize);
        Check("MAXGRID4", Ft8GridField.MaxGrid);

        _output.WriteLine($"corroborated by machine : {corroborated}");
        _output.WriteLine($"uncorroborated          : {uncorroborated.Count} ({string.Join(", ", uncorroborated)})");

        Assert.Equal(3, corroborated);
        Assert.Empty(uncorroborated);
    }

    /// <summary>
    /// The six alphabets, their names and their lengths, read out of the pin at run time.
    /// </summary>
    /// <remarks>
    /// <b>The provenance here is weaker and is reported as such.</b> Upstream states each
    /// alphabet's length in the comment beside its enumerator rather than in a macro, so what this
    /// reads is a comment. A comment can go stale where a macro cannot. It is still a mechanical
    /// read of the pin rather than a transcription, and it is the strongest form available for
    /// these six numbers; that it is the strongest available does not make it strong, and the
    /// report says so.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void AlphabetCountAndLengthsMatchTheEnumeratorsInThePin()
    {
        var source = ReadFromPin(@"ft8\text.h");

        var declared = AlphabetEnumerator.Matches(source)
            .Select(m => (Name: m.Groups["name"].Value, Length: int.Parse(m.Groups["length"].Value)))
            .ToList();

        _output.WriteLine("NAMES AND COUNTS ONLY — no character of any alphabet is printed here.");
        _output.WriteLine($"alphabet enumerators in text.h : {declared.Count}");

        var ours = Enum.GetValues<Ft8CharTable>();
        Assert.Equal(declared.Count, ours.Length);

        // The order is upstream's own declaration order, which is the order this enumeration was
        // written in. Position is what pairs them, so a reordering upstream shows up here.
        for (var i = 0; i < declared.Count; i++)
        {
            var (name, length) = declared[i];
            var matched = Ft8Text.Length(ours[i]) == length;
            _output.WriteLine($"    {name,-38} -> {ours[i],-24} length {(matched ? "matches" : "DIFFERS")}");
            Assert.True(
                matched,
                $"{name} states a different length in the pin than {ours[i]} carries in this library. "
                + "The value is deliberately not in this message.");
        }

        // The alphabets are what the round-trip walks, so their total size is worth stating as a
        // count: it is metadata, and it is the size of the exhaustive bijection test below.
        _output.WriteLine($"total code points across all six : {declared.Sum(d => d.Length)}");
    }

    /// <summary>
    /// The message type codes, read out of the pin's own type function rather than out of its
    /// comments.
    /// </summary>
    /// <remarks>
    /// <b>What is resolvable here is the count, not the individual codes.</b> Upstream keys its
    /// types on a <c>switch</c> over two extracted bit fields, so there is no macro or enumerator
    /// carrying "standard is one". What can be read mechanically is how many members the type
    /// enumeration declares and how many the width of the selector admits; the mapping from code
    /// to type is corroborated by the widths and by nothing else, and it is counted as
    /// uncorroborated.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TypeSelectorWidthsMatchThePin()
    {
        var source = ReadFromPin(@"ft8\message.c");

        // i3 is masked with a three-bit mask and n3 is assembled from two pieces that together
        // span three bits. Both are read as the shapes they are written in.
        var i3Mask = Regex.Matches(source, @"payload\[9\]\s*>>\s*3\)\s*&\s*0x07u").Count;
        var n3Assembly = Regex.Matches(source, @"payload\[8\]\s*<<\s*2\)\s*&\s*0x04u").Count;

        _output.WriteLine($"i3 three-bit extractions found in message.c : {i3Mask}");
        _output.WriteLine($"n3 assembly expressions found in message.c  : {n3Assembly}");

        Assert.True(i3Mask > 0, "the pin no longer extracts the primary type field the way this port does.");
        Assert.True(n3Assembly > 0, "the pin no longer extracts the secondary type field the way this port does.");

        Assert.Equal(3, Ft8MessageTypes.PrimaryBits);
        Assert.Equal(3, Ft8MessageTypes.SecondaryBits);

        _output.WriteLine("primary and secondary selector widths : match");
        _output.WriteLine(
            "type-code-to-type mapping             : NOT mechanically resolvable — it is a switch, "
            + "not a table, and is counted as uncorroborated");
    }

    /// <summary>An alphabet as upstream holds it: an enumerator with its length in the comment beside it.</summary>
    private static readonly Regex AlphabetEnumerator = new(
        @"(?<name>[A-Za-z_][A-Za-z0-9_]*CHAR_TABLE[A-Za-z0-9_]*)\s*,\s*//\s*table\[(?<length>\d+)\]",
        RegexOptions.Compiled);

    private static string ReadFromPin(string relative)
    {
        var path = Path.Combine(ReferenceClone.Location, relative);
        Assert.True(File.Exists(path), $"{path} is not there, so nothing can be corroborated against it.");
        return File.ReadAllText(path);
    }
}
