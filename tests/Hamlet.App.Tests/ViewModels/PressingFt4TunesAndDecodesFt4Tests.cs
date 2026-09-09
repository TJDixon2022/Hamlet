using System;
using System.Collections.Generic;
using System.Linq;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 292 - <b>step 4, criterion 1.</b> Pressing FT4 cuts the band into
/// 7.5-second slots and reads FT4 off it, through the same path the FT8 button uses.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS CATCHES.** The FT4 button tuning the radio correctly and
/// then decoding nothing for the rest of the evening, with no sentence anywhere saying
/// why. That is what the tab did at HEAD `9449d02`, and
/// <see cref="TodayAnFt4PressStillCutsFifteenAndReadsWithTheFt8Decoder"/> is the
/// measurement of it rather than an assertion about it - the recording below holds four
/// real FT4 transmissions and the tab returned nought.</para>
/// <para>**IT DRIVES THE APPLICATION'S PATH AND NOT `Ft4SlotDecoder`.** Unit 289 already
/// proved the library round trip over 106 messages; a test that called the decoder
/// directly would prove that again and this unit's threading not at all. What is
/// unproved here is audio in at the tap, cut on a 7.5 s grid, read by the reader,
/// arriving as a row - so every assertion below goes through
/// <see cref="MainWindowViewModel.ShowDecodes"/>.</para>
/// <para>**AND FT8 IS THE CONTROL.** The same recording, the same call, with FT8 chosen,
/// is asserted to behave exactly as it did before this unit: a 15 s grid, the Deep
/// decoder, and nothing read out of FT4 tones.</para>
/// </remarks>
public sealed class PressingFt4TunesAndDecodesFt4Tests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the grid, the decoder and the rows are printed.</param>
    public PressingFt4TunesAndDecodesFt4Tests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The four messages the recording carries, one per FT4 slot.</summary>
    /// <remarks>
    /// **FOUR DIFFERENT MESSAGES AT FOUR DIFFERENT PLACES IN THE PASSBAND**, so a pass
    /// is four independent agreements rather than one repeated. Every frequency is an
    /// exact multiple of FT4's 20.8333 Hz tone spacing, which is where unit 289 measured
    /// the round trip; off-grid placement is a different measurement and this unit did
    /// not take it.
    /// </remarks>
    private static readonly (string To, string De, string Extra, float Hz)[] Corpus =
    {
        ("CQ", "K1ABC", "FN42", 1000.0f),
        ("K1ABC", "W9XYZ", "EM12", 1250.0f),
        ("W9XYZ", "K1ABC", "-11", 1500.0f),
        ("K1ABC", "W9XYZ", "RR73", 1750.0f),
    };

    /// <summary>
    /// The starting position, measured against today's code: FT4 chosen, and the tab
    /// still cuts fifteen and still hands the slot to the FT8 reader.
    /// </summary>
    /// <remarks>
    /// **THIS IS TASK 1'S TEST AND IT IS DELIBERATELY AN ASSERTION OF THE FAULT.** It is
    /// rewritten by tasks 2 and 3 into
    /// <see cref="AnFt4PressCutsSevenAndAHalfAndReadsFt4"/>; what it records is that the
    /// two facts it names were measured rather than assumed before anything was threaded.
    /// </remarks>
    [Fact]
    public void TodayAnFt4PressStillCutsFifteenAndReadsWithTheFt8Decoder()
    {
        var panel = Panel("FT4");

        // **THE CHOICE IS RECORDED AND THE GRID DOES NOT FOLLOW IT.** `_digitalGrid` is
        // `SlotGrid.Ft8` and `UseGridForTests` is its only writer
        // (`MainWindowViewModel.cs:1661`, `:1684`).
        Assert.Equal("FT4", panel.ChosenDigitalMode);
        Assert.Equal(SlotGrid.Ft8, panel.DigitalGrid);

        var heard = Read(panel);

        _output.WriteLine("  grid     : " + panel.DigitalGrid.Describe());
        _output.WriteLine("  slots    : " + heard.SlotsDecoded);
        _output.WriteLine("  decoder  : " + Decoder(heard));
        _output.WriteLine("  messages : " + heard.Decodes.Count);

        // Two whole FT8 slots were cut out of thirty seconds of FT4, which is the
        // arithmetic working correctly on the wrong grid.
        Assert.Equal(2, heard.SlotsDecoded);
        Assert.Equal("Ft8Sharp.Deep", Decoder(heard));

        // **AND NOT ONE OF THE FOUR MESSAGES CAME BACK.** Zero read, four missed, zero
        // wrong - the standing ruling counts a wrong decode separately from a missed one
        // and both are reported even when one is zero.
        Assert.Empty(heard.Decodes);
    }

    /// <summary>A panel with a sub-mode already chosen, as a restarted app has.</summary>
    private static MainWindowViewModel Panel(string? chosen)
        => new(new AppSettings { LastDigitalSubMode = chosen }, null);

    /// <summary>
    /// Thirty seconds of band with four FT4 transmissions in it, driven through the tab.
    /// </summary>
    /// <remarks>
    /// **THE RECORDING ENDS ON A WHOLE MINUTE AND THE CLOCK IS EXACT**, so both grids
    /// land where the arithmetic says they land and neither test is measuring a rounding.
    /// Thirty seconds is two whole FT8 slots and four whole FT4 slots, which is what lets
    /// one recording answer for both.
    /// </remarks>
    private static Ft8Reception Read(MainWindowViewModel panel)
    {
        var endedAt = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);

        return panel.ShowDecodes(
            Recording(), endedAt, new ClockOffset(0, endedAt.AddSeconds(-1)));
    }

    /// <summary>The recording: four FT4 slots end to end, at four frequencies.</summary>
    private static MonoAudio Recording()
    {
        const int rate = Ft4Waveform.DefaultSampleRate;

        var perSlot = (int)Math.Round(SlotGrid.Ft4.SlotSeconds * rate);
        var samples = new float[perSlot * Corpus.Length];

        for (var i = 0; i < Corpus.Length; i++)
        {
            var (to, de, extra, hz) = Corpus[i];

            var packed = new byte[Ft8Payload.MessageBytes];
            var result = Ft8StandardMessage.TryPack(to, de, extra, packed);

            Assert.True(
                result == Ft8PackResult.Ok,
                $"'{Spoken(to, de, extra)}' did not pack: {result}");

            var slot = Ft4Waveform.SynthesizeSlot(
                Ft4SymbolEncoder.Encode(packed), rate, hz);

            // `SynthesizeSlot` is one whole FT4 slot - silence, signal, silence - so it
            // is the same length as the grid's own slot and drops in at the boundary.
            Array.Copy(slot, 0, samples, i * perSlot, Math.Min(slot.Length, perSlot));
        }

        return new MonoAudio(rate, samples);
    }

    /// <summary>The one decoder every slot in a reading names, or what they disagree on.</summary>
    private static string Decoder(Ft8Reception heard)
    {
        var named = heard.Slots
            .Select(slot => slot.Decoder.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return named.Length == 1 ? named[0] : string.Join(" and ", named);
    }

    private static string Spoken(string to, string de, string extra)
        => string.Join(' ', new[] { to, de, extra }.Where(part => part.Length > 0));
}
