using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// The sanctioned read of the pinned clone for unit 217: <b>what upstream does at the stage this
/// library refuses at.</b> Four questions, each pinned by an assertion, so a re-pin that changes the
/// answer goes red rather than leaving the next unit reasoning from a stale paragraph.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why these four and not others.</b> Unit 216 measured that 2733 candidates over upstream's own
/// sixty recordings satisfied every parity check and their own CRC-14, and only 2263 became text.
/// The 470 that did not died inside <c>Ft8MessageDecoder</c>, which has exactly three ways to refuse.
/// Before that census can be read, four things about upstream have to be facts rather than
/// expectations: what it writes for a hashed callsign it cannot resolve, which types it actually
/// prints, whether it ever re-offers a refused payload, and how long its hash table lives.
/// </para>
/// <para>
/// <b>Read through the test process, because the sandbox refuses a session direct access to the
/// clone</b>, as it has since unit 209. This extends the inventory mechanism —
/// <c>[RequiresReferenceCloneFact]</c>, the reachability probe, brace-matched function bodies,
/// shapes and not values — that <see cref="ReferenceCloneMessageInventoryTests"/> and unit 216's
/// <c>UpstreamExtractionInventoryTests</c> established. It writes no new mechanism.
/// </para>
/// <para>
/// <b>The anchoring split, as every unit since 209 has reported it.</b> A declaration, a documented
/// struct or a public entry point is a STRONG anchor; an expression inside a static function body is
/// WEAK. Both are answers. Saying which is which is the point, because a weak anchor is the one a
/// re-pin quietly moves.
/// </para>
/// <para>
/// <b>Nothing here is a licence.</b> Two of these four answers describe upstream behaviour this
/// library deliberately does not have — the literal <c>&lt;...&gt;</c> written into a message whose
/// station could not be named, and a callsign hash table that outlives the slot. Both are decided:
/// the first is the refusal HM-DEC-009 requires and unit 217 is forbidden to overturn, and the second
/// is unit 208's ruling that the cache is never a static singleton. <b>They are recorded as
/// divergences and measured, never changed.</b>
/// </para>
/// </remarks>
public class UpstreamMessageLayerInventoryTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamMessageLayerInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 217 is licensed to read for shapes, and no others.</summary>
    private static readonly string[] MessageLayerSources =
    {
        @"ft8\message.h", @"ft8\message.c", @"demo\decode_ft8.c",
    };

    /// <summary>
    /// Discovery, and it runs because assuming which file holds the message layer is exactly the
    /// mistake this project has paid for before.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheMessageLayerFilesAreFoundRatherThanAssumed()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone: {location}");

        foreach (var relative in MessageLayerSources)
        {
            var path = Path.Combine(location, relative);
            Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
            _output.WriteLine($"  {relative}: {File.ReadAllLines(path).Length} lines");
        }
    }

    // ------------------------------------------------------------------------------------------
    // QUESTION 1 — what upstream writes for a hashed callsign it cannot resolve.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// <b>QUESTION 1, AND IT IS THE ANCHOR FOR THE PRICE OF FIX C.</b> Upstream writes the literal
    /// six characters <c>&lt;...&gt;</c> into the callsign field and <b>reports the message as
    /// successfully decoded anyway.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Three separate facts, and the third is the one that matters.</b> First,
    /// <c>lookup_callsign</c> writes <c>"&lt;...&gt;"</c> into its output buffer when the hash
    /// interface cannot find the hash. Second, it returns <c>found</c>, which is <c>false</c> in that
    /// case, so the information is available to its caller. <b>Third, neither caller looks.</b>
    /// <c>unpack28</c> calls it and then unconditionally <c>return 0; // Success</c>;
    /// <c>ftx_message_decode_nonstd</c> calls it as a bare statement and returns
    /// <c>FTX_MESSAGE_RC_OK</c>. So a message naming a station upstream cannot name is printed on
    /// upstream's list with <c>&lt;...&gt;</c> where the callsign would be, and its return code says
    /// nothing went wrong.
    /// </para>
    /// <para>
    /// <b>THIS LIBRARY DOES THE OPPOSITE ON PURPOSE AND THAT IS NOT UNDER REVIEW HERE.</b> A message
    /// whose hashed callsign nothing in the slot resolves is refused whole, as
    /// <c>Ft8DecodeStatus.UnresolvedCallsign</c>, with no placeholder written and no partial message
    /// returned. That is HM-DEC-009 and a numbered divergence, and whether a decoder may ever tell an
    /// operator <em>a station I cannot name</em> is reserved to the owner by <c>CLAUDE.md</c> §12.1.
    /// This test measures the difference; it does not close it.
    /// </para>
    /// <para>
    /// <b>Anchoring: WEAK.</b> All three facts are expressions inside static function bodies in
    /// <c>ft8/message.c</c>. The string literal itself is unambiguous — there is exactly one
    /// occurrence of it in the file — but nothing in a header declares it, so a re-pin could change
    /// the placeholder without touching an interface.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamWritesTheLiteralUnresolvedPlaceholderAndCallsTheDecodeSuccessful()
    {
        var message = ReadSource(@"ft8\message.c");

        var occurrences = Regex.Matches(message, @"""<\.\.\.>""").Count;
        _output.WriteLine($"  occurrences of the \"<...>\" literal in ft8/message.c: {occurrences}");
        Assert.Equal(1, occurrences);

        var lookup = Body(message, "lookup_callsign");
        Assert.Contains(@"strcpy(callsign, ""<...>"");", lookup);
        Assert.Contains("return found;", lookup);
        _output.WriteLine("  lookup_callsign writes the placeholder on a miss AND returns the miss to its caller.");

        // AND NEITHER CALLER READS IT. This is the whole finding: the information exists and is
        // discarded twice, which is why upstream's lists carry lines this library refuses.
        var unpack28 = Body(message, "unpack28");
        var hashBranch = Regex.Match(
            Collapse(unpack28),
            @"lookup_callsign\(hash_if, FTX_CALLSIGN_HASH_22_BITS, n28, result\); \*field_type = FTX_FIELD_CALL; return 0;");
        Assert.True(
            hashBranch.Success,
            "unpack28 no longer discards lookup_callsign's return and report success regardless.");
        _output.WriteLine("  unpack28 discards it and returns 0 (success) regardless.");

        var nonstd = Body(message, "ftx_message_decode_nonstd");
        Assert.Contains("lookup_callsign(hash_if, FTX_CALLSIGN_HASH_12_BITS, n12, call_3);", Collapse(nonstd));
        Assert.Contains("return FTX_MESSAGE_RC_OK;", nonstd);
        Assert.DoesNotContain("FTX_MESSAGE_RC_ERROR_CALLSIGN", nonstd);
        _output.WriteLine("  ftx_message_decode_nonstd discards it too and returns FTX_MESSAGE_RC_OK.");

        // add_brackets is what a RESOLVED hash gets, and it is why upstream's resolved lines read
        // <CALL> with the angle brackets kept. This library keeps them too, so the two agree here.
        var brackets = Body(message, "add_brackets");
        Assert.Contains("result[0] = '<';", brackets);
        Assert.Contains("result[length + 1] = '>';", brackets);
        _output.WriteLine("  a RESOLVED hash is bracketed by add_brackets, so <CALL> is upstream's form too.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  ANCHORING: WEAK. All of it is inside static function bodies in ft8/message.c.");
        _output.WriteLine("  THE DIVERGENCE THIS MEASURES IS NOT UNDER REVIEW: this library refuses the whole");
        _output.WriteLine("  message instead, HM-DEC-009, and the placeholder is forbidden. What it costs");
        _output.WriteLine("  criterion 3 is measured elsewhere in this unit and handed to the owner.");
    }

    // ------------------------------------------------------------------------------------------
    // QUESTION 2 — which types upstream actually prints, and what it does with one it cannot read.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// <b>QUESTION 2, AND IT DECIDES TASK 7.</b> Upstream's own message layer decodes <b>exactly
    /// four</b> of the eleven types its enumeration names — and they are exactly the four this
    /// library builds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The switch in <c>ftx_message_decode</c> has four cases and a default.</b> The cases are
    /// <c>FTX_MESSAGE_TYPE_STANDARD</c> (type codes 1 and 2), <c>FTX_MESSAGE_TYPE_NONSTD_CALL</c>
    /// (code 4), <c>FTX_MESSAGE_TYPE_FREE_TEXT</c> (code 0.0) and <c>FTX_MESSAGE_TYPE_TELEMETRY</c>
    /// (code 0.5). The default branch carries the comment <c>// not handled yet</c> and returns
    /// <c>FTX_MESSAGE_RC_ERROR_TYPE</c>.
    /// </para>
    /// <para>
    /// <b>So the six types <c>HM-OPEN-064</c> records as not built are not built upstream either.</b>
    /// 0.1 DXpedition, 0.2 EU VHF, 0.3/0.4 ARRL Field Day, 0.6 Contesting, 3 ARRL RTTY Roundup and 5
    /// WWROF are declared in <c>ftx_message_type_t</c> and none of them has a decode. <b>That is a
    /// mismatch against work instruction 217, reported and not repaired:</b> the instruction says
    /// task 7's field layouts <em>come from the pin's <c>ft8/message.c</c>, read the way units 207 and
    /// 208 read it</em>, and there is nothing there to read — building them would be writing new
    /// protocol work against the QEX paper's table rather than porting, with no upstream behaviour to
    /// be faithful to and no round-trip oracle on the other side.
    /// </para>
    /// <para>
    /// <b>And what the demo does with one is worse than skipping it, which matters for reading the
    /// expected lists.</b> On any non-OK return code it does not drop the line: it
    /// <c>snprintf</c>s <c>"Error [%d] while unpacking!"</c> into the text buffer and prints that on
    /// the message line. So a list written by this decoder would carry a visible error string for
    /// every unsupported type, and none of the sixty expected lists in the pin carries one — which is
    /// one more independent confirmation of unit 216's finding that these lists were not written by
    /// the pinned decoder.
    /// </para>
    /// <para>
    /// <b>Anchoring: STRONG for the type and return-code enumerations</b>, which are declared in
    /// <c>ft8/message.h</c>; <b>WEAK for the dispatch itself</b>, which is a switch inside a function
    /// body, though the function is a public entry point declared in that same header; <b>WEAKEST for
    /// the error string</b>, which is one <c>snprintf</c> inside the demo application's own static
    /// <c>decode</c> helper and is one application's judgement rather than a property of FT8.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamDecodesExactlyFourOfTheElevenTypesItNames()
    {
        var header = ReadSource(@"ft8\message.h");
        var message = ReadSource(@"ft8\message.c");
        var demo = ReadSource(@"demo\decode_ft8.c");

        // STRONG — the enumeration is a declaration in the header.
        var declared = Regex.Matches(header, @"FTX_MESSAGE_TYPE_([A-Z0-9_]+)")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
        _output.WriteLine($"  types DECLARED in ft8/message.h ({declared.Length}):");
        foreach (var name in declared)
        {
            _output.WriteLine($"    FTX_MESSAGE_TYPE_{name}");
        }

        // WEAK — the dispatch is a switch inside ftx_message_decode's body, but the function is
        // declared in the header, so the boundary is public even where the branch list is not.
        var decode = Collapse(Body(message, "ftx_message_decode"));
        var handled = Regex.Matches(decode, @"case FTX_MESSAGE_TYPE_([A-Z0-9_]+):")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  types ftx_message_decode actually DECODES ({handled.Length}):");
        foreach (var name in handled)
        {
            _output.WriteLine($"    FTX_MESSAGE_TYPE_{name}");
        }

        Assert.Equal(
            new[] { "FREE_TEXT", "NONSTD_CALL", "STANDARD", "TELEMETRY" },
            handled);

        _output.WriteLine(string.Empty);
        _output.WriteLine("  types DECLARED AND NOT DECODED anywhere in upstream's message layer:");
        foreach (var name in declared.Except(handled, StringComparer.Ordinal).OrderBy(n => n, StringComparer.Ordinal))
        {
            _output.WriteLine($"    FTX_MESSAGE_TYPE_{name}");
        }

        // The default branch, and its comment, which says in upstream's own words that this is
        // unfinished work rather than a deliberate exclusion.
        Assert.Contains("default: // not handled yet field1 = NULL; rc = FTX_MESSAGE_RC_ERROR_TYPE;", decode);
        _output.WriteLine(string.Empty);
        _output.WriteLine("  the default branch reads: // not handled yet -> FTX_MESSAGE_RC_ERROR_TYPE");

        // WEAKEST — one snprintf in the demo's own static decode helper.
        var demoDecode = Collapse(Body(demo, "decode"));
        Assert.Contains(@"if (unpack_status != FTX_MESSAGE_RC_OK) { snprintf(text, sizeof(text), ""Error [%d] while unpacking!"", (int)unpack_status); }", demoDecode);
        _output.WriteLine("  and the demo PRINTS THE LINE ANYWAY, with \"Error [%d] while unpacking!\" as its text.");
        _output.WriteLine("  So a list written by the pinned decoder would show that string for every");
        _output.WriteLine("  unsupported type. None of the sixty expected lists carries it.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  MISMATCH AGAINST INSTRUCTION 217, REPORTED AND NOT REPAIRED: task 7's field");
        _output.WriteLine("  layouts are said to come from the pin's ft8/message.c. They are not there.");
        _output.WriteLine("  Upstream declares those six types and decodes none of them.");
    }

    /// <summary>
    /// The six values <c>ftx_message_rc_t</c> declares — success and five ways to fail — so the
    /// census's three refusal statuses can be read against upstream's own vocabulary rather than
    /// against a guess at it.
    /// </summary>
    /// <remarks>
    /// <b>Upstream has five failures and this library has three, and the mapping is not one to
    /// one.</b>
    /// Upstream separates a bad first callsign from a bad second one and a bad suffix from a bad grid;
    /// this library folds all four into <c>MalformedField</c>, because the caller's question is
    /// whether the message can be shown and not which field spoiled it. <c>ERROR_TYPE</c> maps to
    /// <c>UnsupportedType</c>. <b>Upstream has no counterpart at all for
    /// <c>UnresolvedCallsign</c></b> — that state is what question 1 measures, and upstream reports
    /// it as <c>RC_OK</c> with a placeholder in the text.
    /// <para><b>Anchoring: STRONG.</b> A declaration in <c>ft8/message.h</c>.</para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamsReturnCodesHaveNoCounterpartForAnUnresolvedHash()
    {
        var header = ReadSource(@"ft8\message.h");

        var block = Regex.Match(header, @"typedef\s+enum[^}]*\}\s*ftx_message_rc_t\s*;", RegexOptions.Singleline);
        Assert.True(block.Success, "ftx_message_rc_t is no longer an enum in ft8/message.h.");

        var codes = Regex.Matches(block.Value, @"FTX_MESSAGE_RC_([A-Z0-9_]+)")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        _output.WriteLine($"  ftx_message_rc_t declares {codes.Length}:");
        foreach (var code in codes)
        {
            _output.WriteLine($"    FTX_MESSAGE_RC_{code}");
        }

        Assert.Equal(
            new[] { "OK", "ERROR_CALLSIGN1", "ERROR_CALLSIGN2", "ERROR_SUFFIX", "ERROR_GRID", "ERROR_TYPE" },
            codes);

        // NOT ONE OF THEM SAYS "a hash I could not resolve". That is the point of this test.
        Assert.DoesNotContain(codes, c => c.Contains("HASH", StringComparison.Ordinal));
        Assert.DoesNotContain(codes, c => c.Contains("UNRESOLVED", StringComparison.Ordinal));

        _output.WriteLine(string.Empty);
        _output.WriteLine("  NONE of them names an unresolved hash. Upstream returns RC_OK with <...> in");
        _output.WriteLine("  the text, so this library's UnresolvedCallsign has no upstream counterpart at");
        _output.WriteLine("  all - it is the divergence and not a renaming of one of these.");
        _output.WriteLine("  ANCHORING: STRONG. A declaration in ft8/message.h.");
    }

    // ------------------------------------------------------------------------------------------
    // QUESTION 3 — does upstream ever re-offer a refused payload? ANSWERED PLAINLY: no.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// <b>QUESTION 3, ANSWERED PLAINLY: NO. Upstream is strictly one pass in score order and never
    /// re-offers a payload.</b> So the second pass this unit measures is an <b>addition</b> and a
    /// numbered divergence, not a port.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The shape of the demo's <c>decode</c> helper settles it.</b> It calls
    /// <c>ftx_find_candidates</c> once, walks the candidate list in one <c>for</c> loop, and calls
    /// <c>ftx_message_decode</c> exactly once — inside that loop, in the <c>found_empty_slot</c>
    /// branch, immediately after a new payload is entered in the duplicate table. There is exactly
    /// <b>one</b> call to <c>ftx_message_decode</c> in the whole application, there is no second loop
    /// over the candidates or over the decoded payloads, and nothing is retained past the printf.
    /// A payload whose hash was unresolvable when its turn came is printed with <c>&lt;...&gt;</c> and
    /// never looked at again.
    /// </para>
    /// <para>
    /// <b>Why upstream does not need a second pass and this library does.</b> Upstream never refuses
    /// for an unresolved hash at all — question 1 — so a payload it could not resolve still reaches
    /// its list, degraded. This library refuses instead, so for it the ordering is the difference
    /// between a message and nothing. <b>That is why the second pass is licensed here and is not a
    /// weakening of anything:</b> the hash must still match a callsign heard spelled out in the same
    /// slot, and a hash nothing matches is still refused.
    /// </para>
    /// <para>
    /// <b>Anchoring: WEAK, and weakest of the four.</b> This is the shape of a static
    /// <c>decode</c> helper inside <c>demo/decode_ft8.c</c>, an application's own loop, not an
    /// interface. It is asserted by counting call sites so that a re-pin which adds a second pass
    /// goes red.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamIsStrictlyOnePassInScoreOrderAndNeverReOffersAPayload()
    {
        var demo = ReadSource(@"demo\decode_ft8.c");
        var body = Body(demo, "decode");
        var collapsed = Collapse(body);

        var unpackCalls = Regex.Matches(body, @"\bftx_message_decode\s*\(").Count;
        var candidateCalls = Regex.Matches(body, @"\bftx_decode_candidate\s*\(").Count;
        var findCalls = Regex.Matches(body, @"\bftx_find_candidates\s*\(").Count;
        var candidateLoops = Regex.Matches(body, @"for\s*\(\s*int\s+idx\s*=\s*0\s*;").Count;

        _output.WriteLine($"  ftx_find_candidates call sites in decode():  {findCalls}");
        _output.WriteLine($"  candidate loops in decode():                 {candidateLoops}");
        _output.WriteLine($"  ftx_decode_candidate call sites in decode(): {candidateCalls}");
        _output.WriteLine($"  ftx_message_decode call sites in decode():   {unpackCalls}");

        Assert.Equal(1, findCalls);
        Assert.Equal(1, candidateLoops);
        Assert.Equal(1, candidateCalls);
        Assert.Equal(1, unpackCalls);

        // ONE call site in the whole file, not merely in this function.
        var unpackCallsInFile = Regex.Matches(demo, @"\bftx_message_decode\s*\(").Count;
        _output.WriteLine($"  ftx_message_decode call sites in the whole file: {unpackCallsInFile}");
        Assert.Equal(1, unpackCallsInFile);

        // And it sits inside the branch that has just entered a NEW payload in the duplicate table,
        // so the unpack happens at the moment of first sight and at no other moment.
        Assert.Contains("if (found_empty_slot) {", collapsed);
        var atFirstSight = collapsed.IndexOf("if (found_empty_slot) {", StringComparison.Ordinal);
        var unpackAt = collapsed.IndexOf("ftx_message_decode(&message, &hash_if, text, &offsets);", StringComparison.Ordinal);
        Assert.True(atFirstSight >= 0 && unpackAt > atFirstSight, "the unpack has moved out of the first-sight branch.");

        _output.WriteLine(string.Empty);
        _output.WriteLine("  SO THE ANSWER IS NO. One search, one loop over the candidates in score order,");
        _output.WriteLine("  one decode attempt each, and the unpack happens at first sight of a payload and");
        _output.WriteLine("  never again. Nothing is retained past the printf and there is no second pass.");
        _output.WriteLine("  THE SECOND PASS THIS UNIT MEASURES IS THEREFORE AN ADDITION, NOT A PORT, and is");
        _output.WriteLine("  recorded as a numbered divergence if it is built.");
        _output.WriteLine("  ANCHORING: WEAK - a static helper inside the demo application, not an interface.");
    }

    // ------------------------------------------------------------------------------------------
    // QUESTION 4 — how long upstream's callsign hash table lives.
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// <b>QUESTION 4: upstream's callsign hash table is PER-PROCESS and aged across slots — not
    /// per-slot and not per-file.</b> This library's is per-slot by construction under unit 208's
    /// ruling, <b>that ruling stands, and the difference is recorded as a divergence rather than
    /// acted on.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Three facts fix the lifetime.</b> The storage is a file-scope <c>static struct</c> array
    /// <c>callsign_hashtable</c> with a file-scope <c>static int callsign_hashtable_size</c> beside
    /// it, in <c>demo/decode_ft8.c</c>. <c>hashtable_init()</c> is called <b>once</b>, in
    /// <c>main</c>, before the <c>do … while (is_live)</c> slot loop. And <c>hashtable_cleanup(10)</c>
    /// is called at the <b>end of every</b> <c>decode()</c>, which increments an age stored in the top
    /// byte of each entry's hash word and evicts entries older than the argument.
    /// </para>
    /// <para>
    /// <b>So upstream can name a station in slot <em>n</em> from a callsign it heard spelled out up to
    /// ten slots earlier.</b> Two and a half minutes of memory, carried in a mutable global. That is
    /// a real capability and this library does not have it.
    /// </para>
    /// <para>
    /// <b>Why the difference is kept.</b> Unit 208 ruled that the cache is never a static singleton,
    /// because a decode that depends on what some other slot happened to contain is not a decode
    /// anybody can reproduce — and every determinism proof this phase has taken, including unit 216's
    /// five-run comparison, rests on that. <b>The cost is real and is stated rather than hidden:</b>
    /// a hash whose owner was spelled out in an earlier slot is resolvable by upstream and refused
    /// here. Since criterion 3 runs each of the sixty recordings independently, with no ordering
    /// between them, upstream's aged table has no meaning across them either — which is why this is a
    /// divergence to record and not a shortfall to chase tonight.
    /// </para>
    /// <para>
    /// <b>Anchoring: WEAK.</b> A file-scope static and two call sites inside
    /// <c>demo/decode_ft8.c</c>. Nothing in <c>ft8/</c> owns the table at all: the library takes a
    /// <c>ftx_callsign_hash_interface_t</c> of two function pointers and the application supplies the
    /// storage, which is <b>a STRONG structural fact in its own right</b> and is the shape this
    /// library follows.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamsCallsignHashTableIsPerProcessAndAgedAcrossSlots()
    {
        var demo = ReadSource(@"demo\decode_ft8.c");
        var header = ReadSource(@"ft8\message.h");

        // The storage is a file-scope static in the APPLICATION, not in the library.
        Assert.Matches(new Regex(@"^static\s+struct", RegexOptions.Multiline), demo);
        Assert.Matches(new Regex(@"^static\s+int\s+callsign_hashtable_size\s*;", RegexOptions.Multiline), demo);
        _output.WriteLine("  storage:  a file-scope `static struct` array plus `static int callsign_hashtable_size`");
        _output.WriteLine("            in demo/decode_ft8.c. NOTHING IN ft8/ OWNS THE TABLE.");

        // STRONG structural fact: the library only ever sees two function pointers.
        Assert.Contains("bool (*lookup_hash)(ftx_callsign_hash_type_t hash_type, uint32_t hash, char* callsign);", header);
        Assert.Contains("void (*save_hash)(const char* callsign, uint32_t n22);", header);
        _output.WriteLine("            ft8/message.h declares only lookup_hash and save_hash. STRONG.");

        // Initialised ONCE, in main, ahead of the slot loop.
        var main = Body(demo, "main");
        var initInMain = Regex.Matches(main, @"\bhashtable_init\s*\(\s*\)\s*;").Count;
        var initInFile = Regex.Matches(demo, @"\bhashtable_init\s*\(\s*\)\s*;").Count;
        _output.WriteLine($"            hashtable_init() call sites: {initInMain} in main, {initInFile} in the file.");
        Assert.Equal(1, initInMain);
        Assert.Equal(1, initInFile);
        var initAt = main.IndexOf("hashtable_init();", StringComparison.Ordinal);
        Assert.True(initAt >= 0, "hashtable_init is no longer called from main.");
        Assert.Matches(new Regex(@"\bdo\s*\{"), main[initAt..]);
        _output.WriteLine("  lifetime: hashtable_init() is called ONCE, in main, BEFORE the do/while slot loop.");

        // And aged, not cleared, at the end of each slot.
        var decode = Body(demo, "decode");
        var cleanup = Regex.Match(decode, @"hashtable_cleanup\((\d+)\);");
        Assert.True(cleanup.Success, "hashtable_cleanup is no longer called from decode().");
        var maxAge = int.Parse(cleanup.Groups[1].Value);
        _output.WriteLine($"            hashtable_cleanup({maxAge}) is called at the END of every decode().");

        var cleanupBody = Collapse(Body(demo, "hashtable_cleanup"));
        Assert.Contains("uint8_t age = (uint8_t)(callsign_hashtable[idx_hash].hash >> 24);", cleanupBody);
        Assert.Contains("if (age > max_age)", cleanupBody);
        _output.WriteLine($"            an age lives in the top byte of each entry's hash word; entries older");
        _output.WriteLine($"            than {maxAge} slots are evicted and the rest are aged by one.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  SO IT IS PER-PROCESS, WITH ROUGHLY {maxAge} SLOTS OF MEMORY - not per-slot, not per-file.");
        _output.WriteLine("  THIS LIBRARY'S IS PER-SLOT BY CONSTRUCTION and unit 208's ruling stands: the cache");
        _output.WriteLine("  is never a static singleton, because a decode that depends on what some other slot");
        _output.WriteLine("  contained is not reproducible. Recorded as a divergence, and the lifetime is NOT");
        _output.WriteLine("  changed. Criterion 3 runs the sixty recordings independently and in no order, so");
        _output.WriteLine("  upstream's aged table would have no meaning across them either.");
        _output.WriteLine("  ANCHORING: WEAK for the table (a static in the demo); STRONG for the interface.");

        // 10 is upstream's own argument. Asserted so a re-pin that changes the memory span goes red.
        Assert.Equal(10, maxAge);
    }

    // ------------------------------------------------------------------------------------------

    private static string Collapse(string source) => Regex.Replace(source, @"\s+", " ").Trim();

    private string ReadSource(string relative)
    {
        var path = Path.Combine(RequireReachableClone(), relative);
        Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Pulls one function's body out by brace matching, so an assertion aimed at
    /// <c>ftx_message_decode</c> cannot be satisfied by a line in <c>ftx_message_decode_std</c>
    /// sitting next door. Unit 209 was caught by exactly that shape of mistake.
    /// </summary>
    private static string Body(string source, string name)
    {
        var head = Regex.Match(
            source,
            $@"^[A-Za-z_][A-Za-z0-9_ \t\*]*\b{Regex.Escape(name)}\s*\([^;{{]*\)\s*\{{",
            RegexOptions.Multiline);
        Assert.True(head.Success, $"{name} is no longer defined in the source read.");

        var depth = 0;
        var start = head.Index + head.Length - 1;
        for (var i = start; i < source.Length; i++)
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
                    return source[start..(i + 1)];
                }
            }
        }

        Assert.Fail($"{name}'s body does not close.");
        return string.Empty;
    }

    private string RequireReachableClone()
    {
        if (ReferenceClone.Probe(out var detail) == ReferenceClone.Reach.PresentButUnreadable)
        {
            Assert.Fail(
                $"{ReferenceClone.Location} exists but the test process could not read it: {detail}. "
                + "There is no other route to the pinned source, so nothing can be read tonight.");
        }

        return ReferenceClone.Location;
    }
}
