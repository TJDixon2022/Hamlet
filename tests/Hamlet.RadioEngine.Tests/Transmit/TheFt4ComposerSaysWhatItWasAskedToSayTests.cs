using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **`Ft4Composer`: the one call that differs.** Work instruction 293, task 2 -
/// the words-to-audio half of step 4's criterion 3.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THESE CATCH** (`CLAUDE.md`: a unit may not add a test
/// without naming it): **a composer that returns audio for a message different from
/// the one asked for.** That is the one fault `Ft8Composer`'s round trip exists to
/// prevent, and it is the first thing a copied-and-edited file loses - the copy
/// keeps the packing that was right on the day it was taken and stops moving when
/// the original does. **So the packing is asserted to be the same code**, by
/// composing the same words through both faces and reading the same bits back out
/// of both.</para>
/// <para>**AND THE SECOND: a seam that drifts from the port.** The audio a message
/// composes to is asserted sample for sample against what `Ft4Waveform` produces
/// for that message's own symbols, so this file cannot come to differ from the
/// thing unit 289 nailed to `gen_ft8 -ft4`.</para>
/// <para>**NOTHING HERE OPENS ANYTHING.** A composer takes words and returns an
/// array of floats.</para>
/// </remarks>
public sealed class TheFt4ComposerSaysWhatItWasAskedToSayTests
{
    /// <summary>A message that carries no signal report and needs no cache.</summary>
    private const string Message = "W1ABC KC3QIS FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheFt4ComposerSaysWhatItWasAskedToSayTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **The audio is the port's own, sample for sample.**
    /// </summary>
    /// <remarks>
    /// **PROVED AGAINST THE PORT AND NOT AGAINST ITSELF.** The expected array is
    /// built here by packing the same words through the shared message layer and
    /// handing the bits to <c>Ft4SymbolEncoder</c> and <c>Ft4Waveform</c> directly.
    /// If <see cref="Ft4Composer"/> ever grows a tone, a phase or a window of its
    /// own, this fails on the first sample that differs.
    /// </remarks>
    [Fact]
    public void TheFt4ComposerIsThePortsOwnAudio()
    {
        var composed = Ft4Composer.ComposeSignal(Message, drivePeak: 1.0f);

        Assert.True(composed.Composed, composed.Explanation);

        var transmission = composed.Transmission!;

        // The same words through the shared message layer, then straight at the
        // port's own two FT4 calls.
        var bits = new byte[Ft8Payload.MessageBytes];
        var packed = Ft8StandardMessage.TryPack("W1ABC", "KC3QIS", "FN00", null, bits);

        Assert.Equal(Ft8PackResult.Ok, packed);

        var symbols = Ft4SymbolEncoder.Encode(bits);
        var expected = Ft4Waveform.Synthesize(
            symbols, transmission.SampleRate, transmission.BaseFrequencyHz);

        _output.WriteLine("message          : \"" + Message + "\"");
        _output.WriteLine("reads back as    : \"" + transmission.ReadsBackAs + "\"");
        _output.WriteLine("message type     : " + transmission.Type);
        _output.WriteLine("symbols          : " + symbols.Length
            + " (Ft4SymbolEncoder.SymbolCount = " + Ft4SymbolEncoder.SymbolCount + ")");
        _output.WriteLine("composed samples : " + transmission.Samples.Length
            + " at " + transmission.SampleRate + " Hz");
        _output.WriteLine("port samples     : " + expected.Length);

        Assert.Equal(Ft4SymbolEncoder.SymbolCount, symbols.Length);
        Assert.Equal(expected.Length, transmission.Samples.Length);
        Assert.Equal(expected, transmission.Samples);

        // AND IT IS NOT FT8'S AUDIO. 105 symbols at 0.048 s, not 79 at 0.16 s.
        Assert.NotEqual(
            Ft8Waveform.SymbolCount * Ft8Waveform.SamplesPerSymbol(transmission.SampleRate),
            transmission.Samples.Length);
    }

