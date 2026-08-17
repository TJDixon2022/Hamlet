using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// The rebuilt fixtures describe a receiver rather than a fantasy (HM-OPEN-018).
/// </summary>
/// <remarks>
/// <para>These test the fixtures, not the decoder. That distinction is the whole
/// reason this session exists: the old fixtures were never checked against what a
/// receiver delivers, so they certified a decoder that could not read a real
/// signal and the validated reference chain scores zero on every one of
/// them.</para>
/// <para>**A FIXTURE HAS TO BE FALSIFIABLE TOO** (§12.5).</para>
/// </remarks>
public sealed class CwFixtureBuildTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public CwFixtureBuildTests(ITestOutputHelper output) => _output = output;

    /// <summary>Every recipe, for a theory.</summary>
    public static TheoryData<string> Names
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var recipe in CwFixtureCatalogue.All)
            {
                data.Add(recipe.Name);
            }

            return data;
        }
    }

    private static CwFixtureRecipe Recipe(string name)
        => CwFixtureCatalogue.All.Single(r => r.Name == name);

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 1, and it is the defect being removed.
    /// **A real receiver never hands over digital silence.** Between elements
    /// there is band noise; the old fixtures put exact zero there, which is a
    /// hundred and fifty decibels away from anything a radio does and which any
    /// transmit-mute guard correctly reads as a muted receiver.</para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(Names))]
    public void NothingInTheFixtureIsDigitalSilence(string name)
    {
        var (audio, _) = CwFixtureGenerator.Generate(Recipe(name));

        var window = audio.SampleRate / 100;
        var quietest = double.MaxValue;

        for (var start = 0; start + window < audio.Samples.Length; start += window)
        {
            double sum = 0;

            for (var i = start; i < start + window; i++)
            {
                double value = audio.Samples[i];
                sum += value * value;
            }

            quietest = Math.Min(quietest, 20 * Math.Log10(Math.Sqrt(sum / window) + 1e-12));
        }

        _output.WriteLine($"{name}: quietest hundredth of a second {quietest:0.0} dBFS");

        // Above the floor the guard treats as a file with nothing in it, and at
        // or below what a muted receiver actually delivers.
        Assert.InRange(quietest, CwTransmitGuard.SilenceBelowDbfs, -10);
    }

    /// <remarks>
    /// Proves HM-OPEN-018 phase 1: **the noise is shaped to the receiver's own
    /// filter.** Noise flat across the whole audio band is a different signal
    /// from what a five hundred hertz filter delivers, and the decoder measures
    /// its own noise floor from bins beside the tone.
    /// </remarks>
    [Fact]
    public void TheNoiseIsShapedToTheReceiversPassband()
    {
        var (audio, _) = CwFixtureGenerator.Generate(Recipe("exchange-easy"));

        var inBand = ToneLevelDb(audio, 500);
        var farOut = ToneLevelDb(audio, 2_500);

        _output.WriteLine($"in band 500 Hz {inBand:0.0} dB, out of band 2500 Hz {farOut:0.0} dB");

        // The skirt is an attenuation and not a deletion, so it is measurable and
        // well down. Twenty decibels is the assertion; the recipe asks for thirty
        // and three biquad sections deliver somewhere past that.
        Assert.True(
            inBand - farOut >= 20,
            $"the passband is only {inBand - farOut:0.0} dB above the skirt");
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 2: **the fourth message's gaps are shorter
    /// than its own dits.** That is what the station on the air was doing, it is
    /// what breaks a decoder classifying gaps as multiples of a dit, and nothing
    /// in the old fixture set contained it.</para>
    /// </remarks>
    [Fact]
    public void TheTightFistSendsGapsShorterThanItsDits()
    {
        var recipe = Recipe("tightfist-easy");

        Assert.True(
            recipe.ElementGapMilliseconds < recipe.DitMilliseconds,
            "the tight fist is not tight");

        // And the character gap is nowhere near the textbook three dits.
        Assert.True(recipe.CharacterGapMilliseconds < 2 * recipe.DitMilliseconds);

        // While an ordinary fist in the same suite is textbook, so the two shapes
        // are both represented.
        var ordinary = Recipe("exchange-easy");

        Assert.Equal(
            ordinary.DitMilliseconds, ordinary.ElementGapMilliseconds, 3);
    }

    /// <remarks>
    /// Proves HM-OPEN-018 phase 2: three tiers, and the fade sits on the one it
    /// was measured on rather than on all of them.
    /// </remarks>
    [Fact]
    public void EveryMessageHasThreeTiersAndTheFadeIsOnOne()
    {
        foreach (var slug in new[] { "exchange", "prosigns", "coverage", "tightfist" })
        {
            var tiers = CwFixtureCatalogue.All
                .Where(r => r.Name.StartsWith(slug + "-", StringComparison.Ordinal))
                .ToList();

            Assert.Equal(3, tiers.Count);
            Assert.Single(tiers, r => r.QsbHz > 0);
            Assert.Equal(
                CwFixtureCatalogue.WorkingDb,
                tiers.Single(r => r.QsbHz > 0).SignalToNoiseDb);
        }
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 3: the preamble is the operator's own
    /// transmission as the receiver hears it. **Mutes at minus ninety, never
    /// zero**, with the changeover hanging either side of each one.</para>
    /// </remarks>
    [Fact]
    public void ThePreambleIsMutedRatherThanSilent()
    {
        var (audio, sidecar) = CwFixtureGenerator.Generate(Recipe("qsk-preamble"));

        _output.WriteLine(sidecar);

        var window = audio.SampleRate / 100;
        var muted = 0;
        var frames = 0;

        // Only the preamble, which sits before the message starts.
        var until = (int)(13.0 * audio.SampleRate);

        for (var start = 0; start + window < Math.Min(until, audio.Samples.Length);
             start += window)
        {
            double sum = 0;

            for (var i = start; i < start + window; i++)
            {
                double value = audio.Samples[i];
                sum += value * value;
            }

            var db = 20 * Math.Log10(Math.Sqrt(sum / window) + 1e-12);
            frames++;

            if (db <= CwTransmitGuard.MuteBelowDbfs)
            {
                muted++;

                Assert.True(
                    db > CwTransmitGuard.SilenceBelowDbfs,
                    $"a mute measured {db:0.0} dBFS, which is a file with nothing "
                    + "in it rather than a muted receiver");
            }
        }

        _output.WriteLine($"{muted} of {frames} preamble frames are muted");

        Assert.True(muted > 0, "the preamble has no mutes in it at all");
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 1 and §5: **the same recipe gives the same
    /// file, byte for byte.** A fixture that quietly changed would take every
    /// assertion resting on it along, and the whole value of a generated fixture
    /// over a recorded one is that anybody can rebuild it and get this file.</para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(Names))]
    public void TheSameRecipeGivesTheSameFileEveryTime(string name)
    {
        var recipe = Recipe(name);

        var (first, firstNotes) = CwFixtureGenerator.Generate(recipe);
        var (second, secondNotes) = CwFixtureGenerator.Generate(recipe);

        Assert.Equal(first.Samples.Length, second.Samples.Length);
        Assert.Equal(firstNotes, secondNotes);

        for (var i = 0; i < first.Samples.Length; i++)
        {
            if (first.Samples[i] != second.Samples[i])
            {
                Assert.Fail($"{name} differs at sample {i}");
            }
        }
    }

    /// <remarks>
    /// Proves HM-OPEN-018 phase 2 and §2.1: **no real callsign appears in any
    /// fixture.** This repository is going out under GPL-3.0 and a synthesized
    /// fixture carrying somebody's callsign is a transmission attributed to them
    /// that they never made.
    /// </remarks>
    [Fact]
    public void NoRealCallsignAppearsAnywhere()
    {
        foreach (var recipe in CwFixtureCatalogue.All)
        {
            var text = recipe.Text;

            Assert.DoesNotContain("VA3", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("KC3", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("W1AW", text, StringComparison.OrdinalIgnoreCase);

            if (text.Contains("CALL", StringComparison.Ordinal))
            {
                Assert.Contains("N0CALL", text, StringComparison.Ordinal);
            }
        }
    }

    /// <summary>How loud one frequency is across the whole recording.</summary>
    private static double ToneLevelDb(MonoAudio audio, double hz)
    {
        double real = 0, imaginary = 0;

        for (var i = 0; i < audio.Samples.Length; i++)
        {
            var angle = 2 * Math.PI * hz * i / audio.SampleRate;
            real += audio.Samples[i] * Math.Cos(angle);
            imaginary += audio.Samples[i] * Math.Sin(angle);
        }

        var magnitude = Math.Sqrt((real * real) + (imaginary * imaginary))
            / audio.Samples.Length;

        return 20 * Math.Log10(magnitude + 1e-15);
    }
}
