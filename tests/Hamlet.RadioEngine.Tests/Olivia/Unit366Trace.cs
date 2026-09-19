using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Olivia;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>
/// Work instruction 366 task 1: **the trace, before the gate is opened** - items 1 to 8.
/// </summary>
/// <remarks>
/// <para>**THIS IS A MEASUREMENT, NOT AN ASSERTION**, the shape of `Unit365Trace`. It asserts
/// nothing at all, so it cannot become a wall, and it is not on the carry-forward line.</para>
/// <para>**THE CODE-LOCATION ITEMS ARE READ FROM THE TREE**, by name and line, rather than
/// transcribed from the instruction: items 1, 2, 5, 6 and 7 print what a grep of `src/` finds
/// now, so a line that has moved reads as moved.</para>
/// <para>**COMPUTED, NOT SEEN**, and nothing here is evidence about the radio (FACT-004).</para>
/// </remarks>
[Collection(CpuMeasuredAlone.Name)]
public sealed class Unit366Trace
{
    /// <summary>The compound callsign the longest Report in the tree is addressed to.</summary>
    internal const string Compound = "VP2V/W1AW";

    /// <summary>The three variants, in the order the timing table carries them.</summary>
    internal static readonly string[] Variants = ["8/250", "16/500", "32/1000"];

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit366Trace(ITestOutputHelper output) => _output = output;

