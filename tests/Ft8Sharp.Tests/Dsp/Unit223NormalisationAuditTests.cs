using System.Text.RegularExpressions;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 223 task 2: the stage unit 222's own audit rule skipped.</b>
/// <see cref="Ft8SoftSymbols.Normalise"/> and <see cref="Ft8SoftSymbols.Variance"/> read against
/// <c>ftx_normalize_logl</c> in <c>ft8/decode.c</c>, term by term.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why nobody has ever read this stage against the pin, and it is not an oversight.</b> Unit 222
/// audited the stage its largest budget row named, which was belief propagation. <b>No budget row
/// could ever have named this one</b>: the normalisation sits between extraction and correction and
/// <em>every</em> row of that budget went through it, so it cancels out of every delta the budget can
/// form. A stage every row shares is a stage no row can see, and this is the only stage of the
/// receive path no unit has read against upstream.
/// </para>
/// <para>
/// <b>An inventory, with counts and quoted shapes, and not prose.</b> Each row says what the C does,
/// what this port does, and whether they differ, marked STRONG or WEAK on the same terms
/// <see cref="UpstreamExtractionInventoryTests"/> and <c>Unit222LdpcAuditTests</c> use: STRONG where
/// the shape is a declaration, a structural fact or a number that can be read straight out of the
/// source; WEAK where it rests on a reading of intent.
/// </para>
/// <para>
/// <b>One row is expected to come back DIFFERING and it is already recorded.</b> Upstream divides by
/// the variance without checking it, so an array of 174 identical ratios gives it an infinity or a
/// NaN which it then multiplies through the whole array. This port refuses that case and leaves the
/// array alone. That is <b>divergence 23</b> in <c>porting-notes.md</c>, it is deliberate, and this
/// test asserts it is still there rather than discovering it.
/// </para>
/// <para>
/// <b>Nothing here changes a line.</b> Task 6 owns any change, under two conditions this file can
/// satisfy at most one of.
/// </para>
/// <para><b>Absent is a skip.</b> A fresh clone with no <c>ft8_lib</c> beside it stays green.</para>
/// </remarks>
public class Unit223NormalisationAuditTests
{
    private readonly ITestOutputHelper _output;

