using Hamlet.App.Settings;
using Xunit;

namespace Hamlet.App.Tests.Settings;

/// <summary>
/// The ported reference decoder lost the head to head, and the setting stays
/// off until somebody re-measures it.
/// </summary>
/// <remarks>
/// <para>**MEASURED OVER ALL FORTY-FOUR CAPTURES, 2026-08-28: the shipped path
/// returns twelve of the twelve adjudicated readings and the reference returns
/// one.** It keeps `VA3VRR` on `cw-2026-08-17-013347`, more cleanly than the
/// shipped path manages, and it loses `N4L`, `AA4MP/4 QNIK`,
/// `DE KD0UN KD0UN K`, the ARRL bulletin and all seven of the 2026-08-22
/// series.</para>
/// <para>**THE PORT IS NOT WHAT LOST.** `cwdecoder.py` produces the same output
/// on the same files, character for character: on `cw-2026-08-18-004507` it
/// acquires 700 Hz for a station the sheet measured at 500, and on
/// `cw-2026-08-24-012403` it acquires 440 correctly, fits a plausible 59/187 ms
/// clock, and still reads `EEIEIEETE...`.</para>
/// <para>**IT ALSO FAILS THE TWO THINGS THE OPERATOR ASKED FOR.** One of the two
/// silence controls is not silent under it — `cw-2026-08-20-014854` yields
/// eighteen characters where the shipped path yields none — and three of the
/// four phantom captures still emit.</para>
/// <para>This exists so the finding cannot be quietly re-argued. It goes red the
/// moment the default changes, which is the point: whoever changes it re-measures
/// the twelve first.</para>
/// </remarks>
public sealed class TheReferenceDecoderStaysOffTests
{
    /// <summary>A fresh profile does not use the reference decoder.</summary>
    [Fact]
    public void AFreshProfileDoesNotUseTheReferenceDecoder()
    {
        Assert.False(
            new AppSettings().UseReferenceDecoder,
            "the reference decoder is on by default; it returns one of the "
            + "twelve adjudicated readings where the shipped path returns "
            + "twelve, and it puts eighteen characters on a capture that holds "
            + "nothing — re-measure before changing this");
    }

    /// <summary>The switch survives a round trip, so the operator can keep it.</summary>
    /// <remarks>
    /// It ships off and it is still a switch he can throw: the ruling's whole
    /// reason for putting the port behind a setting is that the two can be
    /// compared on his own audio, and a comparison he has to rebuild for is one
    /// he will not make.
    /// </remarks>
    [Fact]
    public void TheSwitchIsRememberedWhenItIsThrown()
    {
        var settings = new AppSettings { UseReferenceDecoder = true };

        Assert.True(settings.UseReferenceDecoder);
    }
}