    /// <summary>
    /// **The message layer is the same code, not a copy of it.**
    /// </summary>
    /// <remarks>
    /// The two faces compose the same words, and what comes back out of the bits is
    /// the same string and the same <see cref="Ft8MessageType"/>. **A second packer
    /// beside the first is a second thing to drift**, and the drift would be a
    /// message that says one thing on FT8 and another on FT4.
    /// </remarks>
    [Fact]
    public void FT4CarriesTheSameSeventySevenBitsFT8Does()
    {
        var onFt8 = Ft8Composer.ComposeSignal(Message);
        var onFt4 = Ft4Composer.ComposeSignal(Message);

        Assert.True(onFt8.Composed, onFt8.Explanation);
        Assert.True(onFt4.Composed, onFt4.Explanation);

        _output.WriteLine("FT8 reads back as: \"" + onFt8.Transmission!.ReadsBackAs + "\" ("
            + onFt8.Transmission.Type + ")");
        _output.WriteLine("FT4 reads back as: \"" + onFt4.Transmission!.ReadsBackAs + "\" ("
            + onFt4.Transmission.Type + ")");
        _output.WriteLine("FT8 samples      : " + onFt8.Transmission.Samples.Length);
        _output.WriteLine("FT4 samples      : " + onFt4.Transmission.Samples.Length);

        Assert.Equal(onFt8.Transmission.Text, onFt4.Transmission.Text);
        Assert.Equal(onFt8.Transmission.ReadsBackAs, onFt4.Transmission.ReadsBackAs);
        Assert.Equal(onFt8.Transmission.Type, onFt4.Transmission.Type);
        Assert.Equal(
            onFt8.Transmission.CarriesHashedCallsign,
            onFt4.Transmission.CarriesHashedCallsign);

        // **THE SAME MESSAGE, A DIFFERENT MODULATION.** The audio is the one thing
        // that differs, and it differs in length as well as in content.
        Assert.NotEqual(onFt8.Transmission.Samples.Length, onFt4.Transmission.Samples.Length);
    }

    /// <summary>
    /// **A message that will not read back as itself is refused on FT4 too.**
    /// </summary>
    /// <remarks>
    /// The round trip is the shared half, so this is the same refusal FT8 gives -
    /// which is the point: audio for a message the operator did not ask for is
    /// impossible on both faces or on neither.
    /// </remarks>
    [Fact]
    public void AMessageThatWillNotPackIsRefusedWithASentence()
    {
        var refused = Ft4Composer.ComposeSignal("!!! ??? ###");

        _output.WriteLine("refusal          : " + refused.Refusal);
        _output.WriteLine("explanation      : " + refused.Explanation);

        Assert.False(refused.Composed);
        Assert.Equal(Ft8ComposeRefusal.WillNotPack, refused.Refusal);
        Assert.NotEmpty(refused.Explanation);
        Assert.Null(refused.Transmission);
    }

