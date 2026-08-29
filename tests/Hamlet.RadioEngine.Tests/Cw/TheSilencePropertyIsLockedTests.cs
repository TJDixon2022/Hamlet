using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The captures that hold no station emit no letters, and nothing in unit 046
/// may change that.
/// </summary>
/// <remarks>
/// <para>**THIS IS A LOCK AND NOT A MEASUREMENT** (work instruction 046 task 1,
/// Tim's ruling). In his words: *I'm no longer seeing random characters when
/// there's just noise, so that seems to be solved. Don't break it.* The property
/// is not tradeable at any price, so it is asserted before anything moves and
/// **this file may not be modified by a later task in that unit.** A task that
/// turns it red is reverted rather than accommodated.</para>
/// <para>**THE FIVE COVERED HERE ARE THE ONES THAT CURRENTLY EMIT NOTHING**,
/// measured across the whole corpus on 2026-08-29 rather than assumed.
/// `cw-2026-08-20-014854` and `-014935` are the two the suite has always called
/// HOLDS NOTHING; `cw-2026-08-22-014113`, `-014308` and `cw-2026-08-26-125941`
/// are quiet by measurement.</para>
/// <para>**IT LOCKS ZERO LETTERS AND NOT ZERO CHARACTERS.** A block is Hamlet
/// saying it heard something and will not name it, which is the honest output and
/// not a violation (HM-DEC-048). What must never appear is a letter.</para>
/// </remarks>
public sealed class TheSilencePropertyIsLockedTests
{
    private readonly ITestOutputHelper _output;

    public TheSilencePropertyIsLockedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A capture holding no station puts no letter on the screen.</summary>
    [Theory]
    [InlineData("cw-2026-08-20-014854")]
    [InlineData("cw-2026-08-20-014935")]
    [InlineData("cw-2026-08-22-014113")]
    [InlineData("cw-2026-08-22-014308")]
    [InlineData("cw-2026-08-26-125941")]
    public void ACaptureHoldingNoStationEmitsNoLetters(string name)
    {
        var audio = Read(name);
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var letters = new System.Text.StringBuilder();

        decoder.CharacterSettled += c =>
        {
            if (c.Text != MorseAlphabet.WordGap
                && c.Text != MorseAlphabet.Unreadable)
            {
                letters.Append(c.Text);
            }
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        _output.WriteLine($"{name}: letters \"{letters}\"");

        Assert.True(
            letters.Length == 0,
            $"{name} holds no station and put {letters.Length} letters on the "
            + $"screen: \"{letters}\"");
    }

    /// <summary>Digital silence is not read as a band.</summary>
    /// <remarks>
    /// An all-zero buffer is an absence of measurement rather than a quiet band
    /// (HM-DEC-120), and it is the cheapest case to get wrong.
    /// </remarks>
    [Fact]
    public void AnAllZeroBufferEmitsNothing()
    {
        var decoder = new CwDecoder(48_000, 600);
        var emitted = 0;

        decoder.CharacterSettled += _ => emitted++;

        var silence = new float[48_000 * 20];
        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= silence.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, 48_000, silence.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        Assert.Equal(0, emitted);
    }

    private static MonoAudio Read(string name)
    {
        var direct = Path.Combine(CapturedSignalTests.Folder, name + ".wav");

        return WavAudio.Read(File.Exists(direct)
            ? direct
            : Path.Combine(
                CapturedSignalTests.Folder, "unadjudicated", name + ".wav"));
    }
}
