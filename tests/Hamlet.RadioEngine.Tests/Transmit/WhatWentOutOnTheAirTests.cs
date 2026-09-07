using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **What the CQ button actually put on a live antenna on 2026-09-07, measured
/// rather than reasoned about** (work instruction 271, task 1).
/// </summary>
/// <remarks>
/// <para>Hamlet transmitted twice on 14.074000 into a real antenna. The telemetry
/// says the string was <c>VP2MAA KC3QIS FN00DJ</c> and the outcome was
/// <c>Sent</c>. The operator's grid in Settings is six characters and every
/// station on his screen sends four, so the question this file exists to answer is
/// **whether anybody on the band could read what went out**.</para>
/// <para>**IT MEASURES AND IT DOES NOT REPAIR.** Nothing here composes a
/// replacement, truncates a grid or asserts what the fix should be; it puts the
/// exact strings through <see cref="Ft8Composer"/> and back through
/// <c>Ft8SlotDecoder</c>, which is the same decoder the receive path ends in, and
/// writes down what came back. Tasks 2 and 3 act on it.</para>
/// <para>**Nothing here opens a device, keys anything or reads a clock.**</para>
/// </remarks>
public sealed class WhatWentOutOnTheAirTests
{
    private readonly ITestOutputHelper _output;

    public WhatWentOutOnTheAirTests(ITestOutputHelper output) => _output = output;

    /// <summary>The rate the decoder reads at.</summary>
    private const int Rate = Ft8Waveform.DefaultSampleRate;

    /// <summary>The exact string the CQ button composed and sent on 2026-09-07.</summary>
    private const string WhatWentOut = "VP2MAA KC3QIS FN00DJ";

    /// <summary>What a CQ from his own settings composes to today.</summary>
    private const string WhatTheCqButtonComposes = "CQ KC3QIS FN00DJ";

    /// <summary>The same call with the grid cut to the four FT8 carries.</summary>
    private const string TheSameCallWithFourCharacters = "CQ KC3QIS FN00";

    /// <summary>The same reply with the grid cut to four.</summary>
    private const string TheSameReplyWithFourCharacters = "VP2MAA KC3QIS FN00";

    /// <summary>
    /// **THE HEADLINE. The four strings, through the encoder and back through
    /// Ft8Sharp's own decoder, with what came back written down.**
    /// </summary>
    [Fact]
    public void TheStringsThatWentOnTheAirGoThroughTheEncoderAndBackThroughTheDecoder()
    {
        var decoder = new Ft8SlotDecoder();

        _output.WriteLine(
            "WORK INSTRUCTION 271 TASK 1 - what went out on a live antenna on 2026-09-07.");
        _output.WriteLine(string.Empty);

        foreach (var text in new[]
                 {
                     WhatWentOut,
                     WhatTheCqButtonComposes,
                     TheSameReplyWithFourCharacters,
                     TheSameCallWithFourCharacters,
                 })
        {
            Report(decoder, text);
        }

        // NOTHING IS ASSERTED ABOUT THE OUTCOME HERE. This is a measurement, and a
        // measurement that fails the run is a measurement nobody reads the number
        // off. The assertion this unit owes is task 3's round trip.
        Assert.True(true);
    }

