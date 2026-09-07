using Ft8Sharp.Dsp;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **How big the class is: how many of the messages the menu can offer tonight
/// would be keyed and read back as nothing** (work instruction 272, task 1).
/// </summary>
/// <remarks>
/// <para>**THIS TASK MEASURES AND CHANGES NOTHING.** Unit 271 repaired the input
/// that made both of the operator's live transmissions unreadable - the
/// six-character grid - and it did not close the class. <c>Ft8Composer</c>
/// computes <c>CarriesHashedCallsign</c> on every transmission it returns, and
/// <c>grep -rn CarriesHashedCallsign src/</c> finds it only inside
/// <c>Ft8Composer.cs</c> itself: **no line of the application reads it.** So any
/// message that fails the two passes carrying every callsign in full still falls
/// through to the hashing pass, still composes, and still arms.</para>
/// <para>**WHAT A HASHED CALLSIGN COSTS.** A hashed callsign goes out as a 22-bit
/// hash rather than as a callsign, and a receiver can put a name to it only if it
/// heard the full call in the same slot. <c>Ft8SlotDecoder.Decode</c> builds its
/// cache per slot and drops it when the call returns, so a slot carrying one
/// hashed message decodes to nothing at all. That is a fact about FT8 rather than
/// about this seam, and it is exactly what happened on a live antenna on
/// 2026-09-07.</para>
/// <para>**THE ROUTE IS THE APPLICATION'S OWN.** The corpus is
/// <see cref="Ft8SendOptions"/>'s - the five shapes the right-click menu offers
/// plus the CQ button's call to anyone - so it is what the operator can actually
/// press, not a list written out again here. The rate is 48000, which is what the
/// operator's own render endpoint declares and what
/// <c>MainWindowViewModel.SendMessage</c> composes at. The reading is
/// <c>Ft8Resample.Resample</c> onto the FT8 grid and then
/// <c>Ft8SlotDecoder</c> - the same two calls <c>Ft8Reception</c> makes on every
/// slot Hamlet hears - because decoding a 48000 Hz array directly would measure
/// something no receiver ever does.</para>
/// <para>**NOTHING HERE OPENS A DEVICE, KEYS ANYTHING OR READS A CLOCK.**</para>
/// </remarks>
public sealed class WhatTheSendPathKnowsAndThrowsAwayTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public WhatTheSendPathKnowsAndThrowsAwayTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The operator, as Settings holds him.</summary>
    private const string OperatorCall = "KC3QIS";

    /// <summary>His grid, six characters, verified, and it stays six in Settings.</summary>
    private const string OperatorGrid = "FN00DJ";

    /// <summary>What the endpoint on his machine declares.</summary>
    private const int EndpointRate = 48_000;

    /// <summary>The report his screen measured for the station that went out.</summary>
    private const int MeasuredReport = -12;

    /// <summary>
    /// **THE NUMBER. Every message the menu can offer, against the callsigns he
    /// could plausibly work tonight - composed, encoded, and read back off
    /// Hamlet's own decoder.**
    /// </summary>
    /// <remarks>
    /// **IT ASSERTS NOTHING ABOUT THE COUNT.** The count is the finding, and a
    /// measurement that fails when the number is inconvenient is not a
    /// measurement. What it does assert is the two things that would make the
    /// table a lie: that the composer's two routes agree about how a message
    /// encoded, and that no message counted as decodable was one that carried a
    /// hashed callsign.
    /// </remarks>
    [Fact]
    public void HowManyOfTonightsMessagesWouldBeKeyedAndReadBackAsNothing()
    {
        var decoder = new Ft8SlotDecoder();
        var rows = new List<Row>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (operatorCall, station, why) in Pairs)
        {
            foreach (var (what, text) in MessagesFor(operatorCall, station))
            {
                if (!seen.Add(text))
                {
                    continue;
                }

                rows.Add(Measure(decoder, rows.Count, why, what, text));
            }
        }

        _output.WriteLine(
            $"{"callsign class",-24} {"shape",-16} {"message",-26} {"composes",-9} "
            + $"{"hashed",-7} {"type",-20} {"bits say",-28} decoder returns");
        _output.WriteLine(new string('-', 170));

        foreach (var row in rows)
        {
            _output.WriteLine(
                $"{row.Why,-24} {row.What,-16} {row.Text,-26} {(row.Composes ? "yes" : "NO"),-9} "
                + $"{(row.Composes ? row.Hashed ? "YES" : "no" : "-"),-7} {row.Type,-20} "
                + $"{row.ReadsBackAs,-28} {row.Heard}");
        }

        var unreadable = rows.Where(r => r.Composes && !r.CameBack).ToList();
        var refused = rows.Where(r => !r.Composes).ToList();

        _output.WriteLine(string.Empty);
        _output.WriteLine($"MESSAGES THE MENU CAN OFFER : {rows.Count}");
        _output.WriteLine($"compose and read back       : {rows.Count(r => r.CameBack)}");
        _output.WriteLine($"refused by the composer     : {refused.Count}");
        _output.WriteLine($"carry a hashed callsign     : {rows.Count(r => r.Hashed)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine(
            $"{unreadable.Count} of {rows.Count} would be keyed today and read back as nothing.");

        foreach (var row in unreadable)
        {
            _output.WriteLine(
                $"  \"{row.Text}\" -> \"{row.ReadsBackAs}\" ({row.Type}, hashed {row.Hashed}), "
                + $"decoder returned {row.Heard}");
        }

        // **THE TWO THINGS THAT WOULD MAKE THE TABLE A LIE**, and nothing about
        // the count itself.
        Assert.All(rows, r => Assert.False(
            r.CameBack && r.Hashed,
            $"\"{r.Text}\" was counted as decodable while carrying a hashed callsign."));

        Assert.All(rows, r => Assert.True(
            r.RoutesAgree,
            $"\"{r.Text}\": ComposeSignal and Compose disagreed about how it encoded."));
    }

    /// <summary>
    /// **What the seam actually consults, read off the composer's own record.**
    /// </summary>
    /// <remarks>
    /// The three facts <c>Ft8Transmission</c> hands back on every composed
    /// transmission - the type, what the bits read back as, and whether a callsign
    /// travelled as a hash - are all public, all set before the record is
    /// returned, and none of them is a parameter of <c>ComposeSignal</c>. This is
    /// the answer to question 4: <c>allowHashing</c> is internal to the compose
    /// pipeline, so there is no one-argument route from the application, but the
    /// **flag is reachable** on the returned transmission and a guard at the
    /// caller can read it.
    /// </remarks>
    [Fact]
    public void TheComposerHandsBackTheFactAndComposeSignalHasNoWayToRefuseIt()
    {
        var went = Ft8Composer.ComposeSignal(
            "VP2MAA KC3QIS FN00DJ", EndpointRate, Ft8Composer.DefaultBaseFrequencyHz);

        Assert.True(went.Composed);

        var transmission = went.Transmission!;

        _output.WriteLine("the string that went out on 2026-09-07:");
        _output.WriteLine($"  Text                 : \"{transmission.Text}\"");
        _output.WriteLine($"  ReadsBackAs          : \"{transmission.ReadsBackAs}\"");
        _output.WriteLine($"  Type                 : {transmission.Type}");
        _output.WriteLine($"  CarriesHashedCallsign: {transmission.CarriesHashedCallsign}");
        _output.WriteLine($"  Text.Length          : {transmission.Text.Length}"
            + "   <- what messageLength counted");
        _output.WriteLine($"  ReadsBackAs.Length   : {transmission.ReadsBackAs.Length}"
            + "   <- what actually went out");

        // **THE COMPOSER SAYS SO AND COMPOSES ANYWAY.** Composed is true, which is
        // the only thing the send path looks at.
        Assert.True(transmission.CarriesHashedCallsign);
        Assert.Equal("<VP2MAA KC3QIS> FN00DJ", transmission.ReadsBackAs);

        // **AND THERE IS NO ARGUMENT THAT WOULD HAVE STOPPED IT.** ComposeSignal
        // takes four parameters and none of them is about hashing.
        var parameters = typeof(Ft8Composer)
            .GetMethod(nameof(Ft8Composer.ComposeSignal))!
            .GetParameters()
            .Select(p => p.Name ?? "")
            .ToArray();

        _output.WriteLine(string.Empty);
        _output.WriteLine("ComposeSignal's parameters: " + string.Join(", ", parameters));

        Assert.Equal(["text", "sampleRate", "baseFrequencyHz", "drivePeak"], parameters);
    }

    /// <summary>
    /// **What the telemetry line counted, and what it would count for a compound
    /// call** - question 6, measured rather than described.
    /// </summary>
    [Fact]
    public void WhatMessageLengthCountedOnTheSeventhOfSeptember()
    {
        foreach (var text in new[]
        {
            "CQ KC3QIS FN00DJ",
            "VP2MAA KC3QIS FN00DJ",
            "VP2M/K1ABC KC3QIS FN00",
            "K1ABC/P KC3QIS FN00",
        })
        {
            var composed = Ft8Composer.ComposeSignal(
                text, EndpointRate, Ft8Composer.DefaultBaseFrequencyHz);

            if (!composed.Composed)
            {
                _output.WriteLine($"\"{text}\" would not compose: {composed.Explanation}");
                continue;
            }

            var t = composed.Transmission!;

            _output.WriteLine(
                $"\"{text}\"" + Environment.NewLine
                + $"    messageType  : {t.Type}" + Environment.NewLine
                + $"    messageLength: {t.Text.Length}   (Text.Length, the composed string)"
                + Environment.NewLine
                + $"    what went out: \"{t.ReadsBackAs}\" ({t.ReadsBackAs.Length} characters), "
                + $"hashed {t.CarriesHashedCallsign}");
        }

        // The line as it stood on 2026-09-07, for the record.
        Assert.Equal(16, "CQ KC3QIS FN00DJ".Length);
    }

    /// <summary>One message, measured both ways and read back.</summary>
    private Row Measure(Ft8SlotDecoder decoder, int index, string why, string what, string text)
    {
        // Spread across the decoder's 200-3000 Hz search window, deterministic.
        var baseHz = 400.0f + (index * 173 % 2000);

        // **THE APPLICATION'S OWN CALL** - what SendMessage:8468 makes.
        var signal = Ft8Composer.ComposeSignal(text, EndpointRate, baseHz);

        if (!signal.Composed)
        {
            return new Row(
                why, what, text, Composes: false, Hashed: false, Type: "-",
                ReadsBackAs: "(refused)", Heard: "(never keyed)", CameBack: false,
                RoutesAgree: true);
        }

        var sent = signal.Transmission!;

        // **THE PADDED SLOT, WHICH IS WHAT A DECODER IS HANDED.** The composer's
        // two routes differ in one call and share every packing decision, so this
        // is the same encoding measured on an array a decoder can read.
        var slot = Ft8Composer.Compose(text, EndpointRate, baseHz);
        var heard = slot.Transmission!;

        var routesAgree = heard.Type == sent.Type
            && heard.CarriesHashedCallsign == sent.CarriesHashedCallsign
            && string.Equals(heard.ReadsBackAs, sent.ReadsBackAs, StringComparison.Ordinal);

        // **WHAT ANY RECEIVER DOES**: onto the FT8 grid, then decode.
        var atDecoderRate = Ft8Resample.Resample(
            heard.Samples, EndpointRate, Ft8Resample.TargetSampleRate);

        var texts = decoder.Decode(atDecoderRate).Texts;

        return new Row(
            why,
            what,
            text,
            Composes: true,
            Hashed: sent.CarriesHashedCallsign,
            Type: sent.Type.ToString(),
            ReadsBackAs: sent.ReadsBackAs,
            Heard: texts.Count == 0 ? "NOTHING" : "\"" + string.Join("\", \"", texts) + "\"",
            CameBack: texts.Contains(text, StringComparer.Ordinal),
            RoutesAgree: routesAgree);
    }

    /// <summary>
    /// The CQ button's message and the five the right-click menu offers, taken
    /// from <see cref="Ft8SendOptions"/> rather than written out again.
    /// </summary>
    private IReadOnlyList<(string What, string Text)> MessagesFor(
        string operatorCall, string station)
    {
        var messages = new List<(string, string)>
        {
            ("CQ button", Ft8SendOptions.CallToAnyone(operatorCall, OperatorGrid)),
        };

        var ledger = new Ft8ContactLedger(operatorCall);
        ledger.RecordHeard(
            "CQ " + station + " FK52", new DateTime(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc));

        var record = ledger.For(station);

        if (record is null)
        {
            _output.WriteLine(
                $"NOTE: the ledger booked no record for \"{station}\" off its own CQ, so the "
                + "right-click menu has nothing to offer that station and only the CQ button "
                + "is measured for it.");
            return messages;
        }

        var menu = Ft8SendOptions.For(record, operatorCall, OperatorGrid, MeasuredReport);

        foreach (var option in menu.Options)
        {
            messages.Add((option.Shape.ToString(), option.Text));
        }

        return messages;
    }

    /// <summary>
    /// The callsigns he could plausibly work tonight, and his own two portable
    /// forms.
    /// </summary>
    /// <remarks>
    /// **NONE OF THESE IS INVENTED TO FAIL.** A compound prefix and a portable
    /// suffix are what a DX station answering a CQ on 14.074 routinely signs, and
    /// the operator's own two portable forms are what he would set in Settings
    /// operating away from home.
    /// </remarks>
    private static IReadOnlyList<(string Operator, string Station, string Why)> Pairs =>
    [
        (OperatorCall, "K1ABC", "a plain call"),
        (OperatorCall, "VP2MAA", "the one from 2026-09-07"),
        (OperatorCall, "VP2M/K1ABC", "a compound prefix"),
        (OperatorCall, "K1ABC/P", "a portable suffix"),
        (OperatorCall, "PJ4G", "a short call"),
        (OperatorCall, "SV9/PA3EXX", "a long call"),
        ("KC3QIS/P", "K1ABC", "the operator portable"),
        ("W4/KC3QIS", "K1ABC", "the operator compound"),
    ];

    /// <summary>One line of the table.</summary>
    private sealed record Row(
        string Why,
        string What,
        string Text,
        bool Composes,
        bool Hashed,
        string Type,
        string ReadsBackAs,
        string Heard,
        bool CameBack,
        bool RoutesAgree);
}
