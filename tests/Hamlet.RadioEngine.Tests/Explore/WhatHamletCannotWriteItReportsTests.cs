using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The passband controls Hamlet cannot set are reported, and an unread one is
/// never a quiet yes.
/// </summary>
/// <remarks>
/// <para>**SAYING THE RADIO IS READY WHILE A HAND-SET PBT CLOSES THE WINDOW IS
/// THE PRIME DIRECTIVE BROKEN ON THE ONE SENTENCE THE OPERATOR ACTS ON** (work
/// instruction 051, task 4, §0.0). Hamlet writes the mode and the receive side;
/// it has no PBT write at all and RIT is out of scope, so the honest move is to
/// report and to stop claiming.</para>
/// <para>**AND THE SUPPRESSION IS DRIVEN BY UNCERTAINTY, NOT ONLY BY A BAD
/// READING.** Two of the three controls have no documented read in this
/// repository, so today the claim can never be made — which is the honest state
/// and not a defect.</para>
/// </remarks>
public sealed class WhatHamletCannotWriteItReportsTests
{
    private readonly ITestOutputHelper _output;

    public WhatHamletCannotWriteItReportsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A centred PBT with everything read says nothing and claims nothing.</summary>
    [Fact]
    public void EverythingReadAndCentredIsSilentAndMayClaim()
    {
        var state = Read(PassbandReport.PbtCentre, inner: PassbandReport.PbtCentre, rit: 0);

        var report = PassbandReport.ForState(state);

        Assert.Equal("", report.Sentence);
        Assert.False(report.IsOffCentre);
        Assert.False(report.SomethingWasNotRead);
        Assert.True(report.CanClaimAudible);
    }

    /// <summary>A PBT off centre says so, names the remedy, and stops claiming.</summary>
    /// <remarks>
    /// The remedy is the control on the front of the radio, because that is what
    /// somebody staring at a silent receiver needs.
    /// </remarks>
    [Fact]
    public void APbtOffCentreNamesTheRemedyAndClaimsNothing()
    {
        var state = Read(200, inner: PassbandReport.PbtCentre, rit: 0);

        var report = PassbandReport.ForState(state);

        _output.WriteLine(report.Sentence);

        Assert.True(report.IsOffCentre);
        Assert.False(report.CanClaimAudible);
        Assert.Contains("TWIN PBT CLR", report.Sentence, StringComparison.Ordinal);
    }

    /// <summary>An unread control is not a quiet yes.</summary>
    /// <remarks>
    /// **THIS IS THE ONE THAT MATTERS.** Unknown and centred are different facts
    /// about the radio and the same fact about what Hamlet knows, and the old
    /// failure mode of this project is the second being reported as the first.
    /// </remarks>
    [Fact]
    public void AnUnreadControlSuppressesTheClaim()
    {
        var state = RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.TwinPbtOuter, PassbandReport.PbtCentre, "centred",
                DateTime.UtcNow, "14 08"),
            RigValue.Undocumented(RigField.TwinPbtInner, "no command recorded"),
            RigValue.Undocumented(RigField.Rit, "no command recorded"),
        });

        var report = PassbandReport.ForState(state);

        _output.WriteLine(report.Sentence);

        Assert.False(report.IsOffCentre);
        Assert.True(report.SomethingWasNotRead);
        Assert.False(report.CanClaimAudible);
        Assert.NotEqual("", report.Sentence);
    }

    /// <summary>
    /// On this radio, today, nothing can claim the block is audible.
    /// </summary>
    /// <remarks>
    /// **THE STATE OF THE WORLD, ASSERTED SO IT CANNOT DRIFT SILENTLY.** The
    /// inner Twin PBT and RIT have no documented read in this repository, so a
    /// real rig cannot satisfy the claim. When somebody reads p. 19-4
    /// column-aware and adds them, this test goes red and is the place the change
    /// gets noticed.
    /// </remarks>
    [Fact]
    public void TodayTheRealRadioCanNeverClaimIt()
    {
        Assert.True(
            CivReads.Undocumented.ContainsKey(RigField.TwinPbtInner),
            "the inner Twin PBT gained a read, so revisit the claim rule");

        Assert.True(
            CivReads.Undocumented.ContainsKey(RigField.Rit),
            "RIT gained a read, so revisit the claim rule");

        var state = RigState.Empty.With(
            RigValue.Known(
                RigField.TwinPbtOuter, PassbandReport.PbtCentre, "centred",
                DateTime.UtcNow, "14 08"));

        Assert.False(PassbandReport.ForState(state).CanClaimAudible);
    }

    /// <summary>The outer Twin PBT read exists and is sub-command 08, not 09.</summary>
    /// <remarks>
    /// **09 IS THE CW PITCH AND THAT CONFUSION COST WEEKS** (HM-DEC-050). The two
    /// live next to each other on a two-column page, which is exactly how the
    /// pitch landed on the wrong row, so the pairing is asserted rather than
    /// remembered.
    /// </remarks>
    [Fact]
    public void ThePbtReadIsZeroEightAndThePitchIsZeroNine()
    {
        var pbt = CivReads.For(RigField.TwinPbtOuter);
        var pitch = CivReads.For(RigField.CwPitch);

        Assert.NotNull(pbt);
        Assert.NotNull(pitch);

        Assert.Equal(0x14, pbt!.Command);
        Assert.Equal(new byte[] { 0x08 }, pbt!.SubCommand);
        Assert.Equal(new byte[] { 0x09 }, pitch!.SubCommand);
    }

    /// <summary>Nothing added here writes to the radio.</summary>
    /// <remarks>
    /// **THERE IS NO PBT WRITE AND THIS UNIT ADDS NONE** (§0.4, and the order says
    /// so outright). A `14 08` carrying a payload would move the operator's
    /// passband while trying to ask a question.
    /// </remarks>
    [Fact]
    public void NoPassbandWriteExists()
    {
        var writes = typeof(CivWrites)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(CivWrite))
            .Select(p => (CivWrite)p.GetValue(null)!)
            .ToList();

        var pbt = writes
            .Where(w => w.Command == 0x14 && w.Sub.Length == 1 && w.Sub[0] == 0x08)
            .ToList();

        _output.WriteLine($"{writes.Count} writes in the table, {pbt.Count} of them 14 08");

        Assert.Empty(pbt);
    }

    /// <summary>A ledger with the three controls in it.</summary>
    private static RigState Read(int outer, int inner, int rit)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(
                RigField.TwinPbtOuter, outer, outer.ToString(),
                DateTime.UtcNow, "14 08"),
            RigValue.Known(
                RigField.TwinPbtInner, inner, inner.ToString(),
                DateTime.UtcNow, "test"),
            RigValue.Known(
                RigField.Rit, rit, rit == 0 ? "off" : "on",
                DateTime.UtcNow, "test"),
        });
}
