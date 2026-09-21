using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 377 task 1: **the measurement before one line of criterion 2.2 moves** - what
/// the two RSID files hold, which one the engine is actually reading, what each of the seven
/// Olivia variants does when it is composed today, and what a loopback at each of them costs.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT AND NOT AN ASSERTION**, the shape of `Unit364Trace` and
/// `Unit365Trace`. It asserts nothing at all, so it cannot become a wall, and for that reason it
/// is not on the carry-forward line: a name that cannot fail teaches nothing by being run.</para>
/// <para>**IT CHANGES NOTHING AND REPAIRS NOTHING.** Four of the seven variants throw out of
/// <see cref="OliviaModulator.Compose"/> today; the exception is printed verbatim and left
/// exactly where it is. R41's gate is task 2's and task 3's work.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004). No port
/// is opened, no device is enumerated, nothing is keyed, and every sample lives in an array.</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class Unit377Trace
{
    /// <summary>The one RSID data file, which is what task 2 made it.</summary>
    /// <remarks>
    /// **THE RETIRED PATH IS NOT NAMED HERE** (task 2, section 6 ruling 1). Until task 2 this
    /// trace read both files side by side, and that reading is in the unit's history at the task 1
    /// commit; from task 2 there is one RSID file and only `TheOneRsidFileTests` names the path
    /// that used to hold the other one.
    /// </remarks>
    private const string OnePath = "assets/data/rsid-codes.json";

    /// <summary>The rate the modulator tests use, so the costs here are comparable to theirs.</summary>
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where every reading is printed.</param>
    public Unit377Trace(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **2.2's before, in six parts**: the two files side by side, which file the engine reads,
    /// the seven variants through <c>Compose</c>, the cost of a loopback at each, and what a new
    /// key in a `variants` row would turn red.
    /// </summary>
    [Fact]
    public void WhatTheTreeHoldsWhatComposesAndWhatEachVariantCosts()
    {
        var root = RepoRoot();

        PartOneTheOneRsidFile(root);
        PartTwoWhichFileTheEngineIsReading(root);

        var composed = PartThreeTheSevenVariantsThroughComposeToday();

        PartFourWhatALoopbackCosts(composed);
        PartSixWhatANewKeyInAVariantRowWouldDo(root);
    }

    /// <summary>**Item 1: the one RSID file - every code, its sequence, its length, both tables.**</summary>
    /// <remarks>
    /// **THIS WAS A TWO-FILE TABLE AT TASK 1 AND IS A ONE-FILE TABLE FROM TASK 2.** The before is
    /// in the unit's history at the task 1 commit and in the report; this is the after.
    /// </remarks>
    private void PartOneTheOneRsidFile(string root)
    {
        _output.WriteLine("=== 1. THE ONE RSID FILE ===");

        var at = Path.Combine(root, "assets", "data", "rsid-codes.json");
        var text = File.ReadAllText(at);
        var file = RsidCodes.Parse(text);

        _output.WriteLine($"{OnePath,-32} {text.Length,6} characters, {new FileInfo(at).Length} bytes");
        _output.WriteLine("");

        _output.WriteLine($"symbols        : {file.Symbols}");
        _output.WriteLine($"symbol rate Hz : {file.SymbolRateHz}");
        _output.WriteLine($"silence before : {file.SilenceSymbolsBefore}");
        _output.WriteLine($"first tone     : {file.FirstToneOffsetSymbols}");
        _output.WriteLine($"source         : \"{file.Source}\"");
        _output.WriteLine("");

        _output.WriteLine("code name        | code | tone sequence | length | file's symbols");

        foreach (var (name, code) in file.Codes.OrderBy(c => c.Value))
        {
            var has = file.ToneSequences.TryGetValue(name, out var tones);

            _output.WriteLine(
                $"{name,-16} | {code,4} | {(has ? "yes" : "NO "),-13} | {(has ? tones!.Count.ToString() : "-"),-6} | {file.Symbols}");
        }

        _output.WriteLine("");
        _output.WriteLine($"codes          : {file.Codes.Count}");
        _output.WriteLine($"tone sequences : {file.ToneSequences.Count}");
        _output.WriteLine($"every code the file lists has a sequence: {file.Codes.Keys.All(file.ToneSequences.ContainsKey)}");
        _output.WriteLine($"every sequence is the file's own symbol count: {file.ToneSequences.Values.All(v => v.Count == file.Symbols)}");

        // **THE TWO TABLES `RsidCodes` DOES NOT PARSE**, read straight off the JSON so the answer
        // is the file's and not the parser's.
        using var document = JsonDocument.Parse(text);
        var keys = document.RootElement.EnumerateObject().Select(p => p.Name).ToArray();
        var squares = document.RootElement.TryGetProperty("squares", out var q) ? q.GetArrayLength() : -1;
        var indices = document.RootElement.TryGetProperty("indices", out var i) ? i.GetArrayLength() : -1;

        _output.WriteLine("");
        _output.WriteLine($"  top-level keys : {string.Join(", ", keys)}");
        _output.WriteLine($"  squares        : {(squares < 0 ? "ABSENT" : squares + " entries")}");
        _output.WriteLine($"  indices        : {(indices < 0 ? "ABSENT" : indices + " entries")}");

        if (document.RootElement.TryGetProperty("_note", out var note))
        {
            _output.WriteLine($"  _note          : {note.GetString()}");
        }
    }

    /// <summary>**Item 2: which file the engine is reading, proved rather than assumed.**</summary>
    private void PartTwoWhichFileTheEngineIsReading(string root)
    {
        _output.WriteLine("");
        _output.WriteLine("=== 2. WHICH FILE THE ENGINE IS READING RIGHT NOW ===");

        var embedded = OliviaData.Current.Rsid;

        _output.WriteLine($"resource name  : {OliviaData.RsidResourceName}");
        _output.WriteLine($"RsidCodes.FilePath (what a sentence about it names): {RsidCodes.FilePath}");
        _output.WriteLine($"OliviaData.Current.Problem: {OliviaData.Current.Problem ?? "(none)"}");
        _output.WriteLine("");
        _output.WriteLine($"EMBEDDED copy in the assembly : {embedded!.Codes.Count} codes, {embedded.ToneSequences.Count} tone sequences");

        var tree = RsidCodes.Parse(File.ReadAllText(Path.Combine(root, "assets", "data", "rsid-codes.json")));
        var sameCodes = tree.Codes.Count == embedded.Codes.Count
                        && tree.Codes.All(c => embedded.CodeOf(c.Key) == c.Value);
        var sameSequences = tree.ToneSequences.Count == embedded.ToneSequences.Count
                            && tree.ToneSequences.All(s => embedded.ToneSequences.TryGetValue(s.Key, out var t) && t!.SequenceEqual(s.Value));

        _output.WriteLine(
            $"TREE copy at {OnePath,-32}: {tree.Codes.Count} codes, {tree.ToneSequences.Count} tone sequences; "
            + $"codes match the embedded copy {sameCodes}, sequences match {sameSequences}");

        _output.WriteLine("");
        _output.WriteLine("THE TREE COPY AND THE EMBEDDED COPY ARE ONE FILE, AND THERE IS NO SECOND ONE TO DISAGREE WITH.");
        _output.WriteLine($"Burst available for each of the eight codes, from the embedded copy the engine actually uses:");

        foreach (var (name, code) in embedded.Codes.OrderBy(c => c.Value))
        {
            _output.WriteLine($"  {name,-16} {code,3}  RsidBurst.TonesFor -> {(RsidBurst.TonesFor(embedded, code) is null ? "NULL (no burst)" : "15 tones")}");
        }
    }

    /// <summary>**Item 3: all seven variants through the send's own <c>Compose</c>, today.**</summary>
    private (string Variant, bool Composed, int Samples)[] PartThreeTheSevenVariantsThroughComposeToday()
    {
        _output.WriteLine("");
        _output.WriteLine("=== 3. EVERY ONE OF THE SEVEN VARIANTS THROUGH OliviaModulator.Compose TODAY ===");

        var format = OliviaData.Current.Format!;
        var macro = Psk31Macros.Cq(Unit365Trace.Mine);
        var answers = new List<(string Variant, bool Composed, int Samples)>();

        _output.WriteLine($"the macro   : {macro.Length} characters, the framed CQ the app sends");
        _output.WriteLine($"the center  : 1500 Hz, the rate {Rate} Hz, drive {Ft8Composer.DefaultDrivePeak}");
        _output.WriteLine("");

        foreach (var variant in format.Variants)
        {
            var announcedAs = OliviaModulator.AnnouncedAs(variant.Name);
            var code = OliviaData.Current.Rsid!.CodeOf(announcedAs);

            try
            {
                var composed = OliviaModulator.Compose(macro, variant.Name, 1500, Rate, Ft8Composer.DefaultDrivePeak);
                var heard = RsidDetector.Detect(
                    OliviaData.Current.Rsid!,
                    new MonoAudio(Rate, composed.Samples),
                    Psk31CarrierSearch.PassbandLowHz,
                    Psk31CarrierSearch.PassbandHighHz);

                _output.WriteLine(
                    $"{variant.Name,-8} announced as {announcedAs,-16} code {code,3}  COMPOSED: "
                    + $"{composed.Samples.Length} samples, {composed.Samples.Length / (double)Rate:0.000} s, "
                    + $"announcement {composed.AnnouncementSamples} samples, AnnouncedCode {composed.AnnouncedCode}, "
                    + $"cap {composed.Cap:0.00} s; the detector reads back "
                    + (heard.Count == 0
                        ? "NOTHING"
                        : string.Join("; ", heard.Select(h => $"{h.Name} ({h.Code}) at {h.CenterHz:0.00} Hz"))));

                answers.Add((variant.Name, true, composed.Samples.Length));
            }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException)
            {
                _output.WriteLine(
                    $"{variant.Name,-8} announced as {announcedAs,-16} code {code,3}  THREW "
                    + error.GetType().Name + ": \"" + error.Message + "\"");

                answers.Add((variant.Name, false, 0));
            }
        }

        _output.WriteLine("");
        _output.WriteLine($"COMPOSES TODAY: {string.Join(", ", answers.Where(a => a.Composed).Select(a => a.Variant))}");
        _output.WriteLine($"THROWS TODAY  : {string.Join(", ", answers.Where(a => !a.Composed).Select(a => a.Variant))}");
        _output.WriteLine("The sentence the operator would see for a variant that throws is the catch at");
        _output.WriteLine("MainWindowViewModel's Olivia send path: \"Hamlet did not send it: \" + the message + \".\"");

        return answers.ToArray();
    }

    /// <summary>**Item 4: what a loopback costs at each of the seven, in audio seconds and decode CPU.**</summary>
    /// <remarks>
    /// **THE FOUR THAT CANNOT BE COMPOSED CAN STILL BE MODULATED**, because
    /// <see cref="OliviaModulator.Modulate"/> is the table-driven half and it makes every row of
    /// `format.json`; it is only the announcement that refuses. So the decode CPU is measured for
    /// all seven rather than extrapolated for four, and the seconds of audio are checked against
    /// <see cref="OliviaModulator.TextSeconds"/>, which is the format table's own arithmetic.
    /// </remarks>
    private void PartFourWhatALoopbackCosts((string Variant, bool Composed, int Samples)[] composed)
    {
        _output.WriteLine("");
        _output.WriteLine("=== 4. WHAT A LOOPBACK COSTS AT EACH VARIANT ===");

        var format = OliviaData.Current.Format!;
        var texts = TheOliviaModulatorTests.Texts();
        var macro = texts.First(t => t.Name == "cq");
        var typed = texts.First(t => t.Name == "typed");

        _output.WriteLine($"one macro   : \"cq\", {macro.Text.Length} characters");
        _output.WriteLine($"a typed line: \"typed\", {typed.Text.Length} characters");
        _output.WriteLine("");
        _output.WriteLine("variant  | composes | macro s | typed s | macro decode cpu s | typed decode cpu s | macro chars out | typed chars out");

        var macroTotal = 0.0;
        var typedTotal = 0.0;

        foreach (var variant in format.Variants)
        {
            var row = composed.First(c => c.Variant == variant.Name);
            var macroCost = Cost(format, variant, macro.Text);
            var typedCost = Cost(format, variant, typed.Text);

            macroTotal += macroCost.Cpu;
            typedTotal += typedCost.Cpu;

            _output.WriteLine(
                $"{variant.Name,-8} | {(row.Composed ? "yes" : "NO "),-8} | {macroCost.Seconds,7:0.000} | {typedCost.Seconds,7:0.000} | "
                + $"{macroCost.Cpu,18:0.000} | {typedCost.Cpu,18:0.000} | {macroCost.CharactersOut,15} | {typedCost.CharactersOut,15}");
        }

        _output.WriteLine("");
        _output.WriteLine($"ONE CASE AT EVERY VARIANT: macro {macroTotal:0.000} s of decode CPU, typed {typedTotal:0.000} s, together {macroTotal + typedTotal:0.000} s.");
        _output.WriteLine("THE FULL SWEEP task 3 would run is five texts at two centers at each of seven variants,");
        _output.WriteLine($"which is roughly {(macroTotal + typedTotal) / 2 * 5 * 2:0.0} s of decode CPU by this measurement, plus modulation and detection.");
        _output.WriteLine("The seconds column is also checked against OliviaModulator.TextSeconds, the format table's own arithmetic:");

        foreach (var variant in format.Variants)
        {
            _output.WriteLine(
                $"  {variant.Name,-8} TextSeconds macro {OliviaModulator.TextSeconds(variant.Name, macro.Text.Length),7:0.000} s, "
                + $"typed {OliviaModulator.TextSeconds(variant.Name, typed.Text.Length),7:0.000} s, "
                + $"symbol {variant.SymbolSeconds} s, spacing {variant.ToneSpacingHz} Hz, {variant.Tones} tones");
        }
    }

    /// <summary>Modulate the text at the variant and read it back, timing the decode.</summary>
    private static (double Seconds, double Cpu, int CharactersOut) Cost(OliviaFormat format, OliviaVariant variant, string text)
    {
        var samples = OliviaModulator.Modulate(format, text, variant.Name, 1500, Rate, Ft8Composer.DefaultDrivePeak);
        var demodulator = new OliviaDemodulator(format, variant, 1500, Rate);
        var before = Process.GetCurrentProcess().TotalProcessorTime;
        var decoding = demodulator.Decode(new MonoAudio(Rate, samples), 0);
        var cpu = (Process.GetCurrentProcess().TotalProcessorTime - before).TotalSeconds;

        return (samples.Length / (double)Rate, cpu, decoding.CharactersOut);
    }

    /// <summary>**Item 6: what a new key in a `variants` row would turn red. Read, not changed.**</summary>
    private void PartSixWhatANewKeyInAVariantRowWouldDo(string root)
    {
        _output.WriteLine("");
        _output.WriteLine("=== 6. WHAT A NEW KEY IN A variants ROW WOULD TURN RED ===");

        var formatText = File.ReadAllText(Path.Combine(root, "data", "olivia", "format.json"));

        // **THE PARSE IS ASKED THE QUESTION RATHER THAN THE READER GUESSING IT.** A row with an
        // unknown key added is handed to `OliviaFormat.Parse`; if the parse is strict about a
        // row's exact shape it throws, and if it reads only the keys it names it does not.
        var withNewKey = formatText.Replace(
            "\"variant\": \"4/250\", \"tones\": 4",
            "\"variant\": \"4/250\", \"proved_by_loopback\": false, \"tones\": 4",
            StringComparison.Ordinal);

        _output.WriteLine($"the substitution landed: {!string.Equals(withNewKey, formatText, StringComparison.Ordinal)}");

        try
        {
            var parsed = OliviaFormat.Parse(withNewKey);

            _output.WriteLine(
                $"AN UNKNOWN KEY IN A ROW IS TOLERATED: the parse read {parsed.Variants.Count} variants and "
                + $"4/250 came back with {parsed.Variant("4/250")!.Tones} tones, spacing {parsed.Variant("4/250")!.ToneSpacingHz} Hz.");
        }
        catch (InvalidDataException error)
        {
            _output.WriteLine("AN UNKNOWN KEY IN A ROW IS REFUSED: " + error.Message);
        }

        _output.WriteLine("");
        _output.WriteLine("OliviaFormat.VariantRow reads variant, tones, bandwidth_hz, bits_per_symbol, tone_spacing_hz,");
        _output.WriteLine("symbol_seconds and first_tone_offset_hz by name and enumerates nothing, so a key it does not");
        _output.WriteLine("name is neither read nor refused. It then checks the row agrees with itself - tones are two to");
        _output.WriteLine("the power of the bits, spacing times tones is the bandwidth, spacing times symbol seconds is 1,");
        _output.WriteLine("and the first tone offset is the rule - and none of those would change.");
        _output.WriteLine("");
        _output.WriteLine("THE ONE NAME THAT COMPARES WHOLE ROWS is TheOliviaDataTests");
        _output.WriteLine(".TheOliviaFormatIsReadAtStartupWithItsVariantsAndConstants, which asserts");
        _output.WriteLine("Assert.Equal(format.Variants, fromTree.Format!.Variants) - the embedded copy's rows against the");
        _output.WriteLine("tree copy's, by OliviaVariant's record equality. It compares the two copies with each other and");
        _output.WriteLine("never against a literal, so a field added to both moves with them.");
        _output.WriteLine("");
        _output.WriteLine("AND THE ONE NAME THAT DEPENDS ON A ROW'S EXACT TEXT is TheOliviaDataTests");
        _output.WriteLine(".AMalformedFormatIsReportedInWordsAndNoValue, which does a string replace on");
        _output.WriteLine("\"tone_spacing_hz\": 31.25, \"symbol_seconds\": 0.032, \"first_tone_offset_hz\": -234.375 in the 16/500");
        _output.WriteLine("row. A new key appended AFTER first_tone_offset_hz leaves that substring whole; a new key put");
        _output.WriteLine("between those three would break the substitution and the name would go red on a silent no-op.");
    }

    private static string RepoRoot()
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
