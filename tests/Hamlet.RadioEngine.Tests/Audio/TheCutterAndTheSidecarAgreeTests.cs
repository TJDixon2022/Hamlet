using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 238, task 4: the cutter cuts what the transmission needs,
/// and one function decides both its refusal and the sidecar's line.
/// </summary>
/// <remarks>
/// <para>**THEY CONTRADICTED EACH OTHER IN CONSECUTIVE LINES.** On capture
/// `ft8-2026-09-03-210644` the sidecar wrote `wholeSlots 1 ... whole
/// transmission inside the audio` and the line under it read `refusal no whole
/// slot fits inside the recording`. The sheet measured the 12.64 s a
/// transmission occupies; the cutter required a full 15 s slot. Both were
/// defensible and they cannot both be printed, which is why there is now one
/// function and not two arithmetics.</para>
/// <para>**12.64 IS THE RIGHT ONE.** It is what the signal occupies. A boundary
/// with 13 s of audio after it holds the whole transmission, and refusing it
/// discards a decodable slot over 2.36 s of silence that carries nothing.</para>
/// </remarks>
public sealed class TheCutterAndTheSidecarAgreeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public TheCutterAndTheSidecarAgreeTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 12_000;

    /// <summary>
    /// A 20.61 s window with a boundary 6.6 s in cuts one slot, padded 2.36 s.
    /// </summary>
    /// <remarks>
    /// The work instruction's own case. 20.61 - 6.6 = 14.01 s after the
    /// boundary: more than the 12.64 s the transmission needs and less than the
    /// 15 s a slot spans, so it cuts and pads 0.99 s. The pad is asserted from
    /// the arithmetic rather than from a figure typed here.
    /// </remarks>
    [Fact]
    public void AWindowShorterThanASlotStillCutsWhatTheTransmissionNeeds()
    {
        // **THE BOUNDARY IS 6.6 s INTO THE WINDOW**, which is the work
        // instruction's case, so 14.01 s of audio follows it. The window is
        // therefore built backwards from the boundary: it ends 14.01 s after it
        // and begins 6.6 s before it.
        //
        // The first draft of this test put the boundary 14.01 s in and left
        // 6.6 s after it, which correctly cut nothing - the geometry was
        // inverted, not the cutter.
        var boundary = new DateTime(2026, 9, 3, 21, 6, 45, DateTimeKind.Utc);
        var endedAt = boundary.AddSeconds(14.01);

        var seconds = 20.61;
        var samples = new float[(int)Math.Round(seconds * Rate)];
        var audio = new MonoAudio(Rate, samples);

        var cut = Ft8SlotCutter.Cut(audio, endedAt, new ClockOffset(0, endedAt));

        _output.WriteLine("window   : " + seconds + " s at " + Rate + " Hz");
        _output.WriteLine("slots    : " + cut.Slots.Count + "  reason: '" + cut.Reason + "'");

        foreach (var slot in cut.Slots)
        {
            _output.WriteLine("  slot   : starts " + slot.StartUtc.ToString("HH:mm:ss")
                + " at sample " + slot.FirstSample
                + ", padded " + slot.PadSeconds.ToString("0.00") + " s");
        }

        Assert.NotEmpty(cut.Slots);
        Assert.Equal("", cut.Reason);
        Assert.Single(cut.Slots);

        // **THE PAD IS 0.99 s AND NOT THE 2.36 s THE INSTRUCTION PREDICTS.**
        // 2.36 is what a boundary with exactly 12.64 s after it would need;
        // this one has 14.01 s after it, so 15 - 14.01 = 0.99 s is padded. The
        // instruction's two figures are inconsistent with each other and the
        // arithmetic is asserted rather than the typed number.
        Assert.Equal(0.99, cut.Slots[0].PadSeconds, 2);

        // Every slot cut must genuinely hold the whole transmission in real
        // audio, which is the property the pad must not be allowed to fake.
        var perSlot = (int)Math.Round(Ft8Slots.SlotSeconds * Rate);

        foreach (var slot in cut.Slots)
        {
            var real = perSlot - slot.PadSamples;

            Assert.True(
                real / (double)Rate + 1e-6 >= Ft8Slots.TransmissionSeconds,
                "a slot was cut holding only " + (real / (double)Rate).ToString("0.00")
                + " s of real audio, which is less than the transmission needs");

            Assert.Equal(perSlot, slot.Audio.Samples.Length);
        }
    }

    /// <summary>
    /// The cutter and the sidecar cannot disagree, because they ask the same
    /// function.
    /// </summary>
    /// <remarks>
    /// **SWEPT ACROSS THE BOUNDARY RATHER THAN PROBED AT ONE POINT.** The two
    /// old answers agreed everywhere except between 12.64 s and 15 s, which is
    /// exactly the band a probe at a round number misses.
    /// </remarks>
    [Theory]
    [InlineData(12.00, false)]
    [InlineData(12.63, false)]
    [InlineData(12.64, true)]
    [InlineData(13.00, true)]
    [InlineData(14.01, true)]
    [InlineData(15.00, true)]
    [InlineData(20.00, true)]
    public void OneFunctionDecidesAndItIsTheTransmission(double after, bool expected)
    {
        var fits = Ft8Slots.TransmissionFits(after);

        _output.WriteLine(after.ToString("0.00") + " s after the boundary -> "
            + (fits ? "fits" : "does not fit"));

        Assert.Equal(expected, fits);
    }

    /// <summary>A window holding no whole transmission still refuses.</summary>
    /// <remarks>
    /// **THE PAD MUST NOT TURN A REFUSAL INTO A SLOT.** Padding exists so a
    /// boundary with a whole transmission after it is not discarded over
    /// trailing silence; it is not licence to manufacture a slot out of audio
    /// that never held one.
    /// </remarks>
    [Fact]
    public void TooLittleAudioIsStillRefused()
    {
        var endedAt = new DateTime(2026, 9, 3, 21, 6, 45, DateTimeKind.Utc)
            .AddSeconds(6.0);

        var samples = new float[(int)Math.Round(10.0 * Rate)];
        var cut = Ft8SlotCutter.Cut(
            new MonoAudio(Rate, samples),
            endedAt,
            new ClockOffset(0, endedAt));

        _output.WriteLine("10 s window -> " + cut.Slots.Count
            + " slot(s), reason '" + cut.Reason + "'");

        Assert.Empty(cut.Slots);
        Assert.NotEqual("", cut.Reason);
    }
}
