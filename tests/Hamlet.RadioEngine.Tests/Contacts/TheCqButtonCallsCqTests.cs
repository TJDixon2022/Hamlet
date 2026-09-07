using System.Reflection;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// **The CQ button composes from Settings and from nothing on the table**
/// (work instruction 271, task 2; Tim's ruling of 2026-09-07).
/// </summary>
/// <remarks>
/// <para>On 2026-09-07 the operator pressed CQ and the Send area named a station -
/// <c>VP2MAA</c> - which is a reply and not a call to anyone. **A CQ is composed
/// from his own callsign and his own grid, and from nothing that happens to be on
/// the decoded table.**</para>
/// <para>**THE TABLE IS MODELLED AS THE LEDGER, WHICH IS WHAT THE TABLE IS MADE
/// OF.** This lives in the engine's own tests because the whole of the CQ string
/// is <see cref="Ft8SendOptions.CallToAnyone"/>; the view model adds one call and
/// no arithmetic. A busy ledger - several stations, one of them heard calling
/// anyone, one of them mid-exchange with the operator - is built and then the CQ
/// is composed against it, three times, and compared.</para>
/// <para>**AND IT ASKS THE HARDER QUESTION TOO.** A string that begins with
/// <c>CQ </c> and cannot be decoded by anybody is not a CQ; it is a transmission
/// asserting something nobody receives (0.0, HM-DEC-092). So the composed call
/// also goes through the encoder and back through <c>Ft8Sharp</c>'s own decoder.
/// </para>
/// </remarks>
public sealed class TheCqButtonCallsCqTests
{
    private readonly ITestOutputHelper _output;

    public TheCqButtonCallsCqTests(ITestOutputHelper output) => _output = output;

    private const string OperatorCall = "KC3QIS";

    /// <summary>The operator's grid in Settings - six characters, verified.</summary>
    private const string OperatorGrid = "FN00DJ";

    private const int Rate = Ft8Waveform.DefaultSampleRate;

