using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// Where in the slot a transmission starts, and what the record says about it.
/// </summary>
/// <remarks>
/// <para>**THE OPEN HALF OF STEP 2'S CRITERION 5** (work instruction 255, task
/// 4). The 12.64 s length holds at both rates and unit 254 proved it; where the
/// transmission *sits* did not, because the port centres the signal in the slot
/// and on the air that is about seven tenths of a second late. Unit 254 measured
/// it and correctly refused to fix it in the port. This is where it is fixed:
/// not by trimming an array, but by a second route from the port's own
/// <c>Ft8Waveform.Synthesize</c>, and by the send path stating where the audio
/// begins.</para>
/// <para>**THE OFFSET IS 0.5 s AND IT IS A DECISION, NOT A CITATION.** The slot
/// is 15 s and the signal 12.64, so there are 2.36 s of slack. Half a second at
/// the front leaves 1.86 s at the back: enough that a receiver whose clock is
/// fast still hears the whole transmission, and it is what stations on the band
/// do. **This repository holds no pinned document for FT8 slot timing** (§4,
/// §12.4), so the figure is recorded as a choice with its arithmetic rather than
/// quoted as a specification, and the loopback in the next unit is what would
/// correct it.</para>
/// <para>**AND THE RECORD CARRIES THE SHAPE AND NEVER THE WORDS** (HM-DEC-018).
/// The proof is the callsign test at the end: a message with a real callsign in
/// it goes through a recording telemetry sink, and the callsign appears nowhere
/// in what was written.</para>
/// </remarks>
public sealed class WhereTheTransmissionStartsAndWhatTheRecordSaysTests
{
    /// <summary>
    /// Half a second after the boundary, which is what the send path is told.
    /// </summary>
    private const double StartsAt = 0.5;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">xUnit's output sink.</param>
    public WhereTheTransmissionStartsAndWhatTheRecordSaysTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **The signal route gives 12.64 s of tones and no silence, measured off the
    /// arrays.**
    /// </summary>
    /// <remarks>
    /// Measured, not asserted: the length comes off the array, the padding comes
    /// off the difference between the two routes, and the placement comes from
    /// the port's own <c>PaddingSampleCount</c>. If those three ever disagree,
    /// this goes red.
    /// </remarks>
    [Fact]
    public void TheSignalRouteGivesTheTonesAloneAndTheSlotRouteGivesThePaddedSlot()
    {
        var signal = Ft8Composer.ComposeSignal("CQ KC3QIS FN00");
        var slot = Ft8Composer.Compose("CQ KC3QIS FN00");

        Assert.True(signal.Composed, signal.Explanation);
        Assert.True(slot.Composed, slot.Explanation);

        var signalSamples = signal.Transmission!.Samples.Length;
        var slotSamples = slot.Transmission!.Samples.Length;
        var padding = (slotSamples - signalSamples) / 2;
        var rate = signal.Transmission.SampleRate;

        _output.WriteLine($"rate                 : {rate} Hz");
        _output.WriteLine($"signal samples       : {signalSamples}");
        _output.WriteLine($"slot samples         : {slotSamples}");
        _output.WriteLine($"padding, each end    : {padding} samples, {padding / (double)rate:0.000} s");
        _output.WriteLine($"signal seconds       : {signalSamples / (double)rate:0.00000}");
        _output.WriteLine($"slot seconds         : {slotSamples / (double)rate:0.00000}");
        _output.WriteLine(
            $"port's own padding   : {Ft8Waveform.PaddingSampleCount(rate)} samples");

        // The signal is the 12.64 s the mode occupies, to the sample.
        Assert.Equal(151_680, signalSamples);
        Assert.Equal(Ft8Slots.TransmissionSeconds, signalSamples / (double)rate, 5);

        // The slot is 15 s, and unchanged from what unit 254 measured.
        Assert.Equal(180_000, slotSamples);
        Assert.Equal(Ft8Slots.SlotSeconds, slotSamples / (double)rate, 5);

        // The difference is the port's padding, split evenly, 1.180 s a side.
        Assert.Equal(Ft8Waveform.PaddingSampleCount(rate), padding);
        Assert.Equal(1.180, padding / (double)rate, 3);

        // Both routes say the same message, from the same packing.
        Assert.Equal(slot.Transmission.ReadsBackAs, signal.Transmission.ReadsBackAs);
        Assert.Equal(slot.Transmission.Type, signal.Transmission.Type);
    }

