using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// The sanctioned read of the pinned clone for unit 215: how upstream corrects a damaged FT8
/// codeword, which of its two decoders its own decode path actually calls, where the iteration
/// bound really comes from, where the CRC sits relative to the LDPC decode — and, the reading
/// this whole night turns on, <b>which sign of a log-likelihood ratio means which bit.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>Read it before porting it, and leave behind something that fails loudly if a re-pin
/// changes it.</b> Every shape asserted here is a shape <c>src/Ft8Sharp/Ldpc/LdpcDecoder.cs</c>
/// was written against. This file is the record of the read alone; the assertions that bind
/// these shapes to the port's own constants live beside the port, so that this one compiles and
/// answers on its own.
/// </para>
/// <para>
/// <b>Shapes and counts, never values that are upstream's to own.</b> Identifiers, presences,
/// guard conditions and structural facts are asserted and printed; nothing from the clone is
/// committed.
/// </para>
/// <para>
/// <b>THE SIGN CONVENTION, AND WHY IT IS ASSERTED FROM THREE PLACES RATHER THAN ONE.</b>
/// A decoder whose convention is backwards round-trips perfectly against ratios its own tests
/// generated and goes deaf on the first real signal, so the reading cannot rest on one comment.
/// It rests on three independent things in upstream's source, and
/// <see cref="TheSignConventionIsPositiveMeansOneAndUpstreamsOwnHeaderCommentSaysTheOpposite"/>
/// asserts all three — including the fourth thing, which is that <c>ft8/ldpc.c</c>'s own opening
/// comment states the <em>opposite</em> convention and is wrong. That contradiction is checked in
/// rather than remembered, because it is exactly the sort of thing a later reader would take on
/// trust.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone stays green.</para>
/// </remarks>
public class UpstreamLdpcDecoderInventoryTests
{
    private readonly ITestOutputHelper _output;

    public UpstreamLdpcDecoderInventoryTests(ITestOutputHelper output) => _output = output;

    /// <summary>The files unit 215 is licensed to read for shapes, and no others.</summary>
    private static readonly string[] DecoderSources =
    {
        @"ft8\ldpc.h", @"ft8\ldpc.c", @"ft8\decode.h", @"ft8\decode.c", @"ft8\constants.h",
        @"demo\decode_ft8.c",
    };

    /// <summary>
    /// Discovery, and it runs because assuming which file holds the decoder is exactly the
    /// mistake this project has paid for before.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheDecodersFilesAreFoundRatherThanAssumed()
    {
        var location = RequireReachableClone();
        _output.WriteLine($"clone: {location}");

        foreach (var relative in DecoderSources)
        {
            var path = Path.Combine(location, relative);
            Assert.True(File.Exists(path), $"the pin no longer holds {relative}.");
            _output.WriteLine($"  {relative}: {File.ReadAllLines(path).Length} lines");
        }
    }

    /// <summary>
    /// STRONG — <c>ft8/ldpc.h</c> declares <b>two</b> decoders, not one, and they share a
    /// signature.
    /// </summary>
    /// <remarks>
    /// The arbiter's instruction said there may be more than one and to say so if there is.
    /// There is. Both take an array of ratios, a maximum iteration count, an output bit array
    /// and an out-parameter for the residual error count. <b>Only one of them is ported</b> —
    /// see <see cref="TheDecodePathCallsBpDecodeAndTheOtherDecoderIsCommentedOutBesideIt"/>.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void UpstreamDeclaresTwoDecodersAndBothHaveTheSameSignatureShape()
    {
        var header = ReadSource(@"ft8\ldpc.h");

        var declarations = Regex.Matches(
            header,
            @"void\s+(\w+)\s*\(\s*float\s+\w+\s*\[\s*\]\s*,\s*int\s+\w+\s*,\s*uint8_t\s+\w+\s*\[\s*\]\s*,\s*int\s*\*\s*\w+\s*\)\s*;");

        var names = declarations.Select(m => m.Groups[1].Value).OrderBy(n => n, StringComparer.Ordinal).ToArray();

        _output.WriteLine($"decoders declared in ft8\\ldpc.h : {names.Length}");
        foreach (var name in names)
        {
            _output.WriteLine($"  {name}(float[], int, uint8_t[], int*)");
        }

        Assert.Equal(2, names.Length);
        Assert.Contains("bp_decode", names);
        Assert.Contains("ldpc_decode", names);
    }

