using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Where the refusal floor actually belongs, measured (HM-DEC-117).
/// </summary>
/// <remarks>
/// <para>**SEVENTEEN WAS REASONED AND WAS FOUR DECIBELS OUT.** It came from the
/// offset between the broadband ratio a fixture is generated at and what the
/// decoder reads inside its own tone filter, and it was expected to bite at
/// HM-DEC-097's nought decibel line. It bites at about five, so four decibels of
/// reach were given up by arithmetic rather than by measurement.</para>
/// <para>The property the ruling wanted does hold: nothing is invented anywhere.
/// So the floor stays in force while the number is established by sweeping
/// candidates rather than translating a second time.</para>
/// <para>**THIS REPORTS AND DOES NOT CHOOSE.** A number that decides what the
/// display asserts is Tim's without exception (§12.1), and this one has been
/// guessed wrong once already.</para>
/// </remarks>
public sealed class CwFloorSweepTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public CwFloorSweepTests(ITestOutputHelper output) => _output = output;

    /// <summary>The floors to try, from the ruled one downward.</summary>
    public static readonly double[] Candidates = { 17, 16, 15, 14, 13, 12, 11, 10 };

    /// <remarks>
    /// <para>**THE TABLE HM-DEC-117 ASKED FOR.** For each candidate floor: where
    /// the invented share first rises above zero, and what correct share
    /// survives at five, four and three decibels — the band seventeen gave
    /// away.</para>
    /// </remarks>
    [Fact]
    public void SweepTheCandidateFloors()
    {
        _output.WriteLine(
            "floor   invents from   correct at 5 dB   at 4 dB   at 3 dB   reads to");
        _output.WriteLine(
            "-----   ------------   ---------------   -------   -------   --------");

        foreach (var floor in Candidates)
        {
            var sweep = CwSensitivity.Sweep(refusalFloorDb: floor);

            // Where invention starts: the highest ratio at which anything came
            // back as the wrong character. "never" is the property the ruling
            // wanted and the thing a floor is for.
            var invents = sweep
                .Where(p => p.Wrong > 0)
                .Select(p => (double?)p.SnrDb)
                .FirstOrDefault();

            double Correct(double db)
                => sweep.FirstOrDefault(p => Math.Abs(p.SnrDb - db) < 1e-9).Correct;

            // The lowest ratio that still returns most of the message, which is
            // the reach the floor is spending.
            var reads = sweep
                .Where(p => p.Correct >= 0.8)
                .Select(p => (double?)p.SnrDb)
                .LastOrDefault();

            _output.WriteLine(
                $"{floor,5:0}   {(invents is { } i ? $"{i,8:0.0} dB" : "     never"),12}   "
                + $"{Correct(5),15:0.00}   {Correct(4),7:0.00}   {Correct(3),7:0.00}   "
                + $"{(reads is { } r ? $"{r:0.0} dB" : "never"),8}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "The ruled floor is 17. Anything that invents at any level is worse "
            + "than the floor exists to prevent.");
    }
}
