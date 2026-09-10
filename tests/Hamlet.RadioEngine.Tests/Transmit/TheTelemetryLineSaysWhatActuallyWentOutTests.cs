using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **The telemetry line says whether the encoded message hashed a callsign, and
/// its length counts the encoded message** (work instruction 272, task 3).
/// </summary>
/// <remarks>
/// <para>**THE FIELD THAT WOULD HAVE SHOUTED IS THE ONE NOBODY WROTE.** On
/// 2026-09-07 Hamlet keyed twice on 14.074 and both slots decoded to nothing.
/// The line the second one wrote said <c>messageType: Standard, messageLength:
/// 16</c> and a reader would have called it healthy - because sixteen is the
/// length of <c>CQ KC3QIS FN00DJ</c>, the string the operator asked for, and what
/// actually went out was <c>&lt;CQ KC3QIS&gt; &lt;FN00DJ&gt;</c>, twenty
/// characters, with the callsign as a 22-bit hash.</para>
/// <para>**A LENGTH THAT MEASURES THE WRONG THING IS WORSE THAN NO LENGTH.** Unit
/// 271 reported this and declined to change it because choosing what a diagnostic
/// counts is a decision rather than a repair; work instruction 272's arbiter made
/// the decision and this implements it. <c>MessageLength</c> now counts
/// <c>Ft8Transmission.ReadsBackAs</c> - the message as its own bits read back -
/// which is what went on the air.</para>
/// <para>**HM-DEC-018 HOLDS ABSOLUTELY.** What goes in is shape: a type, a flag
/// and a length. No message text reaches <c>TransmitRecord</c>, its constructor
/// still has no string parameter at all, and
/// <c>ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt</c> is what asserts
/// that by reflection rather than by inspection.</para>
/// <para>**IT IS ASSERTED AT THE SEQUENCE, DELIBERATELY.** Task 2's guard means a
/// hashed message can no longer reach <c>Recorded</c> through the application at
/// all - the send path refuses it before anything is armed. The field still has
/// to be right for anything that reaches the sequence by another route later, so
/// these drive <c>Ft8TransmitSequence.RunAsync</c> directly with a transmission
/// the composer really produced.</para>
/// <para>**NOTHING HERE OPENS A DEVICE OR A PORT.**</para>
/// </remarks>
public sealed class TheTelemetryLineSaysWhatActuallyWentOutTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the two bags are printed.</param>
    public TheTelemetryLineSaysWhatActuallyWentOutTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The string that went out on a live antenna, and hashed.</summary>
    private const string TheOneThatHashed = "VP2MAA KC3QIS FN00DJ";

    /// <summary>A message of the same shape that did not.</summary>
    private const string TheOneThatDidNot = "VP2MAA KC3QIS FN00";

    /// <summary>
    /// **THE TWO BAGS. A transmission that hashed and one that did not, and they
    /// differ in the field that would have shouted.**
    /// </summary>
    [Fact]
    public async Task TheTwoBagsDifferInTheFieldThatWouldHaveShouted()
    {
        var hashed = await Written(TheOneThatHashed);
        var inFull = await Written(TheOneThatDidNot);

        Print("HASHED  - \"" + TheOneThatHashed + "\"", hashed);
        Print("IN FULL - \"" + TheOneThatDidNot + "\"", inFull);

        // **THE NEW FIELD, AND IT IS THE ONLY THING THAT SEPARATES THEM ON THE
        // TYPE.** Both are ordinary-looking; one of them decodes to nothing.
        Assert.Equal(true, hashed["carriedHashedCallsign"]);
        Assert.Equal(false, inFull["carriedHashedCallsign"]);

        // AND THE LINE THAT LOOKED HEALTHY NO LONGER DOES.
        Assert.NotEqual(hashed["carriedHashedCallsign"], inFull["carriedHashedCallsign"]);
    }

    /// <summary>
    /// **The length counts the encoded message, not the string the operator
    /// clicked.**
    /// </summary>
    /// <remarks>
    /// <c>VP2MAA KC3QIS FN00DJ</c> is twenty characters and went out as
    /// <c>&lt;VP2MAA KC3QIS&gt; FN00DJ</c>, twenty-two. The old field counted
    /// twenty and described something that was never transmitted.
    /// </remarks>
    [Fact]
    public async Task TheLengthCountsWhatWentOutAndNotWhatWasAskedFor()
    {
        var composed = Ft8Composer.ComposeSignal(TheOneThatHashed);

        Assert.True(composed.Composed);

        var transmission = composed.Transmission!;
        var bag = await Written(TheOneThatHashed);

        _output.WriteLine($"he asked for  : \"{transmission.Text}\" ({transmission.Text.Length})");
        _output.WriteLine(
            $"what went out : \"{transmission.ReadsBackAs}\" ({transmission.ReadsBackAs.Length})");
        _output.WriteLine($"messageLength : {bag["messageLength"]}");

        Assert.Equal(20, transmission.Text.Length);
        Assert.Equal(22, transmission.ReadsBackAs.Length);

        Assert.Equal(transmission.ReadsBackAs.Length, bag["messageLength"]);
        Assert.NotEqual(transmission.Text.Length, bag["messageLength"]);
    }

    /// <summary>
    /// **What the line said on 2026-09-07, and what it says now.**
    /// </summary>
    /// <remarks>
    /// The CQ string is the one whose line read <c>messageType: Standard,
    /// messageLength: 16</c>. The type is still <c>Standard</c> - that is a true
    /// fact about the format and is not this unit's to change - and what has
    /// changed is that the line beside it now says the callsign was hashed, and
    /// that the length is the twenty characters that went out.
    /// </remarks>
    [Fact]
    public async Task WhatTheLineSaidOnTheSeventhOfSeptemberAndWhatItSaysNow()
    {
        var bag = await Written("CQ KC3QIS FN00DJ");

        Print("THE SECOND SLOT ON 14.074, 2026-09-07", bag);

        // THE TWO FIELDS A READER HAD THAT NIGHT, unchanged in kind.
        Assert.Equal("Standard", bag["messageType"]);

        // AND WHAT THEY NOW SAY.
        Assert.Equal(true, bag["carriedHashedCallsign"]);
        Assert.Equal("<CQ KC3QIS> <FN00DJ>".Length, bag["messageLength"]);
        Assert.NotEqual("CQ KC3QIS FN00DJ".Length, bag["messageLength"]);
    }

    /// <summary>
    /// **A message that reads back as itself is unchanged in every field**, so
    /// the new flag is news rather than noise and no existing line moved.
    /// </summary>
    [Fact]
    public async Task AnOrdinaryTransmissionsLineIsUnchanged()
    {
        var bag = await Written("CQ KC3QIS FN00");

        Print("AN ORDINARY TRANSMISSION", bag);

        Assert.Equal(false, bag["carriedHashedCallsign"]);

        // The length is the same number it always was, because for a message that
        // reads back as itself the two counts are the same count.
        Assert.Equal("CQ KC3QIS FN00".Length, bag["messageLength"]);
        Assert.Equal("Standard", bag["messageType"]);
        Assert.Equal(Ft8TransmitOutcome.Played.ToString(), bag["outcome"]);
        Assert.Equal(true, bag["keyed"]);
    }

    /// <summary>
    /// **And still no words** - the new field is a flag and it opened no door.
    /// </summary>
    [Fact]
    public async Task TheNewFieldCarriesNoWords()
    {
        var bag = await Written(TheOneThatHashed);
        var everything = string.Join(" | ", bag.Select(p => $"{p.Key}={p.Value}"));

        _output.WriteLine(everything);

        foreach (var forbidden in
            new[] { "KC3QIS", "VP2MAA", "FN00", TheOneThatHashed, "<VP2MAA KC3QIS> FN00DJ" })
        {
            Assert.DoesNotContain(forbidden, everything, StringComparison.OrdinalIgnoreCase);
        }

        // The record still has nowhere to put a string.
        var parameters = typeof(TransmitRecord).GetConstructors().Single().GetParameters();

        _output.WriteLine(
            "record parameters: "
            + string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}")));

        Assert.DoesNotContain(parameters, p => p.ParameterType == typeof(string));
    }

    /// <summary>The bag one transmission wrote, driven through the real sequence.</summary>
    private static async Task<IReadOnlyDictionary<string, object?>> Written(string text)
    {
        var telemetry = new RecordingTelemetry();
        var composed = Ft8Composer.ComposeSignal(text);

        Assert.True(composed.Composed, composed.Explanation);

        var send = new OperatorSend(
            composed.Transmission!,
            14_074_000,
            LicenseClass.General,
            true,
            new DateTime(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc),
            0.5);

        await new Ft8TransmitSequence(
                new FakeSerialPort(), new FakeTransmitAudioSink(),
                guard: null, telemetry: telemetry)
            .RunAsync(send);

        return Assert.Single(telemetry.Events).Data;
    }

    /// <summary>Prints one bag, field by field.</summary>
    private void Print(string title, IReadOnlyDictionary<string, object?> bag)
    {
        _output.WriteLine(title);

        foreach (var pair in bag)
        {
            _output.WriteLine($"  {pair.Key,-22}: {pair.Value}");
        }

        _output.WriteLine(string.Empty);
    }

    /// <summary>An <see cref="ITelemetry"/> that keeps what it was given.</summary>
    private sealed class RecordingTelemetry : ITelemetry
    {
        private readonly List<TelemetryEvent> _events = [];

        /// <summary>Everything written, in order.</summary>
        public IReadOnlyList<TelemetryEvent> Events => _events;

        /// <inheritdoc/>
        public long DroppedEventCount => 0;

        /// <inheritdoc/>
        public void Write(
            TelemetryCategory category,
            string eventName,
            IReadOnlyDictionary<string, object?>? data = null,
            TelemetryLevel level = TelemetryLevel.Info)
            => _events.Add(new TelemetryEvent(
                new DateTime(2026, 9, 7, 17, 12, 45, DateTimeKind.Utc),
                "test",
                level,
                "test",
                category,
                eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal)));
    }
}
