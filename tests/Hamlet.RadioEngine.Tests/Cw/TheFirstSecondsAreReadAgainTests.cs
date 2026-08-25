using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The audio the decoder is still holding is read again once it knows the pitch
/// it should have been reading it at.
/// </summary>
/// <remarks>
/// <para>**THE FIRST SECONDS OF EVERY STATION ARE DEMODULATED AT A GUESS.** The
/// stream mixes each sample as it arrives, at whatever the tracker believed
/// then, and until the survey admits a candidate the tracker answers with the
/// middle of the bank it is pointed at. Measured across this repository's
/// thirty-six captures, the first measured pitch lands two to seven seconds in
/// on half of them, and the window is still holding every sample since the start
/// when it does.</para>
/// <para>**WHAT IT IS WORTH.** The adjudicated corpus moves from 158 characters
/// of 384 to 167, and `cw-2026-08-18-003758` gives back `AA4MP/4 QNIK` whole —
/// twelve of twelve, the first time this fixture has ever produced HM-DEC-126's
/// callsign complete — while the ARRL bulletin goes from 22 to 28.</para>
/// <para>The properties below are the ones that make it safe rather than the
/// ones that make it useful, and each is here because it could have gone the
/// other way.</para>
/// </remarks>
public sealed class TheFirstSecondsAreReadAgainTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the counts are printed.</param>
    public TheFirstSecondsAreReadAgainTests(ITestOutputHelper output)
        => _output = output;

    private static CwDecoder Run(string name, int chunk = 240)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(
            audio.SampleRate, TheAdjudicatedReadingsKeepReadingTests.RadioPitchHz);

        for (var at = 0L; at < audio.Samples.Length; at += chunk)
        {
            var take = (int)Math.Min(chunk, audio.Samples.Length - at);

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, take)));
        }

        decoder.Flush();

        return decoder;
    }

    /// <remarks>
    /// Proves HM-DEC-120 is not traded for the re-read. No pitch is ever measured
    /// on a recording holding nothing, so there is nothing to re-read at and the
    /// replay must never fire — and the silence must be exactly the silence it
    /// was.
    /// </remarks>
    /// <param name="name">A recording that holds nothing.</param>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854")]
    [InlineData("unadjudicated/cw-2026-08-20-014935")]
    [InlineData("unadjudicated/cw-2026-08-22-014113")]
    [InlineData("unadjudicated/cw-2026-08-22-014308")]
    public void AnEmptyBandIsNeverReadAgain(string name)
    {
        var decoder = Run(name);

        _output.WriteLine(
            $"{name}: {decoder.Stream.ReReads} re-reads, "
            + $"{decoder.Report.CharactersEmitted} characters");

        Assert.True(
            decoder.Stream.ReReads == 0,
            $"{name} holds no station and the decoder re-read it "
            + $"{decoder.Stream.ReReads} times");

        Assert.True(
            decoder.Report.CharactersEmitted == 0,
            $"{name} holds no station and produced "
            + $"{decoder.Report.CharactersEmitted} characters");
    }

    /// <remarks>
    /// <para>**THE REPLAY IS A FUNCTION OF HOPS AND NOT OF ARRIVING CHUNKS**, so
    /// it fires the same number of times whatever size the sound card hands over.
    /// A re-read that fired at different moments for different buffer sizes would
    /// put back the fault `OneDecoderNotTwoTests` closed, and it would do it
    /// somewhere much harder to see.</para>
    /// <para>The audio it asks the tap for is addressed by the stream's own place
    /// on the clock rather than as "the last N samples", because the tap takes a
    /// whole chunk at once and the decoder walks it a hop at a time — so the last
    /// N would hand the replay hops from the future.</para>
    /// </remarks>
    /// <param name="name">The recording.</param>
    [Theory]
    [MemberData(nameof(OneDecoderNotTwoTests.Captures),
        MemberType = typeof(OneDecoderNotTwoTests))]
    public void TheReplayFiresTheSameWhateverTheBufferSize(string name)
    {
        var first = Run(name, 240).Stream.ReReads;

        foreach (var chunk in new[] { 480, 960, 1920, 4800 })
        {
            var again = Run(name, chunk).Stream.ReReads;

            Assert.True(
                again == first,
                $"{name} re-reads {first} times in chunks of 240 and {again} "
                + $"times in chunks of {chunk}");
        }

        _output.WriteLine($"{name}: {first} re-reads at every buffer size");
    }

    /// <remarks>
    /// **NOTHING ALREADY SAID IS SAID AGAIN.** The replay re-derives characters
    /// the stream has already announced; the settled mark drops them on the same
    /// test that stops a window read twice a second from repeating itself. This
    /// asks the question directly on a capture that re-reads: every settled
    /// character's moment is strictly later than the one before it, which cannot
    /// be true if the replay re-announced anything.
    /// </remarks>
    [Fact]
    public void NothingIsSaidTwice()
    {
        var audio = WavAudio.Read(Path.Combine(
            CapturedSignalTests.Folder,
            "unadjudicated/cw-2026-08-18-003758.wav"));

        var decoder = new CwDecoder(
            audio.SampleRate, TheAdjudicatedReadingsKeepReadingTests.RadioPitchHz);

        var moments = new List<TimeSpan>();

        decoder.CharacterSettled += c => moments.Add(c.At);

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        _output.WriteLine(
            $"{decoder.Stream.ReReads} re-reads, {moments.Count} characters settled");

        Assert.True(decoder.Stream.ReReads > 0, "this capture did not re-read");

        for (var i = 1; i < moments.Count; i++)
        {
            Assert.True(
                moments[i] > moments[i - 1],
                $"character {i} settled at {moments[i]}, which is not after "
                + $"character {i - 1} at {moments[i - 1]} — the replay said "
                + "something twice");
        }
    }

    /// <remarks>
    /// **THE CALLSIGN THE RE-READ WAS SHIPPED FOR.** `cw-2026-08-18-003758`
    /// carries HM-DEC-126's `AA4MP/4 QNIK`, and until the re-read the fixture
    /// gave back nine of its twelve characters — the `AA4` was demodulated at
    /// the bank centre and lost. This is the anchor that replaced its count
    /// floor, asserted here as well because it is the reason the floor retired.
    /// </remarks>
    [Fact]
    public void TheCallsignTheReReadRecovers()
    {
        var text = TheAdjudicatedReadingsKeepReadingTests
            .Settled("unadjudicated/cw-2026-08-18-003758");

        _output.WriteLine(text);

        Assert.Contains("AA4MP/4 QNIK", text, StringComparison.Ordinal);
    }
}
