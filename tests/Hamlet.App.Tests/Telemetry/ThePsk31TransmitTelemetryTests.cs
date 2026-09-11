using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Work instruction 322 task 3: **the PSK31 transmit path writes its record.**
/// </summary>
/// <remarks>
/// <para>**READ-ONLY ON BEHAVIOUR** (§0.2, and §6 of the plan). Nothing here or in the
/// events it exercises keys, arms, composes or plays anything the application would not
/// have done anyway.</para>
/// <para>**AND FOUR OF THE SIX EVENTS HAVE NO PRODUCTION CALL SITE YET, WHICH IS THE
/// FINDING** rather than an omission. The step-4 press half is `blocked` on two rulings
/// the owner has not given: `CanTransmitIn` still answers false for PSK31, and nothing in
/// `src/Hamlet.App` reaches `UnslottedTransmission` at all. **So composing, keying,
/// unkeying and the record are proved at the bench against the real composer**, ready for
/// the unit that builds the press, and **the one that does have a call site - the
/// refusal - is asserted through the running application.**</para>
/// <para>**COMPUTED, NOT SEEN**, and no radio was involved: assertion 4 is that absence
/// written down (FACT-006).</para>
/// </remarks>
public sealed class ThePsk31TransmitTelemetryTests : IDisposable
{
    private const string HisCall = "KC3QIS";
    private const int Rate = 8000;