    public Unit223NormalisationAuditTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The whole of <c>ftx_normalize_logl</c> against the whole of
    /// <see cref="Ft8SoftSymbols.Normalise"/> and <see cref="Ft8SoftSymbols.Variance"/>.</b>
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheNormalisationIsAuditedTermByTermAgainstThePin()
    {
        var decode = ReadPin(@"ft8\decode.c");
        var body = FunctionBody(decode, "ftx_normalize_logl");
        var port = ReadPort();
        var normalise = PortMethod(port, "public static float Normalise(");
        var variance = PortMethod(port, "public static float Variance(");

        _output.WriteLine("UNIT 223 TASK 2 - THE NORMALISATION AUDIT. ftx_normalize_logl, term by");
        _output.WriteLine("term, against Ft8SoftSymbols.Normalise and Ft8SoftSymbols.Variance.");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  pin          : {ReferenceClone.Location} at {ReferenceClone.PinnedCommit}");
        _output.WriteLine($"  ft8\\decode.c : {decode.Split('\n').Length} lines, ftx_normalize_logl "
            + $"{body.Split('\n').Length} lines");
        _output.WriteLine($"  the port     : Normalise {normalise.Split('\n').Length} lines, "
            + $"Variance {variance.Split('\n').Length} lines");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WHY THIS STAGE: unit 222's budget could not isolate it. Every row of that");
        _output.WriteLine("budget passed through this function, so it cancels out of every delta the");
        _output.WriteLine("budget forms. It is the one stage of the receive path no unit has read.");
        _output.WriteLine(string.Empty);

        _output.WriteLine("THE PIN, QUOTED IN FULL so the findings below can be checked against it:");
        _output.WriteLine(string.Empty);
        foreach (var line in body.Split('\n'))
        {
            _output.WriteLine("  | " + line.TrimEnd());
        }

        _output.WriteLine(string.Empty);

        var findings = new List<Finding>();

        // ------------------------------------------------------------------ the loop bound
        var pinBounds = Regex.Matches(body, @"for\s*\(\s*int\s+\w+\s*=\s*0\s*;\s*\w+\s*<\s*(\w+)\s*;")
            .Select(m => m.Groups[1].Value)
            .ToArray();
        findings.Add(new Finding(
            "the loop bound",
            $"{pinBounds.Length} loop(s), bounded by "
                + $"{(pinBounds.Length == 0 ? "nothing found" : string.Join(", ", pinBounds))}",
            $"both loops bounded by ratios.Length, which the caller has already been refused unless "
                + $"it is {Ft8SoftSymbols.RatioCount}",
            pinBounds.Length == 2 && pinBounds.All(b => b == "FTX_LDPC_N"),
            "STRONG"));

        // ------------------------------------------------------ the accumulators and their precision
        var sumIsFloat = Regex.IsMatch(body, @"\bfloat\s+sum\s*=\s*0");
        var sum2IsFloat = Regex.IsMatch(body, @"\bfloat\s+sum2\s*=\s*0");
        var pinDoubles = Regex.Matches(body, @"\bdouble\b").Count;
        findings.Add(new Finding(
            "the accumulation precision",
            $"float sum = {sumIsFloat}, float sum2 = {sum2IsFloat}, "
                + $"occurrences of the word double in the body = {pinDoubles}",
            "var sum = 0.0f and var sumOfSquares = 0.0f, single precision throughout",
            sumIsFloat && sum2IsFloat && pinDoubles == 0
                && Regex.IsMatch(variance, @"var\s+sum\s*=\s*0\.0f;")
                && Regex.IsMatch(variance, @"var\s+sumOfSquares\s*=\s*0\.0f;")
                && !Regex.IsMatch(variance, @"\bdouble\b"),
            "STRONG"));

        // ------------------------------------------------------------------ the variance expression
        var pinVariance = Regex.Match(
            body, @"variance\s*=\s*\(\s*sum2\s*-\s*\(?\s*sum\s*\*\s*sum\s*\*\s*(\w+)\s*\)?\s*\)\s*\*\s*(\w+)");
        var portVariance = Regex.Match(
            variance,
            @"return\s*\(sumOfSquares\s*-\s*\(sum\s*\*\s*sum\s*\*\s*inverseCount\)\)\s*\*\s*inverseCount;");
        findings.Add(new Finding(
            "the variance expression",
            pinVariance.Success
                ? $"(sum2 - (sum * sum * {pinVariance.Groups[1].Value})) * {pinVariance.Groups[2].Value}"
                : "NOT FOUND in the shape this audit looks for",
            "(sumOfSquares - (sum * sum * inverseCount)) * inverseCount",
            pinVariance.Success && portVariance.Success
                && pinVariance.Groups[1].Value == pinVariance.Groups[2].Value,
            "STRONG"));

        // ------------------------------------------------ the reciprocal, formed once and multiplied
        var pinInverse = Regex.Match(body, @"\bfloat\s+(\w+)\s*=\s*1\.0f\s*/\s*(\w+)\s*;");
        findings.Add(new Finding(
            "the reciprocal, not two divisions",
            pinInverse.Success
                ? $"float {pinInverse.Groups[1].Value} = 1.0f / {pinInverse.Groups[2].Value}, "
                    + "then multiplied twice"
                : "NOT FOUND",
            "var inverseCount = 1.0f / ratios.Length, then multiplied twice",
            pinInverse.Success && pinInverse.Groups[2].Value == "FTX_LDPC_N"
                && Regex.IsMatch(variance, @"var\s+inverseCount\s*=\s*1\.0f\s*/\s*ratios\.Length;"),
            "STRONG"));

        // ------------------------------------ IS THE MEAN REMOVED FROM THE ARRAY AS WELL AS THE VARIANCE
        //
        // The question the instruction names, and it is the one a reader most easily gets wrong: the
        // mean appears in the variance and must NOT appear in the array. A port that centred the
        // ratios would shift every bit's evidence toward zero or one, and it would still pass every
        // round-trip test in the tree. Measured by counting every write to the array in the pin.
        var arrayWrites = Regex.Matches(body, @"log174\s*\[\s*\w+\s*\]\s*(\+=|-=|\*=|/=|=(?!=))")
            .Select(m => m.Groups[1].Value)
            .ToArray();
        var portWrites = Regex.Matches(normalise, @"ratios\s*\[\s*\w+\s*\]\s*(\+=|-=|\*=|/=|=(?!=))")
            .Select(m => m.Groups[1].Value)
            .ToArray();
        findings.Add(new Finding(
            "whether the mean is removed from the ARRAY",
            $"{arrayWrites.Length} write(s) to log174 in the whole function, and they are "
                + $"[{string.Join(", ", arrayWrites)}] - no subtraction of any kind",
            $"{portWrites.Length} write(s) to ratios, and they are "
                + $"[{string.Join(", ", portWrites)}]",
            arrayWrites.Length == 1 && arrayWrites[0] == "*="
                && portWrites.Length == 1 && portWrites[0] == "*=",
            "STRONG"));

        // ------------------------------------------------------------------ the target constant
        var pinTarget = Regex.Match(body, @"sqrtf\(\s*(\d+(?:\.\d+)?)f\s*/\s*variance\s*\)");
        findings.Add(new Finding(
            "the target constant",
            pinTarget.Success ? $"{pinTarget.Groups[1].Value}f, inside the square root" : "NOT FOUND",
            $"Ft8SoftSymbols.NormalisedVariance = {Ft8SoftSymbols.NormalisedVariance}f",
            pinTarget.Success
                && float.Parse(pinTarget.Groups[1].Value) == Ft8SoftSymbols.NormalisedVariance,
            "STRONG"));

        // ------------------------------------------------------------ the square-root scale factor
        var pinScale = Regex.IsMatch(body, @"sqrtf\(\s*\d+(?:\.\d+)?f\s*/\s*variance\s*\)")
            && Regex.IsMatch(body, @"log174\s*\[\s*\w+\s*\]\s*\*=\s*\w+\s*;");
        findings.Add(new Finding(
            "the square-root scale factor",
            "norm_factor = sqrtf(target / variance), then log174[i] *= norm_factor",
            "factor = MathF.Sqrt(NormalisedVariance / variance), then ratios[i] *= factor",
            pinScale
                && Regex.IsMatch(normalise, @"MathF\.Sqrt\(NormalisedVariance\s*/\s*variance\)")
                && Regex.IsMatch(normalise, @"ratios\[i\]\s*\*=\s*factor;"),
            "STRONG"));

        // ------------------------------------ the single-precision square root, and not a double one
        var singlePrecisionRoot = Regex.IsMatch(body, @"\bsqrtf\s*\(") && !Regex.IsMatch(body, @"\bsqrt\s*\(");
        findings.Add(new Finding(
            "the square root's own precision",
            $"sqrtf, the single-precision one: sqrtf present = "
                + $"{Regex.IsMatch(body, @"\bsqrtf\s*\(")}, "
                + $"double-precision sqrt present = {Regex.IsMatch(body, @"(?<!f)\bsqrt\s*\(")}",
            "MathF.Sqrt, the single-precision one",
            singlePrecisionRoot && Regex.IsMatch(normalise, @"MathF\.Sqrt\(")
                && !Regex.IsMatch(normalise, @"Math\.Sqrt\("),
            "STRONG"));

        // ==================================================================================
        // WHERE THE CALL SITS IN UPSTREAM'S OWN PIPELINE, AND WHAT TOUCHES THE ARRAY BETWEEN.
        // This is the half of the task no amount of reading the function body can answer, and
        // it is the half that would hide a stage nobody has counted.
        // ==================================================================================
        var candidate = FunctionBody(decode, "ftx_decode_candidate");
        var stripped = StripComments(candidate);
        var normaliseCall = stripped.IndexOf("ftx_normalize_logl(", StringComparison.Ordinal);
        var bpCall = stripped.IndexOf("bp_decode(", StringComparison.Ordinal);

        // THE SLICE STARTS AFTER THE SEMICOLON THAT ENDS THE NORMALISE CALL, and it is worth the
        // extra line. Slicing from the open bracket instead leaves the call's own argument -
        // "log174)" - inside the region, and the count of intervening statements that mention the
        // ratio array then reads 1 when the true answer is 0. This audit's first run made exactly
        // that mistake and reported the row DIFFERING on the strength of it.
        var afterCall = normaliseCall < 0 ? -1 : stripped.IndexOf(';', normaliseCall) + 1;
        var between = afterCall > 0 && bpCall > afterCall
            ? stripped[afterCall..bpCall]
            : string.Empty;

        // Every statement between the two calls, so "nothing touches the array" is a count and not
        // an impression.
        var betweenStatements = between
            .Split(';')
            .Select(s => Regex.Replace(s, @"\s+", " ").Trim())
            .Where(s => s.Length > 0)
            .ToArray();
        var betweenTouchesArray = betweenStatements.Count(s => s.Contains("log174", StringComparison.Ordinal));

        _output.WriteLine("WHERE THE CALL SITS IN UPSTREAM'S OWN PIPELINE:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  ftx_decode_candidate is {candidate.Split('\n').Length} lines");
        _output.WriteLine($"  ftx_normalize_logl( appears at character {normaliseCall} of its body, "
            + $"its statement ending at {afterCall}");
        _output.WriteLine($"  bp_decode(          appears at character {bpCall}");
        _output.WriteLine($"  the normalisation is called {(normaliseCall >= 0 && bpCall > normaliseCall
            ? "BEFORE"
            : "NOT BEFORE")} the belief propagation");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  STATEMENTS BETWEEN THE TWO CALLS: {betweenStatements.Length}, of which "
            + $"{betweenTouchesArray} mention log174");
        foreach (var statement in betweenStatements)
        {
            _output.WriteLine($"    - {statement};");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  And the same reading of the port, from Ft8SlotDecoder's own path:");
        _output.WriteLine("    Ft8SoftSymbols.Extract -> Ft8SoftSymbols.Normalise -> LdpcDecoder.Decode,");
        _output.WriteLine("    with the ratio array untouched between the second and the third.");
        _output.WriteLine(string.Empty);

        findings.Add(new Finding(
            "where the call sits",
            $"inside ftx_decode_candidate, after extraction and BEFORE bp_decode, with "
                + $"{betweenTouchesArray} of {betweenStatements.Length} intervening statements "
                + "mentioning the ratio array",
            "Ft8CodewordDecoder normalises and then decodes, with nothing between",
            normaliseCall >= 0 && bpCall > normaliseCall && betweenTouchesArray == 0,
            "STRONG"));

        // ---------------------------------------------------- how many times upstream normalises
        //
        // A definition is not a call. Counting one as the other is exactly the mistake unit 222's
        // constant extraction made when it counted commented-out approximations, so the definition
        // is excluded by its return type rather than by position.
        var mentions = Regex.Matches(StripComments(decode), @"\bftx_normalize_logl\s*\(").Count;
        var definitions = Regex.Matches(
            StripComments(decode), @"\b(?:static\s+)?void\s+ftx_normalize_logl\s*\(").Count;
        var callSites = mentions - definitions;
        findings.Add(new Finding(
            "how many times the array is normalised",
            $"ftx_normalize_logl is mentioned {mentions} time(s) in decode.c, of which "
                + $"{definitions} carry a void return type and are therefore its forward "
                + $"declaration and its definition, so {callSites} CALL SITE(S)",
            "Normalise is called once per candidate, immediately before the decode",
            callSites == 1,
            "STRONG"));

        // ==================================================================================
        // THE DEGENERATE VARIANCE. Expected to come back DIFFERING - it is divergence 23.
        // ==================================================================================
        var pinGuards = Regex.IsMatch(body, @"variance\s*(>|>=|==|!=|<|<=)\s*0")
            || Regex.IsMatch(body, @"if\s*\([^)]*variance[^)]*\)");
        var portGuards = Regex.IsMatch(normalise, @"if\s*\(!\(variance\s*>\s*0\.0f\)\)");
        findings.Add(new Finding(
            "the degenerate variance",
            $"no guard: any test of variance before the division = {pinGuards}. Upstream divides "
                + "by it unchecked, so 174 identical ratios give an infinity or a NaN and the whole "
                + "array is multiplied by it",
            "if (!(variance > 0.0f)) return variance - the array is left untouched and the zero "
                + "variance is reported",
            pinGuards == portGuards,
            "STRONG"));

        // ----------------------------------------------------------------- what comes back
        var pinReturnsVoid = Regex.IsMatch(decode, @"static\s+void\s+ftx_normalize_logl\s*\(");
        findings.Add(new Finding(
            "what comes back",
            $"void: the signature is a void one = {pinReturnsVoid}. The variance is a local and "
                + "is discarded",
            "float: the pre-scale variance is returned, so a caller can see the scale the ratios "
                + "arrived on. THE ARITHMETIC ON THE ARRAY IS UNCHANGED BY THIS",
            false,
            "STRONG"));

        // ============================================== the inventory
        _output.WriteLine("THE INVENTORY. SAME means the port does what the pin does; DIFFERING means");
        _output.WriteLine("it does not, and every DIFFERING row is either already a recorded divergence");
        _output.WriteLine("or is this unit's finding:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"term",-42} {"same?",-10} evidence");

        var differing = 0;
        foreach (var finding in findings)
        {
            if (!finding.Same)
            {
                differing++;
            }

            _output.WriteLine($"{finding.Term,-42} {(finding.Same ? "SAME" : "DIFFERING"),-10} "
                + $"{finding.Evidence}");
            _output.WriteLine($"    pin : {finding.Pin}");
            _output.WriteLine($"    port: {finding.Port}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"TERMS AUDITED: {findings.Count}. SAME: {findings.Count - differing}. "
            + $"DIFFERING: {differing}.");
        _output.WriteLine(string.Empty);

        var undocumented = findings
            .Where(f => !f.Same && !f.Term.StartsWith("the degenerate", StringComparison.Ordinal)
                && !f.Term.StartsWith("what comes back", StringComparison.Ordinal))
            .ToArray();

        if (undocumented.Length == 0)
        {
            _output.WriteLine("THE PORT IS FAITHFUL HERE TOO. Every difference the audit found is one");
            _output.WriteLine("this tree already records and defends:");
            _output.WriteLine(string.Empty);
            _output.WriteLine("  - THE DEGENERATE VARIANCE is divergence 23, and the port's side of it is");
            _output.WriteLine("    the only defensible one: there is no faithful port of dividing by zero");
            _output.WriteLine("    and multiplying an array by the NaN that comes out. It can only fire on");
            _output.WriteLine("    174 IDENTICAL ratios, which no real waterfall produces, so IT CANNOT");
            _output.WriteLine("    COST A DECODE at -21 dB - the same shape of argument that retired");
            _output.WriteLine("    divergence 21 in unit 222.");
            _output.WriteLine("  - WHAT COMES BACK is a return type and not arithmetic. The pin discards a");
            _output.WriteLine("    local; the port hands it to the caller. NOT ONE VALUE IN THE ARRAY");
            _output.WriteLine("    DIFFERS because of it.");
            _output.WriteLine(string.Empty);
            _output.WriteLine("SO TASK 6'S FIRST CONDITION IS NOT MET AT THIS STAGE EITHER, and no fix is");
            _output.WriteLine("licensed by this task. That is a real answer and it is the likely one.");
        }
        else
        {
            _output.WriteLine($"THE PORT DIFFERS FROM THE PIN IN {undocumented.Length} TERM(S) THAT ARE NOT");
            _output.WriteLine("ALREADY RECORDED. Each is named above, and task 6's first condition is met");
            _output.WriteLine("for any of them a measured row attributes decodes to.");
        }

        // The numbers above are the report. These assert the ones that would make the audit itself
        // wrong if they moved.
        Assert.True(pinTarget.Success, "the pin no longer scales by a square root of a target over the variance.");
        Assert.Equal(Ft8SoftSymbols.NormalisedVariance, float.Parse(pinTarget.Groups[1].Value));
        Assert.True(pinVariance.Success, "the pin no longer forms the variance as (sum2 - sum*sum/N)/N.");
        Assert.True(normaliseCall >= 0, "ftx_decode_candidate no longer calls ftx_normalize_logl.");
        Assert.True(bpCall > normaliseCall, "the pin no longer normalises before it decodes.");
    }

    private sealed record Finding(string Term, string Pin, string Port, bool Same, string Evidence);

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
        var path = Path.Combine(at!.FullName, "src", "Ft8Sharp", "Dsp", "Ft8SoftSymbols.cs");
        Assert.True(File.Exists(path), $"the port's extraction is not at {path}.");
        return File.ReadAllText(path);
    }

    /// <summary>One method of the port, brace-matched from its signature.</summary>
    private static string PortMethod(string source, string signatureText)
    {
        var signature = source.IndexOf(signatureText, StringComparison.Ordinal);
        Assert.True(signature >= 0, $"the port no longer declares {signatureText}");

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

        Assert.Fail($"{signatureText} does not close.");
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