    /// <summary>
    /// **The transmission starts 0.5 s in and finishes inside the slot, measured.**
    /// </summary>
    /// <remarks>
    /// The arithmetic that makes 0.5 s the choice, done from the arrays rather
    /// than from the constants: what is left of the slot after the offset, and
    /// what is left after the transmission has finished.
    /// </remarks>
    [Fact]
    public void TheTransmissionStartsHalfASecondInAndStillFitsInTheSlot()
    {
        var signal = Ft8Composer.ComposeSignal("CQ KC3QIS FN00");
        Assert.True(signal.Composed, signal.Explanation);

        var seconds = signal.Transmission!.Samples.Length
            / (double)signal.Transmission.SampleRate;
        var endsAt = StartsAt + seconds;
        var slack = Ft8Slots.SlotSeconds - seconds;

        // What centring in the slot would have done, for comparison.
        var centred = Ft8Waveform.PaddingSampleCount(signal.Transmission.SampleRate)
            / (double)signal.Transmission.SampleRate;

        _output.WriteLine($"slot                 : {Ft8Slots.SlotSeconds:0.00} s");
        _output.WriteLine($"signal               : {seconds:0.00000} s");
        _output.WriteLine($"slack in the slot    : {slack:0.00000} s");
        _output.WriteLine($"starts at            : {StartsAt:0.000} s");
        _output.WriteLine($"ends at              : {endsAt:0.00000} s");
        _output.WriteLine($"margin at the end    : {Ft8Slots.SlotSeconds - endsAt:0.00000} s");
        _output.WriteLine($"centred would start  : {centred:0.000} s");
        _output.WriteLine($"later by             : {centred - StartsAt:0.000} s");

        // It fits, by the slot clock's own function.
        Assert.True(Ft8Slots.TransmissionFits(Ft8Slots.SlotSeconds - StartsAt));
        Assert.True(endsAt < Ft8Slots.SlotSeconds);

        // The margin at the end is what the offset buys: 1.86 s.
        Assert.Equal(1.86, Ft8Slots.SlotSeconds - endsAt, 2);

        // And centring, which is what the port does for a decoder, would have
        // started it 0.68 s later than this.
        Assert.Equal(0.68, centred - StartsAt, 2);
    }

    /// <summary>
    /// **The same placement at 48000 Hz, measured off the arrays again.**
    /// </summary>
    /// <remarks>
    /// Unit 254 proved the 12.64 s length holds at both rates. This is the other
    /// half of that at the second rate: the padding is still 1.180 s a side, so
    /// the choice of 0.5 s is a fact about the mode rather than about a rate, and
    /// the send path's arithmetic does not change with the device the next unit
    /// opens.
    /// </remarks>
    [Fact]
    public void ThePlacementIsTheSameAtFortyEightThousand()
    {
        const int rate = 48_000;

        var signal = Ft8Composer.ComposeSignal("CQ KC3QIS FN00", rate);
        var slot = Ft8Composer.Compose("CQ KC3QIS FN00", rate);

        Assert.True(signal.Composed, signal.Explanation);
        Assert.True(slot.Composed, slot.Explanation);

        var signalSamples = signal.Transmission!.Samples.Length;
        var slotSamples = slot.Transmission!.Samples.Length;
        var padding = (slotSamples - signalSamples) / 2;

        _output.WriteLine($"rate              : {rate} Hz");
        _output.WriteLine($"signal samples    : {signalSamples}");
        _output.WriteLine($"slot samples      : {slotSamples}");
        _output.WriteLine($"padding, each end : {padding} samples, {padding / (double)rate:0.000} s");
        _output.WriteLine($"signal seconds    : {signalSamples / (double)rate:0.00000}");
        _output.WriteLine($"starts at         : {StartsAt:0.000} s");
        _output.WriteLine(
            $"ends at           : {StartsAt + (signalSamples / (double)rate):0.00000} s");

        Assert.Equal(606_720, signalSamples);
        Assert.Equal(720_000, slotSamples);
        Assert.Equal(Ft8Slots.TransmissionSeconds, signalSamples / (double)rate, 5);
        Assert.Equal(1.180, padding / (double)rate, 3);
        Assert.True(Ft8Slots.TransmissionFits(Ft8Slots.SlotSeconds - StartsAt));
    }

