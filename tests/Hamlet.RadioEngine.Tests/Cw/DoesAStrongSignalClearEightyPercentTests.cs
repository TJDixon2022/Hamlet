using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// How much of a strong signal the production path reads, first time, with the
/// guard in place.
/// </summary>
/// <remarks>
/// <para>**THE TARGET IS EIGHTY PERCENT AND THIS IS WHERE IT IS STATED AS A
/// NUMBER.** Not bypassed, not whole-file, not forced to a speed: the path the
/// operator's audio actually takes.</para>
/// <para>**IT MEASURES AND DOES NOT ASSERT THE TARGET.** A test that failed the
/// build at seventy-nine percent would be a ratchet on a number nobody has
/// ruled, and this project's own history is of ratchets recording that
/// something is still wrong without ever requiring it to stop being wrong
/// (HM-DEC-114 is the ruling that ended that on the easy tiers). What is
/// asserted is the one property that is never traded: audio holding no station
/// emits nothing.</para>
/// </remarks>
public sealed class DoesAStrongSignalClearEightyPercentTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the percentages are printed.</param>
    public DoesAStrongSignalClearEightyPercentTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What KD0UN was sending, from the capture's own adjudication.</summary>
    private const string Sent = "CQ CQ CQ DE KD0UN KD0UN K";

    private static MonoAudio Read(string name) => WavAudio.Read(
        Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

    private static MonoAudio Slice(MonoAudio a, double from, double to)
    {
        var f = Math.Clamp((int)(from * a.SampleRate), 0, a.Samples.Length);
        var t = Math.Clamp((int)(to * a.SampleRate), f, a.Samples.Length);

        return new MonoAudio(a.SampleRate, a.Samples[f..t]);
    }

    /// <summary>Decode the way the operator's audio is decoded.</summary>
    private static (string Text, IReadOnlyList<CwCharacter> Characters, int Cleared, int Windows)
        Production(MonoAudio audio, double startAt)
    {
        var decoder = new CwDecoder(audio.SampleRate, startAt);
        var settled = new List<CwCharacter>();
        var hop = decoder.Tracker.HopSamples;
        var ratios = new List<double>();
        var last = double.NaN;

        decoder.CharacterSettled += settled.Add;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            var r = decoder.Stream.Last.LikelihoodRatio;

            if (!r.Equals(last))
            {
                last = r;
                ratios.Add(r);
            }
        }

        decoder.Flush();

        var scored = ratios.Where(r => r != 0).ToList();

        return (
            string.Concat(settled.Select(c => c.Text)),
            settled,
            scored.Count(r => r >= CwProbabilisticDecoder.Gate),
            scored.Count);
    }

    private double Score(
        string label,
        IReadOnlyList<CwCharacter> characters,
        string sent,
        string text,
        int cleared,
        int windows)
    {
        var matches = CwAlignment.Align(characters, sent);
        var expected = CwAlignment.SymbolCount(sent);

        var correct = matches.Count(
            m => m.Kind == CwMatchKind.Correct && !m.Decoded.IsWordGap);

        var wrong = matches.Count(
            m => m.Kind == CwMatchKind.Wrong && !m.Decoded.IsWordGap);

        var invented = matches.Count(
            m => m.Kind == CwMatchKind.Invented && !m.Decoded.IsWordGap);

        var share = expected == 0 ? 0 : 100.0 * correct / expected;

        _output.WriteLine(
            $"{label}: {share:0.0} % of the sent text read correctly "
            + $"({correct} of {expected}), {wrong} wrong, {invented} never sent, "
            + $"{cleared} of {windows} windows cleared the guard");
        _output.WriteLine($"    sent: {sent}");
        _output.WriteLine($"    read: {text}");

        return share;
    }

    /// <remarks>
    /// **THE NUMBER THE DAY WAS SPENT ON.** `cw-2026-08-24-012403`, whole, through
    /// the production path with the guard in place, against what KD0UN was
    /// sending.
    /// </remarks>
    [Fact]
    public void KD0UNWholeRecordingThroughTheProductionPath()
    {
        var audio = Read("unadjudicated/cw-2026-08-24-012403");
        var (text, characters, cleared, windows) = Production(audio, 439.81);

        Score("012403, whole", characters, Sent, text, cleared, windows);
    }

    /// <remarks>
    /// The strong stretch alone, 20 to 30 seconds, where the station stands
    /// twenty decibels above everything more than forty hertz away. The sent
    /// text over that stretch is the callsign and the closing prosign rather
    /// than the whole call.
    /// </remarks>
    [Fact]
    public void KD0UNStrongStretchThroughTheProductionPath()
    {
        var audio = Slice(Read("unadjudicated/cw-2026-08-24-012403"), 20, 30);
        var (text, characters, cleared, windows) = Production(audio, 439.81);

        Score("012403, 20-30 s", characters, "DE KD0UN KD0UN K", text, cleared, windows);
    }

    /// <remarks>
    /// The cleanest recording in the tree, so one figure is not the whole basis.
    /// The sent text is the ARRL bulletin fragment this recording holds, taken
    /// from what two independent decoders agree on rather than adjudicated
    /// (§12.5): it is a control on the percentage, not an answer key.
    /// </remarks>
    [Fact]
    public void TheBulletinThroughTheProductionPath()
    {
        var audio = Read("cw-2026-08-18-004507");
        var (text, characters, cleared, windows) = Production(audio, 501);

        Score(
            "004507",
            characters,
            "AT ARRL DOT NET <BT> EACH STATION HANDLING THIS MESSAGE",
            text,
            cleared,
            windows);
    }

    /// <remarks>
    /// **THE PROPERTY THAT IS NEVER TRADED** (HM-DEC-120), asserted through the
    /// same path at the same guard. A guard re-expressed to admit a weak station
    /// has to be shown still to refuse an empty band, or the number above was
    /// bought with the thing this application exists to protect.
    /// </remarks>
    /// <param name="name">The recording.</param>
    /// <param name="toneHz">Where to look.</param>
    [Theory]
    [InlineData("unadjudicated/cw-2026-08-20-014854", 600)]
    [InlineData("unadjudicated/cw-2026-08-20-014935", 825)]
    public void AndAudioHoldingNoStationStillEmitsNothing(string name, double toneHz)
    {
        var (text, characters, cleared, windows) = Production(Read(name), toneHz);

        _output.WriteLine(
            $"{name}: {characters.Count} characters, "
            + $"{cleared} of {windows} windows cleared the guard, '{text}'");

        Assert.Empty(characters);
    }
}
