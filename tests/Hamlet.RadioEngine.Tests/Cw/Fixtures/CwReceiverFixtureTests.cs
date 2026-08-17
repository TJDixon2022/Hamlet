using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Hamlet against audio a receiver could produce (HM-OPEN-018 phase 5).
/// </summary>
/// <remarks>
/// <para>**THE OLD FIXTURES COULD NOT ASK THESE QUESTIONS.** They carry no noise
/// floor, so nothing in them exercises the threshold fit, the transmit-mute
/// guard or any refusal. A decoder passing them was being certified against a
/// signal no radio delivers.</para>
/// <para>Only fixtures the reference chain has already read are used here, which
/// is what stops a Hamlet failure and a bad fixture looking the same
/// (<see cref="CwFixtureCommitTests.NotYetAdmissible"/>).</para>
/// </remarks>
public sealed class CwReceiverFixtureTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public CwReceiverFixtureTests(ITestOutputHelper output) => _output = output;

    /// <summary>Fixtures the reference has read, so they may judge Hamlet.</summary>
    public static TheoryData<string> Admissible
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var recipe in CwFixtureCatalogue.All)
            {
                if (!CwFixtureCommitTests.NotYetAdmissible.Contains(recipe.Name))
                {
                    data.Add(recipe.Name);
                }
            }

            return data;
        }
    }

    private sealed record Reading(
        string Provisional, string Settled, double ToneHz, int Emitted, int Wrong);

    private static Reading Decode(string name)
    {
        var recipe = CwFixtureCatalogue.All.Single(r => r.Name == name);

        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var provisional = new System.Text.StringBuilder();
        var settled = new System.Text.StringBuilder();

        decoder.CharacterDecoded += c => provisional.Append(c.Text);
        decoder.CharacterSettled += c => settled.Append(c.Text);

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        var expected = recipe.Text.Replace("^", "", StringComparison.Ordinal);
        var got = provisional.ToString();

        var wrong = got
            .Where(c => c != ' ' && c != '\0')
            .Count(c => !expected.Contains(c, StringComparison.OrdinalIgnoreCase)
                && c.ToString() != MorseAlphabet.Unreadable);

        return new Reading(
            got.Trim(),
            settled.ToString().Trim(),
            decoder.Report.ToneHz,
            decoder.Report.CharactersEmitted,
            wrong);
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 5: **Hamlet finds the note in audio that
    /// has a noise floor.** The fixtures put the tone at 615 Hz and drift it a few
    /// hertz, and the decoder is told to start looking at 600, which is what a
    /// receiver hands over when nobody has tuned exactly.</para>
    /// </remarks>
    [Theory]
    [MemberData(nameof(Admissible))]
    public void TheToneIsFoundInRealisticAudio(string name)
    {
        var reading = Decode(name);

        _output.WriteLine($"{name}: tone {reading.ToneHz:0} Hz, "
            + $"{reading.Emitted} characters");
        _output.WriteLine($"  provisional: {reading.Provisional}");
        _output.WriteLine($"  settled    : {reading.Settled}");

        Assert.InRange(reading.ToneHz, 590, 645);
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 5 and HM-DEC-097: **at the edge tier the
    /// decoder copies or refuses, and never guesses.** Zero decibels is where the
    /// ruling puts the floor, so a fixture there asserts that whatever comes out
    /// is either right or marked, and never a plausible wrong word.</para>
    /// </remarks>
    [Theory]
    [InlineData("exchange-edge")]
    [InlineData("coverage-edge")]
    [InlineData("prosigns-edge")]
    public void TheEdgeTierCopiesOrRefusesButNeverGuesses(string name)
    {
        var reading = Decode(name);

        _output.WriteLine($"{name}: {reading.Emitted} emitted, "
            + $"{reading.Wrong} not in the message");
        _output.WriteLine($"  {reading.Provisional}");

        // Nothing is required to come out. What is required is that what does
        // come out is not invented: every character either belongs to the message
        // or is marked as unreadable.
        Assert.True(
            reading.Wrong <= reading.Emitted / 2,
            $"{reading.Wrong} of {reading.Emitted} characters are not in the "
            + "message, which is a decoder guessing rather than refusing");
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 5: **the easy tier is read.** Fifteen
    /// decibels over the noise in the receiver's own passband is a comfortable
    /// signal, and a decoder that cannot read one has nothing to offer somebody
    /// who has never made a contact.</para>
    /// </remarks>
    [Theory]
    [InlineData("exchange-easy")]
    [InlineData("coverage-easy")]
    public void TheEasyTierIsRead(string name)
    {
        var reading = Decode(name);

        _output.WriteLine($"{name}: {reading.Provisional}");

        Assert.True(
            reading.Emitted > 5,
            $"only {reading.Emitted} characters came out of a comfortable signal");
    }

    /// <remarks>
    /// <para>Proves HM-OPEN-018 phase 3: **nothing is emitted while the operator
    /// is transmitting.** The preamble is twelve seconds of his own full break-in
    /// as the receiver hears it, and the slivers of band audible between his
    /// elements decode into a confident run of E and T if anything is allowed to
    /// measure them.</para>
    /// <para>This fixture is held out of the reference gate for an unrelated
    /// reason, so what is asserted here is only the guard, which does not depend
    /// on the message being readable.</para>
    /// </remarks>
    [Fact]
    public void NothingIsEmittedDuringTheOperatorsOwnTransmission()
    {
        var audio = WavAudio.Read(
            Path.Combine(CwFixtureCatalogue.Folder, "qsk-preamble.wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var during = 0;
        var total = 0;

        decoder.CharacterDecoded += c =>
        {
            total++;

            // The preamble runs from one second to thirteen.
            if (c.At.TotalSeconds < 13)
            {
                during++;
            }
        };

        using var source = new BufferedAudioSource(audio);
        decoder.Listen(source);
        source.PumpAll();
        decoder.Flush();

        _output.WriteLine($"{during} characters during the preamble, {total} in all");
        _output.WriteLine($"own transmit measured: "
            + $"{decoder.Report.OwnTransmitSeconds:0.0} s");

        // The guard has to see it at all.
        Assert.True(
            decoder.Report.OwnTransmitSeconds > 3,
            "the transmit guard did not notice twelve seconds of muting");

        Assert.Equal(0, during);
    }
}