    /// <summary>
    /// **All five refusals work on FT4 and none of them is an exception.**
    /// </summary>
    /// <remarks>
    /// <para>`NothingToSay`, `WillNotPack`, `SampleRateRefused`,
    /// `BaseFrequencyRefused` and `DriveLevelRefused`. **Every one comes back as a
    /// sentence the operator reads**, which is the property the whole compose path
    /// rests on: a refusal that threw would reach him as a crash in the middle of
    /// answering a CQ.</para>
    /// <para>**AND THE NYQUIST ARITHMETIC IS FT4'S OWN.** Four tones at 20.8333 Hz
    /// put the top tone 62.5 Hz above tone 0; FT8's eight at 6.25 put it 43.75 Hz
    /// above. The base frequency below is chosen so that FT4 refuses it and FT8
    /// does not, which is the only way to tell the two arithmetics apart.</para>
    /// </remarks>
    [Fact]
    public void EveryOneOfTheFiveRefusalsIsASentenceOnFt4()
    {
        var nothing = Ft4Composer.ComposeSignal("   ");
        var willNot = Ft4Composer.ComposeSignal("!!! ??? ###");
        var rate = Ft4Composer.ComposeSignal(Message, sampleRate: 11_025);
        var drive = Ft4Composer.ComposeSignal(Message, drivePeak: 0.0f);

        // **THE FREQUENCY THAT SEPARATES THE TWO ARITHMETICS.** At 12 000 Hz the
        // Nyquist limit is 6 000; a tone 0 at 5 950 Hz puts FT4's top tone at
        // 6 012.5 Hz - above it - and FT8's at 5 993.75 Hz - below it.
        const float justInsideForFt8 = 5_950.0f;

        var frequency = Ft4Composer.ComposeSignal(Message, baseFrequencyHz: justInsideForFt8);
        var ft8AtTheSameFrequency =
            Ft8Composer.ComposeSignal(Message, baseFrequencyHz: justInsideForFt8);

        foreach (var (name, result) in new[]
        {
            ("nothing to say  ", nothing),
            ("will not pack   ", willNot),
            ("sample rate     ", rate),
            ("base frequency  ", frequency),
            ("drive level     ", drive),
        })
        {
            _output.WriteLine(name + ": " + result.Refusal);
            _output.WriteLine("                  " + result.Explanation);

            Assert.False(result.Composed);
            Assert.NotEmpty(result.Explanation);
        }

        Assert.Equal(Ft8ComposeRefusal.NothingToSay, nothing.Refusal);
        Assert.Equal(Ft8ComposeRefusal.WillNotPack, willNot.Refusal);
        Assert.Equal(Ft8ComposeRefusal.SampleRateRefused, rate.Refusal);
        Assert.Equal(Ft8ComposeRefusal.BaseFrequencyRefused, frequency.Refusal);
        Assert.Equal(Ft8ComposeRefusal.DriveLevelRefused, drive.Refusal);

        // **THE SAME FREQUENCY IS FINE FOR FT8**, which is what makes the refusal
        // above FT4's own arithmetic rather than a shared constant.
        _output.WriteLine(string.Empty);
        _output.WriteLine("FT8 at " + justInsideForFt8 + " Hz: "
            + (ft8AtTheSameFrequency.Composed ? "composed" : ft8AtTheSameFrequency.Refusal.ToString()));
        _output.WriteLine("FT8 top tone     : "
            + (justInsideForFt8 + ((Ft8Waveform.ToneCount - 1) * Ft8Waveform.ToneSpacingHz)) + " Hz");
        _output.WriteLine("FT4 top tone     : "
            + (justInsideForFt8 + ((Ft4Waveform.ToneCount - 1) * Ft4Waveform.ToneSpacingHz)) + " Hz");

        Assert.True(ft8AtTheSameFrequency.Composed, ft8AtTheSameFrequency.Explanation);
    }

    /// <summary>
    /// **The drive is applied once, at compose time, and it is the same level.**
    /// </summary>
    /// <remarks>
    /// `Ft8Transmission.PeakSample` is measured off the array every time it is
    /// asked for, so a transmission scaled at compose time carries the peak it will
    /// actually play at. **There is one default level in the assembly** and FT4
    /// reads FT8's rather than declaring a second one to disagree with it.
    /// </remarks>
    [Fact]
    public void TheDriveIsAppliedOnceAtComposeTimeAndIsTheOperatorsOwn()
    {
        var atDefault = Ft4Composer.ComposeSignal(Message);
        var atHalf = Ft4Composer.ComposeSignal(Message, drivePeak: 0.5f);

        Assert.True(atDefault.Composed, atDefault.Explanation);
        Assert.True(atHalf.Composed, atHalf.Explanation);

        _output.WriteLine("default drive    : " + Ft8Composer.DefaultDrivePeak);
        _output.WriteLine("peak at default  : " + atDefault.Transmission!.PeakSample.ToString("F4"));
        _output.WriteLine("peak at 0.5      : " + atHalf.Transmission!.PeakSample.ToString("F4"));

        Assert.Equal(
            Ft8Composer.DefaultDrivePeak, atDefault.Transmission.PeakSample, 3);
        Assert.Equal(0.5f, atHalf.Transmission.PeakSample, 3);
    }