    /// <summary>
    /// **A padded slot handed to the send path is refused rather than played.**
    /// </summary>
    /// <remarks>
    /// The measurement above is arithmetic; this is the behaviour that rests on
    /// it. Fifteen seconds of audio does not fit in the 14.5 s left after the
    /// offset, and nothing keys.
    /// </remarks>
    [Fact]
    public async Task ThePaddedSlotIsRefusedByTheSendPathAndNothingKeys()
    {
        var slot = Ft8Composer.Compose("CQ KC3QIS FN00");
        Assert.True(slot.Composed, slot.Explanation);

        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(
            new OperatorSend(
                slot.Transmission!,
                14_074_000,
                LicenseClass.General,
                GuardEnabled: true,
                new DateTime(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc),
                StartsAt));

        _output.WriteLine($"outcome : {run.Outcome}");
        _output.WriteLine($"reason  : {run.Reason}");
        _output.WriteLine($"writes  : {port.WritesAttempted}");

        Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, run.Outcome);
        Assert.Empty(port.Written);
        Assert.Equal(0, port.WritesAttempted);
        Assert.True(sink.WasNeverTouched);
        // **THE CLAUSE IS NOW OFFERED AS A LIKELIHOOD RATHER THAN ASSERTED AS THE
        // CAUSE** (work instruction 294 task 7). This branch fires on any audio
        // longer than the slot has room for; a padded slot is the commonest thing
        // that produces one and is not the only thing, and this line has measured
        // two lengths rather than made a diagnosis. **The measurement is what the
        // test holds it to**, so the numbers are asserted and the hint is asserted
        // as a hint.
        Assert.Contains("The commonest cause is a padded slot", run.Reason, StringComparison.Ordinal);
        Assert.Contains(
            "what is measured here is the length and not the reason",
            run.Reason,
            StringComparison.Ordinal);
        Assert.Contains("s of audio and only", run.Reason, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The transmitted slot is recorded, with what was sent and when.**
    /// </summary>
    /// <remarks>
    /// Step 3's fourth exit criterion. Every field is quoted into the test output
    /// so the report can carry the line verbatim.
    /// </remarks>
    [Fact]
    public async Task TheTransmittedSlotIsRecordedWithItsShapeAndItsMoment()
    {
        var telemetry = new RecordingTelemetry();
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();
        var send = TheUnkeyHappensWhateverGoesWrongTests.Send(startSecondsIntoSlot: StartsAt);

        var run = await new Ft8TransmitSequence(port, sink, guard: null, telemetry: telemetry)
            .RunAsync(send);

        var written = Assert.Single(telemetry.Events);

        _output.WriteLine($"category : {written.Category}");
        _output.WriteLine($"event    : {written.Event}");
        _output.WriteLine($"level    : {written.Level}");
        foreach (var pair in written.Data)
        {
            _output.WriteLine($"  {pair.Key,-22}: {pair.Value}");
        }

        Assert.Equal(Ft8TransmitOutcome.Sent, run.Outcome);
        Assert.Equal(TelemetryCategory.Transmit, written.Category);
        Assert.Equal("ft8_transmission", written.Event);
        Assert.Equal(TelemetryLevel.Info, written.Level);

        // When, where, how long, and at what rate.
        Assert.Equal(
            send.SlotStartUtc.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            written.Data["slotStartUtc"]);
        Assert.Equal(StartsAt, written.Data["startSecondsIntoSlot"]);
        Assert.Equal(14_074_000L, written.Data["frequencyHz"]);
        Assert.Equal(12_000, written.Data["sampleRate"]);
        Assert.Equal(151_680, written.Data["sampleCount"]);
        Assert.Equal(Ft8Slots.TransmissionSeconds, (double)written.Data["durationSeconds"]!, 5);

        // The type and the length - a fact about the format and a count.
        Assert.Equal(Ft8MessageType.Standard.ToString(), written.Data["messageType"]);
        Assert.Equal("CQ KC3QIS FN00".Length, written.Data["messageLength"]);

        // And how it ended.
        Assert.Equal(Ft8TransmitOutcome.Sent.ToString(), written.Data["outcome"]);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey.ToString(), written.Data["cameOutOfTransmit"]);
        Assert.Equal(true, written.Data["keyed"]);
    }

