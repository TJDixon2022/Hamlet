using System.Text.RegularExpressions;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The one place the read of upstream and the port's own constants can drift apart, and the place
/// that goes red when they do.
/// </summary>
/// <remarks>
/// <b>Why this is separate from <c>UpstreamExtractionInventoryTests</c>.</b> That file is the record
/// of the read and prints upstream's numbers rather than asserting them as literals, so that it says
/// what the pin holds rather than what somebody expected it to hold. This file binds those numbers
/// to the port's own constants. Without it, a re-pin could change the normalisation's target and the
/// inventory would happily print the new one while the port went on using the old.
/// <b>Absent is a skip.</b>
/// </remarks>
public class Ft8SoftSymbolsProvenanceTests
{
    private readonly ITestOutputHelper _output;

    public Ft8SoftSymbolsProvenanceTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The normalisation's target variance in the port equals the one in the pin.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheNormalisedVarianceIsTheNumberInThePin()
    {
        var source = File.ReadAllText(Path.Combine(ReferenceClone.Location, @"ft8\decode.c"));
        var match = Regex.Match(source, @"norm_factor\s*=\s*sqrtf\(\s*(\d+(?:\.\d+)?)f\s*/\s*variance\s*\)");
        Assert.True(match.Success, "upstream no longer scales by a square root of a fixed target over the variance.");

        var upstream = float.Parse(match.Groups[1].Value);
        _output.WriteLine($"  pin:  {upstream}");
        _output.WriteLine($"  port: {Ft8SoftSymbols.NormalisedVariance}");
        Assert.Equal(Ft8SoftSymbols.NormalisedVariance, upstream);
    }

    /// <summary>
    /// The ratio count, the tone count, the data symbol count and the bits per symbol in the port
    /// equal the macros in the pin.
    /// </summary>
    [RequiresReferenceCloneFact]
    public void TheCountsExtractionWalksAreTheMacrosInThePin()
    {
        var constants = File.ReadAllText(Path.Combine(ReferenceClone.Location, @"ft8\constants.h"));

        static int Macro(string source, string name)
        {
            var match = Regex.Match(source, $@"#define\s+{Regex.Escape(name)}\s*\((\d+)\)");
            Assert.True(match.Success, $"{name} is no longer a macro in constants.h.");
            return int.Parse(match.Groups[1].Value);
        }

        var rows = new (string Macro, int Pin, int Port, string Where)[]
        {
            ("FTX_LDPC_N", Macro(constants, "FTX_LDPC_N"), Ft8SoftSymbols.RatioCount, "Ft8SoftSymbols.RatioCount"),
            ("FT8_ND", Macro(constants, "FT8_ND"), Ft8SymbolEncoder.DataSymbolCount, "Ft8SymbolEncoder.DataSymbolCount"),
            ("FT8_NN", Macro(constants, "FT8_NN"), Ft8SymbolEncoder.SymbolCount, "Ft8SymbolEncoder.SymbolCount"),
            ("FT8_LENGTH_SYNC", Macro(constants, "FT8_LENGTH_SYNC"), Ft8SymbolEncoder.SyncBlockLength, "Ft8SymbolEncoder.SyncBlockLength"),
            ("FT8_NUM_SYNC", Macro(constants, "FT8_NUM_SYNC"), Ft8SymbolEncoder.SyncBlockCount, "Ft8SymbolEncoder.SyncBlockCount"),
            ("FT8_SYNC_OFFSET", Macro(constants, "FT8_SYNC_OFFSET"), Ft8SymbolEncoder.SyncBlockOffset, "Ft8SymbolEncoder.SyncBlockOffset"),
        };

        _output.WriteLine($"{"macro",-20} {"pin",5} {"port",5}  where");
        foreach (var (macro, pin, port, where) in rows)
        {
            _output.WriteLine($"{macro,-20} {pin,5} {port,5}  {where}");
            Assert.Equal(pin, port);
        }

        // And the identity extraction depends on: three ratios per data symbol fill the codeword
        // exactly, with nothing left over and nothing short.
        Assert.Equal(
            Ft8SoftSymbols.RatioCount,
            Ft8SymbolEncoder.DataSymbolCount * Ft8SymbolEncoder.BitsPerSymbol);
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {Ft8SymbolEncoder.DataSymbolCount} data symbols x "
            + $"{Ft8SymbolEncoder.BitsPerSymbol} bits = {Ft8SoftSymbols.RatioCount} ratios, exactly.");
    }
}
