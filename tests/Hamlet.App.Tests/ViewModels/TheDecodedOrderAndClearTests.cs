using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 241, task 4: the order toggle and the clear.
/// </summary>
/// <remarks>
/// <para>**THE DIRECTION REVERSES SLOTS AND NEVER THE ROWS INSIDE ONE.**
/// Fourteen messages can share one `HHmmss`. They were all on the air at once
/// and the decoder found them in whatever order it searched; the air did not put
/// them in a sequence. Reversing them along with the slots, or sorting them by
/// frequency, would assert an order that did not happen (§0.0).</para>
/// <para>**AND THE SUMMARY NAMES THE NEWEST DECODE WHICHEVER END IT IS AT.** It
/// used to read `DigitalDecodes[^1]`, which was right while there was one
/// possible order and would have named the oldest row the moment this toggle
/// existed.</para>
/// </remarks>
public sealed class TheDecodedOrderAndClearTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the orders are printed.</param>
    public TheDecodedOrderAndClearTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>
    /// The same decodes both ways round give reversed slots with the rows
    /// inside each slot untouched.
    /// </summary>
    [Fact]
    public void TheDirectionReversesSlotsAndNotTheRowsWithinThem()
    {
        var model = Model();

        // Two slots, two messages in each. The second slot is fifteen seconds
        // after the first, which is one FT8 slot exactly.
        Decode(model, 21, 41, 47, ("CQ", "TA3MPK", "KM39", 231f), ("CQ", "W4WTM", "EM74", 2438f));
        Decode(model, 21, 42, 2, ("TA3MPK", "W4WTM", "-11", 917f), ("W4WTM", "TA3MPK", "R-05", 1650f));

        Assert.True(model.DigitalNewestFirst, "the panel does not open newest-first");

        var newestFirst = Snapshot(model);

        _output.WriteLine("newest first:");
        foreach (var line in newestFirst)
        {
            _output.WriteLine("  " + line);
        }

        var summaryNewest = model.DigitalDecodedSummary;

        model.ToggleDigitalOrderCommand.Execute(null);

        var oldestFirst = Snapshot(model);

        _output.WriteLine("oldest first:");
        foreach (var line in oldestFirst)
        {
            _output.WriteLine("  " + line);
        }

        var summaryOldest = model.DigitalDecodedSummary;

        Assert.False(model.DigitalNewestFirst);
        Assert.Equal(4, newestFirst.Count);
        Assert.Equal(4, oldestFirst.Count);

        // **THE SLOTS SWAP ENDS.**
        var newestSlots = newestFirst.Select(Slot).Distinct().ToList();
        var oldestSlots = oldestFirst.Select(Slot).Distinct().ToList();

        Assert.Equal(2, newestSlots.Count);
        Assert.Equal(newestSlots.AsEnumerable().Reverse(), oldestSlots);

        // **AND THE ROWS INSIDE EACH SLOT DO NOT.** Taken slot by slot, the two
        // directions produce exactly the same sequence.
        foreach (var slot in newestSlots)
        {
            var inNewest = newestFirst.Where(l => Slot(l) == slot).ToList();
            var inOldest = oldestFirst.Where(l => Slot(l) == slot).ToList();

            _output.WriteLine("slot " + slot + " newest-first: "
                + string.Join(" | ", inNewest));
            _output.WriteLine("slot " + slot + " oldest-first: "
                + string.Join(" | ", inOldest));

            Assert.Equal(inNewest, inOldest);
        }

        // **THE SUMMARY NAMES THE SAME MESSAGE EITHER WAY.**
        _output.WriteLine("summary newest-first: " + summaryNewest);
        _output.WriteLine("summary oldest-first: " + summaryOldest);

        var newestUtc = summaryNewest.Split(' ')[0];
        var oldestUtc = summaryOldest.Split(' ')[0];

        Assert.Equal(newestUtc, oldestUtc);
        Assert.Equal(Slot(newestFirst[0]), newestUtc);

        // And it says which way round it is, because a collapsed panel still
        // carries its summary (§0.5).
        Assert.Contains("newest first", summaryNewest, StringComparison.Ordinal);
        Assert.Contains("oldest first", summaryOldest, StringComparison.Ordinal);
    }

    /// <summary>Flipping twice returns exactly what was there.</summary>
    /// <remarks>
    /// **THE DISPLAY IS DERIVED FROM ARRIVAL ORDER RATHER THAN REVERSED IN
    /// PLACE**, so this holds by construction. It is asserted because the
    /// in-place version passes the first test and fails this one.
    /// </remarks>
    [Fact]
    public void FlippingTwiceReturnsTheSameRows()
    {
        var model = Model();

        Decode(model, 21, 41, 47, ("CQ", "TA3MPK", "KM39", 231f), ("CQ", "W4WTM", "EM74", 2438f));
        Decode(model, 21, 42, 2, ("TA3MPK", "W4WTM", "-11", 917f));

        var before = Snapshot(model);

        model.ToggleDigitalOrderCommand.Execute(null);
        model.ToggleDigitalOrderCommand.Execute(null);

        var after = Snapshot(model);

        _output.WriteLine("before: " + string.Join(" | ", before));
        _output.WriteLine("after : " + string.Join(" | ", after));

        Assert.Equal(before, after);
        Assert.True(model.DigitalNewestFirst);
    }

    /// <summary>Clearing empties the display and says so.</summary>
    [Fact]
    public void ClearingEmptiesTheDisplayAndThePanelSaysItIsEmpty()
    {
        var model = Model();

        Decode(model, 21, 41, 47, ("CQ", "TA3MPK", "KM39", 231f));

        Assert.NotEmpty(model.DigitalDecodes);
        Assert.True(model.HasDigitalDecodes);

        model.ClearDigitalDecodesCommand.Execute(null);

        _output.WriteLine("after clearing, summary [" + model.DigitalDecodedSummary + "]");
        _output.WriteLine("after clearing, idle    [" + model.DigitalDecodedIdle + "]");

        Assert.Empty(model.DigitalDecodes);
        Assert.False(model.HasDigitalDecodes);

        // **IT SAYS IT IS EMPTY RATHER THAN GOING BLANK** (HM-DEC-021). The idle
        // line is what the panel shows, and it is the same sentence a session
        // that has decoded nothing yet shows - which is correct, because that is
        // now the state.
        Assert.Equal(DigitalIdleText.Decoded, model.DigitalDecodedIdle);
    }

    /// <summary>A new decode after clearing goes in as the newest again.</summary>
    /// <remarks>
    /// **THE KEYS GO WITH THE ROWS.** Clearing that emptied the display but kept
    /// the duplicate-suppression keys would silently swallow the next repeat of
    /// a message, and the panel would look broken for a reason nothing on screen
    /// could explain.
    /// </remarks>
    [Fact]
    public void ADecodeAfterClearingArrivesAgain()
    {
        var model = Model();

        Decode(model, 21, 41, 47, ("CQ", "TA3MPK", "KM39", 231f));
        model.ClearDigitalDecodesCommand.Execute(null);
        Decode(model, 21, 41, 47, ("CQ", "TA3MPK", "KM39", 231f));

        _output.WriteLine("rows after the repeat: " + model.DigitalDecodes.Count);

        Assert.Single(model.DigitalDecodes);
    }

    private static string Slot(string line) => line.Split(' ')[0];

    private static List<string> Snapshot(MainWindowViewModel model)
        => model.DigitalDecodes.Select(r => r.Utc + " " + r.Message).ToList();

    private static MainWindowViewModel Model()
        => new(new AppSettings(), null);

    /// <summary>Decode one slot holding several messages at once.</summary>
    private static void Decode(
        MainWindowViewModel model,
        int hour,
        int minute,
        int second,
        params (string To, string From, string Payload, float Hz)[] messages)
    {
        var samples = new float[Rate * 30];

        foreach (var (to, from, payload, hz) in messages)
        {
            var packed = new byte[Ft8StandardMessage.MessageBytes];

            Assert.Equal(
                Ft8PackResult.Ok,
                Ft8StandardMessage.TryPack(to, from, payload, packed));

            var slot = Ft8Waveform.SynthesizeSlot(
                Ft8SymbolEncoder.Encode(packed), Rate, hz);

            var at = 13 * Rate;

            for (var i = 0; i < slot.Length && at + i < samples.Length; i++)
            {
                samples[at + i] += slot[i];
            }
        }

        model.ShowDecodes(
            new MonoAudio(Rate, samples),
            new DateTime(2026, 9, 4, hour, minute, second, DateTimeKind.Utc),
            new ClockOffset(0, new DateTime(2026, 9, 4, 21, 40, 0, DateTimeKind.Utc)));
    }
}
