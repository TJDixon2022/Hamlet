using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every condition that refuses to transmit still refuses.
/// </summary>
/// <remarks>
/// <para>**THE SEND BUTTON CAME BACK ON 2026-08-27**, after work instructions
/// 026, 027 and 028 removed it on a misreading of HM-DEC-098 — which governs the
/// automated cycle, not the operator keying by hand. HM-DEC-059 authorises the
/// manual send and always did. **A button that keys a transmitter is the one
/// control in this application where the interlocks have to be proved rather than
/// assumed** (§0.2).</para>
/// <para>**WHAT WAS ALREADY COVERED AND WHAT WAS NOT.** Nine of the eleven
/// refusal states had a test somewhere in `TransmitPrivilegeTests` or
/// `CwTransmitTests`. Two did not: a radio already transmitting, and a mode
/// nobody has read. Both are closed here, and the whole set is asserted in one
/// place so the count is checkable rather than inferred from three files.</para>
/// <para>**THE ELEVENTH IS NOT UNCOVERED, IT IS UNREACHABLE**, and that is a
/// finding rather than a gap. See
/// <see cref="ListenOnlyCannotHappenToMorseAndHereIsWhy"/>.</para>
/// <para>**NOTHING HERE TRANSMITS.** Each case asks the readiness check what it
/// would say; none of them reaches a sender, and no radio is connected on this
/// machine. Tim verifies at the rig.</para>
/// </remarks>
public sealed class EveryInterlockStillRefusesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each verdict is printed.</param>
    public EveryInterlockStillRefusesTests(ITestOutputHelper output)
        => _output = output;

    private static readonly DateTime Now =
        new(2026, 8, 27, 12, 0, 0, DateTimeKind.Utc);

    private static RigCapabilities Radio { get; } = new(
        "IC-7300", HasSpectrumScope: true, HasBuiltInCwKeyer: true,
        HasUsbAudio: true, CanTransmit: true, new[] { "40 m", "20 m" });

    private static RigCapabilities Receiver { get; } = new(
        "a receiver", HasSpectrumScope: false, HasBuiltInCwKeyer: false,
        HasUsbAudio: true, CanTransmit: false, new[] { "40 m" });

    /// <summary>A radio ready in every way, so one thing can be spoiled at a time.</summary>
    private static RigState Ready(
        int mode = (int)CivMode.Cw, int breakIn = 2, int transmitting = 0)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, mode, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, breakIn, "full", Now, "CI-V 16 47"),
            RigValue.Known(
                RigField.TransmitStatus, transmitting,
                transmitting == 0 ? "receiving" : "transmitting", Now, "CI-V 1C 00"),
        });

    /// <summary>A ready radio with one field never read at all.</summary>
    private static RigState Without(RigField field)
        => RigState.Empty.With(new[]
        {
            RigValue.Known(RigField.Mode, (int)CivMode.Cw, "CW", Now, "CI-V 04"),
            RigValue.Known(RigField.BreakIn, 2, "full", Now, "CI-V 16 47"),
            RigValue.Known(RigField.TransmitStatus, 0, "receiving", Now, "CI-V 1C 00"),
        }.Where(v => v.Field != field).ToArray());

    private static TransmitPrivileges Allowed
        => new(LicenseClass.General, 7_030_000, true);

    private void Say(string what, CwReadiness readiness)
        => _output.WriteLine(
            $"{what,-30} {readiness.State,-21} maySend={readiness.MaySend,-5} "
            + $"token={readiness.Reason}");

    /// <remarks>
    /// Proves the baseline: with everything in place, nothing refuses. Without
    /// this the other cases could all be passing for the wrong reason — a check
    /// that refuses everything refuses correctly and is worthless.
    /// </remarks>
    [Fact]
    public void AReadyRadioIsReady()
    {
        var readiness = TransmitReadiness.Check(
            connected: true, Radio, Ready(), Now, Allowed);

        Say("everything in place", readiness);

        Assert.Equal(CwReadyState.Ready, readiness.State);
        Assert.True(readiness.MaySend);
    }

    /// <remarks>
    /// <para>Proves the two interlocks nothing covered before the send button came
    /// back.</para>
    /// <para>**A RADIO ALREADY TRANSMITTING** is the one that matters most on the
    /// air: a second send while the first is running puts two messages on top of
    /// each other, and the operator hears only his own sidetone. **A MODE NOBODY
    /// HAS READ** is unknown rather than wrong, and unknown is not permission
    /// (HM-DEC-050) — the keyer sends nothing in a voice mode, so a send from an
    /// unread mode is a message that may simply vanish.</para>
    /// </remarks>
    [Fact]
    public void TheTwoThatNothingCoveredStillRefuse()
    {
        var transmitting = TransmitReadiness.Check(
            connected: true, Radio, Ready(transmitting: 1), Now, Allowed);

        Say("already transmitting", transmitting);

        Assert.Equal(CwReadyState.AlreadyTransmitting, transmitting.State);
        Assert.False(transmitting.MaySend);
        Assert.Equal("already_transmitting", transmitting.Reason);

        var unread = TransmitReadiness.Check(
            connected: true, Radio, Without(RigField.Mode), Now, Allowed);

        Say("mode never read", unread);

        Assert.Equal(CwReadyState.ModeUnknown, unread.State);
        Assert.False(unread.MaySend);
        Assert.Equal("mode_unknown", unread.Reason);

        // AND THE TWO UNKNOWNS ARE NOT THE SAME UNKNOWN. Not having read the
        // mode and not having read break-in call for different things, and
        // HM-DEC-077 keeps them apart in the record for exactly that reason.
        var breakIn = TransmitReadiness.Check(
            connected: true, Radio, Without(RigField.BreakIn), Now, Allowed);

        Say("break-in never read", breakIn);

        Assert.Equal(CwReadyState.BreakInUnknown, breakIn.State);
        Assert.NotEqual(unread.Reason, breakIn.Reason);
    }

    /// <remarks>
    /// <para>Proves the whole set in one place, each row spoiling exactly one
    /// thing about an otherwise ready radio, so the count is checkable.</para>
    /// <para>**AND EVERY REFUSAL SAYS WHY, IN WORDS.** A refusal with no sentence
    /// is a grey button the operator cannot argue with (HM-DEC-080), and this
    /// asserts `Detail` rather than `Reason` — the token is a machine string and
    /// is never empty, so asserting it would prove nothing about what reaches the
    /// screen.</para>
    /// </remarks>
    [Fact]
    public void EveryRefusalRefusesAndSaysWhyInWords()
    {
        var cases = new (string What, CwReadiness Readiness, CwReadyState Expected)[]
        {
            ("nothing connected", TransmitReadiness.Check(
                connected: false, null, Ready(), Now, Allowed),
                CwReadyState.NotConnected),

            ("radio cannot transmit", TransmitReadiness.Check(
                connected: true, Receiver, Ready(), Now, Allowed),
                CwReadyState.RadioCannotTransmit),

            ("already transmitting", TransmitReadiness.Check(
                connected: true, Radio, Ready(transmitting: 1), Now, Allowed),
                CwReadyState.AlreadyTransmitting),

            ("mode never read", TransmitReadiness.Check(
                connected: true, Radio, Without(RigField.Mode), Now, Allowed),
                CwReadyState.ModeUnknown),

            ("not in a Morse mode", TransmitReadiness.Check(
                connected: true, Radio, Ready(mode: (int)CivMode.Usb), Now, Allowed),
                CwReadyState.NotInMorse),

            ("break-in never read", TransmitReadiness.Check(
                connected: true, Radio, Without(RigField.BreakIn), Now, Allowed),
                CwReadyState.BreakInUnknown),

            ("break-in switched off", TransmitReadiness.Check(
                connected: true, Radio, Ready(breakIn: 0), Now, Allowed),
                CwReadyState.BreakInOff),

            ("license class unknown", TransmitReadiness.Check(
                connected: true, Radio, Ready(), Now,
                new TransmitPrivileges(LicenseClass.Unknown, 7_030_000, true)),
                CwReadyState.LicenseClassUnknown),

            ("frequency unknown", TransmitReadiness.Check(
                connected: true, Radio, Ready(), Now,
                new TransmitPrivileges(LicenseClass.General, 0, true)),
                CwReadyState.FrequencyUnknown),

            // 14.300 MHz is inside 20 m and above where a Technician may key.
            ("outside privileges", TransmitReadiness.Check(
                connected: true, Radio, Ready(), Now,
                new TransmitPrivileges(LicenseClass.Technician, 14_300_000, true)),
                CwReadyState.OutsidePrivileges),
        };

        foreach (var (what, readiness, expected) in cases)
        {
            Say(what, readiness);

            Assert.Equal(expected, readiness.State);

            Assert.False(
                readiness.MaySend,
                $"\"{what}\" no longer refuses, and this button keys a transmitter");

            Assert.False(
                string.IsNullOrWhiteSpace(readiness.Detail),
                $"\"{what}\" refuses without telling the operator why");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"{cases.Length} refusals, every one with a sentence behind it");
    }

    /// <remarks>
    /// <para>**PROVES THE ELEVENTH STATE CANNOT ARISE FROM A SEND**, which is why
    /// it has no refusal test and must not be counted as an uncovered
    /// interlock.</para>
    /// <para>`CwReadyState.ListenOnly` means the class holds this stretch but not
    /// in this mode. **Morse has no such stretch**: 97.305(a) permits CW on any
    /// frequency authorised to the control operator, which is why it is absent
    /// from the emission table, and `PrivilegePlan.ModeAllowed` returns true for
    /// `TransmitMode.Cw` before it looks at a single row. So a frequency either
    /// belongs to the class, and Morse is allowed, or it does not, and the answer
    /// is `OutsidePrivileges`.</para>
    /// <para>**IT IS LIVE CODE ALL THE SAME**, reached by the band map drawing
    /// listen-only stretches for data and phone. Nothing here says to delete it.
    /// Swept rather than argued: every class at every 5 kHz across the HF bands,
    /// asked in Morse.</para>
    /// </remarks>
    [Fact]
    public void ListenOnlyCannotHappenToMorseAndHereIsWhy()
    {
        var plan = new PrivilegePlan();

        var classes = new[]
        {
            LicenseClass.Novice, LicenseClass.Technician, LicenseClass.General,
            LicenseClass.Advanced, LicenseClass.Extra,
        };

        var refused = 0;
        var allowed = 0;
        var asked = 0;

        foreach (var cls in classes)
        {
            for (var hz = 1_700_000L; hz <= 29_800_000L; hz += 5_000)
            {
                asked++;

                var verdict = plan.Evaluate(cls, hz, TransmitMode.Cw);

                Assert.NotEqual(PrivilegeStatus.ModeNotAuthorised, verdict.Status);

                if (verdict.MayTransmit)
                {
                    allowed++;
                }
                else
                {
                    refused++;
                }
            }
        }

        _output.WriteLine(
            $"{asked} frequencies asked across {classes.Length} classes: "
            + $"{allowed} allow Morse, {refused} refuse it, "
            + "0 refuse it as a mode");

        // And the sweep must have found refusals, or it proved nothing: a plan
        // that allowed everything would also never say ModeNotAuthorised.
        Assert.True(refused > 0, "the sweep found nothing refused, so it proved nothing");
        Assert.True(allowed > 0, "the sweep found nothing allowed, so it proved nothing");
    }
}