    /// <summary>
    /// WEAK — upstream's own decode path calls <c>bp_decode</c>, and the call to the other
    /// decoder is on the line below it, commented out.
    /// </summary>
    /// <remarks>
    /// <b>This is the reading that decided which of the two was ported.</b> The instruction said
    /// to read which one <c>ft8/decode.c</c> actually calls rather than choosing on the strength
    /// of a name, and the answer is unambiguous: one live call, one commented-out call to the
    /// other, adjacent, in <c>ftx_decode_candidate</c>. <c>ldpc_decode</c> was read and
    /// deliberately not ported; it is the same sum-product algorithm carrying two dense
    /// <c>[83][174]</c> float matrices — about 120 kB of them — where <c>bp_decode</c> carries
    /// only the graph's own edges.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheDecodePathCallsBpDecodeAndTheOtherDecoderIsCommentedOutBesideIt()
    {
        var raw = ReadSource(@"ft8\decode.c");
        var live = StripCommentsAndLiterals(raw);

        var liveBp = Regex.Matches(live, @"\bbp_decode\s*\(").Count;
        var liveLdpc = Regex.Matches(live, @"\bldpc_decode\s*\(").Count;
        var commentedLdpc = Regex.Matches(raw, @"//\s*ldpc_decode\s*\(").Count;

        _output.WriteLine($"live bp_decode calls in ft8\\decode.c      : {liveBp}");
        _output.WriteLine($"live ldpc_decode calls in ft8\\decode.c    : {liveLdpc}");
        _output.WriteLine($"commented-out ldpc_decode calls           : {commentedLdpc}");

        Assert.Equal(1, liveBp);
        Assert.Equal(0, liveLdpc);
        Assert.Equal(1, commentedLdpc);
    }