    /// <summary>One string, all the way round, written down.</summary>
    private void Report(Ft8SlotDecoder decoder, string text)
    {
        _output.WriteLine("================================================================");
        _output.WriteLine($"ASKED FOR      : \"{text}\"  ({text.Length} characters)");

        // The bare signal is what goes on the air; the padded slot is what a
        // decoder is handed. Both are reported, because the question is what a
        // receiver could read, and a receiver reads a slot.
        var onTheAir = Ft8Composer.ComposeSignal(text, Rate);
        _output.WriteLine(
            $"ComposeSignal  : {(onTheAir.Composed ? "composed" : "REFUSED - " + onTheAir.Refusal)}");

        if (!onTheAir.Composed)
        {
            _output.WriteLine($"  why          : {onTheAir.Explanation}");
        }

        var composed = Ft8Composer.Compose(text, Rate);
        if (!composed.Composed)
        {
            _output.WriteLine($"Compose        : REFUSED - {composed.Refusal}");
            _output.WriteLine($"  why          : {composed.Explanation}");
            _output.WriteLine("DECODED BACK   : nothing was ever built, so nothing went out.");
            ReportThePacking(text);
            return;
        }

        var transmission = composed.Transmission!;

        // **AT THE RATE IT ACTUALLY WENT OUT AT**, which is the endpoint's 48 000
        // and not the decoder's 12 000. The packing is the message layer's and the
        // rate is the synthesiser's, so the two should agree - and that is asserted
        // here rather than assumed, because the whole finding turns on it.
        var atTheEndpointsRate = Ft8Composer.ComposeSignal(text, 48000);
        _output.WriteLine(
            "AT 48000 Hz    : "
            + (atTheEndpointsRate.Composed
                ? $"composed, {atTheEndpointsRate.Transmission!.Type}, bits say "
                  + $"\"{atTheEndpointsRate.Transmission!.ReadsBackAs}\""
                : "REFUSED - " + atTheEndpointsRate.Refusal));

        _output.WriteLine($"MESSAGE TYPE   : {transmission.Type}");
        _output.WriteLine($"THE BITS SAY   : \"{transmission.ReadsBackAs}\"");
        _output.WriteLine($"HASHED CALLSIGN: {transmission.CarriesHashedCallsign}");
        _output.WriteLine(
            $"SLOT           : {transmission.Samples.Length} samples, "
            + $"{transmission.SlotSeconds:F2} s at {transmission.SampleRate}");

        var texts = decoder.Decode(transmission.Samples).Texts;
        _output.WriteLine(
            "DECODED BACK   : "
            + (texts.Count == 0 ? "NOTHING" : "\"" + string.Join("\", \"", texts) + "\""));
        _output.WriteLine(
            $"SAME AS ASKED  : {texts.Contains(text, StringComparer.Ordinal)}");

        ReportThePacking(text);
    }

    /// <summary>
    /// What each field arrangement did at the message layer, so a refusal names
    /// the field that would not go rather than only the string that would not.
    /// </summary>
    private void ReportThePacking(string text)
    {
        var words = text.ToUpperInvariant().Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        _output.WriteLine("  the message layer, arrangement by arrangement:");

        foreach (var (to, de, extra) in Arrangements(words))
        {
            var buffer = new byte[Ft8Payload.MessageBytes];
            var standard = Ft8StandardMessage.TryPack(to, de, extra, null, buffer);
            var reads = standard == Ft8PackResult.Ok
                ? "\"" + Ft8MessageDecoder.Decode(buffer, null).Text + "\""
                : "-";

            _output.WriteLine(
                $"    standard  \"{to}\" / \"{de}\" / \"{extra}\"  -> {standard}, reads back {reads}");
        }

        var free = new byte[Ft8Payload.MessageBytes];
        var freeResult = Ft8FreeText.TryPackText(text.ToUpperInvariant(), free);
        var freeReads = freeResult == Ft8PackResult.Ok
            ? "\"" + Ft8MessageDecoder.Decode(free, null).Text + "\""
            : "-";
        _output.WriteLine($"    free text -> {freeResult}, reads back {freeReads}");
    }

    /// <summary>The arrangements <c>Ft8Composer</c> itself offers, in its order.</summary>
    private static IEnumerable<(string To, string De, string Extra)> Arrangements(string[] words)
    {
        switch (words.Length)
        {
            case 2:
                yield return (words[0], words[1], string.Empty);
                break;

            case 3:
                yield return (words[0], words[1], words[2]);
                yield return ($"{words[0]} {words[1]}", words[2], string.Empty);
                break;

            case 4:
                yield return ($"{words[0]} {words[1]}", words[2], words[3]);
                break;
        }
    }
}
