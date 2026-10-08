using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A PASSBAND ALREADY KNOWN IS HELD UNTIL A NEW READING REPLACES IT** (work instruction 555, task 2, HM-DEC-259): what
/// <see cref="CwPassbandHold"/> hands the detector on each tick of a scripted rig state, and how many times the detector
/// rebuilds its bins.
/// </summary>
public sealed class APassbandIsHeldTests
{
    private static readonly DateTime At = new(2026, 10, 7, 20, 0, 0, DateTimeKind.Utc);

    private static RigState Radio(CivMode mode, double? pitch, double? width)
    {
        var values = new List<RigValue> { RigValue.Known(RigField.Mode, (int)mode, mode.ToString(), At, "CI-V 04") };

        values.Add(pitch is { } p ? RigValue.Known(RigField.CwPitch, p, $"{p} Hz", At, "CI-V 14 09") : RigValue.Unknown(RigField.CwPitch, "timed out"));
        values.Add(width is { } w ? RigValue.Known(RigField.FilterBandwidth, w, $"{w} Hz", At, "CI-V 1A 03") : RigValue.Unknown(RigField.FilterBandwidth, "timed out"));

        return RigState.Empty.With(values);
    }

    /// <remarks>An unread pitch or filter holds; a new reading replaces; another mode or nothing known gives the whole band.</remarks>
    [Fact]
    public void AnUnreadTickChangesNothing()
    {
        var hold = new CwPassbandHold();

        Assert.Equal(((double?)null, (double?)null), hold.Update(RigState.Empty));
        Assert.Equal(((double?)600, (double?)500), hold.Update(Radio(CivMode.Cw, 600, 500)));
        Assert.Equal(((double?)600, (double?)500), hold.Update(Radio(CivMode.Cw, null, 500)));
        Assert.Equal(((double?)600, (double?)500), hold.Update(Radio(CivMode.Cw, 600, null)));
        Assert.Equal(((double?)600, (double?)500), hold.Update(Radio(CivMode.Cw, null, null)));
        Assert.Equal(((double?)650, (double?)500), hold.Update(Radio(CivMode.Cw, 650, null)));
        Assert.Equal(((double?)null, (double?)null), hold.Update(Radio(CivMode.Usb, 650, 2400)));
        Assert.Equal(((double?)null, (double?)null), hold.Update(Radio(CivMode.Cw, null, 500)));
        Assert.Equal(((double?)600, (double?)500), hold.Update(Radio(CivMode.Cw, 600, 500)));
        Assert.Equal(((double?)null, (double?)null), hold.Update(RigState.Empty));
    }

    /// <remarks>Through the chain: a dropped read once and for three ticks rebuilds the bins once, from the whole band to the radio's.</remarks>
    [Fact]
    public void TheDetectorRebuildsOnlyWhenTheRadioChanges()
    {
        using var chain = new CwChain(8000);
        var known = Radio(CivMode.Cw, 600, 500);
        var dropped = Radio(CivMode.Cw, null, 500);

        foreach (var state in new[] { known, dropped, known, dropped, dropped, dropped, known })
        {
            chain.SetPassband(state);
        }

        Assert.Equal(1, chain.Detector.PassbandRebuilds);

        chain.SetPassband(Radio(CivMode.Cw, 650, 500));

        Assert.Equal(2, chain.Detector.PassbandRebuilds);
    }
}