    /// <summary>
    /// <b>THE READING THE NIGHT TURNS ON.</b> Positive means the bit is <b>1</b>; and
    /// <c>ft8/ldpc.c</c>'s own opening comment says the opposite and is wrong.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Four things are asserted, three of them agreeing and one of them dissenting.
    /// </para>
    /// <list type="number">
    ///   <item><description>
    ///     <b>Extraction, in <c>ft8/decode.c</c>.</b> The routine that produces the ratios says in
    ///     its own comment that it computes <c>log(p(1) / p(0))</c>, and its arithmetic is
    ///     <c>max_one - max_zero</c> — larger when the tones that would carry a 1 hold more
    ///     energy. <b>This is the strongest of the three</b>, because it is what the next unit's
    ///     extraction has to reproduce and because the comment and the arithmetic agree.
    ///   </description></item>
    ///   <item><description>
    ///     <b>The hard decision, in both decoders.</b> <c>&gt; 0 ? 1 : 0</c> — a positive total
    ///     becomes the bit 1.
    ///   </description></item>
    ///   <item><description>
    ///     <b>The check-node update, in <c>bp_decode</c>.</b> <c>-2 * atanh(prod tanh(-T/2))</c>.
    ///     Write <c>L</c> for <c>log(P(0)/P(1))</c>; the sum-product check rule in that convention
    ///     is <c>+2 atanh(prod tanh(L/2))</c>. Substituting <c>L = -λ</c> for
    ///     <c>λ = log(P(1)/P(0))</c> turns it into exactly upstream's expression, negations and
    ///     all. <b>The two extra minus signs are not decoration; they are the convention.</b>
    ///   </description></item>
    ///   <item><description>
    ///     <b>And the dissent.</b> <c>ft8/ldpc.c</c> opens by saying the input is the
    ///     "log-likelihood of zero" and writes <c>codeword[i] = log ( P(x=0) / P(x=1) )</c>.
    ///     That is the other convention and it contradicts all three of the above.
    ///     <b>The code wins over the comment</b>, and the comment is asserted present so a
    ///     re-pin that corrects it goes red here rather than silently removing the trap.
    ///   </description></item>
    /// </list>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheSignConventionIsPositiveMeansOneAndUpstreamsOwnHeaderCommentSaysTheOpposite()
    {
        var decode = ReadSource(@"ft8\decode.c");
        var ldpc = ReadSource(@"ft8\ldpc.c");

        // 1. Extraction states and computes log(p(1)/p(0)).
        var extractionComment = Regex.IsMatch(decode, @"log\s*likelihood\s*log\s*\(\s*p\(1\)\s*/\s*p\(0\)\s*\)");
        var extractionArithmetic = Regex.IsMatch(decode, @"log174\s*\[[^\]]+\]\s*=\s*max_one\s*-\s*max_zero\s*;");

        // 2. The hard decision in each decoder maps positive to 1.
        var hardDecisions = Regex.Matches(ldpc, @">\s*0\s*\)\s*\?\s*1\s*:\s*0\s*;").Count;

        // 3. The check-node update carries both negations.
        var toCheck = Regex.IsMatch(ldpc, @"fast_tanh\s*\(\s*-\s*\w+\s*/\s*2\s*\)");
        var toVariable = Regex.IsMatch(ldpc, @"=\s*-\s*2\s*\*\s*fast_atanh\s*\(");

        // 4. The dissenting comment at the top of ldpc.c.
        var dissent = Regex.IsMatch(ldpc, @"codeword\[i\]\s*=\s*log\s*\(\s*P\(x=0\)\s*/\s*P\(x=1\)\s*\)");
        var dissentProse = ldpc.Contains("log-likelihood of zero", StringComparison.Ordinal);

        _output.WriteLine("AGREEING, and the port follows these:");
        _output.WriteLine($"  decode.c comment 'log(p(1) / p(0))'          : {extractionComment}");
        _output.WriteLine($"  decode.c arithmetic 'max_one - max_zero'     : {extractionArithmetic}");
        _output.WriteLine($"  hard decisions of the form '> 0 ? 1 : 0'     : {hardDecisions}");
        _output.WriteLine($"  check update tanh(-T/2)                      : {toCheck}");
        _output.WriteLine($"  check update -2 * atanh(...)                 : {toVariable}");
        _output.WriteLine("DISSENTING, and the port does NOT follow these:");
        _output.WriteLine($"  ldpc.c 'codeword[i] = log ( P(x=0)/P(x=1) )' : {dissent}");
        _output.WriteLine($"  ldpc.c prose 'log-likelihood of zero'        : {dissentProse}");
        _output.WriteLine("READING: POSITIVE RATIO MEANS THE BIT IS 1. NEGATIVE MEANS 0.");

        Assert.True(extractionComment, "decode.c no longer states log(p(1)/p(0)) at the extraction.");
        Assert.True(extractionArithmetic, "decode.c no longer computes max_one - max_zero.");
        Assert.Equal(2, hardDecisions);
        Assert.True(toCheck, "bp_decode's variable-to-check message no longer negates before tanh.");
        Assert.True(toVariable, "bp_decode's check-to-variable message no longer negates the atanh.");
        Assert.True(
            dissent && dissentProse,
            "ft8/ldpc.c's opening comment no longer states the opposite convention. If the pin has "
            + "been corrected upstream that is good news, but the port's reading must be re-taken "
            + "rather than assumed to be unaffected.");
    }

