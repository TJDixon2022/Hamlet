using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// The licence gate is inside the send path, not beside it.
/// </summary>
/// <remarks>
/// <para>**THE TEST THAT MATTERS IS NOT THAT IT RETURNED FALSE** (work
/// instruction 255, task 3). It is that the transport saw **zero bytes** and the
/// sink was never touched. A refusal that returns false after keying the radio
/// is not a refusal, and only the wire can tell those apart.</para>
/// <para>**THE GATE PERMITS IN THREE WAYS AND THIS PATH ACCEPTS ONE.**
/// <c>TransmitGuard.Check</c> returns <c>MayTransmit: true</c> when the
/// privileges permit it, when the licence class is unknown, and when the
/// operator's guard toggle is off. §0.2 says the Settings check is not
/// bypassable from any send path, so the last two are refusals here. **Every
/// test below also asserts what <c>Check</c> itself answered**, so the record
/// shows the narrowing living in the new code rather than in the gate.</para>
/// <para>**Nothing about <c>TransmitGuard</c> is changed and no existing caller
/// moves.** <c>CwTransmitter</c> keeps exactly the behaviour it has today. What
/// the other callers should do about the two permissive branches is the owner's
/// question, banked by unit 253, and this unit does not answer it for them.
/// </para>
/// </remarks>
public sealed class TheLicenceGateIsInsideThePathTests
{
    /// <summary>An FT8 frequency on 20 m, which a Technician licence does not cover.</summary>
    private const long Ft8On20Metres = 14_074_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">xUnit's output sink.</param>
    public TheLicenceGateIsInsideThePathTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **Out of privilege: nothing keys, and the port sees nothing at all.**
    /// </summary>
    [Fact]
    public async Task AnOutOfPrivilegeFrequencyRefusesWithoutAByteReachingThePort()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.Technician, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);

        var (run, port, sink) = await Run(LicenseClass.Technician, guardEnabled: true);

        Report("out of privilege", decision, run, port, sink);

        // What the gate itself said: no.
        Assert.False(decision.MayTransmit);

        // What the path did about it.
        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, run.Outcome);
        Assert.False(run.Keyed);
        Assert.Null(run.Abort);
        Assert.Equal(UnkeyRoute.NothingWasKeyed, run.CameOutOfTransmit);

        // The whole point.
        Assert.Empty(port.Written);
        Assert.Equal(0, port.WritesAttempted);
        Assert.True(sink.WasNeverTouched);
    }

    /// <summary>
    /// **An unknown licence class is a refusal on this path, and the gate's own
    /// answer was yes.**
    /// </summary>
    /// <remarks>
    /// The clearest evidence that the narrowing is in the send path: the same
    /// call to the same gate, in the same test, returns <c>MayTransmit: true</c>.
    /// </remarks>
    [Fact]
    public async Task AnUnknownLicenceClassIsARefusalOnThisPathEvenThoughTheGateSaysYes()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.Unknown, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);

        var (run, port, sink) = await Run(LicenseClass.Unknown, guardEnabled: true);

        Report("unknown licence class", decision, run, port, sink);

        // The gate permitted it - and said it was not checking.
        Assert.True(decision.MayTransmit);
        Assert.False(decision.WasOverridden);
        Assert.Empty(decision.Citation);
        Assert.Contains("does not know", decision.Reason, StringComparison.OrdinalIgnoreCase);

        // The path refused it, with nothing on the wire.
        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, run.Outcome);
        Assert.Empty(port.Written);
        Assert.Equal(0, port.WritesAttempted);
        Assert.True(sink.WasNeverTouched);
        // The gate's own sentence, then what this path does about it - said once.
        Assert.StartsWith(decision.Reason, run.Reason, StringComparison.Ordinal);
        Assert.Contains(
            "will not key a transmission nothing checked",
            run.Reason,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **A guard switched off in Settings is a refusal on this path, and the
    /// gate's own answer was yes with the override recorded.**
    /// </summary>
    /// <remarks>
    /// This is unit 253's banked owner-class question answered for this path by
    /// construction and for no other. §0.2's "not bypassable from any send path"
    /// is a sentence about send paths, and this is one.
    /// </remarks>
    [Fact]
    public async Task AnOverriddenGuardIsARefusalOnThisPathEvenThoughTheGateSaysYes()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.Technician, Ft8On20Metres, TransmitMode.Data, guardEnabled: false);

        var (run, port, sink) = await Run(LicenseClass.Technician, guardEnabled: false);

        Report("guard switched off", decision, run, port, sink);

        // The gate permitted it, on the operator's own authority.
        Assert.True(decision.MayTransmit);
        Assert.True(decision.WasOverridden);

        // The path refused it, with nothing on the wire.
        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, run.Outcome);
        Assert.Empty(port.Written);
        Assert.Equal(0, port.WritesAttempted);
        Assert.True(sink.WasNeverTouched);
        Assert.Contains("switched off", run.Reason, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **A refusal says why, in the operator's words, and carries the citation.**
    /// </summary>
    /// <remarks>
    /// HM-DEC-012's shape: a control that refuses without saying why is the fault
    /// that ruling was about. The citation is the gate's own - this path does not
    /// compose one.
    /// </remarks>
    [Fact]
    public async Task TheRefusalSaysWhyAndCarriesTheGatesOwnCitation()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.Technician, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);

        var (run, _, _) = await Run(LicenseClass.Technician, guardEnabled: true);

        _output.WriteLine($"gate reason   : {decision.Reason}");
        _output.WriteLine($"gate citation : {decision.Citation}");
        _output.WriteLine($"run reason    : {run.Reason}");
        _output.WriteLine($"run citation  : {run.Citation}");

        Assert.NotEmpty(run.Reason);
        Assert.NotEmpty(run.Citation);
        Assert.Equal(decision.Reason, run.Reason);
        Assert.Equal(decision.Citation, run.Citation);
    }

    /// <summary>
    /// **Only a permit that carries a citation and was not overridden keys
    /// anything.**
    /// </summary>
    [Fact]
    public async Task APermitThatCarriesACitationAndWasNotOverriddenIsTheOneThatKeys()
    {
        var decision = new TransmitGuard()
            .Check(LicenseClass.General, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);

        var (run, port, sink) = await Run(LicenseClass.General, guardEnabled: true);

        Report("in privilege", decision, run, port, sink);

        Assert.True(decision.MayTransmit);
        Assert.False(decision.WasOverridden);
        Assert.NotEmpty(decision.Citation);

        Assert.Equal(Ft8TransmitOutcome.Sent, run.Outcome);
        Assert.True(run.Keyed);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, run.CameOutOfTransmit);
        Assert.Equal(1, sink.TimesCalled);
    }

    /// <summary>
    /// **`TransmitGuard.Check` still answers exactly what it answered before.**
    /// </summary>
    /// <remarks>
    /// The narrowing is new code being stricter than a general answer, which is
    /// the only direction a send path may differ in. If this test ever goes red,
    /// something loosened or tightened the gate for every caller in the tree,
    /// which is not this unit's to do.
    /// </remarks>
    [Fact]
    public void TheGateItselfIsUntouchedAndStillPermitsInThreeWays()
    {
        var guard = new TransmitGuard();

        var byPrivilege = guard.Check(
            LicenseClass.General, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);
        var unknownClass = guard.Check(
            LicenseClass.Unknown, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);
        var overridden = guard.Check(
            LicenseClass.Technician, Ft8On20Metres, TransmitMode.Data, guardEnabled: false);
        var refused = guard.Check(
            LicenseClass.Technician, Ft8On20Metres, TransmitMode.Data, guardEnabled: true);

        _output.WriteLine($"by privilege  : {byPrivilege}");
        _output.WriteLine($"unknown class : {unknownClass}");
        _output.WriteLine($"overridden    : {overridden}");
        _output.WriteLine($"refused       : {refused}");

        Assert.True(byPrivilege.MayTransmit);
        Assert.True(unknownClass.MayTransmit);
        Assert.True(overridden.MayTransmit);
        Assert.True(overridden.WasOverridden);
        Assert.False(refused.MayTransmit);
    }

    /// <summary>One run of the sequence with a fresh port and a fresh sink.</summary>
    private static async Task<(TransmitRun Run, FakeSerialPort Port, FakeTransmitAudioSink Sink)>
        Run(LicenseClass licenseClass, bool guardEnabled)
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(
            TheUnkeyHappensWhateverGoesWrongTests.Send(
                licenseClass: licenseClass,
                frequencyHz: Ft8On20Metres,
                guardEnabled: guardEnabled));

        return (run, port, sink);
    }

    /// <summary>What the gate said, what the path did, and what the radio saw.</summary>
    private void Report(
        string what,
        TransmitDecision decision,
        TransmitRun run,
        FakeSerialPort port,
        FakeTransmitAudioSink sink)
    {
        _output.WriteLine($"case          : {what}");
        _output.WriteLine(
            $"gate          : mayTransmit {decision.MayTransmit}, "
            + $"overridden {decision.WasOverridden}, citation \"{decision.Citation}\"");
        _output.WriteLine($"gate reason   : {decision.Reason}");
        _output.WriteLine($"path outcome  : {run.Outcome}, came out of tx {run.CameOutOfTransmit}");
        _output.WriteLine($"path reason   : {run.Reason}");
        _output.WriteLine($"writes tried  : {port.WritesAttempted}");
        _output.WriteLine(
            $"wire          : {(port.Written.Length == 0 ? "nothing" : TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written))}");
        _output.WriteLine($"sink touched  : {!sink.WasNeverTouched}");
    }
}