    /// <summary>
    /// **A transmission that ended badly is recorded as a warning.**
    /// </summary>
    [Fact]
    public async Task AnAbortedTransmissionIsRecordedAsAWarningWithHowItCameOut()
    {
        var telemetry = new RecordingTelemetry();
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink { Throws = new IOException("device lost") };

        await new Ft8TransmitSequence(port, sink, guard: null, telemetry: telemetry)
            .RunAsync(TheUnkeyHappensWhateverGoesWrongTests.Send(startSecondsIntoSlot: StartsAt));

        var written = Assert.Single(telemetry.Events);

        _output.WriteLine($"level : {written.Level}");
        foreach (var pair in written.Data)
        {
            _output.WriteLine($"  {pair.Key,-22}: {pair.Value}");
        }

        Assert.Equal(TelemetryLevel.Warn, written.Level);
        Assert.Equal(Ft8TransmitOutcome.AudioFailed.ToString(), written.Data["outcome"]);
        Assert.Equal(UnkeyRoute.TheAbort.ToString(), written.Data["cameOutOfTransmit"]);

        // NOT THE EXCEPTION'S MESSAGE. A failure's text is not the message's
        // text, but it is text, and text is where a message would eventually be
        // put by somebody being helpful.
        Assert.DoesNotContain(
            written.Data.Values.Select(v => v?.ToString() ?? string.Empty),
            value => value.Contains("device lost", StringComparison.Ordinal));
    }