    private readonly ITestOutputHelper _output;
    private readonly string _folder;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the events are printed.</param>
    public ThePsk31TransmitTelemetryTests(ITestOutputHelper output)
    {
        _output = output;

        _folder = Path.Combine(
            Path.GetTempPath(), "hamlet-psk31-tx-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_folder);
    }

    /// <summary>Removes the telemetry folder.</summary>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // A left-over temp folder is not a test failure.
        }
    }

    /// <summary>**Each of the four macros writes what it composed.**</summary>
    /// <remarks>
    /// **THE SECONDS ARE THE MODULATOR'S OWN**, taken off the composed samples rather
    /// than worked out a second time here, so the record cannot disagree with what would
    /// go on the air.
    /// </remarks>
    [Fact]
    public void EachOfTheFourMacrosWritesWhatItComposed()
    {
        var macros = new (string Kind, string Text)[]
        {
            ("cq", Psk31Macros.Cq(HisCall)),
            ("answer", Psk31Macros.Answer("W1AW", HisCall)),
            ("report", Psk31Macros.Report("W1AW", HisCall, "Tim", "Trafford PA", "FN00")),
            ("confirm", Psk31Macros.Confirm("W1AW", HisCall)),
        };

        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            foreach (var (kind, text) in macros)
            {
                var composed = Psk31Modulator.Compose(text, Rate, 1000, 0.5f);

                Psk31Events.SendComposed(
                    telemetry,
                    kind,
                    text.Length,
                    composed.Seconds,
                    OperatorSend.LongestUnslottedSeconds,
                    1000);
            }
        }

        lines = Read();

        var written = Events(lines, "psk31_send_composed");

        foreach (var one in written)
        {
            _output.WriteLine(Text(one, "macro").PadRight(8)
                + Number(one, "characters") + " characters, "
                + Number(one, "seconds").ToString("0.00", CultureInfo.InvariantCulture)
                + " s, cap "
                + Number(one, "capSeconds").ToString("0", CultureInfo.InvariantCulture)
                + ", within "
                + one.GetProperty("data").GetProperty("withinCap").GetBoolean());
        }

        Assert.Equal(4, written.Count);

        // **UNIT 317'S TABLE IS THE TEXT, AND THE EVENT IS THE TRANSMISSION.** The
        // instruction asks for the composed seconds to be within half a second of 9.9,
        // 6.3, 23.2 and 14.7. **Measured, every one is longer by 1.54 s** - and the
        // difference is not drift, it is the idle the modulator puts either side:
        // `IdleBitsBefore` is 32 and `IdleBitsAfter` is 16, which at 31.25 baud is
        // **1.536 s exactly.** The table counted the text; the composer composes what
        // the transmitter would key, and **the keyed length is the honest number for an
        // event about a transmission** - it is the one the cap is measured against.
        // **Reported as a mismatch**, and asserted both ways so neither reading can
        // drift unnoticed.
        var idle = (Psk31Modulator.IdleBitsBefore + Psk31Modulator.IdleBitsAfter)
            / Psk31Demodulator.Baud;

        _output.WriteLine("idle either side: "
            + idle.ToString("0.000", CultureInfo.InvariantCulture) + " s");

        foreach (var (kind, textSeconds) in new (string, double)[]
        {
            ("cq", 9.9), ("answer", 6.3), ("report", 23.2), ("confirm", 14.7),
        })
        {
            var one = written.Single(e => Text(e, "macro") == kind);

            var got = Number(one, "seconds");

            // **THE TEXT ALONE MATCHES THE TABLE.**
            Assert.True(
                Math.Abs(got - idle - textSeconds) <= 0.5,
                kind + " carries " + (got - idle).ToString(
                    "0.00", CultureInfo.InvariantCulture)
                + " s of text where the table says " + textSeconds);

            // **AND THE EVENT REPORTS THE WHOLE KEYED LENGTH**, which is longer.
            Assert.True(
                got > textSeconds,
                kind + " reported " + got.ToString("0.00", CultureInfo.InvariantCulture)
                + " s, which does not include the idle the modulator adds");
        }
    }

    /// <summary>**A macro over the cap is refused, and nothing stages.**</summary>
    [Fact]
    public void AMacroOverTheCapIsRefusedAndNothingStages()
    {
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            // **LONGER THAN THE CAP BY ARITHMETIC, NOT BY GUESS.** At 31.25 baud and
            // about 7.8 bits a character, thirty seconds is a little over 120
            // characters; this is comfortably past it.
            var text = string.Join(" ", Enumerable.Repeat("CQ CQ CQ DE " + HisCall, 20));

            var composed = Psk31Modulator.Compose(text, Rate, 1000, 0.5f);

            _output.WriteLine("composed " + text.Length + " characters, "
                + composed.Seconds.ToString("0.0", CultureInfo.InvariantCulture)
                + " s against a cap of " + OperatorSend.LongestUnslottedSeconds);

            Assert.True(composed.Seconds > OperatorSend.LongestUnslottedSeconds);

            Psk31Events.SendRefused(telemetry, "cap", "cq", "composed");
        }

        lines = Read();

        var refused = Events(lines, "psk31_send_refused");

        var one = Assert.Single(refused);

        Assert.Equal("cap", Text(one, "reason"));

        // **AND NOTHING WENT FURTHER.**
        Assert.Empty(Events(lines, "send_stage"));
        Assert.Empty(Events(lines, "psk31_send_keyed"));
    }

    /// <summary>**A PSK31 press is refused, and nothing reaches the transmitter.**</summary>
    /// <remarks>
    /// **THIS IS THE ONE WITH A PRODUCTION CALL SITE**, so it is driven through the
    /// running application rather than at the bench. `CanTransmitIn` answers false for
    /// PSK31 - the step-4 press half is blocked on the owner's rulings - and the refusal
    /// sits at the one door that composes a signal in the whole of `src/`.
    /// </remarks>
    [Fact]
    public void APsk31PressIsRefusedAndNothingReachesTheTransmitter()
    {
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            var model = Panel(telemetry);

            model.ChooseDigitalModeCommand.Execute("PSK31");

            model.SendCallToAnyoneCommand.Execute(null);

            _output.WriteLine("send line: " + model.DigitalSendLine);
        }

        lines = Read();

        var refused = Events(lines, "psk31_send_refused");

        foreach (var one in refused)
        {
            _output.WriteLine("refused: " + one.GetProperty("data").GetRawText());
        }

        var it = Assert.Single(refused);

        Assert.Equal("mode", Text(it, "reason"));

        // **NOTHING STAGED, NOTHING KEYED, NO RECORD.**
        Assert.Empty(Events(lines, "send_stage"));
        Assert.Empty(Events(lines, "psk31_send_keyed"));
        Assert.Empty(Events(lines, "ft8_transmission"));
    }

    /// <summary>**With no radio, the record says the radio did not answer.**</summary>
    /// <remarks>
    /// <para>**THIS IS ASK 1 TURNED INTO A MEASUREMENT.** `Played` says what the audio
    /// path did; this says what the **radio** did, from the two commands Hamlet already
    /// polls. On this machine there is no radio, so every field is absent and `answered`
    /// is false.</para>
    /// <para>**AND THAT ABSENCE IS THE FINDING**, written as an event rather than left
    /// as a silence somebody has to infer from a missing line (§0.0).</para>
    /// </remarks>
    [Fact]
    public void WithNoRadioTheRecordSaysTheRadioDidNotAnswer()
    {
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            Psk31Events.RadioAfterSend(telemetry, null, null, null, null);
        }

        lines = Read();

        var one = Assert.Single(Events(lines, "psk31_radio_after_send"));

        _output.WriteLine(one.GetProperty("data").GetRawText());

        Assert.False(one.GetProperty("data").GetProperty("answered").GetBoolean());

        // **ABSENT, NOT ZERO** (§0.0). A zero would read as *the radio said nought per
        // cent*, which is a measurement nobody made.
        foreach (var field in new[]
        {
            "transmitting", "transmittingAgeMs", "powerPercent", "powerAgeMs",
        })
        {
            Assert.Equal(
                JsonValueKind.Null,
                one.GetProperty("data").GetProperty(field).ValueKind);
        }
    }

    /// <summary>**Nothing personal reaches any of it.**</summary>
    [Fact]
    public void NothingPersonalReachesAnyOfIt()
    {
        List<string> lines;

        using (var telemetry = new JsonlTelemetry(_folder, "322", _ => true))
        {
            foreach (var (kind, text) in new (string, string)[]
            {
                ("cq", Psk31Macros.Cq(HisCall)),
                ("answer", Psk31Macros.Answer("W1AW", HisCall)),
                ("report", Psk31Macros.Report("W1AW", HisCall, "Tim", "Trafford PA", "FN00")),
                ("confirm", Psk31Macros.Confirm("W1AW", HisCall)),
            })
            {
                var composed = Psk31Modulator.Compose(text, Rate, 1000, 0.5f);

                Psk31Events.SendComposed(
                    telemetry, kind, text.Length, composed.Seconds,
                    OperatorSend.LongestUnslottedSeconds, 1000);
            }

            Psk31Events.SendRefused(telemetry, "cap", "report", "composed");
            Psk31Events.SendKeying(telemetry, keyed: true, 0, 9.9, aborted: false);
            Psk31Events.SendKeying(telemetry, keyed: false, 9.9, 9.9, aborted: false);
            Psk31Events.RadioAfterSend(telemetry, true, 240, 48.5, 240);
        }

        lines = Read();

        var mine = lines.Where(l => l.Contains("psk31_send", StringComparison.Ordinal)
            || l.Contains("psk31_radio", StringComparison.Ordinal)).ToList();

        _output.WriteLine("transmit events scanned: " + mine.Count);

        Assert.NotEmpty(mine);

        // **THE MACROS CARRY HIS CALLSIGN, HIS NAME, HIS TOWN AND HIS GRID.** None of
        // them may reach the file (HM-DEC-018, §2.1).
        foreach (var personal in new[]
        {
            HisCall, "W1AW", "Tim", "Trafford", "FN00",
        })
        {
            foreach (var line in mine)
            {
                Assert.DoesNotContain(personal, line, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private MainWindowViewModel Panel(JsonlTelemetry telemetry)
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, telemetry)
        {
            OperatingMode = "Digital",
            TapForTests = new AudioTap(),
            ClockOffset = new ClockOffset(0.033, DateTime.UtcNow),
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private List<string> Read()
        => Directory.GetFiles(_folder, "*.jsonl")
            .SelectMany(File.ReadAllLines)
            .ToList();

    private static List<JsonElement> Events(IEnumerable<string> lines, string name)
        => lines
            .Where(l => l.Contains("\"" + name + "\"", StringComparison.Ordinal))
            .Select(l => JsonDocument.Parse(l).RootElement)
            .Where(e => e.GetProperty("event").GetString() == name)
            .ToList();

    private static double Number(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.Number
                ? value.GetDouble()
                : 0;

    private static string Text(JsonElement e, string field)
        => e.GetProperty("data").TryGetProperty(field, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? ""
                : "";
}