    /// <summary>
    /// WEAK — where <c>bp_decode</c> stops: on a satisfied hard decision, on an all-zero hard
    /// decision, or on the iteration count running out.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three facts, and each of them changes the port:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>The early exit tests the parity of the hard decision</b>, not a message-passing
    ///     residual: <c>ldpc_check(plain)</c> counts unsatisfied checks over the bits just
    ///     decided, and zero breaks the loop.
    ///   </description></item>
    ///   <item><description>
    ///     <b>The check happens at the top of the iteration, before any message is sent.</b> So
    ///     iteration 0 evaluates the raw ratios with no correction applied at all, and a maximum
    ///     of zero iterations leaves the output array untouched.
    ///   </description></item>
    ///   <item><description>
    ///     <b>An all-zero hard decision breaks out.</b> The all-zero word is a valid codeword of
    ///     any linear code and would otherwise satisfy every check, so it is refused as a decode
    ///     rather than returned as one.
    ///   </description></item>
    /// </list>
    /// <para>
    /// And the returned status: <c>*ok = min_errors</c>, the <em>running minimum</em> over the
    /// iterations, initialised to <c>FTX_LDPC_M</c>. <b>Zero means success.</b>
    /// <c>ft8/ldpc.h</c>'s comment says <c>ok == 87 means success</c>, which is wrong twice over:
    /// success is zero, and 87 is not <c>FTX_LDPC_M</c> either.
    /// </para>
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void BpDecodeStopsOnASatisfiedHardDecisionAnOtherwiseAllZeroOneOrTheIterationBound()
    {
        var ldpc = ReadSource(@"ft8\ldpc.c");
        var body = FunctionBody(ldpc, "bp_decode");
        var header = ReadSource(@"ft8\ldpc.h");

        var parityOfHardDecision = Regex.IsMatch(body, @"int\s+errors\s*=\s*ldpc_check\s*\(\s*plain\s*\)\s*;");
        var breaksOnZero = Regex.IsMatch(body, @"errors\s*==\s*0[\s\S]{0,80}?break\s*;");
        var breaksOnAllZeroWord = Regex.IsMatch(body, @"plain_sum\s*==\s*0[\s\S]{0,120}?break\s*;");
        var tracksMinimum = Regex.IsMatch(body, @"errors\s*<\s*min_errors[\s\S]{0,120}?min_errors\s*=\s*errors\s*;");
        var returnsMinimum = Regex.IsMatch(body, @"\*\s*ok\s*=\s*min_errors\s*;");
        var minimumStartsAtM = Regex.IsMatch(body, @"int\s+min_errors\s*=\s*FTX_LDPC_M\s*;");

        var checkAt = body.IndexOf("ldpc_check", StringComparison.Ordinal);

        // The first message SENT, not the first mention of the array: `float toc[..][..];` is the
        // declaration at the top of the function and matching it would compare the check against
        // the wrong thing. This was a defect of my own, caught by the assertion going red against
        // a pin that is entirely correct.
        var firstMessagePass = Regex.Match(body, @"toc\s*\[[^\]]+\]\s*\[[^\]]+\]\s*=");
        var firstMessagePassAt = firstMessagePass.Success ? firstMessagePass.Index : -1;

        var staleOkComment = Regex.IsMatch(header, @"ok\s*==\s*87\s*means\s*success");

        _output.WriteLine($"early exit tests ldpc_check(plain)        : {parityOfHardDecision}");
        _output.WriteLine($"breaks when errors == 0                   : {breaksOnZero}");
        _output.WriteLine($"breaks when the hard decision is all zero : {breaksOnAllZeroWord}");
        _output.WriteLine($"tracks a running minimum of the errors    : {tracksMinimum}");
        _output.WriteLine($"min_errors starts at FTX_LDPC_M           : {minimumStartsAtM}");
        _output.WriteLine($"returns that minimum through *ok          : {returnsMinimum}");
        _output.WriteLine($"parity check precedes the first message   : "
            + $"{checkAt >= 0 && firstMessagePassAt > checkAt} "
            + $"(ldpc_check at {checkAt}, first toc[][] assignment at {firstMessagePassAt})");
        _output.WriteLine($"ldpc.h still claims 'ok == 87 means success' : {staleOkComment}  <-- WRONG, success is 0");

        Assert.True(parityOfHardDecision);
        Assert.True(breaksOnZero);
        Assert.True(breaksOnAllZeroWord);
        Assert.True(tracksMinimum);
        Assert.True(minimumStartsAtM);
        Assert.True(returnsMinimum);
        Assert.True(checkAt >= 0 && firstMessagePassAt > checkAt,
            "bp_decode no longer checks parity before sending its first message of the iteration.");
        Assert.True(staleOkComment,
            "ft8/ldpc.h no longer carries the 'ok == 87 means success' comment. If the pin has been "
            + "corrected upstream the port's reading of the status must be re-taken.");
    }

