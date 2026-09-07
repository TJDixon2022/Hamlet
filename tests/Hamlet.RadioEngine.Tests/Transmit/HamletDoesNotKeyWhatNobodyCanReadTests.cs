using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **Hamlet does not key a message nobody can read, it says so when it refuses,
/// and a good message still goes through the same call** (work instruction 272,
/// task 2).
/// </summary>
/// <remarks>
/// <para>**THE GATE IS ASSERTED IN BOTH DIRECTIONS OR IT IS NOT A GATE.** A guard
/// that refuses everything is a dead path and would pass half of these; every
/// case here has its mirror, and the good message goes through the same three
/// calls the refused one stopped at.</para>
/// <para>**IT IS THE SEND PATH'S OWN THREE CALLS, IN ORDER.**
/// <c>MainWindowViewModel.SendMessage</c> composes with
/// <c>Ft8Composer.ComposeSignal(wanted, _transmitSampleRate,
/// Ft8Composer.DefaultBaseFrequencyHz, _settings.TransmitDrivePeak)</c>, asks
/// <see cref="Ft8ReadBack.Check"/>, and arms with
/// <c>_armedSend.Arm(new OperatorSend(...))</c>. <see cref="Click"/> makes those
/// three calls with those arguments and adds nothing - **the arming, the port and
/// the sink are the real ones**, <c>Ft8ArmedSend</c> over
/// <c>Ft8TransmitSequence</c> over <see cref="FakeSerialPort"/> and
/// <see cref="FakeTransmitAudioSink"/>, which is the harness
/// <c>OneClickSendsExactlyOneMessageTests</c> has used since unit 257.</para>
/// <para>**WHY IT IS HERE AND NOT IN THE APPLICATION'S OWN TESTS.** The click
/// itself is asserted by
/// <c>Hamlet.App.Tests/ViewModels/TheSendPathRefusesWhatNobodyCanReadTests</c>,
/// which drives <c>SendMessageCommand.Execute</c> and reads
/// <c>DigitalSendLine</c>. Work instruction 272 forbids running
/// <c>Hamlet.App.Tests</c>, so that file is built and not run and this one - which
/// may be run - measures the same gate on the real armed send, the real sequence
/// and the real wire.</para>
/// <para>**NOTHING HERE OPENS A DEVICE, A PORT OR A WINDOW.**</para>
/// </remarks>
public sealed class HamletDoesNotKeyWhatNobodyCanReadTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the Send area line is quoted.</param>
    public HamletDoesNotKeyWhatNobodyCanReadTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A slot boundary, and the offset unit 255 recorded.</summary>
    private static readonly DateTime Boundary =
        new(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc);

    /// <summary>What the operator's own render endpoint declares.</summary>
    private const int EndpointRate = 48_000;

    /// <summary>The drive he set, as FACT-005 records it.</summary>
    private const float Drive = 0.25f;

    /// <summary>
    /// **THE ONE. The exact string that went out on a live antenna, clicked
    /// through to where the arm would happen: nothing armed, zero bytes at the
    /// port, the sound card never touched.**
    /// </summary>
    [Fact]
    public async Task TheStringThatWentOutOnALiveAntennaArmsNothingAndSaysSo()
    {
        var click = Click("VP2MAA KC3QIS FN00DJ");

        _output.WriteLine("he clicked        : \"VP2MAA KC3QIS FN00DJ\"");
        _output.WriteLine("the encoder made  : \"" + click.ReadsBackAs + "\"");
        _output.WriteLine("hashed callsign   : " + click.Hashed);
        _output.WriteLine("armed             : " + click.Armed.IsArmed);
        _output.WriteLine("bytes at the port : " + click.Port.Written.Length);
        _output.WriteLine("sink touched      : " + !click.Sink.WasNeverTouched);
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE SEND AREA LINE, IN FULL:");
        _output.WriteLine(click.SendLine);

        // IT COMPOSED. That is the whole difficulty and it is why the sentence
        // is the *composed and sent nothing* one.
        Assert.True(click.Composed);
        Assert.Equal("<VP2MAA KC3QIS> FN00DJ", click.ReadsBackAs);

        // NOTHING ARMED, NOTHING KEYED, NOTHING PLAYED.
        Assert.False(click.Armed.IsArmed);
        Assert.Empty(click.Port.Written);
        Assert.Equal(0, click.Port.WritesAttempted);
        Assert.True(click.Sink.WasNeverTouched);

        // AND DRIVING THE BOUNDARY IT WOULD HAVE GONE OUT ON STILL SENDS
        // NOTHING - a refusal that leaves something armed is not a refusal.
        var due = await click.Armed.AtBoundaryAsync(Boundary);

        Assert.Equal(Ft8ArmOutcome.NothingArmed, due.Outcome);
        Assert.Null(due.Run);
        Assert.Empty(click.Port.Written);
        Assert.True(click.Sink.WasNeverTouched);

        // THE LINE NAMES WHAT HE CLICKED AND WHAT THE ENCODER MADE OF IT, in the
        // house shape the send path's two other refusals already use.
        Assert.StartsWith(
            "Hamlet composed \"VP2MAA KC3QIS FN00DJ\" and sent nothing: ",
            click.SendLine, StringComparison.Ordinal);
        Assert.Contains("<VP2MAA KC3QIS> FN00DJ", click.SendLine, StringComparison.Ordinal);
        Assert.Contains("hash", click.SendLine, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **The failing combinations task 1 found are refused the same way** - a
    /// compound prefix on the DX station, a long compound call, and the operator's
    /// own compound form.
    /// </summary>
    /// <remarks>
    /// Task 1 measured 15 of 43, and every one of the fifteen is a compound
    /// prefix. **Portable suffixes are not in the class** - <c>K1ABC/P</c> packs
    /// standard and reads back as itself - and its mirror is asserted below, so
    /// this is not a guard that refuses everything with a slash in it.
    /// </remarks>
    [Theory]
    [InlineData("VP2M/K1ABC KC3QIS FN00", "<VP2M/K1ABC> KC3QIS FN00")]
    [InlineData("SV9/PA3EXX KC3QIS -12", "<SV9/PA3EXX> KC3QIS -12")]
    [InlineData("K1ABC W4/KC3QIS 73", "K1ABC <W4/KC3QIS> 73")]
    public void EveryFailingCombinationTaskOneFoundIsRefusedTheSameWay(
        string clicked, string encodedAs)
    {
        var click = Click(clicked);

        _output.WriteLine($"\"{clicked}\" -> \"{click.ReadsBackAs}\"");
        _output.WriteLine(click.SendLine);

        Assert.True(click.Composed);
        Assert.Equal(encodedAs, click.ReadsBackAs);

        Assert.False(click.Armed.IsArmed);
        Assert.Empty(click.Port.Written);
        Assert.True(click.Sink.WasNeverTouched);

        Assert.StartsWith(
            "Hamlet composed \"" + clicked + "\" and sent nothing: ",
            click.SendLine, StringComparison.Ordinal);
        Assert.Contains(encodedAs, click.SendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **THE OTHER HALF, OR IT IS NOT A GATE. A message that reads back as itself
    /// still arms and still sends, through the same call, with the port carrying
    /// its frames.**
    /// </summary>
    /// <remarks>
    /// Four of them, and one is a portable suffix - the shape nearest to the
    /// refused ones and the one a guard written about slashes rather than about
    /// hashing would wrongly stop.
    /// </remarks>
    [Theory]
    [InlineData("CQ KC3QIS FN00")]
    [InlineData("VP2MAA KC3QIS FN00")]
    [InlineData("K1ABC/P KC3QIS -12")]
    [InlineData("CQ KC3QIS/P FN00")]
    public async Task AMessageThatReadsBackAsItselfStillArmsAndStillSends(string clicked)
    {
        var click = Click(clicked);

        Assert.True(click.Composed);
        Assert.False(click.Hashed);
        Assert.Equal(clicked, click.ReadsBackAs);

        // IT ARMED, and the line is the sending one rather than a refusal.
        Assert.True(click.Armed.IsArmed);
        Assert.Equal(string.Empty, click.SendLine);

        var due = await click.Armed.AtBoundaryAsync(Boundary);

        _output.WriteLine($"\"{clicked}\" -> \"{click.ReadsBackAs}\"");
        _output.WriteLine("outcome           : " + due.Outcome);
        _output.WriteLine("sent              : " + due.Run!.Sent);
        _output.WriteLine("bytes at the port : " + click.Port.Written.Length);
        _output.WriteLine("wire              : "
            + TheUnkeyHappensWhateverGoesWrongTests.Hex(click.Port.Written));
        _output.WriteLine("samples to the card: " + click.Sink.SamplesHandedOver
            + " at " + click.Sink.RateAskedFor + " Hz");

        Assert.Equal(Ft8ArmOutcome.Ran, due.Outcome);
        Assert.True(due.Run.Sent, due.Run.Reason);

        // THE PORT CARRIED ITS FRAMES and the card was handed the audio.
        Assert.NotEmpty(click.Port.Written);
        Assert.Equal(2, click.Port.WritesAttempted);
        Assert.False(click.Sink.WasNeverTouched);
        Assert.Equal(EndpointRate, click.Sink.RateAskedFor);
    }

    /// <summary>
    /// **The brackets case is unreachable against this tree, and it is recorded
    /// rather than assumed.**
    /// </summary>
    /// <remarks>
    /// <para>The arbiter's ruling is *a hashed callsign is not the words the
    /// operator clicked, unless he clicked brackets*, so the rule is written on
    /// the two strings: where what the bits say is character-for-character what he
    /// typed, the encoder made no substitution and there is nothing to refuse.
    /// **Measured, this branch is never reached** - <c>Ft8Composer</c> refuses a
    /// bracketed string outright, before the guard is asked, so no bracketed
    /// message exists to arm or to stop.</para>
    /// <para>**The rule keeps its second half anyway**, because it costs nothing
    /// and because a guard written on the flag alone would refuse such a message
    /// for the wrong reason if the composer ever accepted one. This test is what
    /// says the branch is dead and why.</para>
    /// </remarks>
    [Fact]
    public void BracketsAreRefusedByTheComposerBeforeTheGuardIsEverAsked()
    {
        var click = Click("<VP2M/K1ABC> KC3QIS FN00");

        _output.WriteLine("he clicked       : \"<VP2M/K1ABC> KC3QIS FN00\"");
        _output.WriteLine("composed         : " + click.Composed);
        _output.WriteLine("the Send area    : " + click.SendLine);

        // THE COMPOSER SAYS NO FIRST, so the guard is never asked at all.
        Assert.False(click.Composed);
        Assert.False(click.Armed.IsArmed);
        Assert.Empty(click.Port.Written);
        Assert.True(click.Sink.WasNeverTouched);

        // AND IT IS THE COMPOSER'S OWN SENTENCE, not this unit's.
        Assert.StartsWith(
            "Hamlet did not send \"<VP2M/K1ABC> KC3QIS FN00\": ",
            click.SendLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// **Nothing about this guard reaches the menu**, read off
    /// <c>Ft8SendOptions</c>'s own answer rather than asserted.
    /// </summary>
    /// <remarks>
    /// The five shapes toward a station whose every message is refused are still
    /// five, still in exchange order, still with their labels and their expected
    /// mark. **Nothing is forbidden in the menu**; the refusal is at the send path
    /// where the licence gate already refuses.
    /// </remarks>
    [Fact]
    public void TheMenuStillOffersEveryMessageTowardAStationEveryMessageIsRefusedFor()
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        ledger.RecordHeard("CQ VP2M/K1ABC FK52", Boundary);

        var record = ledger.For("VP2M/K1ABC");

        Assert.NotNull(record);

        var menu = Ft8SendOptions.For(record!, "KC3QIS", "FN00DJ", -12);

        foreach (var option in menu.Options)
        {
            var click = Click(option.Text);

            _output.WriteLine(
                $"{option.Label,-22} \"{option.Text}\"  menu: offered, send path: "
                + (click.Armed.IsArmed ? "armed" : "refused"));
        }

        // EVERY ONE STILL OFFERED.
        Assert.Equal(5, menu.Options.Count);
        Assert.Equal(
            [
                Ft8SendShape.Grid, Ft8SendShape.Report, Ft8SendShape.RogerAndReport,
                Ft8SendShape.Acknowledge, Ft8SendShape.Seventy3,
            ],
            menu.Options.Select(o => o.Shape).ToArray());

        // AND EVERY ONE REFUSED AT THE SEND PATH, which is the point: the menu is
        // not where this is answered.
        Assert.All(menu.Options, o => Assert.False(Click(o.Text).Armed.IsArmed));
    }

    /// <summary>
    /// One click, made with the send path's own three calls and its own arguments.
    /// </summary>
    /// <param name="text">The message, exactly as the operator clicked it.</param>
    /// <returns>What the click left behind, and the line the Send area carries.</returns>
    private static Clicked Click(string text)
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        // SendMessage:8468, with the endpoint's rate and the operator's drive.
        var composed = Ft8Composer.ComposeSignal(
            text, EndpointRate, Ft8Composer.DefaultBaseFrequencyHz, Drive);

        if (!composed.Composed)
        {
            return new Clicked(
                armed, port, sink, Composed: false, Hashed: false,
                ReadsBackAs: "(refused)",
                SendLine: "Hamlet did not send \"" + text + "\": " + composed.Explanation);
        }

        var transmission = composed.Transmission!;

        // SendMessage, between :8476 and :8481 - the line this unit adds.
        var verdict = Ft8ReadBack.Check(transmission);

        if (!verdict.WouldReachAnybody)
        {
            return new Clicked(
                armed, port, sink, Composed: true,
                Hashed: transmission.CarriesHashedCallsign,
                ReadsBackAs: transmission.ReadsBackAs,
                SendLine: Ft8ReadBack.SentNothing(text, verdict));
        }

        // SendMessage:8498.
        armed.Arm(new OperatorSend(
            transmission, 14_074_000, LicenseClass.General, true, Boundary, 0.5));

        return new Clicked(
            armed, port, sink, Composed: true,
            Hashed: transmission.CarriesHashedCallsign,
            ReadsBackAs: transmission.ReadsBackAs,
            SendLine: string.Empty);
    }

    /// <summary>What one click left behind.</summary>
    /// <param name="Armed">The real armed send, asked whether anything is on it.</param>
    /// <param name="Port">The wire, asked how many bytes it took.</param>
    /// <param name="Sink">The sound card, asked whether it was touched at all.</param>
    /// <param name="Composed">Whether the composer returned audio.</param>
    /// <param name="Hashed">Whether a callsign travelled as a hash.</param>
    /// <param name="ReadsBackAs">What the bits say.</param>
    /// <param name="SendLine">The Send area line, empty where it armed.</param>
    private sealed record Clicked(
        Ft8ArmedSend Armed,
        FakeSerialPort Port,
        FakeTransmitAudioSink Sink,
        bool Composed,
        bool Hashed,
        string ReadsBackAs,
        string SendLine);
}
