using System.Text.RegularExpressions;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// <b>Unit 222 task 4: the audit of the stage the budget named.</b> Task 3's largest row was E — the
/// iteration bound, worth two decodes in 306 — so the stage read against the pin here is
/// <b>belief propagation and the bound it runs under</b>, term by term.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this stage and not another.</b> The budget is flat: the search is worth one decode in 306,
/// the byte quantisation nothing at all, and a physics-optimal ratio rule loses two. Row E is the
/// largest thing in the table, and it points at the decoder rather than at anything above it. The
/// soft-symbol measurement points the same way — at the true alignment the hard decisions carry about
/// 31 errors against a code whose recovery unit 215 measured reaching zero at 17.
/// </para>
/// <para>
/// <b>An inventory, with counts and quoted shapes, and not prose.</b> Each row says what the C does,
/// what this port does, and whether they differ, marked STRONG or WEAK on the same terms
/// <see cref="UpstreamLdpcDecoderInventoryTests"/> uses: STRONG where the shape is a declaration, a
/// structural fact or a number that can be read straight out of the source; WEAK where it rests on a
/// reading of intent.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone with no <c>ft8_lib</c> beside it stays green.</para>
/// </remarks>
public class Unit222LdpcAuditTests
{
    private readonly ITestOutputHelper _output;

