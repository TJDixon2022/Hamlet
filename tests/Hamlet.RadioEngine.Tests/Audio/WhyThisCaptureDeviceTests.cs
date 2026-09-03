using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Which of the five rules picked the capture device, for every one of them
/// (unit 236).
/// </summary>
/// <remarks>
/// <para>**THE CHOICE IS MADE FOR THE OPERATOR AND WAS RECORDED NOWHERE.** Unit
/// 235 measured that <c>AudioInputDeviceId</c> is not present in his settings at
/// all, so the first rule below — the only one that is his own decision — has
/// never once run on his machine. On 2026-09-03 he sat at 14.074, pressed what
/// this phase was built for, and no file anywhere said which sound card Hamlet had
/// opened or why.</para>
/// <para>**THE BRANCH THAT MATTERS IS THE FOURTH.** A machine with no
/// <c>USB Audio CODEC</c> in the list and nothing remembered falls through to
/// whatever Windows calls the default input, and everything below that point works
/// perfectly — on room noise, all morning.</para>
/// <para>**NO HARDWARE AND NO ENUMERATION** (`ARBITER.md` §6). Every input here is
/// a list this file builds. Nothing opens a device and nothing asks this machine
/// what it has.</para>
/// <para>**AND THE DEVICE IS NEVER THE ANSWER** (HM-DEC-018). What is recorded is
/// the branch. A device name can carry a computer's name, a person's name or the
/// model of somebody's headset.</para>
/// </remarks>
public sealed class WhyThisCaptureDeviceTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the branch table is printed.</param>
    public WhyThisCaptureDeviceTests(ITestOutputHelper output)
        => _output = output;

    private static AudioDevice Codec =>
        new("id-codec", "Microphone (USB Audio CODEC)");

    private static AudioDevice Default =>
        new("id-built-in", "Microphone Array", IsDefault: true);

    private static AudioDevice Webcam =>
        new("id-webcam", "Webcam Microphone");

    /// <summary>**NOTHING PLUGGED IN AT ALL.**</summary>
    [Fact]
    public void AnEmptyListChoosesNothingAndSaysSo()
    {
        var choice = AudioDeviceChoice.ChooseWithReason(
            Array.Empty<AudioDevice>(), "id-codec");

        Print("no devices at all", choice);

        Assert.Null(choice.Device);
        Assert.Equal(AudioDeviceChoiceReason.NothingToChooseFrom, choice.Reason);
    }

    /// <summary>
    /// **THE ONLY BRANCH THAT IS THE OPERATOR'S OWN DECISION**, and it outranks
    /// every guess Hamlet makes — including a device that looks like the radio.
    /// </summary>
    [Fact]
    public void TheRememberedDeviceIsRecordedAsHisChoiceAndNotAsAGuess()
    {
        var choice = AudioDeviceChoice.ChooseWithReason(
            new[] { Default, Codec, Webcam }, "id-webcam");

        Print("he chose the webcam last time", choice);

        Assert.Equal("id-webcam", choice.Device?.Id);
        Assert.Equal(AudioDeviceChoiceReason.OperatorsRemembered, choice.Reason);
    }

    /// <summary>
    /// **A REMEMBERED DEVICE THAT HAS GONE FALLS THROUGH QUIETLY**, and the record
    /// then says the codec was a guess rather than his choice.
    /// </summary>
    [Fact]
    public void ARememberedDeviceThatVanishedIsNotClaimedAsHisChoice()
    {
        var choice = AudioDeviceChoice.ChooseWithReason(
            new[] { Default, Codec }, "id-he-unplugged-this");

        Print("his device is gone, the radio's codec is here", choice);

        Assert.Equal("id-codec", choice.Device?.Id);
        Assert.Equal(AudioDeviceChoiceReason.LooksLikeRadio, choice.Reason);
    }

    /// <summary>**THE RADIO'S CODEC, WITH NOTHING REMEMBERED.**</summary>
    [Fact]
    public void TheRadiosCodecIsChosenWhenNothingIsRemembered()
    {
        var choice = AudioDeviceChoice.ChooseWithReason(
            new[] { Default, Codec }, rememberedId: null);

        Print("nothing remembered, the radio is plugged in", choice);

        Assert.Equal("id-codec", choice.Device?.Id);
        Assert.Equal(AudioDeviceChoiceReason.LooksLikeRadio, choice.Reason);
        Assert.True(choice.Device?.LooksLikeRadio);
    }

    /// <summary>
    /// **THIS IS THE ONE THE UNIT EXISTS FOR.** No codec in the list and nothing
    /// remembered, so Hamlet takes whatever Windows calls the default input.
    /// </summary>
    /// <remarks>
    /// This is the shape of a morning where the radio was off, or its USB cable
    /// was out, or Windows had renamed the codec. Hamlet listens to a laptop
    /// microphone in a quiet room, every layer below works perfectly, and the
    /// table stays empty. Unit 235 measured that this operator has no remembered
    /// device, so this branch or the one below it is the live case on his machine
    /// whenever the codec is not enumerated.
    /// </remarks>
    [Fact]
    public void NoCodecAndNothingRememberedFallsThroughToWhateverWindowsOffers()
    {
        var choice = AudioDeviceChoice.ChooseWithReason(
            new[] { Default, Webcam }, rememberedId: null);

        Print("no radio in the list, nothing remembered", choice);

        Assert.Equal("id-built-in", choice.Device?.Id);
        Assert.Equal(AudioDeviceChoiceReason.SystemDefault, choice.Reason);
        Assert.False(choice.Device?.LooksLikeRadio);
    }

    /// <summary>
    /// **AND WITH NO DEFAULT EITHER, THE FIRST THING IN THE LIST.** Which is a
    /// guess with nothing behind it at all, and the record now says so.
    /// </summary>
    [Fact]
    public void WithNoCodecAndNoDefaultTheFirstDeviceIsTakenAndNamedAsSuch()
    {
        var choice = AudioDeviceChoice.ChooseWithReason(
            new[] { Webcam, new AudioDevice("id-second", "Some Other Input") },
            rememberedId: null);

        Print("no radio, no default, nothing remembered", choice);

        Assert.Equal("id-webcam", choice.Device?.Id);
        Assert.Equal(AudioDeviceChoiceReason.FirstInTheList, choice.Reason);
    }

    /// <summary>
    /// **THE EXISTING CALLERS MUST NOT CHANGE BEHAVIOUR**, which is unit 233's
    /// precedent for adding beside rather than substituting.
    /// </summary>
    /// <remarks>
    /// <see cref="AudioDeviceChoice.Choose"/> is the new method with the reason
    /// dropped, so there is one copy of the order rather than two that could
    /// drift. This holds every case above to that.
    /// </remarks>
    [Fact]
    public void ChooseStillReturnsExactlyWhatItAlwaysDid()
    {
        var devices = new[] { Default, Codec, Webcam };

        foreach (var remembered in new[] { "id-webcam", "id-gone", null })
        {
            Assert.Same(
                AudioDeviceChoice.ChooseWithReason(devices, remembered).Device,
                AudioDeviceChoice.Choose(devices, remembered));
        }

        Assert.Null(AudioDeviceChoice.Choose(Array.Empty<AudioDevice>(), null));

        // Every branch of the enum is reachable from a list, so none of the five
        // is a state the record can never actually hold.
        var reached = new[]
        {
            AudioDeviceChoice.ChooseWithReason(Array.Empty<AudioDevice>(), null).Reason,
            AudioDeviceChoice.ChooseWithReason(devices, "id-webcam").Reason,
            AudioDeviceChoice.ChooseWithReason(devices, null).Reason,
            AudioDeviceChoice.ChooseWithReason(new[] { Default, Webcam }, null).Reason,
            AudioDeviceChoice.ChooseWithReason(new[] { Webcam }, null).Reason,
        };

        Assert.Equal(
            Enum.GetValues<AudioDeviceChoiceReason>().OrderBy(r => r),
            reached.OrderBy(r => r));
    }

    /// <summary>Print the branch before anything is asserted about it.</summary>
    /// <param name="situation">What the machine was offering.</param>
    /// <param name="choice">What came back.</param>
    /// <remarks>
    /// **THE DEVICE ID IS PRINTED HERE AND NOWHERE ELSE.** These are ids this file
    /// invented, in a test's own output. Nothing of the sort reaches telemetry, a
    /// commit message or a report (HM-DEC-018).
    /// </remarks>
    private void Print(string situation, AudioDeviceChoiceResult choice)
        => _output.WriteLine(
            $"  {situation} -> {choice.Reason} ({choice.Device?.Id ?? "nothing"})");
}