    /// <summary>
    /// **The padded slot and the bare signal differ in one call and nothing else.**
    /// </summary>
    /// <remarks>
    /// The same property `Ft8Composer`'s own remark states about its two routes.
    /// The signal is what goes on the air; the padded slot is what a decoder reads,
    /// and playing it would key the radio and send silence while every other station
    /// on the band started on time.
    /// </remarks>
    [Fact]
    public void TheTwoFt4RoutesSayTheSameThingAndDifferOnlyInPadding()
    {
        var signal = Ft4Composer.ComposeSignal(Message);
        var slot = Ft4Composer.Compose(Message);

        Assert.True(signal.Composed, signal.Explanation);
        Assert.True(slot.Composed, slot.Explanation);

        var padding = Ft4Waveform.PaddingSampleCount(signal.Transmission!.SampleRate);

        _output.WriteLine("signal samples   : " + signal.Transmission.Samples.Length);
        _output.WriteLine("slot samples     : " + slot.Transmission!.Samples.Length);
        _output.WriteLine("padding each end : " + padding);

        Assert.Equal(signal.Transmission.Text, slot.Transmission.Text);
        Assert.Equal(signal.Transmission.ReadsBackAs, slot.Transmission.ReadsBackAs);
        Assert.Equal(
            signal.Transmission.Samples.Length + (2 * padding),
            slot.Transmission.Samples.Length);

        // The signal sits inside the padded slot exactly where the port put it.
        for (var i = 0; i < signal.Transmission.Samples.Length; i++)
        {
            Assert.Equal(signal.Transmission.Samples[i], slot.Transmission.Samples[padding + i]);
        }
    }

    /// <summary>
    /// **THE CONTROL: an FT8 press composes the same array it did before this
    /// unit.**
    /// </summary>
    /// <remarks>
    /// <para>**PROVED AGAINST THE PORT RATHER THAN AGAINST A STORED ARRAY**, which
    /// is the same thing: at HEAD `9a81d4d` <c>Ft8Composer.Build</c> called
    /// <c>Ft8SymbolEncoder.Encode</c> and <c>Ft8Waveform.Synthesize</c> on the bits
    /// the shared packer produced and scaled the result once. This asserts exactly
    /// that, sample for sample, so the seam moving into
    /// <see cref="DigitalComposer"/> cannot have moved a byte of what FT8
    /// composes.</para>
    /// <para>**IT IS IN THIS FILE ON PURPOSE.** The control for a change belongs
    /// beside the change; a control in another file is one somebody can move the
    /// behaviour without running.</para>
    /// </remarks>
    [Fact]
    public void AnFt8PressComposesTheSameArrayItDidBeforeThisUnit()
    {
        var composed = Ft8Composer.ComposeSignal(Message, drivePeak: 1.0f);

        Assert.True(composed.Composed, composed.Explanation);

        var transmission = composed.Transmission!;

        var bits = new byte[Ft8Payload.MessageBytes];

        Assert.Equal(
            Ft8PackResult.Ok,
            Ft8StandardMessage.TryPack("W1ABC", "KC3QIS", "FN00", null, bits));

        var expected = Ft8Waveform.Synthesize(
            Ft8SymbolEncoder.Encode(bits),
            transmission.SampleRate,
            transmission.BaseFrequencyHz);

        _output.WriteLine("FT8 composed     : " + transmission.Samples.Length
            + " samples at " + transmission.SampleRate + " Hz");
        _output.WriteLine("FT8 from the port: " + expected.Length + " samples");
        _output.WriteLine("reads back as    : \"" + transmission.ReadsBackAs + "\"");

        Assert.Equal(expected.Length, transmission.Samples.Length);
        Assert.Equal(expected, transmission.Samples);
        Assert.Equal(Message, transmission.ReadsBackAs);
    }
}