    /// <summary>
    /// **Whatever is on the table, the CQ is the same string and it begins with
    /// `CQ `.**
    /// </summary>
    [Fact]
    public void WhateverIsOnTheTableTheCallToAnyoneIsTheSameStringAndBeginsWithCq()
    {
        var slot = new DateTime(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc);

        // Nothing on the table at all.
        var onAnEmptyTable = Ft8SendOptions.CallToAnyone(OperatorCall, OperatorGrid);

        // One station on the table, the one the Send area named.
        var oneStation = new Ft8ContactLedger(OperatorCall);
        oneStation.RecordHeard("VP2MAA W1ABC FN42", slot);
        var withOneRow = Ft8SendOptions.CallToAnyone(OperatorCall, OperatorGrid);

        // Several stations, one of them calling anyone, one of them mid-exchange
        // with the operator, and one third-party pair.
        var several = new Ft8ContactLedger(OperatorCall);
        several.RecordHeard("VP2MAA W1ABC FN42", slot);
        several.RecordHeard("CQ VP2MAA FK52", slot);
        several.RecordHeard("KC3QIS VP2MAA -12", slot);
        several.RecordHeard("K9TC KJ6IX RRR", slot);
        var withSeveralRows = Ft8SendOptions.CallToAnyone(OperatorCall, OperatorGrid);

        _output.WriteLine($"empty table    : \"{onAnEmptyTable}\"");
        _output.WriteLine($"one row        : \"{withOneRow}\"");
        _output.WriteLine($"several rows   : \"{withSeveralRows}\"");
        _output.WriteLine($"stations booked: {string.Join(", ", several.Stations)}");

        Assert.Equal(onAnEmptyTable, withOneRow);
        Assert.Equal(onAnEmptyTable, withSeveralRows);
        Assert.StartsWith("CQ ", onAnEmptyTable, StringComparison.Ordinal);
        Assert.StartsWith("CQ ", withOneRow, StringComparison.Ordinal);
        Assert.StartsWith("CQ ", withSeveralRows, StringComparison.Ordinal);

        // **AND IT NAMES NOBODY.** The station on the table must not appear in the
        // call, which is the whole of what went wrong at the radio.
        Assert.DoesNotContain("VP2MAA", withSeveralRows, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("W1ABC", withSeveralRows, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("K9TC", withSeveralRows, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("KJ6IX", withSeveralRows, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// **The CQ is not one of the messages the table offers.**
    /// </summary>
    /// <remarks>
    /// The right-click menu for a station is where a reply is composed, and every
    /// option on it names that station. **The call to anyone is not on that menu
    /// and cannot be**, which is the structural version of the same assertion.
    /// </remarks>
    [Fact]
    public void TheCallToAnyoneIsNoneOfTheMessagesTheTableOffersForAStation()
    {
        var slot = new DateTime(2026, 9, 7, 17, 12, 30, DateTimeKind.Utc);
        var ledger = new Ft8ContactLedger(OperatorCall);
        ledger.RecordHeard("CQ VP2MAA FK52", slot);

        var record = ledger.For("VP2MAA");
        Assert.NotNull(record);

        var menu = Ft8SendOptions.For(record!, OperatorCall, OperatorGrid, -12);
        var call = Ft8SendOptions.CallToAnyone(OperatorCall, OperatorGrid);

        foreach (var option in menu.Options)
        {
            _output.WriteLine($"the table offers: \"{option.Text}\"  ({option.Label})");
        }

        _output.WriteLine($"the CQ button   : \"{call}\"");

        Assert.NotEmpty(menu.Options);
        Assert.DoesNotContain(menu.Options, o => string.Equals(o.Text, call, StringComparison.Ordinal));
    }

    /// <summary>
    /// **Nothing on the table can reach the call to anyone, by construction.**
    /// </summary>
    /// <remarks>
    /// Reflection rather than reading: <see cref="Ft8SendOptions.CallToAnyone"/>
    /// takes the operator's own callsign and his own grid and **has nowhere to put
    /// a station record, a decoded message or a collection of either**. That is
    /// what makes *it composes from Settings and from nothing else* a property of
    /// the shape rather than a promise about the body, and it is the guard that
    /// fails if a later unit adds a row parameter for convenience.
    /// </remarks>
    [Fact]
    public void TheCallToAnyoneHasNowhereToPutAnythingOffTheTable()
    {
        var overloads = typeof(Ft8SendOptions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(Ft8SendOptions.CallToAnyone))
            .ToList();

        Assert.Single(overloads);

        var parameters = overloads[0].GetParameters();
        foreach (var p in parameters)
        {
            _output.WriteLine($"parameter: {p.ParameterType.Name} {p.Name}");
        }

        Assert.Equal(2, parameters.Length);
        Assert.All(parameters, p => Assert.Equal(typeof(string), p.ParameterType));
        Assert.Equal("operatorCallsign", parameters[0].Name);
        Assert.Equal("gridSquare", parameters[1].Name);
    }

    /// <summary>
    /// **THE ONE THAT WAS RED. A string beginning `CQ ` that nobody can decode is
    /// not a CQ.**
    /// </summary>
    /// <remarks>
    /// Watched failing first, on the tree of 2026-09-07: with <c>FN00DJ</c> in
    /// Settings the button composed <c>CQ KC3QIS FN00DJ</c>, which packs as a
    /// Standard message whose two fields are the hashes of <c>CQ KC3QIS</c> and
    /// <c>FN00DJ</c> - <c>"&lt;CQ KC3QIS&gt; &lt;FN00DJ&gt;"</c> - and
    /// <c>Ft8SlotDecoder</c> returns nothing at all off that slot.
    /// </remarks>
    [Fact]
    public void TheStringTheCqButtonComposesGoesOnTheAirAndComesBackAsItself()
    {
        var call = Ft8SendOptions.CallToAnyone(OperatorCall, OperatorGrid);
        _output.WriteLine($"the CQ button composes: \"{call}\"");

        var composed = Ft8Composer.Compose(call, Rate);
        Assert.True(
            composed.Composed,
            $"\"{call}\" would not compose at all: {composed.Refusal}. {composed.Explanation}");

        var transmission = composed.Transmission!;
        _output.WriteLine($"the bits say          : \"{transmission.ReadsBackAs}\"");
        _output.WriteLine($"hashed callsign       : {transmission.CarriesHashedCallsign}");

        Assert.False(
            transmission.CarriesHashedCallsign,
            $"\"{call}\" goes out with a callsign on the wire as a hash - the bits say "
            + $"\"{transmission.ReadsBackAs}\" - and only a receiver that heard the full call "
            + "in the same slot could put a name to it.");

        var texts = new Ft8SlotDecoder().Decode(transmission.Samples).Texts;
        _output.WriteLine(
            "the decoder returns   : "
            + (texts.Count == 0 ? "NOTHING" : "\"" + string.Join("\", \"", texts) + "\""));

        Assert.Contains(call, texts, StringComparer.Ordinal);
    }
}
