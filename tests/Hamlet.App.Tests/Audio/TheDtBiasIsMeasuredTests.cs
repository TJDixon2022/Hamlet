using System.Globalization;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Audio;

/// <summary>
/// Work instruction 251, task 7: where the `dt` bias is, measured before
/// anything is changed.
/// </summary>
/// <remarks>
/// <para>**THE OBSERVATION.** The operator's PC clock is within 17 ms of UTC -
/// `w32tm` stripchart, five samples, 2026-09-05 17:34 - and every `dt` on his
/// screen reads +0.6 to +1.6, clustered near +0.9, **all positive**. Real
/// stations scatter either side of zero, because they run the same software
/// against the same time servers and their errors are not all in one direction. A
/// uniform positive offset applied to every station is a systematic error
/// somewhere in this application, not propagation.</para>
/// <para>**THIS MEASURES AND DOES NOT CORRECT.** A synthesized transmission is
/// placed at a known offset inside a slot and put through the same route
/// `Ft8Reader.Read` takes on live audio - the cutter, the resample to 12 kHz, the
/// waterfall and the decoder - and what comes back in `Ft8Decode.OffsetSeconds`
/// is printed. Several known placements, so the error can be characterised as an
/// offset, a scale, or neither.</para>
/// <para>**A CONSTANT CORRECTION IS FORBIDDEN** and would be this unit's point
/// missed entirely: a fudge factor that makes the column look right hides a real
/// timing error behind a number nobody can check, which is §0.0's fault wearing a
/// lab coat.</para>
/// <para>**WHAT IT FOUND, 2026-09-05.** The error is a **constant offset and not
/// a scale**, and the bias is **in the decode path rather than in the live
/// capture path** - a signal synthesized at a known placement and never near a
/// radio comes back displaced by the same amount at every placement. The
/// component that is Hamlet's own is a **reference point**: `Ft8Decode`'s
/// `OffsetSeconds` is `Ft8Candidate.TimeSeconds`, which the port documents as
/// *"seconds from the start of the analysis"* - the slot boundary - while `dt` in
/// this mode universally means how early or late a station was against the
/// moment a transmission is supposed to begin. Those are different quantities and
/// they differ by however far into the slot a transmission nominally starts.
/// **That number is not in this tree**, so nothing here corrects for it; see
/// section 4 of the unit's report.</para>
/// </remarks>
public sealed class TheDtBiasIsMeasuredTests
{
    private const int Rate = 48_000;

    /// <summary>The recording is two slots long, so a boundary falls inside it.</summary>
    private const int Seconds = 30;

    /// <summary>Which boundary the transmission is placed against.</summary>
    /// <remarks>
    /// The second one, so a negative placement has recording in front of it to
    /// live in. The same boundary is used for every placement, so the only thing
    /// changing between measurements is the placement itself.
    /// </remarks>
    private const double BoundarySeconds = 15.0;

    /// <summary>
    /// One sub-symbol step of the decoder's own time search, in seconds.
    /// </summary>
    /// <remarks>
    /// FT8's symbol is 0.16 s and the search oversamples it, so the finest
    /// distinction the decoder can draw is a fraction of that. Every figure this
    /// class prints is quantized to it, and a spread of one step across
    /// placements is agreement rather than disagreement.
    /// </remarks>
    private const double SubSymbolSeconds = 0.04;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the test.</summary>
    /// <param name="output">Where the measured offsets are printed.</param>
    public TheDtBiasIsMeasuredTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// The reported offset tracks the real placement one for one, so the error is
    /// a constant and not a scale.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE ASSERTION AND THE BIAS ITSELF IS NOT.** Whichever way
    /// the reference point is eventually ruled, the reported figure has to move
    /// exactly as far as the transmission moved: a station a second later than
    /// another must read a second later, or the column cannot be used to compare
    /// two stations at all. A test that pinned the constant would go red the day
    /// somebody corrected it, which is the opposite of a guard.</para>
    /// <para>**AND IT ANSWERS THE QUESTION THE UNIT ASKED.** The constant is
    /// printed, and it is measured on audio that has never been near a radio - so
    /// whatever it is, it is not the antenna, the sound card or the tap.</para>
    /// </remarks>
    [Fact]
    public void TheReportedOffsetTracksThePlacementOneForOne()
    {
        // **THE LAST TWO ARE NOT ARBITRARY.** 0.5 s is where a transmission
        // nominally begins on the air; 1.18 s is where `Ft8Waveform` puts the
        // signal when it writes a whole slot, splitting the spare evenly across
        // the two ends. They are the two candidate zeros, and the report hands
        // the choice between them up rather than picking one.
        var placements = new[] { -1.0, 0.0, 1.0, 0.5, 1.18 };

        var errors = new List<double>();

        _output.WriteLine("placed      reported     error");
        _output.WriteLine("-------------------------------");

        foreach (var placed in placements)
        {
            var measured = MeasureAt(placed);

            Assert.True(
                measured is not null,
                "nothing decoded with the transmission placed at "
                + Signed(placed) + " s, so no offset could be measured");

            var error = measured!.Value - placed;

            errors.Add(error);

            _output.WriteLine(
                Signed(placed) + "      " + Signed(measured.Value)
                + "      " + Signed(error));
        }

        var mean = errors.Average();
        var spread = errors.Max() - errors.Min();

        _output.WriteLine("");
        _output.WriteLine("constant offset  " + Signed(mean) + " s");
        _output.WriteLine("spread           " + spread.ToString(
            "0.000", CultureInfo.InvariantCulture) + " s");
        _output.WriteLine("");
        _output.WriteLine(
            spread <= (2 * SubSymbolSeconds) + 1e-9
                ? "CHARACTERISED AS A CONSTANT OFFSET. The error does not change "
                  + "with the placement, so it is not a scale and not a drift."
                : "NOT A CONSTANT. The error changes with the placement, which "
                  + "would make it a scale error and a different fault entirely.");
        _output.WriteLine(
            "MEASURED ON SYNTHESIZED AUDIO THAT WAS NEVER NEAR A RADIO, so the "
            + "constant is in the decode path and not in the capture path - not "
            + "the antenna, not the sound card, not the tap's anchor.");

        // **THE SPREAD, NOT THE CONSTANT.** Two sub-symbol steps of tolerance:
        // one for the placement landing between search steps and one for the
        // reported figure being quantized to them.
        Assert.True(
            spread <= (2 * SubSymbolSeconds) + 1e-9,
            "the error changed by " + spread.ToString("0.000", CultureInfo.InvariantCulture)
            + " s across the placements, which is more than the decoder's own "
            + "resolution can account for - it is a scale error rather than the "
            + "constant offset this unit measured");
    }