    /// <summary>Items 1, 2, 5, 6 and 7: every line of `src/` the gate would have to reach.</summary>
    [Fact]
    public void TheSitesTheGateWouldHaveToReach()
    {
        var root = OliviaFixtures.Root();
        var src = Path.Combine(root, "src");

        // Each pattern is one question the instruction asks, and the answer is whatever the
        // tree holds now. Nothing here is transcribed.
        var wanted = new (string Item, string[] Patterns)[]
        {
            ("1 the gate", ["CanTransmitIn", "IsPsk31Chosen\n", "private void SendPsk31", "SendPsk31(wanted)"]),
            ("1 the keying", ["CivConstants.PttOn)", "_armedSend.Arm(", "OperatorSend.Now("]),
            ("1 the offsets", ["_psk31SendAtHz", "ClearSpotForTheCall", "Psk31ClearSpot.Choose", "OffsetOn(row"]),
            ("1 compose", ["Psk31Modulator.Compose(", "OliviaModulator.Compose(", "SendStage.Entered("]),
            ("1 the presses", ["AnswerPsk31(", "SendTypedPsk31(", "CardActionAsync(", "CallToAnyoneText"]),
            ("2 the row", ["_oliviaVariants", "row.Variant", "HasVariant", "ShowPsk31Channels("]),
            ("3 the rate", ["_transmitSampleRate"]),
            ("5 the receipt", ["BookThePsk31Call", "RecordCallToAnyone", "_psk31Mine"]),
            ("6 the turn", ["Psk31Turn.Read(", "AnswerSeconds", "PatienceSeconds"]),
            ("7 the power and the ALC", ["HasPsk31PowerOffer", "Psk31AlcReference", "Psk31AlcReferenceLine", "Psk31AlcLine =", "Psk31PowerOffer\n"]),
        };

        foreach (var (item, patterns) in wanted)
        {
            foreach (var pattern in patterns)
            {
                var needle = pattern.TrimEnd('\n');
                var found = 0;

                foreach (var path in Sources(src))
                {
                    var lines = File.ReadAllLines(path);

                    for (var i = 0; i < lines.Length; i++)
                    {
                        // A doc comment mentioning a name is not a call site, and the counts
                        // this unit reports are of code.
                        var trimmed = lines[i].TrimStart();

                        if (trimmed.StartsWith("///", StringComparison.Ordinal)
                            || trimmed.StartsWith("//", StringComparison.Ordinal)
                            || trimmed.StartsWith("*", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (!lines[i].Contains(needle, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        found++;
                        _output.WriteLine($"({item}) {Path.GetRelativePath(src, path)}:{i + 1}: {trimmed}");
                    }
                }

                if (found == 0)
                {
                    _output.WriteLine($"({item}) {needle}: NO CODE LINE IN src/");
                }
            }
        }
    }

    /// <summary>Item 3: what the app composes at, and what the Olivia paths want at that rate.</summary>
    [Fact]
    public void TheTransmitSampleRate()
    {
        var format = OliviaData.Current.Format!;
        var codes = OliviaData.Current.Rsid!;

        _output.WriteLine(
            $"(3) Ft8Composer.DefaultSampleRate = {Ft8Composer.DefaultSampleRate} Hz - what _transmitSampleRate holds "
            + "before a sink names one, and what it falls back to when the radio goes.");

        foreach (var rate in new[] { 8000, 11025, 12000, 48000 })
        {
            var burst = RsidBurst.LengthInSamples(codes, rate);

            _output.WriteLine($"(3) at {rate} Hz: RSID burst {burst} samples = {burst / (double)rate:0.000} s");

            foreach (var name in Variants)
            {
                var v = format.Variant(name)!;
                var lowest = 1000 + v.FirstToneOffsetHz;
                var highest = lowest + ((v.Tones - 1) * v.ToneSpacingHz);
                var fits = lowest - v.ToneSpacingHz > 0 && highest + v.ToneSpacingHz < rate / 2.0;

                _output.WriteLine(
                    $"(3)   {name} at 1000 Hz: tones {lowest:0.0} to {highest:0.0} Hz, "
                    + $"Modulate {(fits ? "composes" : "THROWS on centerHz")} at {rate} Hz "
                    + $"(needs the band inside 0 to {rate / 2.0:0} Hz with a spacing of margin)");
            }
        }

        _output.WriteLine(
            "(3) RsidDetector takes whatever rate the audio carries (RsidDetector(codes, sampleRate, low, high)); "
            + "it is never told a center or a code, and it steps the passband itself.");
    }

    /// <summary>Item 4: what PSK31's clear-spot rule gives on the Olivia tab, with the two-signal fixture's carriers.</summary>
    [Fact]
    public void TheClearSpotOnTheOliviaTab()
    {
        var format = OliviaData.Current.Format!;

        _output.WriteLine(
            $"(4) Psk31ClearSpot: ClearHz {Psk31ClearSpot.ClearHz}, LowestCallHz {Psk31ClearSpot.LowestCallHz}, "
            + $"HighestCallHz {Psk31ClearSpot.HighestCallHz}, Occupied {Psk31ClearSpot.Occupied}");
        _output.WriteLine($"(4) rule: {Psk31ClearSpot.Rule}");

        // The two-signal fixture: KC3QIS 8/250 at 1000 Hz and EI4GNB 16/500 at 2000 Hz.
        var heard = new (string Variant, double CenterHz)[] { ("8/250", 1000), ("16/500", 2000) };

        foreach (var (variant, centerHz) in heard)
        {
            var v = format.Variant(variant)!;
            var width = (v.Tones - 1) * v.ToneSpacingHz;
            var lowest = centerHz + v.FirstToneOffsetHz;

            _output.WriteLine(
                $"(4) heard: {variant} at {centerHz:0} Hz occupies {lowest:0.0} to {lowest + width:0.0} Hz ({width:0.0} Hz of tones)");
        }

        var spot = Psk31ClearSpot.Choose(
            heard.Select(h => h.CenterHz),
            [],
            Psk31ClearSpot.LowestCallHz,
            Psk31ClearSpot.HighestCallHz);

        _output.WriteLine(
            $"(4) with both carriers handed in: spot = {(spot is { } hz ? hz.ToString("0.0", CultureInfo.InvariantCulture) + " Hz" : "none")}");

        if (spot is { } chosen)
        {
            foreach (var name in Variants)
            {
                var v = format.Variant(name)!;
                var width = (v.Tones - 1) * v.ToneSpacingHz;
                var lowest = chosen + v.FirstToneOffsetHz;
                var highest = lowest + width;

                var clashes = heard
                    .Select(h =>
                    {
                        var hv = format.Variant(h.Variant)!;
                        var hlow = h.CenterHz + hv.FirstToneOffsetHz;
                        var hhigh = hlow + ((hv.Tones - 1) * hv.ToneSpacingHz);
                        var gap = hlow > highest ? hlow - highest : lowest - hhigh;

                        return $"{h.Variant} at {h.CenterHz:0}: {gap:0.0} Hz edge to edge{(gap < 0 ? " OVERLAPS" : "")}";
                    })
                    .ToList();

                _output.WriteLine(
                    $"(4)   a {name} send on the spot would occupy {lowest:0.0} to {highest:0.0} Hz - {string.Join("; ", clashes)}");
            }
        }

        // What ClearSpotForTheCall hands in today, under the Olivia tab: the PSK31 listener is
        // null there, so the carriers and candidates are both empty and the bounds fall back.
        var blind = Psk31ClearSpot.Choose([], [], Psk31ClearSpot.LowestCallHz, Psk31ClearSpot.HighestCallHz);

        _output.WriteLine(
            $"(4) with nothing handed in, which is what the Olivia tab gives it today: spot = "
            + $"{(blind is { } b ? b.ToString("0.0", CultureInfo.InvariantCulture) + " Hz" : "none")}");
    }

    /// <summary>Item 8: the caps, the longest Report, and the longest text that actually fits.</summary>
    [Fact]
    public void TheCapsAndWhatActuallyFits()
    {
        var timing = OliviaData.Current.Timing!;
        var format = OliviaData.Current.Format!;
        const int rate = 12000;
        const double center = 1000;
        const float peak = 0.5f;

        var report = Psk31Macros.Report(Compound, "KC3QIS", "Tim", "Trafford PA", "FN00DJ");

        _output.WriteLine(
            $"(8) timing.json: macro {timing.CapMacroCharacters} characters, typed {timing.CapTypedCharacters}, "
            + $"patience {timing.PatienceCharacters}, retire {timing.RetireAfterCharacters}");
        _output.WriteLine($"(8) the longest Report the app composes: {report.Length} characters, \"{report}\"");
        _output.WriteLine(
            $"(8) PSK31 holds it to OperatorSend.LongestUnslottedSeconds {OperatorSend.LongestUnslottedSeconds} s "
            + $"and a typed line to 60 s, and neither moves.");

        _output.WriteLine("(8) variant | s per char | macro cap s | typed cap s | patience s | retire s | Report s | Report fits");

        foreach (var name in Variants)
        {
            var spc = timing.SecondsPerCharacter[name];
            var macroCap = timing.CapSeconds(name, OliviaSendKind.Macro);
            var typedCap = timing.CapSeconds(name, OliviaSendKind.TypedLine);
            var composed = OliviaModulator.Compose(report, name, center, rate, peak);

            _output.WriteLine(
                $"(8) {name} | {spc:0.00000} | {macroCap:0.00} = {timing.CapMacroCharacters} x {spc:0.00000} | "
                + $"{typedCap:0.00} | {timing.PatienceSeconds(name):0.00} | {timing.RetireWindowSeconds(name):0.00} | "
                + $"{composed.TextSeconds:0.00} | {composed.Fit}");
        }

        _output.WriteLine("(8) variant | kind | cap s | longest text that actually fits | its seconds | the count's own text at that length");

        foreach (var name in Variants)
        {
            foreach (var kind in new[] { OliviaSendKind.Macro, OliviaSendKind.TypedLine })
            {
                var cap = timing.CapSeconds(name, kind);
                var count = kind == OliviaSendKind.TypedLine ? timing.CapTypedCharacters : timing.CapMacroCharacters;
                var longest = LongestThatFits(name, kind, rate, center, peak);
                var atLongest = OliviaModulator.Compose(Filler(longest), name, center, rate, peak);
                var atCount = OliviaModulator.Compose(Filler(count), name, center, rate, peak);

                _output.WriteLine(
                    $"(8) {name} | {kind} | {cap:0.00} | {longest} characters | {atLongest.TextSeconds:0.00} s | "
                    + $"{count} characters would be {atCount.TextSeconds:0.00} s, {atCount.Fit}");
            }
        }

        _output.WriteLine(
            $"(8) a block at each variant: {format.SymbolsPerBlock} symbols, "
            + string.Join("; ", Variants.Select(n =>
            {
                var v = format.Variant(n)!;
                return $"{n} carries {v.BitsPerSymbol} characters a block in {format.SymbolsPerBlock * v.SymbolSeconds:0.000} s";
            })));
    }

    /// <summary>The largest character count whose composed text is still inside the variant's cap.</summary>
    private static int LongestThatFits(string variant, OliviaSendKind kind, int rate, double center, float peak)
    {
        var low = 0;
        var high = 512;

        while (low < high)
        {
            var mid = (low + high + 1) / 2;

            if (OliviaModulator.Compose(Filler(mid), variant, center, rate, peak, kind).Fit == UnslottedFit.Fits)
            {
                low = mid;
            }
            else
            {
                high = mid - 1;
            }
        }

        return low;
    }

    /// <summary>A text of exactly this many characters, all of them ones Olivia carries.</summary>
    private static string Filler(int characters) => new('A', Math.Max(1, characters));

    /// <summary>Every C# file under `src/`, the build's own output left out.</summary>
    private static IEnumerable<string> Sources(string src)
        => Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                        && !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .OrderBy(p => p, StringComparer.Ordinal);
}
