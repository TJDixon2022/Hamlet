using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The speed says nothing while its clock is being re-acquired (HM-DEC-107).
/// </summary>
/// <remarks>
/// <para>**A SPEED IS A FACT ABOUT SOMEBODY'S KEYING, AND SIXTEEN WORDS A MINUTE
/// IS A FACT ABOUT NEITHER STATION.** Across a change of station the rolling
/// estimate necessarily passes through speeds belonging to no one, because for a
/// while its window holds marks from both.</para>
/// <para>Marking the number unsettled was rejected: an unsettled forty-four is
/// still forty-four on the screen a beginner uses to decide whether he could
/// have copied the exchange, and he concludes it was beyond him when it was not.
/// Showing the last proved speed was rejected as asserting a stale fact as a
/// current one.</para>
/// </remarks>
public sealed class CwSpeedSilenceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the speeds are printed.</param>
    public CwSpeedSilenceTests(ITestOutputHelper output) => _output = output;

    private static List<int> SpeedsNamedAcross(string fixture)
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, fixture + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var named = new List<int>();

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);

        var chunk = audio.SampleRate / 8;

        for (var at = 0; at < audio.Samples.Length; at += chunk)
        {
            var take = Math.Min(chunk, audio.Samples.Length - at);

            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan(at, take)));

            if (decoder.WordsPerMinute is { } wpm)
            {
                named.Add(wpm);
            }
        }

        decoder.Flush();

        return named;
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 1 on the recording that produced the
    /// finding: a caller at about eleven words a minute handing over to an
    /// answerer at twenty-two. **No reading between the two may reach a
    /// surface**, because a number between them describes nobody on the
    /// band.</para>
    /// </remarks>
    [Fact]
    public void NoSpeedBetweenTwoStationsIsEverNamed()
    {
        var named = SpeedsNamedAcross(CwFixtureCatalogue.TwoStationName);

        _output.WriteLine(named.Count == 0
            ? "no speed was named at all"
            : $"named: {string.Join(", ", named.Distinct().OrderBy(w => w))}");

        // Eleven and twenty-two are the two stations. Anything strictly between
        // them belongs to neither, and the average is the worst of those because
        // it looks the most reasonable.
        var between = named.Where(w => w is > 13 and < 20).Distinct().ToList();

        Assert.True(
            between.Count == 0,
            $"named {string.Join(", ", between)} words a minute, which is between "
            + "the two stations and describes neither");
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 1: **nothing faster than either station is named
    /// either.** The excursions went as far as forty-four on a recording whose
    /// quicker station was sending at twenty-two, and a number nobody produced is
    /// not made acceptable by being briefly displayed.
    /// </remarks>
    [Fact]
    public void NoSpeedFasterThanEitherStationIsNamed()
    {
        var named = SpeedsNamedAcross(CwFixtureCatalogue.TwoStationName);
        var beyond = named.Where(w => w > 26).Distinct().ToList();

        _output.WriteLine(beyond.Count == 0
            ? "nothing faster than either station was named"
            : $"named: {string.Join(", ", beyond)}");

        Assert.Empty(beyond);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-107 phase 1 does not silence the speed permanently.
    /// **A single station's speed still gets named**, or the cure would be worse
    /// than the fault: a field that never carries a number teaches the operator
    /// to stop looking at it.</para>
    /// </remarks>
    [Fact]
    public void OneStationsSpeedIsStillNamed()
    {
        var named = SpeedsNamedAcross("exchange-easy");

        _output.WriteLine(named.Count == 0
            ? "no speed was named"
            : $"named: {string.Join(", ", named.Distinct().OrderBy(w => w))}");

        Assert.NotEmpty(named);

        // And it is the speed the fixture was generated at, near enough.
        Assert.Contains(named, w => w is >= 10 and <= 15);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 1: the re-acquiring state is readable, so a
    /// surface can say what it is doing rather than merely showing an empty box
    /// (§0.0.1).
    /// </remarks>
    [Fact]
    public void TheReacquiringStateIsReadable()
    {
        var decoder = new CwDecoder(8_000, 600);

        // Nothing has been heard, so nothing has been proved.
        Assert.True(decoder.SpeedIsReacquiring);
        Assert.Null(decoder.WordsPerMinute);
    }
}
