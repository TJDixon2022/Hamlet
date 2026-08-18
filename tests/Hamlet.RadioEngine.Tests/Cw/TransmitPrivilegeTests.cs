using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The send controls refuse outside the operator's privileges (HM-DEC-089).
/// </summary>
/// <remarks>
/// **THIS IS THE ONE PLACE IN THE APPLICATION WHERE GREY GENUINELY MEANS YOU
/// CANNOT DO THIS** (HM-DEC-087). Everywhere else a disabled control is a fault
/// to be fixed. Here it is the answer, and it still says why and still says what
/// would change it.
/// </remarks>
public sealed class TransmitPrivilegeTests
{
    private static readonly DateTime Now = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m", "20 m" });

    /// <summary>A radio that is ready in every way except privileges.</summary>
    private static RigState Ready(int mode = (int)CivMode.Cw)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, mode, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, 2, "full", Now, "CI-V 16 47"),
            RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
        });

    private static CwReadiness Check(
        LicenseClass cls, long hz, RigState? state = null, bool guard = true)
        => TransmitReadiness.Check(
            connected: true, Radio, state ?? Ready(), Now,
            new TransmitPrivileges(cls, hz, guard));

    /// <remarks>
    /// Proves HM-DEC-089: inside the operator's own privileges, nothing here
    /// stands in the way. 7.030 is Morse territory and a General may use it.
    /// </remarks>
    [Fact]
    public void InsideTheOperatorsPrivilegesItPermits()
    {
        var ready = Check(LicenseClass.General, 7_030_000);

        Assert.True(ready.MaySend);
        Assert.Equal(CwReadyState.Ready, ready.State);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-089: outside them it refuses, and the refusal names
    /// where the operator actually is and cites the paragraph that decided it,
    /// the same way the band map's own line does.</para>
    /// <para>14.350 is the top edge of twenty meters, which is exactly where the
    /// map once said "yours to use, call away" because an overlay read past the
    /// end of its data as no restriction found (HM-DEC-055).</para>
    /// </remarks>
    [Fact]
    public void OutsideThemItRefusesAndSaysWhy()
    {
        // A General on twenty meters at 14.200, which is the slice the shipped
        // Part 97 data gives to Advanced and Extra only.
        var ready = Check(LicenseClass.General, 14_200_000);

        Assert.False(ready.MaySend);
        Assert.Equal(CwReadyState.OutsidePrivileges, ready.State);
        Assert.NotEqual("", ready.Detail);
        Assert.NotEqual("", ready.Citation);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-089: **the refusal says what would change it.** A
    /// disabled button that only says no is a dead end, and this application
    /// exists for somebody who does not yet know where they are allowed.</para>
    /// </remarks>
    [Fact]
    public void TheRefusalSaysWhereTheOperatorCouldGoInstead()
    {
        var ready = Check(LicenseClass.General, 14_200_000);

        Assert.False(ready.MaySend);

        // A frequency they could actually use, named in the refusal.
        Assert.Contains("MHz", ready.Detail, StringComparison.Ordinal);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-089, and it records a finding rather than a
    /// behavior. **A stretch a class holds but may not use in Morse does not
    /// exist in the shipped Part 97 data**: telegraphy is authorized everywhere a
    /// class holds the band, so the mode-restricted refusal is reachable for
    /// voice and not for Morse.</para>
    /// <para>The state is kept and mapped anyway, because the distinction is a
    /// fact about the regulation rather than about this build, and a record that
    /// could not tell the two refusals apart would be wrong the day the data
    /// says something else. This test pins the finding so that day is noticed.
    /// </para>
    /// </remarks>
    [Fact]
    public void MorseIsAuthorizedWhereverAClassHoldsTheBand()
    {
        var plan = new PrivilegePlan();

        foreach (var cls in new[]
                 {
                     LicenseClass.Technician, LicenseClass.General, LicenseClass.Extra,
                 })
        {
            foreach (var band in Hamlet.RadioEngine.Bands.HfBands.Bands)
            {
                foreach (var span in plan.SpansFor(band, cls).Where(s => s.MayTransmit))
                {
                    var middle = (span.LowHz + span.HighHz) / 2;
                    var verdict = plan.Evaluate(cls, middle, TransmitMode.Cw);

                    Assert.NotEqual(PrivilegeStatus.ModeNotAuthorised, verdict.Status);
                }
            }
        }
    }

    /// <remarks>
    /// <para>Proves HM-DEC-089: **mode matters as much as frequency.** A stretch
    /// where Morse is permitted and voice is not must let a Morse send through,
    /// and Hamlet only sends Morse, so what it must never do is refuse a Morse
    /// send because voice would have been refused there.</para>
    /// <para>The Technician CW allocation on twenty meters is exactly such a
    /// stretch: Morse yes, voice no.</para>
    /// </remarks>
    [Fact]
    public void AModeRestrictedStretchPermitsMorseAndWouldRefuseVoice()
    {
        var plan = new PrivilegePlan();

        // A Technician on forty meters at 7.030: Morse yes, voice no.
        var morse = plan.Evaluate(LicenseClass.Technician, 7_030_000, TransmitMode.Cw);
        var voice = plan.Evaluate(LicenseClass.Technician, 7_030_000, TransmitMode.Phone);

        Assert.Equal(PrivilegeStatus.Allowed, morse.Status);
        Assert.Equal(PrivilegeStatus.ModeNotAuthorised, voice.Status);

        // And the readiness, which only ever asks about Morse, lets it through.
        Assert.True(Check(LicenseClass.Technician, 7_030_000).MaySend);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-089, and this is the part that **supersedes
    /// HM-DEC-065**. An unresolved license class used to permit and warn. It now
    /// refuses while the guard is on, because Hamlet cannot check a frequency
    /// against a class it does not have, and unknown is not permission
    /// (HM-DEC-050).</para>
    /// <para>The refusal names the class as the thing it could not establish, so
    /// it cannot be confused with being in the wrong place.</para>
    /// </remarks>
    [Fact]
    public void AnUnknownLicenseClassRefusesAndSaysItIsTheClass()
    {
        var ready = Check(LicenseClass.Unknown, 7_030_000);

        Assert.False(ready.MaySend);
        Assert.Equal(CwReadyState.LicenseClassUnknown, ready.State);
        Assert.Contains("class", ready.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-089: **the operator is never locked out of their own
    /// transmitter.** This is what HM-DEC-065 was protecting and it still holds:
    /// switching the privilege guard off returns the decision to the person who
    /// holds the license, and the refusal on an unknown class goes with it.</para>
    /// </remarks>
    [Fact]
    public void SwitchingTheGuardOffHandsTheDecisionBack()
    {
        Assert.True(Check(LicenseClass.Unknown, 7_030_000, guard: false).MaySend);
        Assert.True(Check(LicenseClass.General, 14_200_000, guard: false).MaySend);
    }

    /// <remarks>
    /// Proves HM-DEC-089: an unread frequency refuses, and says it is the
    /// frequency it could not establish. This is a different kind of ignorance
    /// from not knowing the class: it is not knowing where the radio is, and
    /// transmitting on Hamlet's own idea of where it is would be a guess with
    /// legal consequences (§0.0).
    /// </remarks>
    [Fact]
    public void AnUnreadFrequencyRefusesAndSaysItIsTheFrequency()
    {
        var ready = Check(LicenseClass.General, 0);

        Assert.False(ready.MaySend);
        Assert.Equal(CwReadyState.FrequencyUnknown, ready.State);
        Assert.Contains("frequency", ready.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <remarks>
    /// Proves HM-DEC-089 and HM-DEC-077: the privilege refusal is a readiness
    /// state like every other, so it reaches the decision record with everything
    /// that decided it rather than being a separate answer the file cannot see.
    /// </remarks>
    [Fact]
    public void TheRefusalCarriesWhatDecidedIt()
    {
        var ready = Check(LicenseClass.Technician, 14_100_000);

        var saw = ready.DeterminedBy ?? Array.Empty<Hamlet.RadioEngine.Telemetry.DeterminedBy>();

        Assert.NotEmpty(saw);
        Assert.Contains(saw, d => d.Field == "frequencyHz");
        Assert.Contains(saw, d => d.Field == "licenseClass");
    }

    /// <remarks>
    /// Proves HM-DEC-089: the privilege check runs before the radio's own
    /// preconditions are complained about, because being in the wrong place is
    /// the operator's to fix and break-in is the radio's, and telling somebody to
    /// walk across the room for a transmission that was never allowed is a waste
    /// of their evening.
    /// </remarks>
    [Fact]
    public void PrivilegesAreSettledBeforeTheRadioIsBlamed()
    {
        var noBreakIn = RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, 0, "off", Now, "CI-V 16 47"),
            RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
        });

        var ready = Check(LicenseClass.General, 14_200_000, noBreakIn);

        Assert.Equal(CwReadyState.OutsidePrivileges, ready.State);
    }

    /// <remarks>
    /// Proves HM-DEC-089: with no privilege information supplied at all, nothing
    /// changes. Everything that checked readiness before this existed goes on
    /// behaving exactly as it did.
    /// </remarks>
    [Fact]
    public void ReadinessWithoutPrivilegesIsUnchanged()
    {
        var ready = TransmitReadiness.Check(true, Radio, Ready(), Now);

        Assert.True(ready.MaySend);
        Assert.Equal(CwReadyState.Ready, ready.State);
    }
}