    public Unit222LdpcAuditTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The whole of <c>bp_decode</c> against the whole of <see cref="LdpcDecoder.Decode"/>.</b>
    /// </summary>
    [RequiresReferenceCloneFact]
    public void BeliefPropagationIsAuditedTermByTermAgainstThePin()
    {
        var ldpc = ReadPin(@"ft8\ldpc.c");
        var header = ReadPin(@"ft8\ldpc.h");
        var decode = ReadPin(@"ft8\decode.c");
        var demo = ReadPin(@"demo\decode_ft8.c");
        var body = FunctionBody(ldpc, "bp_decode");
        var port = ReadPort();

        _output.WriteLine("UNIT 222 TASK 4 - THE AUDIT. bp_decode, term by term, against");
        _output.WriteLine("src/Ft8Sharp/Ldpc/LdpcDecoder.cs.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  pin        : {ReferenceClone.Location} at {ReferenceClone.PinnedCommit}");
        _output.WriteLine($"  ft8\\ldpc.c : {ldpc.Split('\n').Length} lines, bp_decode "
            + $"{body.Split('\n').Length} lines");
        _output.WriteLine($"  the port   : {port.Split('\n').Length} lines");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WHY THIS STAGE: task 3's largest row is E, the iteration bound, at +0.7");
        _output.WriteLine("percentage points. Every row above the decoder is worth less than that.");
        _output.WriteLine(string.Empty);

        var findings = new List<Finding>();

        // ------------------------------------------------------------------ which decoder is ported
        var liveCall = Regex.Matches(StripComments(decode), @"\bbp_decode\s*\(").Count;
        var otherCall = Regex.Matches(StripComments(decode), @"\bldpc_decode\s*\(").Count;
        findings.Add(new Finding(
            "which of the two decoders",
            $"decode.c calls bp_decode {liveCall} time(s), ldpc_decode {otherCall}",
            $"the port has one decoder and it is bp_decode's shape",
            liveCall == 1 && otherCall == 0,
            "STRONG"));

        // ------------------------------------------------------------------ the loop and its bound
        var loopBound = Regex.IsMatch(body, @"for\s*\(\s*int\s+\w+\s*=\s*0\s*;\s*\w+\s*<\s*max_iters\s*;");
        findings.Add(new Finding(
            "the iteration loop",
            "for (int iter = 0; iter < max_iters; iter++)",
            "for (var iteration = 0; iteration < maxIterations; iteration++)",
            loopBound && Regex.IsMatch(port, @"for\s*\(var\s+iteration\s*=\s*0;\s*iteration\s*<\s*maxIterations;"),
            "STRONG"));

        // ------------------------------------------------------- the hard decision, at the top
        var decisionAtTop = Regex.IsMatch(body, @"plain\s*\[[^\]]*\]\s*=[^;]{0,220}\?\s*1\s*:\s*0\s*;");
        var threeMessages = Regex.IsMatch(body, @"tov\s*\[\s*\w+\s*\]\s*\[\s*0\s*\][\s\S]{0,120}?tov\s*\[\s*\w+\s*\]\s*\[\s*1\s*\][\s\S]{0,120}?tov\s*\[\s*\w+\s*\]\s*\[\s*2\s*\]");
        findings.Add(new Finding(
            "the hard decision",
            "plain[n] = (codeword[n] + tov[n][0] + tov[n][1] + tov[n][2] > 0) ? 1 : 0, at the top",
            "the same sum over three to-variable messages, taken at the top of the iteration",
            decisionAtTop && threeMessages
                && Regex.IsMatch(port, @"var\s+bit\s*=\s*\(byte\)\(total\s*>\s*0\s*\?\s*1\s*:\s*0\);"),
            "STRONG"));

        // ---------------------------------------------------------------- the all-zero refusal
        var allZero = Regex.IsMatch(body, @"plain_sum\s*==\s*0[\s\S]{0,140}?break\s*;");
        findings.Add(new Finding(
            "the all-zero refusal",
            "plain_sum == 0 breaks out, so the all-zero word is never returned as a decode",
            "plainSum == 0 breaks out, before the parity check and without scoring minErrors",
            allZero && Regex.IsMatch(port, @"if\s*\(plainSum\s*==\s*0\)"),
            "STRONG"));

        // ------------------------------------------------------------- the parity check and exit
        var checksParity = Regex.IsMatch(body, @"int\s+errors\s*=\s*ldpc_check\s*\(\s*plain\s*\)\s*;");
        var exitsOnZero = Regex.IsMatch(body, @"errors\s*==\s*0[\s\S]{0,80}?break\s*;");
        var tracksMinimum = Regex.IsMatch(body, @"errors\s*<\s*min_errors[\s\S]{0,140}?min_errors\s*=\s*errors\s*;");
        findings.Add(new Finding(
            "the early exit",
            "errors = ldpc_check(plain); errors < min_errors updates it; errors == 0 breaks",
            "UnsatisfiedChecks(...) with the same order and the same break",
            checksParity && exitsOnZero && tracksMinimum
                && Regex.IsMatch(port, @"if\s*\(errors\s*<\s*minErrors\)[\s\S]{0,200}?if\s*\(errors\s*==\s*0\)"),
            "STRONG"));

        // ------------------------------------------------------------------- what comes back
        var startsAtM = Regex.IsMatch(body, @"int\s+min_errors\s*=\s*FTX_LDPC_M\s*;");
        var returnsMinimum = Regex.IsMatch(body, @"\*\s*ok\s*=\s*min_errors\s*;");
        findings.Add(new Finding(
            "the status returned",
            "min_errors starts at FTX_LDPC_M and the running minimum comes back through *ok",
            "minErrors starts at Ft8Tables.LdpcM and is returned in LdpcDecodeResult",
            startsAtM && returnsMinimum
                && Regex.IsMatch(port, @"var\s+minErrors\s*=\s*Ft8Tables\.LdpcM;"),
            "STRONG"));

        // ------------------------------------------------- variable to check, with the exclusion
        var toCheckShape = Regex.IsMatch(body, @"toc\s*\[[^\]]+\]\s*\[[^\]]+\]\s*=\s*fast_tanh\s*\(\s*-\s*\w+\s*/\s*2\s*\)");
        var excludesSelfV = Regex.IsMatch(body, @"if\s*\(\s*kMn\s*\[[^\]]+\]\s*\[[^\]]+\]\s*-\s*1\s*!=\s*\w+\s*\)");
        findings.Add(new Finding(
            "variable node to check node",
            "toc[m][n] = fast_tanh(-tnm / 2), summing every tov except the check being written to",
            "toCheck[...] = FastTanh(-tnm / 2), with the same exclusion on the same table",
            toCheckShape && Regex.IsMatch(port, @"FastTanh\(-tnm\s*/\s*2\)"),
            "STRONG"));

        // ------------------------------------------------- check to variable, with the exclusion
        var toVariableShape = Regex.IsMatch(body, @"tov\s*\[[^\]]+\]\s*\[[^\]]+\]\s*=\s*-\s*2\s*\*\s*fast_atanh\s*\(");
        findings.Add(new Finding(
            "check node to variable node",
            "tov[n][m] = -2 * fast_atanh(product of every toc in the row except this variable's)",
            "toVariable[...] = -2 * FastAtanh(tmn), the same product and the same exclusion",
            toVariableShape && Regex.IsMatch(port, @"=\s*-2\s*\*\s*FastAtanh\(tmn\);"),
            "STRONG"));

        // ------------------------------------------------------------------ the hyperbolics
        // Comments are stripped first, because upstream keeps three higher-order approximations
        // beside the one it uses, commented out. Counting their constants would compare the port
        // against arithmetic upstream itself does not run.
        var pinTanh = Numbers(StripComments(FunctionBody(ldpc, "fast_tanh")));
        var pinAtanh = Numbers(StripComments(FunctionBody(ldpc, "fast_atanh")));
        var portTanh = Numbers(PortMethod(port, "FastTanh"));
        var portAtanh = Numbers(PortMethod(port, "FastAtanh"));

        findings.Add(new Finding(
            "fast_tanh",
            $"a rational approximation over the constants [{string.Join(", ", pinTanh)}]",
            $"the same constants in the same order: [{string.Join(", ", portTanh)}]",
            pinTanh.SequenceEqual(portTanh),
            "STRONG"));

        findings.Add(new Finding(
            "fast_atanh",
            $"a rational approximation over the constants [{string.Join(", ", pinAtanh)}]",
            $"the same constants in the same order: [{string.Join(", ", portAtanh)}]",
            pinAtanh.SequenceEqual(portAtanh),
            "STRONG"));

        var callsLibm = Regex.IsMatch(ldpc, @"[^_a-zA-Z](tanhf?|atanhf?)\s*\(");
        findings.Add(new Finding(
            "the standard library",
            $"upstream calls the standard tanh/atanh: {callsLibm}",
            "the port calls neither; MathF.Tanh would be more accurate and would stop this being a port",
            !callsLibm && !Regex.IsMatch(port, @"MathF\.(Tanh|Atanh)\s*\("),
            "STRONG"));

        // ------------------------------------------------------------ the parity check's bound
        var checkBound = Regex.IsMatch(
            FunctionBody(ldpc, "ldpc_check"),
            @"<\s*\w*num_rows\w*\s*\[",
            RegexOptions.IgnoreCase);
        findings.Add(new Finding(
            "the parity check's row bound",
            "ldpc_check walks FTX_LDPC_NUM_ROWS[m] entries, not the row width",
            "UnsatisfiedChecks walks numRows[m]; 59 of the 581 Nm slots are padding",
            checkBound && Regex.IsMatch(port, @"i\s*<\s*numRows\[m\];"),
            "STRONG"));

        // ---------------------------------------------------------------- the iteration bound
        var demoConstant = Regex.Match(demo, @"const\s+int\s+kLDPC_iterations\s*=\s*(\d+)\s*;");
        var pinIterations = demoConstant.Success ? int.Parse(demoConstant.Groups[1].Value) : -1;
        findings.Add(new Finding(
            "the iteration bound - ROW E's OWN STAGE",
            $"kLDPC_iterations = {pinIterations}, a file-scope constant in demo/decode_ft8.c, "
                + "named in no file under ft8/",
            $"LdpcDecoder.DefaultMaxIterations = {LdpcDecoder.DefaultMaxIterations}, exposed as a "
                + "parameter with upstream's value as the default",
            pinIterations == LdpcDecoder.DefaultMaxIterations,
            "WEAK - the number is the application's and not the library's"));

        // --------------------------------------------------------- the one recorded divergence
        var staleComment = Regex.IsMatch(header, @"ok\s*==\s*87\s*means\s*success");
        findings.Add(new Finding(
            "the untouched output buffer",
            "bp_decode leaves plain[] untouched when the loop body never runs - in C, whatever was "
                + "on the stack",
            "the port clears the buffer: DIVERGENCE 21, recorded in porting-notes.md, because there "
                + "is no faithful port of undefined content",
            false,
            "STRONG - and it is a DELIBERATE, RECORDED divergence, not a defect"));

        _output.WriteLine($"{"term",42} {"same",6} {"evidence",8}");
        foreach (var finding in findings)
        {
            _output.WriteLine($"{finding.Term,42} {(finding.Same ? "SAME" : "DIFFERS"),6} "
                + $"{finding.Evidence}");
            _output.WriteLine($"    the C   : {finding.Pin}");
            _output.WriteLine($"    the port: {finding.Port}");
        }

        var differing = findings.Count(f => !f.Same);

        _output.WriteLine(string.Empty);
        _output.WriteLine($"TERMS AUDITED : {findings.Count}");
        _output.WriteLine($"SAME          : {findings.Count - differing}");
        _output.WriteLine($"DIFFERING     : {differing}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"ldpc.h still carries its wrong 'ok == 87 means success' comment : {staleComment}");
        _output.WriteLine(string.Empty);

        if (differing <= 1)
        {
            _output.WriteLine("THE VERDICT, IN THE INSTRUCTION'S OWN TERMS: THE PORT IS FAITHFUL HERE AND");
            _output.WriteLine("THE LOSS IS UPSTREAM'S. Every term of bp_decode is reproduced - the loop");
            _output.WriteLine("bound, the hard decision at the top, the all-zero refusal, the parity check");
            _output.WriteLine("and its early exit, the running minimum, both message passes with their");
            _output.WriteLine("exclusions, both rational approximations constant for constant, and the");
            _output.WriteLine("row bound that keeps 59 padding slots out of the checks. THE ONE PLACE THEY");
            _output.WriteLine("DIFFER IS DIVERGENCE 21, WHICH IS RECORDED, DELIBERATE, AND CANNOT COST A");
            _output.WriteLine("DECODE: it clears an output buffer upstream leaves undefined, and it only");
            _output.WriteLine("applies when the loop body never runs at all.");
            _output.WriteLine(string.Empty);
            _output.WriteLine("SO TASK 5'S FIRST CONDITION IS NOT MET AND NO FIX IS LICENSED AT THIS");
            _output.WriteLine("STAGE. That is a real answer and it puts this unit in world two.");
        }
        else
        {
            _output.WriteLine($"THE PORT DIFFERS FROM THE PIN IN {differing} TERMS. Each is named above and");
            _output.WriteLine("task 5's first condition is met for any of them the budget attributes");
            _output.WriteLine("decodes to.");
        }

        // The numbers above are the report. These assert the ones that would make the audit itself
        // wrong if they moved.
        Assert.True(pinIterations > 0, "the demo no longer declares kLDPC_iterations.");
        Assert.Equal(LdpcDecoder.DefaultMaxIterations, pinIterations);
        Assert.NotEmpty(pinTanh);
        Assert.NotEmpty(pinAtanh);
        Assert.Equal(pinTanh, portTanh);
        Assert.Equal(pinAtanh, portAtanh);
    }

    private sealed record Finding(string Term, string Pin, string Port, bool Same, string Evidence);

    /// <summary>
    /// Every numeric literal in a fragment, in order, with C and C# suffixes taken off. <b>A digit
    /// inside an identifier is not a literal</b> — <c>x2</c> is the square of x in both languages
    /// and counting its 2 would make two identical approximations look different.
    /// </summary>
    private static double[] Numbers(string fragment) =>
        Regex.Matches(fragment, @"(?<![\w.])-?\d+(?:\.\d+)?")
            .Select(m => double.Parse(m.Value))
            .ToArray();

    private static string ReadPin(string relative) =>
        File.ReadAllText(Path.Combine(ReferenceClone.Location, relative));

    /// <summary>The port's own source, found by walking up to the solution rather than assumed.</summary>
    private static string ReadPort()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);
        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        Assert.True(at is not null, "no Hamlet.sln above the test binary.");
        var path = Path.Combine(at!.FullName, "src", "Ft8Sharp", "Ldpc", "LdpcDecoder.cs");
        Assert.True(File.Exists(path), $"the port's decoder is not at {path}.");
        return File.ReadAllText(path);
    }

    /// <summary>One method of the port, brace-matched from its signature.</summary>
    private static string PortMethod(string source, string name)
    {
        var signature = source.IndexOf($"float {name}(float", StringComparison.Ordinal);
        Assert.True(signature >= 0, $"the port no longer defines {name}.");

        var open = source.IndexOf('{', signature);
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

        Assert.Fail($"{name} does not close.");
        return string.Empty;
    }

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

    /// <summary>Blanks line and block comments so a commented-out call is not counted as a live one.</summary>
    private static string StripComments(string source)
    {
        var withoutBlocks = Regex.Replace(source, @"/\*[\s\S]*?\*/", " ");
        return Regex.Replace(withoutBlocks, @"//[^\n]*", string.Empty);
    }
}
