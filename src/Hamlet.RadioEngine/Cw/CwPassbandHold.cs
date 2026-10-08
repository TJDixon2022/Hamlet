using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **A PASSBAND ALREADY KNOWN IS HELD UNTIL A NEW READING REPLACES IT** (work instruction 555, task 2, HM-DEC-259): the radio's
/// CW pitch and filter width the detector sums over, kept across a tick where either goes unread.
/// </summary>
/// <remarks>
/// <para>**WHY.** The app sets the detector's passband from the rig state twenty times a second, and a read of the radio's
/// pitch or filter that times out leaves that field unknown for a poll. The detector then summed the whole band and rebuilt its
/// bins, and rebuilt them again when the reading came back: the sender's window shut, its bins' history went, and on W1AW's
/// session played with the pitch dropped for one tick every twenty seconds and three ticks every twenty the text differed in
/// 101 places from the same audio with the passband steady. A missed poll says nothing about the radio; the radio's pitch and
/// filter change only when it reports a different value.</para>
/// <para>**WHAT CHANGES IT.** A known pitch or width that differs from the held one replaces it. A mode read as anything but CW
/// or CW-R gives the whole band and lets the hold go, since the radio's filter no longer centres on its CW pitch. A rig state
/// with nothing known at all, which is what Hamlet holds before the radio first answers and after it disconnects, does the same.
/// An unknown mode alone holds, as an unknown pitch does.</para>
/// <para>**HOW LONG.** As long as the radio stays connected and in CW, however many ticks go unread: the hold carries what was
/// last read, not a guess, and the rig state's own provenance still says the field is unread wherever that is shown.</para>
/// </remarks>
public sealed class CwPassbandHold
{
    private double? _pitchHz;
    private double? _widthHz;

    /// <summary>The passband for this rig state: what it reads, or what was last read where a field is unread.</summary>
    /// <param name="state">What Hamlet knows about the radio.</param>
    /// <returns>The pitch and width to hand <see cref="CwEnvelopeDetector.SetPassband"/>; nulls for the whole band.</returns>
    public (double? PitchHz, double? WidthHz) Update(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var mode = state[RigField.Mode];

        if (state.KnownCount == 0 || (mode is { IsKnown: true, Number: { } m } && !CivValues.IsCw((CivMode)(int)m)))
        {
            _pitchHz = null;
            _widthHz = null;

            return (null, null);
        }

        if (mode.IsKnown && state[RigField.CwPitch] is { IsKnown: true, Number: { } hz } && hz > 0)
        {
            _pitchHz = hz;
        }

        if (mode.IsKnown && state.FilterBandwidthHz is { } width && width > 0)
        {
            _widthHz = width;
        }

        // Both or neither: a pitch without a width, or the reverse, is not a passband, and the detector sums the whole band.
        return _pitchHz is not null && _widthHz is not null ? (_pitchHz, _widthHz) : (null, null);
    }
}
