using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The table HM-DEC-097 needs before its floor can be set, and the cost of
/// setting it.
/// </summary>
/// <remarks>
/// <para>**HM-DEC-097 IS RULED AND HAS NEVER BEEN BUILT.** It says the decoder
/// refuses below nought decibels rather than copying into the band where it is
/// half wrong, because at minus two decibels it emits a full message of which
/// forty-four per cent is invented. There is no such floor in the decoder: the
/// streaming pass gates on coherence and a plausible speed, the settled pass on
/// six decibels of contrast, and neither is what that ruling describes.</para>
/// <para>**IT CANNOT SIMPLY BE ADDED, AND THAT IS WHY IT HAS NOT BEEN.** The
/// ruling's decibels are the broadband ratio the fixture was generated at, and
/// the decoder measures inside a narrow tone filter, which reads far higher for
/// the same audio. Choosing which internal number stands for nought decibels
/// broadband decides what the display asserts and is Tim's (§12.1). **What a
/// session can do is put both scales side by side**, which is what this is.</para>
/// <para>Printed rather than asserted. Nothing here changes the decoder.</para>
/// </remarks>
public sealed class CwRefusalFloorTableTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public CwRefusalFloorTableTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// One level of the sweep, with both scales and what came out.
    /// </summary>
    private sealed record Level(
        double GeneratedDb,
        double MeasuredDb,
        double Correct,
        double Invented,
        int Emitted);

    private static Level Measure(double generatedDb)
    {
        double measured = 0;
        double correct = 0;
        double invented = 0;
        var emitted = 0;
        var seeds = 0;

        for (var seed = 1; seed <= CwSensitivity.Seeds; seed++)
        {
            var request = new CwSignalRequest(
                CwSensitivity.Message,
                WordsPerMinute: CwSensitivity.WordsPerMinute,
                ToneHz: CwSensitivity.ToneHz,
                Amplitude: 0.5,
                NoiseAmplitude: CwSensitivity.NoiseFor(generatedDb),
                Seed: seed * 7919);

            var result = CwDecodeHarness.Decode(request);
            var report = result.Report;
            var characters = result.Characters;

            if (!double.IsNaN(report.SnrDb))
            {
                measured += report.SnrDb;
                seeds++;
            }

            var letters = characters.Where(c => !c.IsWordGap).ToList();
            var matches = CwAlignment.Align(characters, CwSensitivity.Message);
            var expected = CwAlignment.SymbolCount(CwSensitivity.Message);

            correct += (double)matches.Count(
                m => m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap) / expected;

            invented += (double)matches.Count(
                m => m.Kind == CwMatchKind.Wrong && !m.Decoded.IsWordGap) / expected;

            emitted += letters.Count;
        }

        return new Level(
            generatedDb,
            seeds == 0 ? double.NaN : measured / seeds,
            correct / CwSensitivity.Seeds,
            invented / CwSensitivity.Seeds,
            emitted / CwSensitivity.Seeds);
    }

    /// <remarks>
    /// **THE TABLE.** Generated broadband ratio down the left, the decoder's own
    /// measured margin beside it, and what the transcript looked like at each.
    /// Reading across one row tells Tim what a floor set at that internal number
    /// would refuse, and reading down the correct column tells him what it costs.
    /// </remarks>
    [Fact]
    public void TheTwoScalesArePutSideBySide()
    {
        var levels = new List<Level>();

        for (var db = 18.0; db >= -6.0; db -= 2.0)
        {
            levels.Add(Measure(db));
        }

        _output.WriteLine(
            "generated | decoder's own | correct | invented | emitted");

        foreach (var level in levels)
        {
            _output.WriteLine(
                $"{level.GeneratedDb,6:F0} dB | {level.MeasuredDb,10:F1} dB | "
                + $"{level.Correct,6:P0} | {level.Invented,7:P0} | {level.Emitted,3}");
        }

        // **WHAT A FLOOR WOULD COST, WHICH IS THE HALF NOBODY HAS MEASURED.** For
        // each candidate, the levels it would silence and how much of each was
        // being read correctly. A floor that refuses a level reading ninety per
        // cent is buying honesty with copy the operator already had.
        _output.WriteLine("");
        _output.WriteLine("floor (decoder's own) | levels refused | best correct among them");

        foreach (var floor in new[] { 20.0, 18.0, 16.0, 14.0, 12.0, 10.0 })
        {
            var refused = levels
                .Where(l => !double.IsNaN(l.MeasuredDb) && l.MeasuredDb < floor)
                .ToList();

            var best = refused.Count == 0 ? 0 : refused.Max(l => l.Correct);

            _output.WriteLine(
                $"{floor,21:F0} | {refused.Count,14} | {best,23:P0}");
        }

        Assert.NotEmpty(levels);
    }
}