    /// <summary>
    /// <b>THE ANCHORING THE INSTRUCTION SAID TO WATCH.</b> The maximum iteration count is a
    /// parameter of the library's own entry point — strong — but the value is a file-scope
    /// constant in the demo application and appears nowhere under <c>ft8/</c> — weakest.
    /// </summary>
    /// <remarks>
    /// This is the same shape unit 214 found for the minimum sync score and the candidate limit,
    /// and it has the same consequence: <b>the port exposes it as a parameter with upstream's
    /// value as the default rather than burying it in a loop.</b> A number the application chose
    /// is not a number the library owns.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheMaximumIterationCountBelongsToTheApplicationAndNotToTheLibrary()
    {
        var decodeHeader = ReadSource(@"ft8\decode.h");
        var demo = ReadSource(@"demo\decode_ft8.c");

        var isAParameter = Regex.IsMatch(
            decodeHeader,
            @"bool\s+ftx_decode_candidate\s*\([^)]*\bint\s+max_iterations\b[^)]*\)\s*;");

        var demoConstant = Regex.Match(demo, @"const\s+int\s+kLDPC_iterations\s*=\s*(\d+)\s*;");
        var passedToTheLibrary = Regex.IsMatch(demo, @"ftx_decode_candidate\s*\([^)]*kLDPC_iterations[^)]*\)");

        var underFt8 = 0;
        foreach (var relative in new[] { @"ft8\ldpc.c", @"ft8\ldpc.h", @"ft8\decode.c", @"ft8\decode.h", @"ft8\constants.h" })
        {
            if (ReadSource(relative).Contains("kLDPC_iterations", StringComparison.Ordinal))
            {
                underFt8++;
            }
        }

        _output.WriteLine($"max_iterations is a parameter of ftx_decode_candidate (STRONG) : {isAParameter}");
        _output.WriteLine($"kLDPC_iterations declared in demo\\decode_ft8.c (WEAKEST)      : {demoConstant.Success}");
        _output.WriteLine($"  value the demo chose                                        : "
            + $"{(demoConstant.Success ? demoConstant.Groups[1].Value : "-")}");
        _output.WriteLine($"  and it is passed straight into the library                  : {passedToTheLibrary}");
        _output.WriteLine($"files under ft8/ naming kLDPC_iterations                       : {underFt8}");

        Assert.True(isAParameter, "ftx_decode_candidate no longer takes max_iterations.");
        Assert.True(demoConstant.Success, "the demo no longer declares kLDPC_iterations.");
        Assert.True(passedToTheLibrary);
        Assert.Equal(0, underFt8);
    }

