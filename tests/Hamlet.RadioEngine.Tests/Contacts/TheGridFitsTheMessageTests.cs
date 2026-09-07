using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **A standard FT8 message carries a four-character grid, and every message the
/// send path composes comes back off its own decoder as itself** (work
/// instruction 271, task 3).
/// </summary>
/// <remarks>
/// <para>The operator's grid in Settings is <c>FN00DJ</c>, six characters, marked
/// verified from callook.info. Every station on his screen sends four - `FL20`,
/// `JN86`, `EL29`, `EM16`, `EK57`, `JN03`. On 2026-09-07 the six went into a
/// message on a live antenna and **nobody could decode what came out**: the
/// standard packing reads back with the grid truncated, so the round-trip guard
/// refused it, and the words fell through to the pass that puts a callsign on the
/// wire as a hash.</para>
/// <para>**THE SIX CHARACTERS STAY IN SETTINGS.** They are what distance and
/// bearing are computed from and nothing here touches them. What is asserted is
/// that **wherever a grid enters a transmitted message it is the first four
/// characters** - one place, <see cref="Ft8SendOptions"/>, which is where every
/// message the operator can send is composed.</para>
/// <para>**THE ROUND TRIP IS THE TEST, AND IT IS THE ONE THAT WOULD HAVE CAUGHT
/// THIS.** Compose, encode, decode with <c>Ft8Sharp</c>'s own
/// <c>Ft8SlotDecoder</c> - the same call the receive path ends in - and get back
/// the same text. A message that will not survive that is a transmission
/// asserting something nobody receives (0.0, HM-DEC-092).</para>
/// </remarks>
public sealed class TheGridFitsTheMessageTests
{
    private readonly ITestOutputHelper _output;

    public TheGridFitsTheMessageTests(ITestOutputHelper output) => _output = output;

    private const string OperatorCall = "KC3QIS";

    /// <summary>What Settings holds - six characters, verified, and it stays.</summary>
    private const string SixCharacterGrid = "FN00DJ";

    /// <summary>What a standard FT8 message can carry.</summary>
    private const string TheFourItFits = "FN00";

    private const string TheOtherStation = "VP2MAA";

    private const int Rate = Ft8Waveform.DefaultSampleRate;

