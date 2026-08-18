using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What acquisition costs a fast fist, measured without a run-up (HM-DEC-122).
/// </summary>
/// <remarks>
/// <para>**A RUN-UP HIDES THE FAULT ENTIRELY**, which is why every number here is
/// taken without one. Given four seconds of easy signal to acquire on, a fast
/// fist reads about as well as a slow one; tuned onto mid-transmission, which is
/// what an operator sweeping a band actually does, it arrives at about six
/// characters in ten while a slow fist arrives at nine.</para>
/// <para>**THE MEASUREMENT IS THE DELIVERABLE AND NOTHING IN THE DECODER WAS
/// CHANGED.** HM-DEC-122 was built against these numbers and did not survive
/// them: see HM-OPEN-030 for what it did, and what it cost. The bars below pin
/// what the decoder manages today, so the next attempt has a figure to beat
/// rather than an impression to argue with.</para>
/// </remarks>
public sealed class CwAcquisitionWindowTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the shares are printed.</param>
    public CwAcquisitionWindowTests(ITestOutputHelper output) => _output = output;

    private const string Call = "CQ CQ DE N0CALL N0CALL K";

    /// <summary>The tone the message is sent at, off the decoder's own start.</summary>
    private const double ToneHz = 640;

    /// <summary>Three noise draws, because one decides a marginal decode.</summary>
    private static readonly int[] Seeds = { 7919, 104729, 15485863 };

    /// <summary>
    /// How much of the message came back right, averaged over the seeds.
    /// </summary>
    /// <remarks>
    /// The run-up, where there is one, is excluded by ignoring every V rather than
    /// by trimming the transcript, which cannot be done reliably once a decode has
    /// dropped a character. The call contains no V, so this separates what was
    /// acquired on from what was read.
    /// </remarks>
    private double Share(int wordsPerMinute, double snrDb, string prefix)
    {
        var message = prefix + Call;
        var expected = Call.Count(c => c != ' ');
        var total = 0.0;

        foreach (var seed in Seeds)
        {
            var result = CwDecodeHarness.Decode(
                new CwSignalRequest(
                    message,
                    WordsPerMinute: wordsPerMinute,
                    ToneHz: ToneHz,
                    Amplitude: 0.5,
                    NoiseAmplitude: CwSensitivity.NoiseFor(snrDb),
                    Seed: seed));

            total += (double)CwAlignment.Align(result.Characters, message).Count(m =>
                m.Kind == CwMatchKind.Correct
                && !m.Decoded.IsWordGap
                && m.Expected != "V") / expected;
        }

        return total / Seeds.Length;
    }

    /// <remarks>
    /// <para>**THE FIGURE HM-DEC-122 EXISTS TO MOVE.** Twenty-five words a minute
    /// and upward, tuned onto mid-transmission, comes back at about two thirds of
    /// the message however strong the signal is: 0.67, 0.63, 0.70 and 0.63 at
    /// eighteen decibels for 25, 28, 30 and 35 words a minute. Strength does not
    /// help, because the fault is in acquisition rather than in reading.</para>
    /// <para>The bars are a tenth under each measured figure, so this fails on a
    /// real change in either direction and not on a noise draw.</para>
    /// </remarks>
    [Theory]
    [InlineData(25, 0.57)]
    [InlineData(28, 0.53)]
    [InlineData(30, 0.60)]
    [InlineData(35, 0.53)]
    public void AFastFistArrivesShortWithoutARunUp(int wordsPerMinute, double floor)
    {
        var share = Share(wordsPerMinute, 18.0, string.Empty);

        _output.WriteLine($"{wordsPerMinute} wpm at 18 dB, bare: {share:0.00}");

        Assert.InRange(share, floor, 0.85);
    }

    /// <remarks>
    /// <para>**AND THE SAME FIST WITH FOUR SECONDS TO ACQUIRE ON READS WHOLE**,
    /// which is what says the fault is acquisition and not speed. This is the
    /// control for the measurement above and it is the reason the fixtures on
    /// disk carry a run-up (HM-DEC-103).</para>
    /// </remarks>
    [Theory]
    [InlineData(25)]
    [InlineData(28)]
    [InlineData(30)]
    public void TheSameFistWithARunUpDoesNot(int wordsPerMinute)
    {
        var share = Share(wordsPerMinute, 18.0, "VVV ");

        _output.WriteLine($"{wordsPerMinute} wpm at 18 dB, run-up: {share:0.00}");

        Assert.True(
            share >= 0.80,
            $"{wordsPerMinute} words a minute with a run-up came back {share:0.00} "
            + "of the message");
    }

    /// <remarks>
    /// <para>**THE SLOW END, WHICH IS WHAT ANY CHANGE HERE RISKS.** Weak and slow
    /// is this project's best-proven capability and the thing HM-DEC-122 was
    /// worded to protect, so it is measured beside the fast end rather than
    /// assumed to be unaffected.</para>
    /// </remarks>
    [Theory]
    [InlineData(10, 18.0)]
    [InlineData(10, 3.0)]
    [InlineData(12, 18.0)]
    [InlineData(12, 6.0)]
    [InlineData(12, 3.0)]
    public void TheSlowEndReadsTheMessage(int wordsPerMinute, double snrDb)
    {
        var share = Share(wordsPerMinute, snrDb, "VVV ");

        _output.WriteLine($"{wordsPerMinute} wpm at {snrDb:0} dB, run-up: {share:0.00}");

        // Two thirds, which is well under the 0.84 to 0.91 measured here and well
        // over what a decoder that had lost the slow end would manage.
        Assert.True(
            share >= 0.66,
            $"{wordsPerMinute} words a minute at {snrDb:0} dB came back {share:0.00} "
            + "of the message");
    }
}