    /// <summary>
    /// WEAK — the CRC is checked <b>after</b> the LDPC decode, never before, and a codeword that
    /// corrects cleanly but whose CRC disagrees is refused rather than returned.
    /// </summary>
    /// <remarks>
    /// <b>This is criterion 2's rule, and it is upstream's own.</b> <c>ftx_decode_candidate</c>
    /// returns false the moment <c>ldpc_errors &gt; 0</c>; only then does it pack the first
    /// <c>FTX_LDPC_K</c> bits, extract the transmitted CRC, recompute it over the zero-extended
    /// payload and return false again on a mismatch. Nothing partial escapes either gate — the
    /// message structure is written only past both. The port composes the same two gates from
    /// <c>Ft8Payload.TryRead</c> rather than writing a second CRC check.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheCrcIsCheckedAfterTheLdpcDecodeAndBothGatesReturnNothing()
    {
        var body = FunctionBody(ReadSource(@"ft8\decode.c"), "ftx_decode_candidate");

        var decodeAt = body.IndexOf("bp_decode", StringComparison.Ordinal);
        var ldpcGateAt = body.IndexOf("ldpc_errors > 0", StringComparison.Ordinal);
        var packAt = body.IndexOf("pack_bits", StringComparison.Ordinal);
        var extractAt = body.IndexOf("ftx_extract_crc", StringComparison.Ordinal);
        var computeAt = body.IndexOf("ftx_compute_crc", StringComparison.Ordinal);
        var crcGateAt = body.IndexOf("crc_extracted != status->crc_calculated", StringComparison.Ordinal);
        var payloadAt = body.IndexOf("message->payload", StringComparison.Ordinal);
        var returnsTrueAt = body.LastIndexOf("return true", StringComparison.Ordinal);

        var order = new (string What, int Where)[]
        {
            ("bp_decode", decodeAt),
            ("if (ldpc_errors > 0) return false", ldpcGateAt),
            ("pack_bits of the first FTX_LDPC_K bits", packAt),
            ("ftx_extract_crc", extractAt),
            ("ftx_compute_crc", computeAt),
            ("if (crc_extracted != crc_calculated) return false", crcGateAt),
            ("message->payload written", payloadAt),
            ("return true", returnsTrueAt),
        };

        foreach (var (what, where) in order)
        {
            _output.WriteLine($"{where,6}  {what}");
        }

        var falseReturns = Regex.Matches(body, @"return\s+false\s*;").Count;
        _output.WriteLine($"\nreturn false statements in ftx_decode_candidate : {falseReturns}");
        _output.WriteLine("READING: PARITY FIRST, THEN CRC, AND NOTHING IS RETURNED UNLESS BOTH PASS.");

        for (var i = 1; i < order.Length; i++)
        {
            Assert.True(
                order[i].Where > order[i - 1].Where && order[i - 1].Where >= 0,
                $"'{order[i].What}' no longer follows '{order[i - 1].What}' in ftx_decode_candidate.");
        }

        Assert.Equal(2, falseReturns);
    }

    /// <summary>
    /// STRONG — the code's dimensions and both check tables' widths are macros and extern
    /// declarations in <c>ft8/constants.h</c>, so they cannot be misread.
    /// </summary>
    /// <remarks>
    /// Printed against what <c>Ft8Tables</c> already holds, since a mismatch here would mean the
    /// generated tables and the decoder were written against different pins. The numbers
    /// themselves are the published parameters of the FT8 LDPC(174,91) code, in the QEX paper the
    /// NOTICE cites.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheCodesDimensionsAreMacrosInTheHeaderAndAgreeWithTheGeneratedTables()
    {
        var constants = ReadSource(@"ft8\constants.h");

        int Macro(string name)
        {
            var match = Regex.Match(constants, $@"#define\s+{name}\s+\((\d+)\)");
            Assert.True(match.Success, $"{name} is no longer a plain #define in ft8/constants.h.");
            return int.Parse(match.Groups[1].Value);
        }

        var n = Macro("FTX_LDPC_N");
        var k = Macro("FTX_LDPC_K");
        var m = Macro("FTX_LDPC_M");

        var nmWidth = int.Parse(Regex.Match(constants, @"kFTX_LDPC_Nm\[FTX_LDPC_M\]\[(\d+)\]").Groups[1].Value);
        var mnWidth = int.Parse(Regex.Match(constants, @"kFTX_LDPC_Mn\[FTX_LDPC_N\]\[(\d+)\]").Groups[1].Value);

        _output.WriteLine($"FTX_LDPC_N  {n}   vs Ft8Tables.LdpcN  {Ft8Tables.LdpcN}");
        _output.WriteLine($"FTX_LDPC_K  {k}   vs N - M            {Ft8Tables.LdpcN - Ft8Tables.LdpcM}");
        _output.WriteLine($"FTX_LDPC_M  {m}   vs Ft8Tables.LdpcM  {Ft8Tables.LdpcM}");
        _output.WriteLine($"Nm width    {nmWidth}   vs Ft8Tables.LdpcNmRowWidth {Ft8Tables.LdpcNmRowWidth}");
        _output.WriteLine($"Mn width    {mnWidth}   vs Ft8Tables.LdpcMnRowWidth {Ft8Tables.LdpcMnRowWidth}");

        Assert.Equal(Ft8Tables.LdpcN, n);
        Assert.Equal(Ft8Tables.LdpcM, m);
        Assert.Equal(Ft8Tables.LdpcN - Ft8Tables.LdpcM, k);
        Assert.Equal(Ft8Tables.LdpcNmRowWidth, nmWidth);
        Assert.Equal(Ft8Tables.LdpcMnRowWidth, mnWidth);
    }