    /// <summary>
    /// **A six-character grid in Settings produces a four-character grid in the
    /// message** - in the call to anyone and in the grid reply alike.
    /// </summary>
    [Fact]
    public void ASixCharacterGridInSettingsProducesAFourCharacterGridInTheMessage()
    {
        var call = Ft8SendOptions.CallToAnyone(OperatorCall, SixCharacterGrid);

        var record = LedgerHolding("CQ VP2MAA FK52");
        var menu = Ft8SendOptions.For(record, OperatorCall, SixCharacterGrid, -12);
        var reply = menu.Options.Single(o => o.Shape == Ft8SendShape.Grid);

        _output.WriteLine($"Settings holds : \"{SixCharacterGrid}\"");
        _output.WriteLine($"the CQ         : \"{call}\"");
        _output.WriteLine($"the grid reply : \"{reply.Text}\"");

        Assert.Equal("CQ " + OperatorCall + " " + TheFourItFits, call);
        Assert.Equal(TheOtherStation + " " + OperatorCall + " " + TheFourItFits, reply.Text);

        // **AND THE SIX ARE NOT IN EITHER OF THEM.**
        Assert.DoesNotContain(SixCharacterGrid, call, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(SixCharacterGrid, reply.Text, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **A grid already four characters is passed through untouched**, and a
    /// shorter one is not padded, completed or invented (0.0).
    /// </summary>
    [Fact]
    public void AGridThatAlreadyFitsIsUntouchedAndNothingIsInvented()
    {
        Assert.Equal("CQ KC3QIS FN42", Ft8SendOptions.CallToAnyone(OperatorCall, "FN42"));
        Assert.Equal("CQ KC3QIS fn42".ToUpperInvariant(), Ft8SendOptions.CallToAnyone(OperatorCall, "  FN42 "));

        // No grid in Settings is still a legal call to anyone, and no locator is
        // made up to fill the gap.
        Assert.Equal("CQ KC3QIS", Ft8SendOptions.CallToAnyone(OperatorCall, null));
        Assert.Equal("CQ KC3QIS", Ft8SendOptions.CallToAnyone(OperatorCall, "   "));
    }

    /// <summary>
    /// **THE ONE. Every message the send path composes goes out as audio and comes
    /// back off Hamlet's own decoder as itself** - a CQ, a grid reply, a report, an
    /// R- report, RRR, RR73 and 73.
    /// </summary>
    /// <remarks>
    /// The corpus is taken from <see cref="Ft8SendOptions"/> rather than written
    /// out again, so it is what the operator can actually press. <c>RR73</c> is
    /// added as a literal because it is named in the criterion and is not one of
    /// the five shapes the menu offers - reported, not repaired.
    /// </remarks>
    [Fact]
    public void EveryMessageTheSendPathComposesComesBackFromItsOwnDecoderAsItself()
    {
        var record = LedgerHolding("KC3QIS VP2MAA FK52");
        var menu = Ft8SendOptions.For(record, OperatorCall, SixCharacterGrid, -12);

        var corpus = new List<(string What, string Text)>
        {
            ("a CQ", Ft8SendOptions.CallToAnyone(OperatorCall, SixCharacterGrid)),
        };

        foreach (var option in menu.Options)
        {
            corpus.Add((option.Label, option.Text));
        }

        corpus.Add(("RR73", TheOtherStation + " " + OperatorCall + " RR73"));

        var decoder = new Ft8SlotDecoder();
        var failures = new List<string>();

        for (var i = 0; i < corpus.Count; i++)
        {
            var (what, text) = corpus[i];

            // Spread across the decoder's own search window so this is not one
            // frequency measured seven times. Deterministic.
            var baseHz = 400.0f + (i * 211 % 1800);

            var composed = Ft8Composer.Compose(text, Rate, baseHz);
            if (!composed.Composed)
            {
                failures.Add($"[{what}] \"{text}\" would not compose: {composed.Refusal}. {composed.Explanation}");
                _output.WriteLine($"{what,-18} \"{text}\"  -> WOULD NOT COMPOSE");
                continue;
            }

            var transmission = composed.Transmission!;
            var texts = decoder.Decode(transmission.Samples).Texts;
            var cameBack = texts.Contains(text, StringComparer.Ordinal);

            _output.WriteLine(
                $"{what,-18} \"{text}\"  -> {transmission.Type}, bits say "
                + $"\"{transmission.ReadsBackAs}\", hashed {transmission.CarriesHashedCallsign}, "
                + $"decoder returned "
                + (texts.Count == 0 ? "NOTHING" : "\"" + string.Join("\", \"", texts) + "\""));

            if (!cameBack)
            {
                failures.Add(
                    $"[{what}] \"{text}\" went out as \"{transmission.ReadsBackAs}\" at {baseHz:F0} Hz "
                    + $"and the decoder returned "
                    + (texts.Count == 0 ? "nothing" : "\"" + string.Join("\", \"", texts) + "\""));
            }

            Assert.False(
                transmission.CarriesHashedCallsign,
                $"[{what}] \"{text}\" puts a callsign on the wire as a hash.");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"MESSAGES TRIED    : {corpus.Count}");
        _output.WriteLine($"ROUND-TRIPPED     : {corpus.Count - failures.Count}");

        Assert.True(
            failures.Count == 0,
            "these did not come back as themselves:" + Environment.NewLine
            + string.Join(Environment.NewLine, failures));
    }

    /// <summary>A ledger holding one message, and the record it booked.</summary>
    private static Ft8StationRecord LedgerHolding(string message)
    {
        var ledger = new Ft8ContactLedger(OperatorCall);
        ledger.RecordHeard(message, new DateTime(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc));

        var record = ledger.For(TheOtherStation);
        Assert.NotNull(record);
        return record!;
    }
}
