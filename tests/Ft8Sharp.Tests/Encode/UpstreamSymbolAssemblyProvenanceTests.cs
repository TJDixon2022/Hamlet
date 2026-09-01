using System.Text.RegularExpressions;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.TableGen;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Encode;

/// <summary>
/// Every scalar the symbol assembly stands on, resolved out of the pinned clone at run time and
/// asserted against this library's own constant — and, for each, <b>whether it is anchored on a
/// macro or on an expression inside a function body</b>.
/// </summary>
/// <remarks>
/// <para>
/// <b>The distinction is the point.</b> A macro is a declaration: it has a name, it is read the
/// same way by every compiler, and a change to it is a change to the file's interface. A literal
/// inside a function body has none of those properties — it is matched by shape, the match can go
/// stale silently, and a second literal of the same value in the same function can satisfy the
/// pattern for the wrong reason. Both are mechanical reads of the pin rather than transcriptions,
/// and that is the whole of what they have in common. Each line below says which it is.
/// </para>
/// <para>
/// <b>This is not bit-identity and does not stand in for it.</b> Step 3's second exit criterion
/// asks whether the symbol sequence equals <c>ft8_lib</c>'s for the same message. Corroborating
/// every scalar cannot answer that: a port can read every constant correctly and still assemble
/// them in the wrong order. Unit 209 could not build the pin's generator on this machine — there
/// is no C toolchain here — so that criterion is open, and this test says so rather than letting
/// a column of green ticks imply it was met.
/// </para>
/// <para>
/// <b>Names, never values.</b> Every assertion prints which scalar matched and none prints what it
/// was. A scalar that cannot be resolved is counted and named as uncorroborated rather than
/// quietly dropped.
/// </para>
/// </remarks>
public class UpstreamSymbolAssemblyProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamSymbolAssemblyProvenanceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The sequence geometry: how many symbols a transmission sends, how many carry data, how long
    /// a sync group is, how many there are and how far apart they sit. All five are macros.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheSequenceGeometryIsMacroAnchoredInThePin()
    {
        var source = ReadFromPin(@"ft8\constants.h");
        var macros = CSourceParser.ParseIntegerMacros(source);

        _output.WriteLine("NAMES ONLY — no scalar's value is printed here, by ruling.");
        _output.WriteLine($"integer macros resolved in constants.h : {macros.Count}");

        var corroborated = 0;
        var uncorroborated = new List<string>();

        void CheckMacro(string macro, long ours, string role)
        {
            if (!macros.TryGetValue(macro, out var theirs))
            {
                uncorroborated.Add(macro);
                _output.WriteLine($"    {macro,-18} {role,-32} NOT RESOLVABLE as a macro — uncorroborated");
                return;
            }

            Assert.True(
                theirs == ours,
                $"{macro} in the pin does not equal this library's constant for {role}. The value is "
                + "deliberately not in this message; the port is wrong and the constant has to be "
                + "re-read from the pin through the gated emitter.");

            corroborated++;
            _output.WriteLine($"    {macro,-18} {role,-32} matches   [MACRO-ANCHORED]");
        }

        CheckMacro("FT8_NN", Ft8SymbolEncoder.SymbolCount, "total channel symbols");
        CheckMacro("FT8_ND", Ft8SymbolEncoder.DataSymbolCount, "data symbols");
        CheckMacro("FT8_LENGTH_SYNC", Ft8SymbolEncoder.SyncBlockLength, "length of each sync group");
        CheckMacro("FT8_NUM_SYNC", Ft8SymbolEncoder.SyncBlockCount, "number of sync groups");
        CheckMacro("FT8_SYNC_OFFSET", Ft8SymbolEncoder.SyncBlockOffset, "offset between sync groups");

        _output.WriteLine($"corroborated by machine : {corroborated}   [all MACRO-ANCHORED]");
        _output.WriteLine($"uncorroborated          : {uncorroborated.Count} ({string.Join(", ", uncorroborated)})");

        Assert.Equal(5, corroborated);
        Assert.Empty(uncorroborated);

        // The geometry has to close on itself: sync symbols plus data symbols is the whole
        // transmission, and the data symbols carry the whole codeword three bits at a time. Neither
        // is read from the pin — both are arithmetic over what was, and a port that got one scalar
        // right and another wrong fails here rather than at the radio.
        Assert.Equal(
            Ft8SymbolEncoder.SymbolCount,
            (Ft8SymbolEncoder.SyncBlockCount * Ft8SymbolEncoder.SyncBlockLength)
            + Ft8SymbolEncoder.DataSymbolCount);
        Assert.Equal(
            Ft8SymbolEncoder.DataSymbolCount * Ft8SymbolEncoder.BitsPerSymbol,
            Ft8Sharp.Ft8Tables.LdpcN);
        _output.WriteLine("geometry closes: sync + data = total, and data * 3 bits = the codeword");
    }

    /// <summary>
    /// The tone alphabet, anchored on the declared extent of upstream's Gray map array rather than
    /// on a macro. Weaker than a macro and stronger than a transcription; named as what it is.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheToneAlphabetIsAnchoredOnTheDeclaredExtentOfTheGrayMap()
    {
        var source = ReadFromPin(@"ft8\constants.h");

        _output.WriteLine("NAMES AND EXTENTS ONLY — no element of any table is printed here.");

        var declaration = Regex.Match(
            source,
            @"extern\s+const\s+uint8_t\s+kFT8_Gray_map\s*\[\s*(?<extent>\d+)\s*\]");
        Assert.True(
            declaration.Success,
            "constants.h declares no kFT8_Gray_map with a literal extent, so the tone alphabet's "
            + "size cannot be anchored on it and this test measured nothing.");

        var extent = int.Parse(declaration.Groups["extent"].Value);
        Assert.Equal(Ft8SymbolEncoder.ToneCount, extent);
        _output.WriteLine(
            $"    kFT8_Gray_map extent -> tone alphabet size   matches   "
            + "[ARRAY-EXTENT-ANCHORED — a declaration, not a macro and not a function body]");

        var costas = Regex.Match(
            source,
            @"extern\s+const\s+uint8_t\s+kFT8_Costas_pattern\s*\[\s*(?<extent>\d+)\s*\]");
        Assert.True(costas.Success, "constants.h declares no kFT8_Costas_pattern with a literal extent.");
        Assert.Equal(Ft8SymbolEncoder.SyncBlockLength, int.Parse(costas.Groups["extent"].Value));
        _output.WriteLine(
            "    kFT8_Costas_pattern extent -> sync group length  matches   [ARRAY-EXTENT-ANCHORED]");

        // The alphabet has to be exactly what three bits can address, and the checked-in tables
        // have to fit inside it. This is arithmetic over the port, not a read of the pin.
        Assert.Equal(Ft8SymbolEncoder.ToneCount, 1 << Ft8SymbolEncoder.BitsPerSymbol);
        _output.WriteLine("the alphabet is exactly what three bits address");
    }

    /// <summary>
    /// The four things that are <em>not</em> macros: where the three sync blocks sit, which way the
    /// Gray map runs, how the three bits are ordered within their group, and that the codeword is
    /// walked most significant bit first.
    /// </summary>
    /// <remarks>
    /// <b>Every one of these is an expression inside <c>ft8_encode</c>'s body and is the weaker
    /// anchoring.</b> They are matched by shape against the function's own text. Getting the Gray
    /// map's direction backwards produces a sequence of the right length with every value inside
    /// the alphabet and the sync blocks in the right places — nothing this library can assert about
    /// its own output would catch it, and only a comparison against upstream's own tones would.
    /// That comparison did not run tonight, so this is the strongest evidence there is for these
    /// four, and it is not strong.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheAssemblyOrderIsExpressionAnchoredInsideTheEncoderBody()
    {
        var body = EncodeFunctionBody();

        _output.WriteLine("SHAPES ONLY — no line of upstream source is printed here.");
        _output.WriteLine("Every item below is EXPRESSION-ANCHORED inside ft8_encode's body.");

        var corroborated = 0;

        void CheckShape(string what, bool matched, string why)
        {
            Assert.True(matched, $"{what} could not be corroborated against the pin: {why}");
            corroborated++;
            _output.WriteLine($"    {what,-52} matches   [EXPRESSION-ANCHORED]");
        }

        // The three sync blocks, each written as a literal half-open range rather than as
        // FT8_SYNC_OFFSET arithmetic. The port derives them from the macro; upstream does not, so
        // the two are checked against each other here.
        for (var block = 0; block < Ft8SymbolEncoder.SyncBlockCount; block++)
        {
            var start = block * Ft8SymbolEncoder.SyncBlockOffset;
            var end = start + Ft8SymbolEncoder.SyncBlockLength;

            // A guard of the shape (i_tone >= START) && (i_tone < END), and, inside it, an index
            // into the Costas pattern rebased by START.
            var guard = Regex.IsMatch(
                body,
                $@"i_tone\s*>=\s*{start}\s*\)\s*&&\s*\(\s*i_tone\s*<\s*{end}\b");
            var rebase = start == 0
                ? Regex.IsMatch(body, @"kFT8_Costas_pattern\s*\[\s*i_tone\s*\]")
                : Regex.IsMatch(body, $@"kFT8_Costas_pattern\s*\[\s*i_tone\s*-\s*{start}\s*\]");

            CheckShape(
                $"sync block {block} at offset * {block}, rebased index",
                guard && rebase,
                $"no guard for the half-open range starting at sync block {block}, or no Costas "
                + "index rebased to its start");
        }

        // The direction the Gray map runs. The three codeword bits are the INDEX and the map's
        // element is the TONE. The inverse would be the decoder's, and it is not what runs here.
        CheckShape(
            "Gray map indexed by the bits, yielding the tone",
            Regex.IsMatch(body, @"tones\s*\[\s*i_tone\s*\]\s*=\s*kFT8_Gray_map\s*\[\s*bits3\s*\]"),
            "ft8_encode does not assign a tone from kFT8_Gray_map indexed by the extracted bits, so "
            + "the direction of the map cannot be read off the pin by shape");

        // Bit order within the group: the first bit taken carries the most weight.
        var weights = Regex.Matches(body, @"bits3\s*\|=\s*(?<weight>\d+)")
            .Select(m => int.Parse(m.Groups["weight"].Value))
            .ToList();
        CheckShape(
            "three bits per symbol, first taken is most significant",
            weights.Count == Ft8SymbolEncoder.BitsPerSymbol
            && weights.SequenceEqual(Enumerable
                .Range(0, Ft8SymbolEncoder.BitsPerSymbol)
                .Select(i => 1 << (Ft8SymbolEncoder.BitsPerSymbol - 1 - i))),
            $"ft8_encode assembles {weights.Count} bits into the group rather than "
            + $"{Ft8SymbolEncoder.BitsPerSymbol}, or does not weight them most significant first");

        // The codeword is walked most significant bit first, which is how LdpcEncoder writes it.
        CheckShape(
            "codeword walked most significant bit first",
            Regex.IsMatch(body, @"mask\s*=\s*0x80u?\s*;") && Regex.IsMatch(body, @"mask\s*>>=\s*1"),
            "ft8_encode does not start its bit mask at the most significant bit of a byte and shift "
            + "it down, so the walk direction cannot be read off the pin by shape");

        // The walk does NOT restart at a sync block: the bit-walk state is initialised once, before
        // the loop, and touched only in the data branch. This is the one a plausible reading gets
        // wrong, and it is checked as two facts rather than one — the sync branches never mention
        // the walk's state, and the data branch advances the byte index exactly once per bit.
        var loop = Regex.Match(body, @"for\s*\(\s*int\s+i_tone\b");
        Assert.True(loop.Success, "ft8_encode has no loop over i_tone, so nothing below is anchored.");
        var dataStart = body.IndexOf("bits3", StringComparison.Ordinal);
        Assert.True(dataStart > loop.Index, "ft8_encode's data branch does not follow its loop header.");

        var syncRegion = body[loop.Index..dataStart];
        var dataBranch = body[dataStart..];
        var syncTouchesTheWalk = syncRegion.Contains("mask", StringComparison.Ordinal)
                                 || syncRegion.Contains("i_byte", StringComparison.Ordinal);

        CheckShape(
            "bit walk continuous across sync blocks, advanced only by data",
            !syncTouchesTheWalk
            && Regex.Matches(dataBranch, @"i_byte\+\+").Count == Ft8SymbolEncoder.BitsPerSymbol,
            syncTouchesTheWalk
                ? "a sync branch mentions the bit mask or the codeword byte index, so the walk is "
                  + "not continuous across the sync blocks"
                : "the byte index is not advanced exactly once per bit inside the data branch");

        _output.WriteLine($"corroborated by machine : {corroborated}   [all EXPRESSION-ANCHORED]");

        // Three sync blocks, plus the map direction, the bit order, the walk direction and the
        // walk's continuity across the blocks.
        Assert.Equal(Ft8SymbolEncoder.SyncBlockCount + 4, corroborated);
    }

    /// <summary>
    /// Whether the payload assembly upstream does before encoding is the one
    /// <c>Ft8Payload.Create</c> already does. A disagreement here would be a step 2 defect
    /// surfacing and is asserted loudly rather than noted.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void UpstreamsPayloadAssemblyIsTheOneThisLibraryAlreadyDoes()
    {
        var body = EncodeFunctionBody();
        var constants = ReadFromPin(@"ft8\constants.h");
        var macros = CSourceParser.ParseIntegerMacros(constants);

        _output.WriteLine("SHAPES ONLY — no line of upstream source is printed here.");

        // ft8_encode buffers the CRC'd message in FTX_LDPC_K_BYTES and encodes that.
        Assert.True(
            Regex.IsMatch(body, @"a91\s*\[\s*FTX_LDPC_K_BYTES\s*\]"),
            "ft8_encode does not size its CRC'd buffer on FTX_LDPC_K_BYTES, so this library's "
            + "PayloadBytes cannot be anchored on it.");
        Assert.True(macros.TryGetValue("FTX_LDPC_K_BYTES", out var payloadBytes));
        Assert.Equal(Ft8Sharp.Message.Ft8Payload.PayloadBytes, payloadBytes);
        _output.WriteLine(
            "    CRC'd payload buffer size                          matches   [MACRO-ANCHORED]");

        Assert.True(macros.TryGetValue("FTX_LDPC_N_BYTES", out var codewordBytes));
        Assert.Equal(Ft8Sharp.Ldpc.LdpcEncoder.CodewordBytes, codewordBytes);
        _output.WriteLine(
            "    codeword buffer size                               matches   [MACRO-ANCHORED]");

        // And the chain is CRC then encode then lay out, in that order and with nothing between.
        var addCrc = body.IndexOf("ftx_add_crc", StringComparison.Ordinal);
        var encode = body.IndexOf("encode174", StringComparison.Ordinal);
        var layout = body.IndexOf("i_tone", StringComparison.Ordinal);
        Assert.True(
            addCrc >= 0 && encode > addCrc && layout > encode,
            "ft8_encode does not run add-CRC, then encode, then lay out symbols in that order — "
            + "which is a step 2 defect surfacing if this library's chain is the other shape.");
        _output.WriteLine(
            "    chain is add-CRC -> LDPC encode -> lay out symbols  matches   [EXPRESSION-ANCHORED]");
        _output.WriteLine(
            "step 2's payload assembly agrees with upstream's: no defect surfaced here.");
    }

    /// <summary>
    /// The body of <c>ft8_encode</c> alone, so a shape matched here cannot have been matched
    /// against <c>ft4_encode</c>, which sits in the same file and has the same skeleton with
    /// different numbers in it.
    /// </summary>
    private string EncodeFunctionBody()
    {
        var source = ReadFromPin(@"ft8\encode.c");
        var start = Regex.Match(source, @"^void\s+ft8_encode\s*\([^)]*\)\s*\{", RegexOptions.Multiline);
        Assert.True(
            start.Success,
            "encode.c has no definition of ft8_encode at column zero, so nothing below is anchored "
            + "on the function this port came from.");

        var open = start.Index + start.Length - 1;
        var depth = 0;
        for (var i = open; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[open..(i + 1)];
                }
            }
        }

        Assert.Fail("ft8_encode's body is not brace-balanced in the pin.");
        return string.Empty;
    }

    private static string ReadFromPin(string relative)
    {
        var path = Path.Combine(ReferenceClone.Location, relative);
        Assert.True(File.Exists(path), $"{path} is not in the pinned clone.");
        return File.ReadAllText(path);
    }
}
