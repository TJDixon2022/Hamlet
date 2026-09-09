using System;
using System.Linq;
using System.Threading.Tasks;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
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
    /// Task 2's halfway position: the grid follows the press, and the decoder does not
    /// follow it yet.
    /// </summary>
    /// <remarks>
    /// <para>**REWRITTEN FROM TASK 1'S STARTING POSITION, WHICH THIS REPLACES.** What
    /// task 1 measured, on the same recording, was two FIFTEEN-second slots read by
    /// `Ft8Sharp.Deep` and nought of the four messages back. The slots move here and
    /// nothing else does.</para>
    /// <para>**AND THE HALFWAY STATE IS WORTH ASSERTING RATHER THAN SKIPPING PAST**,
    /// because it is the failure this unit's two halves exist to keep apart: the ring
    /// counting down 7.5 s while the reader still reads FT8 is two halves of the tab on
    /// different clocks, each internally consistent, with nothing on screen able to say
    /// which is right. Task 3 rewrites this again into
    /// <see cref="AnFt4PressCutsSevenAndAHalfAndReadsFt4"/>.</para>
    /// </remarks>
    [Fact]
    public void AfterTaskTwoAnFt4PressCutsSevenAndAHalfAndStillReadsWithFt8s()
    {
        var panel = Panel("FT4");

        Assert.Equal("FT4", panel.ChosenDigitalMode);
        Assert.Equal(SlotGrid.Ft4, panel.DigitalGrid);

        var heard = Read(panel);

        _output.WriteLine("  grid     : " + panel.DigitalGrid.Describe());
        _output.WriteLine("  slots    : " + heard.SlotsDecoded);
        _output.WriteLine("  decoder  : " + Decoder(heard));
        _output.WriteLine("  messages : " + heard.Decodes.Count);

        // Four whole FT4 slots out of thirty seconds, where task 1 measured two FT8
        // ones. **The reader is still FT8's**, so the messages are still nought.
        Assert.Equal(4, heard.SlotsDecoded);
        Assert.Equal("Ft8Sharp.Deep", Decoder(heard));
        Assert.Empty(heard.Decodes);
    }

    /// <summary>
    /// Task 2: the grid follows the chip he pressed, and never the block the dial
    /// happens to be in.
    /// </summary>
    /// <remarks>
    /// **THE CHOICE UNDERNEATH THE THREADING, ASSERTED RATHER THAN DESCRIBED.**
    /// `DigitalModeChip` keeps `IsLit` and `IsChosen` apart on purpose, and only one of
    /// them may drive a decoder. Every press below happens with no radio attached, so
    /// nothing is lit and every chip is in the `IsChosenElsewhere` state - which is
    /// exactly the case that separates the two answers.
    /// </remarks>
    [Fact]
    public async Task TheGridFollowsTheChipHePressedAndNotTheDialItLandedOn()
    {
        var panel = Panel(null);

        Assert.Equal(SlotGrid.Ft8, panel.DigitalGrid);

        await panel.ChooseDigitalModeCommand.ExecuteAsync("FT4");

        _output.WriteLine("  after FT4 : " + panel.DigitalGrid.Describe());

        Assert.Equal(SlotGrid.Ft4, panel.DigitalGrid);
        Assert.Equal(DigitalMode.Ft4, panel.DigitalMode);

        // **AND NOTHING IS LIT, SO THE GRID CANNOT HAVE COME FROM THE MAP.** No rig is
        // attached and nothing tuned, so FT4 is chosen with the dial elsewhere - its own
        // appearance (`DigitalModeChip.cs:40`), and the state a grid driven by `IsLit`
        // would have left on fifteen seconds.
        var ft4 = panel.DigitalModeChips.Single(chip => chip.Label == "FT4");

        Assert.True(ft4.IsChosenElsewhere);
        Assert.False(ft4.IsLit);
        Assert.All(panel.DigitalModeChips, chip => Assert.False(chip.IsLit));

        await panel.ChooseDigitalModeCommand.ExecuteAsync("FT8");

        _output.WriteLine("  after FT8 : " + panel.DigitalGrid.Describe());

        Assert.Equal(SlotGrid.Ft8, panel.DigitalGrid);

        // **PSK31 AND WSPR RUN FT8'S GRID, WHICH IS WHAT THEY RAN BEFORE THIS UNIT.**
        // Neither has a decoder, a grid or a cited frequency row anywhere in this tree.
        // **That is a gap this unit names and does not fill**, and it is asserted here
        // so that filling it later is a deliberate change rather than a surprise.
        foreach (var label in new[] { "PSK31", "WSPR" })
        {
            await panel.ChooseDigitalModeCommand.ExecuteAsync(label);

            _output.WriteLine($"  after {label,-5}: {panel.DigitalGrid.Describe()}");

            Assert.Equal(label, panel.ChosenDigitalMode);
            Assert.Equal(SlotGrid.Ft8, panel.DigitalGrid);
        }
    }

    /// <summary>
    /// A band with no FT4 row moves nothing, says so, and still runs the tab on FT4.
    /// </summary>
    /// <remarks>
    /// **THE TUNE AND THE GRID ARE DIFFERENT ANSWERS TO DIFFERENT QUESTIONS.** §0.2.1
    /// says a frequency is never written from memory, so 30 m moves nothing; but he did
    /// press FT4, and a tab that quietly went on cutting fifteen seconds because the
    /// dial could not follow would be deciding for him. **The disagreement is on screen
    /// already** - `DigitalTuneLine` says which bands have a row - so nothing here adds
    /// a second voice about it.
    /// </remarks>
    [Fact]
    public async Task ABandWithNoFt4RowMovesNothingAndTheTabStillRunsFt4()
    {
        // Asserted from the tree rather than assumed, so this fails loudly if the data
        // gains a 30 m FT4 row rather than quietly testing nothing.
        Assert.Null(DigitalCallingFrequencies.Find("30 m", "FT4"));

        var panel = Panel(null);

        panel.SelectedBand = panel.Bands.First(band => band.Band.Name == "30 m");
        panel.FrequencyHz = panel.SelectedBand.Band.JumpHz;

        var before = panel.FrequencyHz;

        await panel.ChooseDigitalModeCommand.ExecuteAsync("FT4");

        _output.WriteLine("  line : " + panel.DigitalTuneLine);
        _output.WriteLine("  grid : " + panel.DigitalGrid.Describe());

        Assert.Equal(before, panel.FrequencyHz);
        Assert.True(panel.DigitalTuneFailed);
        Assert.Equal(SlotGrid.Ft4, panel.DigitalGrid);
    }

    /// <summary>
    /// The two sentences on screen follow the grid, and neither states a transmission
    /// length.
    /// </summary>
    /// <remarks>
    /// **4.48 AGAINST 5.04 IS TIM'S AND A SENTENCE STATING EITHER WOULD ANSWER IT.**
    /// The slot length is measured and settled on both readings; the occupancy is not,
    /// so the screen names the one and never the other.
    /// </remarks>
    [Fact]
    public async Task TheSentencesFollowTheChosenModeAndNameNoTransmissionLength()
    {
        foreach (var (label, slots, idle) in
                 new[] { ("FT4", "7.5 s slots", "7.5 seconds"), ("FT8", "15 s slots", "15 seconds") })
        {
            var panel = Panel(null);

            panel.DigitalSpectrum = new AudioSpectrumSource(48000, simulated: false);
            panel.ClockOffset = new ClockOffset(0, DateTime.UtcNow);

            await panel.ChooseDigitalModeCommand.ExecuteAsync(label);

            _output.WriteLine($"  {label} idle : " + panel.DigitalModeStripLine);
            _output.WriteLine($"  {label} wfall: " + panel.DigitalWaterfallSummary);

            Assert.Contains(idle, panel.DigitalModeStripLine, StringComparison.Ordinal);
            Assert.Contains(slots, panel.DigitalWaterfallSummary, StringComparison.Ordinal);

            // Neither sentence carries an occupancy on either grid, so neither of them
            // is capable of answering a question that is with the owner.
            foreach (var figure in new[] { "4.48", "5.04", "12.64" })
            {
                Assert.DoesNotContain(
                    figure, panel.DigitalModeStripLine, StringComparison.Ordinal);
                Assert.DoesNotContain(
                    figure, panel.DigitalWaterfallSummary, StringComparison.Ordinal);
            }
        }
    }

    /// <summary>
    /// FT8's boundaries and FT8's sentences are what they were, including after a round
    /// trip through FT4.
    /// </summary>
    /// <remarks>
    /// **THE CONTROL.** Threading a mode is exactly how an FT8 boundary moves by a tick
    /// with nobody noticing. Every boundary in a minute is compared against
    /// <see cref="Ft8Slots"/>' own arithmetic, which unit 290 pinned at 3,888
    /// tick-identical moments and which this unit did not touch.
    /// </remarks>
    [Fact]
    public async Task AnFt8PressLeavesEveryBoundaryAndSentenceWhereItWas()
    {
        var fresh = Panel(null);
        var toured = Panel(null);

        await toured.ChooseDigitalModeCommand.ExecuteAsync("FT4");
        await toured.ChooseDigitalModeCommand.ExecuteAsync("FT8");

        Assert.Equal(SlotGrid.Ft8, toured.DigitalGrid);
        Assert.Equal(fresh.DigitalModeStripLine, toured.DigitalModeStripLine);
        Assert.Equal(fresh.DigitalWaterfallSummary, toured.DigitalWaterfallSummary);

        var from = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);

        var mine = toured.DigitalGrid.BoundariesBetween(from, from.AddMinutes(1));
        var theirs = Ft8Slots.BoundariesBetween(from, from.AddMinutes(1));

        _output.WriteLine($"  {mine.Count} boundaries, first {mine[0]:HH:mm:ss.fffffff}");

        Assert.Equal(theirs.Count, mine.Count);

        for (var i = 0; i < mine.Count; i++)
        {
            Assert.Equal(theirs[i].Ticks, mine[i].Ticks);
        }
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