    /// <summary>
    /// **The words cannot get in.**
    /// </summary>
    /// <remarks>
    /// <para>The shape <c>ADecodeWindowCannotCarryDecodedText</c> uses at
    /// <c>RecordDisciplineTests.cs:105</c>, pointed at a transmission: compose
    /// something with a callsign in it, run it through, and assert the callsign
    /// is nowhere in what was written.</para>
    /// <para>Three strings are permitted in the bag and each is checked for what
    /// it is: a moment that parses back as the moment it was given, and two names
    /// of enumeration members. **A message cannot be a member of an
    /// enumeration.**</para>
    /// </remarks>
    [Fact]
    public async Task ATransmitRecordCannotCarryTheMessageOrTheCallsignInIt()
    {
        var telemetry = new RecordingTelemetry();
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();
        var send = TheUnkeyHappensWhateverGoesWrongTests.Send(
            startSecondsIntoSlot: StartsAt, text: "CQ KC3QIS FN00");

        await new Ft8TransmitSequence(port, sink, guard: null, telemetry: telemetry)
            .RunAsync(send);

        var written = Assert.Single(telemetry.Events);
        var everything = string.Join(
            " | ", written.Data.Select(pair => $"{pair.Key}={pair.Value}"));

        _output.WriteLine($"message   : \"{send.Transmission.Text}\"");
        _output.WriteLine($"reads back: \"{send.Transmission.ReadsBackAs}\"");
        _output.WriteLine($"written   : {everything}");

        // The callsign, the grid, the whole message, and the read-back.
        foreach (var forbidden in new[] { "KC3QIS", "FN00", "CQ KC3QIS FN00", send.Transmission.ReadsBackAs })
        {
            Assert.DoesNotContain(forbidden, everything, StringComparison.OrdinalIgnoreCase);
        }

        // THE SHAPE, NOT THE CALL SITE. The record has no string parameter at
        // all, so there is nowhere for a message to be put by a later hand.
        var parameters = typeof(TransmitRecord)
            .GetConstructors()
            .Single()
            .GetParameters();

        _output.WriteLine(
            "record parameters: "
            + string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}")));

        Assert.DoesNotContain(parameters, p => p.ParameterType == typeof(string));

        // And every value written is a number, a flag, the moment it was given,
        // or the name of an enumeration member.
        foreach (var pair in written.Data)
        {
            if (pair.Value is int or long or double or bool)
            {
                continue;
            }

            var text = Assert.IsType<string>(pair.Value);

            var isMoment = pair.Key == "slotStartUtc"
                && DateTime.Parse(text, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind) == send.SlotStartUtc;

            var isEnumName =
                Enum.GetNames<Ft8MessageType>().Contains(text, StringComparer.Ordinal)
                || Enum.GetNames<Ft8TransmitOutcome>().Contains(text, StringComparer.Ordinal)
                || Enum.GetNames<UnkeyRoute>().Contains(text, StringComparer.Ordinal);

            Assert.True(
                isMoment || isEnumName,
                $"'{pair.Key}' is \"{text}\", which is neither the moment nor an enumeration name");
        }
    }

    /// <summary>
    /// **And the words do not reach the disk either.**
    /// </summary>
    /// <remarks>
    /// The fake sink proves what the sequence handed to telemetry; this proves
    /// what telemetry wrote to a file. They are different questions - a
    /// serialiser that reflected over an object rather than taking the bag would
    /// pass the first and fail this one - and the file is what leaves the
    /// machine.
    /// </remarks>
    [Fact]
    public async Task NothingOfTheMessageReachesTheFileOnDisk()
    {
        var folder = Path.Combine(
            Path.GetTempPath(), "hamlet-unit255-" + Guid.NewGuid().ToString("N")[..8]);

        var send = TheUnkeyHappensWhateverGoesWrongTests.Send(
            startSecondsIntoSlot: StartsAt, text: "CQ KC3QIS FN00");

        try
        {
            using (var telemetry = new JsonlTelemetry(folder, "1.12.89", _ => true))
            {
                await new Ft8TransmitSequence(
                        new FakeSerialPort(),
                        new FakeTransmitAudioSink(),
                        guard: null,
                        telemetry: telemetry)
                    .RunAsync(send);
            }

            var files = Directory.GetFiles(folder, "*.jsonl");
            var path = Assert.Single(files);
            var written = File.ReadAllText(path);

            _output.WriteLine($"file : {path}");
            _output.WriteLine($"line : {written.Trim()}");

            // It is there, and it is a transmission.
            Assert.Contains("ft8_transmission", written, StringComparison.Ordinal);
            Assert.Contains("\"transmit\"", written, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("151680", written, StringComparison.Ordinal);

            // And none of it is what was said.
            foreach (var forbidden in
                new[] { "KC3QIS", "FN00", "CQ KC3QIS FN00", send.Transmission.ReadsBackAs })
            {
                Assert.DoesNotContain(forbidden, written, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }

    /// <summary>An <see cref="ITelemetry"/> that keeps what it was given.</summary>
    private sealed class RecordingTelemetry : ITelemetry
    {
        private readonly List<TelemetryEvent> _events = new();

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
                new DateTime(2026, 9, 6, 23, 45, 1, DateTimeKind.Utc),
                "test",
                level,
                "test",
                category,
                eventName,
                data ?? new Dictionary<string, object?>(StringComparer.Ordinal)));
    }
}
