using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 227 task 3c and 3e: the identical slots, read by both decoders, paired slot by slot.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>What this is for, and it is not to raise a rate.</b> Step 6's must-pass criterion 2 — the
/// decode rate at -21 dB against the published figure — stands at 13 of 306, 4.2 per cent, against
/// a band of 40 per cent fixed in writing before unit 221's curve ever ran. Three instruments have
/// been pointed at it from inside this receiver and none moved it: unit 221 measured the curve, unit
/// 222 substituted four stages for oracle-perfect versions and got a flat budget, and unit 223 ran
/// an independently written soft decoder over the identical ratios and then measured that
/// <b>over the 292 trials this library failed, the true codeword scores higher than the word the
/// decoder settled on in zero of them.</b>
/// </para>
/// <para>
/// <b>So the instrument here is outside the receiver for the first time in the phase.</b> If
/// upstream's own program returns about thirteen too, this port is faithful at threshold and the
/// shortfall is inherited — which is the evidence the owner's remaining ruling waits on. If it
/// returns eighty, there is a defect in this port with a slot-by-slot address.
/// </para>
/// <para>
/// <b>Nothing is fixed, tuned, widened, raised or adopted by anything in this file.</b> Under the
/// phase plan's ruling that inheriting Goba's bugs is accepted, a row where upstream decodes better
/// is evidence and never an adoption, and whether this library may deliberately diverge from the pin
/// to hear better is the owner's question and is not resolved by any number here.
/// </para>
/// <para>
/// <b>The reading is fixed before the run</b>, in <see cref="Read"/>, transcribed from the
/// instruction rather than chosen after seeing the number.
/// </para>
/// </remarks>
public class Unit227MeasurementTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    /// <summary>
    /// <b>The rung the whole verdict sits on: requested -21.0, 306 trials, 51 messages by 6
    /// seeds.</b>
    /// </summary>
    [RequiresWorkingDecoderFact]
    public void BothDecodersOnTheIdenticalMinusTwentyOneSlots()
    {
        Rung(-21.0, "3c — THE MEASUREMENT");
    }

    /// <summary>
    /// <b>Task 3e, the named drop candidate: the same paired comparison two rungs up.</b>
    /// </summary>
    /// <remarks>
    /// <b>Two rungs turn a rate gap into a decibel gap</b>, which is the number the owner's ruling
    /// would actually be about. This library reads 24 per cent at -20 and 81 per cent at -19 on the
    /// recorded curve; where upstream's rate falls between them says how much of a decibel separates
    /// the two receivers rather than how many decodes.
    /// </remarks>
    [RequiresWorkingDecoderFact]
    public void BothDecodersTwoRungsUp()
    {
        Rung(-20.0, "3e — THE DECIBEL GAP, upper rung one");
        Rung(-19.0, "3e — THE DECIBEL GAP, upper rung two");
    }

    private void Rung(double requested, string what)
    {
        _output.WriteLine($"UNIT 227 TASK {what}: requested {requested:F1} dB");
        _output.WriteLine($"  decoder    : {Ft8Decoder.ExecutablePath}");
        _output.WriteLine(
            $"  draw rule  : seed + round(requested x 10), seeds "
            + $"{string.Join(", ", Ft8Step6Ladder.Seeds)}");
        _output.WriteLine(
            $"  fixture    : {Unit227Paired.OnGridHz:F2} Hz, offset {Unit227Paired.AlignedOffset}, "
            + $"{Unit227Paired.Rate} Hz, one signal per slot");
        _output.WriteLine(string.Empty);

        var slots = Unit227Paired.WalkRung(requested, _output.WriteLine);

        var ours = Unit227Paired.CountOurs(slots);
        var upstream = Unit227Paired.CountUpstream(slots);
        var paired = Unit227Paired.Pair(slots);
        var delivered = slots.Average(s => s.Delivered);

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  delivered  : {delivered:F3} dB, mean over {slots.Count} slots");
        _output.WriteLine(
            $"  gain stage : peak before scaling {slots.Max(s => s.Peak):F2}, and "
            + $"{100.0 * slots.Average(s => s.ClippedFraction):F1} % of samples would have hit "
            + "save_wav's clamp unscaled — which is why the slot is scaled to "
            + $"{Unit227Paired.TargetPeak} before it is written, and the scale changes no ratio.");
        _output.WriteLine(string.Empty);
        _output.WriteLine(ours.AsRow("ours,     on the same WAV files"));
        _output.WriteLine(upstream.AsRow("upstream, on the same WAV files"));
        _output.WriteLine(string.Empty);
        _output.WriteLine("  the paired counts, which is the sharp instrument:");
        _output.WriteLine($"    both returned it        : {paired.Both}");
        _output.WriteLine($"    ours only               : {paired.OursOnly}");
        _output.WriteLine($"    upstream only           : {paired.UpstreamOnly}");
        _output.WriteLine($"    neither                 : {paired.Neither}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(Read(ours, upstream));
        _output.WriteLine(string.Empty);

        // The slots upstream got and this library did not are the address a defect would have, so
        // they are listed individually whenever there are any — message, seed, and what upstream
        // printed for it.
        var address = slots.Where(s => !s.OursReturned && s.UpstreamReturned).ToArray();
        if (address.Length > 0)
        {
            _output.WriteLine($"  UPSTREAM ONLY, all {address.Length}, which is the address:");
            foreach (var slot in address)
            {
                _output.WriteLine($"    {slot.Label,-44} seed {slot.Seed}  {slot.UpstreamPrinted}");
            }

            _output.WriteLine(string.Empty);
        }

        // The other diagonal, listed for the same reason: a port that decodes what the pin cannot
        // is as much a finding as the reverse, and neither is a licence to change anything.
        var reverse = slots.Where(s => s.OursReturned && !s.UpstreamReturned).ToArray();
        if (reverse.Length > 0)
        {
            _output.WriteLine($"  OURS ONLY, all {reverse.Length}:");
            foreach (var slot in reverse)
            {
                _output.WriteLine($"    {slot.Label,-44} seed {slot.Seed}");
            }

            _output.WriteLine(string.Empty);
        }

        _output.WriteLine(
            "  NOTHING IS FIXED, TUNED, WIDENED, RAISED OR ADOPTED BY THIS TEST. It reports the "
            + "world it measured and stops; the deliberate-divergence question is the owner's.");
        _output.WriteLine(string.Empty);
    }

    /// <summary>
    /// <b>The reading, transcribed from the instruction and fixed before the run.</b>
    /// </summary>
    /// <remarks>
    /// <b>A night that chooses its interpretation after seeing the result is not a measurement.</b>
    /// The three worlds and the numbers that separate them are the instruction's, and anything the
    /// three do not cover is UNSETTLED rather than squeezed into the nearest one.
    /// </remarks>
    private static string Read(Unit227Paired.Side ours, Unit227Paired.Side upstream)
    {
        var (lower, upper) = ours.Interval;
        var v = upstream.Rate;

        // THE TIE, WHICH IS NOT A RE-READING. Wilson's lower bound at zero successes is zero in
        // exact arithmetic and lands a few parts in 10^17 either side of it in double, so an
        // upstream rate EQUAL to ours can read as "below the lower bound" purely on the sign of
        // that last place. Two rates that are the same number are world A by construction, and a
        // tolerance of a millionth of a per cent - far finer than one decode in 306, which is 0.33
        // per cent - restores the instruction's own reading rather than altering it.
        const double tie = 1e-6;

        if (v >= lower - tie && v <= upper + tie)
        {
            return "  WORLD A — AN INHERITED LIMIT. Upstream's rate lies inside the 95 per cent "
                + "Wilson interval of ours, so this port is as deaf as the code it was ported from "
                + "at this rung. Criterion 2 is not reachable by a faithful port and what remains "
                + "is the owner's ruling on deliberate divergence.";
        }

        if (v > upper + tie)
        {
            return "  WORLD B — A DEFECT IN THIS PORT. Upstream's rate is above the upper bound of "
                + "our interval. Criterion 2 is unit work again and the upstream-only slots listed "
                + "below are the address.";
        }

        if (v < lower - tie)
        {
            return "  WORLD C — THE PIN IS DEAFER THAN THIS PORT. Upstream's rate is below the "
                + "lower bound of our interval. This does not close criterion 2 and it is not a "
                + "licence to celebrate.";
        }

        return "  UNSETTLED. The three worlds do not cover this result. Both tables are printed "
            + "above and no world is named.";
    }
}
