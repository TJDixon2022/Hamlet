using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What a fast fist reads without a run-up (HM-DEC-122, HM-OPEN-031).
/// </summary>
/// <remarks>
/// <para>**A RUN-UP HIDES A GREAT DEAL**, which is why every figure in the first
/// theory below is taken without one. An operator sweeping a band tunes onto a
/// station already sending, and what he gets then is what this measures.</para>
/// <para>**WRITTEN AGAINST HM-DEC-122 AND THEN MOVED BY SOMETHING ELSE.** The
/// bars started at 0.63 to 0.70, which is what the decoder managed when the
/// streaming pass had its own two-way gap classifier: a fast fist tuned onto
/// mid-transmission arrived at about two thirds of the message however strong it
/// was. Handing that job to <see cref="CwGapFit"/>, which HM-DEC-115 ruled and
/// which the settled pass already used, took every one of them to about nine
/// tenths. HM-DEC-122 itself is not live and is held at HM-OPEN-030.</para>
/// <para>The bars are set a tenth under each measured figure, so this fails on a
/// real change in either direction rather than on a noise draw.</para>
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
    /// <para>**FROM ABOUT TWO THIRDS TO ABOUT NINE TENTHS**, at 25, 28, 30 and 35
    /// words a minute alike: 0.67, 0.63, 0.70 and 0.63 became 0.89, 0.89, 0.89
    /// and 0.88. The gain is the gap classifier rather than anything about
    /// acquisition, which is worth saying plainly because the test was written
    /// expecting the other answer.</para>
    /// </remarks>
    [Theory]
    [InlineData(25, 0.79)]
    [InlineData(28, 0.79)]
    [InlineData(30, 0.79)]
    [InlineData(35, 0.78)]
    public void AFastFistIsReadWithoutARunUp(int wordsPerMinute, double floor)
    {
        var share = Share(wordsPerMinute, 18.0, string.Empty);

        _output.WriteLine($"{wordsPerMinute} wpm at 18 dB, bare: {share:0.00}");

        Assert.True(
            share >= floor,
            $"{wordsPerMinute} words a minute tuned onto mid-transmission came "
            + $"back {share:0.00} of the message against a bar of {floor:0.00}");
    }

    /// <remarks>
    /// <para>**AND THE SAME FIST WITH FOUR SECONDS TO ACQUIRE ON READS WHOLE**,
    /// which is the control for the measurement above and the reason the fixtures
    /// on disk carry a run-up (HM-DEC-103). Measured at 1.00, 1.00 and 0.96.</para>
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
    /// assumed to be unaffected. It went from 0.89 to between 0.95 and 1.00, so
    /// nothing was traded for the figures above.</para>
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

        // Two thirds, which is well under the 0.95 to 1.00 measured here and well
        // over what a decoder that had lost the slow end would manage.
        Assert.True(
            share >= 0.66,
            $"{wordsPerMinute} words a minute at {snrDb:0} dB came back {share:0.00} "
            + "of the message");
    }
}