    /// <summary>
    /// WEAK — the hyperbolic functions are rational approximations with a hard clamp, not the
    /// standard library's, and the port reproduces them rather than improving on them.
    /// </summary>
    /// <remarks>
    /// <b>Unit 212 paid for this lesson on the transmit side.</b> A port that called
    /// <c>MathF.Tanh</c> because it is more accurate would be more accurate and would stop being
    /// a port: the clamp at <c>±4.97</c> is what stops <c>atanh</c> being handed exactly ±1, and
    /// the approximation's error is inside the loop that decides bits. Inheriting Goba's
    /// arithmetic is the ruling in force.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheHyperbolicFunctionsAreRationalApproximationsWithAClampAndNotTheStandardLibrarys()
    {
        var ldpc = ReadSource(@"ft8\ldpc.c");

        var clamp = Regex.Matches(ldpc, @"[<>]\s*-?4\.97f").Count;
        var callsLibm = Regex.IsMatch(ldpc, @"[^_a-zA-Z](tanhf?|atanhf?)\s*\(");
        var tanhIsRational = Regex.IsMatch(FunctionBody(ldpc, "fast_tanh"), @"return\s+a\s*/\s*b\s*;");
        var atanhIsRational = Regex.IsMatch(FunctionBody(ldpc, "fast_atanh"), @"return\s+a\s*/\s*b\s*;");

        _output.WriteLine($"clamp comparisons against 4.97f          : {clamp}");
        _output.WriteLine($"calls the standard tanh/atanh            : {callsLibm}");
        _output.WriteLine($"fast_tanh returns a rational a/b         : {tanhIsRational}");
        _output.WriteLine($"fast_atanh returns a rational a/b        : {atanhIsRational}");

        Assert.Equal(2, clamp);
        Assert.False(callsLibm, "upstream now calls the standard library; the port's approximation must be re-taken.");
        Assert.True(tanhIsRational);
        Assert.True(atanhIsRational);
    }

