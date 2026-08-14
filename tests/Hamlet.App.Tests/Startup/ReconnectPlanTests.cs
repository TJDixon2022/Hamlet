using Hamlet.App.Startup;
using Xunit;

namespace Hamlet.App.Tests.Startup;

/// <summary>
/// What Hamlet does with the radio when its window opens (HM-DEC-052).
/// </summary>
/// <remarks>
/// Every one of these is a case somebody meets on an ordinary evening and
/// nobody exercises on purpose: the rig switched off, the cable in a different
/// socket, the setting turned off by somebody who did not want it. They are the
/// whole feature, so they are the tests.
/// </remarks>
public sealed class ReconnectPlanTests
{
    private const string Training = "Training radio";

    private static readonly string[] TwoPorts = { Training, "COM3", "COM7" };

    /// <remarks>
    /// The ordinary case: the radio is where it was, so open it.
    /// </remarks>
    [Fact]
    public void TheRememberedRadioIsOpenedWhenItIsStillThere()
    {
        var plan = ReconnectPlan.Decide(
            enabled: true, alreadyConnected: false,
            lastPort: "COM3", available: TwoPorts, trainingRadio: Training);

        Assert.Equal(ReconnectStep.RememberedPort, plan.Step);
        Assert.Equal("COM3", plan.Port);
        Assert.Null(plan.Explanation);
    }

    /// <remarks>
    /// A MISSING PORT IS NAMED, and named as itself. Windows renumbers a USB
    /// radio after an update or a different socket, and it is far and away the
    /// most common reason a reconnect fails. A generic "could not connect" here
    /// sends somebody to check a cable that was never the problem.
    /// </remarks>
    [Fact]
    public void APortThatIsGoneIsNamedRatherThanReportedGenerically()
    {
        var plan = ReconnectPlan.Decide(
            enabled: true, alreadyConnected: false,
            lastPort: "COM9", available: TwoPorts, trainingRadio: Training);

        Assert.Equal(ReconnectStep.TrainingRadio, plan.Step);
        Assert.NotNull(plan.Explanation);
        Assert.Contains("COM9", plan.Explanation, StringComparison.Ordinal);
        Assert.Contains("COM port", plan.Explanation, StringComparison.Ordinal);

        // And it says where the app ended up, not only what went wrong.
        Assert.Contains("training radio", plan.Explanation, StringComparison.Ordinal);
    }

    /// <remarks>
    /// FALLS BACK TO THE TRAINING RADIO RATHER THAN TO NOTHING. An app that
    /// opens dead teaches nothing and looks broken; the training radio at least
    /// puts a band on screen with something moving on it.
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(Training)]
    public void WithNoRealRadioRememberedHamletOpensTheTrainingOne(string? last)
    {
        var plan = ReconnectPlan.Decide(
            enabled: true, alreadyConnected: false,
            lastPort: last, available: TwoPorts, trainingRadio: Training);

        Assert.Equal(ReconnectStep.TrainingRadio, plan.Step);
        Assert.Equal(Training, plan.Port);

        // Nothing went wrong, so nothing is explained.
        Assert.Null(plan.Explanation);
    }

    /// <remarks>
    /// The setting is a setting. Somebody who shares a COM port with a logging
    /// program turns this off and expects Hamlet to keep its hands off the port.
    /// </remarks>
    [Fact]
    public void TheSettingTurnedOffMeansHamletTouchesNothing()
    {
        var plan = ReconnectPlan.Decide(
            enabled: false, alreadyConnected: false,
            lastPort: "COM3", available: TwoPorts, trainingRadio: Training);

        Assert.Equal(ReconnectStep.Nothing, plan.Step);
    }

    /// <remarks>
    /// Proves a rig that is already open is left alone. The window can be shown
    /// more than once and a second connect attempt on a live port would drop the
    /// radio the operator is using.
    /// </remarks>
    [Fact]
    public void ARadioAlreadyConnectedIsLeftAlone()
    {
        var plan = ReconnectPlan.Decide(
            enabled: true, alreadyConnected: true,
            lastPort: "COM3", available: TwoPorts, trainingRadio: Training);

        Assert.Equal(ReconnectStep.Nothing, plan.Step);
    }

    /// <remarks>
    /// Proves the two failures do not read alike. A port that vanished and a
    /// radio that is switched off need different things done about them, so
    /// saying the same words for both would waste the message.
    /// </remarks>
    [Fact]
    public void AMissingPortAndASilentRadioAreSaidDifferently()
    {
        var missing = ReconnectPlan.Decide(
            enabled: true, alreadyConnected: false,
            lastPort: "COM9", available: TwoPorts, trainingRadio: Training).Explanation;

        var silent = ReconnectPlan.NoAnswer("COM3");

        Assert.NotEqual(missing, silent);
        Assert.Contains("Connect", silent, StringComparison.Ordinal);
        Assert.DoesNotContain("COM port", silent, StringComparison.Ordinal);
    }

    /// <remarks>
    /// NEVER A DIALOG AND NEVER A SCOLDING (§0.7). These sentences arrive while
    /// somebody is sitting down to their radio, and every one of them describes
    /// a situation that is nobody's mistake.
    /// </remarks>
    [Fact]
    public void EveryThingItSaysReadsLikeAPersonRatherThanAnError()
    {
        var lines = new[]
        {
            ReconnectPlan.Decide(
                enabled: true, alreadyConnected: false, lastPort: "COM9",
                available: TwoPorts, trainingRadio: Training).Explanation!,
            ReconnectPlan.NoAnswer("COM3"),
            ReconnectPlan.CouldNotOpen(),
        };

        foreach (var line in lines)
        {
            foreach (var word in new[] { "error", "fail", "invalid", "must", "!" })
            {
                Assert.DoesNotContain(
                    word, line, StringComparison.OrdinalIgnoreCase);
            }

            // At most one em dash in a passage, and these want none (HM-DEC-040).
            Assert.DoesNotContain("—", line, StringComparison.Ordinal);
        }
    }
}
