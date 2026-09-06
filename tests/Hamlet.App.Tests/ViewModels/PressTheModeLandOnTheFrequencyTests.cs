using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 251, task 5: pressing a sub-mode takes the radio there, and
/// the display moves on the read-back rather than on the command.
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS EXISTS TO STOP IS THE ONE §0.0 IS ABOUT IN ITS PUREST
/// FORM.** A display naming a frequency the radio did not accept is worse than a
/// display that has not caught up, because every other surface in the application
/// - the map, the licence card, the neighborhood, the decoded table's heading -
/// trusts that number and would all be wrong together, confidently, with nothing
/// on screen saying so.</para>
/// <para>**THE INTERESTING CASE IS THE RADIO THAT ANSWERS AND ANSWERS WRONG.** A
/// radio that is not there is easy: nothing happens. A radio that takes the
/// command, returns no error and sits on a different frequency is what a
/// band-edge clamp, a split VFO or a memory-mode lock actually looks like from
/// this side of the wire, and it is indistinguishable from success unless
/// somebody reads the frequency back.</para>
/// </remarks>
public sealed class PressTheModeLandOnTheFrequencyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the frequencies and the lines are printed.</param>
    public PressTheModeLandOnTheFrequencyTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// A radio that accepts the set and reports a different frequency leaves the
    /// display where it was, and says the tune did not take.
    /// </summary>
    [Fact]
    public async Task ARadioThatComesBackElsewhereLeavesTheDisplayAloneAndSaysSo()
    {
        var model = OnTwentyMetres(out var rig);

        // **IT ACCEPTS THE COMMAND AND SITS SOMEWHERE ELSE.** No exception, no
        // refusal - the shape of a clamp or a lock, which is the shape that used
        // to be indistinguishable from success.
        rig.AnswerWith = 14_070_000;

        var before = model.FrequencyHz;

        await model.ChooseDigitalModeCommand.ExecuteAsync("FT8");

        _output.WriteLine("asked for   : " + rig.LastSet);
        _output.WriteLine("came back   : " + rig.AnswerWith);
        _output.WriteLine("display     : " + model.FrequencyHz);
        _output.WriteLine("line        : " + model.DigitalTuneLine);

        // The command did go out, and at the frequency the tree holds for FT8 on
        // 20 m rather than one written into this test.
        Assert.Equal(Where("20 m", "FT8"), rig.LastSet);
        Assert.Equal(1, rig.Sets);
        Assert.Equal(1, rig.Reads);

        // **THE DISPLAY DID NOT MOVE.** Not to the target, which the radio did
        // not accept, and not to the read-back either - moving there would be
        // right about the radio and would swallow the fact that the press did
        // something other than what it said.
        Assert.Equal(before, model.FrequencyHz);

        Assert.True(model.DigitalTuneFailed);
        Assert.True(model.HasDigitalTuneLine);

        // It names both numbers, so the operator can see what happened rather
        // than only that something did.
        Assert.Contains("did not take", model.DigitalTuneLine, StringComparison.Ordinal);
        Assert.Contains("14.074000", model.DigitalTuneLine, StringComparison.Ordinal);
        Assert.Contains("14.070000", model.DigitalTuneLine, StringComparison.Ordinal);

        // And the press is still remembered, so the chip shows he asked for FT8
        // and the dial is elsewhere.
        Assert.Equal("FT8", model.ChosenDigitalMode);

        var chip = model.DigitalModeChips.Single(c => c.Label == "FT8");

        Assert.True(chip.IsChosenElsewhere);
    }

    /// <summary>
    /// A radio that confirms the frequency is the only case the display follows.
    /// </summary>
    [Fact]
    public async Task ARadioThatConfirmsIsWhatMovesTheDisplay()
    {
        var model = OnTwentyMetres(out var rig);

        rig.ConfirmWhateverItIsGiven = true;

        await model.ChooseDigitalModeCommand.ExecuteAsync("FT8");

        _output.WriteLine("display : " + model.FrequencyHz);
        _output.WriteLine("line    : " + model.DigitalTuneLine);

        Assert.Equal(Where("20 m", "FT8"), model.FrequencyHz);
        Assert.False(model.DigitalTuneFailed);
        Assert.Contains("confirmed", model.DigitalTuneLine, StringComparison.Ordinal);

        // **THE READ-BACK IS WHAT IS ON SCREEN, NOT THE TARGET**, and the two
        // agreeing here is what makes that invisible. The failing test above is
        // where the difference shows.
        Assert.Equal(rig.LastReadReturned, model.FrequencyHz);

        // The chip is chosen and the dial is now in its block, so nothing is
        // claiming the two disagree.
        var chip = model.DigitalModeChips.Single(c => c.Label == "FT8");

        Assert.True(chip.IsChosen);
        Assert.True(chip.IsLit);
    }

    /// <summary>
    /// A radio that throws on the way leaves the display alone and says why.
    /// </summary>
    [Fact]
    public async Task ARadioThatDoesNotAnswerLeavesTheDisplayAlone()
    {
        var model = OnTwentyMetres(out var rig);

        rig.ThrowOnRead = true;

        var before = model.FrequencyHz;

        await model.ChooseDigitalModeCommand.ExecuteAsync("FT8");

        _output.WriteLine("line : " + model.DigitalTuneLine);

        Assert.Equal(before, model.FrequencyHz);
        Assert.True(model.DigitalTuneFailed);
        Assert.Contains("did not take", model.DigitalTuneLine, StringComparison.Ordinal);
    }

    /// <summary>
    /// A band with no row for that mode moves nothing, and says where one is.
    /// </summary>
    /// <remarks>
    /// **WSPR IS THE LIVE CASE AND IT IS A DATA GAP, NOT A BUG.** As of
    /// 2026-09-05 `data/bands/us-neighborhoods.json` carries no WSPR row on any
    /// band. The strip offers the chip because the owner chose those four in
    /// August; the press says plainly that Hamlet has no cited frequency for it
    /// rather than inventing one, which is the thing §0.2.1 forbids outright.
    /// </remarks>
    [Fact]
    public async Task AModeTheDataHasNoRowForMovesNothing()
    {
        var model = OnTwentyMetres(out var rig);

        rig.ConfirmWhateverItIsGiven = true;

        var before = model.FrequencyHz;

        await model.ChooseDigitalModeCommand.ExecuteAsync("WSPR");

        _output.WriteLine("line : " + model.DigitalTuneLine);

        Assert.Equal(0, rig.Sets);
        Assert.Equal(before, model.FrequencyHz);
        Assert.True(model.DigitalTuneFailed);
        Assert.Contains("has not moved", model.DigitalTuneLine, StringComparison.Ordinal);

        // The choice is still recorded - he asked for it, and that is true.
        Assert.Equal("WSPR", model.ChosenDigitalMode);
    }

    /// <summary>
    /// A mode that lives on other bands but not this one says which bands.
    /// </summary>
    [Fact]
    public async Task AModeThatIsOnOtherBandsSaysWhichOnes()
    {
        // FT4 has rows on 80, 40, 20, 15 and 10 in the cited data, and none on
        // 30 m. **Asserted from the tree rather than assumed**, so this test
        // fails loudly if the data gains a 30 m FT4 row rather than quietly
        // testing nothing.
        Assert.Null(DigitalCallingFrequencies.Find("30 m", "FT4"));

        var model = OnBand("30 m", out var rig);

        rig.ConfirmWhateverItIsGiven = true;

        var before = model.FrequencyHz;

        await model.ChooseDigitalModeCommand.ExecuteAsync("FT4");

        _output.WriteLine("line : " + model.DigitalTuneLine);

        Assert.Equal(0, rig.Sets);
        Assert.Equal(before, model.FrequencyHz);
        Assert.True(model.DigitalTuneFailed);

        foreach (var band in DigitalCallingFrequencies.BandsWith("FT4"))
        {
            Assert.Contains(band, model.DigitalTuneLine, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The licence card answers for the tab the operator is on.
    /// </summary>
    /// <remarks>
    /// **A FALSE LICENCE STATEMENT IS WORSE THAN A STALE FREQUENCY** and it is the
    /// one place in this application where a confident wrong answer has legal
    /// consequences (HM-DEC-029). The card read *covers Morse here* on the Digital
    /// tab, about a mode the operator was not using.
    /// </remarks>
    [Fact]
    public void TheLicenceCardAnswersForTheTabAndNotForMorseAlways()
    {
        var settings = new AppSettings();

        settings.Operator.LicenseClass = LicenseClass.General;

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "CW",
        };

        var onCw = model.PrivilegeStatus.Detail;

        model.OperatingMode = "Digital";

        var onDigital = model.PrivilegeStatus.Detail;

        _output.WriteLine("CW      : " + onCw);
        _output.WriteLine("Digital : " + onDigital);

        Assert.Contains("Morse", onCw, StringComparison.Ordinal);

        Assert.DoesNotContain("Morse", onDigital, StringComparison.Ordinal);
        Assert.Contains("digital modes", onDigital, StringComparison.Ordinal);
    }

    /// <summary>Where the tree says a mode lives, so no test writes a number.</summary>
    private static long Where(string band, string mode)
    {
        var block = DigitalCallingFrequencies.Find(band, mode);

        Assert.True(
            block is not null,
            "the cited band data has no " + mode + " row on " + band
            + ", so this test has nothing to aim at");

        return block!.JumpHz;
    }

    private static MainWindowViewModel OnTwentyMetres(out FakeCiv rig)
        => OnBand("20 m", out rig);

    private static MainWindowViewModel OnBand(string band, out FakeCiv rig)
    {
        var model = new MainWindowViewModel(new AppSettings(), null)
        {
            OperatingMode = "Digital",
        };

        model.SelectedBand = model.Bands.First(b => b.Band.Name == band);

        // **THE DIAL GOES WITH THE BAND BUTTON.** Selecting a band does not move
        // the frequency on its own, and a view model showing 20 m over a dial
        // still on 40 m is a starting position the operator can never be in - it
        // would leave "the display did not move" true for the wrong reason.
        model.FrequencyHz = model.SelectedBand.Band.JumpHz;

        rig = new FakeCiv();
        model.UseRigForTests(rig);

        return model;
    }

    /// <summary>
    /// A radio that answers, and answers whatever it is told to answer.
    /// </summary>
    /// <remarks>
    /// **IT IS NOT A BROKEN RADIO, WHICH IS THE POINT.** Every call returns
    /// normally. The only thing under test is whether Hamlet believes a command
    /// it sent or a value it read.
    /// </remarks>
    private sealed class FakeCiv : IRig
    {
        /// <summary>What CI-V 03 comes back with, whatever was set.</summary>
        public long AnswerWith { get; set; }

        /// <summary>When true, the read returns whatever the last set asked for.</summary>
        public bool ConfirmWhateverItIsGiven { get; set; }

        /// <summary>When true, the read throws the way a dead link does.</summary>
        public bool ThrowOnRead { get; set; }

        /// <summary>What the last set asked for.</summary>
        public long LastSet { get; private set; }

        /// <summary>What the last read actually returned.</summary>
        public long LastReadReturned { get; private set; }

        /// <summary>How many sets went out.</summary>
        public int Sets { get; private set; }

        /// <summary>How many reads went out.</summary>
        public int Reads { get; private set; }

        /// <inheritdoc/>
        public bool IsConnected => true;

        /// <inheritdoc/>
        public bool IsSimulated => false;

        /// <inheritdoc/>
        /// <remarks>**IT CANNOT TRANSMIT**, which is the honest answer for a fake
        /// and also means nothing in this file can reach a keying path.</remarks>
        public RigCapabilities Capabilities { get; } = new(
            "fake CI-V", false, false, false, false, HfBands.Names);

        /// <inheritdoc/>
        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        /// <inheritdoc/>
        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        /// <inheritdoc/>
        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        /// <inheritdoc/>
        public Task DisconnectAsync() => Task.CompletedTask;

        /// <inheritdoc/>
        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
        {
            Reads++;

            if (ThrowOnRead)
            {
                throw new IOException("the link stopped answering");
            }

            LastReadReturned = ConfirmWhateverItIsGiven ? LastSet : AnswerWith;

            return Task.FromResult(LastReadReturned);
        }

        /// <inheritdoc/>
        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
        {
            Sets++;
            LastSet = frequencyHz;

            // **IT ACCEPTS EVERYTHING.** A radio that refused would be caught by
            // the exception path, which is a different test.
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<RigValue>>(
                new[] { RigValue.Unknown(field, "the fake radio answers nothing else") });

        /// <inheritdoc/>
        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, byte? filterSlot = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("the fake radio does not do modes"));

        /// <inheritdoc/>
        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("the fake radio does not do settings"));

        /// <inheritdoc/>
        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "nothing in unit 251 keys a transmitter (CLAUDE.md 0.2)");

        /// <inheritdoc/>
        public void AbortCw()
        {
        }

        /// <summary>Keeps the compiler from warning that the events are unused.</summary>
        internal void NobodyRaisesThese()
        {
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(0));
            ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(Array.Empty<RigValue>()));
        }
    }
}
