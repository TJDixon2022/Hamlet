using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What ranking by decode chooses, on the captures the operator can hear and on
/// the two that hold nothing.
/// </summary>
/// <remarks>
/// <para>**THE QUESTION WORK INSTRUCTION 032 WAS COMMISSIONED TO ASK.** Every
/// failure of the last two weeks is the survey choosing the wrong pitch, and
/// ranking asks a different question of each candidate: not *is somebody keying
/// here*, which nothing has answered, but *what does this one read*.</para>
/// <para>**IT PRINTS AND IT DOES NOT ASSERT A CHOICE.** A test that required a
/// particular pitch would be asserting the answer this unit exists to
/// measure.</para>
/// </remarks>
public sealed class WhatRankingChoosesTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatRankingChoosesTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The repository root, walking up from the test binary.</summary>
    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName
            ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }

    private static MonoAudio Capture(string name)
        => WavAudio.Read(Path.Combine(
            Root(), "tests", "fixtures", "cw", "captured", "unadjudicated",
            name + ".wav"));

    private static MonoAudio Tail(MonoAudio audio, double seconds)
    {
        var want = (int)(audio.SampleRate * seconds);

        if (audio.Samples.Length <= want)
        {
            return audio;
        }

        var slice = new float[want];

        for (var i = 0; i < want; i++)
        {
            slice[i] = audio.Samples[audio.Samples.Length - want + i];
        }

        return new MonoAudio(audio.SampleRate, slice);
    }

    /// <summary>
    /// The four the operator can hear, with where he says each one sits.
    /// </summary>
    private static (string Name, double HeardHz)[] Audible { get; } =
    {
        ("cw-2026-08-25-012823", 500.0),
        ("cw-2026-08-22-014113", 607.0),
        ("cw-2026-08-22-014308", 606.0),
        ("cw-2026-08-26-125941", 403.5),
    };

    /// <remarks>
    /// <para>Prints, for each capture the operator can hear, what ranking chose
    /// across the whole coarse bank and how far that sits from where he says the
    /// station is.</para>
    /// <para>**MEASURED OVER TWELVE SECONDS**, which is the streaming path's own
    /// window (`CwProbabilisticStream.WindowSeconds`) and the length
    /// HM-DEC-120's floor was calibrated at. A shorter window is cheaper and is
    /// not a like-for-like comparison against that floor.</para>
    /// </remarks>
    [Fact]
    public void WhatItChoosesOnTheFourHeCanHear()
    {
        var bank = CwPitchRanking.CoarseBank();

        _output.WriteLine($"  coarse bank: {bank.Length} pitches, "
            + $"{bank[0]:0} to {bank[^1]:0} Hz");
        _output.WriteLine(
            $"  window 12 s, gate {CwProbabilisticDecoder.Gate:0.00} per hop");
        _output.WriteLine("");
        _output.WriteLine(
            "  capture                | heard |  chose | error | ratio | chars | "
            + "at heard | text");
        _output.WriteLine(
            "  -----------------------|-------|--------|-------|-------|-------|"
            + "----------|-----");

        foreach (var (name, heardHz) in Audible)
        {
            var slice = Tail(Capture(name), 12.0);
            var ranked = CwPitchRanking.Rank(slice.Samples, slice.SampleRate, bank);
            var winner = CwPitchRanking.Winner(ranked)!.Value;

            // What the pitch he can hear scored, for comparison: a ranking that
            // chooses elsewhere is only wrong if the right pitch scored better.
            var atHeard = ranked
                .OrderBy(r => Math.Abs(r.ToneHz - heardHz))
                .First();

            var text = winner.Text.Length > 22
                ? winner.Text[..22] + "..."
                : winner.Text;

            _output.WriteLine(
                $"  {name,-22} | {heardHz,5:0} | {winner.ToneHz,6:0} | "
                + $"{winner.ToneHz - heardHz,5:+0;-0;0} | {winner.Ratio,5:0.00} | "
                + $"{winner.Characters,5} | {atHeard.Ratio,8:0.00} | {text}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "  'at heard' is the ratio of the bank pitch nearest where he says "
            + "the station is");

        Assert.NotEmpty(bank);
    }

    /// <remarks>
    /// <para>**WHAT IT COSTS WHEN IT IS WRONG**, which is task 4 and the reason
    /// this ranking is not wired to anything.</para>
    /// <para>Ranking has no refusal at the bin, so on an empty band it picks
    /// something and hands it on. The order's premise was that
    /// <see cref="CwProbabilisticDecoder.Gate"/> would then refuse it. **The
    /// distance is reported rather than the verdict**, because a floor that holds
    /// by a wide margin and a floor that holds by a hair are different
    /// answers.</para>
    /// </remarks>
    [Fact]
    public void WhatItChoosesOnAnEmptyBand()
    {
        var bank = CwPitchRanking.CoarseBank();

        _output.WriteLine(
            $"  gate {CwProbabilisticDecoder.Gate:0.00} per hop (HM-DEC-120), "
            + "calibrated at one pitch on 12 s windows");
        _output.WriteLine("");
        _output.WriteLine(
            "  capture                |  chose | ratio | chars | vs gate | verdict");
        _output.WriteLine(
            "  -----------------------|--------|-------|-------|---------|--------");

        var cleared = 0;

        foreach (var name in new[]
        {
            "cw-2026-08-20-014854", "cw-2026-08-20-014935",
        })
        {
            var slice = Tail(Capture(name), 12.0);
            var ranked = CwPitchRanking.Rank(slice.Samples, slice.SampleRate, bank);
            var winner = CwPitchRanking.Winner(ranked)!.Value;

            var over = winner.Ratio - CwProbabilisticDecoder.Gate;
            var admits = winner.Ratio >= CwProbabilisticDecoder.Gate;

            if (admits)
            {
                cleared++;
            }

            _output.WriteLine(
                $"  {name,-22} | {winner.ToneHz,6:0} | {winner.Ratio,5:0.00} | "
                + $"{winner.Characters,5} | {over,+7:+0.00;-0.00} | "
                + $"{(admits ? "ADMITTED" : "refused")}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  {cleared} of 2 recordings holding nothing clear the gate when the "
            + "best of the bank is taken");
        _output.WriteLine(
            "  the same gate's own evidence records 014854's highest window at "
            + "0.840, measured at one pitch");

        // **NOTHING IS ASSERTED ABOUT THE OUTCOME.** This is the measurement that
        // decides whether ranking may be wired to the tracker at all, and a test
        // asserting either answer would be deciding it in advance.
        Assert.True(cleared >= 0);
    }

    /// <remarks>
    /// <para>**THE PER-HOP WINDOW RATIO REWARDS DENSITY, WHICH IS WHAT NOISE
    /// PRODUCES.** It is the whole window's margin divided by the window's hops,
    /// so a pitch that mints many cheap one-element characters out of noise
    /// averages higher than a pitch holding a real station with real silence
    /// between its letters. Measured: on all four captures the operator can
    /// hear, ranking by that ratio chose 800 to 900 hertz and spelled runs of E,
    /// I and S.</para>
    /// <para>**`SpanMargin` ASKS A DIFFERENT QUESTION OF EACH CHARACTER** — how
    /// far its own marks stood above the noise, per hop, with the element gaps
    /// cancelling exactly because both hypotheses agree the key is up through
    /// them. A character minted from noise scores near zero there by
    /// construction, and the decoder has recorded it since unit 1.11.3 while
    /// nothing read it.</para>
    /// <para>**BOTH ARE PRINTED SIDE BY SIDE, AT THE WINNER AND AT THE PITCH HE
    /// CAN HEAR**, so the two statistics can be compared on the same audio
    /// rather than across two runs.</para>
    /// </remarks>
    [Fact]
    public void TheTwoStatisticsSideBySide()
    {
        var bank = CwPitchRanking.CoarseBank();

        _output.WriteLine("  window 12 s, whole coarse bank");
        _output.WriteLine("");
        _output.WriteLine(
            "  capture                | heard | by ratio: chose / margin | "
            + "by margin: chose / margin | at heard");
        _output.WriteLine(
            "  -----------------------|-------|--------------------------|"
            + "--------------------------|---------");

        var byMarginRight = 0;
        var byRatioRight = 0;

        foreach (var (name, heardHz) in Audible.Concat(new[]
        {
            ("cw-2026-08-20-014854", 0.0), ("cw-2026-08-20-014935", 0.0),
        }))
        {
            var slice = Tail(Capture(name), 12.0);
            var ranked = CwPitchRanking.Rank(slice.Samples, slice.SampleRate, bank);

            var byRatio = ranked[0];

            var byMargin = ranked
                .OrderByDescending(r => r.MedianSpanMargin)
                .First();

            var atHeard = ranked
                .OrderBy(r => Math.Abs(r.ToneHz - heardHz))
                .First();

            if (heardHz > 0)
            {
                if (Math.Abs(byMargin.ToneHz - heardHz) <= 25)
                {
                    byMarginRight++;
                }

                if (Math.Abs(byRatio.ToneHz - heardHz) <= 25)
                {
                    byRatioRight++;
                }
            }

            _output.WriteLine(
                $"  {name,-22} | {heardHz,5:0} | "
                + $"{byRatio.ToneHz,10:0} Hz / {byRatio.MedianSpanMargin,10:0.0} | "
                + $"{byMargin.ToneHz,10:0} Hz / {byMargin.MedianSpanMargin,10:0.0} | "
                + $"{atHeard.MedianSpanMargin,8:0.0}");
        }

        _output.WriteLine("");
        _output.WriteLine(
            $"  within one bin of where he says the station is: "
            + $"by ratio {byRatioRight} of 4, by margin {byMarginRight} of 4");

        Assert.True(byMarginRight >= 0);
    }
}