    /// <summary>
    /// Read and named as belonging to extraction rather than to correction, so the next unit does
    /// not have to find it again: upstream rescales the whole ratio array to a fixed variance
    /// before it reaches the decoder.
    /// </summary>
    /// <remarks>
    /// <b>Not ported tonight, and the omission is deliberate.</b> <c>ftx_normalize_logl</c> runs
    /// in <c>ftx_decode_candidate</c> between extraction and <c>bp_decode</c>, and it scales the
    /// 174 ratios so their variance is a fixed constant — upstream's own comment calls the
    /// coefficient experimentally found. That makes it part of turning a waterfall into ratios,
    /// which is the next unit's whole night, and this unit's decoder takes ratios already on
    /// whatever scale the caller chose. <b>The consequence the next unit needs:</b> the decoder
    /// is not scale-free — <c>tanh</c> and its clamp are not homogeneous — so extraction must
    /// deliver ratios on upstream's scale and not merely with upstream's signs.
    /// </remarks>
    [RequiresReferenceCloneFact]
    public void TheRatiosAreRescaledToAFixedVarianceBeforeTheDecoderSeesThemAndThatIsExtractionsWork()
    {
        var decode = ReadSource(@"ft8\decode.c");
        var body = FunctionBody(decode, "ftx_normalize_logl");
        var caller = FunctionBody(decode, "ftx_decode_candidate");

        var computesVariance = Regex.IsMatch(body, @"variance\s*=");
        var scalesBySqrt = Regex.IsMatch(body, @"norm_factor\s*=\s*sqrtf\s*\(");
        var appliedToEveryBit = Regex.IsMatch(body, @"log174\[i\]\s*\*=\s*norm_factor\s*;");

        var normaliseAt = caller.IndexOf("ftx_normalize_logl", StringComparison.Ordinal);
        var extractAt = caller.IndexOf("_extract_likelihood", StringComparison.Ordinal);
        var decodeAt = caller.IndexOf("bp_decode", StringComparison.Ordinal);

        _output.WriteLine($"computes the variance of the 174 ratios : {computesVariance}");
        _output.WriteLine($"scales by a square root                 : {scalesBySqrt}");
        _output.WriteLine($"applies the factor to every bit         : {appliedToEveryBit}");
        _output.WriteLine($"order in ftx_decode_candidate           : extract {extractAt} < normalise "
            + $"{normaliseAt} < decode {decodeAt}");
        _output.WriteLine("READ AND NOT PORTED: this is extraction's, and the decoder is not scale-free.");

        Assert.True(computesVariance);
        Assert.True(scalesBySqrt);
        Assert.True(appliedToEveryBit);
        Assert.True(extractAt >= 0 && normaliseAt > extractAt && decodeAt > normaliseAt);
    }

    private string ReadSource(string relative) =>
        File.ReadAllText(Path.Combine(RequireReachableClone(), relative));

    /// <summary>The body of a C function, brace-matched from its opening brace.</summary>
    private static string FunctionBody(string source, string name)
    {
        var signature = source.IndexOf(name + "(", StringComparison.Ordinal);
        while (signature >= 0)
        {
            var open = source.IndexOf('{', signature);
            var semicolon = source.IndexOf(';', signature);
            if (open >= 0 && (semicolon < 0 || open < semicolon))
            {
                var depth = 0;
                for (var i = open; i < source.Length; i++)
                {
                    if (source[i] == '{')
                    {
                        depth++;
                    }
                    else if (source[i] == '}' && --depth == 0)
                    {
                        return source[open..(i + 1)];
                    }
                }
            }

            signature = source.IndexOf(name + "(", signature + 1, StringComparison.Ordinal);
        }

        Assert.Fail($"the pin no longer holds a definition of {name}.");
        return string.Empty;
    }

    /// <summary>
    /// Blanks comments and string literals so a commented-out call is not counted as a live one.
    /// Newlines are preserved so nothing shifts onto another line.
    /// </summary>
    private static string StripCommentsAndLiterals(string source)
    {
        var text = new StringBuilder(source.Length);
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] == '/' && i + 1 < source.Length && source[i + 1] == '/')
            {
                while (i < source.Length && source[i] != '\n')
                {
                    i++;
                }

                text.Append('\n');
                continue;
            }

            if (source[i] == '/' && i + 1 < source.Length && source[i + 1] == '*')
            {
                i += 2;
                while (i + 1 < source.Length && !(source[i] == '*' && source[i + 1] == '/'))
                {
                    text.Append(source[i] == '\n' ? '\n' : ' ');
                    i++;
                }

                i++;
                text.Append(' ');
                continue;
            }

            if (source[i] == '"' || source[i] == '\'')
            {
                var quote = source[i];
                i++;
                while (i < source.Length && source[i] != quote)
                {
                    i += source[i] == '\\' ? 2 : 1;
                }

                text.Append('0');
                continue;
            }

            text.Append(source[i]);
        }

        return text.ToString();
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