    /// <summary>
    /// The bias is not in the live capture path, because there is no capture path
    /// in this measurement at all.
    /// </summary>
    /// <remarks>
    /// **THE TWO ANSWERS THE INSTRUCTION ASKED THIS TO CHOOSE BETWEEN.** A signal
    /// placed at a known moment inside a recording, with the clock offset set to
    /// zero, reaches the decoder through the cutter and the resampler and nothing
    /// else. If what comes back is displaced from where it was put, the
    /// displacement is in that path - and the tap, the audio latency and the
    /// sample-to-moment mapping are all downstream of nothing here.
    /// </remarks>
    [Fact]
    public void ASignalOnTheBoundaryDoesNotComeBackOnTheBoundary()
    {
        var measured = MeasureAt(0.0);

        Assert.True(measured is not null, "nothing decoded on the boundary");

        _output.WriteLine(
            "a transmission beginning exactly on the slot boundary is reported "
            + "at dt " + Signed(measured!.Value));

        // **A RECORDED FINDING RATHER THAN A HOPE.** The displacement is real and
        // this asserts that it is real, so the day somebody rules the reference
        // point and corrects it, this test says so by failing and is updated
        // deliberately rather than having quietly passed all along.
        Assert.True(
            measured.Value > SubSymbolSeconds,
            "a transmission on the boundary now reports dt "
            + Signed(measured.Value)
            + ", which is on the boundary - the reference point has been "
            + "corrected and this test's finding is out of date");
    }

    /// <summary>
    /// Put one transmission at a known offset through the live path, and return
    /// what `dt` came back.
    /// </summary>
    /// <param name="offsetSeconds">
    /// Where the transmission's first sample sits, relative to its slot's
    /// boundary. Negative means it began before the boundary.
    /// </param>
    /// <returns>The reported `dt`, or null where nothing decoded.</returns>
    /// <remarks>
    /// <para>**IT GOES THROUGH `Ft8Reader.Read` AND NOT AROUND IT**, which is the
    /// instruction's own requirement: the cutter's boundary arithmetic, the
    /// resample to 12 kHz, the waterfall and the decoder are all in the path,
    /// because any of them could be where a fraction of a second goes missing.</para>
    /// <para>**`Synthesize` AND NOT `SynthesizeSlot`.** The slot form wraps the
    /// signal in silence at both ends - 1.18 s of it at 48 kHz - so placing a
    /// slot at the boundary places the *signal* 1.18 s after it, and the
    /// placement this method claims to control would not be the placement being
    /// measured. The bare signal starts at its first sample and nowhere else.</para>
    /// <para>**THE RECORDING ENDS ON A QUARTER MINUTE AND THE CLOCK OFFSET IS
    /// ZERO**, so the boundaries land at whole multiples of 15 s from the first
    /// sample and the placement is exact in samples. A recording ending anywhere
    /// else would put a rounding of its own between the placement and the answer,
    /// which is a second unknown in a measurement that has one.</para>
    /// </remarks>
    private double? MeasureAt(double offsetSeconds)
    {
        var samples = new float[Rate * Seconds];

        var at = (int)Math.Round((BoundarySeconds + offsetSeconds) * Rate);

        var signal = Ft8Waveform.Synthesize(
            Ft8SymbolEncoder.Encode(Packed("CQ", "K1ABC", "FN42")), Rate, 1240f);

        for (var i = 0; i < signal.Length && at + i < samples.Length; i++)
        {
            samples[at + i] += signal[i];
        }

        var endedAt = new DateTime(2026, 9, 5, 21, 30, 30, DateTimeKind.Utc);

        var heard = Ft8Reader.Read(
            new MonoAudio(Rate, samples),
            endedAt,
            new ClockOffset(0, endedAt));

        var boundary = endedAt.AddSeconds(-(Seconds - BoundarySeconds));

        var decode = heard.Decodes.FirstOrDefault(
            d => d.SlotStartUtc == boundary && d.Message == "CQ K1ABC FN42");

        return decode?.OffsetSeconds;
    }

    private static string Signed(double seconds)
        => seconds.ToString("+0.000;-0.000; 0.000", CultureInfo.InvariantCulture);

    private static byte[] Packed(string to, string from, string payload)
    {
        var message = new byte[Ft8StandardMessage.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack(to, from, payload, message));

        return message;
    }
}
